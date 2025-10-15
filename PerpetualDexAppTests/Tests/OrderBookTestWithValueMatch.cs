using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using PerpetualDexApp.Models;
using PerpetualDexApp.Trading;

namespace PerpetualDexApp.Tests
{
    public class OrderBookTestsWithValueMatch
    {
        // A no-operation action to pass when trades are not the focus of the test
        private readonly Action<Trade> noOpTradeExecuted = (t) => { };

        [Fact]
        public void PlaceOrder_FullMatch_ReturnsSingleTrade()
        {
            // Arrange
            var orderBook = new OrderBook();
            var buyOrder = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 1m, Price = 100m, Leverage = 2 };
            var sellOrder = new Order { UserId = "user2", Side = OrderSides.Sell, Quantity = 1m, Price = 100m, Leverage = 2 };

            // Act
            orderBook.PlaceOrder(buyOrder, noOpTradeExecuted); // Place first order
            var trades = orderBook.PlaceOrder(sellOrder, noOpTradeExecuted); // Place second order, triggering match

            // Assert
            Assert.Single(trades);
            Assert.Equal(1m, trades.First().Quantity);
            Assert.Equal(100m, trades.First().Price);
            Assert.Equal(0m, buyOrder.Quantity); // Buy order should be fully filled
            Assert.Equal(0m, sellOrder.Quantity); // Sell order should be fully filled
            Assert.Empty(orderBook.GetBuyOrders()); // Order book should be empty
            Assert.Empty(orderBook.GetSellOrders()); // Order book should be empty
        }

        [Fact]
        public void PlaceOrder_PartialMatch_ReturnsPartialTradeAndRemainingOrder()
        {
            // Arrange
            var orderBook = new OrderBook();
            var buyOrder = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 1m, Price = 100m, Leverage = 2 };
            var sellOrder = new Order { UserId = "user2", Side = OrderSides.Sell, Quantity = 0.5m, Price = 100m, Leverage = 2 };

            // Act
            orderBook.PlaceOrder(buyOrder, noOpTradeExecuted); // Place large buy order
            var trades = orderBook.PlaceOrder(sellOrder, noOpTradeExecuted); // Place smaller sell order

            // Assert
            Assert.Single(trades);
            Assert.Equal(0.5m, trades.First().Quantity);
            Assert.Equal(100m, trades.First().Price);
            Assert.Equal(0.5m, buyOrder.Quantity); // Buy order should be partially filled
            Assert.Equal(0m, sellOrder.Quantity); // Sell order should be fully filled

            var remainingBuyOrders = orderBook.GetBuyOrders().ToList();
            Assert.Single(remainingBuyOrders); // One buy order should remain in the book
            Assert.Equal(0.5m, remainingBuyOrders.First().Quantity);
            Assert.Equal(100m, remainingBuyOrders.First().Price);
            
            Assert.Empty(orderBook.GetSellOrders()); // No sell orders should remain
        }

        [Fact]
        public void PlaceOrder_NoMatch_OrdersRemainInBook()
        {
            // Arrange
            var orderBook = new OrderBook();
            var buyOrder = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 1m, Price = 99m, Leverage = 2 };
            var sellOrder = new Order { UserId = "user2", Side = OrderSides.Sell, Quantity = 1m, Price = 101m, Leverage = 2 };
            
            var trades = new List<Trade>();
            Action<Trade> onTrade = t => trades.Add(t);

            // Act
            orderBook.PlaceOrder(buyOrder, onTrade); // Place buy order (no match)
            orderBook.PlaceOrder(sellOrder, onTrade); // Place sell order (no match)

            // Assert
            Assert.Empty(trades); // No trades should have occurred
            Assert.Equal(1m, buyOrder.Quantity); // Order quantities unchanged
            Assert.Equal(1m, sellOrder.Quantity);

            Assert.Single(orderBook.GetBuyOrders()); // One buy order should be in the book
            Assert.Single(orderBook.GetSellOrders()); // One sell order should be in the book
        }

        [Fact]
        public void PlaceOrder_MultiplePartialFills()
        {
            // Arrange
            var orderBook = new OrderBook();
            var largeBuyOrder = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 1.0m, Price = 100m, Leverage = 2 };
            var smallSellOrder1 = new Order { UserId = "user2", Side = OrderSides.Sell, Quantity = 0.3m, Price = 100m, Leverage = 2 };
            var smallSellOrder2 = new Order { UserId = "user3", Side = OrderSides.Sell, Quantity = 0.4m, Price = 100m, Leverage = 2 };

            var trades = new List<Trade>();
            Action<Trade> onTrade = t => trades.Add(t);

            // Act
            orderBook.PlaceOrder(largeBuyOrder, onTrade);
            orderBook.PlaceOrder(smallSellOrder1, onTrade);
            orderBook.PlaceOrder(smallSellOrder2, onTrade);

            // Assert
            Assert.Equal(2, trades.Count); // Two trades should have occurred
            Assert.Equal(0.3m, trades[0].Quantity);
            Assert.Equal(0.4m, trades[1].Quantity);

            Assert.Equal(0.3m, largeBuyOrder.Quantity); // Large buy order should have remaining quantity
            Assert.Equal(0m, smallSellOrder1.Quantity); // Small sell order 1 fully filled
            Assert.Equal(0m, smallSellOrder2.Quantity); // Small sell order 2 fully filled

            Assert.Single(orderBook.GetBuyOrders()); // One buy order (partially filled) remains
            Assert.Empty(orderBook.GetSellOrders()); // No sell orders remain
            Assert.Equal(0.3m, orderBook.GetBuyOrders().First().Quantity); // Check remaining quantity
        }
    }
}
