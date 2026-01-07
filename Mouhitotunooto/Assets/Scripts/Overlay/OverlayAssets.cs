using UnityEngine;

namespace NovelGame.Overlay
{
    /// <summary>
    /// Overlayアセット参照解決
    /// </summary>
    public class OverlayAssets
    {
        // 表情Sprite（Resourcesから読み込む想定）
        private static readonly string[] ExpressionPaths = new string[]
        {
            "Overlay/Girl/Neutral",
            "Overlay/Girl/Smile",
            "Overlay/Girl/Laugh",
            "Overlay/Girl/Surprise",
            "Overlay/Girl/Thinking",
            "Overlay/Girl/Annoyed",
            "Overlay/Girl/Shock",
            "Overlay/Girl/Concern",
            "Overlay/Girl/Singing"
        };

        // 部屋背景Texture（Resourcesから読み込む想定）
        private static readonly string[] RoomPaths = new string[]
        {
            "Overlay/Room/CleanDay",
            "Overlay/Room/NightGlow",
            "Overlay/Room/Messy",
            "Overlay/Room/Glitchy",
            "Overlay/Room/CalmMorning"
        };

        /// <summary>
        /// 表情Spriteを取得
        /// </summary>
        public static Sprite GetExpressionSprite(GirlExpression expression)
        {
            int index = (int)expression;
            if (index < 0 || index >= ExpressionPaths.Length)
            {
                Debug.LogWarning($"[OverlayAssets] 無効な表情インデックス: {index}");
                return null;
            }

            Sprite sprite = Resources.Load<Sprite>(ExpressionPaths[index]);
            if (sprite == null)
            {
                Debug.LogWarning($"[OverlayAssets] 表情Spriteが見つかりません: {ExpressionPaths[index]}");
            }
            return sprite;
        }

        /// <summary>
        /// 部屋背景Textureを取得
        /// </summary>
        public static Texture2D GetRoomTexture(RoomState roomState)
        {
            int index = (int)roomState;
            if (index < 0 || index >= RoomPaths.Length)
            {
                Debug.LogWarning($"[OverlayAssets] 無効な部屋状態インデックス: {index}");
                return null;
            }

            Texture2D texture = Resources.Load<Texture2D>(RoomPaths[index]);
            if (texture == null)
            {
                Debug.LogWarning($"[OverlayAssets] 部屋Textureが見つかりません: {RoomPaths[index]}");
            }
            return texture;
        }

        /// <summary>
        /// 部屋背景をSpriteとして取得
        /// </summary>
        public static Sprite GetRoomSprite(RoomState roomState)
        {
            Texture2D texture = GetRoomTexture(roomState);
            if (texture == null) return null;

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 音符のSpriteを取得（♫がフォントに含まれていない場合の画像フォールバック）
        /// </summary>
        public static Sprite GetMusicNoteSprite(string note)
        {
            if (note == "♫")
            {
                // ♫の画像を読み込む（オプション、Resources/Overlay/MusicNotes/BeamedNote.pngなど）
                Sprite sprite = Resources.Load<Sprite>("Overlay/MusicNotes/BeamedNote");
                if (sprite == null)
                {
                    Debug.LogWarning("[OverlayAssets] ♫の画像が見つかりません: Overlay/MusicNotes/BeamedNote");
                }
                return sprite;
            }
            // ♪はフォントに含まれているため、nullを返す（テキストとして表示）
            return null;
        }
    }
}

