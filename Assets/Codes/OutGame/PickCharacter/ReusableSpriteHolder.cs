using System;
using System.Collections.Generic;
using Codes.Util;
using Cysharp.Threading.Tasks;
using Plugins;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Codes.OutGame.PickCharacter
{
    public class ReusableSpriteHolder : MonoSingleton<ReusableSpriteHolder>
    {
        protected override void Initialize()
        { }

        [SerializeField] private SerializableDictionary<string, Sprite> sprites;
        private Dictionary<string, UniTask<Sprite>> loadingTasks = new Dictionary<string, UniTask<Sprite>>();
        private HashSet<Sprite> addressableLoadedSprites = new HashSet<Sprite>();
    
        
        public async UniTask<Sprite> GetSprite(string name)
        {
            if (sprites.TryGetValue(name, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            if (loadingTasks.TryGetValue(name, out var ongoingTask))
            {
                return await ongoingTask;
            }
            
            var loadTask = LoadSpriteInternal(name);
            loadingTasks[name] = loadTask;
            
            var resultSprite = await loadTask;
            
            loadingTasks.Remove(name);

            return resultSprite;
        }
        
        private async UniTask<Sprite> LoadSpriteInternal(string spriteName)
        {
            try
            {
                var sprite = await Addressables.LoadAssetAsync<Sprite>($"Assets/sprite/{spriteName}");
                
                if (sprite != null)
                {
                    if (!sprites.ContainsKey(spriteName))
                    {
                        sprites.Add(new SerializableDictionary<string, Sprite>.Pair(spriteName, sprite));
                        
                        addressableLoadedSprites.Add(sprite);
                    }
                }
                return sprite;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReusableSpriteHolder] 스프라이트 로드 실패: {spriteName} / 에러: {e.Message}");
                return null;
            }
        }

        private const string PortraitPrefix = "sprite_character_portrait_";
        public async UniTask<Sprite> GetCharacterPortraitSprite(string characterId)
        {
            if (!characterId.StartsWith(PortraitPrefix))
                characterId = PortraitPrefix + characterId;
            return await GetSprite(characterId);
        }
        public void ReleaseAll()
        {
            foreach (var sprite in addressableLoadedSprites)
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
