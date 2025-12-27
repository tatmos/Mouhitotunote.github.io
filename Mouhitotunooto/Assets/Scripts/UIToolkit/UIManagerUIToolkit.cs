using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// UI ToolkitベースのUIManager
    /// </summary>
    public class UIManagerUIToolkit : MonoBehaviour
    {
        [Header("UI Documents")]
        [SerializeField] private UIDocument selectionScreenDocument;
        [SerializeField] private UIDocument scenarioScreenDocument;
        [SerializeField] private UIDocument resultScreenDocument;
        [SerializeField] private UIDocument profileScreenDocument;
        [SerializeField] private UIDocument creditsScreenDocument;
        [SerializeField] private UIDocument achievementsScreenDocument;

        [Header("UXML Files")]
        [SerializeField] private VisualTreeAsset selectionScreenUXML;
        [SerializeField] private VisualTreeAsset scenarioScreenUXML;
        [SerializeField] private VisualTreeAsset resultScreenUXML;
        [SerializeField] private VisualTreeAsset profileScreenUXML;
        [SerializeField] private VisualTreeAsset creditsScreenUXML;
        [SerializeField] private VisualTreeAsset achievementsScreenUXML;

        [Header("Background Images")]
        [SerializeField] private Sprite[] scenarioBackgrounds = new Sprite[6];
        [SerializeField] private Sprite selectionScreenBackground;
        [SerializeField] private Sprite profileScreenBackground;
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] wordGetSounds; // 「もうひとつ」をゲットした時の効果音（複数からランダムに選択）

        private GameManager gameManager;
        private UIDocument currentDocument;
        private List<GameObject> currentButtons = new List<GameObject>();
        
        // マネージャークラスのインスタンス
        private TypewriterEffectManager typewriterEffectManager;
        private CountdownManager countdownManager;
        private ScreenTransitionManager screenTransitionManager;
        private ProfileScreenManager profileScreenManager;
        private AchievementsScreenManager achievementsScreenManager;
        private CreditsScreenManager creditsScreenManager;
        
        // プロフィール関連
        private int selectedProfileId = 1; // 選択中のプロフィールID
        
        // 「もうひとつ」関連
        private bool wordFoundInCurrentScenario = false; // 現在のシナリオで「もうひとつ」を見つけたか
        
        // オーディオ関連
        private AudioSource audioSource; // 効果音再生用のAudioSource

        private void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManagerが見つかりません！");
                return;
            }

            // マネージャークラスのインスタンスを作成
            typewriterEffectManager = gameObject.AddComponent<TypewriterEffectManager>();
            countdownManager = gameObject.AddComponent<CountdownManager>();
            screenTransitionManager = gameObject.AddComponent<ScreenTransitionManager>();
            profileScreenManager = new ProfileScreenManager(gameManager);
            achievementsScreenManager = new AchievementsScreenManager(gameManager);
            creditsScreenManager = new CreditsScreenManager();
            
            // AudioSourceを追加（効果音再生用）
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            gameManager.OnScoreChanged += UpdateScoreDisplay;
            ShowSelectionScreen();
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreChanged -= UpdateScoreDisplay;
            }
        }

        public void ShowSelectionScreen()
        {
            HideAllScreens();
            
            if (selectionScreenDocument == null)
            {
                Debug.LogError("SelectionScreenDocumentがアサインされていません！");
                return;
            }

            selectionScreenDocument.gameObject.SetActive(true);
            currentDocument = selectionScreenDocument;
            
            var root = selectionScreenDocument.rootVisualElement;
            if (root == null) return;
            
            // 背景画像を設定
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                }
            }

            // タイトルを設定
            var titleLabel = root.Q<Label>("TitleText");
            if (titleLabel != null)
            {
                titleLabel.text = "ミニノベルゲーム";
                titleLabel.AddToClassList("title-text");
            }

            // プロフィールボタンの設定
            var showProfileButton = root.Q<Button>("ShowProfileButton");
            if (showProfileButton != null)
            {
                showProfileButton.clicked += ShowProfileScreen;
            }

            // エンドクレジットボタンの設定（真実の扉クリア後のみ表示）
            var showCreditsButton = root.Q<Button>("ShowCreditsButton");
            if (showCreditsButton != null)
            {
                var scenario6Result = gameManager.GetScenarioResult(6);
                if (scenario6Result != null)
                {
                    showCreditsButton.style.display = DisplayStyle.Flex;
                    showCreditsButton.clicked += ShowCreditsScreen;
                }
                else
                {
                    showCreditsButton.style.display = DisplayStyle.None;
                }
            }

            // 実績ボタンの設定（全シナリオクリア後のみ表示）
            var showAchievementsButton = root.Q<Button>("ShowAchievementsButton");
            if (showAchievementsButton != null)
            {
                var scenarios = gameManager.GetScenarios();
                int totalCompleted = 0;
                foreach (var scenario in scenarios)
                {
                    if (gameManager.IsScenarioCompleted(scenario.id))
                    {
                        totalCompleted++;
                    }
                }
                
                if (totalCompleted >= scenarios.Count)
                {
                    showAchievementsButton.style.display = DisplayStyle.Flex;
                    showAchievementsButton.clicked += ShowAchievementsScreen;
                }
                else
                {
                    showAchievementsButton.style.display = DisplayStyle.None;
                }
            }

            UpdateScoreDisplay();
            CreateScenarioButtons(root);
            
            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        public void ShowProfileScreen()
        {
            HideAllScreens();
            
            if (profileScreenDocument == null)
            {
                Debug.LogError("ProfileScreenDocumentがアサインされていません！");
                return;
            }

            profileScreenDocument.gameObject.SetActive(true);
            currentDocument = profileScreenDocument;
            
            var root = profileScreenDocument.rootVisualElement;
            
            // 背景画像を設定
            if (root != null && profileScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(profileScreenBackground);
                }
            }

            // タイトルを設定
            var titleLabel = root.Q<Label>("ProfileSectionTitle");
            if (titleLabel != null)
            {
                bool isDarkMode = gameManager.IsDarkMode();
                titleLabel.text = isDarkMode ? "登場人物プロフィール【データ破損】" : "登場人物プロフィール";
                if (isDarkMode)
                {
                    titleLabel.AddToClassList("title-text-dark");
                }
                else
                {
                    titleLabel.AddToClassList("title-text");
                }
            }

            if (profileScreenManager != null)
            {
                profileScreenManager.CreateProfileCards(root);
            }
            
            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromProfile");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }
            
            // トランジション開始
            if (root != null && screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        public void ShowScenarioScreen()
        {
            HideAllScreens();
            
            if (scenarioScreenDocument == null)
            {
                Debug.LogError("ScenarioScreenDocumentがアサインされていません！");
                return;
            }

            scenarioScreenDocument.gameObject.SetActive(true);
            currentDocument = scenarioScreenDocument;
            
            var scenario = gameManager.GetCurrentScenario();
            if (scenario == null) return;

            var root = scenarioScreenDocument.rootVisualElement;
            
            // 背景画像を設定
            SetBackgroundImage(scenario.id, true);

            // タイトルと設定テキストを設定
            var titleLabel = root.Q<Label>("ScenarioTitleText");
            if (titleLabel != null)
            {
                titleLabel.text = scenario.title;
                bool isDarkMode = gameManager.IsDarkMode() && scenario.id == 6;
                if (isDarkMode)
                {
                    titleLabel.AddToClassList("title-text-dark");
                }
                else
                {
                    titleLabel.AddToClassList("title-text");
                }
            }

            var setupContainer = root.Q<VisualElement>("SetupText");
            var choiceButtonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
            Debug.Log($"ShowScenarioScreen: choiceButtonContainer={(choiceButtonContainer != null ? "見つかった" : "見つからない")}");
            
            // フラグをリセット
            wordFoundInCurrentScenario = false;
            
            // 既存のカウントダウンを停止
            if (countdownManager != null)
            {
                countdownManager.StopCountdown();
            }
            
            // 選択肢ボタンコンテナを最初は非表示にする
            if (choiceButtonContainer != null)
            {
                choiceButtonContainer.style.display = DisplayStyle.None;
                Debug.Log("選択肢ボタンコンテナを非表示にしました");
            }
            else
            {
                Debug.LogWarning("choiceButtonContainerが見つかりません");
            }
            
            // メッセージラベルを非表示にする
            if (wordFoundMessageLabel != null)
            {
                wordFoundMessageLabel.style.display = DisplayStyle.None;
            }
            
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.display = DisplayStyle.None;
            }
            
            // カウントダウンコンテナを非表示にする
            if (countdownContainer != null)
            {
                countdownContainer.style.display = DisplayStyle.None;
            }
            
            if (setupContainer != null)
            {
                // 既存の子要素をクリア
                setupContainer.Clear();
                
                // タイプライター効果で表示（完了後に選択肢ボタンを表示）
                if (typewriterEffectManager != null)
                {
                    typewriterEffectManager.StartTypewriterEffectWithClickableWord(setupContainer, scenario.setup, () =>
                    {
                        // タイプライター効果が完了したら選択肢ボタンを表示
                        Debug.Log("setupのタイプライター効果が完了しました。選択肢ボタンを表示します。");
                        // 再取得を試みる
                        var buttonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
                        if (buttonContainer != null)
                        {
                            buttonContainer.style.display = DisplayStyle.Flex;
                            Debug.Log($"選択肢ボタンコンテナを表示: {buttonContainer.childCount}個のボタン");
                        }
                        else
                        {
                            Debug.LogWarning("choiceButtonContainerが見つかりません");
                        }
                    }, (found) => {
                        if (found)
                        {
                            wordFoundInCurrentScenario = true;
                            
                            // 効果音を再生
                            PlayWordGetSound();
                            
                            // メッセージを表示
                            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
                            if (wordFoundMessageLabel != null)
                            {
                                wordFoundMessageLabel.text = "あなたは何かをみつけた気がした";
                                wordFoundMessageLabel.style.display = DisplayStyle.Flex;
                                StartCoroutine(ShakeAnimation(wordFoundMessageLabel));
                            }
                            
                            // 選択肢ボタンを表示
                            var choiceButtonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
                            if (choiceButtonContainer != null)
                            {
                                choiceButtonContainer.style.display = DisplayStyle.Flex;
                            }
                        }
                    });
                }
            }
            else
            {
                // タイプライター効果がない場合は即座に選択肢ボタンを表示
                if (choiceButtonContainer != null)
                {
                    choiceButtonContainer.style.display = DisplayStyle.Flex;
                }
            }

            CreateChoiceButtons(root, scenario);

            // トランジション開始（シナリオ画面のみスケールアニメーションあり）
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root, withScale: true);
            }
        }

        public void ShowResultScreen()
        {
            HideAllScreens();
            
            if (resultScreenDocument == null)
            {
                Debug.LogError("ResultScreenDocumentがアサインされていません！");
                return;
            }

            resultScreenDocument.gameObject.SetActive(true);
            currentDocument = resultScreenDocument;
            
            var scenario = gameManager.GetCurrentScenario();
            if (scenario == null) return;

            var result = gameManager.GetScenarioResult(scenario.id);
            if (result == null) return;

            var root = resultScreenDocument.rootVisualElement;
            
            // 背景画像を設定
            SetBackgroundImage(scenario.id, false);

            bool isDarkMode = gameManager.IsDarkMode() && scenario.id == 6;

            // 後日談を設定（最初は非表示）
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
                
                if (isDarkMode)
                {
                    epilogueText = result.choiceId == 1
                        ? "世界は完全に崩壊しました。\nシミュレーションの整合性は失われ、修復不可能な状態です。\n\n登場人物たちは、データの欠片となって消えていきました。\nもも子、うみ、ひろ、とおる、つばさ...\nすべてが、あなたの異常な行動の結果です。\n\nあなたは、空っぽの世界に一人取り残されました。\n「もう...戻れない...」\n\n【エンド：世界崩壊】"
                        : "あなたは、世界の真実を知ってしまいました。\nこの世界は、シミュレーションだったのです。\n\nしかし、あなたの異常な行動が、世界を破壊してしまいました。\n登場人物たちは、バグによって歪んだ姿となっています。\n\nもも子は「も」という文字を失い、\nうみは「う」という文字を失い、\nひろは「ひ」という文字を失い、\nとおるは「と」という文字を失い、\nつばさは「つ」という文字を失いました。\n\n「もうひとつ」という言葉は、永遠に失われました。\n\n【エンド：言葉の消失】";
                    epilogueLabel.AddToClassList("epilogue-text-dark");
                }
                else
                {
                    epilogueText = result.epilogue;
                    epilogueLabel.AddToClassList("epilogue-text");
                }
            }

            // ワードゲット表示（最初は非表示、結果テキストのタイプライター効果が完了したら表示）
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            var wordGetLabel = root.Q<Label>("WordGetText");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
            // フラグをリセット（結果画面で「もうひとつ」を探すため）
            // ただし、すでにHandleChoiceでhasWord=trueになっている場合は、そのまま保持
            if (result != null && !result.hasWord)
            {
                wordFoundInCurrentScenario = false;
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
            
            // 結果テキストを設定（タイプライター効果で表示）
            var resultLabel = root.Q<Label>("ResultText");
            if (resultLabel != null)
            {
                string resultText = "";
                if (isDarkMode)
                {
                    resultText = result.choiceId == 1
                        ? "私：「すみません...壊してしまって...」\n\n壊れた声：「謝っても...もう遅い...」\n世界が歪み始める。\n\n壊れた声：「この世界は...シミュレーションだった...」\n「あなたの異常な行動が...世界を破壊した...」\n「もう...修復できない...」\n\n画面が歪み、文字が崩れていく。\nあなたは、自分が何をしてしまったのか理解した。"
                        : "私：「この世界は...何ですか？」\n\n壊れた声：「シミュレーション...すべてが...」\n「あなたは...バグを起こした...」\n「世界の整合性が...崩壊している...」\n\n周囲の空間が歪み、現実が崩れていく。\n登場人物たちの姿が、データの欠片となって消えていく。\n\n壊れた声：「もう...戻れない...」\n「あなたは...世界を壊した...」";
                }
                else
                {
                    resultText = scenario.branches[result.choiceId].text;
                }
                
                // 結果テキストをVisualElementに変更して「もうひとつ」をクリッカブルにする
                var resultContainer = new VisualElement();
                resultContainer.style.fontSize = 18;
                resultContainer.style.whiteSpace = WhiteSpace.Normal;
                resultContainer.style.maxWidth = 800;
                resultContainer.style.marginBottom = 20;
                
                // 元のLabelを非表示にして、新しいコンテナを追加
                resultLabel.style.display = DisplayStyle.None;
                resultLabel.parent.Insert(resultLabel.parent.IndexOf(resultLabel), resultContainer);
                
                // 結果テキストに「【もうひとつ】」が含まれているか確認
                bool hasMouhitotsu = resultText.Contains("【もうひとつ】");
                
                // タイプライター効果で表示
                if (typewriterEffectManager != null)
                {
                    if (hasMouhitotsu)
                    {
                        // 「もうひとつ」が含まれている場合：クリッカブルワード付きタイプライター効果
                        typewriterEffectManager.StartTypewriterEffectWithClickableWord(resultContainer, resultText, () =>
                        {
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
                                        // wordGetLabelのテキストを設定
                                        if (wordGetLabel != null)
                                        {
                                            wordGetLabel.ClearClassList();
                                            if (isDarkMode)
                                            {
                                                wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                                                wordGetLabel.AddToClassList("word-get-dark");
                                            }
                                            else if (wordFoundInCurrentScenario)
                                            {
                                                wordGetLabel.text = "✨ 【もうひとつ】ワードゲット! ✨";
                                                wordGetLabel.AddToClassList("word-get-success");
                                            }
                                            else
                                            {
                                                wordGetLabel.text = "残念...【もうひとつ】は出ませんでした";
                                                wordGetLabel.AddToClassList("word-get-failed");
                                            }
                                        }
                                        
                                        if (wordFoundInCurrentScenario && epilogueContainer != null)
                                        {
                                            epilogueContainer.style.display = DisplayStyle.Flex;
                                            if (epilogueLabel != null && !string.IsNullOrEmpty(epilogueText) && typewriterEffectManager != null)
                                            {
                                                typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText, () =>
                                                {
                                                    ShowBackButton();
                                                });
                                            }
                                            else
                                            {
                                                ShowBackButton();
                                            }
                                        }
                                        else
                                        {
                                            ShowBackButton();
                                        }
                                    },
                                    ShowBackButton
                                );
                            }
                        }, (found) => {
                        if (found)
                        {
                            wordFoundInCurrentScenario = true;
                            
                            // 効果音を再生
                            PlayWordGetSound();
                            
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
                            
                            // メッセージを表示
                            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
                            if (wordFoundMessageLabel != null)
                            {
                                wordFoundMessageLabel.text = "あなたは何かをみつけた気がした";
                                wordFoundMessageLabel.style.display = DisplayStyle.Flex;
                                StartCoroutine(ShakeAnimation(wordFoundMessageLabel));
                            }
                            
                            // ワードゲット表示を表示
                            if (wordGetContainer != null)
                            {
                                wordGetContainer.style.display = DisplayStyle.Flex;
                            }
                            
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
                                    wordGetLabel.text = "✨ 【もうひとつ】ワードゲット! ✨";
                                    wordGetLabel.AddToClassList("word-get-success");
                                }
                            }
                            
                            // HandleChoiceを再度呼び出して、hasWordをtrueに更新
                            if (scenario != null && result != null)
                            {
                                gameManager.HandleChoice(result.choiceId, true);
                                // resultを再取得
                                result = gameManager.GetScenarioResult(scenario.id);
                                // 後日談テキストを再取得
                                if (result != null && !isDarkMode)
                                {
                                    epilogueText = result.epilogue;
                                }
                            }
                            
                            // 後日談を表示
                            if (epilogueContainer != null)
                            {
                                epilogueContainer.style.display = DisplayStyle.Flex;
                                
                                // 後日談のタイプライター効果を開始
                                if (epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
                                {
                                    if (typewriterEffectManager != null)
                                    {
                                        typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText, () =>
                                        {
                                            // 後日談のタイプライター効果が完了したら戻るボタンを表示
                                            ShowBackButton();
                                        });
                                    }
                                    else
                                    {
                                        ShowBackButton();
                                    }
                                }
                                else
                                {
                                    ShowBackButton();
                                }
                            }
                            else
                            {
                                ShowBackButton();
                            }
                        }
                    });
                    }
                    else
                    {
                        // 「もうひとつ」が含まれていない場合：通常のタイプライター効果
                        var resultLabelForTypewriter = new Label();
                        resultLabelForTypewriter.style.fontSize = 18;
                        resultLabelForTypewriter.style.whiteSpace = WhiteSpace.Normal;
                        resultLabelForTypewriter.style.maxWidth = 800;
                        resultLabelForTypewriter.style.marginBottom = 20;
                        resultContainer.Add(resultLabelForTypewriter);
                        
                        typewriterEffectManager.StartTypewriterEffect(resultLabelForTypewriter, resultText, () =>
                        {
                            // タイプライター効果が完了したら、即座に戻るボタンを表示
                            ShowBackButton();
                        });
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
            // ここでは初期化のみ（クラスをクリア）
            if (wordGetLabel != null)
            {
                wordGetLabel.ClearClassList();
                wordGetLabel.text = ""; // テキストは後で設定
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
            }

            // 戻るボタン（最初は非表示）
            var backButton = root.Q<Button>("BackToSelectionButton");
            if (backButton != null)
            {
                backButton.style.display = DisplayStyle.None;
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        private void HideAllScreens()
        {
            if (selectionScreenDocument != null) selectionScreenDocument.gameObject.SetActive(false);
            if (scenarioScreenDocument != null) scenarioScreenDocument.gameObject.SetActive(false);
            if (resultScreenDocument != null) resultScreenDocument.gameObject.SetActive(false);
            if (profileScreenDocument != null) profileScreenDocument.gameObject.SetActive(false);
            if (creditsScreenDocument != null) creditsScreenDocument.gameObject.SetActive(false);
            if (achievementsScreenDocument != null) achievementsScreenDocument.gameObject.SetActive(false);
        }

        private void UpdateScoreDisplay()
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) return;

            var scoreLabel = currentDocument.rootVisualElement.Q<Label>("ScoreText");
            if (scoreLabel != null && gameManager != null)
            {
                int score = gameManager.GetScore();
                int totalScenarios = gameManager.GetScenarios().Count;
                scoreLabel.text = $"【もうひとつ】ワードゲット数: {score} / {totalScenarios}";
                
                // 異常なスコアの場合はスタイルを適用
                scoreLabel.ClearClassList();
                if (score > totalScenarios)
                {
                    scoreLabel.AddToClassList("score-text-anomaly");
                }
                else
                {
                    scoreLabel.AddToClassList("score-text");
                }
            }
            
            // 選択画面のスコアも更新
            if (selectionScreenDocument != null && selectionScreenDocument.gameObject.activeSelf)
            {
                var root = selectionScreenDocument.rootVisualElement;
                if (root != null)
                {
                    var selectionScoreLabel = root.Q<Label>("ScoreText");
                    if (selectionScoreLabel != null && gameManager != null)
                    {
                        int score = gameManager.GetScore();
                        int totalScenarios = gameManager.GetScenarios().Count;
                        selectionScoreLabel.text = $"【もうひとつ】ワードゲット数: {score} / {totalScenarios}";
                        
                        // 異常なスコアの場合はスタイルを適用
                        selectionScoreLabel.ClearClassList();
                        if (score > totalScenarios)
                        {
                            selectionScoreLabel.AddToClassList("score-text-anomaly");
                        }
                        else
                        {
                            selectionScoreLabel.AddToClassList("score-text");
                        }
                    }
                }
            }
        }

        private void CreateScenarioButtons(VisualElement root)
        {
            var buttonContainer = root.Q<VisualElement>("ScenarioButtonContainer");
            if (buttonContainer == null) return;

            // 既存のボタンを削除
            buttonContainer.Clear();

            var scenarios = gameManager.GetScenarios();
            foreach (var scenario in scenarios)
            {
                // シナリオ6は最初の5つをクリアするまで表示しない
                if (scenario.id == 6 && !gameManager.CanAccessScenario(6))
                {
                    continue;
                }

                // ボタンを作成
                Button button = new Button();
                
                // グリッド用のスタイルを適用
                button.AddToClassList("scenario-button");
                
                // ボタンの内容を構造化
                var buttonContent = new VisualElement();
                buttonContent.style.flexDirection = FlexDirection.Column;
                buttonContent.style.alignItems = Align.FlexStart;
                buttonContent.style.width = Length.Percent(100);
                
                var titleLabel = new Label(scenario.title);
                titleLabel.style.fontSize = 20;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.whiteSpace = WhiteSpace.Normal;
                titleLabel.style.marginBottom = 5;
                buttonContent.Add(titleLabel);
                
                // シナリオの説明を追加（2行まで）
                var descriptionLabel = new Label(scenario.setup);
                descriptionLabel.style.fontSize = 14;
                descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
                descriptionLabel.style.opacity = 0.9f;
                descriptionLabel.style.maxHeight = 40; // 2行分の高さに制限
                descriptionLabel.style.overflow = Overflow.Hidden;
                buttonContent.Add(descriptionLabel);
                
                button.Add(buttonContent);
                
                bool isCompleted = gameManager.IsScenarioCompleted(scenario.id);
                bool isLocked = !gameManager.CanAccessScenario(scenario.id);

                if (isLocked)
                {
                    button.SetEnabled(false);
                    var lockLabel = new Label("🔒 ロック");
                    lockLabel.style.fontSize = 12;
                    lockLabel.style.marginTop = 5;
                    buttonContent.Add(lockLabel);
                    button.AddToClassList("scenario-button-locked");
                }
                else if (isCompleted)
                {
                    button.AddToClassList("scenario-button-completed");
                    // 完了マークを追加
                    var completedMark = new Label("✓");
                    completedMark.style.fontSize = 16;
                    completedMark.style.position = Position.Absolute;
                    completedMark.style.top = 5;
                    completedMark.style.right = 5;
                    button.Add(completedMark);
                }
                else
                {
                    button.AddToClassList("scenario-button-normal");
                }

                int scenarioId = scenario.id;
                button.clicked += () => OnScenarioSelected(scenarioId);

                buttonContainer.Add(button);
            }
        }

        private void OnScenarioSelected(int scenarioId)
        {
            gameManager.SetCurrentScenario(scenarioId);
            ShowScenarioScreen();
        }

        private void CreateChoiceButtons(VisualElement root, Scenario scenario)
        {
            var buttonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            if (buttonContainer == null) return;

            // 既存のボタンを削除
            buttonContainer.Clear();

            bool isDarkMode = gameManager.IsDarkMode() && scenario.id == 6;
            var choices = isDarkMode 
                ? new List<Choice> 
                { 
                    new Choice { id = 1, text = "「すみません...」と謝る", preview = "私：「壊してしまって..." },
                    new Choice { id = 2, text = "「これは何ですか？」と問う", preview = "私：「この世界は..." }
                }
                : scenario.choices;

            foreach (var choice in choices)
            {
                // ボタンを作成
                Button button = new Button();
                
                // ダークモードの場合はダークスタイルを適用
                if (isDarkMode)
                {
                    button.AddToClassList("choice-button-dark");
                }
                else
                {
                    button.AddToClassList("choice-button");
                }
                
                // ボタンの中にテキストを配置
                var buttonText = new Label($"選択肢{choice.id}：{choice.text}");
                buttonText.style.fontSize = 18;
                buttonText.style.whiteSpace = WhiteSpace.Normal;
                buttonText.style.unityFontStyleAndWeight = FontStyle.Bold;
                
                var previewText = new Label(choice.preview);
                previewText.style.fontSize = 14;
                previewText.style.opacity = 0.8f;
                previewText.style.whiteSpace = WhiteSpace.Normal;

                var buttonContent = new VisualElement();
                buttonContent.style.flexDirection = FlexDirection.Column;
                buttonContent.style.alignItems = Align.FlexStart;
                buttonContent.Add(buttonText);
                buttonContent.Add(previewText);
                button.Add(buttonContent);

                int choiceId = choice.id;
                button.clicked += () => OnChoiceSelected(choiceId);

                buttonContainer.Add(button);
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromScenario");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }
        }

        private void OnChoiceSelected(int choiceId)
        {
            // wordFoundInCurrentScenarioフラグをhasWordとして使用
            gameManager.HandleChoice(choiceId, wordFoundInCurrentScenario);
            ShowResultScreen();
        }


        private void ToggleEpilogue2(int scenarioId)
        {
            if (profileScreenManager != null)
            {
                profileScreenManager.ToggleEpilogue2(scenarioId);
                ShowProfileScreen(); // 再生成
            }
        }

        private string GetScenarioTitle(int scenarioId)
        {
            var scenarios = gameManager.GetScenarios();
            var scenario = scenarios.Find(s => s.id == scenarioId);
            return scenario != null ? scenario.title : "";
        }

        private string GetDarkModeEpilogue(int scenarioId, int choiceId)
        {
            return "【データ破損】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "のデータは完全に崩壊しました。";
        }

        private string GetDarkModeEpilogue2(int scenarioId)
        {
            return "【完全崩壊】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "は完全にデータの欠片となって消えました。";
        }

        private void SetBackgroundImage(int scenarioId, bool isScenarioScreen)
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) return;

            int backgroundIndex = scenarioId - 1;
            
            if (backgroundIndex >= 0 && backgroundIndex < scenarioBackgrounds.Length && scenarioBackgrounds[backgroundIndex] != null)
            {
                var backgroundImage = currentDocument.rootVisualElement.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(scenarioBackgrounds[backgroundIndex]);
                }
            }
        }

        public void ShowAchievementsScreen()
        {
            HideAllScreens();
            
            if (achievementsScreenDocument == null)
            {
                Debug.LogError("AchievementsScreenDocumentがアサインされていません！");
                return;
            }

            achievementsScreenDocument.gameObject.SetActive(true);
            currentDocument = achievementsScreenDocument;
            
            var root = achievementsScreenDocument.rootVisualElement;
            if (root == null) return;

            // 背景画像を設定（選択画面と同じ背景を使用）
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                }
            }

            var achievementsContainer = root.Q<VisualElement>("AchievementsContainer");
            if (achievementsContainer == null) return;

            if (achievementsScreenManager != null)
            {
                achievementsScreenManager.CreateAchievements(achievementsContainer);
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromAchievements");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }


        public void ShowCreditsScreen()
        {
            HideAllScreens();
            
            if (creditsScreenDocument == null)
            {
                Debug.LogError("CreditsScreenDocumentがアサインされていません！");
                return;
            }

            creditsScreenDocument.gameObject.SetActive(true);
            currentDocument = creditsScreenDocument;
            
            var root = creditsScreenDocument.rootVisualElement;
            if (root == null) return;

            // 背景画像を設定（選択画面と同じ背景を使用）
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                }
            }

            var creditsContent = root.Q<VisualElement>("CreditsContent");
            if (creditsContent == null) return;

            if (creditsScreenManager != null)
            {
                creditsScreenManager.CreateCredits(creditsContent);
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromCredits");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }


        /// <summary>
        /// 戻るボタンを表示
        /// </summary>
        private void ShowBackButton()
        {
            VisualElement root = null;
            if (resultScreenDocument != null && resultScreenDocument.gameObject.activeSelf)
            {
                root = resultScreenDocument.rootVisualElement;
            }
            
            if (root != null)
            {
                var backButton = root.Q<Button>("BackToSelectionButton");
                if (backButton != null)
                {
                    backButton.style.display = DisplayStyle.Flex;
                }
            }
        }

        /// <summary>
        /// シェイクアニメーション
        /// </summary>
        private IEnumerator ShakeAnimation(Label label)
        {
            float duration = 0.5f;
            float shakeIntensity = 10f;
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

        /// <summary>
        /// 「もうひとつ」をゲットした時の効果音を再生（複数からランダムに選択）
        /// </summary>
        private void PlayWordGetSound()
        {
            if (wordGetSounds != null && wordGetSounds.Length > 0 && audioSource != null)
            {
                // 配列からランダムに1つ選択
                int randomIndex = Random.Range(0, wordGetSounds.Length);
                AudioClip selectedSound = wordGetSounds[randomIndex];
                
                if (selectedSound != null)
                {
                    audioSource.PlayOneShot(selectedSound);
                }
            }
        }

    }
}

