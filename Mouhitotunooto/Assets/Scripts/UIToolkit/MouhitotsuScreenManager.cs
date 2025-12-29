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
                title.parent.Insert(title.parent.IndexOf(title) + 1, progressLabel);
            }
        }
    }
}
