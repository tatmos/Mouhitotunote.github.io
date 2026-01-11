using System.Text.RegularExpressions;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// シナリオのsetupテキストから動物名を抽出するヘルパークラス
    /// </summary>
    public static class ExtractAnimalNameHelper
    {
        /// <summary>
        /// シナリオのsetupテキストから動物名を抽出
        /// フォーマット：「試験官：「{animalName}を出現させなさい」」
        /// </summary>
        public static string ExtractAnimalNameFromSetup(string setup)
        {
            if (string.IsNullOrEmpty(setup))
            {
                return "";
            }

            // パターン1: 「試験官：「{animalName}を出現させなさい」」
            var pattern1 = @"試験官：「([^」]+)を出現させなさい」";
            var match1 = Regex.Match(setup, pattern1);
            if (match1.Success && match1.Groups.Count > 1)
            {
                return match1.Groups[1].Value.Trim();
            }

            // パターン2: 試験官：「{animalName}を出現させなさい」（「」なし）
            var pattern2 = @"試験官：""([^""]+)を出現させなさい""";
            var match2 = Regex.Match(setup, pattern2);
            if (match2.Success && match2.Groups.Count > 1)
            {
                return match2.Groups[1].Value.Trim();
            }

            return "";
        }
    }
}
