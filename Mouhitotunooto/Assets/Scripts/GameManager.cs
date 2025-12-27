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
        
        // 見たエンドを記録（シナリオID -> ダークモードかどうか -> 見たchoiceIdのセット）
        private Dictionary<int, Dictionary<bool, HashSet<int>>> seenEndsByMode = new Dictionary<int, Dictionary<bool, HashSet<int>>>();

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
                var dataLoader = FindFirstObjectByType<ScenarioDataLoader>();
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

        public void HandleChoice(int choiceId, bool? overrideHasWord = null)
        {
            var scenario = GetCurrentScenario();
            if (scenario == null) return;

            var branch = scenario.branches[choiceId];
            var scenarioId = scenario.id;

            bool hasWord = overrideHasWord ?? branch.hasWord;
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

            bool wasDarkMode = IsDarkMode() && scenarioId == 6;
            
            scenarioResults[scenarioId] = new ScenarioResult
            {
                hasWord = hasWord,
                choiceId = choiceId,
                epilogue = branch.epilogue,
                epilogue2 = branch.epilogue2,
                scoreAtCompletion = score
            };
            
            // 見たエンドを記録（通常モード/ダークモードを区別）
            if (!seenEndsByMode.ContainsKey(scenarioId))
            {
                seenEndsByMode[scenarioId] = new Dictionary<bool, HashSet<int>>();
            }
            if (!seenEndsByMode[scenarioId].ContainsKey(wasDarkMode))
            {
                seenEndsByMode[scenarioId][wasDarkMode] = new HashSet<int>();
            }
            seenEndsByMode[scenarioId][wasDarkMode].Add(choiceId);

            OnScenarioCompleted?.Invoke();
        }
        
        /// <summary>
        /// 指定されたシナリオの指定されたchoiceIdのエンドを見たかどうかを取得（通常モード/ダークモードを区別）
        /// </summary>
        public bool HasSeenEnd(int scenarioId, int choiceId, bool? isDarkMode = null)
        {
            if (!seenEndsByMode.ContainsKey(scenarioId))
            {
                return false;
            }
            
            // isDarkModeが指定されている場合は、そのモードのみチェック
            if (isDarkMode.HasValue)
            {
                if (seenEndsByMode[scenarioId].ContainsKey(isDarkMode.Value))
                {
                    return seenEndsByMode[scenarioId][isDarkMode.Value].Contains(choiceId);
                }
                return false;
            }
            
            // isDarkModeが指定されていない場合は、どちらかで見ていればtrue
            foreach (var modeSet in seenEndsByMode[scenarioId].Values)
            {
                if (modeSet.Contains(choiceId))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 指定されたシナリオのTrueエンドを見たかどうかを取得
        /// </summary>
        public bool HasSeenTrueEnd(int scenarioId)
        {
            var scenario = GetScenarios().Find(s => s.id == scenarioId);
            if (scenario == null) return false;
            
            var trueChoiceId = scenario.choices.Find(c => scenario.branches.ContainsKey(c.id) && scenario.branches[c.id].hasWord)?.id ?? -1;
            if (trueChoiceId == -1) return false;
            
            return HasSeenEnd(scenarioId, trueChoiceId);
        }
        
        /// <summary>
        /// 指定されたシナリオのFalseエンドを見たかどうかを取得
        /// </summary>
        public bool HasSeenFalseEnd(int scenarioId)
        {
            var scenario = GetScenarios().Find(s => s.id == scenarioId);
            if (scenario == null) return false;
            
            var falseChoiceId = scenario.choices.Find(c => scenario.branches.ContainsKey(c.id) && !scenario.branches[c.id].hasWord)?.id ?? -1;
            if (falseChoiceId == -1) return false;
            
            return HasSeenEnd(scenarioId, falseChoiceId);
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
            seenEndsByMode.Clear();
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

