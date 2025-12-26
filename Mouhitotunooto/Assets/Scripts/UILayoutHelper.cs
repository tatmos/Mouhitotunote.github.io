using UnityEngine;
using UnityEngine.UI;

namespace NovelGame
{
    /// <summary>
    /// UIのレイアウトを自動的に設定するヘルパークラス
    /// </summary>
    public static class UILayoutHelper
    {
        /// <summary>
        /// スクロール可能なコンテンツエリアを作成
        /// </summary>
        public static GameObject CreateScrollableContent(Transform parent, string name = "ScrollContent")
        {
            // ScrollViewを作成
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);
            
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var image = scrollView.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0); // 透明
            
            // Viewportを作成
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            
            var viewportMask = viewport.AddComponent<Mask>();
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.1f);
            
            // Contentを作成
            GameObject content = new GameObject(name);
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;
            
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            return content;
        }

        /// <summary>
        /// 画面全体を覆うパネルを作成
        /// </summary>
        public static GameObject CreateFullScreenPanel(Transform parent, string name, Color? backgroundColor = null)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            var image = panel.AddComponent<Image>();
            image.color = backgroundColor ?? new Color(1, 1, 1, 0.95f);
            
            return panel;
        }
    }
}


