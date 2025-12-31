using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// パズル完成時の絵の内容管理クラス
    /// ランダムな絵の内容とリアクションを提供する
    /// </summary>
    public static class PuzzleImageManager
    {
        private static List<PuzzleImageData> puzzleImages = null;
        private static System.Random random = new System.Random();

        private class PuzzleImageData
        {
            public string imageDescription;
            public string reaction;
        }

        /// <summary>
        /// パズル絵のリストを初期化
        /// </summary>
        private static void InitializePuzzleImages()
        {
            if (puzzleImages != null) return;

            puzzleImages = new List<PuzzleImageData>
            {
                new PuzzleImageData { imageDescription = "美しい星空", reaction = "達成感に満たされる。" },
                new PuzzleImageData { imageDescription = "夕日の海", reaction = "心が温かくなる。" },
                new PuzzleImageData { imageDescription = "桜の木", reaction = "春の訪れを感じる。" },
                new PuzzleImageData { imageDescription = "森の小道", reaction = "自然の美しさに感動する。" },
                new PuzzleImageData { imageDescription = "山の頂上", reaction = "達成感と爽快感が広がる。" },
                new PuzzleImageData { imageDescription = "花畑", reaction = "色彩の美しさに目を奪われる。" },
                new PuzzleImageData { imageDescription = "古い街並み", reaction = "懐かしさがこみ上げてくる。" },
                new PuzzleImageData { imageDescription = "湖の風景", reaction = "静けさと美しさに包まれる。" },
                new PuzzleImageData { imageDescription = "雪景色", reaction = "清らかな美しさに心が洗われる。" },
                new PuzzleImageData { imageDescription = "虹の橋", reaction = "希望の光を感じる。" },
                new PuzzleImageData { imageDescription = "夕焼け空", reaction = "時間の流れを感じる。" },
                new PuzzleImageData { imageDescription = "草原", reaction = "広がる景色に心が解放される。" },
                new PuzzleImageData { imageDescription = "滝", reaction = "力強さと美しさに圧倒される。" },
                new PuzzleImageData { imageDescription = "街の夜景", reaction = "光の美しさに魅了される。" },
                new PuzzleImageData { imageDescription = "海辺の風景", reaction = "波の音が聞こえてきそうだ。" },
                new PuzzleImageData { imageDescription = "田園風景", reaction = "のどかな気持ちになる。" },
                new PuzzleImageData { imageDescription = "紅葉の山", reaction = "秋の美しさに心を動かされる。" },
                new PuzzleImageData { imageDescription = "雲海", reaction = "まるで空に浮かんでいるようだ。" },
                new PuzzleImageData { imageDescription = "古い城", reaction = "歴史の重みを感じる。" },
                new PuzzleImageData { imageDescription = "橋", reaction = "つながりを感じる。" },
                new PuzzleImageData { imageDescription = "灯台", reaction = "希望の光を感じる。" },
                new PuzzleImageData { imageDescription = "風車", reaction = "のんびりとした時間が流れる。" },
                new PuzzleImageData { imageDescription = "教会", reaction = "静かな祈りの気持ちになる。" },
                new PuzzleImageData { imageDescription = "港町", reaction = "活気と美しさが調和している。" },
                new PuzzleImageData { imageDescription = "竹林", reaction = "静寂と美しさに包まれる。" },
                new PuzzleImageData { imageDescription = "砂漠", reaction = "広大な自然の力に圧倒される。" },
                new PuzzleImageData { imageDescription = "氷河", reaction = "神秘的な美しさに息を呑む。" },
                new PuzzleImageData { imageDescription = "オーロラ", reaction = "幻想的な光に心を奪われる。" },
                new PuzzleImageData { imageDescription = "富士山", reaction = "日本の美しさを感じる。" },
                new PuzzleImageData { imageDescription = "桜並木", reaction = "春の訪れを心から喜ぶ。" },
                new PuzzleImageData { imageDescription = "向日葵畑", reaction = "明るい気持ちになる。" },
                new PuzzleImageData { imageDescription = "コスモス畑", reaction = "秋の風情を感じる。" },
                new PuzzleImageData { imageDescription = "ラベンダー畑", reaction = "香りが漂ってきそうだ。" },
                new PuzzleImageData { imageDescription = "チューリップ畑", reaction = "色彩の豊かさに驚く。" },
                new PuzzleImageData { imageDescription = "梅の花", reaction = "早春の美しさに心が和む。" },
                new PuzzleImageData { imageDescription = "蓮の池", reaction = "清らかな美しさに感動する。" },
                new PuzzleImageData { imageDescription = "紅葉の渓谷", reaction = "自然の芸術に感動する。" },
                new PuzzleImageData { imageDescription = "雪化粧の山", reaction = "純白の美しさに心が洗われる。" },
                new PuzzleImageData { imageDescription = "朝焼け", reaction = "新しい一日の始まりを感じる。" },
                new PuzzleImageData { imageDescription = "満月の夜", reaction = "静かな美しさに包まれる。" },
                new PuzzleImageData { imageDescription = "星降る夜", reaction = "ロマンチックな気分になる。" },
                new PuzzleImageData { imageDescription = "朝霧", reaction = "幻想的な雰囲気に包まれる。" },
                new PuzzleImageData { imageDescription = "夕暮れの街", reaction = "一日の終わりを感じる。" },
                new PuzzleImageData { imageDescription = "雨上がり", reaction = "清々しい気持ちになる。" },
                new PuzzleImageData { imageDescription = "雪の結晶", reaction = "細部の美しさに驚く。" },
                new PuzzleImageData { imageDescription = "蝶々", reaction = "自由な美しさに心が軽くなる。" },
                new PuzzleImageData { imageDescription = "鳥の群れ", reaction = "生命の躍動を感じる。" },
                new PuzzleImageData { imageDescription = "イルカ", reaction = "優しさと力強さを感じる。" },
                new PuzzleImageData { imageDescription = "クジラ", reaction = "壮大な自然の力に感動する。" },
                new PuzzleImageData { imageDescription = "ペンギン", reaction = "愛らしさに心が和む。" },
                new PuzzleImageData { imageDescription = "キツネ", reaction = "神秘的な美しさに魅了される。" },
                new PuzzleImageData { imageDescription = "鹿", reaction = "優雅な美しさに心を動かされる。" }
            };
        }

        /// <summary>
        /// ランダムなパズル絵の内容を取得
        /// </summary>
        /// <returns>ランダムに選ばれたパズル絵のデータ</returns>
        public static PuzzleImageResult GetRandomPuzzleImage()
        {
            InitializePuzzleImages();
            
            if (puzzleImages == null || puzzleImages.Count == 0)
            {
                return new PuzzleImageResult { ImageDescription = "美しい星空", Reaction = "達成感に満たされる。" }; // フォールバック
            }

            int index = random.Next(puzzleImages.Count);
            var data = puzzleImages[index];
            return new PuzzleImageResult { ImageDescription = data.imageDescription, Reaction = data.reaction };
        }

        /// <summary>
        /// 保存されたパズル絵の内容を取得（GameManagerから）
        /// </summary>
        /// <returns>保存されたパズル絵のデータ。見つからない場合は生成してから返す</returns>
        public static PuzzleImageResult GetPuzzleImage()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                string savedImage = gameManager.GetScenarioRandomData(5, "puzzleImage");
                string savedReaction = gameManager.GetScenarioRandomData(5, "puzzleReaction");
                if (!string.IsNullOrEmpty(savedImage) && !string.IsNullOrEmpty(savedReaction))
                {
                    return new PuzzleImageResult { ImageDescription = savedImage, Reaction = savedReaction };
                }
                // 保存されていない場合は生成してから取得
                gameManager.GenerateScenarioRandomData();
                savedImage = gameManager.GetScenarioRandomData(5, "puzzleImage");
                savedReaction = gameManager.GetScenarioRandomData(5, "puzzleReaction");
                if (!string.IsNullOrEmpty(savedImage) && !string.IsNullOrEmpty(savedReaction))
                {
                    return new PuzzleImageResult { ImageDescription = savedImage, Reaction = savedReaction };
                }
            }
            // フォールバック：ランダムなパズル絵を返す
            return GetRandomPuzzleImage();
        }

        /// <summary>
        /// パズル絵の結果データ
        /// </summary>
        public class PuzzleImageResult
        {
            public string ImageDescription;
            public string Reaction;
        }
    }
}

