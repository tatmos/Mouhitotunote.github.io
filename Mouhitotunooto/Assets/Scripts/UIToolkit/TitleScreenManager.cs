using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// タイトル画面の表示を管理するクラス
    /// </summary>
    public class TitleScreenManager
    {
        private GameManager gameManager;
        private TypewriterEffectManager typewriterEffectManager;
        private ScreenTransitionManager screenTransitionManager;
        private UIDocument titleScreenDocument;
        
        // Settings
        private Sprite selectionScreenBackground;
        private Sprite uiButtonNormalImage;
        
        // Callbacks
        private System.Action onStartButtonClicked;
        private System.Action onHoverSound;
        private System.Action<VisualElement> onApplyBackgroundDistortion;
        private System.Action<VisualElement, Texture2D> onCacheBackgroundTexture;
        private System.Action<Button, Sprite, Color> onApplyButtonImage;
        private System.Func<VisualElement, IEnumerator> onFadeOutTitleTextAndShowSelection;

        public TitleScreenManager(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        /// <summary>
        /// TypewriterEffectManagerを設定
        /// </summary>
        public void SetTypewriterEffectManager(TypewriterEffectManager manager)
        {
            this.typewriterEffectManager = manager;
        }

        /// <summary>
        /// ScreenTransitionManagerを設定
        /// </summary>
        public void SetScreenTransitionManager(ScreenTransitionManager manager)
        {
            this.screenTransitionManager = manager;
        }

        /// <summary>
        /// UIDocumentを設定
        /// </summary>
        public void SetTitleScreenDocument(UIDocument document)
        {
            this.titleScreenDocument = document;
        }

        /// <summary>
        /// 設定を初期化
        /// </summary>
        public void InitializeSettings(Sprite selectionScreenBackground, Sprite uiButtonNormalImage)
        {
            this.selectionScreenBackground = selectionScreenBackground;
            this.uiButtonNormalImage = uiButtonNormalImage;
        }

        /// <summary>
        /// コールバックを設定
        /// </summary>
        public void SetCallbacks(
            System.Action onStartButtonClicked,
            System.Action onHoverSound,
            System.Action<VisualElement> onApplyBackgroundDistortion,
            System.Action<VisualElement, Texture2D> onCacheBackgroundTexture,
            System.Action<Button, Sprite, Color> onApplyButtonImage)
        {
            this.onStartButtonClicked = onStartButtonClicked;
            this.onHoverSound = onHoverSound;
            this.onApplyBackgroundDistortion = onApplyBackgroundDistortion;
            this.onCacheBackgroundTexture = onCacheBackgroundTexture;
            this.onApplyButtonImage = onApplyButtonImage;
        }

        /// <summary>
        /// タイトル画面をセットアップ
        /// </summary>
        public void Setup(VisualElement root)
        {
            if (root == null) return;

            // スクロールバーを非表示にする
            root.style.overflow = Overflow.Hidden;

            // 背景画像を設定
            SetupBackground(root);

            // スタートボタンの設定
            SetupStartButton(root);

            // 謎の声テキストを非表示に設定
            var mysteryVoiceText = root.Q<VisualElement>("MysteryVoiceText");
            if (mysteryVoiceText != null)
            {
                mysteryVoiceText.style.display = DisplayStyle.None;
            }

            // バージョン情報の表示
            SetupVersionText(root);

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        /// <summary>
        /// 背景画像を設定
        /// </summary>
        private void SetupBackground(VisualElement root)
        {
            if (selectionScreenBackground == null) return;

            var backgroundImage = root.Q<VisualElement>("BackgroundImage");
            if (backgroundImage != null)
            {
                backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);

                // 背景テクスチャを事前にキャッシュ
                if (selectionScreenBackground.texture != null)
                {
                    onCacheBackgroundTexture?.Invoke(backgroundImage, selectionScreenBackground.texture);
                }

                // ダークモード時は背景を歪ませる
                onApplyBackgroundDistortion?.Invoke(backgroundImage);
            }
        }

        /// <summary>
        /// スタートボタンを設定
        /// </summary>
        private void SetupStartButton(VisualElement root)
        {
            var startButton = root.Q<Button>("StartButton");
            if (startButton == null) return;

            startButton.clicked += OnStartButtonClickedInternal;
            startButton.RegisterCallback<PointerEnterEvent>(evt => onHoverSound?.Invoke());

            // 3周目：スタートボタンのテキストも伏字にする
            var lostLetters = gameManager.GetLostLetters();
            if (lostLetters.Count > 0)
            {
                string buttonText = "もうひとつを探す";
                foreach (char lostLetter in lostLetters)
                {
                    buttonText = buttonText.Replace(lostLetter.ToString(), "※");
                }
                startButton.text = buttonText;
            }
            else
            {
                startButton.text = "もうひとつを探す";
            }

            // スタートボタンに画像を適用
            Color startButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
            onApplyButtonImage?.Invoke(startButton, uiButtonNormalImage, startButtonTextColor);
        }

        /// <summary>
        /// バージョン情報を設定
        /// </summary>
        private void SetupVersionText(VisualElement root)
        {
            var versionText = root.Q<Label>("VersionText");
            if (versionText != null)
            {
                string text = "v1.9.0 (2026-01-11)";
                var lostLetters = gameManager.GetLostLetters();
                var collectedLetters = gameManager.GetCollectedLetters();
                versionText.text = TextFormatter.FormatText(text, collectedLetters, lostLetters, true);
            }
        }

        /// <summary>
        /// スタートボタンがクリックされた時の処理（内部処理）
        /// </summary>
        private void OnStartButtonClickedInternal()
        {
            if (titleScreenDocument == null) return;

            var root = titleScreenDocument.rootVisualElement;
            if (root == null) return;

            // スタートボタンを非表示
            var startButton = root.Q<Button>("StartButton");
            if (startButton != null)
            {
                startButton.style.display = DisplayStyle.None;
            }

            // 謎の声テキストを表示
            var mysteryVoiceText = root.Q<VisualElement>("MysteryVoiceText");
            if (mysteryVoiceText != null && typewriterEffectManager != null)
            {
                mysteryVoiceText.style.display = DisplayStyle.Flex;

                // 3周目の場合はテキストを変更
                string mysteryText = gameManager.IsThirdLoop()
                    ? "謎の声：\nあなたは\n※※※※※ を探す使命を...\n忘れてはいけません。"
                    : "謎の声：\nあなたは\nもうひとつ を探す使命が\n与えられています。";

                // ダークモード：失われた文字を置換、取得した文字に色を付ける
                var lostLetters = gameManager.GetLostLetters();
                var collectedLetters = gameManager.GetCollectedLetters();
                mysteryText = TextFormatter.FormatText(mysteryText, collectedLetters, lostLetters, true);

                // 強調ワードを含むタイプライター効果でテキストを表示
                typewriterEffectManager.StartTypewriterEffectWithClickableWord(mysteryVoiceText, mysteryText, () =>
                {
                    // タイプライター効果完了後、テキストをフェードアウト
                    onStartButtonClicked?.Invoke();
                }, fontSize: 24, isClickable: false);
            }
            else
            {
                // タイプライター効果が使えない場合は即座にコールバックを呼ぶ
                onStartButtonClicked?.Invoke();
            }
        }
    }
}
