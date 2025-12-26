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

        private GameManager gameManager;
        private UIDocument currentDocument;
        private List<GameObject> currentButtons = new List<GameObject>();
        private HashSet<int> expandedProfiles = new HashSet<int>();
        private int selectedProfileId = 1; // 選択中のプロフィールID
        private Coroutine currentTransition; // 現在実行中のトランジション
        private Coroutine currentTypewriterEffect; // 現在実行中のタイプライター効果

        private void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManagerが見つかりません！");
                return;
            }

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
            StartScreenTransition(root);
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

            CreateProfileCards(root);
            
            // トランジション開始
            if (root != null)
            {
                StartScreenTransition(root);
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

            var setupLabel = root.Q<Label>("SetupText");
            var choiceButtonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            
            // 選択肢ボタンコンテナを最初は非表示にする
            if (choiceButtonContainer != null)
            {
                choiceButtonContainer.style.display = DisplayStyle.None;
            }
            
            if (setupLabel != null)
            {
                // タイプライター効果で表示（完了後に選択肢ボタンを表示）
                StartTypewriterEffect(setupLabel, scenario.setup, () =>
                {
                    // タイプライター効果が完了したら選択肢ボタンを表示
                    if (choiceButtonContainer != null)
                    {
                        choiceButtonContainer.style.display = DisplayStyle.Flex;
                    }
                });
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
            StartScreenTransition(root, withScale: true);
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
                
                // タイプライター効果で表示（完了後に後日談を表示）
                StartTypewriterEffect(resultLabel, resultText, () =>
                {
                    // 結果テキストのタイプライター効果が完了したら後日談を表示
                    if (epilogueContainer != null)
                    {
                        epilogueContainer.style.display = DisplayStyle.Flex;
                    }
                    
                    // 後日談のタイプライター効果を開始
                    if (epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
                    {
                        StartTypewriterEffect(epilogueLabel, epilogueText);
                    }
                });
            }
            else if (epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
            {
                // 結果テキストがない場合は即座に後日談を表示
                if (epilogueContainer != null)
                {
                    epilogueContainer.style.display = DisplayStyle.Flex;
                }
                StartTypewriterEffect(epilogueLabel, epilogueText);
            }

            // ワードゲット表示
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            var wordGetLabel = root.Q<Label>("WordGetText");
            if (wordGetLabel != null)
            {
                // 既存のクラスをクリア
                wordGetLabel.ClearClassList();
                
                if (isDarkMode)
                {
                    wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                    wordGetLabel.AddToClassList("word-get-dark");
                }
                else if (result.hasWord)
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

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButton");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            StartScreenTransition(root);
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
            gameManager.HandleChoice(choiceId);
            ShowResultScreen();
        }

        private void CreateProfileCards(VisualElement root)
        {
            var profileList = root.Q<VisualElement>("ProfileList");
            var profileDetail = root.Q<VisualElement>("ProfileDetail");
            
            if (profileList == null || profileDetail == null) return;

            // 既存の要素を削除
            profileList.Clear();
            profileDetail.Clear();

            var scenarios = gameManager.GetScenarios();
            bool isDarkMode = gameManager.IsDarkMode();
            bool scenario6Completed = gameManager.IsScenarioCompleted(6);

            // 利用可能なプロフィールIDのリストを作成
            List<int> availableProfileIds = new List<int>();
            
            // シナリオ1-5のプロフィール
            for (int i = 1; i <= 5; i++)
            {
                var profile = CharacterProfileManager.GetProfile(i);
                if (profile != null)
                {
                    availableProfileIds.Add(i);
                }
            }

            // シナリオ6のプロフィール（クリア後のみ表示）
            if (scenario6Completed)
            {
                var profile = CharacterProfileManager.GetProfile(6);
                if (profile != null)
                {
                    availableProfileIds.Add(6);
                }
            }

            // 選択中のプロフィールが利用可能でない場合、最初の利用可能なものを選択
            if (!availableProfileIds.Contains(selectedProfileId) && availableProfileIds.Count > 0)
            {
                selectedProfileId = availableProfileIds[0];
            }

            // 左側にプロフィールリストを作成
            foreach (int profileId in availableProfileIds)
            {
                var profile = CharacterProfileManager.GetProfile(profileId);
                if (profile == null) continue;

                var result = gameManager.GetScenarioResult(profileId);
                bool isUnlocked = result != null;

                // リストボタンを作成
                Button listButton = new Button();
                listButton.AddToClassList("profile-list-button");
                
                // ボタンの中身を構造化
                var buttonContent = new VisualElement();
                buttonContent.style.flexDirection = FlexDirection.Column;
                buttonContent.style.alignItems = Align.FlexStart;
                
                var nameLabel = new Label(isUnlocked ? profile.name : "???");
                nameLabel.style.fontSize = 16;
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.marginBottom = 4;
                
                var roleLabel = new Label($"({profile.role})");
                roleLabel.style.fontSize = 12;
                roleLabel.style.opacity = 0.8f;
                
                buttonContent.Add(nameLabel);
                buttonContent.Add(roleLabel);
                listButton.Add(buttonContent);
                
                if (!isUnlocked)
                {
                    listButton.AddToClassList("locked");
                }
                
                if (profileId == selectedProfileId && isUnlocked)
                {
                    listButton.AddToClassList("active");
                }

                int currentProfileId = profileId;
                listButton.clicked += () => {
                    if (isUnlocked)
                    {
                        selectedProfileId = currentProfileId;
                        ShowProfileScreen(); // 再生成して詳細を更新
                    }
                };

                profileList.Add(listButton);
            }

            // 右側に選択中のプロフィール詳細を表示
            if (selectedProfileId > 0)
            {
                var selectedProfile = CharacterProfileManager.GetProfile(selectedProfileId);
                if (selectedProfile != null)
                {
                    var result = gameManager.GetScenarioResult(selectedProfileId);
                    bool isUnlocked = result != null;
                    
                    CreateProfileDetail(profileDetail, selectedProfile, result, isUnlocked, isDarkMode, scenario6Completed);
                }
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromProfile");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }
        }

        private void CreateProfileDetail(VisualElement container, CharacterProfile profile, ScenarioResult result, bool isUnlocked, bool isDarkMode, bool scenario6Completed)
        {
            // プロフィール詳細コンテナを作成
            var detailCard = new VisualElement();
            
            // キャラクターごとの色分けクラスを追加（index.htmlのスタイルに合わせる）
            if (isUnlocked)
            {
                switch (profile.scenarioId)
                {
                    case 1:
                        detailCard.AddToClassList("profile-card-momo");
                        break;
                    case 2:
                        detailCard.AddToClassList("profile-card-umi");
                        break;
                    case 3:
                        detailCard.AddToClassList("profile-card-hiro");
                        break;
                    case 4:
                        detailCard.AddToClassList("profile-card-toru");
                        break;
                    case 5:
                        detailCard.AddToClassList("profile-card-tsubasa");
                        break;
                    case 6:
                        detailCard.AddToClassList("profile-card-voice");
                        break;
                }
                detailCard.style.backgroundColor = profile.profileColor;
            }
            else
            {
                detailCard.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            }
            
            detailCard.style.paddingTop = 20;
            detailCard.style.paddingBottom = 20;
            detailCard.style.paddingLeft = 20;
            detailCard.style.paddingRight = 20;
            detailCard.style.width = Length.Percent(100);
            detailCard.style.maxWidth = Length.Percent(100);
            detailCard.style.minWidth = 0;
            
            // ボーダー半径を各角に設定
            detailCard.style.borderTopLeftRadius = 8;
            detailCard.style.borderTopRightRadius = 8;
            detailCard.style.borderBottomLeftRadius = 8;
            detailCard.style.borderBottomRightRadius = 8;
            
            // ボーダー幅を各方向に設定
            var borderColor = isUnlocked ? profile.borderColor : new Color(0.2f, 0.2f, 0.2f);
            detailCard.style.borderTopWidth = 2;
            detailCard.style.borderRightWidth = 2;
            detailCard.style.borderBottomWidth = 2;
            detailCard.style.borderLeftWidth = 2;
            
            // ボーダー色を各方向に設定
            detailCard.style.borderTopColor = borderColor;
            detailCard.style.borderRightColor = borderColor;
            detailCard.style.borderBottomColor = borderColor;
            detailCard.style.borderLeftColor = borderColor;

            // 名前
            var nameLabel = new Label(isUnlocked ? $"{profile.name}（{profile.role}）" : $"???（{profile.role}）");
            nameLabel.AddToClassList("profile-name");
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            nameLabel.style.maxWidth = Length.Percent(100);
            detailCard.Add(nameLabel);

            if (isUnlocked)
            {
                // 情報
                var infoLabel = new Label();
                string info = $"職業: {(isDarkMode ? "【データ欠損】" : profile.job)}\n";
                info += $"特徴: {(isDarkMode ? profile.featureDarkMode : profile.feature)}";
                
                if (scenario6Completed && !isDarkMode && !string.IsNullOrEmpty(profile.relationshipWithVoice))
                {
                    info += $"\n\n謎の声との関係: {profile.relationshipWithVoice}";
                }
                else if (scenario6Completed && isDarkMode && !string.IsNullOrEmpty(profile.bugDescription))
                {
                    info += $"\n\n【バグ】: {profile.bugDescription}";
                }
                
                infoLabel.text = info;
                infoLabel.AddToClassList("profile-info");
                infoLabel.style.whiteSpace = WhiteSpace.Normal;
                infoLabel.style.maxWidth = Length.Percent(100);
                detailCard.Add(infoLabel);

                // セリフ
                if (!string.IsNullOrEmpty(profile.quote) || !string.IsNullOrEmpty(profile.quoteDarkMode))
                {
                    var quoteLabel = new Label(isDarkMode ? profile.quoteDarkMode : profile.quote);
                    quoteLabel.AddToClassList("profile-quote");
                    quoteLabel.style.color = isDarkMode ? Color.red : profile.borderColor;
                    quoteLabel.style.whiteSpace = WhiteSpace.Normal;
                    quoteLabel.style.maxWidth = Length.Percent(100);
                    detailCard.Add(quoteLabel);
                }

                // 後日談
                if (result != null)
                {
                    var epilogueLabel = new Label(isDarkMode ? GetDarkModeEpilogue(profile.scenarioId, result.choiceId) : result.epilogue);
                    epilogueLabel.AddToClassList("profile-epilogue");
                    epilogueLabel.style.whiteSpace = WhiteSpace.Normal;
                    epilogueLabel.style.maxWidth = Length.Percent(100);
                    detailCard.Add(epilogueLabel);

                    // 後日談の後日談
                    if (result.hasWord && profile.scenarioId <= 5)
                    {
                        var scenario = gameManager.GetScenarios().Find(s => s.id == profile.scenarioId);
                        if (scenario != null && scenario.branches.ContainsKey(result.choiceId) && 
                            !string.IsNullOrEmpty(scenario.branches[result.choiceId].epilogue2))
                        {
                            bool isExpanded = expandedProfiles.Contains(profile.scenarioId);
                            
                            var expandButton = new Button();
                            expandButton.text = isExpanded ? "▼ 後日談の後日談を隠す" : "▶ 後日談の後日談を見る";
                            expandButton.clicked += () => ToggleEpilogue2(profile.scenarioId);
                            detailCard.Add(expandButton);

                            if (isExpanded)
                            {
                                var epilogue2Label = new Label(isDarkMode ? GetDarkModeEpilogue2(profile.scenarioId) : scenario.branches[result.choiceId].epilogue2);
                                epilogue2Label.AddToClassList("profile-epilogue2");
                                epilogue2Label.style.whiteSpace = WhiteSpace.Normal;
                                epilogue2Label.style.maxWidth = Length.Percent(100);
                                detailCard.Add(epilogue2Label);
                            }
                        }
                    }

                    // ヒント
                    if (!result.hasWord && profile.scenarioId <= 5)
                    {
                        var scenario = gameManager.GetScenarios().Find(s => s.id == profile.scenarioId);
                        if (scenario != null && scenario.branches.ContainsKey(result.choiceId) && 
                            !string.IsNullOrEmpty(scenario.branches[result.choiceId].hint))
                        {
                            var hintLabel = new Label(scenario.branches[result.choiceId].hint);
                            hintLabel.AddToClassList("profile-hint");
                            hintLabel.style.whiteSpace = WhiteSpace.Normal;
                            hintLabel.style.maxWidth = Length.Percent(100);
                            detailCard.Add(hintLabel);
                        }
                    }
                }
            }
            else
            {
                var lockedLabel = new Label($"シナリオ「{GetScenarioTitle(profile.scenarioId)}」をクリアすると表示されます");
                lockedLabel.AddToClassList("profile-locked");
                detailCard.Add(lockedLabel);
            }

            container.Add(detailCard);
        }
        
        // 旧メソッド（互換性のため残すが、使用しない）
        private void CreateProfileCard(VisualElement container, CharacterProfile profile, ScenarioResult result, bool isUnlocked, bool isDarkMode, bool scenario6Completed)
        {
            CreateProfileDetail(container, profile, result, isUnlocked, isDarkMode, scenario6Completed);
        }

        private void ToggleEpilogue2(int scenarioId)
        {
            if (expandedProfiles.Contains(scenarioId))
            {
                expandedProfiles.Remove(scenarioId);
            }
            else
            {
                expandedProfiles.Add(scenarioId);
            }
            ShowProfileScreen(); // 再生成
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

            achievementsContainer.Clear();

            var scenarios = gameManager.GetScenarios();
            int totalCompleted = 0;
            foreach (var scenario in scenarios)
            {
                if (gameManager.IsScenarioCompleted(scenario.id))
                {
                    totalCompleted++;
                }
            }

            // 全シナリオクリア後のみ表示
            if (totalCompleted < scenarios.Count)
            {
                return;
            }

            var gridContainer = new VisualElement();
            gridContainer.style.flexDirection = FlexDirection.Row;
            gridContainer.style.flexWrap = Wrap.Wrap;
            gridContainer.style.justifyContent = Justify.Center;
            gridContainer.AddToClassList("achievement-grid");
            gridContainer.style.width = Length.Percent(100);

            // シナリオ1-5のエンド
            for (int i = 1; i <= 5; i++)
            {
                var scenario = scenarios.Find(s => s.id == i);
                if (scenario == null) continue;

                var result = gameManager.GetScenarioResult(i);
                var trueChoiceId = scenario.choices.Find(c => scenario.branches.ContainsKey(c.id) && scenario.branches[c.id].hasWord)?.id ?? -1;
                var falseChoiceId = scenario.choices.Find(c => scenario.branches.ContainsKey(c.id) && !scenario.branches[c.id].hasWord)?.id ?? -1;
                
                bool trueEndSeen = result != null && result.hasWord && result.choiceId == trueChoiceId;
                bool falseEndSeen = result != null && !result.hasWord && result.choiceId == falseChoiceId;

                var scenarioCard = CreateAchievementCard(scenario.title, trueEndSeen, falseEndSeen, true);
                gridContainer.Add(scenarioCard);
            }

            // 真実の扉のエンド
            var scenario6 = scenarios.Find(s => s.id == 6);
            if (scenario6 != null)
            {
                var result6 = gameManager.GetScenarioResult(6);
                bool wasDarkMode = result6 != null && result6.scoreAtCompletion > scenarios.Count;
                bool trueEndSeen = result6 != null && result6.hasWord && result6.choiceId == 2 && !wasDarkMode;
                bool falseEndSeen = result6 != null && !result6.hasWord && result6.choiceId == 1 && !wasDarkMode;
                bool darkModeEnd1Seen = result6 != null && wasDarkMode && result6.choiceId == 1;
                bool darkModeEnd2Seen = result6 != null && wasDarkMode && result6.choiceId == 2;

                var scenario6Card = CreateAchievementCardForScenario6(trueEndSeen, falseEndSeen, darkModeEnd1Seen, darkModeEnd2Seen);
                gridContainer.Add(scenario6Card);
            }

            achievementsContainer.Add(gridContainer);

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromAchievements");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            StartScreenTransition(root);
        }

        private VisualElement CreateAchievementCard(string scenarioTitle, bool trueEndSeen, bool falseEndSeen, bool isNormalScenario)
        {
            var card = new VisualElement();
            card.AddToClassList("achievement-card");
            card.style.width = 300;
            card.style.marginBottom = 16;

            var titleLabel = new Label(scenarioTitle);
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 12;
            card.Add(titleLabel);

            var endsContainer = new VisualElement();
            endsContainer.style.flexDirection = FlexDirection.Column;
            endsContainer.AddToClassList("achievement-ends-container");

            // Trueエンド
            var trueEndBox = new VisualElement();
            trueEndBox.AddToClassList(trueEndSeen ? "achievement-end-unlocked" : "achievement-end-locked");
            var trueEndLabel = new Label("✨ Trueエンド");
            trueEndLabel.style.fontSize = 14;
            trueEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            trueEndLabel.style.color = trueEndSeen ? new Color(0.13f, 0.4f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            trueEndBox.Add(trueEndLabel);
            if (trueEndSeen)
            {
                var trueEndDesc = new Label("【もうひとつ】を獲得したエンド");
                trueEndDesc.style.fontSize = 12;
                trueEndDesc.style.marginTop = 4;
                trueEndBox.Add(trueEndDesc);
            }
            endsContainer.Add(trueEndBox);

            // Falseエンド
            var falseEndBox = new VisualElement();
            falseEndBox.AddToClassList(falseEndSeen ? "achievement-end-unlocked-false" : "achievement-end-locked");
            var falseEndLabel = new Label("❌ Falseエンド");
            falseEndLabel.style.fontSize = 14;
            falseEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            falseEndLabel.style.color = falseEndSeen ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.5f, 0.5f, 0.5f);
            falseEndBox.Add(falseEndLabel);
            if (falseEndSeen)
            {
                var falseEndDesc = new Label("【もうひとつ】を獲得できなかったエンド");
                falseEndDesc.style.fontSize = 12;
                falseEndDesc.style.marginTop = 4;
                falseEndBox.Add(falseEndDesc);
            }
            endsContainer.Add(falseEndBox);

            card.Add(endsContainer);
            return card;
        }

        private VisualElement CreateAchievementCardForScenario6(bool trueEndSeen, bool falseEndSeen, bool darkModeEnd1Seen, bool darkModeEnd2Seen)
        {
            var card = new VisualElement();
            card.AddToClassList("achievement-card");
            card.style.width = 300;
            card.style.marginBottom = 16;

            var titleLabel = new Label("真実の扉");
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 12;
            card.Add(titleLabel);

            var endsContainer = new VisualElement();
            endsContainer.style.flexDirection = FlexDirection.Column;
            endsContainer.AddToClassList("achievement-ends-container");

            // Trueエンド
            var trueEndBox = new VisualElement();
            trueEndBox.AddToClassList(trueEndSeen ? "achievement-end-unlocked" : "achievement-end-locked");
            var trueEndLabel = new Label("✨ Trueエンド");
            trueEndLabel.style.fontSize = 14;
            trueEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            trueEndLabel.style.color = trueEndSeen ? new Color(0.13f, 0.4f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            trueEndBox.Add(trueEndLabel);
            if (trueEndSeen)
            {
                var trueEndDesc = new Label("「答えを知りたかった」を選んだエンド");
                trueEndDesc.style.fontSize = 12;
                trueEndDesc.style.marginTop = 4;
                trueEndBox.Add(trueEndDesc);
            }
            endsContainer.Add(trueEndBox);

            // Falseエンド
            var falseEndBox = new VisualElement();
            falseEndBox.AddToClassList(falseEndSeen ? "achievement-end-unlocked-false" : "achievement-end-locked");
            var falseEndLabel = new Label("❌ Falseエンド");
            falseEndLabel.style.fontSize = 14;
            falseEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            falseEndLabel.style.color = falseEndSeen ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.5f, 0.5f, 0.5f);
            falseEndBox.Add(falseEndLabel);
            if (falseEndSeen)
            {
                var falseEndDesc = new Label("「好奇心から」を選んだエンド");
                falseEndDesc.style.fontSize = 12;
                falseEndDesc.style.marginTop = 4;
                falseEndBox.Add(falseEndDesc);
            }
            endsContainer.Add(falseEndBox);

            // ダークエンド1
            var darkEnd1Box = new VisualElement();
            darkEnd1Box.AddToClassList(darkModeEnd1Seen ? "achievement-end-dark" : "achievement-end-locked");
            var darkEnd1Label = new Label("⚠️ ダークエンド1");
            darkEnd1Label.style.fontSize = 14;
            darkEnd1Label.style.unityFontStyleAndWeight = FontStyle.Bold;
            darkEnd1Label.style.color = darkModeEnd1Seen ? new Color(1f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
            darkEnd1Box.Add(darkEnd1Label);
            if (darkModeEnd1Seen)
            {
                var darkEnd1Desc = new Label("「すみません...」と謝ったエンド");
                darkEnd1Desc.style.fontSize = 12;
                darkEnd1Desc.style.marginTop = 4;
                darkEnd1Box.Add(darkEnd1Desc);
            }
            endsContainer.Add(darkEnd1Box);

            // ダークエンド2
            var darkEnd2Box = new VisualElement();
            darkEnd2Box.AddToClassList(darkModeEnd2Seen ? "achievement-end-dark" : "achievement-end-locked");
            var darkEnd2Label = new Label("⚠️ ダークエンド2");
            darkEnd2Label.style.fontSize = 14;
            darkEnd2Label.style.unityFontStyleAndWeight = FontStyle.Bold;
            darkEnd2Label.style.color = darkModeEnd2Seen ? new Color(1f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
            darkEnd2Box.Add(darkEnd2Label);
            if (darkModeEnd2Seen)
            {
                var darkEnd2Desc = new Label("「これは何ですか？」と問うたエンド");
                darkEnd2Desc.style.fontSize = 12;
                darkEnd2Desc.style.marginTop = 4;
                darkEnd2Box.Add(darkEnd2Desc);
            }
            endsContainer.Add(darkEnd2Box);

            card.Add(endsContainer);
            return card;
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

            creditsContent.Clear();

            // クレジット情報を追加
            AddCreditItem(creditsContent, "ゲームデザイン", "tatmos");
            AddCreditItem(creditsContent, "AIディレクション", "tatmos");
            AddCreditItem(creditsContent, "シナリオ", "Claude sonnet 4.5");
            AddCreditItem(creditsContent, "リードプログラマ", "Claude sonnet 4.5");
            AddCreditItem(creditsContent, "プログラマ", "tatmos");
            AddCreditItem(creditsContent, "音楽", "tatmos");
            AddCreditItem(creditsContent, "効果音", "tatmos");
            AddCreditItem(creditsContent, "グラフィック", "Chat GPT 5.2");

            // エンドクレジット楽曲セクション
            var musicSection = new VisualElement();
            musicSection.style.marginTop = 48;
            musicSection.style.paddingTop = 32;
            musicSection.style.borderTopWidth = 1;
            musicSection.style.borderTopColor = new Color(1f, 1f, 1f, 0.3f);
            musicSection.style.width = Length.Percent(100);
            musicSection.style.flexDirection = FlexDirection.Column;
            musicSection.style.alignItems = Align.Center;

            var musicTitle = new Label("エンドクレジット楽曲");
            musicTitle.style.fontSize = 36;
            musicTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            musicTitle.style.marginBottom = 24;
            musicTitle.style.color = new Color(1f, 0.84f, 0f); // yellow-300
            musicSection.Add(musicTitle);

            var songInfo = new Label("曲：「もうひとつ」 / 作曲：suno ai v5 / 作詞：Claude sonnet 4.5");
            songInfo.style.fontSize = 24;
            songInfo.style.unityFontStyleAndWeight = FontStyle.Bold;
            songInfo.style.marginBottom = 16;
            songInfo.style.whiteSpace = WhiteSpace.Normal;
            songInfo.style.maxWidth = Length.Percent(100);
            musicSection.Add(songInfo);

            AddCreditItem(musicSection, "歌", "suno ai v5");
            AddCreditItem(musicSection, "演奏", "suno ai v5");
            AddCreditItem(musicSection, "ミキシング", "suno ai v5");
            AddCreditItem(musicSection, "マスタリング", "suno ai v5");
            AddCreditItem(musicSection, "サウンドエンジニア", "tatmos");

            creditsContent.Add(musicSection);

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromCredits");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            StartScreenTransition(root);
        }

        private void AddCreditItem(VisualElement container, string role, string name)
        {
            var item = new VisualElement();
            item.AddToClassList("credits-content-item");
            item.style.flexDirection = FlexDirection.Column;
            item.style.alignItems = Align.Center;
            item.style.marginBottom = 16;
            item.style.width = Length.Percent(100);

            var roleLabel = new Label(role);
            roleLabel.style.fontSize = 24;
            roleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            roleLabel.style.marginBottom = 8;
            roleLabel.style.color = new Color(1f, 0.84f, 0f); // yellow-300
            roleLabel.style.whiteSpace = WhiteSpace.Normal;
            roleLabel.style.maxWidth = Length.Percent(100);
            item.Add(roleLabel);

            var nameLabel = new Label(name);
            nameLabel.style.fontSize = 20;
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            nameLabel.style.maxWidth = Length.Percent(100);
            item.Add(nameLabel);

            container.Add(item);
        }

        /// <summary>
        /// 画面トランジションを開始（背景は即座に表示、UIコンテンツはフェードイン）
        /// </summary>
        private void StartScreenTransition(VisualElement root, bool withScale = false)
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
        /// タイプライター効果を開始（1行ずつ時間差で、左から文字を表示）
        /// </summary>
        private void StartTypewriterEffect(Label label, string fullText, System.Action onComplete = null)
        {
            // 既存のタイプライター効果を停止
            if (currentTypewriterEffect != null)
            {
                StopCoroutine(currentTypewriterEffect);
            }

            // 初期状態：テキストを空にする
            label.text = "";

            // タイプライター効果開始
            currentTypewriterEffect = StartCoroutine(TypewriterEffectCoroutine(label, fullText, onComplete));
        }

        /// <summary>
        /// 遅延付きタイプライター効果（後日談など、他のテキストの後に表示）
        /// </summary>
        private IEnumerator DelayedTypewriterEffect(Label label, string fullText, float delay)
        {
            yield return new WaitForSeconds(delay);
            StartTypewriterEffect(label, fullText);
        }

        /// <summary>
        /// タイプライター効果コルーチン（1行ずつ時間差で、左から文字を表示）
        /// </summary>
        private IEnumerator TypewriterEffectCoroutine(Label label, string fullText, System.Action onComplete = null)
        {
            // テキストを行ごとに分割
            string[] lines = fullText.Split('\n');
            
            float charDelay = 0.03f; // 1文字あたりの遅延（秒）
            float lineDelay = 0.15f; // 行間の遅延（秒）

            string displayedText = "";

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                
                // 各行を1文字ずつ表示
                for (int charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    // 現在の行までの完全に表示されたテキスト + 現在の行の部分的なテキスト
                    string currentText = displayedText + line.Substring(0, charIndex + 1);
                    
                    label.text = currentText;
                    yield return new WaitForSeconds(charDelay);
                }

                // 行を完全に表示したら、displayedTextに追加
                displayedText += line;
                
                // 最後の行以外は改行を追加
                if (lineIndex < lines.Length - 1)
                {
                    displayedText += "\n";
                    label.text = displayedText; // 改行も表示
                    
                    // 行間の遅延
                    yield return new WaitForSeconds(lineDelay);
                }
            }

            // 最終的なテキストを設定（念のため）
            label.text = fullText;
            
            // 完了コールバックを呼び出し
            onComplete?.Invoke();
            
            currentTypewriterEffect = null;
        }
    }
}

