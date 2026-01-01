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
        /// シナリオ6解放演出を表示
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

            // 光の粒子演出（金色系に変更）
            int particleCount = 20;
            List<VisualElement> particles = new List<VisualElement>();
            for (int i = 0; i < particleCount; i++)
            {
                var p = new VisualElement();
                p.style.position = Position.Absolute;
                p.style.width = 10;
                p.style.height = 10;
                p.style.backgroundColor = new Color(1f, 0.84f, 0f); // 金色系
                p.style.borderTopLeftRadius = 5;
                p.style.borderTopRightRadius = 5;
                p.style.borderBottomLeftRadius = 5;
                p.style.borderBottomRightRadius = 5;
                p.style.left = startPos.x;
                p.style.top = startPos.y;
                effectContainer.Add(p);
                particles.Add(p);
            }

            // 真実の扉出現音を再生
            if (audioManager != null)
            {
                audioManager.PlaySparkleSound();
            }

            // アニメーション：集まってから飛んでいく
            float duration = 1.0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easeT = Mathf.SmoothStep(0, 1, t);

                for (int i = 0; i < particleCount; i++)
                {
                    // 少しバラけさせながら移動
                    float angle = (i / (float)particleCount) * Mathf.PI * 2;
                    float spread = (1 - t) * 50f;
                    Vector2 currentPos = Vector2.Lerp(startPos, endPos, easeT);
                    particles[i].style.left = currentPos.x + Mathf.Cos(angle) * spread;
                    particles[i].style.top = currentPos.y + Mathf.Sin(angle) * spread;
                    particles[i].style.opacity = 1.0f - (t * 0.5f);
                    particles[i].style.scale = new StyleScale(new Scale(Vector3.one * (1.5f - t)));
                }
                yield return null;
            }

            // 最後に大きな光（金色系に変更）
            var flash = new VisualElement();
            flash.style.position = Position.Absolute;
            flash.style.left = endPos.x - 50;
            flash.style.top = endPos.y - 50;
            flash.style.width = 100;
            flash.style.height = 100;
            flash.style.backgroundColor = new Color(1f, 0.84f, 0f); // 金色系
            flash.style.borderTopLeftRadius = 50;
            flash.style.borderTopRightRadius = 50;
            flash.style.borderBottomLeftRadius = 50;
            flash.style.borderBottomRightRadius = 50;
            flash.style.opacity = 1f;
            effectContainer.Add(flash);

            if (audioManager != null)
            {
                audioManager.PlayTruthDoorUnlockSound();
            }

            float flashDuration = 0.5f;
            elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                flash.style.opacity = 1.0f - t;
                flash.style.scale = new StyleScale(new Scale(Vector3.one * (1f + t * 2f)));
                
                // ボタンをフェードイン
                targetButton.style.opacity = t;
                yield return null;
            }

            targetButton.style.opacity = 1f;
            effectContainer.RemoveFromHierarchy();
        }
    }
}

