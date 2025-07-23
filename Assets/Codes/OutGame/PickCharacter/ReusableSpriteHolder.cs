using Codes.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Codes.OutGame.PickCharacter
{
    public class ReusableSpriteHolder : MonoBungleton<ReusableSpriteHolder>
    {
        protected override void Initialize()
        { }

        [SerializeField] private SerializableDictionary<string, Sprite> sprites;

    
        private const string PortraitPrefix = "sprite_character_portrait_";
        public async UniTask<Sprite> GetSprite(string name)
        {
        
            if (!name.StartsWith(PortraitPrefix))
                name = PortraitPrefix + name;
            if (!sprites.ContainsKey(name))
            {
                var sprite = await Addressables.LoadAssetAsync<Sprite>($"sprite/{name}").ToUniTask();
                sprites.Add(new SerializableDictionary<string, Sprite>.Pair(name,sprite));
                return sprite;
            }
            else return sprites[name];
        }
    
        public void ReleaseAll()
        {
            foreach (var sprite in sprites.Values)
            {
                Addressables.Release(sprite);
            }
            sprites.Clear();
        }

        protected override void OnDestroy()
        {
            ReleaseAll();
            base.OnDestroy();
        }
    }
}
