using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// シナリオの選択肢を提供するクラス
    /// ダークモード時の選択肢変更ロジックを含む
    /// </summary>
    public static class ScenarioChoiceProvider
    {
        /// <summary>
        /// シナリオIDとモードに応じた選択肢を取得
        /// </summary>
        /// <param name="scenario">元のシナリオ</param>
        /// <param name="isDarkMode">ダークモードかどうか</param>
        /// <returns>選択肢のリスト</returns>
        public static List<Choice> GetChoices(Scenario scenario, bool isDarkMode)
        {
            if (isDarkMode)
            {
                return GetDarkModeChoices(scenario.id);
            }
            return scenario.choices;
        }

        /// <summary>
        /// ダークモード時の選択肢を取得（2回目の伏字）
        /// </summary>
        private static List<Choice> GetDarkModeChoices(int scenarioId)
        {
            return scenarioId switch
            {
                1 => new List<Choice>
                {
                    new Choice { id = 1, text = "「何かがおかしい...」と感じる", preview = "私：「も...もど...」" },
                    new Choice { id = 2, text = "「何が起きているのか」確認する", preview = "私：「この世界は...何が起きている...」" }
                },
                2 => new List<Choice>
                {
                    new Choice { id = 1, text = "「何も見えない...」と混乱する", preview = "私：「う...うみ...？」" },
                    new Choice { id = 2, text = "「何か間違いがある」と気づく", preview = "私：「データが...崩壊している...」" }
                },
                3 => new List<Choice>
                {
                    new Choice { id = 1, text = "「何か思い出せない...」と不安になる", preview = "私：「ひ...ひろ...？」" },
                    new Choice { id = 2, text = "「何かが欠けている」と感じる", preview = "私：「過去のデータが...消えていく...」" }
                },
                4 => new List<Choice>
                {
                    new Choice { id = 1, text = "「何も起こらない...」と困惑する", preview = "私：「と...とおる...？」" },
                    new Choice { id = 2, text = "「何かが壊れている」と理解する", preview = "私：「コードが...エラーを起こしている...」" }
                },
                5 => new List<Choice>
                {
                    new Choice { id = 1, text = "「何かが足りない...」と焦る", preview = "私：「つ...つばさ...？」" },
                    new Choice { id = 2, text = "「何も完成しない」と諦める", preview = "私：「永遠に...完成できない...」" }
                },
                6 => new List<Choice>
                {
                    new Choice { id = 1, text = "「申し訳ありません...」と詫びる", preview = "私：「壊してしまって..." },
                    new Choice { id = 2, text = "「これはどういうことですか？」と尋ねる", preview = "私：「この世界は..." }
                },
                _ => new List<Choice>()
            };
        }
    }
}

