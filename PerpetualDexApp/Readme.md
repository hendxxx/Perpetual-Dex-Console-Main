#Perpertual DEX App

## How to run
- dotnet restore
- dotnet build
- dotnet run 

## input.json
Put in root

for example:
```json
{
  "users": [
    {"id": "user1", "collateral": 20000.00},
    {"id": "user2", "collateral": 50000.00}
  ],
  "prices": [
    60000.00, 60500.00, 61000.00, 58000.00, 59200.00, 60000.00, 61000.00, 62000.00,  
    61500.00, 60800.00, 60100.00, 59500.00, 58900.00, 58000.00, 57500.00, 56000.00,  
    55000.00, 54500.00, 55200.00, 56000.00, 56500.00, 57000.00, 57500.00, 58000.00  
  ],
  "events": [
    {"time": 0, "action": "PlaceOrder", "user": "user1", "side": "Buy", "quantity": 1.5, "price": 59500.00, "leverage": 5},
    {"time": 0, "action": "PlaceOrder", "user": "user2", "side": "Sell", "quantity": 0.5, "price": 59500.00, "leverage": 5},
    {"time": 2, "action": "PlaceOrder", "user": "user1", "side": "Buy", "quantity": 0.2, "price": 58500.00, "leverage": 3},
    {"time": 4, "action": "PlaceOrder", "user": "user2", "side": "Sell", "quantity": 0.3, "price": 59000.00, "leverage": 4},
    {"time": 8, "action": "ApplyFunding"},
    {"time": 11, "action": "PriceUpdate",  "user": "", "price": 61500.00}

  ]
}
```

