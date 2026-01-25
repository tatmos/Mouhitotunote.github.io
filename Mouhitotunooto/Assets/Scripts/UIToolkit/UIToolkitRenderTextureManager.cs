using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

namespace NovelGame
{
    /// <summary>
    /// UI ToolkitをRender Textureに描き出し、パーティクルとの前後関係を制御するクラス
    /// </summary>
    public class UIToolkitRenderTextureManager : MonoBehaviour
    {
        private RenderTexture uiRenderTexture;
        private RawImage uiDisplayImage;
        private Canvas uiDisplayCanvas;
        private List<PanelSettings> originalPanelSettings = new List<PanelSettings>();
        
        [Header("Render Texture Settings")]
        [SerializeField] private int renderTextureWidth = 960;
        [SerializeField] private int renderTextureHeight = 540;
        [SerializeField] private int renderTextureDepth = 24;
        
        [Header("Display Settings")]
        [SerializeField] private int displayCanvasSortOrder = 0; // パーティクルより後ろに表示（0に変更してUIが表示されるように）
        
        /// <summary>
        /// Render Texture方式を初期化
        /// </summary>
        public void Initialize()
        {
            // Render Textureを作成
            CreateRenderTexture();
            
            // すべてのUIDocumentのPanelSettingsを取得し、Render Textureを設定
            SetupPanelSettingsForRenderTexture();
            
            // Render Textureを表示するRaw Imageを作成
            CreateUIDisplayCanvas();
        }
        
        /// <summary>
        /// Render Textureを作成
        /// </summary>
        private void CreateRenderTexture()
        {
            uiRenderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, renderTextureDepth, RenderTextureFormat.ARGB32);
            uiRenderTexture.name = "UIToolkitRenderTexture";
            uiRenderTexture.Create();
        }
        
        /// <summary>
        /// すべてのPanelSettingsにRender Textureを設定
        /// </summary>
        private void SetupPanelSettingsForRenderTexture()
        {
            var allPanelSettings = Resources.FindObjectsOfTypeAll<PanelSettings>();
            if (allPanelSettings != null && allPanelSettings.Length > 0)
            {
                foreach (var panel in allPanelSettings)
                {
                    if (panel != null)
                    {
                        // 元の設定を保存（復元用）
                        originalPanelSettings.Add(panel);
                        
                        // PanelSettingsのtargetTextureはpublic RenderTextureプロパティなので直接設定可能
                        panel.targetTexture = uiRenderTexture;
                    }
                }
            }
            else
            {
                Debug.LogWarning("PanelSettingsが見つかりませんでした。");
            }
        }
        
        /// <summary>
        /// Render Textureを表示するCanvasを作成
        /// </summary>
        private void CreateUIDisplayCanvas()
        {
            // Canvasを作成
            GameObject canvasObject = new GameObject("UIDisplayCanvas");
            canvasObject.transform.SetParent(transform, false);
            
            uiDisplayCanvas = canvasObject.AddComponent<Canvas>();
            uiDisplayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiDisplayCanvas.worldCamera = Camera.main;
            uiDisplayCanvas.sortingOrder = displayCanvasSortOrder; // パーティクルより後ろに表示
            
            // CanvasScalerは使用しない（RenderTextureをそのまま表示するため）
            // RenderTextureのサイズ（960x540）とPanelSettingsのReference Resolution（960x540）が一致しているため、
            // CanvasScalerを使わずに、RawImageを直接画面サイズに合わせる
            
            // Raw Imageを作成（Render Textureを表示）
            GameObject imageObject = new GameObject("UIDisplayImage");
            imageObject.transform.SetParent(canvasObject.transform, false);
            
            RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
            // 画面全体をカバーするように設定
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            uiDisplayImage = imageObject.AddComponent<RawImage>();
            uiDisplayImage.texture = uiRenderTexture;
            uiDisplayImage.raycastTarget = false; // クリック判定を無効化（UI ToolkitのUIがクリック可能なため）
            
            // RenderTextureを画面全体に表示するため、uvRectを調整
            // RenderTextureのアスペクト比と画面のアスペクト比が異なる場合に備えて、
            // アスペクト比を維持しながら画面全体に表示
            float renderTextureAspect = (float)renderTextureWidth / renderTextureHeight;
            float screenAspect = (float)Screen.width / Screen.height;
            
            if (renderTextureAspect > screenAspect)
            {
                // RenderTextureの方が横長の場合：高さを基準に表示
                float scale = (float)Screen.height / renderTextureHeight;
                float scaledWidth = renderTextureWidth * scale;
                float uvWidth = Screen.width / scaledWidth;
                float uvOffsetX = (1.0f - uvWidth) * 0.5f;
                uiDisplayImage.uvRect = new Rect(uvOffsetX, 0, uvWidth, 1.0f);
            }
            else
            {
                // 画面の方が横長の場合：幅を基準に表示
                float scale = (float)Screen.width / renderTextureWidth;
                float scaledHeight = renderTextureHeight * scale;
                float uvHeight = Screen.height / scaledHeight;
                float uvOffsetY = (1.0f - uvHeight) * 0.5f;
                uiDisplayImage.uvRect = new Rect(0, uvOffsetY, 1.0f, uvHeight);
            }
        }
        
        /// <summary>
        /// Render Texture方式をクリーンアップ
        /// </summary>
        public void Cleanup()
        {
            // PanelSettingsのtargetTextureを元に戻す
            foreach (var panel in originalPanelSettings)
            {
                if (panel != null)
                {
                    panel.targetTexture = null;
                }
            }
            originalPanelSettings.Clear();
            
            // Render Textureを解放
            if (uiRenderTexture != null)
            {
                uiRenderTexture.Release();
                Destroy(uiRenderTexture);
                uiRenderTexture = null;
            }
            
            // Canvasを削除
            if (uiDisplayCanvas != null)
            {
                Destroy(uiDisplayCanvas.gameObject);
                uiDisplayCanvas = null;
            }
        }
        
        void OnDestroy()
        {
            Cleanup();
        }
    }
}
