using System;
using System.Collections.Generic;
using Plugins;
using UnityEngine;

namespace Codes.InGame
{
    public class InGameLogicStatic : MonoSingleton<InGameLogicStatic>
    {
        public Dictionary<int,Player> players = new();
        
        public Dictionary<string,Mover> ingameMovers = new();
        
        
        protected override void Initialize()
        {
            players.Clear();
            ingameMovers.Clear();
        }

        public void PrepareToNewMatch()
        {
            players.Clear();
            ingameMovers.Clear();
        }
    
        private Player GetPlayerById(int id)
        {
            try
            {
                return players[id];
            }
            catch
            {
                return null;
            }
        }

        
    }
}
