using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using PerpetualDexApp.Models; 
using PerpetualDexApp.Trading;

namespace PerpetualDexApp
{
    public class Program
    {
        public static Exchange? _exchange;
        public static List<decimal>? _priceOracleData;
        public static List<EventConfig>? _simulationEvents;
        public static int _currentHour = 0;
        public const int fundingIntervalHours = 8;

        public static void Main(string[] args)
        {
            Console.WriteLine("Perpetual Futures Trading Simulator (C#)");
            Console.WriteLine("------------------------------------------");

            //Get base directory
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // load input.json file
            var config = LoadConfiguration(baseDirectory + "input.json");
            if (config == null) return;

            // Initialize users, and price data and events
            _exchange = new Exchange(config.Users?? []);
            _priceOracleData = config.Prices;
            _simulationEvents = [.. (config.Events ?? []).OrderBy(e => e.Time)];

            //Welcome message
            Console.WriteLine($"Starting simulation with {_priceOracleData?.Count} price points over {_priceOracleData?.Count} hours.");
            
            //show user balances
            Console.WriteLine("Initial User Balances:");
            foreach (var user in _exchange.GetUsers().Values) 
            {
                Console.WriteLine($"  {user.Id}: {user.Collateral:C}"); 
            }
            Console.WriteLine("------------------------------------------");

            // Main simulation loop
            for (_currentHour = 0; _currentHour < _priceOracleData?.Count; _currentHour++)
            {
                // show current hour and mark price
                decimal currentMarkPrice = _priceOracleData[_currentHour]; 
                Console.WriteLine($"\n--- Simulated Hour {_currentHour}: Mark Price = {currentMarkPrice:C} ---");

                // process user actions and events for the hour
                ProcessScheduledEvents([.. (config.Events ?? []).Where(e => e.Time == _currentHour)]); 
                _exchange.SetMarkPrice(currentMarkPrice);

                if (_currentHour > 0 && _currentHour % fundingIntervalHours == 0)
                {
                    // apply funding payments
                    Console.WriteLine($"Applying funding for hour {_currentHour}...");
                    _exchange.ApplyFunding(); 
                }
                
                // show summary hourly
                DisplayHourlySummary(); 
            }

            //Simulation complete
            Console.WriteLine("\n--- Simulation Complete ---");
            
            //Generate final report
            GenerateFinalReport(); 
        }

        public static SimulatorConfig? LoadConfiguration(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new JsonStringEnumConverter());

                var config = JsonSerializer.Deserialize<SimulatorConfig>(jsonString, options);
                Console.WriteLine("Configuration loaded successfully.");
                return config;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: Configuration file not found at {filePath}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON configuration: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred loading configuration: {ex.Message}");
            }
            return null;
        }

        public static void ProcessScheduledEvents(List<EventConfig> events)
        {
            foreach (var evt in events)
            {
                switch (evt.Action)
                {
                    case ActionTypes.PlaceOrder:
                        var order = new Order
                        {
                            UserId = evt.User,
                            Side = evt.Side,
                            Quantity = evt.Quantity,
                            Price = evt.Price,
                            Leverage = evt.Leverage,
                            PlacedTime = DateTime.UtcNow
                        };
                        _exchange?.PlaceOrder(order);
                        break;
                    case ActionTypes.PriceUpdate:
                        if (evt.Price > 0)
                        {
                            Console.WriteLine($"--- Event: Price Updated to {evt.Price:C} ---");
                            _exchange?.SetMarkPrice(evt.Price);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: PriceUpdate event specified with invalid price: {evt.Price}");
                        }
                        break;
                    case ActionTypes.ApplyFunding:
                        Console.WriteLine($"--- Event: Applying Funding ---");
                        _exchange?.ApplyFunding();
                        break;
                    default:
                        Console.WriteLine($"Warning: Unhandled event action '{evt.Action}' at hour {evt.Time}.");
                        break;
                }
            }

        }
        public static void DisplayHourlySummary() 
        {
            Console.WriteLine($"\n--- Hourly Summary (End of Hour {_currentHour}) ---");
            if (_exchange != null)
            {
                foreach (var user in _exchange.GetUsers().Values)
                {
                    Console.WriteLine($"  User: {user.Id}, Collateral: {user.Collateral:C}"); 
                    if (user.CurrentPosition.IsOpen)
                    {
                        var pos = user.CurrentPosition;
                        Console.WriteLine($"    Position: {pos.Side} {pos.Quantity} BTC, Entry: {pos.EntryPrice:C}, PNL: {pos.UnrealizedPnl:C}, maine. Margin: {pos.MaintenanceMarginRequirement:C}");
                    }
                    else
                    {
                        Console.WriteLine("    No open position.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Exchange is not initialized.");
            }
            Console.WriteLine("------------------------------------------");
        }

        public static void GenerateFinalReport() 
        {
            Console.WriteLine("\n--- Final Simulation Report ---");
            Console.WriteLine("Final User Balances:");
            if (_exchange != null)
            {
                foreach (var user in _exchange.GetUsers().Values)
                {
                    Console.WriteLine($"  {user.Id}: {user.Collateral:C}"); 
                }
            }
            else
            {
                Console.WriteLine("Exchange is not initialized.");
            }

            Console.WriteLine("\nTrade History:");
            if (_exchange != null)
            {
                var tradeHistory = _exchange.GetTradeHistory(); 
                if (tradeHistory.Any())
                {
                    foreach (var trade in tradeHistory)
                    {
                        Console.WriteLine($"  Trade {trade.Id}: {trade.BuyerId} bought {trade.Quantity} BTC at {trade.Price:C} from {trade.SellerId} at {trade.Timestamp}"); 
                    }
                }
                else
                {
                    Console.WriteLine("  No trades executed.");
                }
            }
            else
            {
                Console.WriteLine("Exchange is not initialized.");
            }

            Console.WriteLine("------------------------------------------");
        }
    }   
}