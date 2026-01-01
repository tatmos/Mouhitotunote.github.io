using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// Division（周回管理）を管理するクラス
    /// </summary>
    public class DivisionManager : MonoBehaviour
    {
        public static DivisionManager Instance { get; private set; }

        [Header("Debug Settings")]
        [Tooltip("デバッグモードを有効化")]
        [SerializeField] private bool debugMode = false;
        
        [Tooltip("デバッグモード時に開始するDivisionを選択")]
        [SerializeField] private DebugStartDivision debugStartDivision = DebugStartDivision.None;

        /// <summary>
        /// デバッグ用の開始Division
        /// </summary>
        public enum DebugStartDivision
        {
            None,
            Prologue,
            A,
            B,
            C,
            D,
            E
        }

        // Divisionのクリア状況
        private HashSet<string> clearedDivisions = new HashSet<string>();
        
        // 全Divisionを表示するデバッグフラグ
        [SerializeField] private bool debugShowAllDivisions = false;

        [Header("Current Status")]
        [Tooltip("現在実行中のDivision（読み取り専用）")]
        [SerializeField, TextArea(1, 3)] private string currentDivision = "Prologue";

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
            // デバッグモードで開始Divisionが指定されている場合、そのDivisionにジャンプ
            if (debugMode && debugStartDivision != DebugStartDivision.None)
            {
                string divisionId = debugStartDivision.ToString();
                if (divisionId == "Prologue")
                {
                    divisionId = "Prologue";
                }
                JumpToDivision(divisionId);
                Debug.Log($"[DivisionManager] デバッグモード: Division {divisionId} から開始します。");
            }
        }

        private void Update()
        {
            // 実行中に現在のDivisionを更新
            UpdateCurrentDivisionDisplay();
        }

        /// <summary>
        /// 現在のDivisionを判定して表示を更新
        /// </summary>
        private void UpdateCurrentDivisionDisplay()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                currentDivision = "GameManager not found";
                return;
            }

            string division = GetCurrentDivision(gameManager);
            currentDivision = $"Current Division: {division}\n" +
                             $"Cleared Divisions: {string.Join(", ", clearedDivisions)}";
        }

        /// <summary>
        /// 現在のDivisionを取得
        /// </summary>
        public string GetCurrentDivision(GameManager gameManager = null)
        {
            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }

            if (gameManager == null)
            {
                return "Unknown";
            }

            // Division Eがクリアされている
            if (IsDivisionCleared("E"))
            {
                return "E";
            }

            // Division Dがクリアされている
            if (IsDivisionCleared("D"))
            {
                return "D";
            }

            // Division Cがクリアされている
            if (IsDivisionCleared("C"))
            {
                return "C";
            }

            // Division Bがクリアされている
            if (IsDivisionCleared("B"))
            {
                return "B";
            }

            // Division Aがクリアされている
            if (IsDivisionCleared("A"))
            {
                return "A";
            }

            // まだDivision Aに到達していない
            return "Prologue";
        }

        /// <summary>
        /// Divisionの判定を行い、新しく到達した場合はログを出力して保存する
        /// </summary>
        public void UpdateAndLogDivisionStatus(int scenarioId, bool playedInDarkMode, bool isActuallyDarkMode, bool isThirdLoop, int score)
        {
            if (!isThirdLoop)
            {
                // 以前の状態が通常モードだった場合
                if (!playedInDarkMode)
                {
                    if (scenarioId != 6) return;
                    if (score < 7)
                    {
                        LogDivision("A", "クリア数オーバーなしでシナリオ6クリア -> まだ、もうひとつの世界に気づいていない");
                    }
                    else
                    {
                        LogDivision("B", "クリア数オーバーありでシナリオ6クリア -> 真実の扉で不正を判定され、修正プログラムが暴走し始める（ダークモード突入）");
                    }
                }
                else if (isActuallyDarkMode)
                {
                    // Division C はダークモード中にシナリオ6をクリアした時点で到達
                    // ただし、ここでは明示的にログを出さない（既にBで到達しているため）
                }
            }
            else if (isThirdLoop)
            {
                if (scenarioId != 6) return;
                if (score < 7)
                {
                    LogDivision("D", "伏字モードでクリア数オーバーなしでシナリオ6クリア -> すべての文字を取り返した、エンドクレジットともうひとつの世界（ゲームから離れた現実）終焉エンド");
                }
                else
                {
                    LogDivision("E", "2週目：伏字モードでクリア数オーバーありでシナリオ6クリア -> すべての文字を取り返したが、バグも発生させた、エンドクレジットともうひとつの世界（ゲームから離れた現実）終焉エンド");
                }
            }
        }

        /// <summary>
        /// Divisionをログに記録
        /// </summary>
        public void LogDivision(string divisionId, string message)
        {
            if (!clearedDivisions.Contains(divisionId))
            {
                Debug.Log($"[DivisionManager] division {divisionId}: {message}");
                clearedDivisions.Add(divisionId);
            }
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
        /// クリア済みのDivision数を取得
        /// </summary>
        public int GetClearedDivisionsCount()
        {
            return clearedDivisions.Count;
        }

        /// <summary>
        /// 特定のDivisionへジャンプ（デバッグ/再挑戦用）
        /// </summary>
        public void JumpToDivision(string divisionId)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[DivisionManager] GameManager.Instance が見つかりません。");
                return;
            }

            gameManager.ResetGame();
            var scenarios = gameManager.GetScenarios();
            int totalScenarios = scenarios.Count;

            switch (divisionId)
            {
                case "Prologue":
                    Debug.Log("[DivisionManager] プロローグを開始します。");
                    gameManager.SetGameStartTime(DateTime.Now);
                    break;
                case "A":
                    // 通常モード、未クリア状態
                    LogDivision("A", "Division A を開始します（手動ジャンプ）。");
                    break;
                case "B":
                    // 通常モード、全クリア状態
                    LogDivision("B", "Division B を開始します（手動ジャンプ）。");
                    for (int i = 1; i <= totalScenarios; i++)
                    {
                        gameManager.ForceCompleteScenario(i, 1, false);
                    }
                    gameManager.SetIsDarkMode(false);
                    gameManager.SetIsScenario6Unlocked(true);
                    break;
                case "C":
                    // ダークモード、全文字消失直前
                    LogDivision("C", "Division C を開始します（手動ジャンプ）。");
                    gameManager.SetIsDarkMode(true);
                    gameManager.SetIsScenario6Unlocked(true);
                    for (int i = 1; i <= 5; i++)
                    {
                        gameManager.ForceCompleteScenario(i, 1, true);
                    }
                    gameManager.ForceCompleteScenario(6, 1, false);
                    break;
                case "D":
                    // 3周目、開始状態
                    LogDivision("D", "Division D を開始します（手動ジャンプ）。");
                    gameManager.SetIsScenario6Unlocked(true);
                    gameManager.TriggerThirdLoop();
                    break;
                case "E":
                    // 3周目、全文字復活直前
                    LogDivision("E", "Division E を開始します（手動ジャンプ）。");
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
            Debug.Log($"[DivisionManager] Division {divisionId} へジャンプしました。");
        }

        /// <summary>
        /// 物語の解明度（パーセンテージ）を取得
        /// </summary>
        public int GetStoryProgressPercentage(int score, HashSet<int> completedScenarios, HashSet<char> restoredLetters)
        {
            float percentage = 0;

            // 各ディビジョンの基本進捗 (計 5段階想定)
            // A: 20%, B: 40%, C: 60%, D: 80%, E: 100%
            
            // 1. Division A 以前 (通常クリア)
            if (!IsDivisionCleared("A"))
            {
                // シナリオ1-6のクリア状況 (最大6つ)
                int completed = 0;
                for (int i = 1; i <= 6; i++)
                {
                    if (completedScenarios.Contains(i)) completed++;
                }
                percentage = (completed / 6f) * 20f;
            }
            else if (!IsDivisionCleared("B"))
            {
                percentage = 20f;
            }
            else if (!IsDivisionCleared("C"))
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
            else if (!IsDivisionCleared("D") && !IsDivisionCleared("E"))
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
                if (IsDivisionCleared("D") && !IsDivisionCleared("E"))
                {
                    percentage = 100f;
                }
            }

            return Mathf.Clamp((int)percentage, 0, 100);
        }

        /// <summary>
        /// Divisionのクリア状況をリセット
        /// </summary>
        public void ResetDivisions()
        {
            clearedDivisions.Clear();
        }
    }
}

