using System;
using System.Collections.Generic;

namespace PerpDEXSimulator.Models
{
    public enum ActionTypes {PlaceOrder, ApplyFunding, PriceUpdate}
     
    public enum OrderSides { Buy, Sell }
    
    public enum PositionSides {Long, Short}

    public class User
    {
        public string? Id { get; set; } 
        public decimal Collateral { get; set; } 
        public Position CurrentPosition { get; set; } = new Position(); 
    }

    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public string? UserId { get; set; } 
        public OrderSides? Side { get; set; } 
        public decimal Quantity { get; set; } 
        public decimal Price { get; set; } 
        public decimal Leverage { get; set; } 
        public decimal InitialMarginRequirement { get; set; } 
        public DateTime PlacedTime { get; set; } = DateTime.UtcNow; 
    }

    public class Position
    {
        public PositionSides? Side { get; set; } 
        public decimal Quantity { get; set; }
        public decimal EntryPrice { get; set; } 
        public decimal Leverage { get; set; } 
        public decimal UnrealizedPnl { get; set; } 
        public decimal MaintenanceMarginRequirement { get; set; } 
        public bool IsOpen => Quantity > 0; 
    }

    public class Trade
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; 
        public string? BuyerId { get; set; }
        public string? SellerId { get; set; }
        public decimal Quantity { get; set; } 
        public decimal Price { get; set; } 
        public Order? BuyOrder { get; set; } 
        public Order? SellOrder { get; set; } 
    }

    public class UserConfig
    {
        public string? Id { get; set; } 
        public decimal Collateral { get; set; } 
    }

    public class EventConfig
    {
        public int Time { get; set; } 
        public ActionTypes Action { get; set; } 
        public string? User { get; set; }
        public OrderSides Side { get; set; } 
        public decimal Quantity { get; set; } 
        public decimal Price { get; set; } 
        public decimal Leverage { get; set; } 
    }

    public class SimulatorConfig
    {
        public List<UserConfig>? Users { get; set; }
        public List<decimal>? Prices { get; set; } 
        public List<EventConfig>? Events { get; set; }
    }
}
