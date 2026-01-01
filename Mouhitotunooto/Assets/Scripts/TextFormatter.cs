using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// テキストのアレンジ処理（文字の色付け、伏字化など）を管理するクラス
    /// </summary>
    public static class TextFormatter
    {
        /// <summary>
        /// テキストをアレンジ（取得した文字に色を付け、失われた文字を伏字化）
        /// </summary>
        /// <param name="text">元のテキスト</param>
        /// <param name="collectedLetters">取得済みの文字のセット</param>
        /// <param name="lostLetters">失われた文字のセット</param>
        /// <param name="useRichText">リッチテキストを使用するかどうか（UI Toolkitの場合はtrue）</param>
        /// <returns>アレンジ後のテキスト</returns>
        public static string FormatText(string text, HashSet<char> collectedLetters, HashSet<char> lostLetters, bool useRichText = true)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (!useRichText)
            {
                // リッチテキストを使用しない場合は、伏字化のみ
                return ReplaceLostLetters(text, lostLetters);
            }

            // リッチテキストを使用する場合
            StringBuilder result = new StringBuilder();
            char[] allLetters = MouhitotsuWordManager.GetAllLetters();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool isTargetLetter = MouhitotsuWordManager.IsValidLetter(c);

                if (isTargetLetter)
                {
                    // 失われた文字の場合は伏字化
                    if (lostLetters != null && lostLetters.Contains(c))
                    {
                        result.Append("※");
                    }
                    // 取得済みの文字の場合は色を付ける
                    else if (collectedLetters != null && collectedLetters.Contains(c))
                    {
                        int scenarioId = GetScenarioIdByLetter(c);
                        Color letterColor = GetLetterColor(scenarioId);
                        string colorTag = ColorToHex(letterColor);
                        result.Append($"<color=#{colorTag}>{c}</color>");
                    }
                    else
                    {
                        // まだ取得していない文字はそのまま
                        result.Append(c);
                    }
                }
                else
                {
                    // 対象外の文字はそのまま
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 失われた文字を伏字（※）に置き換える
        /// </summary>
        /// <param name="text">元のテキスト</param>
        /// <param name="lostLetters">失われた文字のセット</param>
        /// <returns>置換後のテキスト</returns>
        public static string ReplaceLostLetters(string text, HashSet<char> lostLetters)
        {
            if (string.IsNullOrEmpty(text) || lostLetters == null || lostLetters.Count == 0)
            {
                return text;
            }

            string result = text;
            foreach (char lostLetter in lostLetters)
            {
                if (MouhitotsuWordManager.IsValidLetter(lostLetter))
                {
                    result = result.Replace(lostLetter.ToString(), "※");
                }
            }
            return result;
        }

        /// <summary>
        /// 取得した文字に色を付ける（リッチテキスト形式）
        /// </summary>
        /// <param name="text">元のテキスト</param>
        /// <param name="collectedLetters">取得済みの文字のセット</param>
        /// <returns>色付け後のテキスト</returns>
        public static string ApplyLetterColors(string text, HashSet<char> collectedLetters)
        {
            if (string.IsNullOrEmpty(text) || collectedLetters == null || collectedLetters.Count == 0)
            {
                return text;
            }

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (MouhitotsuWordManager.IsValidLetter(c) && collectedLetters.Contains(c))
                {
                    int scenarioId = GetScenarioIdByLetter(c);
                    Color letterColor = GetLetterColor(scenarioId);
                    string colorTag = ColorToHex(letterColor);
                    result.Append($"<color=#{colorTag}>{c}</color>");
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 文字からシナリオIDを取得
        /// </summary>
        /// <param name="letter">文字</param>
        /// <returns>シナリオID（1-5）。無効な文字の場合は0</returns>
        private static int GetScenarioIdByLetter(char letter)
        {
            char[] allLetters = MouhitotsuWordManager.GetAllLetters();
            for (int i = 0; i < allLetters.Length; i++)
            {
                if (allLetters[i] == letter)
                {
                    return i + 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// シナリオIDから文字の色を取得
        /// </summary>
        /// <param name="scenarioId">シナリオID（1-5）</param>
        /// <returns>文字の色。無効なIDの場合は白</returns>
        private static Color GetLetterColor(int scenarioId)
        {
            var profile = CharacterProfileManager.GetProfile(scenarioId);
            if (profile != null)
            {
                // profileColorを少し濃くして、文字が読みやすくする
                Color baseColor = profile.profileColor;
                // RGB値を少し暗くしてコントラストを上げる
                return new Color(
                    Mathf.Clamp01(baseColor.r * 0.7f),
                    Mathf.Clamp01(baseColor.g * 0.7f),
                    Mathf.Clamp01(baseColor.b * 0.7f),
                    1f
                );
            }
            return Color.white;
        }

        /// <summary>
        /// Colorを16進数文字列に変換（リッチテキスト用）
        /// </summary>
        /// <param name="color">色</param>
        /// <returns>16進数文字列（例: "FF0000"）</returns>
        private static string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// テキスト内の「もうひとつ」という文字列をアレンジ（取得した文字に色を付け、失われた文字を伏字化）
        /// </summary>
        /// <param name="text">元のテキスト</param>
        /// <param name="collectedLetters">取得済みの文字のセット</param>
        /// <param name="lostLetters">失われた文字のセット</param>
        /// <param name="useRichText">リッチテキストを使用するかどうか</param>
        /// <returns>アレンジ後のテキスト</returns>
        public static string FormatMouhitotsuWord(string text, HashSet<char> collectedLetters, HashSet<char> lostLetters, bool useRichText = true)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // 「もうひとつ」という文字列を検索して置き換え
            string word = MouhitotsuWordManager.GetWord();
            string formattedWord = MouhitotsuWordManager.GetFormattedWord();

            // まず「【もうひとつ】」を処理
            if (text.Contains(formattedWord))
            {
                string formatted = FormatWord(formattedWord, collectedLetters, lostLetters, useRichText);
                text = text.Replace(formattedWord, formatted);
            }

            // 次に「もうひとつ」を処理（【】なし）
            if (text.Contains(word))
            {
                string formatted = FormatWord(word, collectedLetters, lostLetters, useRichText);
                text = text.Replace(word, formatted);
            }

            return text;
        }

        /// <summary>
        /// 単語（「もうひとつ」）をアレンジ
        /// </summary>
        /// <param name="word">単語</param>
        /// <param name="collectedLetters">取得済みの文字のセット</param>
        /// <param name="lostLetters">失われた文字のセット</param>
        /// <param name="useRichText">リッチテキストを使用するかどうか</param>
        /// <returns>アレンジ後の単語</returns>
        private static string FormatWord(string word, HashSet<char> collectedLetters, HashSet<char> lostLetters, bool useRichText)
        {
            StringBuilder result = new StringBuilder();
            char[] allLetters = MouhitotsuWordManager.GetAllLetters();

            // 「【」や「】」などの記号を保持しながら処理
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                bool isTargetLetter = MouhitotsuWordManager.IsValidLetter(c);

                if (isTargetLetter)
                {
                    // 失われた文字の場合は伏字化
                    if (lostLetters != null && lostLetters.Contains(c))
                    {
                        result.Append("※");
                    }
                    // 取得済みの文字の場合は色を付ける
                    else if (collectedLetters != null && collectedLetters.Contains(c))
                    {
                        if (useRichText)
                        {
                            int scenarioId = GetScenarioIdByLetter(c);
                            Color letterColor = GetLetterColor(scenarioId);
                            string colorTag = ColorToHex(letterColor);
                            result.Append($"<color=#{colorTag}>{c}</color>");
                        }
                        else
                        {
                            result.Append(c);
                        }
                    }
                    else
                    {
                        // まだ取得していない文字はそのまま
                        result.Append(c);
                    }
                }
                else
                {
                    // 記号などはそのまま
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}

