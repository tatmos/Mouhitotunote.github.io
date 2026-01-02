using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    public class SelectionScreenManager
    {
        private VisualElement root;
        private GameManager gameManager;
        private AudioManager audioManager;
        private UIManagerUIToolkit uiManager;
        
        // Settings/Parameters passed from UIManager
        private Sprite selectionScreenBackground;
        private Sprite titleImage;
        private Sprite menuButtonImage;
        private Sprite creditsIcon;
        private Sprite achievementsIcon;
        private Sprite soundIcon;
        private VisualTreeAsset soundSettingsPanelUXML;
        
        private System.Action onShowProfile;
        private System.Action onShowAchievements;
        private System.Action onShowMouhitotsu;
        private System.Action<string, System.Action> onShowConfirmationDialog;
        private System.Action onShowCredits;
        private System.Action onShowSpecialCredits;
        private System.Action onPlayHoverSound;
        private System.Action onUpdateScoreDisplay;
        private System.Action<VisualElement> onCreateScenarioButtons;
        private System.Action<VisualElement> onApplyBackgroundDistortion;
        private System.Action<Button, Sprite, Color> onApplyButtonImage;
        private System.Action<Button, Sprite, string> onSetupButtonWithIcon;
        
        private SoundSettingsManager soundSettingsManager;

        public SelectionScreenManager(
            VisualElement root,
            GameManager gameManager,
            AudioManager audioManager,
            UIManagerUIToolkit uiManager,
            SelectionScreenSettings settings,
            SelectionScreenActions actions)
        {
            this.root = root;
            this.gameManager = gameManager;
            this.audioManager = audioManager;
            this.uiManager = uiManager;
            
            // Set settings
            this.selectionScreenBackground = settings.selectionScreenBackground;
            this.titleImage = settings.titleImage;
            this.menuButtonImage = settings.menuButtonImage;
            this.creditsIcon = settings.creditsIcon;
            this.achievementsIcon = settings.achievementsIcon;
            this.soundIcon = settings.soundIcon;
            this.soundSettingsPanelUXML = settings.soundSettingsPanelUXML;
            
            // Set actions
            this.onShowProfile = actions.onShowProfile;
            this.onShowAchievements = actions.onShowAchievements;
            this.onShowMouhitotsu = actions.onShowMouhitotsu;
            this.onShowConfirmationDialog = actions.onShowConfirmationDialog;
            this.onShowCredits = actions.onShowCredits;
            this.onShowSpecialCredits = actions.onShowSpecialCredits;
            this.onPlayHoverSound = actions.onPlayHoverSound;
            this.onUpdateScoreDisplay = actions.onUpdateScoreDisplay;
            this.onCreateScenarioButtons = actions.onCreateScenarioButtons;
            this.onApplyBackgroundDistortion = actions.onApplyBackgroundDistortion;
            this.onApplyButtonImage = actions.onApplyButtonImage;
            this.onSetupButtonWithIcon = actions.onSetupButtonWithIcon;
        }

        public void Initialize()
        {
            if (root == null) return;

            // 背景画像の設定
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                    onApplyBackgroundDistortion?.Invoke(backgroundImage);
                }
            }

            // タイトルを画像に置き換え
            SetupTitle();

            // プロフィールボタンの設定
            SetupMenuButton("ShowProfileButton", onShowProfile);

            // エンドクレジットボタンの設定
            SetupCreditsButton();

            // 実績ボタンの設定
            SetupMenuButtonWithIcon("ShowAchievementsButton", achievementsIcon, "実績一覧を見る", onShowAchievements);

            // 「もうひとつ」ボタンの設定
            SetupMenuButton("ShowMouhitotsuButton", onShowMouhitotsu);

            // サウンド設定ボタンの設定
            SetupSoundButton();

            // スコア表示を更新
            onUpdateScoreDisplay?.Invoke();

            // シナリオボタンを作成
            var scenarioButtonContainer = root.Q<VisualElement>("ScenarioButtonContainer");
            if (scenarioButtonContainer != null)
            {
                onCreateScenarioButtons?.Invoke(scenarioButtonContainer);
            }
        }

        private void SetupTitle()
        {
            var titleLabel = root.Q<Label>("TitleText");
            if (titleLabel != null && titleImage != null && titleImage.texture != null)
            {
                var titleContainer = titleLabel.parent;
                if (titleContainer != null)
                {
                    // 既に画像があるか確認
                    var existingImage = titleContainer.Q<VisualElement>("TitleImage");
                    if (existingImage != null)
                    {
                        titleLabel.style.display = DisplayStyle.None;
                        return;
                    }

                    var titleImageElement = new VisualElement();
                    titleImageElement.name = "TitleImage";
                    
                    float originalWidth = titleImage.texture.width;
                    float originalHeight = titleImage.texture.height;
                    float aspectRatio = originalHeight / originalWidth;
                    
                    float maxWidth = 600f;
                    float calculatedWidth = Mathf.Min(originalWidth, maxWidth);
                    float calculatedHeight = calculatedWidth * aspectRatio;
                    
                    if (calculatedHeight > 200f)
                    {
                        calculatedHeight = 200f;
                        calculatedWidth = calculatedHeight / aspectRatio;
                    }
                    
                    titleImageElement.style.width = calculatedWidth;
                    titleImageElement.style.height = calculatedHeight;
                    titleImageElement.style.backgroundImage = new StyleBackground(titleImage.texture);
                    titleImageElement.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    titleImageElement.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    titleImageElement.style.marginBottom = 20;
                    titleContainer.Insert(titleContainer.IndexOf(titleLabel), titleImageElement);
                    titleLabel.style.display = DisplayStyle.None;
                }
            }
            else if (titleLabel != null)
            {
                string titleText = "ミニノベルゲーム";
                var lostLetters = gameManager.GetLostLetters();
                if (lostLetters.Count > 0)
                {
                    foreach (char lostLetter in lostLetters)
                    {
                        titleText = titleText.Replace(lostLetter.ToString(), "※");
                    }
                }
                titleLabel.text = titleText;
                titleLabel.AddToClassList("title-text");
                titleLabel.style.display = DisplayStyle.Flex;
                
                // 画像があれば削除
                var titleContainer = titleLabel.parent;
                var existingImage = titleContainer?.Q<VisualElement>("TitleImage");
                if (existingImage != null)
                {
                    titleContainer.Remove(existingImage);
                }
            }
        }

        private void SetupMenuButton(string name, System.Action onClick)
        {
            var button = root.Q<Button>(name);
            if (button != null)
            {
                button.clicked += () => onClick?.Invoke();
                button.RegisterCallback<PointerEnterEvent>(evt => onPlayHoverSound?.Invoke());
                Color menuButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f);
                onApplyButtonImage?.Invoke(button, menuButtonImage, menuButtonTextColor);
                button.style.display = DisplayStyle.Flex;
            }
        }

        private void SetupMenuButtonWithIcon(string name, Sprite icon, string text, System.Action onClick)
        {
            var button = root.Q<Button>(name);
            if (button != null)
            {
                onSetupButtonWithIcon?.Invoke(button, icon, text);
                button.clicked += () => onClick?.Invoke();
                button.RegisterCallback<PointerEnterEvent>(evt => onPlayHoverSound?.Invoke());
                Color menuButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f);
                onApplyButtonImage?.Invoke(button, menuButtonImage, menuButtonTextColor);
                button.style.display = DisplayStyle.Flex;
            }
        }

        private void SetupCreditsButton()
        {
            var showCreditsButton = root.Q<Button>("ShowCreditsButton");
            if (showCreditsButton != null)
            {
                onSetupButtonWithIcon?.Invoke(showCreditsButton, creditsIcon, "エンドクレジットを見る");
                showCreditsButton.RegisterCallback<PointerEnterEvent>(evt => onPlayHoverSound?.Invoke());
                Color menuButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f);
                onApplyButtonImage?.Invoke(showCreditsButton, menuButtonImage, menuButtonTextColor);
                
                var scenario6Result = gameManager.GetScenarioResult(6);
                if (scenario6Result != null)
                {
                    showCreditsButton.style.display = DisplayStyle.Flex;
                    showCreditsButton.clicked += () => {
                        if (gameManager.IsThirdLoop())
                        {
                            onShowConfirmationDialog?.Invoke("ここから先に進むともう戻れませんがよろしいですか？", () => {
                                onShowSpecialCredits?.Invoke();
                            });
                        }
                        else
                        {
                            onShowCredits?.Invoke();
                        }
                    };
                }
                else
                {
                    showCreditsButton.style.display = DisplayStyle.None;
                }
            }
        }

        private void SetupSoundButton()
        {
            var soundButton = root.Q<Button>("SoundButton");
            if (soundButton != null)
            {
                if (soundIcon != null)
                {
                    onSetupButtonWithIcon?.Invoke(soundButton, soundIcon, "");
                }
                else
                {
                    soundButton.text = "🔊";
                }

                soundButton.RegisterCallback<PointerEnterEvent>(evt => onPlayHoverSound?.Invoke());
                soundButton.clicked += () => {
                    if (soundSettingsManager == null)
                    {
                        soundSettingsManager = new SoundSettingsManager(root, soundSettingsPanelUXML, onPlayHoverSound);
                    }
                    soundSettingsManager.Show(root);
                };
                
                soundButton.style.backgroundColor = Color.clear;
                soundButton.style.backgroundImage = null;
                soundButton.style.borderTopWidth = 0;
                soundButton.style.borderRightWidth = 0;
                soundButton.style.borderBottomWidth = 0;
                soundButton.style.borderLeftWidth = 0;
                
                if (soundIcon == null)
                {
                    soundButton.style.color = Color.white;
                }
            }
        }
    }

    public struct SelectionScreenSettings
    {
        public Sprite selectionScreenBackground;
        public Sprite titleImage;
        public Sprite menuButtonImage;
        public Sprite creditsIcon;
        public Sprite achievementsIcon;
        public Sprite soundIcon;
        public VisualTreeAsset soundSettingsPanelUXML;
    }

    public struct SelectionScreenActions
    {
        public System.Action onShowProfile;
        public System.Action onShowAchievements;
        public System.Action onShowMouhitotsu;
        public System.Action<string, System.Action> onShowConfirmationDialog;
        public System.Action onShowCredits;
        public System.Action onShowSpecialCredits;
        public System.Action onPlayHoverSound;
        public System.Action onUpdateScoreDisplay;
        public System.Action<VisualElement> onCreateScenarioButtons;
        public System.Action<VisualElement> onApplyBackgroundDistortion;
        public System.Action<Button, Sprite, Color> onApplyButtonImage;
        public System.Action<Button, Sprite, string> onSetupButtonWithIcon;
    }
}
