using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Codes.InGame
{
    public class StaticInteractionManager : MonoBehaviour
    {
        public static StaticInteractionManager instance;
        public Color interactColor = Color.cyan;
        public List<GameObject> interactableObjects;
        private void Awake()
        {
            instance = this;
        }

        public void Interaction(string userId, string interactableId)
        {
            
        }
    }
}
