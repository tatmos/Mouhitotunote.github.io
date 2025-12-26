using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NovelGame
{
    public class UIManager : MonoBehaviour
    {
        [Header("Selection Screen")]
        [SerializeField] private GameObject selectionScreen;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Transform scenarioButtonParent;
        [SerializeField] private GameObject scenarioButtonPrefab;
        [SerializeField] private Button showProfileButton;

        [Header("Scenario Screen")]
        [SerializeField] private GameObject scenarioScreen;
        [SerializeField] private TextMeshProUGUI scenarioTitleText;
        [SerializeField] private TextMeshProUGUI setupText;
        [SerializeField] private Transform choiceButtonParent;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Button backToSelectionButtonFromScenario;

        [Header("Result Screen")]
        [SerializeField] private GameObject resultScreen;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI wordGetText;
        [SerializeField] private TextMeshProUGUI epilogueText;
        [SerializeField] private Button backToSelectionButton;

        [Header("Profile Screen")]
        [SerializeField] private GameObject profileScreen;
        [SerializeField] private Transform profileParent;
        [SerializeField] private GameObject profileCardPrefab;
        [SerializeField] private TextMeshProUGUI profileSectionTitle;
        [SerializeField] private Button backToSelectionButtonFromProfile;

        private GameManager gameManager;
        private List<GameObject> currentButtons = new List<GameObject>();
        private HashSet<int> expandedProfiles = new HashSet<int>();
        private List<GameObject> currentProfileCards = new List<GameObject>();

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
            selectionScreen.SetActive(true);
            scenarioScreen.SetActive(false);
            resultScreen.SetActive(false);
            if (profileScreen != null) profileScreen.SetActive(false);

            // 選択画面全体のレイアウトを設定
            SetupSelectionScreenLayout();

            if (titleText != null)
            {
                titleText.text = "ミニノベルゲーム";
            }

            UpdateScoreDisplay();
            CreateScenarioButtons();

            // プロフィールボタンの設定
            if (showProfileButton != null)
            {
                showProfileButton.onClick.RemoveAllListeners();
                showProfileButton.onClick.AddListener(ShowProfileScreen);
            }
        }

        public void ShowProfileScreen()
        {
            selectionScreen.SetActive(false);
            scenarioScreen.SetActive(false);
            resultScreen.SetActive(false);
            if (profileScreen != null) profileScreen.SetActive(true);

            CreateProfileCards();

            // 選択画面に戻るボタンの設定
            if (backToSelectionButtonFromProfile != null)
            {
                backToSelectionButtonFromProfile.onClick.RemoveAllListeners();
                backToSelectionButtonFromProfile.onClick.AddListener(ShowSelectionScreen);
            }
        }

        private void SetupSelectionScreenLayout()
        {
            if (selectionScreen == null) return;

            // 選択画面のRectTransformを設定
            var selectionScreenRect = selectionScreen.GetComponent<RectTransform>();
            if (selectionScreenRect != null)
            {
                selectionScreenRect.anchorMin = Vector2.zero;
                selectionScreenRect.anchorMax = Vector2.one;
                selectionScreenRect.sizeDelta = Vector2.zero;
                selectionScreenRect.anchoredPosition = Vector2.zero;
            }

            // 選択画面全体にVerticalLayoutGroupを追加
            var verticalLayout = selectionScreen.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = selectionScreen.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            verticalLayout.spacing = 20f;
            verticalLayout.padding = new RectOffset(20, 20, 20, 20);
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.MiddleCenter; // 中央揃えに変更

            // 各要素にLayoutElementを設定
            SetupLayoutElement(titleText?.transform, preferredHeight: 60f);
            SetupLayoutElement(scoreText?.transform, preferredHeight: 30f);
            SetupLayoutElement(scenarioButtonParent, preferredHeight: -1); // 自動調整
            SetupLayoutElement(showProfileButton?.transform, preferredHeight: 50f);
        }

        private void SetupLayoutElement(Transform target, float preferredHeight = -1)
        {
            if (target == null) return;

            var layoutElement = target.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = target.gameObject.AddComponent<LayoutElement>();
            }
            
            if (preferredHeight > 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.flexibleHeight = 0f;
            }
            else
            {
                layoutElement.preferredHeight = -1;
                layoutElement.flexibleHeight = 0f; // VerticalLayoutGroupが自動調整
            }

            // RectTransformも設定
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        private void SetupProfileScreenLayout()
        {
            if (profileScreen == null) return;

            // プロフィール画面のRectTransformを設定
            var profileScreenRect = profileScreen.GetComponent<RectTransform>();
            if (profileScreenRect != null)
            {
                profileScreenRect.anchorMin = Vector2.zero;
                profileScreenRect.anchorMax = Vector2.one;
                profileScreenRect.sizeDelta = Vector2.zero;
                profileScreenRect.anchoredPosition = Vector2.zero;
            }

            // プロフィール画面全体にVerticalLayoutGroupを追加
            var verticalLayout = profileScreen.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = profileScreen.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            verticalLayout.spacing = 20f;
            verticalLayout.padding = new RectOffset(20, 20, 20, 20);
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.MiddleCenter; // 中央揃えに変更

            // 各要素にLayoutElementを設定
            SetupLayoutElement(profileSectionTitle?.transform, preferredHeight: 60f);
            SetupLayoutElement(profileParent, preferredHeight: -1); // 自動調整
            SetupLayoutElement(backToSelectionButtonFromProfile?.transform, preferredHeight: 50f);
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null && gameManager != null)
            {
                int score = gameManager.GetScore();
                int totalScenarios = gameManager.GetScenarios().Count;
                scoreText.text = $"【もうひとつ】ワードゲット数: {score} / {totalScenarios}";
            }
        }

        private void CreateScenarioButtons()
        {
            // 既存のボタンを削除
            foreach (var button in currentButtons)
            {
                if (button != null) Destroy(button);
            }
            currentButtons.Clear();

            if (scenarioButtonParent == null || scenarioButtonPrefab == null) return;

            // シナリオボタン親のRectTransformを設定
            var buttonParentRect = scenarioButtonParent.GetComponent<RectTransform>();
            if (buttonParentRect != null)
            {
                // AnchorとPivotを設定（VerticalLayoutGroup内なので、自動配置される）
                buttonParentRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonParentRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonParentRect.pivot = new Vector2(0.5f, 0.5f);
                buttonParentRect.anchoredPosition = Vector2.zero;
                // サイズはGridLayoutGroupとContentSizeFitterで自動調整
            }

            // レイアウトコンポーネントを追加（GridLayoutGroup）
            var gridLayout = scenarioButtonParent.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = scenarioButtonParent.gameObject.AddComponent<GridLayoutGroup>();
            }
            gridLayout.cellSize = new Vector2(300, 100);
            gridLayout.spacing = new Vector2(20, 20);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3; // 3列
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.padding = new RectOffset(10, 10, 10, 10);

            // ContentSizeFitterを追加（高さを自動調整）
            var contentSizeFitter = scenarioButtonParent.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = scenarioButtonParent.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scenarios = gameManager.GetScenarios();
            foreach (var scenario in scenarios)
            {
                // シナリオ6は最初の5つをクリアするまで表示しない
                if (scenario.id == 6 && !gameManager.CanAccessScenario(6))
                {
                    continue;
                }

                GameObject buttonObj = Instantiate(scenarioButtonPrefab, scenarioButtonParent);
                var button = buttonObj.GetComponent<Button>();
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                {
                    text.text = scenario.title;
                }

                bool isCompleted = gameManager.IsScenarioCompleted(scenario.id);
                bool isLocked = !gameManager.CanAccessScenario(scenario.id);

                if (isLocked)
                {
                    button.interactable = false;
                    if (text != null) text.text += " (🔒 ロック)";
                }
                else if (isCompleted)
                {
                    // 完了したシナリオは緑色に
                    var colors = button.colors;
                    colors.normalColor = new Color(0.2f, 0.8f, 0.2f);
                    button.colors = colors;
                }

                int scenarioId = scenario.id;
                button.onClick.AddListener(() => OnScenarioSelected(scenarioId));

                currentButtons.Add(buttonObj);
            }
        }

        public void OnScenarioSelected(int scenarioId)
        {
            gameManager.SetCurrentScenario(scenarioId);
            ShowScenarioScreen();
        }

        private void ShowScenarioScreen()
        {
            selectionScreen.SetActive(false);
            scenarioScreen.SetActive(true);
            resultScreen.SetActive(false);

            var scenario = gameManager.GetCurrentScenario();
            if (scenario == null) return;

            if (scenarioTitleText != null)
            {
                scenarioTitleText.text = scenario.title;
            }

            if (setupText != null)
            {
                setupText.text = scenario.setup;
            }

            CreateChoiceButtons();

            // 選択画面に戻るボタンの設定
            if (backToSelectionButtonFromScenario != null)
            {
                backToSelectionButtonFromScenario.onClick.RemoveAllListeners();
                backToSelectionButtonFromScenario.onClick.AddListener(ShowSelectionScreen);
            }
        }

        private void CreateChoiceButtons()
        {
            // 既存のボタンを削除
            foreach (var button in currentButtons)
            {
                if (button != null) Destroy(button);
            }
            currentButtons.Clear();

            if (choiceButtonParent == null || choiceButtonPrefab == null) return;

            // レイアウトコンポーネントを追加（VerticalLayoutGroup）
            var verticalLayout = choiceButtonParent.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = choiceButtonParent.gameObject.AddComponent<VerticalLayoutGroup>();
                verticalLayout.spacing = 15f;
                verticalLayout.padding = new RectOffset(10, 10, 10, 10);
                verticalLayout.childControlWidth = true;
                verticalLayout.childControlHeight = false;
                verticalLayout.childForceExpandWidth = true;
                verticalLayout.childForceExpandHeight = false;
            }

            // ContentSizeFitterを追加
            var contentSizeFitter = choiceButtonParent.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = choiceButtonParent.gameObject.AddComponent<ContentSizeFitter>();
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            var scenario = gameManager.GetCurrentScenario();
            if (scenario == null) return;

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
                GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
                var button = buttonObj.GetComponent<Button>();
                var texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();

                // ボタンのレイアウト要素を設定
                var layoutElement = buttonObj.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = buttonObj.AddComponent<LayoutElement>();
                }
                layoutElement.preferredHeight = 120f; // 選択肢ボタンの高さ

                if (texts.Length > 0 && texts[0] != null)
                {
                    texts[0].text = $"選択肢{choice.id}：{choice.text}";
                }
                if (texts.Length > 1 && texts[1] != null)
                {
                    texts[1].text = choice.preview;
                }

                int choiceId = choice.id;
                button.onClick.AddListener(() => OnChoiceSelected(choiceId));

                currentButtons.Add(buttonObj);
            }
        }

        public void OnChoiceSelected(int choiceId)
        {
            gameManager.HandleChoice(choiceId);
            ShowResultScreen();
        }

        private void ShowResultScreen()
        {
            selectionScreen.SetActive(false);
            scenarioScreen.SetActive(false);
            resultScreen.SetActive(true);

            var scenario = gameManager.GetCurrentScenario();
            if (scenario == null) return;

            var result = gameManager.GetScenarioResult(scenario.id);
            if (result == null) return;

            bool isDarkMode = gameManager.IsDarkMode() && scenario.id == 6;

            // 結果テキストを設定
            if (resultText != null)
            {
                if (isDarkMode)
                {
                    resultText.text = result.choiceId == 1
                        ? "私：「すみません...壊してしまって...」\n\n壊れた声：「謝っても...もう遅い...」\n世界が歪み始める。\n\n壊れた声：「この世界は...シミュレーションだった...」\n「あなたの異常な行動が...世界を破壊した...」\n「もう...修復できない...」\n\n画面が歪み、文字が崩れていく。\nあなたは、自分が何をしてしまったのか理解した。"
                        : "私：「この世界は...何ですか？」\n\n壊れた声：「シミュレーション...すべてが...」\n「あなたは...バグを起こした...」\n「世界の整合性が...崩壊している...」\n\n周囲の空間が歪み、現実が崩れていく。\n登場人物たちの姿が、データの欠片となって消えていく。\n\n壊れた声：「もう...戻れない...」\n「あなたは...世界を壊した...」";
                }
                else
                {
                    resultText.text = scenario.branches[result.choiceId].text;
                }
            }

            // ワードゲット表示
            if (wordGetText != null)
            {
                if (isDarkMode)
                {
                    wordGetText.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                    wordGetText.color = Color.red;
                }
                else if (result.hasWord)
                {
                    wordGetText.text = "✨ 【もうひとつ】ワードゲット! ✨";
                    wordGetText.color = Color.green;
                }
                else
                {
                    wordGetText.text = "残念...【もうひとつ】は出ませんでした";
                    wordGetText.color = Color.red;
                }
            }

            // 後日談を設定
            if (epilogueText != null)
            {
                if (isDarkMode)
                {
                    epilogueText.text = result.choiceId == 1
                        ? "世界は完全に崩壊しました。\nシミュレーションの整合性は失われ、修復不可能な状態です。\n\n登場人物たちは、データの欠片となって消えていきました。\nもも子、うみ、ひろ、とおる、つばさ...\nすべてが、あなたの異常な行動の結果です。\n\nあなたは、空っぽの世界に一人取り残されました。\n「もう...戻れない...」\n\n【エンド：世界崩壊】"
                        : "あなたは、世界の真実を知ってしまいました。\nこの世界は、シミュレーションだったのです。\n\nしかし、あなたの異常な行動が、世界を破壊してしまいました。\n登場人物たちは、バグによって歪んだ姿となっています。\n\nもも子は「も」という文字を失い、\nうみは「う」という文字を失い、\nひろは「ひ」という文字を失い、\nとおるは「と」という文字を失い、\nつばさは「つ」という文字を失いました。\n\n「もうひとつ」という言葉は、永遠に失われました。\n\n【エンド：言葉の消失】";
                }
                else
                {
                    epilogueText.text = result.epilogue;
                }
            }

            if (backToSelectionButton != null)
            {
                backToSelectionButton.onClick.RemoveAllListeners();
                backToSelectionButton.onClick.AddListener(ShowSelectionScreen);
            }
        }

        private void CreateProfileCards()
        {
            // 既存のプロフィールカードを削除
            foreach (var card in currentProfileCards)
            {
                if (card != null) Destroy(card);
            }
            currentProfileCards.Clear();

            if (profileParent == null || profileCardPrefab == null) return;

            // プロフィール画面のレイアウトを設定
            SetupProfileScreenLayout();

            // プロフィール親のRectTransformを設定
            var profileParentRect = profileParent.GetComponent<RectTransform>();
            if (profileParentRect != null)
            {
                // VerticalLayoutGroup内なので、中央基準で設定
                profileParentRect.anchorMin = new Vector2(0.5f, 0.5f);
                profileParentRect.anchorMax = new Vector2(0.5f, 0.5f);
                profileParentRect.pivot = new Vector2(0.5f, 0.5f);
                profileParentRect.anchoredPosition = Vector2.zero;
                // サイズはGridLayoutGroupとContentSizeFitterで自動調整
            }

            // レイアウトコンポーネントを追加（GridLayoutGroup）
            var gridLayout = profileParent.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = profileParent.gameObject.AddComponent<GridLayoutGroup>();
            }
            gridLayout.cellSize = new Vector2(300, 380);
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3; // 3列
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.padding = new RectOffset(10, 10, 10, 10);

            // ContentSizeFitterを追加（高さを自動調整）
            var contentSizeFitter = profileParent.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = profileParent.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scenarios = gameManager.GetScenarios();
            bool isDarkMode = gameManager.IsDarkMode();
            bool scenario6Completed = gameManager.IsScenarioCompleted(6);

            // プロフィールセクションタイトルの更新
            if (profileSectionTitle != null)
            {
                profileSectionTitle.text = isDarkMode ? "登場人物プロフィール【データ破損】" : "登場人物プロフィール";
                profileSectionTitle.color = isDarkMode ? Color.red : Color.black;
            }

            // シナリオ1-5のプロフィール
            for (int i = 1; i <= 5; i++)
            {
                var profile = CharacterProfileManager.GetProfile(i);
                if (profile == null) continue;

                var result = gameManager.GetScenarioResult(i);
                bool isUnlocked = result != null;

                CreateProfileCard(profile, result, isUnlocked, isDarkMode, scenario6Completed);
            }

            // シナリオ6のプロフィール（クリア後のみ表示）
            if (scenario6Completed)
            {
                var profile = CharacterProfileManager.GetProfile(6);
                if (profile != null)
                {
                    var result = gameManager.GetScenarioResult(6);
                    CreateProfileCard(profile, result, true, isDarkMode, scenario6Completed);
                }
            }
        }

        private void CreateProfileCard(CharacterProfile profile, ScenarioResult result, bool isUnlocked, bool isDarkMode, bool scenario6Completed)
        {
            GameObject cardObj = Instantiate(profileCardPrefab, profileParent);
            var cardImage = cardObj.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.color = isUnlocked ? profile.profileColor : new Color(0.8f, 0.8f, 0.8f);
            }

            // プロフィールカードの構造を設定
            // カードには以下の子要素が必要：
            // - NameText (TextMeshProUGUI): 名前
            // - RoleText (TextMeshProUGUI): 役割
            // - InfoText (TextMeshProUGUI): 職業・特徴
            // - QuoteText (TextMeshProUGUI): セリフ
            // - EpilogueText (TextMeshProUGUI): 後日談
            // - ExpandButton (Button): 後日談の後日談を展開するボタン
            // - Epilogue2Text (TextMeshProUGUI): 後日談の後日談
            // - HintText (TextMeshProUGUI): ヒント

            var nameText = cardObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var roleText = cardObj.transform.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
            var infoText = cardObj.transform.Find("InfoText")?.GetComponent<TextMeshProUGUI>();
            var quoteText = cardObj.transform.Find("QuoteText")?.GetComponent<TextMeshProUGUI>();
            var epilogueText = cardObj.transform.Find("EpilogueText")?.GetComponent<TextMeshProUGUI>();
            var expandButton = cardObj.transform.Find("ExpandButton")?.GetComponent<Button>();
            var epilogue2Text = cardObj.transform.Find("Epilogue2Text")?.GetComponent<TextMeshProUGUI>();
            var hintText = cardObj.transform.Find("HintText")?.GetComponent<TextMeshProUGUI>();
            var lockedOverlay = cardObj.transform.Find("LockedOverlay");

            // ロック状態の表示
            if (lockedOverlay != null)
            {
                lockedOverlay.gameObject.SetActive(!isUnlocked);
            }

            if (nameText != null)
            {
                nameText.text = isUnlocked ? $"{profile.name}（{profile.role}）" : $"???（{profile.role}）";
            }

            if (infoText != null)
            {
                string info = "";
                if (isUnlocked)
                {
                    info = $"<b>職業:</b> {(isDarkMode ? "【データ欠損】" : profile.job)}\n";
                    info += $"<b>特徴:</b> {(isDarkMode ? profile.featureDarkMode : profile.feature)}";
                    
                    if (scenario6Completed && !isDarkMode && !string.IsNullOrEmpty(profile.relationshipWithVoice))
                    {
                        info += $"\n\n<b style=\"color:#{ColorUtility.ToHtmlStringRGB(profile.borderColor)}\">謎の声との関係:</b> {profile.relationshipWithVoice}";
                    }
                    else if (scenario6Completed && isDarkMode && !string.IsNullOrEmpty(profile.bugDescription))
                    {
                        info += $"\n\n<b style=\"color:red\">【バグ】:</b> {profile.bugDescription}";
                    }
                }
                else
                {
                    info = $"シナリオ「{GetScenarioTitle(profile.scenarioId)}」をクリアすると表示されます";
                }
                infoText.text = info;
            }

            if (quoteText != null)
            {
                if (isUnlocked)
                {
                    quoteText.text = isDarkMode ? profile.quoteDarkMode : profile.quote;
                    quoteText.color = isDarkMode ? Color.red : profile.borderColor;
                    quoteText.gameObject.SetActive(true);
                }
                else
                {
                    quoteText.gameObject.SetActive(false);
                }
            }

            // 後日談の表示
            if (epilogueText != null && result != null)
            {
                epilogueText.text = isDarkMode ? GetDarkModeEpilogue(profile.scenarioId, result.choiceId) : result.epilogue;
                epilogueText.gameObject.SetActive(true);
            }
            else if (epilogueText != null)
            {
                epilogueText.gameObject.SetActive(false);
            }

            // 後日談の後日談
            if (result != null && result.hasWord && profile.scenarioId <= 5)
            {
                var scenario = gameManager.GetScenarios().Find(s => s.id == profile.scenarioId);
                if (scenario != null && scenario.branches.ContainsKey(result.choiceId) && 
                    !string.IsNullOrEmpty(scenario.branches[result.choiceId].epilogue2))
                {
                    bool isExpanded = expandedProfiles.Contains(profile.scenarioId);
                    
                    if (expandButton != null)
                    {
                        expandButton.gameObject.SetActive(true);
                        expandButton.onClick.RemoveAllListeners();
                        expandButton.onClick.AddListener(() => ToggleEpilogue2(profile.scenarioId));
                        
                        var expandButtonText = expandButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (expandButtonText != null)
                        {
                            expandButtonText.text = isExpanded ? "▼ 後日談の後日談を隠す" : "▶ 後日談の後日談を見る";
                        }
                    }

                    if (epilogue2Text != null)
                    {
                        epilogue2Text.text = isDarkMode ? GetDarkModeEpilogue2(profile.scenarioId) : scenario.branches[result.choiceId].epilogue2;
                        epilogue2Text.gameObject.SetActive(isExpanded);
                    }
                }
                else
                {
                    if (expandButton != null) expandButton.gameObject.SetActive(false);
                    if (epilogue2Text != null) epilogue2Text.gameObject.SetActive(false);
                }
            }
            else
            {
                if (expandButton != null) expandButton.gameObject.SetActive(false);
                if (epilogue2Text != null) epilogue2Text.gameObject.SetActive(false);
            }

            // ヒントの表示
            if (hintText != null && result != null && !result.hasWord && profile.scenarioId <= 5)
            {
                var scenario = gameManager.GetScenarios().Find(s => s.id == profile.scenarioId);
                if (scenario != null && scenario.branches.ContainsKey(result.choiceId) && 
                    !string.IsNullOrEmpty(scenario.branches[result.choiceId].hint))
                {
                    hintText.text = scenario.branches[result.choiceId].hint;
                    hintText.gameObject.SetActive(true);
                }
                else
                {
                    hintText.gameObject.SetActive(false);
                }
            }
            else if (hintText != null)
            {
                hintText.gameObject.SetActive(false);
            }

            currentProfileCards.Add(cardObj);
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
            CreateProfileCards(); // 再生成
        }

        private string GetScenarioTitle(int scenarioId)
        {
            var scenarios = gameManager.GetScenarios();
            var scenario = scenarios.Find(s => s.id == scenarioId);
            return scenario != null ? scenario.title : "";
        }

        private string GetDarkModeEpilogue(int scenarioId, int choiceId)
        {
            // ダークモードの後日談テキスト
            return "【データ破損】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "のデータは完全に崩壊しました。";
        }

        private string GetDarkModeEpilogue2(int scenarioId)
        {
            return "【完全崩壊】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "は完全にデータの欠片となって消えました。";
        }
    }
}

