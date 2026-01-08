using System;
using System.Collections.Generic;
using UnityEngine;
using unityroom.Api;
using NovelGame.Overlay;

namespace NovelGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private NovelGameData gameData;
        private List<Scenario> scenarios = new List<Scenario>();
        
        internal int score = 0;
        private HashSet<int> completedScenarios = new HashSet<int>();
        private HashSet<int> completedScenariosInDarkMode = new HashSet<int>(); // ダークモード中にクリアしたシナリオ
        private HashSet<int> startedScenariosInDarkMode = new HashSet<int>(); // ダークモード中に開始したシナリオ（1-5のみ）
        private Dictionary<int, ScenarioResult> scenarioResults = new Dictionary<int, ScenarioResult>();
        private HashSet<char> collectedLetters = new HashSet<char>();
        private HashSet<char> restoredLetters = new HashSet<char>();
        private HashSet<char> lastLostLetters = new HashSet<char>();
        private int currentScenarioIndex = -1;
        private bool isDarkMode = false;
        internal bool isThirdLoop = false;
        private bool pendingDarkMode = false; // ダークモード突入待ちフラグ
        private bool hasCompletedFirstLoop = false; // 1周目をクリアしたかどうか
        private bool isScenario6Unlocked = false; // シナリオ6が解放されたかどうか（演出用フラグ）
        // 各シナリオの2周目クリア回数（シナリオID -> 2周目クリア回数）
        // 2周目でクリアした回数が1回 = 3周目、2回 = 4周目、というように増えていく
        private Dictionary<int, int> scenarioThirdLoopCounts = new Dictionary<int, int>();
        
        // タイムトラッキング
        private DateTime gameStartTime;
        private DateTime gameEndTime;

        // Chapterのクリア状況（ChapterManagerに移行済み。互換性のため残存）
        private HashSet<string> clearedChapters = new HashSet<string>();
        // 全Chapterを表示するデバッグフラグ（ChapterManagerに移行済み。互換性のため残存）
        [SerializeField] private bool debugShowAllChapters = false;
        
        // 見たエンドを記録（シナリオID -> ダークモードかどうか -> 見たchoiceIdのセット）
        private Dictionary<int, Dictionary<bool, HashSet<int>>> seenEndsByMode = new Dictionary<int, Dictionary<bool, HashSet<int>>>();
        
        // シナリオごとのランダム要素を保存（シナリオID -> 要素名 -> 値）
        // 例: {1: {"buildingName": "旧市庁舎"}, {4: {"animalName": "ウサギ"}}, {2: {"menuName": "おせち"}}}
        private Dictionary<int, Dictionary<string, string>> scenarioRandomData = new Dictionary<int, Dictionary<string, string>>();

        // 設定：物語の解明度表示ON/OFF
        private bool showStoryProgress = true;

        public event Action OnScoreChanged;
        public event Action OnScenarioCompleted;
        public event Action<char> OnLetterLost; // 文字が失われた時のイベント

        /// <summary>
        /// スコア変更イベントを発火（外部から呼び出し可能）
        /// </summary>
        public void NotifyScoreChanged()
        {
            OnScoreChanged?.Invoke();
        }

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
            gameStartTime = DateTime.Now;
            // 設定をPlayerPrefsから読み込み
            showStoryProgress = PlayerPrefs.GetInt("ShowStoryProgress", 1) == 1;
        }

        /// <summary>
        /// 物語の解明度表示ON/OFFを設定
        /// </summary>
        public void SetShowStoryProgress(bool show)
        {
            showStoryProgress = show;
            PlayerPrefs.SetInt("ShowStoryProgress", show ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 物語の解明度表示ON/OFFを取得
        /// </summary>
        public bool GetShowStoryProgress()
        {
            return showStoryProgress;
        }

        public DateTime GetGameStartTime() => gameStartTime;

        public void SetGameStartTime(DateTime time)
        {
            gameStartTime = time;
        }

        public void SetGameEndTime(DateTime endTime)
        {
            gameEndTime = endTime;
        }

        public DateTime GetGameEndTime() => gameEndTime;

        public string GetPlayTimeDisplay()
        {
            TimeSpan duration = gameEndTime - gameStartTime;
            return $"{(int)duration.TotalHours}時間{duration.Minutes}分{duration.Seconds}秒";
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
            // ランダム要素が生成されていない場合は生成する（初回呼び出し時など）
            if (scenarioRandomData.Count == 0)
            {
                GenerateScenarioRandomData();
            }
            
            // 周回数が変わった可能性があるため、常に最新のシナリオを生成
            // （ScenarioDefinitions.CreateScenarios()が最新の周回数を取得するため）
            // 直接ScenarioDefinitions.CreateScenarios()を呼び出して、常に最新のシナリオを取得
            scenarios = ScenarioDefinitions.CreateScenarios();
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
            
            // Overlayイベント発火
            OverlayEventHub.Publish(new ScenarioStartedEvt(scenarioId));
            
            // ダークモード時にシナリオ（1-5のみ）を開始した場合、そのシナリオの文字を失う
            if (IsDarkMode() && !isThirdLoop && MouhitotsuWordManager.IsValidScenarioId(scenarioId))
            {
                if (!startedScenariosInDarkMode.Contains(scenarioId))
                {
                    startedScenariosInDarkMode.Add(scenarioId);
                    char lostLetter = MouhitotsuWordManager.GetLetterByScenarioId(scenarioId);
                    Debug.Log($"[GameManager] ダークモードでシナリオ{scenarioId}を開始しました。文字「{lostLetter}」を失います。");
                    // 文字が失われたイベントを発火
                    OnLetterLost?.Invoke(lostLetter);
                    CheckLostLettersUpdate();
                }
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

            // Overlayイベント発火（選択肢選択）
            OverlayEventHub.Publish(new ChoiceSelectedEvt(scenarioId, choiceId.ToString()));

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
                    if (!completedScenariosInDarkMode.Contains(scenarioId))
                    {
                        completedScenariosInDarkMode.Add(scenarioId);
                        // シナリオ1-5の場合、文字が失われたイベントを発火
                        if (MouhitotsuWordManager.IsValidScenarioId(scenarioId))
                        {
                            char lostLetter = MouhitotsuWordManager.GetLetterByScenarioId(scenarioId);
                            OnLetterLost?.Invoke(lostLetter);
                        }
                    }
                }

                // 文字を収集（シナリオ1-5のみ）
                if (MouhitotsuWordManager.IsValidScenarioId(scenarioId))
                {
                    char collectedLetter = MouhitotsuWordManager.GetLetterByScenarioId(scenarioId);
                    if (collectedLetter != '\0')
                    {
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
                    // 1周目をクリアしたことを記録
                    if (!hasCompletedFirstLoop)
                    {
                        hasCompletedFirstLoop = true;
                        Debug.Log("[GameManager] 1周目をクリアしました。");
                    }
                    // ダークモード突入を予約（リザルト画面の後に有効化される）
                    pendingDarkMode = true;
                    Debug.Log("[GameManager] 真実の扉で不正を判定されました。修正プログラムを起動します（ダークモード予約）。");
                }
                // シナリオ6をクリアした時点で1周目クリアと判定（スコアが7未満でも）
                else if (!IsDarkMode() && !isThirdLoop && scenarioId == 6 && !hasCompletedFirstLoop)
                {
                    hasCompletedFirstLoop = true;
                    Debug.Log("[GameManager] 1周目をクリアしました。");
                }
                
                CheckLostLettersUpdate();
            }

            // 2周目以降（isThirdLoop == true）の場合は、シナリオIDに対応する文字を復活させる
            if (isThirdLoop && hasWord)
            {
                if (MouhitotsuWordManager.IsValidScenarioId(scenarioId))
                {
                    char letter = MouhitotsuWordManager.GetLetterByScenarioId(scenarioId);
                    if (letter != '\0')
                    {
                        RestoreLetter(letter);
                    }
                }
                
                // 2周目でクリアした回数をカウント（3周目以降の判定に使用）
                if (!scenarioThirdLoopCounts.ContainsKey(scenarioId))
                {
                    scenarioThirdLoopCounts[scenarioId] = 0;
                }
                scenarioThirdLoopCounts[scenarioId]++;
                Debug.Log($"[GameManager] シナリオ{scenarioId}を2周目でクリアしました。現在の2周目クリア回数: {scenarioThirdLoopCounts[scenarioId]}（周回数: {scenarioThirdLoopCounts[scenarioId] + 2}）");
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

            // 節目（Chapter）の判定とログ出力
            if (ChapterManager.Instance != null)
            {
                ChapterManager.Instance.UpdateAndLogChapterStatus(scenarioId, playedInDarkMode, isActuallyDarkMode, isThirdLoop, score);
            }
            else
            {
                Debug.LogError("[GameManager] ChapterManager.Instance が null です。Chapterの判定をスキップします。");
            }
            
            // チートモードが有効な場合はスコア送信をスキップ
            bool isCheatModeEnabled = ChapterManager.Instance != null && ChapterManager.Instance.GetDebugShowAllChapters();
            if (!isCheatModeEnabled)
            {
                // ボードNo1にスコア123.45fを送信する。
                UnityroomApiClient.Instance.SendScore(1, GetStoryProgressPercentage(), ScoreboardWriteMode.HighScoreDesc);
                UnityroomApiClient.Instance.SendScore(2, GetScore(), ScoreboardWriteMode.HighScoreDesc);
            }
            else
            {
                Debug.Log("[GameManager] チートモードが有効なため、Unityroomへのスコア送信をスキップしました。");
            }

            // Overlayイベント発火（シナリオクリア）
            OverlayEventHub.Publish(new ScenarioClearedEvt(scenarioId, hasWord));

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
            bool wasDarkMode = isDarkMode;
            isDarkMode = enabled;
            pendingDarkMode = false; // 明示的な設定時は予約を解除
            
            // Overlayイベント発火（モード変更）
            if (wasDarkMode != enabled)
            {
                GameMode mode = isThirdLoop ? GameMode.Third : (enabled ? GameMode.Dark : GameMode.Normal);
                OverlayEventHub.Publish(new ModeChangedEvt(mode));
            }
            
            CheckLostLettersUpdate();
        }

        /// <summary>
        /// 予約されているダークモードを取得
        /// </summary>
        public bool GetPendingDarkMode()
        {
            return pendingDarkMode;
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
                
                // Overlayイベント発火（モード変更）
                GameMode mode = isThirdLoop ? GameMode.Third : GameMode.Dark;
                OverlayEventHub.Publish(new ModeChangedEvt(mode));
                
                CheckLostLettersUpdate();
                OnScoreChanged?.Invoke(); // 伏字表示の更新などのために通知
            }
        }

        /// <summary>
        /// クリア済みのDivision数を取得
        /// </summary>
        public int GetClearedChaptersCount()
        {
            if (ChapterManager.Instance != null)
            {
                return ChapterManager.Instance.GetClearedChaptersCount();
            }
            else
            {
                // フォールバック（ChapterManagerがない場合）
                return clearedChapters.Count;
            }
        }

        /// <summary>
        /// 物語の解明度（パーセンテージ）を取得
        /// </summary>
        public int GetStoryProgressPercentage()
        {
            if (ChapterManager.Instance != null)
            {
                return ChapterManager.Instance.GetStoryProgressPercentage(score, completedScenarios, restoredLetters);
            }
            else
            {
                // フォールバック（ChapterManagerがない場合）
                return GetStoryProgressPercentageFallback();
            }
        }

        private int GetStoryProgressPercentageFallback()
        {
            float percentage = 0;

            // 各ディビジョンの基本進捗 (計 5段階想定)
            // A: 20%, B: 40%, C: 60%, D: 80%, E: 100%
            
            // 1. Division A 以前 (通常クリア)
            if (!IsChapterCleared("A"))
            {
                // シナリオ1-6のクリア状況 (最大6つ)
                int completed = 0;
                for (int i = 1; i <= 6; i++)
                {
                    if (completedScenarios.Contains(i)) completed++;
                }
                percentage = (completed / 6f) * 20f;
            }
            else if (!IsChapterCleared("B"))
            {
                percentage = 20f;
            }
            else if (!IsChapterCleared("C"))
            {
                // 2. Division B 以前 (スコア7以上、不正発覚)
                percentage = 40f;
                
                // ダークモード
                // 足りない数で計算
                if (score == 6)
                {
                    percentage += 20f;
                }
                else if (score > 6)
                {
                    percentage += ((score - 6) * 20f) / 6.0f;
                }
            }
            else if (!IsChapterCleared("D") && !IsChapterCleared("E"))
            {
                // 3. Division C (3周目、文字の復元)
                percentage = 60f;
                
                // 復元した文字数 (最大5つ)
                float subProgress = Mathf.Clamp01(restoredLetters.Count / 5f);
                // 3周目のシナリオ6クリアで一気に100%に近づくため、ここでは80%まで
                percentage += subProgress * 20f;
            }
            else
            {
                // 5. 最終段階
                // 4. Division D/E クリア
                percentage = 80f;
                
                // Dのみクリア（不正なしなら）
                if (IsChapterCleared("D") && !IsChapterCleared("E"))
                {
                    percentage = 100f;
                }
            }

            return Mathf.Clamp((int)percentage, 0, 100);
        }

        public bool IsThirdLoop()
        {
            return isThirdLoop;
        }

        /// <summary>
        /// 2周目かどうかを判定（isThirdLoopがtrueの時点で2周目）
        /// </summary>
        public bool IsSecondLoop()
        {
            return isThirdLoop;
        }

        /// <summary>
        /// 指定されたシナリオの現在の周回数を取得（1, 2, 3以上）
        /// </summary>
        /// <param name="scenarioId">シナリオID</param>
        /// <returns>1: 1周目, 2: 2周目（isThirdLoop == trueの最初のプレイ）, 3以上: 3周目以降（2周目でクリアした回数+2）</returns>
        public int GetScenarioLoopCount(int scenarioId)
        {
            if (!isThirdLoop) return 1; // 1周目
            
            // 2周目以降の場合、そのシナリオを2周目でクリアした回数を取得
            if (scenarioThirdLoopCounts.ContainsKey(scenarioId))
            {
                // 2周目でクリアした回数が1回 = 3周目、2回 = 4周目、というように増えていく
                return scenarioThirdLoopCounts[scenarioId] + 2;
            }
            
            // 2周目でまだクリアしていない場合は2周目
            return 2;
        }


        /// <summary>
        /// シナリオ6が解放されたかどうか（演出用）をチェックし、フラグを更新する
        /// </summary>
        /// <returns>今回初めて解放された場合はtrue</returns>
        public bool CheckAndConsumeScenario6Unlocked()
        {
            if (CanAccessScenario(6) && !isScenario6Unlocked)
            {
                isScenario6Unlocked = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// シナリオ6の解放演出がすでに実行されたかどうか（UIManager用）
        /// </summary>
        public bool IsScenario6Unlocked()
        {
            return isScenario6Unlocked;
        }

        public void SetIsScenario6Unlocked(bool value)
        {
            isScenario6Unlocked = value;
        }

        public void SetIsDarkMode(bool value)
        {
            isDarkMode = value;
        }

        /// <summary>
        /// Divisionのクリア状況を取得
        /// </summary>
        public bool IsChapterCleared(string chapterId)
        {
            if (ChapterManager.Instance != null)
            {
                return ChapterManager.Instance.IsChapterCleared(chapterId);
            }
            else
            {
                // フォールバック（ChapterManagerがない場合）
                if (debugShowAllChapters) return true;
                return clearedChapters.Contains(chapterId);
            }
        }


        public bool AreAllLettersLost()
        {
            if (isThirdLoop) return false;
            if (!IsDarkMode()) return false;
            
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
            startedScenariosInDarkMode.Clear(); // 3周目開始時は開始したシナリオもクリア
            scenarioResults.Clear();
            collectedLetters.Clear();
            restoredLetters.Clear();
            lastLostLetters.Clear();
            seenEndsByMode.Clear();
            currentScenarioIndex = -1;
            isDarkMode = false; // 2周目開始時は破損状態をリセットし、伏字のみの状態にする
            isThirdLoop = true; // 2周目開始（isThirdLoop == true の時点で2周目）
            pendingDarkMode = false;
            // scenarioThirdLoopCountsはクリアしない（各シナリオの3周目以降のカウントを維持）
            
            // Overlayイベント発火（モード変更）
            OverlayEventHub.Publish(new ModeChangedEvt(GameMode.Third));
            
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

        public HashSet<int> GetCompletedScenarios()
        {
            return new HashSet<int>(completedScenarios);
        }

        public HashSet<char> GetRestoredLetters()
        {
            return new HashSet<char>(restoredLetters);
        }

        /// <summary>
        /// シナリオの2周目以降のクリア回数を取得
        /// </summary>
        /// <param name="scenarioId">シナリオID</param>
        /// <returns>2周目以降のクリア回数</returns>
        public int GetScenarioThirdLoopCount(int scenarioId)
        {
            if (scenarioThirdLoopCounts.ContainsKey(scenarioId))
            {
                return scenarioThirdLoopCounts[scenarioId];
            }
            return 0;
        }

        /// <summary>
        /// シナリオを強制的に完了させる（デバッグ用）
        /// </summary>
        public void ForceCompleteScenario(int scenarioId, int choiceId, bool inDarkMode)
        {
            completedScenarios.Add(scenarioId);
            if (inDarkMode)
            {
                completedScenariosInDarkMode.Add(scenarioId);
            }
            if (!inDarkMode)
            {
                score++;
            }

            // 文字を収集（シナリオ1-5のみ）
            if (MouhitotsuWordManager.IsValidScenarioId(scenarioId))
            {
                char collectedLetter = MouhitotsuWordManager.GetLetterByScenarioId(scenarioId);
                if (collectedLetter != '\0')
                {
                    collectedLetters.Add(collectedLetter);
                }
            }

            scenarioResults[scenarioId] = new ScenarioResult
            {
                hasWord = true,
                choiceId = choiceId,
                epilogue = "",
                epilogue2 = "",
                scoreAtCompletion = score
            };
        }

        /// <summary>
        /// ダークモードで失われた文字を取得
        /// </summary>
        public HashSet<char> GetLostLetters()
        {
            HashSet<char> lostLetters = new HashSet<char>();
            char[] allLetters = MouhitotsuWordManager.GetAllLetters();

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

            // ダークモード中に開始したシナリオ（1〜5）に対応する文字を失われた文字に加える
            // シナリオを開始したタイミングで文字を失う
            for (int i = 1; i <= 5; i++)
            {
                if (startedScenariosInDarkMode.Contains(i))
                {
                    char letter = MouhitotsuWordManager.GetLetterByScenarioId(i);
                    if (letter != '\0')
                    {
                        lostLetters.Add(letter);
                    }
                }
            }

            // ダークモード中に完了したシナリオ（1〜5）に対応する文字も失われた文字に加える（念のため）
            for (int i = 1; i <= 5; i++)
            {
                if (completedScenariosInDarkMode.Contains(i))
                {
                    char letter = MouhitotsuWordManager.GetLetterByScenarioId(i);
                    if (letter != '\0')
                    {
                        lostLetters.Add(letter);
                    }
                }
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
            startedScenariosInDarkMode.Clear(); // リセット時は開始したシナリオもクリア
            scenarioResults.Clear();
            collectedLetters.Clear();
            restoredLetters.Clear();
            lastLostLetters.Clear();
            seenEndsByMode.Clear();
            isDarkMode = false;
            isThirdLoop = false; // 通常のリセットでは2周目フラグも落とす
            pendingDarkMode = false;
            isScenario6Unlocked = false;
            currentScenarioIndex = -1;
            hasCompletedFirstLoop = false; // リセット時は1周目クリアフラグもリセット
            scenarioThirdLoopCounts.Clear(); // リセット時は各シナリオの2周目クリア回数もリセット
            scenarioRandomData.Clear(); // リセット時はランダム要素もクリア
            gameStartTime = DateTime.Now;
            OnScoreChanged?.Invoke();
        }
        
        /// <summary>
        /// シナリオごとのランダム要素を生成して保存
        /// 既にデータが存在する場合は生成しない（一度生成したら保持）
        /// </summary>
        public void GenerateScenarioRandomData()
        {
            // 既にデータが存在する場合は生成しない
            if (scenarioRandomData.Count > 0)
            {
                return;
            }
            
            // シナリオ1: 建物名と失踪人物名
            if (!scenarioRandomData.ContainsKey(1))
            {
                scenarioRandomData[1] = new Dictionary<string, string>();
            }
            scenarioRandomData[1]["buildingName"] = BuildingNameManager.GetRandomBuildingName();
            scenarioRandomData[1]["missingPersonName"] = MissingPersonNameManager.GetRandomMissingPersonName();
            
            // シナリオ2: メニュー名（日付ベースなので毎回同じだが、念のため保存）
            if (!scenarioRandomData.ContainsKey(2))
            {
                scenarioRandomData[2] = new Dictionary<string, string>();
            }
            scenarioRandomData[2]["menuName"] = RestaurantMenuManager.GetTodayRecommendation();
            
            // シナリオ3: タイムカプセルアイテム
            if (!scenarioRandomData.ContainsKey(3))
            {
                scenarioRandomData[3] = new Dictionary<string, string>();
            }
            scenarioRandomData[3]["timeCapsuleItem"] = TimeCapsuleItemManager.GetRandomTimeCapsuleItem();
            
            // シナリオ4: 動物名
            if (!scenarioRandomData.ContainsKey(4))
            {
                scenarioRandomData[4] = new Dictionary<string, string>();
            }
            scenarioRandomData[4]["animalName"] = AnimalNameManager.GetRandomAnimalName();
            
            // シナリオ5: パズル絵の内容とリアクション
            if (!scenarioRandomData.ContainsKey(5))
            {
                scenarioRandomData[5] = new Dictionary<string, string>();
            }
            var puzzleImage = PuzzleImageManager.GetRandomPuzzleImage();
            scenarioRandomData[5]["puzzleImage"] = puzzleImage.ImageDescription;
            scenarioRandomData[5]["puzzleReaction"] = puzzleImage.Reaction;
        }
        
        /// <summary>
        /// シナリオのランダム要素を取得
        /// </summary>
        /// <param name="scenarioId">シナリオID</param>
        /// <param name="key">要素名（"buildingName", "animalName", "menuName"など）</param>
        /// <returns>ランダム要素の値。見つからない場合はnull</returns>
        public string GetScenarioRandomData(int scenarioId, string key)
        {
            if (scenarioRandomData.ContainsKey(scenarioId) && scenarioRandomData[scenarioId].ContainsKey(key))
            {
                return scenarioRandomData[scenarioId][key];
            }
            return null;
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

