using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// 画面トランジション機能を管理するクラス
    /// </summary>
    public class ScreenTransitionManager : MonoBehaviour
    {
        private Coroutine currentTransition;

        /// <summary>
        /// 画面トランジションを開始
        /// </summary>
        public void StartScreenTransition(VisualElement root, bool withScale = false)
        {
            // 既存のトランジションを停止
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }

            var content = root.Q<VisualElement>("Content");
            if (content == null) return;

            // 初期状態：UIコンテンツを非表示
            content.style.opacity = 0f;
            if (withScale)
            {
                content.style.scale = new Scale(new Vector2(0.8f, 0.8f));
            }
            else
            {
                content.style.scale = new Scale(new Vector2(1.0f, 1.0f));
            }

            // トランジション開始
            currentTransition = StartCoroutine(TransitionCoroutine(content, withScale));
        }

        /// <summary>
        /// トランジションコルーチン（1秒かけてフェードイン、オプションでスケール）
        /// </summary>
        private IEnumerator TransitionCoroutine(VisualElement element, bool withScale)
        {
            float duration = 1.0f;
            float elapsed = 0f;
            float startOpacity = 0f;
            float endOpacity = 1f;
            float startScale = withScale ? 0.8f : 1.0f;
            float endScale = 1.0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // イージング関数（ease-out）
                float easedT = 1f - Mathf.Pow(1f - t, 3f);

                // 透明度を補間
                float currentOpacity = Mathf.Lerp(startOpacity, endOpacity, easedT);
                element.style.opacity = currentOpacity;

                // スケールを補間（withScaleがtrueの場合のみ）
                if (withScale)
                {
                    float currentScale = Mathf.Lerp(startScale, endScale, easedT);
                    element.style.scale = new Scale(new Vector2(currentScale, currentScale));
                }

                yield return null;
            }

            // 最終状態を設定
            element.style.opacity = endOpacity;
            element.style.scale = new Scale(new Vector2(endScale, endScale));

            currentTransition = null;
        }

        /// <summary>
        /// トランジションを停止
        /// </summary>
        public void StopTransition()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }
        }
    }
}

