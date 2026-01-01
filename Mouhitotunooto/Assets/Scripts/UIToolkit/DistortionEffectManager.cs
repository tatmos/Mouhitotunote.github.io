using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// 背景歪み効果を管理するクラス
    /// </summary>
    public class DistortionEffectManager : MonoBehaviour
    {
        [Header("Distortion Settings")]
        [SerializeField] private Material distortionMaterial;
        
        private const float DistortionUpdateInterval = 0.2f; // 更新間隔（5FPS = 0.2秒）
        private const float DistortionResolutionScale = 0.4f; // 解像度スケール（0.4 = 40%の解像度）
        
        public void SetDistortionMaterial(Material material)
        {
            distortionMaterial = material;
        }

        // RenderTextureを使用した歪み効果のための変数
        private RenderTexture distortionRenderTexture;
        private Texture2D currentDistortionSourceTexture;
        private VisualElement currentDistortionElement;
        private Coroutine distortionUpdateCoroutine;
        private Texture2D distortionTexture2D; // 再利用するTexture2D
        
        // 背景テクスチャのキャッシュ（VisualElement → Texture2D）
        private Dictionary<VisualElement, Texture2D> backgroundTextureCache = new Dictionary<VisualElement, Texture2D>();

        private void OnDestroy()
        {
            CleanupDistortionEffect();
        }

        /// <summary>
        /// 背景に歪み効果を適用
        /// </summary>
        public void ApplyBackgroundDistortion(VisualElement backgroundImage, bool isDarkMode)
        {
            if (backgroundImage == null)
            {
                Debug.LogWarning("[DistortionEffectManager] backgroundImage is null");
                return;
            }

            // 既存のコルーチンを停止
            if (distortionUpdateCoroutine != null)
            {
                StopCoroutine(distortionUpdateCoroutine);
                distortionUpdateCoroutine = null;
            }

            if (isDarkMode && distortionMaterial != null)
            {
                // ダークモード時のみ、RenderTextureを使用した歪み効果を適用
                SetupDistortionEffect(backgroundImage);
            }
            else
            {
                // ダークモードでない場合は、歪み効果を無効化
                CleanupDistortionEffect(backgroundImage);
            }
        }

        private void SetupDistortionEffect(VisualElement backgroundImage)
        {
            // 元のテクスチャを取得
            Texture2D sourceTexture = null;
            var styleBg = backgroundImage.style.backgroundImage;
            if (styleBg != null && styleBg.value != null)
            {
                var bg = styleBg.value;
                if (bg.texture != null)
                {
                    sourceTexture = bg.texture;
                }
            }

            if (sourceTexture == null)
            {
                // テクスチャが見つからない場合は、キャッシュから取得を試みる
                if (backgroundTextureCache.TryGetValue(backgroundImage, out sourceTexture) && sourceTexture != null)
                {
                    // キャッシュから取得成功
                }
                else
                {
                    Debug.LogWarning("[DistortionEffectManager] Source texture not found");
                    return;
                }
            }
            else
            {
                // キャッシュに保存
                backgroundTextureCache[backgroundImage] = sourceTexture;
            }

            currentDistortionSourceTexture = sourceTexture;
            currentDistortionElement = backgroundImage;

            // RenderTextureをセットアップ（解像度を下げてパフォーマンスを向上）
            int renderWidth = Mathf.Max(1, (int)(sourceTexture.width * DistortionResolutionScale));
            int renderHeight = Mathf.Max(1, (int)(sourceTexture.height * DistortionResolutionScale));
            
            if (distortionRenderTexture == null)
            {
                distortionRenderTexture = new RenderTexture(renderWidth, renderHeight, 0, RenderTextureFormat.ARGB32);
                distortionRenderTexture.Create();
            }
            else if (distortionRenderTexture.width != renderWidth || distortionRenderTexture.height != renderHeight)
            {
                distortionRenderTexture.Release();
                distortionRenderTexture = new RenderTexture(renderWidth, renderHeight, 0, RenderTextureFormat.ARGB32);
                distortionRenderTexture.Create();
            }

            // Texture2Dを再利用（毎回新規作成しない）
            if (distortionTexture2D == null || distortionTexture2D.width != renderWidth || distortionTexture2D.height != renderHeight)
            {
                if (distortionTexture2D != null)
                {
                    Destroy(distortionTexture2D);
                }
                distortionTexture2D = new Texture2D(renderWidth, renderHeight, TextureFormat.RGBA32, false);
            }

            // Graphics.Blitを使用して歪みシェーダーを適用
            Graphics.Blit(sourceTexture, distortionRenderTexture, distortionMaterial);

            // RenderTextureの内容をTexture2DにコピーしてUIに設定（初回のみ）
            UpdateDistortionTexture();

            // 定期的にRenderTextureを更新するコルーチンを開始（更新頻度を下げる）
            distortionUpdateCoroutine = StartCoroutine(UpdateDistortionEffect());
        }

        /// <summary>
        /// RenderTextureの内容をTexture2Dにコピー（アロケーションを最小限に）
        /// </summary>
        private void UpdateDistortionTexture()
        {
            if (distortionTexture2D == null || distortionRenderTexture == null || currentDistortionElement == null)
                return;

            // RenderTextureの内容を再利用可能なTexture2Dにコピー
            RenderTexture.active = distortionRenderTexture;
            distortionTexture2D.ReadPixels(new Rect(0, 0, distortionRenderTexture.width, distortionRenderTexture.height), 0, 0);
            distortionTexture2D.Apply();
            RenderTexture.active = null;

            // UIの背景画像を更新
            currentDistortionElement.style.backgroundImage = new StyleBackground(distortionTexture2D);
        }

        private IEnumerator UpdateDistortionEffect()
        {
            while (currentDistortionElement != null && currentDistortionSourceTexture != null)
            {
                // Graphics.Blitを使用して歪みシェーダーを適用（時間ベースの歪みが動的に更新される）
                Graphics.Blit(currentDistortionSourceTexture, distortionRenderTexture, distortionMaterial);
                
                // RenderTextureの内容をTexture2Dにコピー（アロケーションを最小限に）
                UpdateDistortionTexture();

                // 更新頻度を下げてパフォーマンスを向上（5FPS = 0.2秒間隔）
                yield return new WaitForSeconds(DistortionUpdateInterval);
            }
        }

        private void CleanupDistortionEffect(VisualElement backgroundImage = null)
        {
            // コルーチンを停止
            if (distortionUpdateCoroutine != null)
            {
                StopCoroutine(distortionUpdateCoroutine);
                distortionUpdateCoroutine = null;
            }

            // 元のテクスチャに戻す
            if (backgroundImage != null && backgroundTextureCache.TryGetValue(backgroundImage, out Texture2D originalTexture))
            {
                if (originalTexture != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(originalTexture);
                }
            }

            // RenderTextureを解放
            if (distortionRenderTexture != null)
            {
                distortionRenderTexture.Release();
                distortionRenderTexture = null;
            }

            // Texture2Dを削除
            if (distortionTexture2D != null)
            {
                Destroy(distortionTexture2D);
                distortionTexture2D = null;
            }

            currentDistortionSourceTexture = null;
            currentDistortionElement = null;
        }
    }
}

