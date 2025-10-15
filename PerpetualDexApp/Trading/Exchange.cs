using System;
using System.Collections.Generic;
using System.Linq;
using PerpetualDexApp.Models;

namespace PerpetualDexApp.Trading
{
    public class Exchange
    {
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        private OrderBook _orderBook = new OrderBook();
        private List<Trade> _tradeHistory = new List<Trade>();

        private decimal _currentMarkPrice;
        private const decimal maintenanceMarginRate = 0.05m;
        private const decimal liquidationFeeRate = 0.01m;
        private const int maxLeverage = 10;
        
        //add user from input.json
        public Exchange(List<UserConfig> initialUsers)
        {
            foreach (var uc in initialUsers)
            {
                if (uc.Id != null)
                {
                    _users[uc.Id] = new User { Id = uc.Id, Collateral = uc.Collateral };
                }
                else
                {
                    throw new ArgumentNullException(nameof(uc.Id), "UserConfig.Id cannot be null.");
                }
            }
        }

        //Set Mark Price
        public void SetMarkPrice(decimal newMarkPrice)
        {
            _currentMarkPrice = newMarkPrice;
            UpdateUsersPnl();
            CheckForLiquidations(); 
        }
        public decimal GetMarkPrice() => _currentMarkPrice;

        public void PlaceOrder(Order order)
        {
            if (order.UserId == null || !_users.TryGetValue(order.UserId, out User? user))
            {
                Console.WriteLine($"Error: UserId is null or User {order.UserId} not found.");
                return;
            }

            if (order.Quantity <= 0 || order.Price <= 0 || order.Leverage <= 0 || order.Leverage > maxLeverage)
            {
                Console.WriteLine($"Error: Invalid order parameters for user {user.Id}.");
                return;
            }

            decimal orderNotionalValue = order.Quantity * order.Price;
            order.InitialMarginRequirement = orderNotionalValue / order.Leverage;

            decimal currentPositionMarginHeld = user.CurrentPosition.IsOpen ?
                (user.CurrentPosition.Quantity * user.CurrentPosition.EntryPrice) / user.CurrentPosition.Leverage : 0m;

            decimal availableCollateral = user.Collateral - currentPositionMarginHeld;

            if (!user.CurrentPosition.IsOpen ||
                (user.CurrentPosition.IsOpen && user.CurrentPosition.Side == (PositionSides?)order.Side))
            {
                if (availableCollateral < order.InitialMarginRequirement)
                {
                    Console.WriteLine($"Order Rejected for {user.Id}: Insufficient collateral (Needed: {order.InitialMarginRequirement:C}, Available: {availableCollateral:C}).");
                    return;
                }
            }

            Console.WriteLine($"Placing order for {user.Id}: {order.Side} {order.Quantity} BTC at {order.Price:C} (x{order.Leverage}).");
            _orderBook.PlaceOrder(order, OnTradeExecuted);
        }

        private void OnTradeExecuted(Trade trade)
        {
            if (trade.BuyOrder != null)
            {
                if (trade.BuyerId != null)
                {
                    UpdateUserPositionOnTrade(trade.BuyerId, trade.Quantity, trade.Price, trade.BuyOrder.Leverage, OrderSides.Buy);
                }
                else
                {
                    Console.WriteLine("Warning: BuyerId is null in trade execution.");
                }
            }
            else
            {
                Console.WriteLine("Warning: BuyOrder is null in trade execution.");
            }

            if (trade.SellOrder != null)
            {
                if (trade.SellerId != null)
                {
                    UpdateUserPositionOnTrade(trade.SellerId, trade.Quantity, trade.Price, trade.SellOrder.Leverage, OrderSides.Sell);
                }
                else
                {
                    Console.WriteLine("Warning: SellerId is null in trade execution.");
                }
            }
            else
            {
                Console.WriteLine("Warning: SellOrder is null in trade execution.");
            }

            _tradeHistory.Add(trade);
        }

        private void UpdateUserPositionOnTrade(string userId, decimal tradeQuantity, decimal tradePrice, decimal tradeLeverage, OrderSides orderSide)
        {
            var user = _users[userId];
            var currentPos = user.CurrentPosition;

            bool isOpeningOrIncreasing = (orderSide == OrderSides.Buy && currentPos.Side == PositionSides.Long) ||
                                         (orderSide == OrderSides.Sell && currentPos.Side == PositionSides.Short) ||
                                         !currentPos.IsOpen;

            bool isClosingOrReducing = (orderSide == OrderSides.Buy && currentPos.Side == PositionSides.Short) ||
                                       (orderSide == OrderSides.Sell && currentPos.Side == PositionSides.Long);

            decimal tradeInitialMargin = (tradeQuantity * tradePrice) / tradeLeverage;

            if (isOpeningOrIncreasing)
            {
                decimal currentNotional = currentPos.Quantity * currentPos.EntryPrice;
                decimal tradeNotional = tradeQuantity * tradePrice;
                decimal newTotalQuantity = currentPos.Quantity + tradeQuantity;

                if (newTotalQuantity > 0)
                {
                    currentPos.EntryPrice = (currentNotional + tradeNotional) / newTotalQuantity;
                }
                else
                {
                    currentPos.EntryPrice = tradePrice;
                }

                currentPos.Quantity = newTotalQuantity;
                currentPos.Side = (orderSide == OrderSides.Buy) ? PositionSides.Long : PositionSides.Short;
                currentPos.Leverage = tradeLeverage;

                user.Collateral -= tradeInitialMargin;
                Console.WriteLine($"  {user.Id} used {tradeInitialMargin:C} margin to {currentPos.Side} {tradeQuantity} BTC. New Collateral: {user.Collateral:C}");
            }
            else if (isClosingOrReducing)
            {
                decimal realizedPnl = (currentPos.Side == PositionSides.Long ? (tradePrice - currentPos.EntryPrice) : (currentPos.EntryPrice - tradePrice)) * tradeQuantity;
                user.Collateral += realizedPnl;

                decimal marginReleased = (tradeQuantity * currentPos.EntryPrice) / currentPos.Leverage;
                user.Collateral += marginReleased;

                Console.WriteLine($"{user.Id} closed {tradeQuantity} BTC. Realized PNL: {realizedPnl:C}. Margin Released: {marginReleased:C}. New Collateral: {user.Collateral:C}");

                if (tradeQuantity >= currentPos.Quantity)
                {
                    Console.WriteLine($"{user.Id} closed position fully.");
                    user.CurrentPosition = new Position();
                }
                else
                {
                    currentPos.Quantity -= tradeQuantity;
                }
            }
            UpdatePositionMarginsAndPnl(user);
        }
        
