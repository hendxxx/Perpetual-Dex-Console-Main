using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using PerpetualDexApp.Models;
using PerpetualDexApp.Trading;

namespace PerpetualDexApp.Tests
{
    public class ExchangeTestsWithValueMatch
    {
        // Helper method to set up a fresh exchange for each test
        private Exchange SetupExchange()
        {
            var users = new List<UserConfig>
            {
                new UserConfig { Id = "user1", Collateral = 100000m },
                new UserConfig { Id = "user2", Collateral = 50000m },
                new UserConfig { Id = "user3", Collateral = 1000m } // User with lower collateral for liquidation tests
            };
            return new Exchange(users);
        }

        // --- PNL Calculations ---

        [Fact]
        public void SetMarkPrice_PnlPositive_LongPosition()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"];
            user1.CurrentPosition.Side = PositionSides.Long;
            user1.CurrentPosition.Quantity = 1m;
            user1.CurrentPosition.EntryPrice = 60000m;
            
            // Act
            exchange.SetMarkPrice(61000m); // Price moves up

            // Assert
            Assert.Equal(1000m, user1.CurrentPosition.UnrealizedPnl); // (61000 - 60000) * 1 = 1000
        }

        [Fact]
        public void SetMarkPrice_PnlNegative_LongPosition()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"];
            user1.CurrentPosition.Side = PositionSides.Long;
            user1.CurrentPosition.Quantity = 1m;
            user1.CurrentPosition.EntryPrice = 60000m;
            
            // Act
            exchange.SetMarkPrice(59000m); // Price moves down

