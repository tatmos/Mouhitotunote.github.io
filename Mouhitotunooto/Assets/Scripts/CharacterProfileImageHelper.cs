using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// 登場人物の画像をResourcesフォルダから読み込むヘルパークラス
    /// </summary>
    public static class CharacterProfileImageHelper
    {
        /// <summary>
        /// 指定されたシナリオIDに対応する登場人物の画像を読み込む
        /// </summary>
        /// <param name="scenarioId">シナリオID（1-6）</param>
        /// <returns>画像のSprite（見つからない場合はnull）</returns>
        public static Sprite GetProfileImage(int scenarioId)
        {
            string imagePath = GetImagePath(scenarioId);
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }
            
            var sprite = Resources.Load<Sprite>(imagePath);
            if (sprite == null)
            {
                // エラーログは出力しない（画像がまだない場合は正常な状態）
                return null;
            }
            
            return sprite;
        }
        
        /// <summary>
        /// シナリオIDに対応する画像のResourcesパスを取得
        /// </summary>
        /// <param name="scenarioId">シナリオID（1-6）</param>
        /// <returns>Resourcesパス（拡張子なし）</returns>
        private static string GetImagePath(int scenarioId)
        {
            switch (scenarioId)
            {
                case 1:
                    return "UI/Characters/Character_Momoko"; // 田中 もも子
                case 2:
                    return "UI/Characters/Character_Umi"; // 海原 うみ
                case 3:
                    return "UI/Characters/Character_Hiro"; // 広瀬 ひろ
                case 4:
                    return "UI/Characters/Character_Toru"; // 遠藤 とおる
                case 5:
                    return "UI/Characters/Character_Tsubasa"; // 月島 つばさ
                case 6:
                    return "UI/Characters/Character_Voice"; // 謎の声
                default:
                    return null;
            }
        }
    }
}
