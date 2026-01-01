using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// フェード効果を管理するクラス
    /// </summary>
    public class FadeEffectManager : MonoBehaviour
    {
        /// <summary>
        /// タイトルテキストをフェードアウト
        /// </summary>
        public IEnumerator FadeOutTitleText(VisualElement titleElement, float duration = 3.0f)
        {
            if (titleElement == null) yield break;
            
            // 初期opacityを取得（設定されていない場合は1.0）
            float startOpacity = 1.0f;
            if (titleElement.style.opacity.value > 0f)
            {
                startOpacity = titleElement.style.opacity.value;
            }
            else
            {
                titleElement.style.opacity = startOpacity;
            }
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float opacity = Mathf.Lerp(startOpacity, 0f, t);
                titleElement.style.opacity = opacity;
                yield return null;
            }
            
            // 完全に透明になったことを確認
            titleElement.style.opacity = 0f;
        }

        /// <summary>
        /// 背景オーバーレイをフェードイン
        /// </summary>
        public IEnumerator FadeInBackgroundOverlay(VisualElement overlay, float duration = 0.5f)
        {
            if (overlay == null) yield break;

            overlay.style.opacity = 0f;
            overlay.style.display = DisplayStyle.Flex;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float opacity = Mathf.Lerp(0f, 1f, elapsed / duration);
                overlay.style.opacity = opacity;
                yield return null;
            }

            overlay.style.opacity = 1f;
        }

        /// <summary>
        /// 背景オーバーレイをフェードアウト
        /// </summary>
        public IEnumerator FadeOutBackgroundOverlay(VisualElement overlay, float duration = 0.5f)
        {
            if (overlay == null) yield break;

            float startOpacity = overlay.style.opacity.value;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float opacity = Mathf.Lerp(startOpacity, 0f, elapsed / duration);
                overlay.style.opacity = opacity;
                yield return null;
            }

            overlay.style.opacity = 0f;
            overlay.style.display = DisplayStyle.None;
        }
    }
}

