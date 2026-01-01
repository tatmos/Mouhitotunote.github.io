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
        [System.Obsolete("TextFormatter.ReplaceLostLetters を使用してください")]
        public static string ReplaceLostLetters(string text, HashSet<char> lostLetters)
        {
            return TextFormatter.ReplaceLostLetters(text, lostLetters);
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

        /// <summary>
        /// 文字を取得したときのエピローグメッセージを生成
        /// </summary>
        /// <param name="letter">取得した文字</param>
        /// <param name="scenarioId">シナリオID</param>
        /// <param name="loopCount">周回数</param>
        /// <param name="isRestored">復活した文字かどうか（3周目以降の場合）</param>
        /// <returns>エピローグメッセージ。文字が取得済みでない場合は空文字列</returns>
        public static string GetLetterAcquiredEpilogue(char letter, int scenarioId, int loopCount = 1, bool isRestored = false)
        {
            if (!IsValidLetter(letter))
            {
                return string.Empty;
            }

            // 3周目以降で復活した場合
            if (isRestored && loopCount >= 3)
            {
                return $"(文字が復活し始めている...「{letter}」という文字を取り戻した)";
            }

            // 1周目または2周目で取得した場合
            if (loopCount >= 2)
            {
                return $"(何かが集まり始めている...「{letter}」という文字を手に入れた)";
            }

            return $"(何かが集まり始めている...「{letter}」という文字を手に入れた)";
        }

        /// <summary>
        /// 文字を取得したときのエピローグ2メッセージを生成（シナリオ1, 2用）
        /// </summary>
        /// <param name="letter">取得した文字</param>
        /// <param name="scenarioId">シナリオID</param>
        /// <param name="loopCount">周回数</param>
        /// <param name="isRestored">復活した文字かどうか（3周目以降の場合）</param>
        /// <param name="context">シナリオの文脈（例：「写真の隅」「地下の部屋」「メニュー」「料理の皿の底」など）</param>
        /// <returns>エピローグ2メッセージ。文字が取得済みでない場合は空文字列</returns>
        public static string GetLetterAcquiredEpilogue2(char letter, int scenarioId, int loopCount = 1, bool isRestored = false, string context = "")
        {
            if (!IsValidLetter(letter))
            {
                return string.Empty;
            }

            // シナリオ1（「も」）の場合
            if (scenarioId == 1 && letter == 'も')
            {
                if (isRestored && loopCount >= 3)
                {
                    return $"もも子さんは、あなたが写真の真実を発見したことを喜んでいました。\n「あなたは本当に優秀な探偵ね。この真実に気づいた人は、あなたが初めてよ」\nそう言って、もも子さんは深い笑顔を浮かべました。\nあなたは、写真から浮かび上がった文字「{letter}」を手に取りました。\nその文字は、まるで生きているかのように温かく、あなたの手の中で輝いていました。";
                }
                else if (loopCount >= 2)
                {
                    string location = string.IsNullOrEmpty(context) ? "地下の部屋" : context;
                    return $"もも子さんは、あなたが建物の秘密を発見したことを喜んでいました。\n「あなたは本当に優秀な探偵ね。{location}なんて、誰も気づかなかったわ」\nそう言って、もも子さんは笑顔で帰っていきました。\nあなたは、{location}で見つけた小さな文字「{letter}」を大切に保管することにしました。\nその文字には、何か特別な力が宿っているような気がしました。";
                }
                else
                {
                    string location = string.IsNullOrEmpty(context) ? "写真の隅" : context;
                    return $"もも子さんは、あなたの探偵としての成長を認めてくれました。\n「あなたは本当に優秀な探偵ね。また何かあったら、お願いします」\nそう言って、もも子さんは笑顔で帰っていきました。\nあなたは、{location}にあった小さな文字「{letter}」を大切に保管することにしました。";
                }
            }

            // シナリオ2（「う」）の場合
            if (scenarioId == 2 && letter == 'う')
            {
                if (isRestored && loopCount >= 3)
                {
                    return $"うみシェフは、あなたがメニューの秘密を発見したことを喜んでいました。\n「あなたは本当に特別なお客様ね。このメッセージに気づいた人は、あなたが初めてよ」\nそう言って、うみシェフは深い笑顔を浮かべました。\nあなたは、メニューから浮かび上がった文字「{letter}」を手に取りました。\nその文字は、まるで料理の香りのように温かく、あなたの心に染み込んでいきました。";
                }
                else if (loopCount >= 2)
                {
                    string location = string.IsNullOrEmpty(context) ? "料理の皿の底" : context;
                    return $"うみシェフは、あなたが料理の秘密に気づいたことを喜んでいました。\n「よく気づいてくれたわね。あの食材は、特別なお客様だけに提供しているの」\nあなたは、うみシェフのこだわりと情熱に、ますますこのレストランが好きになりました。\n「また来てね」と、うみシェフは手を振って見送ってくれました。\n{location}に、小さな文字「{letter}」が刻まれているのを見つけました。";
                }
                else
                {
                    string location = string.IsNullOrEmpty(context) ? "皿の底" : context;
                    return $"うみシェフは、あなたが{location}の文字に気づいたことを喜んでいました。\n「よく気づいてくれたわね。あれは特別なお客様へのメッセージなの」\nあなたは、うみシェフの遊び心と温かさに、ますますこのレストランが好きになりました。\n「また来てね」と、うみシェフは手を振って見送ってくれました。";
                }
            }

            // シナリオ3（「ひ」）の場合
            if (scenarioId == 3 && letter == 'ひ')
            {
                string timeCapsuleItem = string.IsNullOrEmpty(context) ? "タイムカプセル" : context;
                
                if (isRestored && loopCount >= 3)
                {
                    return $"{timeCapsuleItem}の秘密を発見した後、ひろは「一緒に開けて良かった。一人では気づけなかったかもしれない」と言いました。\n「友情の力で、この秘密に気づけたんだね」\n{timeCapsuleItem}に刻まれた文字「{letter}」は、友情の証として、あなたとひろの心に刻まれました。\n二人は、これからもずっと友達でいられることを確信しました。";
                }
                else if (loopCount >= 2)
                {
                    return $"{timeCapsuleItem}の秘密を発見した後、あなたはひろにこのことを伝えることにしました。\n「実は、一人で開けたんだ。{timeCapsuleItem}に秘密があったの」\nひろは驚きながらも、あなたの行動を理解してくれました。\n「一人で開けることも、時には必要なんだね」\n{timeCapsuleItem}に刻まれた文字「{letter}」は、あなたの成長の証として、あなたの心に刻まれました。";
                }
                else
                {
                    return "新しいタイムカプセルを埋めた後、ひろは「今度は20年後、また一緒に開けよう」と言いました。\n「約束だよ」と、あなたはひろと小指を絡めました。\n手紙の裏にあった文字「ひ」は、友情の証として、あなたの心に刻まれました。\n二人は、これからもずっと友達でいられることを確信しました。";
                }
            }

            // シナリオ4（「と」）の場合
            if (scenarioId == 4 && letter == 'と')
            {
                string animalName = string.IsNullOrEmpty(context) ? "動物" : context;
                
                if (isRestored && loopCount >= 3)
                {
                    return $"とおる試験官は、あなたが魔法と手品の両方を理解したことを喜んでいました。\n「あなたは本当に特別な才能を持っている。魔法と手品、両方を組み合わせることができるのは、あなたが初めてだ」\nあなたは、{animalName}の周りに浮かんだ文字「{letter}」を手に取りました。\nその文字は、まるで魔法の力そのもののように、あなたの心に染み込んでいきました。";
                }
                else if (loopCount >= 2)
                {
                    return "とおる試験官は、あなたの才能を高く評価してくれました。\n「手品と魔法、両方を理解できるのは珍しい。君は特別な才能を持っている」\nあなたは、帽子の中に浮かんだ文字「と」を、魔法の証として大切にしました。\n「これからも、もうひとつの可能性を探し続けてほしい」と、とおる試験官は言いました。";
                }
                else
                {
                    return $"とおる試験官は、あなたの才能を高く評価してくれました。\n「魔法と手品、両方を使えるのは珍しい。君は特別な才能を持っている」\nあなたは、消えた{animalName}の跡に浮かんだ文字「{letter}」を、魔法の証として大切にしました。\n「これからも、もうひとつの可能性を探し続けてほしい」と、とおる試験官は言いました。";
                }
            }

            // シナリオ5（「つ」）の場合
            if (scenarioId == 5 && letter == 'つ')
            {
                string location = string.IsNullOrEmpty(context) ? "パズル" : context;
                
                if (isRestored && loopCount >= 3)
                {
                    return "パズルの秘密を発見した後、つばさは「一緒に完成させて良かった。一人では気づけなかったかもしれない」と言いました。\n「愛情の力で、この秘密に気づけたんだね」\nパズルに刻まれた文字「つ」は、愛情の証として、あなたとつばさの心に刻まれました。\n「これからも、ずっと一緒にパズルを完成させようね」\nあなたは、つばさの優しさに包まれながら、幸せを噛みしめました。";
                }
                else if (loopCount >= 2)
                {
                    return "パズルの裏側にあった文字「つ」を発見した後、つばさは「よく気づいてくれたね」と微笑みました。\n「実は、このパズルには特別な仕掛けがあったの。完成させた人だけが、その秘密を知ることができる」\nあなたは、つばさの愛情と工夫に感動しました。\n「これからも、ずっと一緒にパズルを完成させようね」\nあなたは、つばさの優しさに包まれながら、幸せを噛みしめました。";
                }
                else
                {
                    return "新しいパズルを完成させた後、つばさは「君のために、いつも準備してるんだよ」と微笑みました。\n箱の蓋の内側にあった文字「つ」は、つばさの愛情の証でした。\n「これからも、ずっと一緒にパズルを完成させようね」\nあなたは、つばさの優しさに包まれながら、幸せを噛みしめました。";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 指定された文字が取得済みかどうかを判定
        /// </summary>
        /// <param name="letter">判定する文字</param>
        /// <param name="collectedLetters">収集済みの文字のセット</param>
        /// <returns>取得済みの場合は true</returns>
        public static bool IsLetterCollected(char letter, HashSet<char> collectedLetters)
        {
            if (collectedLetters == null)
            {
                return false;
            }
            return collectedLetters.Contains(letter);
        }
    }
}

