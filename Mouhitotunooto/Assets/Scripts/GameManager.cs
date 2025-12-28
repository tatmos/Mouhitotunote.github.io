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
        private HashSet<int> completedScenariosInDarkMode = new HashSet<int>(); // ダークモード中にクリアしたシナリオ
        private Dictionary<int, ScenarioResult> scenarioResults = new Dictionary<int, ScenarioResult>();
        private HashSet<char> collectedLetters = new HashSet<char>();
        private HashSet<char> restoredLetters = new HashSet<char>();
        private HashSet<char> lastLostLetters = new HashSet<char>();
        private int currentScenarioIndex = -1;
        private bool isDarkMode = false;
        private bool isThirdLoop = false;
        private bool pendingDarkMode = false; // ダークモード突入待ちフラグ
        
        // Divisionのクリア状況
        private HashSet<string> clearedDivisions = new HashSet<string>();
        // 全Divisionを表示するデバッグフラグ
        [SerializeField] private bool debugShowAllDivisions = false;
        
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
            
            // ダークモード中にシナリオを選択した際、スコアが6に戻っていればDivision Cへ強制転送
            if (IsDarkMode() && !isThirdLoop && score <= 6)
            {
                Debug.Log("[GameManager] 不正なデータが修正されました。システムを強制再起動します。");
                // 3周目への移行（Division C相当の処理）
                UIManagerUIToolkit.Instance.TriggerDivisionCTransition(score);
            }
            
            CheckLostLettersUpdate();
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

            // 選択されたブランチの情報を取得
            var branch = scenario.branches[choiceId];
            var scenarioId = scenario.id;

            // シナリオ開始時のモードを記録（エンド記録用）
            // HandleChoiceの中でモードが変わる可能性があるため、現在のモードを保持しておく
            bool playedInDarkMode = IsDarkMode();

            bool hasWord = overrideHasWord ?? branch.hasWord;
            if (hasWord)
            {
                // ダークモード中はスコア（ワードゲット数）が増えないようにする
                if (!playedInDarkMode)
                {
                    score++;
                }
                
                completedScenarios.Add(scenarioId);
                
                // ダークモード中にクリアした場合は記録
                if (playedInDarkMode)
                {
                    completedScenariosInDarkMode.Add(scenarioId);
                }

                // 文字を収集（シナリオ1-5のみ）
                if (scenarioId <= 5)
                {
                    char[] letters = { 'も', 'う', 'ひ', 'と', 'つ' };
                    int letterIndex = scenarioId - 1;
                    if (letterIndex >= 0 && letterIndex < letters.Length)
                    {
                        char collectedLetter = letters[letterIndex];
                        if (!collectedLetters.Contains(collectedLetter))
                        {
                            Debug.Log($"[GameManager] 文字を取得しました: {collectedLetter}");
                        }
                        collectedLetters.Add(collectedLetter);
                    }
                }

                OnScoreChanged?.Invoke();
                
                // 通常モードで全シナリオ(1-6)をクリアし、スコアが7に達したらダークモード突入を予約
                // ただし、シナリオ6をクリアした瞬間に判定する（既存の仕様を維持しつつ、不意の突入を防ぐ）
                if (!IsDarkMode() && !isThirdLoop && scenarioId == 6 && score >= 7)
                {
                    // ダークモード突入を予約（リザルト画面の後に有効化される）
                    pendingDarkMode = true;
                    Debug.Log("[GameManager] 真実の扉で不正を判定されました。修正プログラムを起動します（ダークモード予約）。");
                }
                
                CheckLostLettersUpdate();
            }

            // 3周目の場合は、シナリオIDに対応する文字を復活させる
            if (isThirdLoop && hasWord)
            {
                char[] letters = { 'も', 'う', 'ひ', 'と', 'つ' };
                int letterIndex = scenarioId - 1;
                if (letterIndex >= 0 && letterIndex < letters.Length)
                {
                    RestoreLetter(letters[letterIndex]);
                }
            }

            // ダークモード中に False エンド（ワード取得失敗）を選んでスコアが6に戻った場合も、Division Cへの移行を検討
            // ただし、現在は playedInDarkMode かつ !hasWord の場合、score はそのまま（増えていない）
            // スコアが 7 から 6 に戻るという状況は、「ダークモード中にわざとワードを取らない」ことで発生させたい
            if (playedInDarkMode && !isThirdLoop && !hasWord)
            {
                if (score > 6)
                {
                    score--;
                    Debug.Log($"[GameManager] 不正なデータを破棄しました。現在のスコア: {score}");
                    OnScoreChanged?.Invoke();
                }
            }

            // ダークモード判定（Division判定に使用）
            // このターンでダークモード突入条件を満たした場合も考慮
            bool isActuallyDarkMode = isDarkMode || isThirdLoop || pendingDarkMode;
            
            scenarioResults[scenarioId] = new ScenarioResult
            {
                hasWord = hasWord,
                choiceId = choiceId,
                epilogue = branch.epilogue,
                epilogue2 = branch.epilogue2,
                scoreAtCompletion = score
            };
            
            // 見たエンドを記録（シナリオ開始時のモードを使用）
            if (!seenEndsByMode.ContainsKey(scenarioId))
            {
                seenEndsByMode[scenarioId] = new Dictionary<bool, HashSet<int>>();
            }
            if (!seenEndsByMode[scenarioId].ContainsKey(playedInDarkMode))
            {
                seenEndsByMode[scenarioId][playedInDarkMode] = new HashSet<int>();
            }
            seenEndsByMode[scenarioId][playedInDarkMode].Add(choiceId);

            // 節目（Division）の判定とログ出力
            if (scenarioId == 6)
            {
                int totalScenarios = GetScenarios().Count;
                
                if (!isThirdLoop)
                {
                    // 以前の状態が通常モードだった場合
                    if (!playedInDarkMode)
                    {
                        if (score < 7)
                        {
                            Debug.Log("[GameManager] division A: 伏字なしモードでクリア数オーバーなしでシナリオ6クリア -> まだ、もうひとつの世界に気づいていない");
                            clearedDivisions.Add("A");
                        }
                        else
                        {
                            Debug.Log("[GameManager] division B: 伏字なしモードでクリア数オーバーありでシナリオ6クリア -> 真実の扉で不正を判定され、修正プログラムが暴走し始める（ダークモード突入）");
                            clearedDivisions.Add("B");
                        }
                    }
                    else if (isActuallyDarkMode)
                    {
                        // ダークモード中の判定
                        if (AreAllLettersLost())
                        {
                            Debug.Log("[GameManager] division C: すべての文字を失った状態でシナリオ6クリア -> 3周目へ強制移行");
                            clearedDivisions.Add("C");
                        }
                    }
                }
                else if (isThirdLoop)
                {
                    if (score < 7)
                    {
                        Debug.Log("[GameManager] division D: 伏字モードでクリア数オーバーなしでシナリオ6クリア -> すべての文字を取り返した、エンドクレジットともうひとつの世界（ゲームから離れた現実）終焉エンド");
                        clearedDivisions.Add("D");
                    }
                    else
                    {
                        Debug.Log("[GameManager] division E: 2週目：伏字モードでクリア数オーバーありでシナリオ6クリア -> すべての文字を取り返したが、バグも発生させた、エンドクレジットともうひとつの世界（ゲームから離れた現実）終焉エンド");
                        clearedDivisions.Add("E");
                    }
                }
            }

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
            // 3周目の間は、破損演出（ダークモード）は無効にする。
            // ただし、内部的な条件判定のために isDarkMode フラグ自体は残っている可能性があるが、
            // 演出としては isThirdLoop が優先されるべき。
            if (isThirdLoop) return false;
            
            // 明示的なダークモードフラグ、あるいはスコアが規定を超えている（かつ、まだ通常ループ中ではない）場合にダークモードとする
            return isDarkMode;
        }

        public void SetDarkMode(bool enabled)
        {
            isDarkMode = enabled;
            pendingDarkMode = false; // 明示的な設定時は予約を解除
            CheckLostLettersUpdate();
        }

        /// <summary>
        /// 予約されているダークモードを有効化する
        /// </summary>
        public void ActivatePendingDarkMode()
        {
            if (pendingDarkMode)
            {
                isDarkMode = true;
                pendingDarkMode = false;
                Debug.Log("[GameManager] 予約されていたダークモードを有効化しました。");
                CheckLostLettersUpdate();
                OnScoreChanged?.Invoke(); // 伏字表示の更新などのために通知
            }
        }

        /// <summary>
        /// クリア済みのDivision数を取得
        /// </summary>
        public int GetClearedDivisionsCount()
        {
            return clearedDivisions.Count;
        }

        public bool IsThirdLoop()
        {
            return isThirdLoop;
        }

        /// <summary>
        /// Divisionのクリア状況を取得
        /// </summary>
        public bool IsDivisionCleared(string divisionId)
        {
            if (debugShowAllDivisions) return true;
            return clearedDivisions.Contains(divisionId);
        }

        /// <summary>
        /// 特定のDivisionへジャンプ（デバッグ/再挑戦用）
        /// </summary>
        public void JumpToDivision(string divisionId)
        {
            ResetGame();
            int totalScenarios = GetScenarios().Count;

            switch (divisionId)
            {
                case "Prologue":
                    Debug.Log("[GameManager] プロローグを開始します。");
                    break;
                case "A":
                    // 通常モード、未クリア状態
                    Debug.Log("[GameManager] Division A を開始します。");
                    break;
                case "B":
                    // 通常モード、全クリア状態
                    Debug.Log("[GameManager] Division B を開始します。");
                    for (int i = 1; i <= totalScenarios; i++)
                    {
                        completedScenarios.Add(i);
                        score++;
                    }
                    // シナリオ6もクリア済みにすることでダークモード条件を満たす
                    isDarkMode = false;
                    break;
                case "C":
                    // ダークモード、全文字消失直前
                    Debug.Log("[GameManager] Division C を開始します。");
                    isDarkMode = true;
                    for (int i = 1; i <= 5; i++)
                    {
                        completedScenarios.Add(i);
                        completedScenariosInDarkMode.Add(i);
                        score++;
                    }
                    score++; // シナリオ6分
                    break;
                case "D":
                    // 3周目、開始状態
                    Debug.Log("[GameManager] Division D を開始します。");
                    TriggerThirdLoop();
                    break;
                case "E":
                    // 3周目、全文字復活直前
                    Debug.Log("[GameManager] Division E を開始します。");
                    TriggerThirdLoop();
                    for (int i = 1; i <= 5; i++)
                    {
                        completedScenarios.Add(i);
                        score++;
                        char[] letters = { 'も', 'う', 'ひ', 'と', 'つ' };
                        RestoreLetter(letters[i-1]);
                    }
                    break;
            }
            OnScoreChanged?.Invoke();
            Debug.Log($"[GameManager] Division {divisionId} へジャンプしました。");
        }

        public bool AreAllLettersLost()
        {
            if (isThirdLoop) return true;
            if (!IsDarkMode()) return false;
            
            char[] allLetters = { 'も', 'う', 'ひ', 'と', 'つ' };
            int count = 0;
            for (int i = 1; i <= 5; i++)
            {
                if (completedScenariosInDarkMode.Contains(i)) count++;
            }
            
            return count >= 5;
        }

        public void TriggerThirdLoop()
        {
            // ResetGameの前に一時的にフラグを退避させるか、ResetGameを呼んでからフラグを立てる
            score = 0;
            completedScenarios.Clear();
            completedScenariosInDarkMode.Clear();
            scenarioResults.Clear();
            collectedLetters.Clear();
            restoredLetters.Clear();
            lastLostLetters.Clear();
            seenEndsByMode.Clear();
            currentScenarioIndex = -1;
            isDarkMode = false; // 3周目開始時は破損状態をリセットし、伏字のみの状態にする
            isThirdLoop = true;
            pendingDarkMode = false;
            OnScoreChanged?.Invoke();
        }

        /// <summary>
        /// 文字を復活させる（3周目用）
        /// </summary>
        public void RestoreLetter(char letter)
        {
            if (isThirdLoop && !restoredLetters.Contains(letter))
            {
                restoredLetters.Add(letter);
                Debug.Log($"[GameManager] 文字が復活しました: {letter}");
                CheckLostLettersUpdate();
            }
        }

        /// <summary>
        /// ダークモードで失われた文字を取得
        /// </summary>
        public HashSet<char> GetLostLetters()
        {
            HashSet<char> lostLetters = new HashSet<char>();
            char[] allLetters = { 'も', 'う', 'ひ', 'と', 'つ' };

            // 3周目は最初から全ての文字が失われている（ただし復活した文字は除く）
            if (isThirdLoop)
            {
                foreach (char c in allLetters)
                {
                    if (!restoredLetters.Contains(c))
                    {
                        lostLetters.Add(c);
                    }
                }
                return lostLetters;
            }

            if (!IsDarkMode()) return lostLetters;

            // ダークモード中に完了したシナリオ（1〜5）に対応する文字を失われた文字に加える
            for (int i = 1; i <= 5; i++)
            {
                if (completedScenariosInDarkMode.Contains(i))
                {
                    lostLetters.Add(allLetters[i - 1]);
                }
            }

            // シナリオ6のプレイ中（または終了直後）は、追加の消失（累積的なものなど）を抑制する場合があるが、
            // 既に完了したシナリオの文字は消えたままにする。
            var currentScenario = GetCurrentScenario();
            if (currentScenario != null && currentScenario.id >= 1 && currentScenario.id <= 5)
            {
                // 現在プレイ中のシナリオに対応する文字も「消失」として扱う（シナリオ1〜5のみ）
                lostLetters.Add(allLetters[currentScenario.id - 1]);
            }
            
            // スコアがシナリオ数+1ごとに1文字ずつ累積的に失われる演出
            // ※「シナリオをクリアして消える」という直感的な演出を優先するため、現在はコメントアウトまたは無効化
            /*
            int lostCount = score - GetScenarios().Count;
            for (int i = 0; i < lostCount && i < allLetters.Length; i++)
            {
                lostLetters.Add(allLetters[i]);
            }
            */
            
            return lostLetters;
        }

        /// <summary>
        /// 消失した文字の更新をチェックし、新しく消失した文字があればログを出力
        /// </summary>
        private void CheckLostLettersUpdate()
        {
            var currentLostLetters = GetLostLetters();
            foreach (char c in currentLostLetters)
            {
                if (!lastLostLetters.Contains(c))
                {
                    Debug.Log($"[GameManager] 文字が消失しました: {c}");
                }
            }
            lastLostLetters = currentLostLetters;
        }

        public void ResetGame()
        {
            score = 0;
            completedScenarios.Clear();
            completedScenariosInDarkMode.Clear();
            scenarioResults.Clear();
            collectedLetters.Clear();
            restoredLetters.Clear();
            lastLostLetters.Clear();
            seenEndsByMode.Clear();
            isDarkMode = false;
            isThirdLoop = false; // 通常のリセットでは3周目フラグも落とす
            pendingDarkMode = false;
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

