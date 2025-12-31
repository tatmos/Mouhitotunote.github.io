using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// レストランのメニュー管理クラス
    /// 日付に基づいて旬の料理名を提供する
    /// </summary>
    public static class RestaurantMenuManager
    {
        /// <summary>
        /// 季節を表す列挙型
        /// </summary>
        public enum Season
        {
            Spring,   // 春（3-5月）
            Summer,   // 夏（6-8月）
            Autumn,   // 秋（9-11月）
            Winter    // 冬（12-2月）
        }

        /// <summary>
        /// 現在の日付に基づいて季節を取得
        /// </summary>
        public static Season GetCurrentSeason()
        {
            DateTime now = DateTime.Now;
            int month = now.Month;

            if (month >= 3 && month <= 5)
                return Season.Spring;
            else if (month >= 6 && month <= 8)
                return Season.Summer;
            else if (month >= 9 && month <= 11)
                return Season.Autumn;
            else
                return Season.Winter;
        }

        /// <summary>
        /// 現在の日付に基づいて「本日のおすすめ」の料理名を取得
        /// </summary>
        public static string GetTodayRecommendation()
        {
            DateTime now = DateTime.Now;
            Season season = GetCurrentSeason();
            int month = now.Month;
            int day = now.Day;

            // 特定の日付に特別な料理を設定
            string specialDish = GetSpecialDateDish(month, day);
            if (!string.IsNullOrEmpty(specialDish))
            {
                return specialDish;
            }

            // 季節に応じた料理をランダムに選択
            List<string> seasonalDishes = GetSeasonalDishes(season);
            if (seasonalDishes.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, seasonalDishes.Count);
                return seasonalDishes[randomIndex];
            }

            // フォールバック
            return "シェフの特選コース";
        }

        /// <summary>
        /// 特定の日付に特別な料理を返す
        /// </summary>
        private static string GetSpecialDateDish(int month, int day)
        {
            // 正月関連
            if (month == 1)
            {
                if (day >= 1 && day <= 3) return "おせち料理";
                if (day == 7) return "七草がゆ";
                if (day == 15) return "小豆がゆ";
            }

            // 節分
            if (month == 2 && day == 3) return "恵方巻き";

            // ひな祭り
            if (month == 3 && day == 3) return "ちらし寿司";

            // 花見
            if (month == 3 && day >= 20 && day <= 31) return "花見弁当";
            if (month == 4 && day >= 1 && day <= 10) return "花見弁当";

            // こどもの日
            if (month == 5 && day == 5) return "柏餅";

            // 七夕
            if (month == 7 && day == 7) return "そうめん";

            // お盆
            if (month == 8 && day >= 13 && day <= 16) return "精進料理";

            // 月見
            if (month == 9 && day == 15) return "月見団子";
            if (month == 10 && day >= 1 && day <= 3) return "月見団子";

            // ハロウィン
            if (month == 10 && day == 31) return "かぼちゃ料理";

            // 七五三
            if (month == 11 && day == 15) return "千歳飴";

            // クリスマス
            if (month == 12 && day == 24) return "クリスマスケーキ";
            if (month == 12 && day == 25) return "クリスマスディナー";

            // 大晦日
            if (month == 12 && day == 31) return "年越しそば";

            return null;
        }

        /// <summary>
        /// 季節に応じた料理リストを取得
        /// </summary>
        private static List<string> GetSeasonalDishes(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return GetSpringDishes();
                case Season.Summer:
                    return GetSummerDishes();
                case Season.Autumn:
                    return GetAutumnDishes();
                case Season.Winter:
                    return GetWinterDishes();
                default:
                    return new List<string>();
            }
        }

        /// <summary>
        /// 春の料理リスト
        /// </summary>
        private static List<string> GetSpringDishes()
        {
            return new List<string>
            {
                "桜エビの天ぷら",
                "たけのこご飯",
                "菜の花のおひたし",
                "わかめと新玉ねぎのサラダ",
                "さくらもち",
                "いちごのショートケーキ",
                "新じゃがのポテトサラダ",
                "アスパラガスのバター炒め",
                "さやえんどうの和え物",
                "ふきのとうの天ぷら",
                "新玉ねぎのサラダ",
                "春キャベツのサラダ",
                "うどとわかめの味噌汁",
                "さやいんげんの胡麻和え",
                "新じゃがのコロッケ",
                "たけのこの土佐煮",
                "菜の花のからし和え",
                "いちご大福",
                "桜餅",
                "新茶を使った茶碗蒸し",
                "春菊のおひたし",
                "さやえんどうの炒め物",
                "新じゃがのマッシュポテト",
                "たけのこの木の芽和え",
                "わかめと豆腐の味噌汁",
                "新じゃがのバター焼き",
                "たけのこの若竹煮",
                "菜の花の辛子和え",
                "いちごのタルト",
                "桜の花びらの天ぷら",
                "新玉ねぎのスープ",
                "春キャベツのロールキャベツ",
                "さやえんどうの天ぷら",
                "ふきのとうの味噌和え",
                "新じゃがのサラダ",
                "たけのこの炊き込みご飯",
                "菜の花のパスタ",
                "いちごのムース",
                "桜の花の塩漬け",
                "新玉ねぎのマリネ",
                "春キャベツのサラダ",
                "さやえんどうの煮物",
                "ふきのとうの炒め物",
                "新じゃがのフライ",
                "たけのこの天ぷら",
                "菜の花のパンケーキ",
                "いちごのアイスクリーム",
                "桜の花のクレープ",
                "新玉ねぎのグラタン",
                "春キャベツのスープ",
                "さやえんどうのサラダ",
                "ふきのとうの天ぷら",
                "新じゃがのチップス"
            };
        }

        /// <summary>
        /// 夏の料理リスト
        /// </summary>
        private static List<string> GetSummerDishes()
        {
            return new List<string>
            {
                "冷やし中華",
                "そうめん",
                "冷やしトマト",
                "きゅうりの浅漬け",
                "トマトとモッツァレラのサラダ",
                "かき氷",
                "冷やし茶碗蒸し",
                "なすの揚げびたし",
                "オクラのネバネバサラダ",
                "ゴーヤチャンプルー",
                "冷やしうどん",
                "トマトの冷製スープ",
                "きゅうりとわかめの酢の物",
                "枝豆",
                "冷やしわかめ",
                "なすの味噌炒め",
                "トマトとバジルのパスタ",
                "冷やし豆腐",
                "きゅうりのもろみ和え",
                "オクラの天ぷら",
                "ゴーヤの天ぷら",
                "なすの田楽",
                "トマトとキュウリのサラダ",
                "冷やしそうめん",
                "きゅうりのピクルス",
                "トマトのカプレーゼ",
                "なすのラタトゥイユ",
                "オクラのサラダ",
                "ゴーヤのサラダ",
                "きゅうりのサラダ",
                "トマトのサラダ",
                "なすのマリネ",
                "オクラの和え物",
                "ゴーヤの和え物",
                "きゅうりの和え物",
                "トマトの和え物",
                "なすの煮物",
                "オクラの煮物",
                "ゴーヤの煮物",
                "きゅうりの煮物",
                "トマトの煮物",
                "なすのスープ",
                "オクラのスープ",
                "ゴーヤのスープ",
                "きゅうりのスープ",
                "トマトのスープ",
                "なすのグラタン",
                "オクラのグラタン",
                "ゴーヤのグラタン",
                "きゅうりのグラタン",
                "トマトのグラタン"
            };
        }

        /// <summary>
        /// 秋の料理リスト
        /// </summary>
        private static List<string> GetAutumnDishes()
        {
            return new List<string>
            {
                "きのこご飯",
                "さつまいもの天ぷら",
                "栗ご飯",
                "さんまの塩焼き",
                "秋刀魚の蒲焼き",
                "さつまいものスイートポテト",
                "きのこのバター炒め",
                "かぼちゃの煮物",
                "さつまいものコロッケ",
                "栗の渋皮煮",
                "きのこの味噌汁",
                "かぼちゃのサラダ",
                "さつまいものマッシュポテト",
                "きのこのパスタ",
                "かぼちゃのスープ",
                "さつまいもの天ぷら",
                "きのこの和え物",
                "かぼちゃの天ぷら",
                "さつまいもの煮物",
                "きのこの炊き込みご飯",
                "かぼちゃのコロッケ",
                "さつまいものスイートポテト",
                "きのこのグラタン",
                "かぼちゃのプリン",
                "さつまいものケーキ",
                "きのこのリゾット",
                "さつまいものパン",
                "栗のモンブラン",
                "さんまの刺身",
                "秋刀魚の刺身",
                "きのこのスープ",
                "かぼちゃのパイ",
                "さつまいものパイ",
                "きのこのオムレツ",
                "かぼちゃのオムレツ",
                "さつまいものオムレツ",
                "きのこのドリア",
                "かぼちゃのドリア",
                "さつまいものドリア",
                "きのこのピザ",
                "かぼちゃのピザ",
                "さつまいものピザ",
                "きのこのサラダ",
                "かぼちゃのサラダ",
                "さつまいものサラダ",
                "きのこのマリネ",
                "かぼちゃのマリネ",
                "さつまいものマリネ",
                "きのこのフライ",
                "かぼちゃのフライ",
                "さつまいものフライ"
            };
        }

        /// <summary>
        /// 冬の料理リスト
        /// </summary>
        private static List<string> GetWinterDishes()
        {
            return new List<string>
            {
                "おでん",
                "鍋料理",
                "白菜の鍋",
                "大根の煮物",
                "かぶの煮物",
                "にんじんのグラッセ",
                "ブロッコリーのサラダ",
                "白菜の漬物",
                "大根おろし",
                "かぶのサラダ",
                "にんじんのサラダ",
                "ブロッコリーの天ぷら",
                "白菜のクリーム煮",
                "大根の味噌汁",
                "かぶの味噌汁",
                "にんじんのスープ",
                "ブロッコリーのグラタン",
                "白菜の炒め物",
                "大根のサラダ",
                "かぶの煮物",
                "にんじんの煮物",
                "ブロッコリーの和え物",
                "白菜のサラダ",
                "大根の天ぷら",
                "かぶの天ぷら",
                "白菜のスープ",
                "大根のスープ",
                "かぶのスープ",
                "にんじんのスープ",
                "ブロッコリーのスープ",
                "白菜のグラタン",
                "大根のグラタン",
                "かぶのグラタン",
                "にんじんのグラタン",
                "ブロッコリーのグラタン",
                "白菜のパスタ",
                "大根のパスタ",
                "かぶのパスタ",
                "にんじんのパスタ",
                "ブロッコリーのパスタ",
                "白菜のリゾット",
                "大根のリゾット",
                "かぶのリゾット",
                "にんじんのリゾット",
                "ブロッコリーのリゾット",
                "白菜のオムレツ",
                "大根のオムレツ",
                "かぶのオムレツ",
                "にんじんのオムレツ",
                "ブロッコリーのオムレツ"
            };
        }
        
        /// <summary>
        /// 保存されたメニュー名を取得（GameManagerから）
        /// </summary>
        /// <returns>保存されたメニュー名。見つからない場合は生成してから返す</returns>
        public static string GetMenuName()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                string savedName = gameManager.GetScenarioRandomData(2, "menuName");
                if (!string.IsNullOrEmpty(savedName))
                {
                    return savedName;
                }
                // 保存されていない場合は生成してから取得
                gameManager.GenerateScenarioRandomData();
                savedName = gameManager.GetScenarioRandomData(2, "menuName");
                if (!string.IsNullOrEmpty(savedName))
                {
                    return savedName;
                }
            }
            // フォールバック：本日のおすすめを返す
            return GetTodayRecommendation();
        }
    }
}

