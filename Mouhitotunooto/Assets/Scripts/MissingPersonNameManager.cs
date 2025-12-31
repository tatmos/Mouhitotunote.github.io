using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// 失踪人物名管理クラス
    /// ランダムな失踪人物名を提供する
    /// </summary>
    public static class MissingPersonNameManager
    {
        private static List<string> missingPersonNames = null;
        private static System.Random random = new System.Random();

        /// <summary>
        /// 失踪人物名リストを初期化
        /// </summary>
        private static void InitializeMissingPersonNames()
        {
            if (missingPersonNames != null) return;

            missingPersonNames = new List<string>
            {
                "田中",
                "佐藤",
                "鈴木",
                "高橋",
                "伊藤",
                "渡辺",
                "中村",
                "小林",
                "加藤",
                "吉田",
                "山田",
                "松本",
                "井上",
                "木村",
                "林",
                "斎藤",
                "清水",
                "山本",
                "森",
                "池田",
                "橋本",
                "前田",
                "藤田",
                "後藤",
                "石川",
                "村上",
                "近藤",
                "坂本",
                "遠藤",
                "青木",
                "藤原",
                "岡田",
                "長谷川",
                "中島",
                "田村",
                "新井",
                "原田",
                "藤井",
                "西村",
                "上田",
                "村田",
                "太田",
                "竹内",
                "金子",
                "福田",
                "中川",
                "藤本",
                "小川",
                "三浦",
                "野口"
            };
        }

        /// <summary>
        /// ランダムな失踪人物名を取得
        /// </summary>
        /// <returns>ランダムに選ばれた失踪人物名</returns>
        public static string GetRandomMissingPersonName()
        {
            InitializeMissingPersonNames();
            
            if (missingPersonNames == null || missingPersonNames.Count == 0)
            {
                return "田中"; // フォールバック
            }

            int index = random.Next(missingPersonNames.Count);
            return missingPersonNames[index];
        }

        /// <summary>
        /// 保存された失踪人物名を取得（GameManagerから）
        /// </summary>
        /// <returns>保存された失踪人物名。見つからない場合は生成してから返す</returns>
        public static string GetMissingPersonName()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                string savedName = gameManager.GetScenarioRandomData(1, "missingPersonName");
                if (!string.IsNullOrEmpty(savedName))
                {
                    return savedName;
                }
                // 保存されていない場合は生成してから取得
                gameManager.GenerateScenarioRandomData();
                savedName = gameManager.GetScenarioRandomData(1, "missingPersonName");
                if (!string.IsNullOrEmpty(savedName))
                {
                    return savedName;
                }
            }
            // フォールバック：ランダムな失踪人物名を返す
            return GetRandomMissingPersonName();
        }

        /// <summary>
        /// すべての失踪人物名リストを取得（デバッグ用）
        /// </summary>
        /// <returns>失踪人物名のリスト</returns>
        public static List<string> GetAllMissingPersonNames()
        {
            InitializeMissingPersonNames();
            return new List<string>(missingPersonNames);
        }
    }
}

