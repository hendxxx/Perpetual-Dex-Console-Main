using PerpetualDexApp.Trading;
using PerpetualDexApp.Models;

namespace PerpetualDexApp.Tests
{
    public class ExchangeTest
    {
        private Exchange CreateExchangeWithUsers()
        {
            var users = new List<UserConfig>
            {
                new UserConfig { Id = "user1", Collateral = 10000m },
                new UserConfig { Id = "user2", Collateral = 10000m }
            };
            return new Exchange(users);
        }

        [Fact]
        public void Exchange_InitializesUsersCorrectly()
        {
            var exchange = CreateExchangeWithUsers();
            var users = exchange.GetUsers();
            Assert.True(users.ContainsKey("user1"));
            Assert.True(users.ContainsKey("user2"));
            Assert.Equal(10000m, users["user1"].Collateral);
        }

        [Fact]
        public void Exchange_SetMarkPrice_UpdatesMarkPrice()
        {
            var exchange = CreateExchangeWithUsers();
            exchange.SetMarkPrice(50000m);
            Assert.Equal(50000m, exchange.GetMarkPrice());
        }

        [Fact]
        public void Exchange_PlaceOrder_RejectsInvalidOrder()
        {
            var exchange = CreateExchangeWithUsers();
            var order = new Order
            {
                UserId = "user1",
                Quantity = 0, // Invalid Quantity
                Price = 50000m,
                Leverage = 5,
                Side = OrderSides.Buy
            };
            exchange.PlaceOrder(order);
            // Should not throw, but should not place order either
            var trades = exchange.GetTradeHistory();
            Assert.Empty(trades);
        }

        [Fact]
        public void Exchange_PlaceOrder_RejectsInsufficientCollateral()
        {
            var exchange = CreateExchangeWithUsers();
            var order = new Order
            {
                UserId = "user1",
                Quantity = 10,
                Price = 100000m, // Notional = 1,000,000
                Leverage = 1, // Margin required = 1,000,000
                Side = OrderSides.Buy
            };
            exchange.PlaceOrder(order);
            var trades = exchange.GetTradeHistory();
            Assert.Empty(trades);
        }

        [Fact]
        public void Exchange_PlaceOrder_ExecutesTradeAndUpdatesPosition()
        {
            var exchange = CreateExchangeWithUsers();
            exchange.SetMarkPrice(50000m);

            var buyOrder = new Order
            {
                UserId = "user1",
                Quantity = 1,
                Price = 50000m,
                Leverage = 5,
                Side = OrderSides.Buy
            };
            var sellOrder = new Order
            {
                UserId = "user2",
                Quantity = 1,
                Price = 50000m,
                Leverage = 5,
                Side = OrderSides.Sell
            };

            exchange.PlaceOrder(buyOrder);
            exchange.PlaceOrder(sellOrder);

            var trades = exchange.GetTradeHistory();
            Assert.Single(trades);

            var users = exchange.GetUsers();
            Assert.True(users["user1"].CurrentPosition.IsOpen);
            Assert.True(users["user2"].CurrentPosition.IsOpen);
            Assert.Equal(PositionSides.Long, users["user1"].CurrentPosition.Side);
            Assert.Equal(PositionSides.Short, users["user2"].CurrentPosition.Side);
        }

        [Fact]
        public void Exchange_ApplyFunding_AdjustsCollateral()
        {
            var exchange = CreateExchangeWithUsers();
            exchange.SetMarkPrice(50000m);

            var buyOrder = new Order
            {
                UserId = "user1",
                Quantity = 1,
                Price = 50000m,
                Leverage = 5,
                Side = OrderSides.Buy
            };
            var sellOrder = new Order
            {
                UserId = "user2",
                Quantity = 1,
                Price = 50000m,
                Leverage = 5,
                Side = OrderSides.Sell
            };

            exchange.PlaceOrder(buyOrder);
            exchange.PlaceOrder(sellOrder);

            var user1CollateralBefore = exchange.GetUsers()["user1"].Collateral;
            var user2CollateralBefore = exchange.GetUsers()["user2"].Collateral;

            exchange.ApplyFunding();

            var user1CollateralAfter = exchange.GetUsers()["user1"].Collateral;
            var user2CollateralAfter = exchange.GetUsers()["user2"].Collateral;

            Assert.NotEqual(user1CollateralBefore, user1CollateralAfter);
            Assert.NotEqual(user2CollateralBefore, user2CollateralAfter);
        }

        [Fact]
        public void Exchange_LiquidatesPosition_WhenBelowMaintenanceMargin()
        {
            var exchange = CreateExchangeWithUsers();
            exchange.SetMarkPrice(50000m);

            var buyOrder = new Order
            {
                UserId = "user1",
                Quantity = 1,
                Price = 50000m,
                Leverage = 2,
                Side = OrderSides.Buy
            };
            exchange.PlaceOrder(buyOrder);

            // Simulate Price drop to trigger liquidation
            exchange.SetMarkPrice(20000m);

            var user = exchange.GetUsers()["user1"];
            Assert.False(user.CurrentPosition.IsOpen);
        }
    }
}