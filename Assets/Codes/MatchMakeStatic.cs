using System.Collections.Generic;
using NetCode;
using Plugins;
using UnityEngine;

public class MatchMakeStatic : MonoSingleton<MatchMakeStatic>
{
    private List<NewPlayerDto> playerConstructor = new List<NewPlayerDto>();
    protected override void Initialize()
    {
        
    }
}
