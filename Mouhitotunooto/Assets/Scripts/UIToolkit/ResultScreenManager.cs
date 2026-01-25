using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using NovelGame.Overlay;

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
        
        // 後日談テキスト（結果テキスト設定で使用）
        private string epilogueText = "";
        
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
            
            // オーディオのフェードアウト（UIManagerUIToolkit側で既に行われているため、ここでは呼び出さない）
            // onFadeOutAudioOnSceneChange?.Invoke();
            // onFadeOutAmbientSoundForResult?.Invoke();
            
            // スクロールバーを非表示にする（UIManagerUIToolkit側で既に行われているため、ここでは行わない）
            // root.style.overflow = Overflow.Hidden;
            
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
            Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
            
            // 後日談を設定
            SetupEpilogue(scenario, result, isDarkMode, brightTextColor);
            
            // ワードゲット表示を設定
            SetupWordGetDisplay(scenario, result, isDarkMode, brightTextColor);
            
            // 結果テキストを設定（タイプライター効果で表示）
            SetupResultText(scenario, result, isDarkMode, brightTextColor, coroutineRunner);
            
            // 戻るボタンを設定
            SetupBackButton(brightTextColor);
            
            // スクロールバーのスタイルを適用（UIManagerUIToolkit側で既に行われているため、ここでは行わない）
            // onApplyScrollbarStyles?.Invoke(root);
            
            // トランジション開始（UIManagerUIToolkit側で既に行われているため、ここでは行わない）
            // onUpdateScoreDisplay?.Invoke();
            // if (screenTransitionManager != null)
            // {
            //     screenTransitionManager.StartScreenTransition(root);
            // }
        }
        
        /// <summary>
        /// 結果テキストを設定（タイプライター効果で表示）
        /// このメソッドは、UIManagerUIToolkitから一時的に呼び出される
        /// 後でResultScreenManager内に実装を移行する
        /// </summary>
        public void SetupResultTextExternal(Scenario scenario, ScenarioResult result, bool isDarkMode, Color brightTextColor, MonoBehaviour coroutineRunner)
        {
            SetupResultText(scenario, result, isDarkMode, brightTextColor, coroutineRunner);
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
                
                // フィールドに保存（結果テキスト設定で使用）
                this.epilogueText = epilogueText;
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
                scoreLabel.style.fontSize = 10; // 20pxから10pxに縮小（半分）
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
            // 必要な要素を取得
            var epilogueContainer = root.Q<VisualElement>("EpilogueContainer");
            var epilogueLabel = root.Q<Label>("EpilogueText");
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            var wordGetLabel = root.Q<Label>("WordGetText");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
            // 結果テキストを設定（タイプライター効果で表示）
            var resultLabel = root.Q<Label>("ResultText");
            if (resultLabel != null)
            {
                string resultText = "";
                if (isDarkMode)
                {
                    // シナリオごとのダークモード結果テキスト
                    resultText = scenario.id switch
                    {
                        1 => result.choiceId == 1
                            ? "私：「も...もど...もどれない...」\n\nデータが歪み始める。写真から人物の姿が消えていく。\n\n壊れた声：「【エラー】もも子のデータが...破損しています...」\n「「も」という文字が...消失しました...」\n「写真の人物は...存在しません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、データが破壊されていることを理解した。"
                            : "私：「このデータは...修復できるか？」\n\n壊れた声：「修復不可能です...」\n「もも子のデータは...完全に破損しています...」\n「「も」という文字が...永遠に失われました...」\n\n写真はデータの欠片となって消えていく。\n探偵事務所の存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        2 => result.choiceId == 1
                            ? "私：「う...うみ...うみへ...」\n\nメニューが文字化けし、レストランのデータが崩壊していく。\n\n壊れた声：「【エラー】うみのデータが...破損しています...」\n「「う」という文字が...消失しました...」\n「料理のデータは...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、レストランが機能しなくなることを理解した。"
                            : "私：「システムエラーを報告する...」\n\n壊れた声：「報告は...無意味です...」\n「うみのデータは...完全に破損しています...」\n「「う」という文字が...永遠に失われました...」\n\n料理のデータは欠片となって消えていく。\nレストランの存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        3 => result.choiceId == 1
                            ? "私：「ひ...ひろ...ひろが...」\n\n過去の記憶が歪み、タイムカプセルのデータが崩壊していく。\n\n壊れた声：「【エラー】ひろのデータが...破損しています...」\n「「ひ」という文字が...消失しました...」\n「友情の記憶は...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、記憶が失われることを理解した。"
                            : "私：「データを修復しようとする...」\n\n壊れた声：「修復不可能です...」\n「ひろのデータは...完全に破損しています...」\n「「ひ」という文字が...永遠に失われました...」\n\n過去の記憶は欠片となって消えていく。\nタイムカプセルの存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        4 => result.choiceId == 1
                            ? "私：「と...とおる...とおるが...」\n\n魔法のコードがエラーを起こし、魔法学校のシステムが崩壊していく。\n\n壊れた声：「【エラー】とおるのデータが...破損しています...」\n「「と」という文字が...消失しました...」\n「魔法のデータは...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、魔法が機能しなくなることを理解した。"
                            : "私：「システムの整合性を確認する...」\n\n壊れた声：「確認は...無意味です...」\n「とおるのデータは...完全に破損しています...」\n「「と」という文字が...永遠に失われました...」\n\n呪文のコードは欠片となって消えていく。\n魔法学校の存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        5 => result.choiceId == 1
                            ? "私：「つ...つばさ...つばさが...」\n\nパズルのピースが永遠に足りず、完成のデータが崩壊していく。\n\n壊れた声：「【エラー】つばさのデータが...破損しています...」\n「「つ」という文字が...消失しました...」\n「愛の記憶は...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、パズルが完成できなくなることを理解した。"
                            : "私：「完成できないことに気づく...」\n\n壊れた声：「気づいても...もう遅い...」\n「つばさのデータは...完全に破損しています...」\n「「つ」という文字が...永遠に失われました...」\n\n愛の記憶は欠片となって消えていく。\nパズルの存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        6 => result.choiceId == 1
                            ? "私：「すみません...壊してしまって...」\n\n壊れた声：「謝っても...もう遅い...」\n世界が歪み始める。\n\n壊れた声：「この世界は...シミュレーションだった...」\n「あなたの異常な行動が...世界を破壊した...」\n「もう...修復できない...」\n\n画面が歪み、文字が崩れていく。\nあなたは、自分が何をしてしまったのか理解した。"
                            : "私：「この世界は...何ですか？」\n\n壊れた声：「シミュレーション...すべてが...」\n「あなたは...バグを起こした...」\n「世界の整合性が...崩壊している...」\n\n周囲の空間が歪み、現実が崩れていく。\n登場人物たちの姿が、データの欠片となって消えていく。\n\n壊れた声：「もう...戻れない...」\n「あなたは...世界を壊した...」",
                        _ => "【データ破損】"
                    };
                }
                else
                {
                    resultText = scenario.branches[result.choiceId].text;
                    
                    // シナリオ2（不思議なレストラン）の場合、選択肢1（本日のおすすめ）を選んだ時に料理に対するセリフを追加
                    if (scenario.id == 2 && result.choiceId == 1)
                    {
                        string todayRecommendation = RestaurantMenuManager.GetTodayRecommendation();
                        string comment = RestaurantCommentManager.GetCommentForDish(todayRecommendation);
                        
                        // 結果テキストの最後にセリフを追加
                        if (!string.IsNullOrEmpty(comment))
                        {
                            resultText += $"\n\n{comment}";
                        }
                    }
                }
                
                // 結果テキストをVisualElementに変更して「もうひとつ」をクリッカブルにする
                var resultContainer = new VisualElement();
                resultContainer.style.fontSize = UIConstants.FontSizeNormal;
                resultContainer.style.whiteSpace = WhiteSpace.Normal;
                resultContainer.style.maxWidth = 800;
                resultContainer.style.marginBottom = 20;
                resultContainer.style.alignItems = Align.FlexStart; // 左揃え
                resultContainer.style.alignSelf = Align.FlexStart; // 左揃え
                resultContainer.style.width = Length.Percent(100); // 幅を100%に設定
                
                // 元のLabelを非表示にして、新しいコンテナを追加
                resultLabel.style.display = DisplayStyle.None;
                resultLabel.parent.Insert(resultLabel.parent.IndexOf(resultLabel), resultContainer);
                
                // 結果テキストに「【もうひとつ】」が含まれているか確認
                string originalResultText = resultText; // フォーマット前のテキストを保存
                string[] mouhitotsuPatterns = { "【もうひとつ】", "もうひとつ", "もう、ひとつ", "もう,ひとつ", "『もうひとつ』", "\"もうひとつ\"", "「もうひとつ」" };
                bool hasMouhitotsu = false;
                foreach (var pattern in mouhitotsuPatterns)
                {
                    if (originalResultText.Contains(pattern))
                    {
                        hasMouhitotsu = true;
                        Debug.Log($"[ResultScreenManager] 「もうひとつ」パターンを検出: '{pattern}'");
                        break;
                    }
                }
                
                // 取得した文字に色を付け、失われた文字を伏字化（表示用のテキストをフォーマット）
                var collectedLetters = gameManager.GetCollectedLetters();
                var lostLetters = gameManager.GetLostLetters();
                string formattedResultText = TextFormatter.FormatText(originalResultText, collectedLetters, lostLetters, true);
                
                // タイプライター効果で表示
                if (typewriterEffectManager != null)
                {
                    if (hasMouhitotsu)
                    {
                        // 「もうひとつ」が含まれている場合：クリッカブルワード付きタイプライター効果
                        typewriterEffectManager.StartTypewriterEffectWithClickableWord(resultContainer, formattedResultText, () =>
                        {
                            // 既にワードが見つかっている場合は、カウントダウンの開始をスキップ
                            if (wordFoundInCurrentScenario)
                            {
                                Debug.Log("既にワードが見つかっているため、カウントダウンの開始をスキップします。");
                                return;
                            }

                            // 結果テキストのタイプライター効果が完了したらカウントダウンを開始
                            if (countdownManager != null)
                            {
                                countdownManager.StartCountdown(
                                    countdownText,
                                    countdownContainer,
                                    wordGetContainer,
                                    wordFailedMessageLabel,
                                    () => {
                                        // ワードが見つかった場合の処理
                                        wordFoundInCurrentScenario = true;
                                    },
                                    () => {
                                        // カウントダウン完了時の処理
                                        if (wordFoundInCurrentScenario && scenario != null && result != null)
                                        {
                                            int previousScore = gameManager.GetScore();
                                            gameManager.HandleChoice(result.choiceId, true);
                                            // resultを再取得
                                            result = gameManager.GetScenarioResult(scenario.id);
                                            
                                            // シナリオ6でスコアが7になった場合、背景を段階的に歪ませる
                                            int currentScore = gameManager.GetScore();
                                            if (scenario.id == 6 && previousScore == 6 && currentScore >= 7 && !isDarkMode)
                                            {
                                                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                                                if (backgroundImage != null && distortionEffectManager != null)
                                                {
                                                    distortionEffectManager.ApplyGradualBackgroundDistortion(backgroundImage, currentScore, 6);
                                                }
                                            }
                                            
                                            // Overlayイベント発火（「もうひとつ」成功）
                                            OverlayEventHub.Publish(new MouhitotuResultEvt(scenario.id, true, "カウントダウン中に発見"));
                                            
                                            // wordGetLabelのテキストを設定
                                            if (wordGetLabel != null)
                                            {
                                                wordGetLabel.ClearClassList();
                                                if (isDarkMode)
                                                {
                                                    wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                                                    wordGetLabel.AddToClassList("word-get-dark");
                                                }
                                                else
                                                {
                                                    // ✨を画像で置き換え
                                                    onSetupWordGetLabelWithSparkle?.Invoke(wordGetContainer, wordGetLabel, onGetMaskedWordGetText?.Invoke());
                                                    wordGetLabel.AddToClassList("word-get-success");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // ワードが見つからなかった場合
                                            if (scenario != null)
                                            {
                                                OverlayEventHub.Publish(new MouhitotuResultEvt(scenario.id, false, "カウントダウン終了"));
                                            }
                                        }
                                        
                                        onShowBackButton?.Invoke();
                                    },
                                    onShowBackButton
                                );
                            }
                        }, (found, pos) => {
                        if (found)
                        {
                            wordFoundInCurrentScenario = true;
                            
                            // Overlayイベント発火（「もうひとつ」成功）
                            if (scenario != null)
                            {
                                OverlayEventHub.Publish(new MouhitotuResultEvt(scenario.id, true, "クリックで発見"));
                            }
                            
                            // 効果音を再生
                            if (audioManager != null)
                            {
                                audioManager.PlayWordGetIncreaseSound();
                                audioManager.PlayWordGetSound();
                            }
                            
                            // カウントダウンを停止
                            if (countdownManager != null)
                            {
                                countdownManager.NotifyWordFound();
                            }
                            
                            // カウントダウンコンテナを非表示にする
                            if (countdownContainer != null)
                            {
                                countdownContainer.style.display = DisplayStyle.None;
                            }
                            
                            // HandleChoiceを呼び出して、取得した文字をcollectedLettersに反映
                            if (scenario != null && result != null)
                            {
                                int previousScore = gameManager.GetScore();
                                gameManager.HandleChoice(result.choiceId, true);
                                // resultを再取得
                                result = gameManager.GetScenarioResult(scenario.id);
                                
                                // シナリオ6でスコアが7になった場合、背景を段階的に歪ませる
                                int currentScore = gameManager.GetScore();
                                if (scenario.id == 6 && previousScore == 6 && currentScore >= 7 && !isDarkMode)
                                {
                                    var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                                    if (backgroundImage != null && distortionEffectManager != null)
                                    {
                                        distortionEffectManager.ApplyGradualBackgroundDistortion(backgroundImage, currentScore, 6);
                                    }
                                }
                            }
                            
                            // 綺麗な演出とともに一呼吸してから表示
                            if (wordGetEffectManager != null)
                            {
                                coroutineRunner.StartCoroutine(wordGetEffectManager.ShowWordGetWithEffect(root, isDarkMode, pos, () =>
                                {
                                    // 演出完了後の処理
                                    if (wordGetLabel != null)
                                    {
                                        wordGetLabel.ClearClassList();
                                        if (isDarkMode)
                                        {
                                            wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                                            wordGetLabel.AddToClassList("word-get-dark");
                                        }
                                        else
                                        {
                                            // ✨を画像で置き換え
                                            onSetupWordGetLabelWithSparkle?.Invoke(wordGetContainer, wordGetLabel, onGetMaskedWordGetText?.Invoke());
                                            wordGetLabel.AddToClassList("word-get-success");
                                        }
                                        
                                        // フェードインとスケールアニメーション
                                        if (onAnimateWordGetLabelFadeIn != null)
                                        {
                                            coroutineRunner.StartCoroutine(onAnimateWordGetLabelFadeIn(wordGetLabel));
                                        }
                                    }
                                    
                                    // 後日談を表示
                                    if (epilogueContainer != null && epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
                                    {
                                        epilogueContainer.style.display = DisplayStyle.Flex;
                                        if (typewriterEffectManager != null)
                                        {
                                            typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText, () =>
                                            {
                                                // 後日談のタイプライター効果が完了したら戻るボタンを表示
                                                onShowBackButton?.Invoke();
                                            });
                                        }
                                        else
                                        {
                                            onShowBackButton?.Invoke();
                                        }
                                    }
                                    else
                                    {
                                        onShowBackButton?.Invoke();
                                    }
                                }));
                            }
                            else
                            {
                                // フォールバック：元のメソッドを使用
                                if (wordFoundInCurrentScenario && scenario != null && result != null)
                                {
                                    int previousScore = gameManager.GetScore();
                                    gameManager.HandleChoice(result.choiceId, true);
                                    result = gameManager.GetScenarioResult(scenario.id);
                                    
                                    int currentScore = gameManager.GetScore();
                                    if (scenario.id == 6 && previousScore == 6 && currentScore >= 7 && !isDarkMode)
                                    {
                                        var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                                        if (backgroundImage != null && distortionEffectManager != null)
                                        {
                                            distortionEffectManager.ApplyGradualBackgroundDistortion(backgroundImage, currentScore, 6);
                                        }
                                    }
                                }
                                if (onShowWordGetWithEffect != null)
                                {
                                    coroutineRunner.StartCoroutine(onShowWordGetWithEffect(root, isDarkMode, scenario, result, epilogueContainer, epilogueLabel, pos));
                                }
                            }
            
                            // スコア表示へ光が飛んでいく演出を開始
                            if (!isDarkMode && wordGetEffectManager != null)
                            {
                                coroutineRunner.StartCoroutine(wordGetEffectManager.ShowLetterGetAnimation(pos, root));
                            }
                            else if (!isDarkMode && onShowLetterGetAnimation != null)
                            {
                                coroutineRunner.StartCoroutine(onShowLetterGetAnimation(pos, root));
                            }
                        }
                    }, fontSize: 18, isClickable: true, originalText: originalResultText);
                    }
                    else
                    {
                        // 「もうひとつ」が含まれていない場合：通常のタイプライター効果
                        var resultLabelForTypewriter = new Label();
                        resultLabelForTypewriter.style.fontSize = UIConstants.FontSizeNormal;
                        resultLabelForTypewriter.style.whiteSpace = WhiteSpace.Normal;
                        resultLabelForTypewriter.style.maxWidth = 800;
                        resultLabelForTypewriter.style.marginBottom = 20;
                        resultLabelForTypewriter.style.alignSelf = Align.FlexStart; // 左揃え
                        resultLabelForTypewriter.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                        // 明るい色を適用
                        resultLabelForTypewriter.style.color = brightTextColor;
                        resultLabelForTypewriter.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        resultContainer.Add(resultLabelForTypewriter);
                        
                        typewriterEffectManager.StartTypewriterEffect(resultLabelForTypewriter, resultText, () =>
                        {
                            // タイプライター効果が完了したら、即座に戻るボタンを表示
                            onShowBackButton?.Invoke();
                        });
                    }
                    
                    // resultContainer内のすべてのLabelに明るい色を適用
                    foreach (var child in resultContainer.Children())
                    {
                        if (child is Label label)
                        {
                            label.style.color = brightTextColor;
                            label.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        }
                    }
                }
            }
            else if (wordFoundInCurrentScenario && epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
            {
                // 結果テキストがない場合は即座に後日談を表示（wordFoundInCurrentScenarioがtrueの場合のみ）
                if (epilogueContainer != null)
                {
                    epilogueContainer.style.display = DisplayStyle.Flex;
                }
                if (typewriterEffectManager != null)
                {
                    typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText);
                }
            }
            if (wordGetContainer != null)
            {
                wordGetContainer.style.display = DisplayStyle.None;
            }
            
            // wordGetLabelのテキストは、カウントダウンが終了した時、または「もうひとつ」をクリックした時に設定する
            if (wordGetLabel != null)
            {
                wordGetLabel.ClearClassList();
                wordGetLabel.text = ""; // テキストは後で設定
                // 非表示時は背景画像をクリア
                wordGetLabel.style.backgroundImage = null;
                wordGetLabel.style.backgroundColor = Color.clear;
                wordGetLabel.style.paddingTop = 0;
                wordGetLabel.style.paddingBottom = 0;
                wordGetLabel.style.paddingLeft = 0;
                wordGetLabel.style.paddingRight = 0;
            }
            
            // 後日談のタイトルも更新
            var epilogueTitle = root.Q<Label>("EpilogueTitle");
            if (epilogueTitle != null)
            {
                epilogueTitle.ClearClassList();
                if (isDarkMode)
                {
                    epilogueTitle.AddToClassList("epilogue-title-dark");
                }
                else
                {
                    epilogueTitle.AddToClassList("epilogue-title");
                }
                // 明るい色を適用
                epilogueTitle.style.color = brightTextColor;
                epilogueTitle.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
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
        public System.Action<VisualElement> onApplyScrollbarStyles;
        public System.Action onShowBackButton;
        public System.Action<Button, Sprite, Color> onApplyButtonImage;
        public System.Func<Vector2, VisualElement, IEnumerator> onShowLetterGetAnimation;
        public System.Action onShowSelectionScreen;
        public System.Action onShowTitleScreenWithFade;
    }
}
