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
        // UIManagerUIToolkitから共有されるキャッシュへの参照
        private Dictionary<VisualElement, Texture2D> backgroundTextureCache = null;
        
        // スコアベースの歪み制御
        private bool isScoreBasedDistortionActive = false;
        private int currentScore = 6; // 正常なスコア
        private int normalScore = 6; // 正常なスコア値
        private float baseDistortionStrength = 0f; // スコアに基づく基本歪み強度
        private string distortionPropertyName = null; // 歪みシェーダーのプロパティ名
        private float distortionStartTime = 0f; // 歪み開始時刻

        /// <summary>
        /// 背景テクスチャキャッシュを設定（UIManagerUIToolkitから呼び出される）
        /// </summary>
        public void SetBackgroundTextureCache(Dictionary<VisualElement, Texture2D> cache)
        {
            backgroundTextureCache = cache;
        }

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

        /// <summary>
        /// 背景に段階的に歪み効果を適用（シナリオ6でスコア7になった時など）
        /// </summary>
        public void ApplyGradualBackgroundDistortion(VisualElement backgroundImage, int score = 7, int normalScore = 6)
        {
            if (backgroundImage == null)
            {
                Debug.LogWarning("[DistortionEffectManager] backgroundImage is null");
                return;
            }

            if (distortionMaterial == null)
            {
                Debug.LogWarning("[DistortionEffectManager] distortionMaterial is null");
                return;
            }

            // 歪みシェーダーのプロパティ名を検出（初回のみ）
            if (distortionPropertyName == null)
            {
                distortionPropertyName = "_DistortionStrength";
                if (!distortionMaterial.HasProperty(distortionPropertyName))
                {
                    if (distortionMaterial.HasProperty("_Amount"))
                    {
                        distortionPropertyName = "_Amount";
                    }
                    else if (distortionMaterial.HasProperty("_Intensity"))
                    {
                        distortionPropertyName = "_Intensity";
                    }
                    else
                    {
                        distortionPropertyName = null;
                    }
                }
            }

            // スコアベースの歪み制御を開始
            this.currentScore = score;
            this.normalScore = normalScore;
            this.isScoreBasedDistortionActive = true;
            this.distortionStartTime = Time.time;

            // 既存のコルーチンを停止
            if (distortionUpdateCoroutine != null)
            {
                StopCoroutine(distortionUpdateCoroutine);
                distortionUpdateCoroutine = null;
            }

            // 段階的に歪み効果を開始
            StartCoroutine(GradualDistortionEffect(backgroundImage, score, normalScore));
        }

        /// <summary>
        /// スコアに応じて歪みの強度を更新
        /// </summary>
        public void UpdateDistortionByScore(int score, int normalScore = 6)
        {
            if (!isScoreBasedDistortionActive || distortionMaterial == null)
            {
                return;
            }

            this.currentScore = score;
            this.normalScore = normalScore;

            // スコアが正常値に戻ったら歪みを停止
            if (score == normalScore)
            {
                StopScoreBasedDistortion();
                return;
            }

            // スコアに基づく基本歪み強度を計算
            // スコアが正常値から離れているほど強い
            int scoreDifference = Mathf.Abs(score - normalScore);
            baseDistortionStrength = Mathf.Clamp01(scoreDifference * 0.5f); // 1離れるごとに0.5強度
        }

        /// <summary>
        /// スコアベースの歪みを停止
        /// </summary>
        public void StopScoreBasedDistortion(VisualElement backgroundImage = null)
        {
            isScoreBasedDistortionActive = false;
            baseDistortionStrength = 0f;

            if (distortionPropertyName != null && distortionMaterial != null)
            {
                distortionMaterial.SetFloat(distortionPropertyName, 0f);
            }

            // コルーチンを停止して、元のテクスチャに戻す
            if (distortionUpdateCoroutine != null)
            {
                StopCoroutine(distortionUpdateCoroutine);
                distortionUpdateCoroutine = null;
            }

            CleanupDistortionEffect(backgroundImage);
        }

        // エンドクレジット用の不定期な歪み効果
        private bool isIntermittentDistortionActive = false;
        private Coroutine intermittentDistortionCoroutine;

        /// <summary>
        /// エンドクレジット画面で、スコアに応じた不定期な背景歪み効果を開始
        /// </summary>
        public void StartIntermittentDistortionForCredits(VisualElement backgroundImage, int score, int normalScore = 6)
        {
            if (backgroundImage == null)
            {
                Debug.LogWarning("[DistortionEffectManager] backgroundImage is null");
                return;
            }

            if (distortionMaterial == null)
            {
                Debug.LogWarning("[DistortionEffectManager] distortionMaterial is null");
                return;
            }

            // スコアが正常値の場合は歪み効果を開始しない
            if (score == normalScore)
            {
                StopIntermittentDistortionForCredits(backgroundImage);
                return;
            }

            // 既存のコルーチンを停止
            if (intermittentDistortionCoroutine != null)
            {
                StopCoroutine(intermittentDistortionCoroutine);
                intermittentDistortionCoroutine = null;
            }

            // 歪みシェーダーのプロパティ名を検出（初回のみ）
            if (distortionPropertyName == null)
            {
                distortionPropertyName = "_DistortionStrength";
                if (!distortionMaterial.HasProperty(distortionPropertyName))
                {
                    if (distortionMaterial.HasProperty("_Amount"))
                    {
                        distortionPropertyName = "_Amount";
                    }
                    else if (distortionMaterial.HasProperty("_Intensity"))
                    {
                        distortionPropertyName = "_Intensity";
                    }
                    else
                    {
                        distortionPropertyName = null;
                    }
                }
            }

            this.currentScore = score;
            this.normalScore = normalScore;
            this.isIntermittentDistortionActive = true;

            // 不定期な歪み効果を開始
            intermittentDistortionCoroutine = StartCoroutine(IntermittentDistortionForCredits(backgroundImage, score, normalScore));
        }

        /// <summary>
        /// エンドクレジット用の不定期な歪み効果を停止
        /// </summary>
        public void StopIntermittentDistortionForCredits(VisualElement backgroundImage = null)
        {
            isIntermittentDistortionActive = false;

            if (distortionPropertyName != null && distortionMaterial != null)
            {
                distortionMaterial.SetFloat(distortionPropertyName, 0f);
            }

            // コルーチンを停止
            if (intermittentDistortionCoroutine != null)
            {
                StopCoroutine(intermittentDistortionCoroutine);
                intermittentDistortionCoroutine = null;
            }

            // 元のテクスチャに戻す
            CleanupDistortionEffect(backgroundImage);
        }

        /// <summary>
        /// エンドクレジット用の不定期な歪み効果のコルーチン
        /// </summary>
        private IEnumerator IntermittentDistortionForCredits(VisualElement backgroundImage, int score, int normalScore)
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
                if (backgroundTextureCache != null && backgroundTextureCache.TryGetValue(backgroundImage, out sourceTexture) && sourceTexture != null)
                {
                    // キャッシュから取得成功
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                // キャッシュに保存（キャッシュが設定されている場合のみ）
                if (backgroundTextureCache != null)
                {
                    backgroundTextureCache[backgroundImage] = sourceTexture;
                }
            }

            currentDistortionSourceTexture = sourceTexture;
            currentDistortionElement = backgroundImage;

            // RenderTextureをセットアップ
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

            // Texture2Dを再利用
            if (distortionTexture2D == null || distortionTexture2D.width != renderWidth || distortionTexture2D.height != renderHeight)
            {
                if (distortionTexture2D != null)
                {
                    Destroy(distortionTexture2D);
                }
                distortionTexture2D = new Texture2D(renderWidth, renderHeight, TextureFormat.RGBA32, false);
            }

            // スコアに基づく基本歪み強度を計算
            int scoreDifference = Mathf.Abs(score - normalScore);
            float maxDistortionStrength = Mathf.Clamp01(scoreDifference * 0.4f); // スコアが離れているほど強い

            float lastDistortionTime = Time.time;
            float nextDistortionInterval = Random.Range(3f, 8f); // 3-8秒ごとに不定期に歪む
            bool isCurrentlyDistorting = false;
            float distortionDuration = 0f;
            float distortionElapsed = 0f;

            // 不定期に歪み効果を適用
            while (isIntermittentDistortionActive && currentDistortionElement != null)
            {
                float currentTime = Time.time;
                float timeSinceLastDistortion = currentTime - lastDistortionTime;

                // 歪みが発生するタイミングをチェック
                if (!isCurrentlyDistorting && timeSinceLastDistortion >= nextDistortionInterval)
                {
                    // 歪みを開始
                    isCurrentlyDistorting = true;
                    distortionDuration = Random.Range(0.5f, 2.0f); // 0.5-2秒間歪む
                    distortionElapsed = 0f;
                    lastDistortionTime = currentTime;
                    nextDistortionInterval = Random.Range(3f, 8f); // 次の歪みまでの間隔を設定
                }

                // 歪みが発生中の場合
                if (isCurrentlyDistorting)
                {
                    distortionElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(distortionElapsed / distortionDuration);

                    // 歪みの強度をフェードイン・フェードアウト（0から最大値へ、そして0へ）
                    float distortionStrength;
                    if (t < 0.3f)
                    {
                        // フェードイン（0.3秒）
                        distortionStrength = Mathf.Lerp(0f, maxDistortionStrength, t / 0.3f);
                    }
                    else if (t < 0.7f)
                    {
                        // 最大強度を維持（0.4秒）
                        distortionStrength = maxDistortionStrength;
                    }
                    else
                    {
                        // フェードアウト（0.3秒）
                        distortionStrength = Mathf.Lerp(maxDistortionStrength, 0f, (t - 0.7f) / 0.3f);
                    }

                    // 時間経過による揺り戻しを追加（スコアが離れているほど大きい）
                    float wobble = CalculateWobble(score, normalScore) * 0.5f; // 揺れを少し小さめに
                    float finalDistortionStrength = Mathf.Clamp01(distortionStrength + wobble);

                    if (distortionPropertyName != null)
                    {
                        distortionMaterial.SetFloat(distortionPropertyName, finalDistortionStrength);
                    }
                }
                else
                {
                    // 歪みが発生していない場合は歪み強度を0に
                    if (distortionPropertyName != null)
                    {
                        distortionMaterial.SetFloat(distortionPropertyName, 0f);
                    }
                }

                // 歪み時間が終了したら、歪み状態をリセット
                if (isCurrentlyDistorting && distortionElapsed >= distortionDuration)
                {
                    isCurrentlyDistorting = false;
                }

                // 時間ベースの歪みも適用（シェーダーが時間パラメータを使用する場合）
                if (distortionMaterial.HasProperty("_Time"))
                {
                    distortionMaterial.SetFloat("_Time", Time.time);
                }

                // Graphics.Blitを使用して歪みシェーダーを適用
                Graphics.Blit(sourceTexture, distortionRenderTexture, distortionMaterial);

                // RenderTextureの内容をTexture2Dにコピー
                UpdateDistortionTexture();

                // 更新頻度を下げてパフォーマンスを向上（5FPS = 0.2秒間隔）
                yield return new WaitForSeconds(DistortionUpdateInterval);
            }

            // 歪み効果を無効化
            if (distortionPropertyName != null && distortionMaterial != null)
            {
                distortionMaterial.SetFloat(distortionPropertyName, 0f);
            }

            CleanupDistortionEffect(backgroundImage);
        }

        /// <summary>
        /// 段階的に歪み効果を適用するコルーチン
        /// </summary>
        private IEnumerator GradualDistortionEffect(VisualElement backgroundImage, int score, int normalScore)
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
                if (backgroundTextureCache != null && backgroundTextureCache.TryGetValue(backgroundImage, out sourceTexture) && sourceTexture != null)
                {
                    // キャッシュから取得成功
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                // キャッシュに保存（キャッシュが設定されている場合のみ）
                if (backgroundTextureCache != null)
                {
                    backgroundTextureCache[backgroundImage] = sourceTexture;
                }
            }

            currentDistortionSourceTexture = sourceTexture;
            currentDistortionElement = backgroundImage;

            // RenderTextureをセットアップ
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

            // Texture2Dを再利用
            if (distortionTexture2D == null || distortionTexture2D.width != renderWidth || distortionTexture2D.height != renderHeight)
            {
                if (distortionTexture2D != null)
                {
                    Destroy(distortionTexture2D);
                }
                distortionTexture2D = new Texture2D(renderWidth, renderHeight, TextureFormat.RGBA32, false);
            }

            // スコアに基づく基本歪み強度を計算
            int scoreDifference = Mathf.Abs(score - normalScore);
            float targetDistortionStrength = Mathf.Clamp01(scoreDifference * 0.5f); // 1離れるごとに0.5強度

            // 段階的に歪みを強くしていく
            float duration = 3.0f; // 3秒かけて歪みを強くする
            float elapsed = 0f;

            // 歪み効果を段階的に適用
            while (elapsed < duration && currentDistortionElement != null && isScoreBasedDistortionActive)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 歪みの強度を0から目標値まで段階的に増やす
                float currentDistortionStrength = Mathf.Lerp(0f, targetDistortionStrength, t);
                baseDistortionStrength = currentDistortionStrength;
                
                // 時間経過による揺り戻しを追加
                float wobble = CalculateWobble(score, normalScore);
                float finalDistortionStrength = currentDistortionStrength + wobble;
                
                if (distortionPropertyName != null)
                {
                    distortionMaterial.SetFloat(distortionPropertyName, finalDistortionStrength);
                }
                
                // 時間ベースの歪みも適用（シェーダーが時間パラメータを使用する場合）
                if (distortionMaterial.HasProperty("_Time"))
                {
                    distortionMaterial.SetFloat("_Time", Time.time);
                }

                // Graphics.Blitを使用して歪みシェーダーを適用
                Graphics.Blit(sourceTexture, distortionRenderTexture, distortionMaterial);
                
                // RenderTextureの内容をTexture2Dにコピー
                UpdateDistortionTexture();

                yield return null;
            }

            // 定期的にRenderTextureを更新するコルーチンを開始（スコアベースの歪み継続）
            distortionUpdateCoroutine = StartCoroutine(UpdateDistortionEffectWithScore(sourceTexture, backgroundImage));
        }

        /// <summary>
        /// 時間経過による揺り戻しを計算（スコアが正常に近いほど安定）
        /// </summary>
        private float CalculateWobble(int score, int normalScore)
        {
            // スコアが正常値から離れているほど揺れが大きい
            int scoreDifference = Mathf.Abs(score - normalScore);
            float wobbleAmount = scoreDifference * 0.15f; // 離れているほど揺れが大きい
            
            // スコアが正常に近いほど揺れが小さい（安定）
            float stability = Mathf.Clamp01(1f - (scoreDifference * 0.2f)); // 正常に近いほど1に近づく
            wobbleAmount *= (1f - stability); // 安定性が高いほど揺れが小さい
            
            // 時間ベースの揺らぎ（サイン波とコサイン波の組み合わせ）
            float time = Time.time - distortionStartTime;
            float wobble = Mathf.Sin(time * 1.5f) * 0.5f + Mathf.Cos(time * 2.3f) * 0.3f;
            wobble *= wobbleAmount;
            
            return wobble;
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
                if (backgroundTextureCache != null && backgroundTextureCache.TryGetValue(backgroundImage, out sourceTexture) && sourceTexture != null)
                {
                    // キャッシュから取得成功
                }
                else
                {
                    // テクスチャが見つからない場合は、歪み効果を適用せずに終了
                    // 警告は出さない（背景画像が設定されていない場合など、正常なケースもあるため）
                    return;
                }
            }
            else
            {
                // キャッシュに保存（キャッシュが設定されている場合のみ）
                if (backgroundTextureCache != null)
                {
                    backgroundTextureCache[backgroundImage] = sourceTexture;
                }
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

        /// <summary>
        /// スコアベースの歪み効果を更新するコルーチン（時間経過で揺り戻しあり）
        /// </summary>
        private IEnumerator UpdateDistortionEffectWithScore(Texture2D sourceTexture, VisualElement backgroundImage)
        {
            while (currentDistortionElement != null && isScoreBasedDistortionActive)
            {
                // スコアが正常値に戻ったら停止
                if (currentScore == normalScore)
                {
                    StopScoreBasedDistortion(backgroundImage);
                    yield break;
                }

                // スコアに基づく基本歪み強度を計算
                int scoreDifference = Mathf.Abs(currentScore - normalScore);
                baseDistortionStrength = Mathf.Clamp01(scoreDifference * 0.5f);

                // 時間経過による揺り戻しを追加
                float wobble = CalculateWobble(currentScore, normalScore);
                float finalDistortionStrength = baseDistortionStrength + wobble;
                finalDistortionStrength = Mathf.Clamp01(finalDistortionStrength);

                if (distortionPropertyName != null && distortionMaterial != null)
                {
                    distortionMaterial.SetFloat(distortionPropertyName, finalDistortionStrength);
                }

                // 時間ベースの歪みも適用
                if (distortionMaterial != null && distortionMaterial.HasProperty("_Time"))
                {
                    distortionMaterial.SetFloat("_Time", Time.time);
                }

                // Graphics.Blitを使用して歪みシェーダーを適用
                if (sourceTexture != null && distortionRenderTexture != null && distortionMaterial != null)
                {
                    Graphics.Blit(sourceTexture, distortionRenderTexture, distortionMaterial);
                }

                // RenderTextureの内容をTexture2Dにコピー
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
            if (backgroundImage != null && backgroundTextureCache != null && backgroundTextureCache.TryGetValue(backgroundImage, out Texture2D originalTexture))
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