        private void UpdateUsersPnl()
        {
            foreach (var user in _users.Values)
            {
                UpdatePositionMarginsAndPnl(user);
            }
        }
        private void UpdatePositionMarginsAndPnl(User user) 
        {
            if (!user.CurrentPosition.IsOpen) 
            {
                user.CurrentPosition.UnrealizedPnl = 0; 
                user.CurrentPosition.MaintenanceMarginRequirement = 0; 
                return;
            }

            var pos = user.CurrentPosition; 
            decimal notionalValue = pos.Quantity * _currentMarkPrice; 

            if (pos.Side == PositionSides.Long) 
            {
                pos.UnrealizedPnl = (GetMarkPrice() - pos.EntryPrice) * pos.Quantity; 
            }
            else
            {
                pos.UnrealizedPnl = (pos.EntryPrice - GetMarkPrice()) * pos.Quantity; 
            }

            pos.MaintenanceMarginRequirement = notionalValue * maintenanceMarginRate; 
        }
        public void ApplyFunding(decimal? indexPriceOverride = null) 
        {
            decimal indexPrice = indexPriceOverride ?? (_currentMarkPrice * 0.999m); 
            decimal fundingRate = (_currentMarkPrice - indexPrice) / indexPrice * (1m / 8m); 

            Console.WriteLine($"Calculated Funding Rate: {fundingRate * 100:F4}% for period.");

            if (fundingRate == 0)
            {
                Console.WriteLine("Funding rate is zero, no funding applied this interval.");
                return;
            }

            foreach (var user in _users.Values)
            {
                if (user.CurrentPosition.IsOpen) 
                {
                    decimal positionNotional = user.CurrentPosition.Quantity * _currentMarkPrice; 
                    decimal fundingAmount = positionNotional * fundingRate; 

                    if (user.CurrentPosition.Side == PositionSides.Long) 
                    {
                        user.Collateral -= fundingAmount; 
                        Console.WriteLine($"{user.Id} (Long) paid funding: {fundingAmount:C}. New Collateral: {user.Collateral:C}");  
                    }
                    else
                    {
                        user.Collateral += fundingAmount; 
                        Console.WriteLine($"{user.Id} (Short) received funding: {fundingAmount:C}. New Collateral: {user.Collateral:C}");  
                    }
                }
            }
        }
        private void CheckForLiquidations() 
        {
            foreach (var user in _users.Values)
            {
                if (!user.CurrentPosition.IsOpen) continue; 

                decimal equity = user.Collateral + user.CurrentPosition.UnrealizedPnl; 

                if (equity < user.CurrentPosition.MaintenanceMarginRequirement) 
                {
                    LiquidatePosition(user); 
                }
            }
        }

        private void LiquidatePosition(User user)
        {
            Console.WriteLine($"--- LIQUIDATION TRIGGERED for {user.Id}! ---");
            var pos = user.CurrentPosition;

            //decimal realizedPnl = (pos.side == PositionSides.Long ? (_currentMarkPrice - pos.entryPrice) : (pos.entryPrice - _currentMarkPrice)) * pos.quantity; 
            //decimal realizedPnl = user.currentPosition.unrealizedPnl; // PNL at the point of liquidation
            decimal realizedPnl = pos.UnrealizedPnl;

            user.Collateral += realizedPnl;

            decimal liquidationFee = pos.Quantity * _currentMarkPrice * liquidationFeeRate;
            user.Collateral -= liquidationFee;

            decimal marginReleased = (pos.Quantity * _currentMarkPrice) / pos.Leverage;
            user.Collateral += marginReleased;

            Console.WriteLine($"  Position ({pos.Side} {pos.Quantity} BTC) liquidated at {_currentMarkPrice:C}.");
            Console.WriteLine($"  Realized PNL on liquidation: {realizedPnl:C}.");
            Console.WriteLine($"  Liquidation fee applied: {liquidationFee:C}.");
            Console.WriteLine($"  Margin released on liquidation: {marginReleased:C}.");
            Console.WriteLine($"  {user.Id}'s final collateral after liquidation: {user.Collateral:C}.");

            user.CurrentPosition = new Position();
        }

        public Dictionary<string, User> GetUsers() => _users;  
        public List<Trade> GetTradeHistory() => _tradeHistory; 

    }
}