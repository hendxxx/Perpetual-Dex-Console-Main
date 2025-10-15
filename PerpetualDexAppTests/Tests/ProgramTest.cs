using Xunit;
using PerpetualDexApp.Models;
using System.IO;
using PerpetualDexApp;
using PerpetualDexApp.Trading;

namespace PerpetualDexApp.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void LoadConfiguration_WithValidFile_ReturnsCorrectConfig()
        {
            // Arrange
            string json = @"{
            ""users"": [ { ""id"": ""testUser"", ""collateral"": 5000 } ],
            ""prices"": [ 100, 101, 102 ],
            ""events"": []
            }";
            var testFilePath = "test_input.json";
            File.WriteAllText(testFilePath, json);

            // Act
            var config = Program.LoadConfiguration(testFilePath);

            // Assert
            Assert.NotNull(config);
            Assert.Single(config.Users == null ? new List<User>() : config.Users);
            Assert.Equal("testUser", config.Users?[0].Id);
            Assert.Equal(5000, config.Users?[0].Collateral);
            Assert.Equal(3, config.Prices?.Count);

            // Clean up test file
            File.Delete(testFilePath);
        }

        [Fact]
        public void GenerateFinalReport_WithOrder_PrintsCorrectReportToConsole()
        {
            // Arrange
            // Redirect the console output to a StringWriter
            var stringWriter = new StringWriter();
            var originalConsoleOut = Console.Out;
            Console.SetOut(stringWriter);
 
            try
            {
                // Create mock data
                var mockUsers = new List<UserConfig>();
                mockUsers.Add(new UserConfig { Id = "userA", Collateral = 1200 });
                mockUsers.Add(new UserConfig { Id = "userB", Collateral = 800 });


                var mockExchange = new Exchange(mockUsers?? []);
                var order = new Order
                {
                    UserId = "userA",
                    Side = OrderSides.Buy,
                    Quantity = 1,
                    Price = 110,
                    Leverage = 1,
                    PlacedTime = DateTime.Parse("2025-10-15T10:00:00")
                };
                mockExchange?.PlaceOrder(order);
                 
                Program._exchange = mockExchange;
                // Use reflection to call the private static method
                Program.GenerateFinalReport();

                // Act

                // Assert
                var expectedOutput = "Placing order for userA: Buy 1 BTC at Rp110,00 (x1)." + Environment.NewLine +
                                     "\n--- Final Simulation Report ---" + Environment.NewLine +
                                     "Final User Balances:" + Environment.NewLine +
                                     "  userA: Rp1.200,00" + Environment.NewLine +
                                     "  userB: Rp800,00" + Environment.NewLine +
                                     "\nTrade History:" + Environment.NewLine +
                                     "  No trades executed." + Environment.NewLine +
                                     "------------------------------------------" + Environment.NewLine;

                Assert.Equal(expectedOutput, stringWriter.ToString());
            }
            finally
            {
                // Restore the original console output stream
                Console.SetOut(originalConsoleOut);
            }
        }
        
    }
}