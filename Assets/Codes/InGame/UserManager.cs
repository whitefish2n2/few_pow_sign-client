using System.Collections.Generic;
using UnityEngine;

namespace Codes.InGame
{
    public class UserManager : MonoBehaviour
    {
        public Dictionary<int,Player> IngameUsers = new();

        /*public void ChangeGun(int playerId)
        {
            GetPlayerById(playerId)
        }*/
    
    
    
    
    
    
    
    
    
    
    
    
    
        private Player GetPlayerById(int id)
        {
            try
            {
                return IngameUsers[id];
            }
            catch
            {
                return null;
            }
        }
    }
}
