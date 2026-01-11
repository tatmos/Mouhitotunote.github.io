using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// ダイアログ関連のヘルパークラス
    /// </summary>
    public static class UIDialogHelper
    {
        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        /// <param name="root">ルートVisualElement</param>
        /// <param name="message">メッセージテキスト</param>
        /// <param name="onConfirm">OKボタンクリック時のコールバック</param>
        /// <param name="onCancel">キャンセルボタンクリック時のコールバック（オプション）</param>
        /// <param name="onHover">ホバー時のコールバック（オプション）</param>
        /// <param name="okButtonImage">OKボタン用の画像（オプション）</param>
        /// <param name="cancelButtonImage">キャンセルボタン用の画像（オプション）</param>
        /// <param name="okButtonTextColor">OKボタンのテキスト色（デフォルト: DarkBrown）</param>
        /// <param name="cancelButtonTextColor">キャンセルボタンのテキスト色（デフォルト: 明るい青白系）</param>
        /// <returns>作成されたモーダル背景のVisualElement</returns>
        public static VisualElement ShowConfirmationDialog(
            VisualElement root,
            string message,
            System.Action onConfirm,
            System.Action onCancel = null,
            System.Action onHover = null,
            Sprite okButtonImage = null,
            Sprite cancelButtonImage = null,
            Color? okButtonTextColor = null,
            Color? cancelButtonTextColor = null)
        {
            if (root == null) return null;
            
            // モーダル背景
            var modalBackground = new VisualElement();
            modalBackground.style.position = Position.Absolute;
            modalBackground.style.left = 0;
            modalBackground.style.top = 0;
            modalBackground.style.right = 0;
            modalBackground.style.bottom = 0;
            modalBackground.style.backgroundColor = new Color(0, 0, 0, 0.8f);
            modalBackground.style.justifyContent = Justify.Center;
            modalBackground.style.alignItems = Align.Center;
            // zIndexが使えない場合は、rootの最後に追加することで最前面に表示される
            
            // ダイアログ本体
            var dialog = new VisualElement();
            dialog.AddToClassList("card");
            dialog.style.paddingTop = 32;
            dialog.style.paddingBottom = 32;
            dialog.style.paddingLeft = 32;
            dialog.style.paddingRight = 32;
            dialog.style.width = 500;
            dialog.style.alignItems = Align.Center;
            // 黒または濃い藍色系の半透明背景を追加
            dialog.style.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.95f); // 濃い藍色系、ほぼ不透明
            dialog.style.borderTopLeftRadius = 10;
            dialog.style.borderTopRightRadius = 10;
            dialog.style.borderBottomLeftRadius = 10;
            dialog.style.borderBottomRightRadius = 10;
            
            // メッセージ
            var label = new Label(message);
            label.style.fontSize = UIConstants.FontSizeMedium;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 30;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = Color.white; // 文字色を白に設定
            dialog.Add(label);
            
            // ボタンコンテナ
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.SpaceBetween;
            buttonContainer.style.width = Length.Percent(100);
            
            // OKボタン
            var okButton = new Button(() => {
                root.Remove(modalBackground);
                onConfirm?.Invoke();
            });
            okButton.text = "OK";
            okButton.AddToClassList("button-gradient");
            okButton.style.flexGrow = 1;
            okButton.style.marginRight = 10;
            if (onHover != null)
            {
                okButton.RegisterCallback<PointerEnterEvent>(evt => onHover());
            }
            // OKボタンに画像を適用
            Color okColor = okButtonTextColor ?? UIConstants.DarkBrown;
            UIButtonHelper.ApplyButtonImage(okButton, okButtonImage, okColor);
            buttonContainer.Add(okButton);
            
            // キャンセルボタン
            var cancelButton = new Button(() => {
                root.Remove(modalBackground);
                onCancel?.Invoke();
            });
            cancelButton.text = "キャンセル";
            cancelButton.AddToClassList("button-gradient-indigo");
            cancelButton.style.flexGrow = 1;
            cancelButton.style.marginLeft = 10;
            if (onHover != null)
            {
                cancelButton.RegisterCallback<PointerEnterEvent>(evt => onHover());
            }
            // キャンセルボタンに画像を適用（明るめの文字色に変更）
            Color cancelColor = cancelButtonTextColor ?? new Color(0.9f, 0.9f, 1f, 1f); // 明るい青白系
            UIButtonHelper.ApplyButtonImage(cancelButton, cancelButtonImage, cancelColor);
            buttonContainer.Add(cancelButton);
            
            dialog.Add(buttonContainer);
            modalBackground.Add(dialog);
            root.Add(modalBackground);
            
            return modalBackground;
        }
    }
}
