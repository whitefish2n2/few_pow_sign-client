using System.Collections.Generic;
using System.Threading.Tasks;
using Codes.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CharacterPortraitSpriteHolder : MonoBungleton<CharacterPortraitSpriteHolder>
{
    protected override void Initialize()
    { }

    [SerializeField] private SerializableDictionary<string, Sprite> Sprites;

    public async Task<Sprite> GetSprite(string name)
    {
        if (!Sprites.ContainsKey(name))
        {
            var sprite = await Addressables.LoadAssetAsync<Sprite>($"sprite/{name}");
            Sprites.Add(new SerializableDictionary<string, Sprite>.Pair(name,sprite));
            return sprite;
        }
        else return Sprites[name];
    }
    
    public void ReleaseAll()
    {
        foreach (var sprite in Sprites.Values)
        {
            Addressables.Release(sprite);
        }
        Sprites.Clear();
    }

    protected override void OnDestroy()
    {
        ReleaseAll();
        base.OnDestroy();
    }
}
