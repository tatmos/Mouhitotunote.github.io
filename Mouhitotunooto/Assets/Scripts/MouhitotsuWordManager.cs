using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// 「もうひとつ」の各文字（「も」「う」「ひ」「と」「つ」）を管理するクラス
    /// </summary>
    public static class MouhitotsuWordManager
    {
        /// <summary>
        /// 「もうひとつ」の各文字の配列
        /// </summary>
        private static readonly char[] Letters = { 'も', 'う', 'ひ', 'と', 'つ' };

        /// <summary>
        /// すべての文字を取得
        /// </summary>
        /// <returns>「も」「う」「ひ」「と」「つ」の文字配列</returns>
        public static char[] GetAllLetters()
        {
            return (char[])Letters.Clone();
        }

        /// <summary>
        /// シナリオIDから対応する文字を取得
        /// </summary>
        /// <param name="scenarioId">シナリオID（1-5）</param>
        /// <returns>対応する文字。無効なIDの場合は null 文字（'\0'）</returns>
        public static char GetLetterByScenarioId(int scenarioId)
        {
            int letterIndex = scenarioId - 1;
            if (letterIndex >= 0 && letterIndex < Letters.Length)
            {
                return Letters[letterIndex];
            }
            return '\0';
        }

        /// <summary>
        /// 文字が有効な「もうひとつ」の文字かどうかを判定
        /// </summary>
        /// <param name="letter">判定する文字</param>
        /// <returns>有効な文字の場合は true</returns>
        public static bool IsValidLetter(char letter)
        {
            foreach (char c in Letters)
            {
                if (c == letter)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// シナリオIDが有効な範囲（1-5）かどうかを判定
        /// </summary>
        /// <param name="scenarioId">シナリオID</param>
        /// <returns>有効な範囲の場合は true</returns>
        public static bool IsValidScenarioId(int scenarioId)
        {
            return scenarioId >= 1 && scenarioId <= Letters.Length;
        }

        /// <summary>
        /// 「もうひとつ」という文字列を取得
        /// </summary>
        /// <returns>「もうひとつ」という文字列</returns>
        public static string GetWord()
        {
            return new string(Letters);
        }

        /// <summary>
        /// 「【もうひとつ】」というフォーマット済み文字列を取得
        /// </summary>
        /// <returns>「【もうひとつ】」という文字列</returns>
        public static string GetFormattedWord()
        {
            return "【もうひとつ】";
        }

        /// <summary>
        /// 失われた文字を置換文字（※）に置き換えた文字列を取得
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
                if (IsValidLetter(lostLetter))
                {
                    result = result.Replace(lostLetter.ToString(), "※");
                }
            }
            return result;
        }

        /// <summary>
        /// 収集済みの文字数と総文字数を取得
        /// </summary>
        /// <param name="collectedLetters">収集済みの文字のセット</param>
        /// <returns>（収集済み文字数, 総文字数）のタプル</returns>
        public static (int collected, int total) GetCollectionStatus(HashSet<char> collectedLetters)
        {
            if (collectedLetters == null)
            {
                return (0, Letters.Length);
            }

            int collectedCount = 0;
            foreach (char letter in Letters)
            {
                if (collectedLetters.Contains(letter))
                {
                    collectedCount++;
                }
            }

            return (collectedCount, Letters.Length);
        }

        /// <summary>
        /// 復活済みの文字数と総文字数を取得
        /// </summary>
        /// <param name="restoredLetters">復活済みの文字のセット</param>
        /// <returns>（復活済み文字数, 総文字数）のタプル</returns>
        public static (int restored, int total) GetRestorationStatus(HashSet<char> restoredLetters)
        {
            if (restoredLetters == null)
            {
                return (0, Letters.Length);
            }

            int restoredCount = 0;
            foreach (char letter in Letters)
            {
                if (restoredLetters.Contains(letter))
                {
                    restoredCount++;
                }
            }

            return (restoredCount, Letters.Length);
        }
    }
}

