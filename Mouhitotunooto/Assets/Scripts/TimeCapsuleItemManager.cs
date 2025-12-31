using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// タイムカプセルのアイテム管理クラス
    /// ランダムなタイムカプセルのアイテムを提供する
    /// </summary>
    public static class TimeCapsuleItemManager
    {
        private static List<string> timeCapsuleItems = null;
        private static System.Random random = new System.Random();

        /// <summary>
        /// タイムカプセルアイテムリストを初期化
        /// </summary>
        private static void InitializeTimeCapsuleItems()
        {
            if (timeCapsuleItems != null) return;

            timeCapsuleItems = new List<string>
            {
                "壊れたキーホルダー",
                "古い写真",
                "錆びたコイン",
                "色褪せた手紙",
                "壊れた時計",
                "古いマンガ",
                "色褪せた切手",
                "壊れたおもちゃ",
                "古いペン",
                "錆びた鍵",
                "色褪せた絵",
                "壊れたCD",
                "古いノート",
                "錆びたメダル",
                "色褪せたポスター",
                "壊れたカメラ",
                "古い手帳",
                "錆びたバッジ",
                "色褪せたチケット",
                "壊れたラジオ",
                "古い雑誌",
                "錆びたネックレス",
                "色褪せたカード",
                "壊れたゲーム機",
                "古い本",
                "錆びた指輪",
                "色褪せたシール",
                "壊れた携帯電話",
                "古い日記",
                "錆びたブレスレット",
                "色褪せたポストカード",
                "壊れたオルゴール",
                "古い地図",
                "錆びたペンダント",
                "色褪せたチラシ",
                "壊れたミニカー",
                "古い手紙",
                "錆びたコレクション",
                "色褪せたポスターカード",
                "壊れた人形",
                "古いアルバム",
                "錆びたアクセサリー",
                "色褪せたメモ",
                "壊れた模型",
                "古い新聞",
                "錆びた記念品",
                "色褪せたチケットスタブ",
                "壊れた楽器",
                "古い手作り品",
                "錆びたトロフィー"
            };
        }

        /// <summary>
        /// ランダムなタイムカプセルアイテムを取得
        /// </summary>
        /// <returns>ランダムに選ばれたタイムカプセルアイテム</returns>
        public static string GetRandomTimeCapsuleItem()
        {
            InitializeTimeCapsuleItems();
            
            if (timeCapsuleItems == null || timeCapsuleItems.Count == 0)
            {
                return "壊れたキーホルダー"; // フォールバック
            }

            int index = random.Next(timeCapsuleItems.Count);
            return timeCapsuleItems[index];
        }

        /// <summary>
        /// 保存されたタイムカプセルアイテムを取得（GameManagerから）
        /// </summary>
        /// <returns>保存されたタイムカプセルアイテム。見つからない場合は生成してから返す</returns>
        public static string GetTimeCapsuleItem()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                string savedItem = gameManager.GetScenarioRandomData(3, "timeCapsuleItem");
                if (!string.IsNullOrEmpty(savedItem))
                {
                    return savedItem;
                }
                // 保存されていない場合は生成してから取得
                gameManager.GenerateScenarioRandomData();
                savedItem = gameManager.GetScenarioRandomData(3, "timeCapsuleItem");
                if (!string.IsNullOrEmpty(savedItem))
                {
                    return savedItem;
                }
            }
            // フォールバック：ランダムなタイムカプセルアイテムを返す
            return GetRandomTimeCapsuleItem();
        }

        /// <summary>
        /// すべてのタイムカプセルアイテムリストを取得（デバッグ用）
        /// </summary>
        /// <returns>タイムカプセルアイテムのリスト</returns>
        public static List<string> GetAllTimeCapsuleItems()
        {
            InitializeTimeCapsuleItems();
            return new List<string>(timeCapsuleItems);
        }
    }
}

