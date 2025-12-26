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

        [Header("UXML Files")]
        [SerializeField] private VisualTreeAsset selectionScreenUXML;
        [SerializeField] private VisualTreeAsset scenarioScreenUXML;
        [SerializeField] private VisualTreeAsset resultScreenUXML;
        [SerializeField] private VisualTreeAsset profileScreenUXML;

        [Header("Background Images")]
        [SerializeField] private Sprite[] scenarioBackgrounds = new Sprite[6];
        [SerializeField] private Sprite selectionScreenBackground;
        [SerializeField] private Sprite profileScreenBackground;

        private GameManager gameManager;
        private UIDocument currentDocument;
        private List<GameObject> currentButtons = new List<GameObject>();
        private HashSet<int> expandedProfiles = new HashSet<int>();

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
            }

            // プロフィールボタンの設定
            var showProfileButton = root.Q<Button>("ShowProfileButton");
            if (showProfileButton != null)
            {
                showProfileButton.clicked += ShowProfileScreen;
            }

            UpdateScoreDisplay();
            CreateScenarioButtons(root);
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
                titleLabel.style.color = isDarkMode ? Color.red : Color.black;
            }

            CreateProfileCards(root);
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
            }

            var setupLabel = root.Q<Label>("SetupText");
            if (setupLabel != null)
            {
                setupLabel.text = scenario.setup;
            }

            CreateChoiceButtons(root, scenario);
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

            // 結果テキストを設定
            var resultLabel = root.Q<Label>("ResultText");
            if (resultLabel != null)
            {
                if (isDarkMode)
                {
                    resultLabel.text = result.choiceId == 1
                        ? "私：「すみません...壊してしまって...」\n\n壊れた声：「謝っても...もう遅い...」\n世界が歪み始める。\n\n壊れた声：「この世界は...シミュレーションだった...」\n「あなたの異常な行動が...世界を破壊した...」\n「もう...修復できない...」\n\n画面が歪み、文字が崩れていく。\nあなたは、自分が何をしてしまったのか理解した。"
                        : "私：「この世界は...何ですか？」\n\n壊れた声：「シミュレーション...すべてが...」\n「あなたは...バグを起こした...」\n「世界の整合性が...崩壊している...」\n\n周囲の空間が歪み、現実が崩れていく。\n登場人物たちの姿が、データの欠片となって消えていく。\n\n壊れた声：「もう...戻れない...」\n「あなたは...世界を壊した...」";
                }
                else
                {
                    resultLabel.text = scenario.branches[result.choiceId].text;
                }
            }

            // ワードゲット表示
            var wordGetLabel = root.Q<Label>("WordGetText");
            if (wordGetLabel != null)
            {
                if (isDarkMode)
                {
                    wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                    wordGetLabel.style.color = Color.red;
                }
                else if (result.hasWord)
                {
                    wordGetLabel.text = "✨ 【もうひとつ】ワードゲット! ✨";
                    wordGetLabel.style.color = Color.green;
                }
                else
                {
                    wordGetLabel.text = "残念...【もうひとつ】は出ませんでした";
                    wordGetLabel.style.color = Color.red;
                }
            }

            // 後日談を設定
            var epilogueLabel = root.Q<Label>("EpilogueText");
            if (epilogueLabel != null)
            {
                if (isDarkMode)
                {
                    epilogueLabel.text = result.choiceId == 1
                        ? "世界は完全に崩壊しました。\nシミュレーションの整合性は失われ、修復不可能な状態です。\n\n登場人物たちは、データの欠片となって消えていきました。\nもも子、うみ、ひろ、とおる、つばさ...\nすべてが、あなたの異常な行動の結果です。\n\nあなたは、空っぽの世界に一人取り残されました。\n「もう...戻れない...」\n\n【エンド：世界崩壊】"
                        : "あなたは、世界の真実を知ってしまいました。\nこの世界は、シミュレーションだったのです。\n\nしかし、あなたの異常な行動が、世界を破壊してしまいました。\n登場人物たちは、バグによって歪んだ姿となっています。\n\nもも子は「も」という文字を失い、\nうみは「う」という文字を失い、\nひろは「ひ」という文字を失い、\nとおるは「と」という文字を失い、\nつばさは「つ」という文字を失いました。\n\n「もうひとつ」という言葉は、永遠に失われました。\n\n【エンド：言葉の消失】";
                }
                else
                {
                    epilogueLabel.text = result.epilogue;
                }
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButton");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }
        }

        private void HideAllScreens()
        {
            if (selectionScreenDocument != null) selectionScreenDocument.gameObject.SetActive(false);
            if (scenarioScreenDocument != null) scenarioScreenDocument.gameObject.SetActive(false);
            if (resultScreenDocument != null) resultScreenDocument.gameObject.SetActive(false);
            if (profileScreenDocument != null) profileScreenDocument.gameObject.SetActive(false);
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
                button.text = scenario.title;
                
                bool isCompleted = gameManager.IsScenarioCompleted(scenario.id);
                bool isLocked = !gameManager.CanAccessScenario(scenario.id);

                if (isLocked)
                {
                    button.SetEnabled(false);
                    button.text += " (🔒 ロック)";
                }
                else if (isCompleted)
                {
                    button.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
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
                
                // ボタンの中にテキストを配置
                var buttonText = new Label($"選択肢{choice.id}：{choice.text}");
                buttonText.style.fontSize = 18;
                buttonText.style.whiteSpace = WhiteSpace.Normal;
                
                var previewText = new Label(choice.preview);
                previewText.style.fontSize = 14;
                previewText.style.opacity = 0.8f;
                previewText.style.whiteSpace = WhiteSpace.Normal;

                var buttonContent = new VisualElement();
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
            var profileContainer = root.Q<VisualElement>("ProfileContainer");
            if (profileContainer == null) return;

            // 既存のカードを削除
            profileContainer.Clear();

            var scenarios = gameManager.GetScenarios();
            bool isDarkMode = gameManager.IsDarkMode();
            bool scenario6Completed = gameManager.IsScenarioCompleted(6);

            // シナリオ1-5のプロフィール
            for (int i = 1; i <= 5; i++)
            {
                var profile = CharacterProfileManager.GetProfile(i);
                if (profile == null) continue;

                var result = gameManager.GetScenarioResult(i);
                bool isUnlocked = result != null;

                CreateProfileCard(profileContainer, profile, result, isUnlocked, isDarkMode, scenario6Completed);
            }

            // シナリオ6のプロフィール（クリア後のみ表示）
            if (scenario6Completed)
            {
                var profile = CharacterProfileManager.GetProfile(6);
                if (profile != null)
                {
                    var result = gameManager.GetScenarioResult(6);
                    CreateProfileCard(profileContainer, profile, result, true, isDarkMode, scenario6Completed);
                }
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromProfile");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }
        }

        private void CreateProfileCard(VisualElement container, CharacterProfile profile, ScenarioResult result, bool isUnlocked, bool isDarkMode, bool scenario6Completed)
        {
            // プロフィールカードを作成
            var card = new VisualElement();
            card.AddToClassList("profile-card");
            
            if (isUnlocked)
            {
                card.style.backgroundColor = profile.profileColor;
            }
            else
            {
                card.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            }

            // 名前
            var nameLabel = new Label(isUnlocked ? $"{profile.name}（{profile.role}）" : $"???（{profile.role}）");
            nameLabel.AddToClassList("profile-name");
            card.Add(nameLabel);

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
                card.Add(infoLabel);

                // セリフ
                if (!string.IsNullOrEmpty(profile.quote) || !string.IsNullOrEmpty(profile.quoteDarkMode))
                {
                    var quoteLabel = new Label(isDarkMode ? profile.quoteDarkMode : profile.quote);
                    quoteLabel.AddToClassList("profile-quote");
                    quoteLabel.style.color = isDarkMode ? Color.red : profile.borderColor;
                    card.Add(quoteLabel);
                }

                // 後日談
                if (result != null)
                {
                    var epilogueLabel = new Label(isDarkMode ? GetDarkModeEpilogue(profile.scenarioId, result.choiceId) : result.epilogue);
                    epilogueLabel.AddToClassList("profile-epilogue");
                    card.Add(epilogueLabel);

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
                            card.Add(expandButton);

                            if (isExpanded)
                            {
                                var epilogue2Label = new Label(isDarkMode ? GetDarkModeEpilogue2(profile.scenarioId) : scenario.branches[result.choiceId].epilogue2);
                                epilogue2Label.AddToClassList("profile-epilogue2");
                                card.Add(epilogue2Label);
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
                            card.Add(hintLabel);
                        }
                    }
                }
            }
            else
            {
                var lockedLabel = new Label($"シナリオ「{GetScenarioTitle(profile.scenarioId)}」をクリアすると表示されます");
                lockedLabel.AddToClassList("profile-locked");
                card.Add(lockedLabel);
            }

            container.Add(card);
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
    }
}

