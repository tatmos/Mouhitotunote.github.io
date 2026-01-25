using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// シナリオ画面の表示を管理するクラス
    /// </summary>
    public class ScenarioScreenManager
    {
        private VisualElement root;
        private GameManager gameManager;
        private AudioManager audioManager;
        private TypewriterEffectManager typewriterEffectManager;
        private CountdownManager countdownManager;
        private ScreenTransitionManager screenTransitionManager;
        private ShakeAnimationManager shakeAnimationManager;
        
        // Settings
        private Sprite[] scenarioBackgrounds;
        private Sprite uiButtonNormalImage;
        private Sprite uiButtonDarkImage;
        private Sprite clockIcon;
        
        // Actions (コールバック)
        private System.Action<int> onStartAmbientSound;
        private System.Action onUpdateScoreDisplay;
        private System.Action<int, bool> onSetBackgroundImage; // scenarioId, isScenarioScreen
        private System.Action<Button, Sprite, Color> onApplyButtonImage;
        private System.Action onPlayHoverSound;
        private System.Action<VisualElement, Scenario, System.Action<int>> onCreateChoiceButtons;
        private System.Func<VisualElement, IEnumerator> onShowChoicesSequentially;
        private System.Action<int, bool> onChoiceSelected; // choiceId, wordFound
        private System.Func<Label, IEnumerator> onShakeAnimation;
        private System.Func<Vector2, VisualElement, IEnumerator> onShowLetterGetAnimation;
        private System.Action onShowSelectionScreen;
        private System.Action onShowTitleScreenWithFade;
        
        // フラグ
        private bool wordFoundInCurrentScenario = false;
        
        public ScenarioScreenManager(
            VisualElement root,
            GameManager gameManager,
            AudioManager audioManager,
            TypewriterEffectManager typewriterEffectManager,
            CountdownManager countdownManager,
            ScreenTransitionManager screenTransitionManager,
            ShakeAnimationManager shakeAnimationManager,
            ScenarioScreenSettings settings,
            ScenarioScreenActions actions)
        {
            this.root = root;
            this.gameManager = gameManager;
            this.audioManager = audioManager;
            this.typewriterEffectManager = typewriterEffectManager;
            this.countdownManager = countdownManager;
            this.screenTransitionManager = screenTransitionManager;
            this.shakeAnimationManager = shakeAnimationManager;
            
            // Set settings
            this.scenarioBackgrounds = settings.scenarioBackgrounds;
            this.uiButtonNormalImage = settings.uiButtonNormalImage;
            this.uiButtonDarkImage = settings.uiButtonDarkImage;
            this.clockIcon = settings.clockIcon;
            
            // Set actions
            this.onStartAmbientSound = actions.onStartAmbientSound;
            this.onUpdateScoreDisplay = actions.onUpdateScoreDisplay;
            this.onSetBackgroundImage = actions.onSetBackgroundImage;
            this.onApplyButtonImage = actions.onApplyButtonImage;
            this.onPlayHoverSound = actions.onPlayHoverSound;
            this.onCreateChoiceButtons = actions.onCreateChoiceButtons;
            this.onShowChoicesSequentially = actions.onShowChoicesSequentially;
            this.onChoiceSelected = actions.onChoiceSelected;
            this.onShakeAnimation = actions.onShakeAnimation;
            this.onShowLetterGetAnimation = actions.onShowLetterGetAnimation;
            this.onShowSelectionScreen = actions.onShowSelectionScreen;
            this.onShowTitleScreenWithFade = actions.onShowTitleScreenWithFade;
        }
        
        /// <summary>
        /// シナリオ画面をセットアップ
        /// </summary>
        public void Setup(Scenario scenario, MonoBehaviour coroutineRunner)
        {
            if (root == null || scenario == null) return;
            
            // フラグをリセット
            wordFoundInCurrentScenario = false;
            
            // スクロールバーのスタイルを適用
            ApplyScrollbarStyle();
            
            // 背景画像を設定
            onSetBackgroundImage?.Invoke(scenario.id, true);
            
            // タイトルを設定
            SetupTitle(scenario);
            
            // スタイルを適用
            ApplyStyles();
            
            // 時計アイコンを設定
            SetupClockIcon();
            
            // カウントダウンを停止
            if (countdownManager != null)
            {
                countdownManager.StopCountdown();
            }
            
            // UI要素を非表示にする
            HideUIElements();
            
            // SetupTextを設定してタイプライター効果で表示
            SetupScenarioText(scenario, coroutineRunner);
            
            // 選択肢ボタンを作成
            if (onCreateChoiceButtons != null && onChoiceSelected != null)
            {
                onCreateChoiceButtons(root, scenario, (choiceId) => {
                    // wordFoundInCurrentScenarioフラグを取得してonChoiceSelectedを呼び出す
                    bool wordFound = wordFoundInCurrentScenario;
                    onChoiceSelected(choiceId, wordFound);
                });
            }
            
            // スコア表示を更新
            onUpdateScoreDisplay?.Invoke();
            
            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root, withScale: true);
            }
        }
        
        /// <summary>
        /// スクロールバーのスタイルを適用
        /// </summary>
        private void ApplyScrollbarStyle()
        {
            var scrollView = root.Q<ScrollView>("ScenarioScrollView");
            if (scrollView == null) return;
            
            // 複数のタイミングで適用（確実に適用されるように）
            scrollView.RegisterCallback<GeometryChangedEvent>(evt => {
                ApplyVerticalScrollbarStyle(scrollView);
            });
            
            scrollView.RegisterCallback<AttachToPanelEvent>(evt => {
                scrollView.schedule.Execute(() => {
                    ApplyVerticalScrollbarStyle(scrollView);
                }).ExecuteLater(50);
            });
            
            // 即座にも適用を試みる（既にレンダリングされている場合）
            ApplyVerticalScrollbarStyle(scrollView);
            
            // 複数のタイミングで適用（確実に適用されるように）
            scrollView.schedule.Execute(() => {
                ApplyVerticalScrollbarStyle(scrollView);
            }).ExecuteLater(50);
            
            scrollView.schedule.Execute(() => {
                ApplyVerticalScrollbarStyle(scrollView);
            }).ExecuteLater(100);
            
            scrollView.schedule.Execute(() => {
                ApplyVerticalScrollbarStyle(scrollView);
            }).ExecuteLater(200);
            
            scrollView.schedule.Execute(() => {
                ApplyVerticalScrollbarStyle(scrollView);
            }).ExecuteLater(500);
        }
        
        /// <summary>
        /// 縦スクロールバーのスタイルを適用（ゲームのデザインに合わせる）
        /// </summary>
        private void ApplyVerticalScrollbarStyle(ScrollView scrollView)
        {
            if (scrollView == null) return;

            // 縦スクロールバーのコンテナ（複数の方法で確実に検索）
            VisualElement verticalScroller = null;
            
            // 方法1: 直接検索
            verticalScroller = scrollView.Q<VisualElement>(className: "unity-scroll-view__vertical-scroller");
            
            // 方法2: すべての子要素から検索
            if (verticalScroller == null)
            {
                var scrollViewChildren = scrollView.Children().ToList();
                foreach (var child in scrollViewChildren)
                {
                    if (child.ClassListContains("unity-scroll-view__vertical-scroller"))
                    {
                        verticalScroller = child;
                        break;
                    }
                }
            }
            
            // 方法3: Queryを使用して検索
            if (verticalScroller == null)
            {
                var scrollers = scrollView.Query<VisualElement>(className: "unity-scroll-view__vertical-scroller").ToList();
                if (scrollers.Count > 0)
                {
                    verticalScroller = scrollers[0];
                }
            }
            
            // 方法4: すべての子孫要素から検索（最も確実）
            if (verticalScroller == null)
            {
                var scrollViewDescendants = scrollView.Query<VisualElement>(className: "unity-scroll-view__vertical-scroller").ToList();
                if (scrollViewDescendants.Count > 0)
                {
                    verticalScroller = scrollViewDescendants[0];
                }
            }
            
            if (verticalScroller == null) return;
            
            // コンテナのスタイルを適用（背景は透明、ボタンは非表示）
            verticalScroller.style.width = 10;
            verticalScroller.style.backgroundColor = Color.clear; // 背景を透明に
            verticalScroller.style.borderTopLeftRadius = 5;
            verticalScroller.style.borderTopRightRadius = 5;
            verticalScroller.style.borderBottomLeftRadius = 5;
            verticalScroller.style.borderBottomRightRadius = 5;
            verticalScroller.style.borderTopWidth = 0;
            verticalScroller.style.borderRightWidth = 0;
            verticalScroller.style.borderBottomWidth = 0;
            verticalScroller.style.borderLeftWidth = 0;
            verticalScroller.style.width = new StyleLength(new Length(10, LengthUnit.Pixel));
            verticalScroller.MarkDirtyRepaint();

            // スクロールバー内のすべての子要素を検索
            var scrollerChildren = verticalScroller.Children().ToList();
            foreach (var child in scrollerChildren)
            {
                // ボタン要素を非表示（上矢印・下矢印ボタン）
                string className = string.Join(" ", child.GetClasses());
                string name = child.name;
                
                // ボタンっぽい要素を非表示
                if (className.Contains("button") || className.Contains("Button") ||
                    className.Contains("up") || className.Contains("down") ||
                    className.Contains("scrollbar") && (className.Contains("up") || className.Contains("down")) ||
                    name.Contains("button") || name.Contains("Button") ||
                    name.Contains("up") || name.Contains("down") ||
                    name.Contains("Up") || name.Contains("Down"))
                {
                    child.style.display = DisplayStyle.None;
                    child.style.visibility = Visibility.Hidden;
                }
                
                // Button型の要素も非表示
                if (child is Button)
                {
                    child.style.display = DisplayStyle.None;
                    child.style.visibility = Visibility.Hidden;
                }
            }

            // ドラッガー（つまみ）を検索（複数のパターンで確実に検索）
            VisualElement dragger = null;
            
            // 方法1: 直接検索
            dragger = verticalScroller.Q<VisualElement>(className: "unity-base-slider__dragger");
            
            // 方法2: Slider内のドラッガーを検索
            if (dragger == null)
            {
                var slider = verticalScroller.Q<Slider>();
                if (slider != null)
                {
                    dragger = slider.Q<VisualElement>(className: "unity-base-slider__dragger");
                }
            }
            
            // 方法3: すべての子孫要素から検索
            if (dragger == null)
            {
                var draggerDescendants = verticalScroller.Query<VisualElement>(className: "unity-base-slider__dragger").ToList();
                if (draggerDescendants.Count > 0)
                {
                    dragger = draggerDescendants[0];
                }
            }
            
            if (dragger != null)
            {
                // ドラッガーのスタイルを適用（重要度: 最高）
                dragger.style.backgroundColor = new Color(218f / 255f, 165f / 255f, 32f / 255f, 0.8f);
                dragger.style.borderTopLeftRadius = 4;
                dragger.style.borderTopRightRadius = 4;
                dragger.style.borderBottomLeftRadius = 4;
                dragger.style.borderBottomRightRadius = 4;
                dragger.style.width = 8;
                dragger.style.marginLeft = 1;
                dragger.style.marginRight = 1;
                dragger.style.marginTop = 1;
                dragger.style.marginBottom = 1;
                dragger.style.borderTopWidth = 0;
                dragger.style.borderRightWidth = 0;
                dragger.style.borderBottomWidth = 0;
                dragger.style.borderLeftWidth = 0;
                dragger.MarkDirtyRepaint();
            }

            // トラッカー（背景）を検索（複数のパターンで確実に検索）
            VisualElement tracker = null;
            
            // 方法1: 直接検索
            tracker = verticalScroller.Q<VisualElement>(className: "unity-base-slider__tracker");
            
            // 方法2: Slider内のトラッカーを検索
            if (tracker == null)
            {
                var slider = verticalScroller.Q<Slider>();
                if (slider != null)
                {
                    tracker = slider.Q<VisualElement>(className: "unity-base-slider__tracker");
                }
            }
            
            // 方法3: すべての子孫要素から検索
            if (tracker == null)
            {
                var trackerDescendants = verticalScroller.Query<VisualElement>(className: "unity-base-slider__tracker").ToList();
                if (trackerDescendants.Count > 0)
                {
                    tracker = trackerDescendants[0];
                }
            }
            
            if (tracker != null)
            {
                // トラッカーのスタイルを適用
                tracker.style.backgroundColor = Color.clear;
                tracker.style.borderTopWidth = 0;
                tracker.style.borderRightWidth = 0;
                tracker.style.borderBottomWidth = 0;
                tracker.style.borderLeftWidth = 0;
                tracker.MarkDirtyRepaint();
            }
            
            // すべてのSlider要素にも直接スタイルを適用（念のため）
            var allSliders = verticalScroller.Query<Slider>().ToList();
            foreach (var slider in allSliders)
            {
                // Slider内のドラッガーとトラッカーを再検索
                var sliderDragger = slider.Q<VisualElement>(className: "unity-base-slider__dragger");
                if (sliderDragger != null)
                {
                    sliderDragger.style.backgroundColor = new Color(218f / 255f, 165f / 255f, 32f / 255f, 0.8f);
                    sliderDragger.style.borderTopLeftRadius = 4;
                    sliderDragger.style.borderTopRightRadius = 4;
                    sliderDragger.style.borderBottomLeftRadius = 4;
                    sliderDragger.style.borderBottomRightRadius = 4;
                    sliderDragger.style.width = 8;
                    sliderDragger.style.marginLeft = 1;
                    sliderDragger.style.marginRight = 1;
                    sliderDragger.style.marginTop = 1;
                    sliderDragger.style.marginBottom = 1;
                    sliderDragger.style.borderTopWidth = 0;
                    sliderDragger.style.borderRightWidth = 0;
                    sliderDragger.style.borderBottomWidth = 0;
                    sliderDragger.style.borderLeftWidth = 0;
                    sliderDragger.MarkDirtyRepaint();
                }
                
                var sliderTracker = slider.Q<VisualElement>(className: "unity-base-slider__tracker");
                if (sliderTracker != null)
                {
                    sliderTracker.style.backgroundColor = Color.clear;
                    sliderTracker.style.borderTopWidth = 0;
                    sliderTracker.style.borderRightWidth = 0;
                    sliderTracker.style.borderBottomWidth = 0;
                    sliderTracker.style.borderLeftWidth = 0;
                    sliderTracker.MarkDirtyRepaint();
                }
            }
        }
        
        /// <summary>
        /// タイトルを設定
        /// </summary>
        private void SetupTitle(Scenario scenario)
        {
            var titleLabel = root.Q<Label>("ScenarioTitleText");
            if (titleLabel == null) return;
            
            bool isDarkMode = gameManager.IsDarkMode();
            if (isDarkMode)
            {
                // ダークモード時のタイトル
                string darkTitle = scenario.id switch
                {
                    1 => "謎の依頼【データ破損】",
                    2 => "不思議なレストラン【データ破損】",
                    3 => "タイムカプセル【データ破損】",
                    4 => "魔法学校の試験【データ破損】",
                    5 => "最後のピース【データ破損】",
                    6 => "真実の扉【ダークモード】",
                    _ => scenario.title + "【データ破損】"
                };
                titleLabel.text = darkTitle;
                titleLabel.AddToClassList("title-text-dark");
            }
            else
            {
                titleLabel.text = scenario.title;
                titleLabel.AddToClassList("title-text");
            }
            
            // 明るい色を適用
            Color textColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
            titleLabel.style.color = textColor;
            titleLabel.style.textShadow = new TextShadow { offset = new Vector2(2, 2), blurRadius = 4, color = new Color(0, 0, 0, 0.8f) };
        }
        
        /// <summary>
        /// スタイルを適用
        /// </summary>
        private void ApplyStyles()
        {
            Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
            
            // スコア表示
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel != null)
            {
                scoreLabel.style.fontSize = 10; // 20pxから10pxに縮小（半分）
                scoreLabel.style.color = brightTextColor;
                scoreLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット成功メッセージ
            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
            if (wordFoundMessageLabel != null)
            {
                wordFoundMessageLabel.style.color = brightTextColor;
                wordFoundMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット失敗メッセージ
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.color = brightTextColor;
                wordFailedMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // カウントダウンテキスト
            var countdownText = root.Q<Label>("CountdownText");
            if (countdownText != null)
            {
                countdownText.style.color = brightTextColor;
                countdownText.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // SetupText内のLabelに明るい色を適用
            var setupContainer = root.Q<VisualElement>("SetupText");
            if (setupContainer != null)
            {
                Color textColor = brightTextColor;
                foreach (var child in setupContainer.Children())
                {
                    if (child is Label label)
                    {
                        label.style.color = textColor;
                        label.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                    }
                }
            }
        }
        
        /// <summary>
        /// 時計アイコンを設定
        /// </summary>
        private void SetupClockIcon()
        {
            var clockIconElement = root.Q<Image>("ClockIcon");
            if (clockIconElement != null && clockIcon != null)
            {
                clockIconElement.sprite = clockIcon;
            }
        }
        
        /// <summary>
        /// UI要素を非表示にする
        /// </summary>
        private void HideUIElements()
        {
            var choiceButtonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            if (choiceButtonContainer != null)
            {
                choiceButtonContainer.style.display = DisplayStyle.None;
            }
            
            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
            if (wordFoundMessageLabel != null)
            {
                wordFoundMessageLabel.style.display = DisplayStyle.None;
            }
            
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.display = DisplayStyle.None;
            }
            
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            if (countdownContainer != null)
            {
                countdownContainer.style.display = DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// シナリオテキストを設定してタイプライター効果で表示
        /// </summary>
        private void SetupScenarioText(Scenario scenario, MonoBehaviour coroutineRunner)
        {
            var setupContainer = root.Q<VisualElement>("SetupText");
            if (setupContainer == null) return;
            
            // 既存の子要素をクリア
            setupContainer.Clear();
            
            // ダークモード時のsetupテキストを取得
            bool isDarkMode = gameManager.IsDarkMode() && !gameManager.IsThirdLoop();
            bool isThirdLoop = gameManager.IsThirdLoop();
            string setupText = scenario.setup;
            
            if (isDarkMode)
            {
                setupText = scenario.id switch
                {
                    1 => $"【エラー】探偵事務所のデータが破損しています。\n写真の人物が歪み、存在が不安定になっています。\nバグの影響で「も」という文字が消失しました。\n\n{scenario.setup}",
                    2 => $"【エラー】レストランのデータが破損しています。\nメニューが文字化けし、料理のデータが読み込めません。\nバグの影響で「う」という文字が消失しました。\n\n{scenario.setup}",
                    3 => $"【エラー】タイムカプセルのデータが破損しています。\n過去の記憶が歪み、データが欠損しています。\nバグの影響で「ひ」という文字が消失しました。\n\n{scenario.setup}",
                    4 => $"【エラー】魔法学校のデータが破損しています。\n呪文のコードがエラーを起こし、魔法が機能しません。\nバグの影響で「と」という文字が消失しました。\n\n{scenario.setup}",
                    5 => $"【エラー】パズルのデータが破損しています。\nピースの整合性が失われ、完成することができません。\nバグの影響で「つ」という文字が消失しました。\n\n{scenario.setup}",
                    6 => scenario.setup,
                    _ => scenario.setup
                };
            }
            
            // タイプライター効果で表示
            if (typewriterEffectManager != null)
            {
                typewriterEffectManager.StartTypewriterEffectWithClickableWord(setupContainer, setupText, () =>
                {
                    // タイプライター効果が完了したら選択肢ボタンを順次表示
                    if (coroutineRunner != null && onShowChoicesSequentially != null)
                    {
                        coroutineRunner.StartCoroutine(onShowChoicesSequentially(root));
                    }
                }, (found, pos) => {
                    if (found)
                    {
                        wordFoundInCurrentScenario = true;
                        
                        // 効果音を再生
                        if (audioManager != null)
                        {
                            audioManager.PlayWordGetIncreaseSound();
                            audioManager.PlayWordGetSound();
                        }
                        
                        // メッセージを表示
                        var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
                        if (wordFoundMessageLabel != null && coroutineRunner != null)
                        {
                            wordFoundMessageLabel.text = isDarkMode || isThirdLoop
                                ? "⚠️ システムエラー：データ破損を検出 ⚠️"
                                : "あなたは何かをみつけた気がした";
                            wordFoundMessageLabel.style.display = DisplayStyle.Flex;
                            
                            if (onShakeAnimation != null)
                            {
                                coroutineRunner.StartCoroutine(onShakeAnimation(wordFoundMessageLabel));
                            }
                        }
                        
                        // 選択肢ボタンを順次表示
                        if (coroutineRunner != null && onShowChoicesSequentially != null)
                        {
                            coroutineRunner.StartCoroutine(onShowChoicesSequentially(root));
                        }

                        // スコア表示へ光が飛んでいく演出を開始
                        if (!isDarkMode && !isThirdLoop && coroutineRunner != null && onShowLetterGetAnimation != null)
                        {
                            coroutineRunner.StartCoroutine(onShowLetterGetAnimation(pos, root));
                        }
                    }
                });
            }
            else
            {
                // タイプライター効果がない場合は即座に選択肢ボタンを順次表示
                if (coroutineRunner != null && onShowChoicesSequentially != null)
                {
                    coroutineRunner.StartCoroutine(onShowChoicesSequentially(root));
                }
            }
        }
        
        /// <summary>
        /// ワードが見つかったかを取得
        /// </summary>
        public bool GetWordFoundInCurrentScenario()
        {
            return wordFoundInCurrentScenario;
        }
    }
    
    /// <summary>
    /// ScenarioScreenManager用の設定
    /// </summary>
    public struct ScenarioScreenSettings
    {
        public Sprite[] scenarioBackgrounds;
        public Sprite uiButtonNormalImage;
        public Sprite uiButtonDarkImage;
        public Sprite clockIcon;
    }
    
    /// <summary>
    /// ScenarioScreenManager用のアクション（コールバック）
    /// </summary>
    public struct ScenarioScreenActions
    {
        public System.Action<int> onStartAmbientSound;
        public System.Action onUpdateScoreDisplay;
        public System.Action<int, bool> onSetBackgroundImage; // scenarioId, isScenarioScreen
        public System.Action<Button, Sprite, Color> onApplyButtonImage;
        public System.Action onPlayHoverSound;
        public System.Action<VisualElement, Scenario, System.Action<int>> onCreateChoiceButtons; // root, scenario, onChoiceSelected
        public System.Func<VisualElement, IEnumerator> onShowChoicesSequentially;
        public System.Action<int, bool> onChoiceSelected; // choiceId, wordFound
        public System.Func<Label, IEnumerator> onShakeAnimation;
        public System.Func<Vector2, VisualElement, IEnumerator> onShowLetterGetAnimation;
        public System.Action onShowSelectionScreen;
        public System.Action onShowTitleScreenWithFade;
    }
}
