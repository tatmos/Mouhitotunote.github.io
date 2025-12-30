using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// 「もうひとつ」（再挑戦）画面の表示を管理するクラス
    /// </summary>
    public class MouhitotsuScreenManager
    {
        private GameManager gameManager;
        private System.Action onHoverSound;
        private System.Action<string> onDivisionJump;

        public MouhitotsuScreenManager(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        /// <summary>
        /// Divisionジャンプ時のコールバックを設定
        /// </summary>
        public void SetOnDivisionJumpCallback(System.Action<string> callback)
        {
            this.onDivisionJump = callback;
        }

        /// <summary>
        /// ホバー音再生用のコールバックを設定
        /// </summary>
        public void SetOnHoverSoundCallback(System.Action callback)
        {
            this.onHoverSound = callback;
        }

        /// <summary>
        /// 再挑戦ボタンを作成
        /// </summary>
        public void CreateRetryButtons(VisualElement root)
        {
            var container = root.Q<VisualElement>("MouhitotsuContainer");
            if (container == null) return;
            
            container.Clear();

            // 進捗度の表示を追加
            AddProgressDisplay(root);

            // 設定UIを追加（物語の解明度の下）
            AddSettingsUI(root);

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Column;
            buttonContainer.style.alignItems = Align.Center;
            buttonContainer.style.width = Length.Percent(100);

            string[] divisions = { "Prologue", "A", "B", "C", "D", "E" };
            string[] divisionNames = { 
                "プロローグ",
                "Division A: 通常の物語", 
                "Division B: ダークモードへの予兆", 
                "Division C: 3周目への門", 
                "Division D: 救済のエンド", 
                "Division E: 終焉のエンド" 
            };

            for (int i = 0; i < divisions.Length; i++)
            {
                string id = divisions[i];
                string name = divisionNames[i];

                if (id == "Prologue" || gameManager.IsDivisionCleared(id))
                {
                    Button btn = new Button();
                    btn.text = name;
                    btn.style.fontSize = 20;
                    btn.style.paddingLeft = 30;
                    btn.style.paddingRight = 30;
                    btn.style.paddingTop = 15;
                    btn.style.paddingBottom = 15;
                    btn.style.marginBottom = 15;
                    btn.style.minWidth = 400;
                    btn.style.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
                    btn.style.color = Color.white;
                    btn.style.borderTopLeftRadius = 5;
                    btn.style.borderTopRightRadius = 5;
                    btn.style.borderBottomLeftRadius = 5;
                    btn.style.borderBottomRightRadius = 5;

                    btn.clicked += () => onDivisionJump?.Invoke(id);
                    btn.RegisterCallback<PointerEnterEvent>(evt => onHoverSound?.Invoke());

                    buttonContainer.Add(btn);
                }
            }

            container.Add(buttonContainer);
        }

        /// <summary>
        /// 進捗度表示を追加
        /// </summary>
        private void AddProgressDisplay(VisualElement root)
        {
            // 既存の進捗ラベルがあれば削除（再生成のため）
            var oldProgress = root.Q<Label>("MouhitotsuProgress");
            if (oldProgress != null)
            {
                oldProgress.parent.Remove(oldProgress);
            }

            // 物語の解明度表示がOFFの場合は表示しない
            if (!gameManager.GetShowStoryProgress())
            {
                return;
            }

            int percentage = gameManager.GetStoryProgressPercentage();

            var progressLabel = new Label($"物語の解明度: {percentage}%");
            progressLabel.name = "MouhitotsuProgress";
            progressLabel.style.fontSize = 24;
            progressLabel.style.marginBottom = 20;
            progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            progressLabel.style.color = new Color(0.8f, 0.8f, 1f);
            progressLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // タイトルの直下に挿入
            var title = root.Q<Label>("MouhitotsuTitle");
            if (title != null)
            {
                int insertIndex = title.parent.IndexOf(title) + 1;
                title.parent.Insert(insertIndex, progressLabel);
            }
        }

        /// <summary>
        /// 設定UIを追加（BGM音量スライダー、物語の解明度表示ON/OFF）
        /// </summary>
        private void AddSettingsUI(VisualElement root)
        {
            // 既存の設定UIがあれば削除（再生成のため）
            var oldSettings = root.Q<VisualElement>("MouhitotsuSettings");
            if (oldSettings != null)
            {
                oldSettings.parent.Remove(oldSettings);
            }

            // 設定コンテナを作成
            var settingsContainer = new VisualElement();
            settingsContainer.name = "MouhitotsuSettings";
            settingsContainer.style.flexDirection = FlexDirection.Column;
            settingsContainer.style.alignItems = Align.Center;
            settingsContainer.style.width = Length.Percent(100);
            settingsContainer.style.marginTop = 20;
            settingsContainer.style.marginBottom = 30;
            settingsContainer.style.paddingTop = 20;
            settingsContainer.style.paddingBottom = 20;
            settingsContainer.style.paddingLeft = 20;
            settingsContainer.style.paddingRight = 20;
            settingsContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            settingsContainer.style.borderTopLeftRadius = 10;
            settingsContainer.style.borderTopRightRadius = 10;
            settingsContainer.style.borderBottomLeftRadius = 10;
            settingsContainer.style.borderBottomRightRadius = 10;

            // 設定タイトル
            var settingsTitle = new Label("設定");
            settingsTitle.style.fontSize = 20;
            settingsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            settingsTitle.style.color = new Color(0.9f, 0.9f, 1f);
            settingsTitle.style.marginBottom = 15;
            settingsContainer.Add(settingsTitle);

            // BGM音量スライダー
            var bgmContainer = new VisualElement();
            bgmContainer.style.flexDirection = FlexDirection.Row;
            bgmContainer.style.alignItems = Align.Center;
            bgmContainer.style.width = Length.Percent(80);
            bgmContainer.style.maxWidth = 500;
            bgmContainer.style.marginBottom = 15;

            var bgmLabel = new Label("BGM音量:");
            bgmLabel.style.fontSize = 16;
            bgmLabel.style.color = Color.white;
            bgmLabel.style.minWidth = 100;
            bgmContainer.Add(bgmLabel);

            var bgmSlider = new Slider(0f, 1f);
            bgmSlider.style.flexGrow = 1;
            bgmSlider.style.minWidth = 200;
            
            // AudioManagerから現在の音量を取得
            var audioManager = AudioManager.Instance;
            float currentVolume = 1.0f;
            if (audioManager != null)
            {
                currentVolume = audioManager.GetBGMVolume();
                bgmSlider.value = currentVolume;
            }
            else
            {
                bgmSlider.value = currentVolume;
            }

            var bgmValueLabel = new Label($"{Mathf.RoundToInt(currentVolume * 100)}%");
            bgmValueLabel.style.fontSize = 14;
            bgmValueLabel.style.color = Color.white;
            bgmValueLabel.style.minWidth = 50;
            bgmValueLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            // スライダー値変更時のコールバック（音量設定と値ラベル更新を同時に実行）
            bgmSlider.RegisterValueChangedCallback(evt =>
            {
                if (audioManager != null)
                {
                    audioManager.SetBGMVolume(evt.newValue);
                }
                bgmValueLabel.text = $"{Mathf.RoundToInt(evt.newValue * 100)}%";
            });

            bgmContainer.Add(bgmSlider);
            bgmContainer.Add(bgmValueLabel);

            settingsContainer.Add(bgmContainer);

            // 物語の解明度表示ON/OFFトグル
            var progressToggleContainer = new VisualElement();
            progressToggleContainer.style.flexDirection = FlexDirection.Row;
            progressToggleContainer.style.alignItems = Align.Center;
            progressToggleContainer.style.width = Length.Percent(80);
            progressToggleContainer.style.maxWidth = 500;

            var progressToggleLabel = new Label("物語の解明度表示:");
            progressToggleLabel.style.fontSize = 16;
            progressToggleLabel.style.color = Color.white;
            progressToggleLabel.style.minWidth = 180;
            progressToggleContainer.Add(progressToggleLabel);

            var progressToggle = new Toggle();
            progressToggle.value = gameManager.GetShowStoryProgress();
            progressToggle.RegisterValueChangedCallback(evt =>
            {
                gameManager.SetShowStoryProgress(evt.newValue);
                // 物語の解明度表示を即座に更新
                AddProgressDisplay(root);
            });
            progressToggleContainer.Add(progressToggle);

            settingsContainer.Add(progressToggleContainer);

            // 物語の解明度ラベルの下に挿入（ない場合はタイトルの下）
            var progressLabel = root.Q<Label>("MouhitotsuProgress");
            if (progressLabel != null && progressLabel.parent != null)
            {
                int insertIndex = progressLabel.parent.IndexOf(progressLabel) + 1;
                progressLabel.parent.Insert(insertIndex, settingsContainer);
            }
            else
            {
                // 物語の解明度が表示されていない場合、タイトルの下に挿入
                var title = root.Q<Label>("MouhitotsuTitle");
                if (title != null && title.parent != null)
                {
                    int insertIndex = title.parent.IndexOf(title) + 1;
                    title.parent.Insert(insertIndex, settingsContainer);
                }
            }
        }
    }
}
