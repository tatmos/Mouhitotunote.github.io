using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// シェイクアニメーションを管理するクラス
    /// </summary>
    public class ShakeAnimationManager : MonoBehaviour
    {
        /// <summary>
        /// Labelを揺らすアニメーション
        /// </summary>
        public IEnumerator ShakeAnimation(Label label, float duration = 0.5f, float shakeIntensity = 10f)
        {
            if (label == null)
            {
                yield break;
            }

            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float offsetX = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                float offsetY = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                
                label.style.translate = new Translate(offsetX, offsetY, 0);
                
                yield return null;
            }
            
            // 元の位置に戻す
            label.style.translate = new Translate(0, 0, 0);
        }
    }
}

