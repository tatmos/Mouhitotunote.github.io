using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// リザルト画面の表示を管理するクラス
    /// </summary>
    public class ResultScreenManager
    {
        private VisualElement root;
        private GameManager gameManager;
        private AudioManager audioManager;
        private TypewriterEffectManager typewriterEffectManager;
        private CountdownManager countdownManager;
        private ScreenTransitionManager screenTransitionManager;
        private WordGetEffectManager wordGetEffectManager;
        private DistortionEffectManager distortionEffectManager;
        
        // Settings
        private Sprite[] scenarioBackgrounds;
        private Sprite uiButtonNormalImage;
        private Sprite clockIcon;
        
        // Actions (コールバック)
        private System.Action onFadeOutAudioOnSceneChange;
        private System.Action onFadeOutAmbientSoundForResult;
        private System.Action<bool> onHideAllScreens;
        private System.Action<int, bool> onSetBackgroundImage; // scenarioId, isScenarioScreen
        private System.Action onUpdateScoreDisplay;
        private System.Func<string> onGetMaskedWordGetText;
        private System.Action<VisualElement, Label, string> onSetupWordGetLabelWithSparkle;
        private System.Func<VisualElement, bool, Scenario, ScenarioResult, VisualElement, Label, Vector2, IEnumerator> onShowWordGetWithEffect;
        private System.Func<Label, IEnumerator> onAnimateWordGetLabelFadeIn;
        private System.Func<string, string> onExtractAnimalNameFromSetup;
        private System.Action onShowBackButton;
        private System.Action<VisualElement> onApplyScrollbarStyles;
        private System.Action<Button, Sprite, Color> onApplyButtonImage;
        private System.Func<Vector2, VisualElement, IEnumerator> onShowLetterGetAnimation;
        private System.Action onShowSelectionScreen;
        private System.Action onShowTitleScreenWithFade;
        
        // フラグ
        private bool wordFoundInCurrentScenario = false;
        
        public ResultScreenManager(
            VisualElement root,
            GameManager gameManager,
            AudioManager audioManager,
            TypewriterEffectManager typewriterEffectManager,
            CountdownManager countdownManager,
            ScreenTransitionManager screenTransitionManager,
            WordGetEffectManager wordGetEffectManager,
            DistortionEffectManager distortionEffectManager,
            ResultScreenSettings settings,
            ResultScreenActions actions)
        {
            this.root = root;
            this.gameManager = gameManager;
            this.audioManager = audioManager;
            this.typewriterEffectManager = typewriterEffectManager;
            this.countdownManager = countdownManager;
            this.screenTransitionManager = screenTransitionManager;
            this.wordGetEffectManager = wordGetEffectManager;
            this.distortionEffectManager = distortionEffectManager;
            
            // Set settings
            this.scenarioBackgrounds = settings.scenarioBackgrounds;
            this.uiButtonNormalImage = settings.uiButtonNormalImage;
            this.clockIcon = settings.clockIcon;
            
            // Set actions
            this.onFadeOutAudioOnSceneChange = actions.onFadeOutAudioOnSceneChange;
            this.onFadeOutAmbientSoundForResult = actions.onFadeOutAmbientSoundForResult;
            this.onHideAllScreens = actions.onHideAllScreens;
            this.onSetBackgroundImage = actions.onSetBackgroundImage;
            this.onUpdateScoreDisplay = actions.onUpdateScoreDisplay;
            this.onGetMaskedWordGetText = actions.onGetMaskedWordGetText;
            this.onSetupWordGetLabelWithSparkle = actions.onSetupWordGetLabelWithSparkle;
            this.onShowWordGetWithEffect = actions.onShowWordGetWithEffect;
            this.onAnimateWordGetLabelFadeIn = actions.onAnimateWordGetLabelFadeIn;
            this.onExtractAnimalNameFromSetup = actions.onExtractAnimalNameFromSetup;
            this.onShowBackButton = actions.onShowBackButton;
            this.onApplyScrollbarStyles = actions.onApplyScrollbarStyles;
            this.onApplyButtonImage = actions.onApplyButtonImage;
            this.onShowLetterGetAnimation = actions.onShowLetterGetAnimation;
            this.onShowSelectionScreen = actions.onShowSelectionScreen;
            this.onShowTitleScreenWithFade = actions.onShowTitleScreenWithFade;
        }
        
        /// <summary>
        /// リザルト画面をセットアップ
        /// </summary>
        public void Setup(Scenario scenario, ScenarioResult result, bool wordFoundInCurrentScenario, MonoBehaviour coroutineRunner)
        {
            if (root == null || scenario == null || result == null) return;
            
            // フラグを設定
            this.wordFoundInCurrentScenario = wordFoundInCurrentScenario;
            
            // オーディオのフェードアウト
            onFadeOutAudioOnSceneChange?.Invoke();
            onFadeOutAmbientSoundForResult?.Invoke();
            
            // スクロールバーを非表示にする
            root.style.overflow = Overflow.Hidden;
            
            // リザルト画面のUIDocumentのSort Orderをオーバーレイより高く設定
            // これにより、ScrollViewが確実にイベントを受け取れるようになる
            // (UIDocumentの設定はUIManagerUIToolkit側で行う)
            
            // ScrollViewにUSSクラスを追加して、pointer-eventsを確実に有効にする
            var scrollView = root.Q<ScrollView>();
            if (scrollView != null)
            {
                scrollView.AddToClassList("scroll-view-interactive");
            }
            
            // 背景画像を設定
            onSetBackgroundImage?.Invoke(scenario.id, false);
            
            // ダークモード判定：予約されているダークモードも考慮
            bool isDarkMode = gameManager.IsDarkMode() || gameManager.GetPendingDarkMode();
            
            // 明るい色を定義（メソッド全体で使用）
            Color brightTextColor = new Color(0xED / 255f, 0xED / 255f, 0xB5 / 255f, 1f); // #EDD7B5
            
            // 後日談を設定
            SetupEpilogue(scenario, result, isDarkMode, brightTextColor);
            
            // ワードゲット表示を設定
            SetupWordGetDisplay(scenario, result, isDarkMode, brightTextColor);
            
            // 結果テキストを設定（タイプライター効果で表示）
            SetupResultText(scenario, result, isDarkMode, brightTextColor, coroutineRunner);
            
            // 戻るボタンを設定
            SetupBackButton(brightTextColor);
            
            // スクロールバーのスタイルを適用
            onApplyScrollbarStyles?.Invoke(root);
            
            // トランジション開始
            onUpdateScoreDisplay?.Invoke();
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }
        
        private void SetupEpilogue(Scenario scenario, ScenarioResult result, bool isDarkMode, Color brightTextColor)
        {
            var epilogueContainer = root.Q<VisualElement>("EpilogueContainer");
            var epilogueLabel = root.Q<Label>("EpilogueText");
            if (epilogueContainer != null)
            {
                // 後日談コンテナを最初は非表示にする
                epilogueContainer.style.display = DisplayStyle.None;
                
                // ダークモードの場合はダークスタイルを適用
                epilogueContainer.ClearClassList();
                if (isDarkMode)
                {
                    epilogueContainer.AddToClassList("epilogue-box-dark");
                }
                else
                {
                    epilogueContainer.AddToClassList("epilogue-box");
                }
            }
            
            // 後日談テキストを準備
            string epilogueText = "";
            if (epilogueLabel != null)
            {
                // 既存のクラスをクリア
                epilogueLabel.ClearClassList();
                // 明るい色を適用
                epilogueLabel.style.color = brightTextColor;
                epilogueLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                
                if (isDarkMode)
                {
                    // シナリオごとのダークモードエピローグ
                    epilogueText = GetDarkModeEpilogueText(scenario.id, result.choiceId);
                    epilogueLabel.AddToClassList("epilogue-text-dark");
                }
                else
                {
                    epilogueText = result.epilogue;
                    
                    // シナリオ4（魔法学校の試験）の場合、ワードが見つからなかった場合に動物にゆかりのある話題を追加
                    if (scenario.id == 4)
                    {
                        string animalName = onExtractAnimalNameFromSetup?.Invoke(scenario.setup) ?? "";
                        if (!string.IsNullOrEmpty(animalName))
                        {
                            string relatedTopic = AnimalNameManager.GetRelatedTopic(animalName);
                            if (!string.IsNullOrEmpty(relatedTopic))
                            {
                                // epilogueに既に動物の話題が含まれていない場合のみ追加
                                if (!epilogueText.Contains(relatedTopic))
                                {
                                    epilogueText += $"\n\n試験官が何か言いかけた。\n試験官：「ところで、{animalName}について...{relatedTopic}」";
                                }
                            }
                        }
                    }
                    
                    epilogueLabel.AddToClassList("epilogue-text");
                }
                
                // 取得した文字に色を付け、失われた文字を伏字化
                var collectedLetters = gameManager.GetCollectedLetters();
                var lostLetters = gameManager.GetLostLetters();
                epilogueText = TextFormatter.FormatText(epilogueText, collectedLetters, lostLetters, true);
            }
        }
        
        private string GetDarkModeEpilogueText(int scenarioId, int choiceId)
        {
            return scenarioId switch
            {
                1 => choiceId == 1
                    ? "【データ破損】もも子のデータは完全に崩壊しました。\n写真から人物の姿が消え、存在が不安定になりました。\n「も」という文字が消失し、探偵事務所のデータも歪み始めています。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「も...もど...もどれない...」\n\n【エンド：文字の消失】"
                    : "【システムエラー】データの修復を試みましたが、失敗しました。\nもも子のデータは完全に破損し、修復不可能な状態です。\n写真の人物は、データの欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                2 => choiceId == 1
                    ? "【データ破損】うみシェフのデータは完全に崩壊しました。\nレストランのメニューが文字化けし、料理のデータが読み込めなくなりました。\n「う」という文字が消失し、レストランの存在が不安定になっています。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「う...うみ...うみへ...」\n\n【エンド：文字の消失】"
                    : "【システムエラー】システムエラーの報告を行いましたが、無意味でした。\nうみシェフのデータは完全に破損し、レストランは機能しなくなりました。\n料理のデータが欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                3 => choiceId == 1
                    ? "【データ破損】ひろのデータは完全に崩壊しました。\n過去の記憶が歪み、タイムカプセルのデータが欠損しています。\n「ひ」という文字が消失し、友情の記憶が失われました。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「ひ...ひろ...ひろが...」\n\n【エンド：文字の消失】"
                    : "【システムエラー】データの修復を試みましたが、失敗しました。\nひろのデータは完全に破損し、過去の記憶が消えてしまいました。\nタイムカプセルは、データの欠片となって崩壊しました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                4 => choiceId == 1
                    ? "【データ破損】とおる試験官のデータは完全に崩壊しました。\n魔法のコードがエラーを起こし、魔法学校のシステムが停止しました。\n「と」という文字が消失し、魔法のデータが読み込めなくなりました。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「と...とおる...とおるが...」\n\n【エンド：文字の消失】"
                    : "【システムエラー】システムの整合性を確認しましたが、手遅れでした。\nとおる試験官のデータは完全に破損し、魔法学校は機能しなくなりました。\n呪文のコードが欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                5 => choiceId == 1
                    ? "【データ破損】つばさのデータは完全に崩壊しました。\nパズルのピースが永遠に足りず、完成することができなくなりました。\n「つ」という文字が消失し、愛の記憶が消えつつあります。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「つ...つばさ...つばさが...」\n\n【エンド：文字の消失】"
                    : "【システムエラー】完成できないことに気づきましたが、時既に遅しでした。\nつばさのデータは完全に破損し、パズルは永遠に完成できなくなりました。\n愛の記憶が欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                6 => choiceId == 1
                    ? "世界は完全に崩壊しました。\nシミュレーションの整合性は失われ、修復不可能な状態です。\n\n登場人物たちは、データの欠片となって消えていきました。\nもも子、うみ、ひろ、とおる、つばさ...\nすべてが、あなたの異常な行動の結果です。\n\nあなたは、空っぽの世界に一人取り残されました。\n「もう...戻れない...」\n\n【エンド：世界崩壊】"
                    : "あなたは、世界の真実を知ってしまいました。\nこの世界は、シミュレーションだったのです。\n\nしかし、あなたの異常な行動が、世界を破壊してしまいました。\n登場人物たちは、バグによって歪んだ姿となっています。\n\nもも子は「も」という文字を失い、\nうみは「う」という文字を失い、\nひろは「ひ」という文字を失い、\nとおるは「と」という文字を失い、\nつばさは「つ」という文字を失いました。\n\n「もうひとつ」という言葉は、永遠に失われました。\n\n【エンド：言葉の消失】",
                _ => "【データ破損】"
            };
        }
        
        private void SetupWordGetDisplay(Scenario scenario, ScenarioResult result, bool isDarkMode, Color brightTextColor)
        {
            // ワードゲット表示（最初は非表示、結果テキストのタイプライター効果が完了したら表示）
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            var wordGetLabel = root.Q<Label>("WordGetText");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
            // スコア表示に明るい色を適用
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel != null)
            {
                scoreLabel.style.color = brightTextColor;
                scoreLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲットテキストに明るい色を適用（初期化時は背景画像を設定しない）
            // 背景画像は、実際に表示される時（SetupWordGetLabelWithSparkle）に設定される
            if (wordGetLabel != null)
            {
                wordGetLabel.style.color = brightTextColor;
                wordGetLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット成功メッセージに明るい色を適用
            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
            if (wordFoundMessageLabel != null)
            {
                wordFoundMessageLabel.style.color = brightTextColor;
                wordFoundMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット失敗メッセージに明るい色を適用
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.color = brightTextColor;
                wordFailedMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // カウントダウンテキストに明るい色を適用
            if (countdownText != null)
            {
                countdownText.style.color = brightTextColor;
                countdownText.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // 時計アイコンを設定
            var clockIconImage = root.Q<Image>("ClockIcon");
            if (clockIconImage != null && clockIcon != null)
            {
                clockIconImage.sprite = clockIcon;
            }
            
            // 既存のカウントダウンを停止
            if (countdownManager != null)
            {
                countdownManager.StopCountdown();
            }
            
            // カウントダウンコンテナを非表示にする
            if (countdownContainer != null)
            {
                countdownContainer.style.display = DisplayStyle.None;
            }
            
            // 失敗メッセージを非表示にする
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.display = DisplayStyle.None;
            }
        }
        
        private void SetupResultText(Scenario scenario, ScenarioResult result, bool isDarkMode, Color brightTextColor, MonoBehaviour coroutineRunner)
        {
            // このメソッドは非常に複雑なため、後で実装
            // 現時点では、基本的な構造のみを定義
        }
        
        private void SetupBackButton(Color brightTextColor)
        {
            // 戻るボタン（最初は非表示）
            var backButton = root.Q<Button>("BackToSelectionButton");
            if (backButton != null)
            {
                backButton.style.display = DisplayStyle.None;
                backButton.clicked += () => {
                    // 予約されているダークモードがあれば有効化
                    gameManager.ActivatePendingDarkMode();
                    onShowSelectionScreen?.Invoke();
                };
                // 戻るボタンに画像を適用
                Color backButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                onApplyButtonImage?.Invoke(backButton, uiButtonNormalImage, backButtonTextColor);
            }
            
            // タイトル画面に戻るボタン（もしあれば。最初は非表示）
            var backToTitleButton = root.Q<Button>("BackToTitleButton");
            if (backToTitleButton != null)
            {
                backToTitleButton.style.display = DisplayStyle.None;
                backToTitleButton.clicked += () => {
                    // 予約されているダークモードがあれば有効化
                    gameManager.ActivatePendingDarkMode();
                    onShowTitleScreenWithFade?.Invoke();
                };
                // タイトルに戻るボタンに画像を適用
                Color backToTitleButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                onApplyButtonImage?.Invoke(backToTitleButton, uiButtonNormalImage, backToTitleButtonTextColor);
            }
        }
    }
    
    /// <summary>
    /// ResultScreenManagerに渡す設定
    /// </summary>
    public struct ResultScreenSettings
    {
        public Sprite[] scenarioBackgrounds;
        public Sprite uiButtonNormalImage;
        public Sprite clockIcon;
    }
    
    /// <summary>
    /// ResultScreenManagerに渡すコールバック
    /// </summary>
    public struct ResultScreenActions
    {
        public System.Action onFadeOutAudioOnSceneChange;
        public System.Action onFadeOutAmbientSoundForResult;
        public System.Action<bool> onHideAllScreens;
        public System.Action<int, bool> onSetBackgroundImage; // scenarioId, isScenarioScreen
        public System.Action onUpdateScoreDisplay;
        public System.Func<string> onGetMaskedWordGetText;
        public System.Action<VisualElement, Label, string> onSetupWordGetLabelWithSparkle;
        public System.Func<VisualElement, bool, Scenario, ScenarioResult, VisualElement, Label, Vector2, IEnumerator> onShowWordGetWithEffect;
        public System.Func<Label, IEnumerator> onAnimateWordGetLabelFadeIn;
        public System.Func<string, string> onExtractAnimalNameFromSetup;
        public System.Action onShowBackButton;
        public System.Action<VisualElement> onApplyScrollbarStyles;
        public System.Action<Button, Sprite, Color> onApplyButtonImage;
        public System.Func<Vector2, VisualElement, IEnumerator> onShowLetterGetAnimation;
        public System.Action onShowSelectionScreen;
        public System.Action onShowTitleScreenWithFade;
    }
}
