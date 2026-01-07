using UnityEngine.UIElements;

namespace NovelGame.Overlay
{
    /// <summary>
    /// UXML要素名定数と要素取得ユーティリティ
    /// </summary>
    public static class OverlayViewBindings
    {
        // 要素名定数
        public const string OverlayRoot = "OverlayRoot";
        public const string RoomImage = "RoomImage";
        public const string GirlImage = "GirlImage";
        public const string BalloonRoot = "BalloonRoot";
        public const string BalloonLabel = "BalloonLabel";
        public const string ThoughtBalloonRoot = "ThoughtBalloonRoot";
        public const string ThoughtBalloonLabel = "ThoughtBalloonLabel";
        public const string PropsLayer = "PropsLayer";
        public const string MusicNoteLayer = "MusicNoteLayer";

        /// <summary>
        /// 要素を取得（null安全）
        /// </summary>
        public static T GetElement<T>(VisualElement root, string name) where T : VisualElement
        {
            if (root == null)
            {
                UnityEngine.Debug.LogWarning($"[OverlayViewBindings] rootがnullです");
                return null;
            }

            var element = root.Q<T>(name);
            if (element == null)
            {
                UnityEngine.Debug.LogWarning($"[OverlayViewBindings] 要素が見つかりません: {name}");
            }
            return element;
        }
    }
}

