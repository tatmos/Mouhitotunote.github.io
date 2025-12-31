using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// 建物名管理クラス
    /// ランダムな建物名を提供する
    /// </summary>
    public static class BuildingNameManager
    {
        private static List<string> buildingNames = null;
        private static System.Random random = new System.Random();

        /// <summary>
        /// 建物名リストを初期化
        /// </summary>
        private static void InitializeBuildingNames()
        {
            if (buildingNames != null) return;

            buildingNames = new List<string>
            {
                "旧市庁舎",
                "廃校になった小学校",
                "閉鎖された図書館",
                "取り壊し予定の病院",
                "廃墟となった工場",
                "閉店した映画館",
                "使われなくなった駅舎",
                "廃墟のホテル",
                "閉鎖された博物館",
                "取り壊された劇場",
                "廃校になった中学校",
                "閉店したデパート",
                "使われなくなった倉庫",
                "廃墟のアパート",
                "閉鎖された研究所",
                "取り壊し予定のマンション",
                "廃校になった高校",
                "閉店したレストラン",
                "使われなくなった教会",
                "廃墟の商店街",
                "閉鎖された刑務所",
                "取り壊された体育館",
                "廃校になった大学",
                "閉店したカフェ",
                "使われなくなった郵便局",
                "廃墟の銀行",
                "閉鎖された警察署",
                "取り壊し予定の消防署",
                "廃校になった専門学校",
                "閉店した書店",
                "使われなくなった美容院",
                "廃墟の理髪店",
                "閉鎖された薬局",
                "取り壊されたコンビニ",
                "廃校になった幼稚園",
                "閉店したスーパー",
                "使われなくなった銭湯",
                "廃墟のゲームセンター",
                "閉鎖されたパチンコ店",
                "取り壊し予定のボウリング場",
                "廃校になった保育園",
                "閉店した居酒屋",
                "使われなくなったバー",
                "廃墟のカラオケ",
                "閉鎖されたネットカフェ",
                "取り壊されたマンガ喫茶",
                "廃校になった予備校",
                "閉店した学習塾",
                "使われなくなった英会話学校",
                "廃墟のダンススタジオ"
            };
        }

        /// <summary>
        /// ランダムな建物名を取得
        /// </summary>
        /// <returns>ランダムに選ばれた建物名</returns>
        public static string GetRandomBuildingName()
        {
            InitializeBuildingNames();
            
            if (buildingNames == null || buildingNames.Count == 0)
            {
                return "旧市庁舎"; // フォールバック
            }

            int index = random.Next(buildingNames.Count);
            return buildingNames[index];
        }

        /// <summary>
        /// すべての建物名リストを取得（デバッグ用）
        /// </summary>
        /// <returns>建物名のリスト</returns>
        public static List<string> GetAllBuildingNames()
        {
            InitializeBuildingNames();
            return new List<string>(buildingNames);
        }
    }
}

