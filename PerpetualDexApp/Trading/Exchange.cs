using System;
using System.Collections.Generic;
using System.Linq;
using PerpDEXSimulator.Models;

namespace PerpDEXSimulator.Trading
{
    public class Exchange
    {
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        private decimal _currentMarkPrice;

        //add user from input.json
        public Exchange(List<UserConfig> initialUsers)
        {
            foreach (var uc in initialUsers)
            {
                if (uc.Id != null)
                {
                    _users[uc.Id] = new User { Id = uc.Id, Collateral = uc.Collateral };
                }
                else
                {
                    throw new ArgumentNullException(nameof(uc.Id), "UserConfig.Id cannot be null.");
                }
            }
        }
        public void setMarkPrice(decimal newMarkPrice) 
        {
            _currentMarkPrice = newMarkPrice;
            // updateUsersPnl(); 
            // checkForLiquidations(); 
        }
        
        public Dictionary<string, User> getUsers() => _users;  
    }
}