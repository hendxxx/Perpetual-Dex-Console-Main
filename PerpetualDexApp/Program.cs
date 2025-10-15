using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using PerpDEXSimulator.Models; 

namespace PerpDEXSimulator
{
    public class Program
    {

        private static List<decimal>? _priceOracleData;
        private static List<EventConfig>? _simulationEvents;
        private static int _currentHour = 0;
        private const int fundingIntervalHours = 8;

        public static void Main(string[] args)
        {
            Console.WriteLine("Perpetual Futures Trading Simulator (C#)");
            Console.WriteLine("------------------------------------------");

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var config = loadConfiguration(baseDirectory + "input.json");
            if (config == null) return;
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