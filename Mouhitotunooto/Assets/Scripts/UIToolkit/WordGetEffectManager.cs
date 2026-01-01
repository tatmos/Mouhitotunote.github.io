using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// ワードゲット演出を管理するクラス
    /// </summary>
    public class WordGetEffectManager : MonoBehaviour
    {
        private GameManager gameManager;
        private AudioManager audioManager;
        private Sprite sparkleIcon;

        public void Initialize(GameManager gameManager, AudioManager audioManager, Sprite sparkleIcon)
        {
            this.gameManager = gameManager;
            this.audioManager = audioManager;
            this.sparkleIcon = sparkleIcon;
        }

        /// <summary>
        /// ワードゲットラベルにスパークルアイコンを追加
        /// </summary>
        public void SetupWordGetLabelWithSparkle(VisualElement container, Label label, string text, System.Action onSparkleClick = null)
        {
            if (container == null || label == null) return;
            
            // コンテナをクリア
            container.Clear();
            
            // 水平レイアウトコンテナを作成
            var horizontalContainer = new VisualElement();
            horizontalContainer.style.flexDirection = FlexDirection.Row;
            horizontalContainer.style.alignItems = Align.Center;
            horizontalContainer.style.justifyContent = Justify.Center;
            horizontalContainer.style.width = Length.Percent(100);
            
            // 左側のスパークルアイコン
            if (sparkleIcon != null)
            {
                var leftSparkle = new Image();
                leftSparkle.sprite = sparkleIcon;
                leftSparkle.style.width = 24f;
                leftSparkle.style.height = 24f;
                leftSparkle.style.marginRight = 8f;
                leftSparkle.RegisterCallback<ClickEvent>(evt => {
                    onSparkleClick?.Invoke();
                    if (audioManager != null)
                    {
                        audioManager.PlaySparkleSound();
                    }
                });
                horizontalContainer.Add(leftSparkle);
            }
            
            // テキストラベル
            label.text = text;
            horizontalContainer.Add(label);
            
            // 右側のスパークルアイコン
            if (sparkleIcon != null)
            {
                var rightSparkle = new Image();
                rightSparkle.sprite = sparkleIcon;
                rightSparkle.style.width = 24f;
                rightSparkle.style.height = 24f;
                rightSparkle.style.marginLeft = 8f;
                rightSparkle.RegisterCallback<ClickEvent>(evt => {
                    onSparkleClick?.Invoke();
                    if (audioManager != null)
                    {
                        audioManager.PlaySparkleSound();
                    }
                });
                horizontalContainer.Add(rightSparkle);
            }
            
            container.Add(horizontalContainer);
        }

        /// <summary>
        /// ワードゲット時の綺麗な演出を表示
        /// </summary>
        public IEnumerator ShowWordGetWithEffect(VisualElement root, bool isDarkMode, Vector2 clickPosition = default, System.Action onComplete = null)
        {
            // 演出用のオーバーレイを作成（生成り系に変更）
            var effectOverlay = new VisualElement();
            effectOverlay.style.position = Position.Absolute;
            effectOverlay.style.left = 0;
            effectOverlay.style.top = 0;
            effectOverlay.style.right = 0;
            effectOverlay.style.bottom = 0;
            effectOverlay.style.backgroundColor = new Color(0.93f, 0.84f, 0.71f, 0f); // 生成り系
            
            // clickPositionが指定されていない場合のみ中央揃えにする
            if (clickPosition == default)
            {
                effectOverlay.style.justifyContent = Justify.Center;
                effectOverlay.style.alignItems = Align.Center;
            }
            
            root.Add(effectOverlay);
            
            // 光るエフェクト（円形のグラデーション風）
            var glowEffect = new VisualElement();
            glowEffect.style.width = 200f;
            glowEffect.style.height = 200f;
            // 円形にするため、すべての角に同じ値を設定
            float borderRadius = 100f;
            glowEffect.style.borderTopLeftRadius = borderRadius;
            glowEffect.style.borderTopRightRadius = borderRadius;
            glowEffect.style.borderBottomLeftRadius = borderRadius;
            glowEffect.style.borderBottomRightRadius = borderRadius;
            glowEffect.style.backgroundColor = new Color(1f, 0.84f, 0f, 0f); // 黄色
            glowEffect.style.position = Position.Absolute;

            // クリック位置が指定されている場合は、その位置にエフェクトを表示
            if (clickPosition != default)
            {
                // UI Toolkitの座標系にあわせる
                glowEffect.style.left = clickPosition.x - 100f;
                glowEffect.style.top = clickPosition.y - 100f;
            }

            effectOverlay.Add(glowEffect);
            
            // エフェクトアニメーション（拡大してフェードアウト）
            float effectDuration = 1.0f;
            float elapsed = 0f;
            float startScale = 0.5f;
            float endScale = 2.0f;
            
            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / effectDuration;
                
                // スケールアニメーション
                float currentScale = Mathf.Lerp(startScale, endScale, t);
                float currentWidth = 200f * currentScale;
                float currentHeight = 200f * currentScale;
                glowEffect.style.width = currentWidth;
                glowEffect.style.height = currentHeight;

                // 位置の調整（中心を維持するため）
                if (clickPosition != default)
                {
                    glowEffect.style.left = clickPosition.x - (currentWidth / 2f);
                    glowEffect.style.top = clickPosition.y - (currentHeight / 2f);
                }

                // 円形を維持するため、すべての角に同じ値を設定
                float currentBorderRadius = (currentWidth / 2f);
                glowEffect.style.borderTopLeftRadius = currentBorderRadius;
                glowEffect.style.borderTopRightRadius = currentBorderRadius;
                glowEffect.style.borderBottomLeftRadius = currentBorderRadius;
                glowEffect.style.borderBottomRightRadius = currentBorderRadius;
                
                // フェードアウト
                float alpha = Mathf.Lerp(0.8f, 0f, t);
                glowEffect.style.backgroundColor = new Color(1f, 0.84f, 0f, alpha);
                
                // 背景も少し明るく（生成り系に変更）
                float bgAlpha = Mathf.Lerp(0f, 0.3f, Mathf.Sin(t * Mathf.PI));
                effectOverlay.style.backgroundColor = new Color(0.93f, 0.84f, 0.71f, bgAlpha); // 生成り系
                
                yield return null;
            }
            
            // エフェクトを削除
            root.Remove(effectOverlay);
            
            // 一呼吸（0.5秒待つ）
            yield return new WaitForSeconds(0.5f);
            
            // ワードゲット表示を表示
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            if (wordGetContainer != null)
            {
                wordGetContainer.style.display = DisplayStyle.Flex;
            }
            
            // コールバックを呼び出して後続処理を実行
            onComplete?.Invoke();
        }

        /// <summary>
        /// ワード取得時に、結果画面からスコア表示へ光が飛んでいく演出
        /// </summary>
        public IEnumerator ShowLetterGetAnimation(Vector2 startPos, VisualElement root)
        {
            if (root == null) yield break;

            // スコア表示（ScoreText）を探す
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel == null) yield break;

            // 座標確定待ち
            yield return null;
            Vector2 endPos = scoreLabel.worldBound.center;

            // 演出用コンテナ
            var effectContainer = new VisualElement();
            effectContainer.style.position = Position.Absolute;
            effectContainer.style.left = 0;
            effectContainer.style.top = 0;
            effectContainer.style.right = 0;
            effectContainer.style.bottom = 0;
            effectContainer.pickingMode = PickingMode.Ignore;
            root.Add(effectContainer);

            // 光の粒子演出
            int particleCount = 10;
            System.Collections.Generic.List<VisualElement> particles = new System.Collections.Generic.List<VisualElement>();
            for (int i = 0; i < particleCount; i++)
            {
                var p = new VisualElement();
                p.style.position = Position.Absolute;
                p.style.width = 8;
                p.style.height = 8;
                p.style.backgroundColor = Color.white;
                p.style.borderTopLeftRadius = 4;
                p.style.borderTopRightRadius = 4;
                p.style.borderBottomLeftRadius = 4;
                p.style.borderBottomRightRadius = 4;
                p.style.left = startPos.x;
                p.style.top = startPos.y;
                effectContainer.Add(p);
                particles.Add(p);
            }

            if (audioManager != null)
            {
                audioManager.PlaySparkleSound();
            }

            float duration = 0.8f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                for (int i = 0; i < particles.Count; i++)
                {
                    var p = particles[i];
                    float offset = (float)i / particles.Count * 0.3f;
                    float currentT = Mathf.Clamp01(t + offset);
                    
                    // ベジェ曲線的な動き（開始時は遅く、終了時に加速）
                    float easedT = currentT * currentT;
                    
                    Vector2 currentPos = Vector2.Lerp(
                        new Vector2(startPos.x, startPos.y),
                        new Vector2(endPos.x, endPos.y),
                        easedT
                    );
                    
                    p.style.left = currentPos.x - 4;
                    p.style.top = currentPos.y - 4;
                    
                    // フェードアウト
                    float opacity = 1f - currentT;
                    p.style.opacity = opacity;
                }
                
                yield return null;
            }
            
            // パーティクルを削除
            effectContainer.RemoveFromHierarchy();
        }

        /// <summary>
        /// ワードが失われた時のアニメーション
        /// </summary>
        public IEnumerator PlayWordLostAnimation(int newScore, int oldScore, VisualElement root, Label scoreLabel)
        {
            if (root == null || scoreLabel == null) yield break;
            if (gameManager == null) yield break;
            
            // ワードゲット数が減る時の効果音を再生
            if (audioManager != null)
            {
                audioManager.PlayWordGetDecreaseSound();
            }
            
            // 文字が奪われていく演出
            // 「もうひとつ」の各文字を順番に消していく
            char[] allLetters = MouhitotsuWordManager.GetAllLetters();
            string[] characters = new string[allLetters.Length];
            for (int i = 0; i < allLetters.Length; i++)
            {
                characters[i] = allLetters[i].ToString();
            }
            string baseText = $"{MouhitotsuWordManager.GetFormattedWord()}ワードゲット数";
            
            // 失われた文字を取得
            var lostLetters = gameManager.GetLostLetters();
            
            // 各文字を順番に消していく（0.15秒間隔）
            for (int i = 0; i < characters.Length; i++)
            {
                // 既に失われている文字はスキップ
                if (lostLetters.Contains(characters[i][0]))
                {
                    continue;
                }
                
                // 文字を「※」に置き換え
                baseText = baseText.Replace(characters[i], "※");
                
                // スコア表示を更新（徐々に減らしていく）
                int totalScenarios = gameManager.GetScenarios().Count;
                int displayScore = oldScore - (i + 1);
                scoreLabel.text = $"{baseText}: {displayScore} / {totalScenarios}";
                
                // 文字が消えるアニメーション（スケールダウン + 揺れ）
                float shakeDuration = 0.15f;
                float shakeAmount = 3f;
                float originalScale = 1.0f;
                float elapsed = 0f;
                
                while (elapsed < shakeDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / shakeDuration;
                    
                    // 揺れ効果
                    float offsetX = Mathf.Sin(progress * Mathf.PI * 4) * shakeAmount * (1f - progress);
                    float offsetY = Mathf.Cos(progress * Mathf.PI * 4) * shakeAmount * (1f - progress);
                    scoreLabel.style.translate = new StyleTranslate(new Translate(offsetX, offsetY));
                    
                    // スケールダウン効果
                    float scale = Mathf.Lerp(originalScale, 0.95f, progress);
                    scoreLabel.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
                    
                    yield return null;
                }
                
                // 元の位置とスケールに戻す
                scoreLabel.style.translate = new StyleTranslate(new Translate(0, 0));
                scoreLabel.style.scale = new StyleScale(new Scale(Vector3.one));
                
                // 次の文字まで待機
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}

