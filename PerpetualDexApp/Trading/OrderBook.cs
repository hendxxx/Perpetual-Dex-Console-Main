using System;
using System.Collections.Generic;
using System.Linq;
using PerpetualDexApp.Models;

namespace PerpetualDexApp.Trading
{
    public class OrderBook
    {
        private SortedDictionary<decimal, List<Order>> _buyOrders = new SortedDictionary<decimal, List<Order>>();
        private SortedDictionary<decimal, List<Order>> _sellOrders = new SortedDictionary<decimal, List<Order>>();
        private Dictionary<Guid, Order> _allOrders = new Dictionary<Guid, Order>();

        public IEnumerable<Order> GetBuyOrders() => _buyOrders.SelectMany(kvp => kvp.Value);
        public IEnumerable<Order> GetSellOrders() => _sellOrders.SelectMany(kvp => kvp.Value);

        public List<Trade> PlaceOrder(Order newOrder, Action<Trade> onTradeExecuted)
        {
            var executedTrades = new List<Trade>();
            _allOrders[newOrder.Id] = newOrder;

            if (newOrder.Side == OrderSides.Buy)
            {
                MatchBuyOrder(newOrder, executedTrades, onTradeExecuted); // Pass the action here
                if (newOrder.Quantity > 0)
                {
                    AddToBook(_buyOrders, newOrder);
                }
            }
            else
            {
                MatchSellOrder(newOrder, executedTrades, onTradeExecuted); // Pass the action here
                if (newOrder.Quantity > 0)
                {
                    AddToBook(_sellOrders, newOrder);
                }
            }
            return executedTrades;
        }

        private void MatchBuyOrder(Order buyOrder, List<Trade> executedTrades, Action<Trade> onTradeExecuted)
        {
            foreach (var priceLevel in _sellOrders.ToList())
            {
                decimal currentSellPrice = priceLevel.Key;
                if (buyOrder.Price < currentSellPrice)
                    break;

                var ordersAtPrice = priceLevel.Value;
                while (ordersAtPrice.Any() && buyOrder.Quantity > 0)
                {
                    var matchingSellOrder = ordersAtPrice.First();
                    decimal tradeQuantity = Math.Min(buyOrder.Quantity, matchingSellOrder.Quantity);

                    var trade = new Trade
                    {
                        BuyerId = buyOrder.UserId,
                        SellerId = matchingSellOrder.UserId,
                        Quantity = tradeQuantity,
                        Price = currentSellPrice,
                        BuyOrder = buyOrder,
                        SellOrder = matchingSellOrder
                    };
                    executedTrades.Add(trade);
                    onTradeExecuted?.Invoke(trade); // <<< CRITICAL FIX: Invoke the callback here!

                    buyOrder.Quantity -= tradeQuantity;
                    matchingSellOrder.Quantity -= tradeQuantity;

                    if (matchingSellOrder.Quantity == 0)
                    {
                        ordersAtPrice.RemoveAt(0);
                        _allOrders.Remove(matchingSellOrder.Id);
                    }
                }

                if (!ordersAtPrice.Any())
                {
                    _sellOrders.Remove(currentSellPrice);
                }

                if (buyOrder.Quantity == 0)
                    break;
            }
        }

        private void MatchSellOrder(Order sellOrder, List<Trade> executedTrades, Action<Trade> onTradeExecuted)
        {
            foreach (var priceLevel in _buyOrders.Reverse().ToList())
            {
                decimal currentBuyPrice = priceLevel.Key;
                if (sellOrder.Price > currentBuyPrice)
                    break;

                var ordersAtPrice = priceLevel.Value;
                while (ordersAtPrice.Any() && sellOrder.Quantity > 0)
                {
                    var matchingBuyOrder = ordersAtPrice.First();
                    decimal tradeQuantity = Math.Min(sellOrder.Quantity, matchingBuyOrder.Quantity);

                    var trade = new Trade
                    {
                        BuyerId = matchingBuyOrder.UserId,
                        SellerId = sellOrder.UserId,
                        Quantity = tradeQuantity,
                        Price = currentBuyPrice,
                        BuyOrder = matchingBuyOrder,
                        SellOrder = sellOrder
                    };
                    executedTrades.Add(trade);
                    onTradeExecuted?.Invoke(trade); // <<< CRITICAL FIX: Invoke the callback here!

                    sellOrder.Quantity -= tradeQuantity;
                    matchingBuyOrder.Quantity -= tradeQuantity;

                    if (matchingBuyOrder.Quantity == 0)
                    {
                        ordersAtPrice.RemoveAt(0);
                        _allOrders.Remove(matchingBuyOrder.Id);
                    }
                }

                if (!ordersAtPrice.Any())
                {
                    _buyOrders.Remove(currentBuyPrice);
                }

                if (sellOrder.Quantity == 0)
                    break;
            }
        }

        private void AddToBook(SortedDictionary<decimal, List<Order>> book, Order order)
        {
            if (!book.TryGetValue(order.Price, out List<Order>? value))
            {
                value = [];
                book[order.Price] = value;
            }

            value.Add(order);
        }
    }
}
