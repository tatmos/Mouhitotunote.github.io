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

        private GameManager gameManager;
        private List<GameObject> currentButtons = new List<GameObject>();

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

            if (titleText != null)
            {
                titleText.text = "ミニノベルゲーム";
            }

            UpdateScoreDisplay();
            CreateScenarioButtons();
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
    }
}