Example Result:
```shell
Perpetual Futures Trading Simulator (C#)
------------------------------------------
Configuration loaded successfully.
Starting simulation with 24 price points over 24 hours.
Initial User Balances:
  user1: Rp20.000,00
  user2: Rp50.000,00
------------------------------------------

--- Simulated Hour 0: Mark Price = Rp60.000,00 ---
Placing order for user1: Buy 1,5 BTC at Rp59.500,00 (x5).
Placing order for user2: Sell 0,5 BTC at Rp59.500,00 (x5).
  user1 used Rp5.950,00 margin to Long 0,5 BTC. New Collateral: Rp14.050,00
  user2 used Rp5.950,00 margin to Short 0,5 BTC. New Collateral: Rp44.050,00

--- Hourly Summary (End of Hour 0) ---
  User: user1, Collateral: Rp14.050,00
    Position: Long 0,5 BTC, Entry: Rp59.500,00, PNL: Rp250,00, maine. Margin: Rp1.500,00
  User: user2, Collateral: Rp44.050,00
    Position: Short 0,5 BTC, Entry: Rp59.500,00, PNL: -Rp250,00, maine. Margin: Rp1.500,00
------------------------------------------

--- Simulated Hour 1: Mark Price = Rp60.500,00 ---

--- Hourly Summary (End of Hour 1) ---
  User: user1, Collateral: Rp14.050,00
    Position: Long 0,5 BTC, Entry: Rp59.500,00, PNL: Rp500,00, maine. Margin: Rp1.512,50
  User: user2, Collateral: Rp44.050,00
    Position: Short 0,5 BTC, Entry: Rp59.500,00, PNL: -Rp500,00, maine. Margin: Rp1.512,50
------------------------------------------

--- Simulated Hour 2: Mark Price = Rp61.000,00 ---
Placing order for user1: Buy 0,2 BTC at Rp58.500,00 (x3).

--- Hourly Summary (End of Hour 2) ---
  User: user1, Collateral: Rp14.050,00
    Position: Long 0,5 BTC, Entry: Rp59.500,00, PNL: Rp750,00, maine. Margin: Rp1.525,00
  User: user2, Collateral: Rp44.050,00
    Position: Short 0,5 BTC, Entry: Rp59.500,00, PNL: -Rp750,00, maine. Margin: Rp1.525,00
------------------------------------------

--- Simulated Hour 3: Mark Price = Rp58.000,00 ---

--- Hourly Summary (End of Hour 3) ---
  User: user1, Collateral: Rp14.050,00
    Position: Long 0,5 BTC, Entry: Rp59.500,00, PNL: -Rp750,00, maine. Margin: Rp1.450,00
  User: user2, Collateral: Rp44.050,00
    Position: Short 0,5 BTC, Entry: Rp59.500,00, PNL: Rp750,00, maine. Margin: Rp1.450,00
------------------------------------------

--- Simulated Hour 4: Mark Price = Rp59.200,00 ---
Placing order for user2: Sell 0,3 BTC at Rp59.000,00 (x4).
  user1 used Rp3.570,00 margin to Long 0,3 BTC. New Collateral: Rp10.480,00
  user2 used Rp4.462,50 margin to Short 0,3 BTC. New Collateral: Rp39.587,50

--- Hourly Summary (End of Hour 4) ---
  User: user1, Collateral: Rp10.480,00
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp240,00, maine. Margin: Rp2.368,00
  User: user2, Collateral: Rp39.587,50
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp240,00, maine. Margin: Rp2.368,00
------------------------------------------

--- Simulated Hour 5: Mark Price = Rp60.000,00 ---

--- Hourly Summary (End of Hour 5) ---
  User: user1, Collateral: Rp10.480,00
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: Rp400,00, maine. Margin: Rp2.400,00
  User: user2, Collateral: Rp39.587,50
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp400,00, maine. Margin: Rp2.400,00
------------------------------------------

--- Simulated Hour 6: Mark Price = Rp61.000,00 ---

--- Hourly Summary (End of Hour 6) ---
  User: user1, Collateral: Rp10.480,00
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: Rp1.200,00, maine. Margin: Rp2.440,00
  User: user2, Collateral: Rp39.587,50
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp1.200,00, maine. Margin: Rp2.440,00
------------------------------------------

--- Simulated Hour 7: Mark Price = Rp62.000,00 ---

--- Hourly Summary (End of Hour 7) ---
  User: user1, Collateral: Rp10.480,00
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: Rp2.000,00, maine. Margin: Rp2.480,00
  User: user2, Collateral: Rp39.587,50
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp2.000,00, maine. Margin: Rp2.480,00
------------------------------------------

--- Simulated Hour 8: Mark Price = Rp61.500,00 ---
--- Event: Applying Funding ---
Calculated Funding Rate: 0,0125% for period.
user1 (Long) paid funding: Rp6,21. New Collateral: Rp10.473,79
user2 (Short) received funding: Rp6,21. New Collateral: Rp39.593,71
Applying funding for hour 8...
Calculated Funding Rate: 0,0125% for period.
user1 (Long) paid funding: Rp6,16. New Collateral: Rp10.467,64
user2 (Short) received funding: Rp6,16. New Collateral: Rp39.599,86

--- Hourly Summary (End of Hour 8) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: Rp1.600,00, maine. Margin: Rp2.460,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp1.600,00, maine. Margin: Rp2.460,00
------------------------------------------

--- Simulated Hour 9: Mark Price = Rp60.800,00 ---

--- Hourly Summary (End of Hour 9) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: Rp1.040,00, maine. Margin: Rp2.432,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp1.040,00, maine. Margin: Rp2.432,00
------------------------------------------

--- Simulated Hour 10: Mark Price = Rp60.100,00 ---

--- Hourly Summary (End of Hour 10) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: Rp480,00, maine. Margin: Rp2.404,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp480,00, maine. Margin: Rp2.404,00
------------------------------------------

--- Simulated Hour 11: Mark Price = Rp59.500,00 ---
--- Event: Price Updated to Rp61.500,00 ---

--- Hourly Summary (End of Hour 11) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: Rp0,00, maine. Margin: Rp2.380,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp0,00, maine. Margin: Rp2.380,00
------------------------------------------

--- Simulated Hour 12: Mark Price = Rp58.900,00 ---

--- Hourly Summary (End of Hour 12) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp480,00, maine. Margin: Rp2.356,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp480,00, maine. Margin: Rp2.356,00
------------------------------------------

--- Simulated Hour 13: Mark Price = Rp58.000,00 ---

--- Hourly Summary (End of Hour 13) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp1.200,00, maine. Margin: Rp2.320,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp1.200,00, maine. Margin: Rp2.320,00
------------------------------------------

--- Simulated Hour 14: Mark Price = Rp57.500,00 ---

--- Hourly Summary (End of Hour 14) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp1.600,00, maine. Margin: Rp2.300,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp1.600,00, maine. Margin: Rp2.300,00
------------------------------------------

--- Simulated Hour 15: Mark Price = Rp56.000,00 ---

--- Hourly Summary (End of Hour 15) ---
  User: user1, Collateral: Rp10.467,64
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp2.800,00, maine. Margin: Rp2.240,00
  User: user2, Collateral: Rp39.599,86
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp2.800,00, maine. Margin: Rp2.240,00
------------------------------------------

--- Simulated Hour 16: Mark Price = Rp55.000,00 ---
Applying funding for hour 16...
Calculated Funding Rate: 0,0125% for period.
user1 (Long) paid funding: Rp5,51. New Collateral: Rp10.462,13
user2 (Short) received funding: Rp5,51. New Collateral: Rp39.605,37

--- Hourly Summary (End of Hour 16) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp3.600,00, maine. Margin: Rp2.200,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp3.600,00, maine. Margin: Rp2.200,00
------------------------------------------

--- Simulated Hour 17: Mark Price = Rp54.500,00 ---

--- Hourly Summary (End of Hour 17) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp4.000,00, maine. Margin: Rp2.180,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp4.000,00, maine. Margin: Rp2.180,00
------------------------------------------

--- Simulated Hour 18: Mark Price = Rp55.200,00 ---

--- Hourly Summary (End of Hour 18) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp3.440,00, maine. Margin: Rp2.208,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp3.440,00, maine. Margin: Rp2.208,00
------------------------------------------

--- Simulated Hour 19: Mark Price = Rp56.000,00 ---

--- Hourly Summary (End of Hour 19) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp2.800,00, maine. Margin: Rp2.240,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp2.800,00, maine. Margin: Rp2.240,00
------------------------------------------

--- Simulated Hour 20: Mark Price = Rp56.500,00 ---

--- Hourly Summary (End of Hour 20) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp2.400,00, maine. Margin: Rp2.260,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp2.400,00, maine. Margin: Rp2.260,00
------------------------------------------

--- Simulated Hour 21: Mark Price = Rp57.000,00 ---

--- Hourly Summary (End of Hour 21) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp2.000,00, maine. Margin: Rp2.280,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp2.000,00, maine. Margin: Rp2.280,00
------------------------------------------

--- Simulated Hour 22: Mark Price = Rp57.500,00 ---

--- Hourly Summary (End of Hour 22) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp1.600,00, maine. Margin: Rp2.300,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp1.600,00, maine. Margin: Rp2.300,00
------------------------------------------

--- Simulated Hour 23: Mark Price = Rp58.000,00 ---

--- Hourly Summary (End of Hour 23) ---
  User: user1, Collateral: Rp10.462,13
    Position: Long 0,8 BTC, Entry: Rp59.500,00, PNL: -Rp1.200,00, maine. Margin: Rp2.320,00
  User: user2, Collateral: Rp39.605,37
    Position: Short 0,8 BTC, Entry: Rp59.500,00, PNL: Rp1.200,00, maine. Margin: Rp2.320,00
------------------------------------------

--- Simulation Complete ---

--- Final Simulation Report ---
Final User Balances:
  user1: Rp10.462,13
  user2: Rp39.605,37

Trade History:
  Trade 48718115-b115-4246-8ba4-a955630e395e: user1 bought 0,5 BTC at Rp59.500,00 from user2 at 16/10/2025 00:01:27
  Trade b5f289a5-af6e-484f-bff2-8ee18f82c53e: user1 bought 0,3 BTC at Rp59.500,00 from user2 at 16/10/2025 00:01:27
------------------------------------------
```
