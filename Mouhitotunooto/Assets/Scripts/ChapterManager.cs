using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// Chapter（物語の章）を管理するクラス
    /// </summary>
    public class ChapterManager : MonoBehaviour
    {
        public static ChapterManager Instance { get; private set; }

        [Header("Debug Settings")]
        [Tooltip("デバッグモードを有効化")]
        [SerializeField] private bool debugMode = false;
        
        [Tooltip("デバッグモード時に開始するChapterを選択")]
        [SerializeField] private DebugStartChapter debugStartChapter = DebugStartChapter.None;

        /// <summary>
        /// デバッグ用の開始Chapter
        /// </summary>
        public enum DebugStartChapter
        {
            None,
            Prologue,
            PreA,
            A,
            B,
            C,
            PreD,
            D,
            E
        }

        // Chapterのクリア状況
        private HashSet<string> clearedChapters = new HashSet<string>();
        
        // 全Chapterを表示するデバッグフラグ
        [SerializeField] private bool debugShowAllChapters = false;
        
        // 現在アクティブなChapter（JumpToChapterで設定された場合）
        private string currentActiveChapter = null;

        [Header("Current Status")]
        [Tooltip("現在実行中のChapter（読み取り専用）")]
        [SerializeField, TextArea(1, 3)] private string currentChapter = "Prologue";

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
            // ゲーム開始時は必ずPrologueから開始する（clearedChaptersをクリア）
            // ただし、デバッグモードで開始Chapterが指定されている場合は、そのChapterにジャンプ
            if (debugMode && debugStartChapter != DebugStartChapter.None)
            {
                string chapterId = debugStartChapter.ToString();
                if (chapterId == "Prologue")
                {
                    chapterId = "Prologue";
                }
                JumpToChapter(chapterId);
                Debug.Log($"[ChapterManager] デバッグモード: Chapter {chapterId} から開始します。");
            }
            else
            {
                // デバッグモードでない場合、またはデバッグモードでNoneが指定されている場合は、Prologueから開始
                // clearedChaptersをクリアして、初期状態にする
                clearedChapters.Clear();
                currentActiveChapter = null;
                Debug.Log("[ChapterManager] ゲームを初期状態（Prologue）にリセットしました。");
            }
        }

        private void Update()
        {
            // 実行中に現在のChapterを更新
            UpdateCurrentChapterDisplay();
        }

        /// <summary>
        /// 現在のChapterを判定して表示を更新
        /// </summary>
        private void UpdateCurrentChapterDisplay()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                currentChapter = "GameManager not found";
                return;
            }

            string chapter = GetCurrentChapter(gameManager);
            currentChapter = $"Current Chapter: {chapter}\n" +
                             $"Cleared Chapters: {string.Join(", ", clearedChapters)}";
        }

        /// <summary>
        /// 現在のChapterを取得
        /// </summary>
        public string GetCurrentChapter(GameManager gameManager = null)
        {
            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }

            if (gameManager == null)
            {
                return "Unknown";
            }

            // JumpToChapterで設定されたChapterがある場合は、それを優先的に返す
            if (!string.IsNullOrEmpty(currentActiveChapter))
            {
                return currentActiveChapter;
            }

            // チートモードが有効な場合でも、実際にクリアされたチャプターを優先的に返す
            // （debugShowAllChaptersが有効でも、実際の進行状況を反映する）
            
            // Chapter Eがクリアされている（実際にクリアされた場合のみ）
            if (clearedChapters.Contains("E"))
            {
                return "E";
            }

            // Chapter Dがクリアされている（実際にクリアされた場合のみ）
            if (clearedChapters.Contains("D"))
            {
                return "D";
            }

            // PreD（3周目で真実の扉が開いた）がクリアされている（実際にクリアされた場合のみ）
            if (clearedChapters.Contains("PreD"))
            {
                return "PreD";
            }

            // Chapter Cがクリアされている（実際にクリアされた場合のみ）
            if (clearedChapters.Contains("C"))
            {
                return "C";
            }

            // Chapter Bがクリアされている（実際にクリアされた場合のみ）
            if (clearedChapters.Contains("B"))
            {
                return "B";
            }

            // Chapter Aがクリアされている（実際にクリアされた場合のみ）
            if (clearedChapters.Contains("A"))
            {
                return "A";
            }

            // PreA（真実の扉が開いた）がクリアされている（実際にクリアされた場合のみ）
            if (clearedChapters.Contains("PreA"))
            {
                return "PreA";
            }

            // まだChapter PreAに到達していない
            return "Prologue";
        }

        /// <summary>
        /// Chapterの判定を行い、新しく到達した場合はログを出力して保存する
        /// </summary>
        public void UpdateAndLogChapterStatus(int scenarioId, bool playedInDarkMode, bool isActuallyDarkMode, bool isThirdLoop, int score)
        {
            if (!isThirdLoop)
            {
                // 以前の状態が通常モードだった場合
                if (!playedInDarkMode)
                {
                    if (scenarioId != 6) return;
                    if (score < 7)
                    {
                        LogChapter("A", "クリア数オーバーなしでシナリオ6クリア -> まだ、もうひとつの世界に気づいていない");
                    }
                    else
                    {
                        LogChapter("B", "クリア数オーバーありでシナリオ6クリア -> 真実の扉で不正を判定され、修正プログラムが暴走し始める（ダークモード突入）");
                    }
                }
                else if (isActuallyDarkMode)
                {
                    // Chapter C はダークモード中にシナリオ6をクリアした時点で到達
                    // ただし、ここでは明示的にログを出さない（既にBで到達しているため）
                }
            }
            else if (isThirdLoop)
            {
                if (scenarioId != 6) return;
                if (score < 7)
                {
                    LogChapter("D", "伏字モードでクリア数オーバーなしでシナリオ6クリア -> すべての文字を取り返した、エンドクレジットともうひとつの世界（ゲームから離れた現実）終焉エンド");
                }
                else
                {
                    LogChapter("E", "2週目：伏字モードでクリア数オーバーありでシナリオ6クリア -> すべての文字を取り返したが、バグも発生させた、エンドクレジットともうひとつの世界（ゲームから離れた現実）終焉エンド");
                }
            }
        }

        /// <summary>
        /// Chapterをログに記録
        /// </summary>
        public void LogChapter(string chapterId, string message)
        {
            if (!clearedChapters.Contains(chapterId))
            {
                Debug.Log($"[ChapterManager] chapter {chapterId}: {message}");
                clearedChapters.Add(chapterId);
            }
        }

        /// <summary>
        /// Chapterのクリア状況を取得
        /// </summary>
        public bool IsChapterCleared(string chapterId)
        {
            if (debugShowAllChapters) return true;
            return clearedChapters.Contains(chapterId);
        }

        /// <summary>
        /// クリア済みのChapter数を取得
        /// </summary>
        public int GetClearedChaptersCount()
        {
            return clearedChapters.Count;
        }

        /// <summary>
        /// 特定のChapterへジャンプ（デバッグ/再挑戦用）
        /// </summary>
        public void JumpToChapter(string chapterId)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[ChapterManager] GameManager.Instance が見つかりません。");
                return;
            }

            // 現在アクティブなChapterを設定
            currentActiveChapter = chapterId;

            gameManager.ResetGame();
            var scenarios = gameManager.GetScenarios();
            int totalScenarios = scenarios.Count;

            switch (chapterId)
            {
                case "Prologue":
                    Debug.Log("[ChapterManager] プロローグを開始します。");
                    gameManager.SetGameStartTime(DateTime.Now);
                    break;
                case "PreA":
                    // 真実の扉が開いた状態
                    LogChapter("PreA", "真実の扉が開いた（シナリオ1-5をクリア）");
                    for (int i = 1; i <= 5; i++)
                    {
                        gameManager.ForceCompleteScenario(i, 1, false);
                    }
                    // 演出を表示するため、isScenario6Unlockedをfalseに設定
                    // ShowSelectionScreenでCheckAndConsumeScenario6Unlocked()がtrueを返すようにする
                    gameManager.SetIsScenario6Unlocked(false);
                    break;
                case "A":
                    // 通常モード、未クリア状態
                    LogChapter("A", "Chapter A を開始します（手動ジャンプ）。");
                    break;
                case "B":
                    // 通常モード、全クリア状態
                    LogChapter("B", "Chapter B を開始します（手動ジャンプ）。");
                    for (int i = 1; i <= totalScenarios; i++)
                    {
                        gameManager.ForceCompleteScenario(i, 1, false);
                    }
                    gameManager.SetIsDarkMode(false);
                    gameManager.SetIsScenario6Unlocked(true);
                    break;
                case "C":
                    // ダークモード、全文字消失直前
                    LogChapter("C", "Chapter C を開始します（手動ジャンプ）。");
                    gameManager.SetIsDarkMode(true);
                    gameManager.SetIsScenario6Unlocked(true);
                    for (int i = 1; i <= 5; i++)
                    {
                        gameManager.ForceCompleteScenario(i, 1, true);
                    }
                    gameManager.ForceCompleteScenario(6, 1, false);
                    break;
                case "PreD":
                    // 3周目で真実の扉が開いた状態
                    LogChapter("PreD", "真実の扉が開いた（3周目でシナリオ1-5をクリア）");
                    gameManager.TriggerThirdLoop();
                    for (int i = 1; i <= 5; i++)
                    {
                        gameManager.ForceCompleteScenario(i, 1, false);
                    }
                    // 演出を表示するため、isScenario6Unlockedをfalseに設定
                    // ShowSelectionScreenでCheckAndConsumeScenario6Unlocked()がtrueを返すようにする
                    gameManager.SetIsScenario6Unlocked(false);
                    break;
                case "D":
                    // 3周目、開始状態
                    LogChapter("D", "Chapter D を開始します（手動ジャンプ）。");
                    gameManager.SetIsScenario6Unlocked(true);
                    gameManager.TriggerThirdLoop();
                    break;
                case "E":
                    // 3周目、全文字復活直前
                    LogChapter("E", "Chapter E を開始します（手動ジャンプ）。");
                    gameManager.SetIsScenario6Unlocked(true);
                    gameManager.TriggerThirdLoop();
                    for (int i = 1; i <= 5; i++)
                    {
                        gameManager.ForceCompleteScenario(i, 1, false);
                        char letter = MouhitotsuWordManager.GetLetterByScenarioId(i);
                        if (letter != '\0')
                        {
                            gameManager.RestoreLetter(letter);
                        }
                    }
                    break;
            }
            gameManager.NotifyScoreChanged();
            Debug.Log($"[ChapterManager] Chapter {chapterId} へジャンプしました。");
        }

        /// <summary>
        /// 物語の解明度（パーセンテージ）を取得
        /// </summary>
        public int GetStoryProgressPercentage(int score, HashSet<int> completedScenarios, HashSet<char> restoredLetters)
        {
            float percentage = 0;

            // 各チャプターの基本進捗 (計 6段階想定)
            // PreA: 10%, A: 20%, B: 40%, C: 60%, D: 80%, E: 100%
            
            // 1. Chapter PreA 以前 (通常クリア)
            if (!IsChapterCleared("PreA"))
            {
                // シナリオ1-5のクリア状況 (最大5つ)
                int completed = 0;
                for (int i = 1; i <= 5; i++)
                {
                    if (completedScenarios.Contains(i)) completed++;
                }
                percentage = (completed / 5f) * 10f; // PreA到達まで10%
            }
            else if (!IsChapterCleared("A"))
            {
                // PreA到達済み、Chapter A 以前
                percentage = 10f; // PreA到達で10%
                // シナリオ6のクリア状況
                if (completedScenarios.Contains(6))
                {
                    percentage = 20f; // Chapter A到達で20%
                }
            }
            else if (!IsChapterCleared("B"))
            {
                percentage = 20f;
            }
            else if (!IsChapterCleared("C"))
            {
                // 2. Chapter B 以前 (スコア7以上、不正発覚)
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
            else if (!IsChapterCleared("PreD"))
            {
                // 3. Chapter C (3周目、文字の復元)
                percentage = 60f;
                
                // 復元した文字数 (最大5つ)
                float subProgress = Mathf.Clamp01(restoredLetters.Count / 5f);
                // PreD到達まで80%
                percentage += subProgress * 20f;
            }
            else if (!IsChapterCleared("D") && !IsChapterCleared("E"))
            {
                // 4. PreD到達済み、Chapter D/E 以前
                percentage = 80f;
            }
            else
            {
                // 5. 最終段階
                // Chapter D/E クリア
                percentage = 80f;
                
                // Dのみクリア（不正なしなら）
                if (IsChapterCleared("D") && !IsChapterCleared("E"))
                {
                    percentage = 100f;
                }
                else if (IsChapterCleared("E"))
                {
                    percentage = 100f;
                }
            }

            return Mathf.Clamp((int)percentage, 0, 100);
        }

        /// <summary>
        /// Chapterのクリア状況をリセット
        /// </summary>
        public void ResetChapters()
        {
            clearedChapters.Clear();
            currentActiveChapter = null;
        }

        /// <summary>
        /// デバッグモードで全Chapterを表示するかどうかを設定
        /// </summary>
        public void SetDebugShowAllChapters(bool enabled)
        {
            debugShowAllChapters = enabled;
        }

        /// <summary>
        /// デバッグモードで全Chapterを表示するかどうかを取得
        /// </summary>
        public bool GetDebugShowAllChapters()
        {
            return debugShowAllChapters;
        }
    }
}

