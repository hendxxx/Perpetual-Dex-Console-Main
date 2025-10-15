using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using PerpDEXSimulator.Models; 
using PerpDEXSimulator.Trading;

namespace PerpDEXSimulator
{
    public class Program
    {
        private static Exchange? _exchange;
        private static List<decimal>? _priceOracleData;
        private static List<EventConfig>? _simulationEvents;
        private static int _currentHour = 0;
        private const int fundingIntervalHours = 8;

        public static void Main(string[] args)
        {
            Console.WriteLine("Perpetual Futures Trading Simulator (C#)");
            Console.WriteLine("------------------------------------------");

            //Get base directory
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // load input.json file
            var config = loadConfiguration(baseDirectory + "input.json");
            if (config == null) return;

            // Initialize users, and price data and events
            _exchange = new Exchange(config.Users?? []);
            _priceOracleData = config.Prices;
            _simulationEvents = [.. (config.Events ?? []).OrderBy(e => e.Time)];

            //Welcome message
            Console.WriteLine($"Starting simulation with {_priceOracleData?.Count} price points over {_priceOracleData?.Count} hours.");
            
            //show user balances
            Console.WriteLine("Initial User Balances:");
            foreach (var user in _exchange.getUsers().Values) 
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
                // processScheduledEvents(config.Events.Where(e => e.Time == _currentHour).ToList()); 
                _exchange.setMarkPrice(currentMarkPrice);

                if (_currentHour > 0 && _currentHour % fundingIntervalHours == 0)
                {
                    // apply funding payments
                    Console.WriteLine($"Applying funding for hour {_currentHour}...");
                    // _exchange.applyFunding(); 
                }
                
                // show summary hourly
                // displayHourlySummary(); 
            }

            //Simulation complete
            Console.WriteLine("\n--- Simulation Complete ---");
            
            //Generate final report
            // generateFinalReport(); 
        }

        private static SimulatorConfig? loadConfiguration(string filePath)
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
    }   
}