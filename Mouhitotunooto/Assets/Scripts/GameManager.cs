using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private NovelGameData gameData;
        private List<Scenario> scenarios = new List<Scenario>();
        
        private int score = 0;
        private HashSet<int> completedScenarios = new HashSet<int>();
        private Dictionary<int, ScenarioResult> scenarioResults = new Dictionary<int, ScenarioResult>();
        private HashSet<char> collectedLetters = new HashSet<char>();
        private int currentScenarioIndex = -1;

        public event Action OnScoreChanged;
        public event Action OnScenarioCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeScenarios();
        }

        private void InitializeScenarios()
        {
            if (gameData != null && gameData.scenarios != null && gameData.scenarios.Count > 0)
            {
                scenarios = gameData.scenarios;
            }
            else
            {
                // gameDataが設定されていない場合、ScenarioDataLoaderから取得
                var dataLoader = FindObjectOfType<ScenarioDataLoader>();
                if (dataLoader != null)
                {
                    scenarios = dataLoader.GetScenarios();
                }
                else
                {
                    // フォールバック：直接作成（簡易版）
                    Debug.LogWarning("GameDataとScenarioDataLoaderが見つかりません。デフォルトデータを使用します。");
                }
            }
        }

        public List<Scenario> GetScenarios()
        {
            if (scenarios == null || scenarios.Count == 0)
            {
                InitializeScenarios();
            }
            return scenarios;
        }

        public Scenario GetCurrentScenario()
        {
            var scenarios = GetScenarios();
            if (currentScenarioIndex >= 0 && currentScenarioIndex < scenarios.Count)
            {
                return scenarios[currentScenarioIndex];
            }
            return null;
        }

        public void SetCurrentScenario(int scenarioId)
        {
            var scenarios = GetScenarios();
            currentScenarioIndex = scenarios.FindIndex(s => s.id == scenarioId);
        }

        public int GetScore()
        {
            return score;
        }

        public bool IsScenarioCompleted(int scenarioId)
        {
            return completedScenarios.Contains(scenarioId);
        }

        public ScenarioResult GetScenarioResult(int scenarioId)
        {
            return scenarioResults.ContainsKey(scenarioId) ? scenarioResults[scenarioId] : null;
        }

        public HashSet<char> GetCollectedLetters()
        {
            return new HashSet<char>(collectedLetters);
        }

        public void HandleChoice(int choiceId)
        {
            var scenario = GetCurrentScenario();
            if (scenario == null) return;

            var branch = scenario.branches[choiceId];
            var scenarioId = scenario.id;

            bool hasWord = branch.hasWord;
            if (hasWord)
            {
                score++;
                completedScenarios.Add(scenarioId);

                // 文字を収集（シナリオ1-5のみ）
                if (scenarioId <= 5)
                {
                    char[] letters = { 'も', 'う', 'ひ', 'と', 'つ' };
                    int letterIndex = scenarioId - 1;
                    if (letterIndex >= 0 && letterIndex < letters.Length)
                    {
                        collectedLetters.Add(letters[letterIndex]);
                    }
                }

                OnScoreChanged?.Invoke();
            }

            scenarioResults[scenarioId] = new ScenarioResult
            {
                hasWord = hasWord,
                choiceId = choiceId,
                epilogue = branch.epilogue,
                epilogue2 = branch.epilogue2,
                scoreAtCompletion = score
            };

            OnScenarioCompleted?.Invoke();
        }

        public bool CanAccessScenario(int scenarioId)
        {
            // シナリオ6は最初の5つをクリアするまでアクセス不可
            if (scenarioId == 6)
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (!completedScenarios.Contains(i))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsDarkMode()
        {
            return score > GetScenarios().Count;
        }

        public void ResetGame()
        {
            score = 0;
            completedScenarios.Clear();
            scenarioResults.Clear();
            collectedLetters.Clear();
            currentScenarioIndex = -1;
            OnScoreChanged?.Invoke();
        }
    }

    [Serializable]
    public class ScenarioResult
    {
        public bool hasWord;
        public int choiceId;
        public string epilogue;
        public string epilogue2;
        public int scoreAtCompletion;
    }
}

