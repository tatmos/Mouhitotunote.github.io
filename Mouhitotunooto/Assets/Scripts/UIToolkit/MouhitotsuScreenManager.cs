using System.Linq;
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

            // 進捗度と設定を横並びにするコンテナを作成
            var progressAndSettingsContainer = new VisualElement();
            progressAndSettingsContainer.style.flexDirection = FlexDirection.Row;
            progressAndSettingsContainer.style.alignItems = Align.Center;
            progressAndSettingsContainer.style.justifyContent = Justify.Center;
            progressAndSettingsContainer.style.width = Length.Percent(100);
            progressAndSettingsContainer.style.marginTop = 0;
            progressAndSettingsContainer.style.marginBottom = 8;
            progressAndSettingsContainer.style.flexWrap = Wrap.Wrap;

            // 進捗度の表示を追加
            var progressElement = AddProgressDisplay(root);
            if (progressElement != null)
            {
                progressElement.style.marginBottom = 0;
                progressElement.style.marginRight = 8;
                progressAndSettingsContainer.Add(progressElement);
            }

            // 設定UIを追加
            var settingsElement = AddSettingsUI(root);
            if (settingsElement != null)
            {
                settingsElement.style.marginTop = 0;
                settingsElement.style.marginBottom = 0;
                progressAndSettingsContainer.Add(settingsElement);
            }

            // タイトルの下に挿入
            var title = root.Q<Label>("MouhitotsuTitle");
            if (title != null && title.parent != null)
            {
                int insertIndex = title.parent.IndexOf(title) + 1;
                title.parent.Insert(insertIndex, progressAndSettingsContainer);
            }

            // 横スクロール可能なScrollViewでタイムラインを囲む
            var timelineScrollView = new ScrollView(ScrollViewMode.Horizontal);
            timelineScrollView.style.width = Length.Percent(100);
            timelineScrollView.style.height = Length.Auto(); // 高さは自動調整（コンテンツに合わせる）
            timelineScrollView.style.minHeight = 180; // 最小高さを設定（ボタンの高さ + 余白）
            timelineScrollView.style.flexShrink = 0; // 縮小しない
            timelineScrollView.style.flexGrow = 0; // 拡大しない（コンテンツに合わせる）
            timelineScrollView.style.overflow = Overflow.Hidden;

            // 時系列レイアウト用のコンテナ
            var timelineContainer = new VisualElement();
            timelineContainer.style.flexDirection = FlexDirection.Row;
            timelineContainer.style.alignItems = Align.FlexStart;
            timelineContainer.style.justifyContent = Justify.FlexStart;
            timelineContainer.style.width = StyleKeyword.Auto; // コンテンツに合わせて自動調整
            timelineContainer.style.minWidth = Length.Percent(100); // 最小幅は100%
            timelineContainer.style.height = Length.Auto(); // 高さは自動調整
            timelineContainer.style.paddingTop = 15;
            timelineContainer.style.paddingBottom = 15;
            timelineContainer.style.paddingLeft = 30;
            timelineContainer.style.paddingRight = 30;
            timelineContainer.style.flexWrap = Wrap.NoWrap;
            
            timelineScrollView.Add(timelineContainer);

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
            prologueContainer.style.minWidth = 120;
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
                preAContainer.style.minWidth = 120;
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
                abContainer.style.minWidth = 120;
                
                if (hasA)
                {
                    var btnA = CreateChapterButton("A", "Chapter A", "通常の物語", currentChapter);
                    btnA.style.marginBottom = 8;
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
                cContainer.style.minWidth = 120;
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
                preDContainer.style.minWidth = 120;
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
                deContainer.style.minWidth = 120;
                
                if (hasD)
                {
                    var btnD = CreateChapterButton("D", "Chapter D", "救済のエンド", currentChapter);
                    btnD.style.marginBottom = 8;
                    deContainer.Add(btnD);
                }
                
                if (hasE)
                {
                    var btnE = CreateChapterButton("E", "Chapter E", "終焉のエンド", currentChapter);
                    deContainer.Add(btnE);
                }
                
                timelineContainer.Add(deContainer);
            }

            container.Add(timelineScrollView);

            // スクロールバーのスタイルを適用（レンダリング後に実行されるようにコールバックを使用）
            timelineScrollView.RegisterCallback<GeometryChangedEvent>(evt => {
                ApplyHorizontalScrollbarStyle(timelineScrollView);
                ApplyVerticalScrollbarStyle(timelineScrollView); // 縦スクロールバーも適用
            });
            
            // 即座にも適用を試みる（既にレンダリングされている場合）
            ApplyHorizontalScrollbarStyle(timelineScrollView);
            ApplyVerticalScrollbarStyle(timelineScrollView); // 縦スクロールバーも適用
            
            // 複数のタイミングで適用（確実に適用されるように）
            timelineScrollView.schedule.Execute(() => {
                ApplyHorizontalScrollbarStyle(timelineScrollView);
                ApplyVerticalScrollbarStyle(timelineScrollView);
            }).ExecuteLater(50);
            
            timelineScrollView.schedule.Execute(() => {
                ApplyHorizontalScrollbarStyle(timelineScrollView);
                ApplyVerticalScrollbarStyle(timelineScrollView);
            }).ExecuteLater(100);
            
            timelineScrollView.schedule.Execute(() => {
                ApplyHorizontalScrollbarStyle(timelineScrollView);
                ApplyVerticalScrollbarStyle(timelineScrollView);
            }).ExecuteLater(200);
            
            // レイアウト変更時にも適用
            timelineScrollView.RegisterCallback<AttachToPanelEvent>(evt => {
                timelineScrollView.schedule.Execute(() => {
                    ApplyHorizontalScrollbarStyle(timelineScrollView);
                    ApplyVerticalScrollbarStyle(timelineScrollView);
                }).ExecuteLater(50);
            });

            // MouhitotsuScrollViewの縦スクロールバーのスタイルを適用
            ApplyMouhitotsuScrollViewStyle(root);
        }

        /// <summary>
        /// MouhitotsuScrollViewの縦スクロールバーのスタイルを適用
        /// </summary>
        private void ApplyMouhitotsuScrollViewStyle(VisualElement root)
        {
            var mouhitotsuScrollView = root.Q<ScrollView>("MouhitotsuScrollView");
            if (mouhitotsuScrollView != null)
            {
                // レンダリング後に実行されるようにコールバックを使用
                mouhitotsuScrollView.RegisterCallback<GeometryChangedEvent>(evt => {
                    ApplyVerticalScrollbarStyle(mouhitotsuScrollView);
                });
                
                // パネルにアタッチされた時にも適用
                mouhitotsuScrollView.RegisterCallback<AttachToPanelEvent>(evt => {
                    mouhitotsuScrollView.schedule.Execute(() => {
                        ApplyVerticalScrollbarStyle(mouhitotsuScrollView);
                    }).ExecuteLater(50);
                });
                
                // 即座にも適用を試みる（既にレンダリングされている場合）
                ApplyVerticalScrollbarStyle(mouhitotsuScrollView);
                
                // 複数のタイミングで適用（確実に適用されるように）
                mouhitotsuScrollView.schedule.Execute(() => {
                    ApplyVerticalScrollbarStyle(mouhitotsuScrollView);
                }).ExecuteLater(50);
                
                mouhitotsuScrollView.schedule.Execute(() => {
                    ApplyVerticalScrollbarStyle(mouhitotsuScrollView);
                }).ExecuteLater(100);
                
                mouhitotsuScrollView.schedule.Execute(() => {
                    ApplyVerticalScrollbarStyle(mouhitotsuScrollView);
                }).ExecuteLater(200);
                
                mouhitotsuScrollView.schedule.Execute(() => {
                    ApplyVerticalScrollbarStyle(mouhitotsuScrollView);
                }).ExecuteLater(500);
            }
        }

        /// <summary>
        /// 横スクロールバーのスタイルを適用（ゲームのデザインに合わせる）
        /// </summary>
        private void ApplyHorizontalScrollbarStyle(ScrollView scrollView)
        {
            if (scrollView == null) return;

            // 横スクロールバーのコンテナ
            var horizontalScroller = scrollView.Q<VisualElement>(className: "unity-scroll-view__horizontal-scroller");
            if (horizontalScroller != null)
            {
                horizontalScroller.style.height = 10;
                horizontalScroller.style.backgroundColor = new Color(0, 0, 0, 0.3f);
                horizontalScroller.style.borderTopLeftRadius = 5;
                horizontalScroller.style.borderTopRightRadius = 5;
                horizontalScroller.style.borderBottomLeftRadius = 5;
                horizontalScroller.style.borderBottomRightRadius = 5;
                // 枠を非表示
                horizontalScroller.style.borderTopWidth = 0;
                horizontalScroller.style.borderRightWidth = 0;
                horizontalScroller.style.borderBottomWidth = 0;
                horizontalScroller.style.borderLeftWidth = 0;

                // スクロールバー内のすべての子要素を検索してボタンを非表示
                var allChildren = horizontalScroller.Children().ToList();
                foreach (var child in allChildren)
                {
                    // ボタンっぽい要素を検索（クラス名や名前で判断）
                    string className = string.Join(" ", child.GetClasses());
                    string name = child.name;
                    
                    if (className.Contains("button") || className.Contains("Button") ||
                        className.Contains("left") || className.Contains("right") ||
                        name.Contains("button") || name.Contains("Button") ||
                        name.Contains("left") || name.Contains("right") ||
                        name.Contains("Left") || name.Contains("Right"))
                    {
                        child.style.display = DisplayStyle.None;
                    }
                    
                    // Button型の要素も非表示
                    if (child is Button)
                    {
                        child.style.display = DisplayStyle.None;
                    }
                }

                // ドラッガーを検索
                var dragger = horizontalScroller.Q<VisualElement>(className: "unity-base-slider__dragger");
                if (dragger != null)
                {
                    dragger.style.backgroundColor = new Color(218f / 255f, 165f / 255f, 32f / 255f, 0.8f);
                    dragger.style.borderTopLeftRadius = 4;
                    dragger.style.borderTopRightRadius = 4;
                    dragger.style.borderBottomLeftRadius = 4;
                    dragger.style.borderBottomRightRadius = 4;
                    dragger.style.height = 8;
                    dragger.style.marginLeft = 1;
                    dragger.style.marginRight = 1;
                    dragger.style.marginTop = 1;
                    dragger.style.marginBottom = 1;
                    // 枠を非表示
                    dragger.style.borderTopWidth = 0;
                    dragger.style.borderRightWidth = 0;
                    dragger.style.borderBottomWidth = 0;
                    dragger.style.borderLeftWidth = 0;
                }

                // トラッカーを検索
                var tracker = horizontalScroller.Q<VisualElement>(className: "unity-base-slider__tracker");
                if (tracker != null)
                {
                    tracker.style.backgroundColor = Color.clear;
                    // 枠を非表示
                    tracker.style.borderTopWidth = 0;
                    tracker.style.borderRightWidth = 0;
                    tracker.style.borderBottomWidth = 0;
                    tracker.style.borderLeftWidth = 0;
                }
            }
        }

        /// <summary>
        /// 進捗度表示を追加
        /// </summary>
        private VisualElement AddProgressDisplay(VisualElement root)
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
                return null;
            }

            int percentage = gameManager.GetStoryProgressPercentage();

            var progressLabel = new Label($"物語の解明度: {percentage}%");
            progressLabel.name = "MouhitotsuProgress";
            progressLabel.style.fontSize = 18;
            progressLabel.style.marginBottom = 0;
            progressLabel.style.marginTop = 0;
            progressLabel.style.marginLeft = 0;
            progressLabel.style.marginRight = 0;
            progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            progressLabel.style.color = new Color(0.8f, 0.8f, 1f);
            progressLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            progressLabel.style.paddingLeft = 8;
            progressLabel.style.paddingRight = 8;
            progressLabel.style.paddingTop = 4;
            progressLabel.style.paddingBottom = 4;

            return progressLabel;
        }

        /// <summary>
        /// 設定UIを追加（物語の解明度表示ON/OFF、チートモード）
        /// </summary>
        private VisualElement AddSettingsUI(VisualElement root)
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
            settingsContainer.style.flexDirection = FlexDirection.Row;
            settingsContainer.style.alignItems = Align.Center;
            settingsContainer.style.justifyContent = Justify.Center;
            settingsContainer.style.width = StyleKeyword.Auto;
            settingsContainer.style.marginTop = 0;
            settingsContainer.style.marginBottom = 0;
            settingsContainer.style.marginLeft = 0;
            settingsContainer.style.marginRight = 0;
            settingsContainer.style.paddingTop = 8;
            settingsContainer.style.paddingBottom = 8;
            settingsContainer.style.paddingLeft = 12;
            settingsContainer.style.paddingRight = 12;
            settingsContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            settingsContainer.style.borderTopLeftRadius = 8;
            settingsContainer.style.borderTopRightRadius = 8;
            settingsContainer.style.borderBottomLeftRadius = 8;
            settingsContainer.style.borderBottomRightRadius = 8;

            // 物語の解明度表示ON/OFFトグル
            var progressToggleLabel = new Label("物語の解明度表示:");
            progressToggleLabel.style.fontSize = 12;
            progressToggleLabel.style.color = Color.white;
            progressToggleLabel.style.marginRight = 4;
            settingsContainer.Add(progressToggleLabel);

            var progressToggle = new Toggle();
            progressToggle.value = gameManager.GetShowStoryProgress();
            progressToggle.RegisterValueChangedCallback(evt =>
            {
                gameManager.SetShowStoryProgress(evt.newValue);
                // 物語の解明度表示を即座に更新
                var container = root.Q<VisualElement>("MouhitotsuContainer");
                if (container != null)
                {
                    CreateRetryButtons(root);
                }
            });
            settingsContainer.Add(progressToggle);

            // チートモードチェックボックス
            var cheatModeLabel = new Label("チートモード:");
            cheatModeLabel.style.fontSize = 12;
            cheatModeLabel.style.color = new Color(1f, 0.8f, 0.8f);
            cheatModeLabel.style.marginLeft = 8;
            cheatModeLabel.style.marginRight = 4;
            settingsContainer.Add(cheatModeLabel);

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
            settingsContainer.Add(cheatModeToggle);

            return settingsContainer;
        }

        /// <summary>
        /// Chapterボタンを作成
        /// </summary>
        private Button CreateChapterButton(string id, string name, string description, string currentChapter)
        {
            Button btn = new Button();
            btn.text = $"{name}\n{description}";
            btn.style.fontSize = 14;
            btn.style.paddingLeft = 15;
            btn.style.paddingRight = 15;
            btn.style.paddingTop = 10;
            btn.style.paddingBottom = 10;
            btn.style.marginBottom = 0;
            btn.style.minWidth = 120;
            btn.style.maxWidth = 120;
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
            line.style.width = 30;
            line.style.height = 3;
            line.style.backgroundColor = new Color(0.5f, 0.5f, 0.6f);
            line.style.marginTop = 20; // ボタンの中央に合わせる
            line.style.alignSelf = Align.Center;
            return line;
        }

        /// <summary>
        /// 縦スクロールバーのスタイルを適用（ゲームのデザインに合わせる）
        /// </summary>
        private void ApplyVerticalScrollbarStyle(ScrollView scrollView)
        {
            if (scrollView == null) return;

            // 縦スクロールバーのコンテナ（複数の方法で確実に検索）
            VisualElement verticalScroller = null;
            
            // 方法1: 直接検索
            verticalScroller = scrollView.Q<VisualElement>(className: "unity-scroll-view__vertical-scroller");
            
            // 方法2: すべての子要素から検索
            if (verticalScroller == null)
            {
                var scrollViewChildren = scrollView.Children().ToList();
                foreach (var child in scrollViewChildren)
                {
                    if (child.ClassListContains("unity-scroll-view__vertical-scroller"))
                    {
                        verticalScroller = child;
                        break;
                    }
                }
            }
            
            // 方法3: Queryを使用して検索
            if (verticalScroller == null)
            {
                var scrollers = scrollView.Query<VisualElement>(className: "unity-scroll-view__vertical-scroller").ToList();
                if (scrollers.Count > 0)
                {
                    verticalScroller = scrollers[0];
                }
            }
            
            // 方法4: すべての子孫要素から検索（最も確実）
            if (verticalScroller == null)
            {
                var scrollViewDescendants = scrollView.Query<VisualElement>(className: "unity-scroll-view__vertical-scroller").ToList();
                if (scrollViewDescendants.Count > 0)
                {
                    verticalScroller = scrollViewDescendants[0];
                }
            }
            
            if (verticalScroller == null)
            {
                // デバッグ: スクロールバーが見つからない場合
                Debug.LogWarning("[MouhitotsuScreenManager] 縦スクロールバーのコンテナが見つかりませんでした。");
                return;
            }
            
            // コンテナのスタイルを適用（背景は透明、ボタンは非表示）
            verticalScroller.style.width = 10;
            verticalScroller.style.backgroundColor = Color.clear; // 背景を透明に
            verticalScroller.style.borderTopLeftRadius = 5;
            verticalScroller.style.borderTopRightRadius = 5;
            verticalScroller.style.borderBottomLeftRadius = 5;
            verticalScroller.style.borderBottomRightRadius = 5;
            verticalScroller.style.borderTopWidth = 0;
            verticalScroller.style.borderRightWidth = 0;
            verticalScroller.style.borderBottomWidth = 0;
            verticalScroller.style.borderLeftWidth = 0;
            // 重要度を最高に設定して、デフォルトスタイルを上書き
            verticalScroller.style.width = new StyleLength(new Length(10, LengthUnit.Pixel));
            verticalScroller.MarkDirtyRepaint();

            // スクロールバー内のすべての子要素を検索
            var scrollerChildren = verticalScroller.Children().ToList();
            foreach (var child in scrollerChildren)
            {
                // ボタン要素を非表示（上矢印・下矢印ボタン）
                string className = string.Join(" ", child.GetClasses());
                string name = child.name;
                
                // ボタンっぽい要素を非表示
                if (className.Contains("button") || className.Contains("Button") ||
                    className.Contains("up") || className.Contains("down") ||
                    className.Contains("scrollbar") && (className.Contains("up") || className.Contains("down")) ||
                    name.Contains("button") || name.Contains("Button") ||
                    name.Contains("up") || name.Contains("down") ||
                    name.Contains("Up") || name.Contains("Down"))
                {
                    child.style.display = DisplayStyle.None;
                    child.style.visibility = Visibility.Hidden;
                }
                
                // Button型の要素も非表示
                if (child is Button)
                {
                    child.style.display = DisplayStyle.None;
                    child.style.visibility = Visibility.Hidden;
                }
            }

            // ドラッガー（つまみ）を検索（複数のパターンで確実に検索）
            VisualElement dragger = null;
            
            // 方法1: 直接検索
            dragger = verticalScroller.Q<VisualElement>(className: "unity-base-slider__dragger");
            
            // 方法2: Slider内のドラッガーを検索
            if (dragger == null)
            {
                var slider = verticalScroller.Q<Slider>();
                if (slider != null)
                {
                    dragger = slider.Q<VisualElement>(className: "unity-base-slider__dragger");
                }
            }
            
            // 方法3: すべての子孫要素から検索
            if (dragger == null)
            {
                var draggerDescendants = verticalScroller.Query<VisualElement>(className: "unity-base-slider__dragger").ToList();
                if (draggerDescendants.Count > 0)
                {
                    dragger = draggerDescendants[0];
                }
            }
            
            if (dragger != null)
            {
                // ドラッガーのスタイルを適用（重要度: 最高）
                dragger.style.backgroundColor = new Color(218f / 255f, 165f / 255f, 32f / 255f, 0.8f);
                dragger.style.borderTopLeftRadius = 4;
                dragger.style.borderTopRightRadius = 4;
                dragger.style.borderBottomLeftRadius = 4;
                dragger.style.borderBottomRightRadius = 4;
                dragger.style.width = 8;
                dragger.style.marginLeft = 1;
                dragger.style.marginRight = 1;
                dragger.style.marginTop = 1;
                dragger.style.marginBottom = 1;
                dragger.style.borderTopWidth = 0;
                dragger.style.borderRightWidth = 0;
                dragger.style.borderBottomWidth = 0;
                dragger.style.borderLeftWidth = 0;
                dragger.MarkDirtyRepaint();
            }
            else
            {
                Debug.LogWarning("[MouhitotsuScreenManager] スクロールバーのドラッガーが見つかりませんでした。");
            }

            // トラッカー（背景）を検索（複数のパターンで確実に検索）
            VisualElement tracker = null;
            
            // 方法1: 直接検索
            tracker = verticalScroller.Q<VisualElement>(className: "unity-base-slider__tracker");
            
            // 方法2: Slider内のトラッカーを検索
            if (tracker == null)
            {
                var slider = verticalScroller.Q<Slider>();
                if (slider != null)
                {
                    tracker = slider.Q<VisualElement>(className: "unity-base-slider__tracker");
                }
            }
            
            // 方法3: すべての子孫要素から検索
            if (tracker == null)
            {
                var trackerDescendants = verticalScroller.Query<VisualElement>(className: "unity-base-slider__tracker").ToList();
                if (trackerDescendants.Count > 0)
                {
                    tracker = trackerDescendants[0];
                }
            }
            
            if (tracker != null)
            {
                // トラッカーのスタイルを適用
                tracker.style.backgroundColor = Color.clear;
                tracker.style.borderTopWidth = 0;
                tracker.style.borderRightWidth = 0;
                tracker.style.borderBottomWidth = 0;
                tracker.style.borderLeftWidth = 0;
                tracker.MarkDirtyRepaint();
            }
            
            // すべてのSlider要素にも直接スタイルを適用（念のため）
            var allSliders = verticalScroller.Query<Slider>().ToList();
            foreach (var slider in allSliders)
            {
                // Slider内のドラッガーとトラッカーを再検索
                var sliderDragger = slider.Q<VisualElement>(className: "unity-base-slider__dragger");
                if (sliderDragger != null)
                {
                    sliderDragger.style.backgroundColor = new Color(218f / 255f, 165f / 255f, 32f / 255f, 0.8f);
                    sliderDragger.style.borderTopLeftRadius = 4;
                    sliderDragger.style.borderTopRightRadius = 4;
                    sliderDragger.style.borderBottomLeftRadius = 4;
                    sliderDragger.style.borderBottomRightRadius = 4;
                    sliderDragger.style.width = 8;
                    sliderDragger.style.marginLeft = 1;
                    sliderDragger.style.marginRight = 1;
                    sliderDragger.style.marginTop = 1;
                    sliderDragger.style.marginBottom = 1;
                    sliderDragger.style.borderTopWidth = 0;
                    sliderDragger.style.borderRightWidth = 0;
                    sliderDragger.style.borderBottomWidth = 0;
                    sliderDragger.style.borderLeftWidth = 0;
                    sliderDragger.MarkDirtyRepaint();
                }
                
                var sliderTracker = slider.Q<VisualElement>(className: "unity-base-slider__tracker");
                if (sliderTracker != null)
                {
                    sliderTracker.style.backgroundColor = Color.clear;
                    sliderTracker.style.borderTopWidth = 0;
                    sliderTracker.style.borderRightWidth = 0;
                    sliderTracker.style.borderBottomWidth = 0;
                    sliderTracker.style.borderLeftWidth = 0;
                    sliderTracker.MarkDirtyRepaint();
                }
            }
        }
    }
}
