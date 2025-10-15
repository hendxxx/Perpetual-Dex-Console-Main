using Xunit;
using PerpetualDexApp.Trading;
using PerpetualDexApp.Models;

namespace PerpetualDexApp.Tests
{
    public class OrderBookTest
    {
        private Order CreateOrder(string userId, OrderSides side, decimal price, decimal quantity)
        {
            return new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Side = side,
                Price = price,
                Quantity = quantity
            };
        }

        [Fact]
        public void PlaceBuyOrder_NoMatch_AddsToBook()
        {
            var orderBook = new OrderBook();
            var buyOrder = CreateOrder(Guid.NewGuid().ToString(), OrderSides.Buy, 100, 1);

            var trades = orderBook.PlaceOrder(buyOrder, _ => { });

            Assert.Empty(trades);
            Assert.Contains(buyOrder, orderBook.GetBuyOrders());
        }

        [Fact]
        public void PlaceSellOrder_NoMatch_AddsToBook()
        {
            var orderBook = new OrderBook();
            var sellOrder = CreateOrder(Guid.NewGuid().ToString(), OrderSides.Sell, 105, 2);

            var trades = orderBook.PlaceOrder(sellOrder, _ => { });

            Assert.Empty(trades);
            Assert.Contains(sellOrder, orderBook.GetSellOrders());
        }

        [Fact]
        public void PlaceBuyOrder_MatchesSellOrder_ExecutesTrade()
        {
            var orderBook = new OrderBook();
            var sellerId = Guid.NewGuid().ToString();
            var buyerId = Guid.NewGuid().ToString();

            var sellOrder = CreateOrder(sellerId, OrderSides.Sell, 100, 2);
            orderBook.PlaceOrder(sellOrder, _ => { });

            var buyOrder = CreateOrder(buyerId, OrderSides.Buy, 100, 1);
            var trades = orderBook.PlaceOrder(buyOrder, _ => { });

            Assert.Single(trades);
            var trade = trades[0];
            Assert.Equal(buyerId, trade.BuyerId);
            Assert.Equal(sellerId, trade.SellerId);
            Assert.Equal(1, trade.Quantity);
            Assert.Equal(100, trade.Price);
            Assert.Equal(1, sellOrder.Quantity); // Remaining quantity
        }

        [Fact]
        public void PlaceSellOrder_MatchesBuyOrder_ExecutesTrade()
        {
            var orderBook = new OrderBook();
            var buyerId = Guid.NewGuid().ToString();
            var sellerId = Guid.NewGuid().ToString();

            var buyOrder = CreateOrder(buyerId, OrderSides.Buy, 105, 2);
            orderBook.PlaceOrder(buyOrder, _ => { });

            var sellOrder = CreateOrder(sellerId, OrderSides.Sell, 105, 1);
            var trades = orderBook.PlaceOrder(sellOrder, _ => { });

            Assert.Single(trades);
            var trade = trades[0];
            Assert.Equal(buyerId, trade.BuyerId);
            Assert.Equal(sellerId, trade.SellerId);
            Assert.Equal(1, trade.Quantity);
            Assert.Equal(105, trade.Price);
            Assert.Equal(1, buyOrder.Quantity); // Remaining quantity
        }

        [Fact]
        public void PlaceOrder_TradeCallbackIsInvoked()
        {
            var orderBook = new OrderBook();
            var buyerId = Guid.NewGuid().ToString();
            var sellerId = Guid.NewGuid().ToString();

            var sellOrder = CreateOrder(sellerId, OrderSides.Sell, 100, 1);
            orderBook.PlaceOrder(sellOrder, _ => { });

            var buyOrder = CreateOrder(buyerId, OrderSides.Buy, 100, 1);

            Trade? callbackTrade = null;
            var trades = orderBook.PlaceOrder(buyOrder, t => callbackTrade = t);

            Assert.NotNull(callbackTrade);
            Assert.Equal(buyerId, callbackTrade.BuyerId);
            Assert.Equal(sellerId, callbackTrade.SellerId);
            Assert.Equal(1, callbackTrade.Quantity);
            Assert.Equal(100, callbackTrade.Price);
        }

        [Fact]
        public void PlaceOrder_PartialFill_RemainingOrderInBook()
        {
            var orderBook = new OrderBook();
            var sellerId = Guid.NewGuid().ToString();
            var buyerId = Guid.NewGuid().ToString();

            var sellOrder = CreateOrder(sellerId, OrderSides.Sell, 100, 2);
            orderBook.PlaceOrder(sellOrder, _ => { });

            var buyOrder = CreateOrder(buyerId, OrderSides.Buy, 100, 1);
            orderBook.PlaceOrder(buyOrder, _ => { });

            Assert.Single(orderBook.GetSellOrders());
            Assert.Equal(1, orderBook.GetSellOrders().First().Quantity);
        }
    }
}