            // Assert
            Assert.Equal(-1000m, user1.CurrentPosition.UnrealizedPnl); // (59000 - 60000) * 1 = -1000
        }

        [Fact]
        public void SetMarkPrice_PnlPositive_ShortPosition()
        {
            // Arrange
            var exchange = SetupExchange();
            var user2 = exchange.GetUsers()["user2"];
            user2.CurrentPosition.Side = PositionSides.Short;
            user2.CurrentPosition.Quantity = 1m;
            user2.CurrentPosition.EntryPrice = 60000m;
            
            // Act
            exchange.SetMarkPrice(59000m); // Price moves down (favorable for short)

            // Assert
            Assert.Equal(1000m, user2.CurrentPosition.UnrealizedPnl); // (60000 - 59000) * 1 = 1000
        }

        [Fact]
        public void SetMarkPrice_PnlNegative_ShortPosition()
        {
            // Arrange
            var exchange = SetupExchange();
            var user2 = exchange.GetUsers()["user2"];
            user2.CurrentPosition.Side = PositionSides.Short;
            user2.CurrentPosition.Quantity = 1m;
            user2.CurrentPosition.EntryPrice = 60000m;
            
            // Act
            exchange.SetMarkPrice(61000m); // Price moves up (unfavorable for short)

            // Assert
            Assert.Equal(-1000m, user2.CurrentPosition.UnrealizedPnl); // (60000 - 61000) * 1 = -1000
        }

        // --- Funding Applications ---

        [Fact]
    public void ApplyFunding_PositiveRate_LongPaysShort()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"]; // Long
            var user2 = exchange.GetUsers()["user2"]; // Short
            
            user1.CurrentPosition.Side = PositionSides.Long;
            user1.CurrentPosition.Quantity = 1m;
            user1.CurrentPosition.EntryPrice = 60000m;
            user1.CurrentPosition.Leverage = 5m;
            user1.Collateral = 100000m - (1m * 60000m / 5m); // 88000m collateral held as margin
            
            user2.CurrentPosition.Side = PositionSides.Short;
            user2.CurrentPosition.Quantity = 1m;
            user2.CurrentPosition.EntryPrice = 60000m;
            user2.CurrentPosition.Leverage = 5m;
            user2.Collateral = 50000m - (1m * 60000m / 5m); // 38000m collateral held as margin
            
            // Set mark price to calculate funding rate (mark > index)
            exchange.SetMarkPrice(61000m); 
            
            // Store collateral before funding
            decimal user1CollateralBeforeFunding = user1.Collateral;
            decimal user2CollateralBeforeFunding = user2.Collateral;

            // Act
            exchange.ApplyFunding(); // Call ApplyFunding

            // Assert
            decimal expectedFunding = (1m * exchange.GetMarkPrice()) * ((exchange.GetMarkPrice() - exchange.GetMarkPrice() * 0.999m) / (exchange.GetMarkPrice() * 0.999m)) * (1m/8m);
            
            // Long user pays
            Assert.Equal(user1CollateralBeforeFunding - expectedFunding, user1.Collateral, 20);
            // Short user receives
            Assert.Equal(user2CollateralBeforeFunding + expectedFunding, user2.Collateral, 20);
        }

        [Fact]
    public void ApplyFunding_NegativeRate_ShortPaysLong()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"]; // Long
            var user2 = exchange.GetUsers()["user2"]; // Short
            
            user1.CurrentPosition.Side = PositionSides.Long;
            user1.CurrentPosition.Quantity = 1m;
            user1.CurrentPosition.EntryPrice = 60000m;
            user1.CurrentPosition.Leverage = 5m;
            user1.Collateral = 100000m - (1m * 60000m / 5m);
            
            user2.CurrentPosition.Side = PositionSides.Short;
            user2.CurrentPosition.Quantity = 1m;
            user2.CurrentPosition.EntryPrice = 60000m;
            user2.CurrentPosition.Leverage = 5m;
            user2.Collateral = 50000m - (1m * 60000m / 5m);
            
            // Set mark price for funding calculation
            exchange.SetMarkPrice(60000m); 
            
            // For this test, manually define a higher index price to ensure a negative rate
            decimal currentMark = exchange.GetMarkPrice();
            decimal assumedIndexPrice = currentMark * 1.001m;
            decimal fundingRate = (currentMark - assumedIndexPrice) / assumedIndexPrice * (1m/8m); // This will be negative
            decimal expectedFundingMagnitude = (1m * currentMark) * Math.Abs(fundingRate);
            
            // Store collateral before funding
            decimal user1CollateralBeforeFunding = user1.Collateral;
            decimal user2CollateralBeforeFunding = user2.Collateral;

            // Act
            exchange.ApplyFunding(assumedIndexPrice); // <<< FIX: Pass the index price to override

            // Assert
            Assert.Equal(user1CollateralBeforeFunding + expectedFundingMagnitude, user1.Collateral, 20); 
            Assert.Equal(user2CollateralBeforeFunding - expectedFundingMagnitude, user2.Collateral, 20);

        }


        // --- Liquidation Triggers ---

        [Fact]
    public void LiquidatePosition_PriceDropLiquidatesLongPosition()
        {
            // Arrange
            var exchange = SetupExchange();
            var user3 = exchange.GetUsers()["user3"]; // 1000m collateral
            user3.CurrentPosition.Side = PositionSides.Long;
            user3.CurrentPosition.Quantity = 0.1m;
            user3.CurrentPosition.EntryPrice = 60000m;
            user3.CurrentPosition.Leverage = 10m; // Max leverage
            user3.Collateral = 1000m - (0.1m * 60000m / 10m); // 1000 - 600 = 400m collateral

            // Act
            // Set mark price that triggers liquidation for a long position
            // PNL: (52000 - 60000) * 0.1 = -800
            // Equity: 400 - 800 = -400
            // Maint Margin Req: 0.1 * 52000 * 0.05 = 260
            // Equity (-400) < Maint Margin Req (260) => Liquidated
            exchange.SetMarkPrice(52000m); 

            // Assert
            Assert.False(user3.CurrentPosition.IsOpen);
            // Expected Final Collateral (after liquidation):
            // Start: 400
            // + Realized PNL: -800 (from current unrealized PNL)
            // - Liquidation Fee: 0.1 * 52000 * 0.01 = 52
            // + Margin Released: (0.1 * 52000) / 10 = 520 (using liquidation price for margin release)
            // Final: 400 - 800 - 52 + 520 = 68m
            Assert.Equal(68.000m, user3.Collateral);
        }

        [Fact]
    public void LiquidatePosition_PriceIncreaseLiquidatesShortPosition()
        {
            // Arrange
            var exchange = SetupExchange();
            var user3 = exchange.GetUsers()["user3"]; // 1000m collateral
            user3.CurrentPosition.Side = PositionSides.Short;
            user3.CurrentPosition.Quantity = 0.1m;
            user3.CurrentPosition.EntryPrice = 60000m;
            user3.CurrentPosition.Leverage = 10m; // Max leverage
            user3.Collateral = 1000m - (0.1m * 60000m / 10m); // 1000 - 600 = 400m collateral

            // Act
            // Set mark price that triggers liquidation for a short position
            // PNL: (68000 - 60000) * 0.1 = -800 (negative for short)
            // Equity: 400 - 800 = -400
            // Maint Margin Req: 0.1 * 68000 * 0.05 = 340
            // Equity (-400) < Maint Margin Req (340) => Liquidated
            exchange.SetMarkPrice(68000m); 

            // Assert
            Assert.False(user3.CurrentPosition.IsOpen);
            // Expected Final Collateral (after liquidation):
            // Start: 400
            // + Realized PNL: -800 (from current unrealized PNL)
            // - Liquidation Fee: 0.1 * 68000 * 0.01 = 68
            // + Margin Released: (0.1 * 68000) / 10 = 680 (using liquidation price for margin release)
            // Final: 400 - 800 - 68 + 680 = 212m
            Assert.Equal(212m, user3.Collateral);
        }

        // --- Edge Cases ---

        [Fact]
    public void PlaceOrder_ZeroQuantity_OrderRejected()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"];
            var order = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 0m, Price = 60000m, Leverage = 1 };
            decimal initialCollateral = user1.Collateral;

            // Act
            exchange.PlaceOrder(order);

            // Assert
            Assert.Empty(exchange.GetTradeHistory());
            Assert.False(user1.CurrentPosition.IsOpen);
            Assert.Equal(initialCollateral, user1.Collateral); 
        }

        [Fact]
    public void PlaceOrder_NegativePrice_OrderRejected()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"];
            var order = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 1m, Price = -100m, Leverage = 1 };
            decimal initialCollateral = user1.Collateral;

            // Act
            exchange.PlaceOrder(order);

            // Assert
            Assert.Empty(exchange.GetTradeHistory());
            Assert.False(user1.CurrentPosition.IsOpen);
            Assert.Equal(initialCollateral, user1.Collateral); 
        }

        [Fact]
    public void PlaceOrder_ZeroLeverage_OrderRejected()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"];
            var order = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 1m, Price = 60000m, Leverage = 0m };
            decimal initialCollateral = user1.Collateral;

            // Act
            exchange.PlaceOrder(order);

            // Assert
            Assert.Empty(exchange.GetTradeHistory());
            Assert.False(user1.CurrentPosition.IsOpen);
            Assert.Equal(initialCollateral, user1.Collateral); 
        }

        [Fact]
    public void PlaceOrder_InvalidLeverageGreaterThanMax_OrderRejected()
        {
            // Arrange
            var exchange = SetupExchange();
            var user1 = exchange.GetUsers()["user1"];
            var order = new Order { UserId = "user1", Side = OrderSides.Buy, Quantity = 1m, Price = 60000m, Leverage = 11m }; // Invalid leverage > 10m
            decimal initialCollateral = user1.Collateral;

            // Act
            exchange.PlaceOrder(order);

            // Assert
            Assert.Empty(exchange.GetTradeHistory());
            Assert.False(user1.CurrentPosition.IsOpen);
            Assert.Equal(initialCollateral, user1.Collateral); 
        }
    }
}
