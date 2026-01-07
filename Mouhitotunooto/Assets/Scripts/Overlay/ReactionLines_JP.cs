using System.Collections.Generic;

namespace NovelGame.Overlay
{
    /// <summary>
    /// セリフ辞書（日本語）
    /// </summary>
    public static class ReactionLines_JP
    {
        /// <summary>
        /// プレイヤータイプ別セリフを取得
        /// </summary>
        public static string GetLine(string eventKey, PlayerType playerType, bool useCommon = true)
        {
            var lines = GetLines(eventKey, playerType);
            if (lines == null || lines.Count == 0) return null;
            
            // ランダムに1つ選択
            int index = UnityEngine.Random.Range(0, lines.Count);
            return lines[index];
        }

        /// <summary>
        /// プレイヤータイプ別セリフリストを取得
        /// </summary>
        public static List<string> GetLines(string eventKey, PlayerType playerType)
        {
            // 共通セリフを取得
            if (commonLines.ContainsKey(eventKey))
            {
                var common = commonLines[eventKey];
                if (common != null && common.Count > 0)
                {
                    return common;
                }
            }

            // タイプ別セリフを取得
            string typeKey = $"{eventKey}_{playerType}";
            if (typeLines.ContainsKey(typeKey))
            {
                return typeLines[typeKey];
            }

            return null;
        }

        // 共通セリフ
        private static readonly Dictionary<string, List<string>> commonLines = new Dictionary<string, List<string>>
        {
            ["DivisionB_Presence"] = new List<string>
            {
                "……今、空気変わったよね。",
                "あ、これ\"戻れない\"やつだ。"
            },
            ["DarkStart_Active"] = new List<string>
            {
                "うわ、画面が\"直されてる\"。",
                "ねえ、今の音…混ざったよね。"
            },
            ["MouhitotuSuccess_First"] = new List<string>
            {
                "今の、取った。……取れた。",
                "見つけた…たぶん、見つけた。"
            },
            ["DivisionC_Active"] = new List<string>
            {
                "……あ、落ちる。これ落ちる。",
                "強制再起動って、言ったよね今。"
            },
            ["ThirdStart_Active"] = new List<string>
            {
                "最初から伏字…徹底してる。",
                "UIまで隠すの、やりすぎだって。"
            },
            ["DivisionD_Quiet"] = new List<string>
            {
                "……ここまで戻せたんだ。",
                "うん。これで、たぶん。"
            },
            ["DivisionE_Quiet"] = new List<string>
            {
                "……まだ触るの？",
                "やめとけ、って言うべきかな。……でも。"
            }
        };

        // タイプ別セリフ
        private static readonly Dictionary<string, List<string>> typeLines = new Dictionary<string, List<string>>
        {
            // Division B - FastClicker
            ["DivisionB_Presence_FastClicker"] = new List<string>
            {
                "はい出た。取りすぎ警察。"
            },
            // Division B - CarefulReader
            ["DivisionB_Presence_CarefulReader"] = new List<string>
            {
                "……修正、走った。たぶん。"
            },
            // Division B - Explorer
            ["DivisionB_Presence_Explorer"] = new List<string>
            {
                "これ、裏側に踏み込めたってこと？"
            },
            // Dark Start - FastClicker
            ["DarkStart_Active_FastClicker"] = new List<string>
            {
                "スコア増えないの、ケチすぎ。"
            },
            // Dark Start - CarefulReader
            ["DarkStart_Active_CarefulReader"] = new List<string>
            {
                "\"正しい遊び方\"に戻されてる感じがする。"
            },
            // Dark Start - Explorer
            ["DarkStart_Active_Explorer"] = new List<string>
            {
                "じゃあ、逆に壊していこっか。"
            },
            // Mouhitotu Success - FastClicker
            ["MouhitotuSuccess_First_FastClicker"] = new List<string>
            {
                "そこで反応するの、素直すぎ。"
            },
            // Mouhitotu Success - CarefulReader
            ["MouhitotuSuccess_First_CarefulReader"] = new List<string>
            {
                "文字、戻る…？　いや、まだか。"
            },
            // Mouhitotu Success - Explorer
            ["MouhitotuSuccess_First_Explorer"] = new List<string>
            {
                "このバグ、育てられるかも。"
            },
            // Division C - FastClicker
            ["DivisionC_Active_FastClicker"] = new List<string>
            {
                "修正って言いながら破壊してない？"
            },
            // Division C - CarefulReader
            ["DivisionC_Active_CarefulReader"] = new List<string>
            {
                "これは\"戻す\"じゃない。\"作り直す\"だ。"
            },
            // Division C - Explorer
            ["DivisionC_Active_Explorer"] = new List<string>
            {
                "じゃ、2周目じゃなくて…3周目？"
            },
            // Third Start - CarefulReader
            ["ThirdStart_Active_CarefulReader"] = new List<string>
            {
                "復活条件…\"見つける\"か。"
            },
            // Third Start - Explorer
            ["ThirdStart_Active_Explorer"] = new List<string>
            {
                "拾い直すフェーズだね。回収しよ。"
            }
        };
    }
}

