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
        private System.Action<string> onChapterJump;
        private System.Action<string, System.Action> onShowConfirmationDialog;

        public MouhitotsuScreenManager(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        /// <summary>
        /// Chapterジャンプ時のコールバックを設定
        /// </summary>
        public void SetOnChapterJumpCallback(System.Action<string> callback)
        {
            this.onChapterJump = callback;
        }

        /// <summary>
        /// ホバー音再生用のコールバックを設定
        /// </summary>
        public void SetOnHoverSoundCallback(System.Action callback)
        {
            this.onHoverSound = callback;
        }

        /// <summary>
        /// 確認ダイアログ表示用のコールバックを設定
        /// </summary>
        public void SetOnShowConfirmationDialogCallback(System.Action<string, System.Action> callback)
        {
            this.onShowConfirmationDialog = callback;
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

            // 時系列レイアウト用のコンテナ
            var timelineContainer = new VisualElement();
            timelineContainer.style.flexDirection = FlexDirection.Row;
            timelineContainer.style.alignItems = Align.FlexStart;
            timelineContainer.style.justifyContent = Justify.FlexStart;
            timelineContainer.style.width = Length.Percent(100);
            timelineContainer.style.paddingTop = 20;
            timelineContainer.style.paddingBottom = 20;
            timelineContainer.style.paddingLeft = 40;
            timelineContainer.style.paddingRight = 40;
            timelineContainer.style.flexWrap = Wrap.NoWrap;

            // 現在のChapterを取得
            string currentChapter = "Prologue";
            if (ChapterManager.Instance != null)
            {
                currentChapter = ChapterManager.Instance.GetCurrentChapter(gameManager);
            }

            // Chapterの定義（時系列順）
            // Prologue → PreA → A/B（分岐） → C → PreD → D/E（分岐）
            
            // Prologue
            var prologueBtn = CreateChapterButton("Prologue", "プロローグ", "物語の始まり", currentChapter);
            var prologueContainer = new VisualElement();
            prologueContainer.style.flexDirection = FlexDirection.Column;
            prologueContainer.style.alignItems = Align.Center;
            prologueContainer.style.minWidth = 150;
            prologueContainer.Add(prologueBtn);
            timelineContainer.Add(prologueContainer);
            
            // PreA
            if (gameManager.IsChapterCleared("PreA"))
            {
                var line = CreateConnectionLine();
                timelineContainer.Add(line);
                
                var preABtn = CreateChapterButton("PreA", "PreA", "真実の扉が開いた", currentChapter);
                var preAContainer = new VisualElement();
                preAContainer.style.flexDirection = FlexDirection.Column;
                preAContainer.style.alignItems = Align.Center;
                preAContainer.style.minWidth = 150;
                preAContainer.Add(preABtn);
                timelineContainer.Add(preAContainer);
            }

            // A/B（分岐）
            bool hasA = gameManager.IsChapterCleared("A");
            bool hasB = gameManager.IsChapterCleared("B");
            if (hasA || hasB)
            {
                var line = CreateConnectionLine();
                timelineContainer.Add(line);
                
                var abContainer = new VisualElement();
                abContainer.style.flexDirection = FlexDirection.Column;
                abContainer.style.alignItems = Align.Center;
                abContainer.style.minWidth = 150;
                
                if (hasA)
                {
                    var btnA = CreateChapterButton("A", "Chapter A", "通常の物語", currentChapter);
                    btnA.style.marginBottom = 10;
                    abContainer.Add(btnA);
                }
                
                if (hasB)
                {
                    var btnB = CreateChapterButton("B", "Chapter B", "ダークモードへの予兆", currentChapter);
                    abContainer.Add(btnB);
                }
                
                timelineContainer.Add(abContainer);
            }

            // C
            if (gameManager.IsChapterCleared("C"))
            {
                var line = CreateConnectionLine();
                timelineContainer.Add(line);
                
                var cBtn = CreateChapterButton("C", "Chapter C", "3周目への門", currentChapter);
                var cContainer = new VisualElement();
                cContainer.style.flexDirection = FlexDirection.Column;
                cContainer.style.alignItems = Align.Center;
                cContainer.style.minWidth = 150;
                cContainer.Add(cBtn);
                timelineContainer.Add(cContainer);
            }

            // PreD
            if (gameManager.IsChapterCleared("PreD"))
            {
                var line = CreateConnectionLine();
                timelineContainer.Add(line);
                
                var preDBtn = CreateChapterButton("PreD", "PreD", "真実の扉が開いた（3周目）", currentChapter);
                var preDContainer = new VisualElement();
                preDContainer.style.flexDirection = FlexDirection.Column;
                preDContainer.style.alignItems = Align.Center;
                preDContainer.style.minWidth = 150;
                preDContainer.Add(preDBtn);
                timelineContainer.Add(preDContainer);
            }

            // D/E（分岐）
            bool hasD = gameManager.IsChapterCleared("D");
            bool hasE = gameManager.IsChapterCleared("E");
            if (hasD || hasE)
            {
                var line = CreateConnectionLine();
                timelineContainer.Add(line);
                
                var deContainer = new VisualElement();
                deContainer.style.flexDirection = FlexDirection.Column;
                deContainer.style.alignItems = Align.Center;
                deContainer.style.minWidth = 150;
                
                if (hasD)
                {
                    var btnD = CreateChapterButton("D", "Chapter D", "救済のエンド", currentChapter);
                    btnD.style.marginBottom = 10;
                    deContainer.Add(btnD);
                }
                
                if (hasE)
                {
                    var btnE = CreateChapterButton("E", "Chapter E", "終焉のエンド", currentChapter);
                    deContainer.Add(btnE);
                }
                
                timelineContainer.Add(deContainer);
            }

            container.Add(timelineContainer);
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
        /// 設定UIを追加（物語の解明度表示ON/OFF、チートモード）
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

            // 物語の解明度表示ON/OFFトグル
            var progressToggleContainer = new VisualElement();
            progressToggleContainer.style.flexDirection = FlexDirection.Row;
            progressToggleContainer.style.alignItems = Align.Center;
            progressToggleContainer.style.width = Length.Percent(80);
            progressToggleContainer.style.maxWidth = 500;
            progressToggleContainer.style.marginBottom = 15;

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

            // チートモードチェックボックス
            var cheatModeContainer = new VisualElement();
            cheatModeContainer.style.flexDirection = FlexDirection.Row;
            cheatModeContainer.style.alignItems = Align.Center;
            cheatModeContainer.style.width = Length.Percent(80);
            cheatModeContainer.style.maxWidth = 500;

            var cheatModeLabel = new Label("チートモード:");
            cheatModeLabel.style.fontSize = 16;
            cheatModeLabel.style.color = new Color(1f, 0.8f, 0.8f);
            cheatModeLabel.style.minWidth = 180;
            cheatModeContainer.Add(cheatModeLabel);

            var cheatModeToggle = new Toggle();
            // 現在のチートモード状態を取得
            bool isCheatModeEnabled = false;
            if (ChapterManager.Instance != null)
            {
                isCheatModeEnabled = ChapterManager.Instance.GetDebugShowAllChapters();
            }
            cheatModeToggle.value = isCheatModeEnabled;
            cheatModeToggle.RegisterValueChangedCallback(evt =>
            {
                // チェックボックスが変更されたとき、確認ダイアログを表示
                if (evt.newValue && !evt.previousValue)
                {
                    // チェックが入った場合のみ確認ダイアログを表示
                    if (onShowConfirmationDialog != null)
                    {
                        onShowConfirmationDialog("ネタばれを含みますが、チートモードを有効にしますか？", () =>
                        {
                            // 「はい」が選択された場合
                            if (ChapterManager.Instance != null)
                            {
                                ChapterManager.Instance.SetDebugShowAllChapters(true);
                            }
                            // 画面を再生成してすべてのチャプターを表示
                            var container = root.Q<VisualElement>("MouhitotsuContainer");
                            if (container != null)
                            {
                                CreateRetryButtons(root);
                            }
                        });
                        // ダイアログが表示されたので、チェックボックスを元に戻す（確認後に設定される）
                        cheatModeToggle.SetValueWithoutNotify(evt.previousValue);
                    }
                }
                else if (!evt.newValue && evt.previousValue)
                {
                    // チェックが外された場合は即座に無効化
                    if (ChapterManager.Instance != null)
                    {
                        ChapterManager.Instance.SetDebugShowAllChapters(false);
                    }
                    // 画面を再生成
                    var container = root.Q<VisualElement>("MouhitotsuContainer");
                    if (container != null)
                    {
                        CreateRetryButtons(root);
                    }
                }
            });
            cheatModeContainer.Add(cheatModeToggle);

            settingsContainer.Add(cheatModeContainer);

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

        /// <summary>
        /// Chapterボタンを作成
        /// </summary>
        private Button CreateChapterButton(string id, string name, string description, string currentChapter)
        {
            Button btn = new Button();
            btn.text = $"{name}\n{description}";
            btn.style.fontSize = 16;
            btn.style.paddingLeft = 20;
            btn.style.paddingRight = 20;
            btn.style.paddingTop = 12;
            btn.style.paddingBottom = 12;
            btn.style.marginBottom = 0;
            btn.style.minWidth = 150;
            btn.style.maxWidth = 150;
            btn.style.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 5;
            btn.style.borderTopRightRadius = 5;
            btn.style.borderBottomLeftRadius = 5;
            btn.style.borderBottomRightRadius = 5;
            btn.style.unityTextAlign = TextAnchor.MiddleCenter;
            btn.style.whiteSpace = WhiteSpace.Normal;

            // 現在のChapterの場合は黄色枠を追加
            if (id == currentChapter)
            {
                btn.style.borderLeftWidth = 4;
                btn.style.borderRightWidth = 4;
                btn.style.borderTopWidth = 4;
                btn.style.borderBottomWidth = 4;
                btn.style.borderLeftColor = Color.yellow;
                btn.style.borderRightColor = Color.yellow;
                btn.style.borderTopColor = Color.yellow;
                btn.style.borderBottomColor = Color.yellow;
            }

            btn.clicked += () => onChapterJump?.Invoke(id);
            btn.RegisterCallback<PointerEnterEvent>(evt => onHoverSound?.Invoke());

            return btn;
        }

        /// <summary>
        /// 接続線を作成
        /// </summary>
        private VisualElement CreateConnectionLine()
        {
            var line = new VisualElement();
            line.style.width = 40;
            line.style.height = 3;
            line.style.backgroundColor = new Color(0.5f, 0.5f, 0.6f);
            line.style.marginTop = 30; // ボタンの中央に合わせる
            line.style.alignSelf = Align.Center;
            return line;
        }
    }
}
