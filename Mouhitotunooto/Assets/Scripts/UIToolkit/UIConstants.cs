using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// UI関連の定数を定義するクラス
    /// </summary>
    public static class UIConstants
    {
        // カラー定数
        /// <summary>
        /// 濃茶色（ボタンテキストなどに使用）#2B1F18
        /// </summary>
        public static readonly Color DarkBrown = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f);
        
        /// <summary>
        /// 明るい色（テキストに使用）#EDD7B5
        /// </summary>
        public static readonly Color BrightText = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f);
        
        // フォントサイズ定数
        /// <summary>
        /// 通常のフォントサイズ
        /// </summary>
        public const int FontSizeNormal = 18;
        
        /// <summary>
        /// 中サイズのフォントサイズ
        /// </summary>
        public const int FontSizeMedium = 20;
        
        /// <summary>
        /// タイトルのフォントサイズ
        /// </summary>
        public const int FontSizeTitle = 36;
        
        // オーバーレイ関連
        /// <summary>
        /// オーバーレイの不透明度（完全に不透明）
        /// </summary>
        public const float OverlayOpacityFull = 1f;
        
        /// <summary>
        /// オーバーレイの不透明度（透明）
        /// </summary>
        public const float OverlayOpacityTransparent = 0f;
        
        /// <summary>
        /// 背景オーバーレイの不透明度
        /// </summary>
        public const float BackgroundOverlayOpacity = 0.6f;
        
        // フェード関連
        /// <summary>
        /// デフォルトのフェード時間
        /// </summary>
        public const float DefaultFadeDuration = 1.5f;
        
        /// <summary>
        /// 背景オーバーレイのフェード時間
        /// </summary>
        public const float BackgroundOverlayFadeDuration = 0.5f;
        
        // テキストシャドウ
        /// <summary>
        /// デフォルトのテキストシャドウ設定
        /// </summary>
        public static TextShadow DefaultTextShadow => new TextShadow
        {
            offset = new Vector2(1, 1),
            blurRadius = 2,
            color = new Color(0, 0, 0, 0.8f)
        };
    }
}
