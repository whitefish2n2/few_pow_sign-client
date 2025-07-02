using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codes.InGame
{
    public class MatchStatic : MonoBehaviour
    {
        public Dictionary<int,Player> Players = new();
        
        public Dictionary<string,Mover> IngameMovers = new();
        private string score;

        /// <summary>
        /// 여깄는 score 참조해서 사용해줬음 함
        /// </summary>
        public event Action OnScoreChanged;
        public void ChangeScore(string newScore)
        {
            score = newScore;
            OnScoreChanged?.Invoke();
        }
    
    
    
    
    
    
    
    
    
    
    
    
    
        private Player GetPlayerById(int id)
        {
            try
            {
                return Players[id];
            }
            catch
            {
                return null;
            }
        }
    }
}
