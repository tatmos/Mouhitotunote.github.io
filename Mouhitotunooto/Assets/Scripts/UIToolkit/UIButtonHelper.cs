using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// ボタン関連のヘルパークラス
    /// </summary>
    public static class UIButtonHelper
    {
        /// <summary>
        /// ボタンに画像を適用する
        /// </summary>
        /// <param name="button">対象のボタン</param>
        /// <param name="buttonImage">ボタン画像（スプライト）</param>
        /// <param name="textColor">テキストの色</param>
        public static void ApplyButtonImage(Button button, Sprite buttonImage, Color textColor)
        {
            if (button == null) return;
            
            // テキストの色を設定
            button.style.color = textColor;
            
            // 背景画像を設定（9-slice対応）
            if (buttonImage != null)
            {
                button.style.backgroundImage = new StyleBackground(buttonImage);
                button.style.backgroundColor = Color.clear; // 背景画像が設定されている場合は背景色をクリア
            }
            else
            {
                button.style.backgroundImage = StyleKeyword.None;
            }
        }
        
        /// <summary>
        /// ボタンにアイコンとツールチップを設定する
        /// </summary>
        /// <param name="button">対象のボタン</param>
        /// <param name="icon">アイコン（スプライト）</param>
        /// <param name="tooltip">ツールチップテキスト</param>
        public static void SetupButtonWithIcon(Button button, Sprite icon, string tooltip)
        {
            if (button == null) return;
            
            // ツールチップを設定
            if (!string.IsNullOrEmpty(tooltip))
            {
                button.tooltip = tooltip;
            }
            
            // アイコンを設定（現在の実装では、アイコンは画像として表示されるため、背景画像として設定）
            // 注意: 既存の実装を確認する必要がある
            if (icon != null)
            {
                button.style.backgroundImage = new StyleBackground(icon);
            }
        }
        
        /// <summary>
        /// ボタンにイベントハンドラを設定する（ホバー音付き）
        /// </summary>
        /// <param name="button">対象のボタン</param>
        /// <param name="onClick">クリック時のコールバック</param>
        /// <param name="onHover">ホバー時のコールバック（オプション）</param>
        public static void SetupButtonWithEvents(Button button, System.Action onClick, System.Action onHover = null)
        {
            if (button == null) return;
            
            button.clicked += onClick;
            
            if (onHover != null)
            {
                button.RegisterCallback<PointerEnterEvent>(evt => onHover());
            }
            
            // オーバーレイによるブロックを防ぐため、USSクラスを追加
            button.AddToClassList("button-interactive");
        }
    }
}
