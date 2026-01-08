using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// シナリオ解放演出を管理するクラス
    /// </summary>
    public class ScenarioUnlockEffectManager : MonoBehaviour
    {
        private GameManager gameManager;
        private AudioManager audioManager;
        private Sprite scenarioButtonNormalImage;
        private System.Action<int> onScenarioSelected;
        private System.Action onHoverSound;

        public void Initialize(GameManager gameManager, AudioManager audioManager, Sprite scenarioButtonNormalImage, System.Action<int> onScenarioSelected, System.Action onHoverSound)
        {
            this.gameManager = gameManager;
            this.audioManager = audioManager;
            this.scenarioButtonNormalImage = scenarioButtonNormalImage;
            this.onScenarioSelected = onScenarioSelected;
            this.onHoverSound = onHoverSound;
        }

        /// <summary>
        /// シナリオ6解放演出を表示（α加算合成風の美しいエフェクト）
        /// </summary>
        public IEnumerator ShowScenario6UnlockAnimation(VisualElement root, System.Func<string, System.Collections.Generic.HashSet<char>, System.Collections.Generic.HashSet<char>, string> formatText = null)
        {
            // 少し待つ
            yield return new WaitForSeconds(0.5f);

            // 「もうひとつ」ワードゲット数（ScoreText）を探す
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel == null) yield break;

            // 座標を取得（レイアウト確定を待つ）
            yield return null; 
            
            // 演出用のコンテナ（最前面）
            var effectContainer = new VisualElement();
            effectContainer.style.position = Position.Absolute;
            effectContainer.style.left = 0;
            effectContainer.style.top = 0;
            effectContainer.style.right = 0;
            effectContainer.style.bottom = 0;
            effectContainer.pickingMode = PickingMode.Ignore;
            root.Add(effectContainer);

            // 「もうひとつ」ワードゲット数の位置から光が出るように変更
            Vector2 startPos = scoreLabel.worldBound.center;
            
            // シナリオ6ボタンの位置を特定するために、一旦ボタンを作成して非表示で追加する
            var buttonContainer = root.Q<VisualElement>("ScenarioButtonContainer");
            if (buttonContainer == null) yield break;

            // シナリオ6ボタン（演出用）
            var scenario6 = gameManager.GetScenarios().Find(s => s.id == 6);
            if (scenario6 == null) yield break;

            // 本来のボタン生成ロジックを流用したいが、アニメーションのために個別に制御
            Button targetButton = new Button();
            targetButton.AddToClassList("scenario-button");
            targetButton.AddToClassList("scenario-button-normal");
            targetButton.style.opacity = 0;
            
            // ボタンの内容を構造化（CreateScenarioButtonsと同じスタイル）
            var buttonContent = new VisualElement();
            buttonContent.style.flexDirection = FlexDirection.Column;
            buttonContent.style.alignItems = Align.FlexStart;
            buttonContent.style.width = Length.Percent(100);
            
            // 失われた文字を置換
            var lostLetters = gameManager.GetLostLetters();
            string scenarioTitleText = scenario6.title;
            string scenarioDescriptionText = scenario6.setup;
            
            // テキストフォーマット
            if (formatText != null)
            {
                var collectedLetters = gameManager.GetCollectedLetters();
                scenarioTitleText = formatText(scenarioTitleText, collectedLetters, lostLetters);
                scenarioDescriptionText = formatText(scenarioDescriptionText, collectedLetters, lostLetters);
            }
            
            // 文字色の定義
            Color normalTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
            
            var titleLabel = new Label(scenarioTitleText);
            titleLabel.style.fontSize = 20;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.whiteSpace = WhiteSpace.Normal;
            titleLabel.style.marginBottom = 5;
            titleLabel.style.color = normalTextColor;
            buttonContent.Add(titleLabel);
            
            // シナリオの説明を追加（2行まで、文字あふれ防止）
            var descriptionLabel = new Label(scenarioDescriptionText);
            descriptionLabel.style.fontSize = 14;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionLabel.style.opacity = 0.9f;
            descriptionLabel.style.maxHeight = 40; // 2行分の高さに制限
            descriptionLabel.style.overflow = Overflow.Hidden;
            descriptionLabel.style.color = normalTextColor;
            buttonContent.Add(descriptionLabel);
            
            targetButton.Add(buttonContent);
            
            // クリア前の画像を設定（9-slice対応）
            if (scenarioButtonNormalImage != null && scenarioButtonNormalImage.texture != null)
            {
                targetButton.style.backgroundImage = new StyleBackground(scenarioButtonNormalImage.texture);
                targetButton.style.backgroundColor = Color.clear; // 背景色をクリア
            }
            
            int scenarioId = scenario6.id;
            targetButton.clicked += () => onScenarioSelected?.Invoke(scenarioId);
            targetButton.RegisterCallback<PointerEnterEvent>(evt => onHoverSound?.Invoke());
            
            // コンテナの最後に追加
            buttonContainer.Add(targetButton);
            
            // レイアウト確定を待つ
            yield return null;
            Vector2 endPos = targetButton.worldBound.center;
            
            // 座標をローカル座標に変換
            Vector2 localStartPos = startPos - root.worldBound.position;
            Vector2 localEndPos = endPos - root.worldBound.position;

            // α加算合成風の背景オーバーレイ（全体を明るく）
            var glowOverlay = new VisualElement();
            glowOverlay.style.position = Position.Absolute;
            glowOverlay.style.left = 0;
            glowOverlay.style.top = 0;
            glowOverlay.style.right = 0;
            glowOverlay.style.bottom = 0;
            glowOverlay.style.backgroundColor = new Color(1f, 0.95f, 0.8f, 0f); // 温かみのある光
            glowOverlay.pickingMode = PickingMode.Ignore;
            effectContainer.Add(glowOverlay);

            // 光の粒子演出（α加算合成風：明るく、多層に）
            int particleCount = 50; // パーティクル数を増やす
            List<VisualElement> particles = new List<VisualElement>();
            List<Vector2> particleVelocities = new List<Vector2>();
            List<float> particleSizes = new List<float>();
            
            for (int i = 0; i < particleCount; i++)
            {
                var p = new VisualElement();
                p.style.position = Position.Absolute;
                
                // サイズをランダムに（より多様に）
                float size = Random.Range(8f, 20f);
                p.style.width = size;
                p.style.height = size;
                particleSizes.Add(size);
                
                // α加算合成風の色（明るく、透明度を高めに）
                float hue = Random.Range(0.08f, 0.18f); // 金色～黄色系
                float saturation = Random.Range(0.7f, 1.0f);
                float brightness = Random.Range(0.9f, 1.2f); // 1.0を超えて明るく（加算合成風）
                Color particleColor = Color.HSVToRGB(hue, saturation, Mathf.Clamp01(brightness));
                particleColor.a = Random.Range(0.6f, 1.0f); // 透明度を高めに
                p.style.backgroundColor = particleColor;
                
                // 円形
                float radius = size / 2f;
                p.style.borderTopLeftRadius = radius;
                p.style.borderTopRightRadius = radius;
                p.style.borderBottomLeftRadius = radius;
                p.style.borderBottomRightRadius = radius;
                
                // 初期位置（開始位置から少し広がる）
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float initialSpread = Random.Range(0f, 30f);
                Vector2 initialPos = localStartPos + new Vector2(
                    Mathf.Cos(angle) * initialSpread,
                    Mathf.Sin(angle) * initialSpread
                );
                p.style.left = initialPos.x - size / 2f;
                p.style.top = initialPos.y - size / 2f;
                
                // 速度（終点に向かう方向に、少しランダム性を持たせる）
                Vector2 direction = (localEndPos - localStartPos).normalized;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                float spread = Random.Range(-0.4f, 0.4f);
                Vector2 velocity = direction * Random.Range(300f, 600f) + perpendicular * spread * 150f;
                particleVelocities.Add(velocity);
                
                effectContainer.Add(p);
                particles.Add(p);
            }

            // 星のパーティクル音を再生
            if (audioManager != null)
            {
                audioManager.PlaySparkleSound();
            }

            // アニメーション：集まってから飛んでいく（α加算合成風）
            float duration = 1.2f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easeT = Mathf.SmoothStep(0, 1, t);

                // 背景オーバーレイの明るさ（α加算合成風）
                float overlayAlpha = Mathf.Lerp(0f, 0.3f, Mathf.Sin(t * Mathf.PI));
                glowOverlay.style.backgroundColor = new Color(1f, 0.95f, 0.8f, overlayAlpha);

                for (int i = 0; i < particleCount; i++)
                {
                    var p = particles[i];
                    Vector2 velocity = particleVelocities[i];
                    
                    // 物理的な動き（減速しながら）
                    float speedMultiplier = 1f - (t * t * 0.5f); // 緩やかに減速
                    Vector2 currentPos = localStartPos + velocity * elapsed * speedMultiplier;
                    
                    p.style.left = currentPos.x - particleSizes[i] / 2f;
                    p.style.top = currentPos.y - particleSizes[i] / 2f;
                    
                    // α加算合成風：透明度を高めに、明るく
                    float opacity = Mathf.Lerp(1f, 0.3f, t);
                    float glowIntensity = Mathf.Lerp(1.2f, 0.8f, t); // 加算合成風に明るく
                    Color currentColor = p.style.backgroundColor.value;
                    currentColor.a = opacity;
                    // 明るさを上げる（加算合成風）
                    float currentBrightness = Mathf.Clamp01(currentColor.r * glowIntensity);
                    currentColor = new Color(currentBrightness, currentBrightness * 0.95f, currentBrightness * 0.8f, opacity);
                    p.style.backgroundColor = currentColor;
                    
                    // スケール（集まるときに少し大きくなる）
                    float scale = Mathf.Lerp(1.0f, 1.3f, 1f - t);
                    p.style.scale = new Scale(new Vector2(scale, scale));
                }
                yield return null;
            }

            // グローエフェクト（複数層でα加算合成風）
            List<VisualElement> glowLayers = new List<VisualElement>();
            for (int layer = 0; layer < 3; layer++)
            {
                var glow = new VisualElement();
                glow.style.position = Position.Absolute;
                float baseSize = 80f + layer * 40f;
                glow.style.width = baseSize;
                glow.style.height = baseSize;
                float radius = baseSize / 2f;
                glow.style.borderTopLeftRadius = radius;
                glow.style.borderTopRightRadius = radius;
                glow.style.borderBottomLeftRadius = radius;
                glow.style.borderBottomRightRadius = radius;
                
                // 各層で色と透明度を変える（α加算合成風）
                float layerAlpha = 0.4f - layer * 0.1f;
                Color glowColor = new Color(1f, 0.9f, 0.6f, layerAlpha);
                glow.style.backgroundColor = glowColor;
                
                glow.style.left = localEndPos.x - baseSize / 2f;
                glow.style.top = localEndPos.y - baseSize / 2f;
                glow.style.opacity = 0f;
                effectContainer.Add(glow);
                glowLayers.Add(glow);
            }
            
            // 真実の扉出現音を再生
            if (audioManager != null)
            {
                audioManager.PlayTruthDoorUnlockSound();
            }

            // グローエフェクトのアニメーション
            float flashDuration = 0.8f;
            elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                
                // 各グローレイヤーをアニメーション
                for (int layer = 0; layer < glowLayers.Count; layer++)
                {
                    var glow = glowLayers[layer];
                    float layerT = Mathf.Clamp01(t - layer * 0.1f); // レイヤーごとに少しずつ遅延
                    
                    // フェードインしてからフェードアウト
                    float opacity = 0f;
                    if (layerT < 0.3f)
                    {
                        opacity = Mathf.Lerp(0f, 1f, layerT / 0.3f);
                    }
                    else
                    {
                        opacity = Mathf.Lerp(1f, 0f, (layerT - 0.3f) / 0.7f);
                    }
                    glow.style.opacity = opacity;
                    
                    // 拡大
                    float scale = 1f + layerT * 2f;
                    float baseSize = 80f + layer * 40f;
                    glow.style.width = baseSize * scale;
                    glow.style.height = baseSize * scale;
                    float radius = (baseSize * scale) / 2f;
                    glow.style.borderTopLeftRadius = radius;
                    glow.style.borderTopRightRadius = radius;
                    glow.style.borderBottomLeftRadius = radius;
                    glow.style.borderBottomRightRadius = radius;
                    glow.style.left = localEndPos.x - (baseSize * scale) / 2f;
                    glow.style.top = localEndPos.y - (baseSize * scale) / 2f;
                    
                    // α加算合成風：明るさを上げる
                    Color glowColor = glow.style.backgroundColor.value;
                    float brightness = 1f + layerT * 0.5f; // 加算合成風に明るく
                    glowColor.r = Mathf.Clamp01(glowColor.r * brightness);
                    glowColor.g = Mathf.Clamp01(glowColor.g * brightness);
                    glowColor.b = Mathf.Clamp01(glowColor.b * brightness);
                    glow.style.backgroundColor = glowColor;
                }
                
                // ボタンをフェードイン
                targetButton.style.opacity = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            targetButton.style.opacity = 1f;
            
            // 背景オーバーレイをフェードアウト
            float fadeOutDuration = 0.3f;
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                glowOverlay.style.backgroundColor = new Color(1f, 0.95f, 0.8f, Mathf.Lerp(0.3f, 0f, t));
                yield return null;
            }
            
            effectContainer.RemoveFromHierarchy();
        }
    }
}
