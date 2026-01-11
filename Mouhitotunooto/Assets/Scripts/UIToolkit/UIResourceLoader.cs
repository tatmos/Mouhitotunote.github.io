using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// UIリソースをResourcesフォルダから読み込むヘルパークラス
    /// </summary>
    public static class UIResourceLoader
    {
        /// <summary>
        /// オーディオクリップを読み込む
        /// </summary>
        public static AudioClip LoadAudioClip(string path)
        {
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogError($"[UIResourceLoader] AudioClip not found: {path}");
            }
            return clip;
        }

        /// <summary>
        /// スプライトを読み込む
        /// </summary>
        public static Sprite LoadSprite(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"[UIResourceLoader] Sprite not found: {path}");
            }
            return sprite;
        }

        /// <summary>
        /// マテリアルを読み込む
        /// </summary>
        public static Material LoadMaterial(string path)
        {
            var material = Resources.Load<Material>(path);
            if (material == null)
            {
                Debug.LogError($"[UIResourceLoader] Material not found: {path}");
            }
            return material;
        }

        /// <summary>
        /// VisualTreeAsset (UXML)を読み込む
        /// </summary>
        public static VisualTreeAsset LoadUXML(string path)
        {
            var uxml = Resources.Load<VisualTreeAsset>(path);
            if (uxml == null)
            {
                Debug.LogError($"[UIResourceLoader] VisualTreeAsset not found: {path}");
            }
            return uxml;
        }

        /// <summary>
        /// 複数のオーディオクリップを読み込む（配列用）
        /// </summary>
        public static AudioClip[] LoadAudioClips(string[] paths)
        {
            var clips = new AudioClip[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                clips[i] = LoadAudioClip(paths[i]);
            }
            return clips;
        }

        /// <summary>
        /// 複数のスプライトを読み込む（配列用）
        /// </summary>
        public static Sprite[] LoadSprites(string[] paths)
        {
            var sprites = new Sprite[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                sprites[i] = LoadSprite(paths[i]);
            }
            return sprites;
        }
    }
}
