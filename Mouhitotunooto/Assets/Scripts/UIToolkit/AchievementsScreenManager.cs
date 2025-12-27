using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// 実績画面の表示を管理するクラス
    /// </summary>
    public class AchievementsScreenManager
    {
        private GameManager gameManager;

        public AchievementsScreenManager(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        /// <summary>
        /// 実績カードを作成
        /// </summary>
        public void CreateAchievements(VisualElement container)
        {
            container.Clear();

            var scenarios = gameManager.GetScenarios();
            int totalCompleted = 0;
            foreach (var scenario in scenarios)
            {
                if (gameManager.IsScenarioCompleted(scenario.id))
                {
                    totalCompleted++;
                }
            }

            // 全シナリオクリア後のみ表示
            if (totalCompleted < scenarios.Count)
            {
                return;
            }

            var gridContainer = new VisualElement();
            gridContainer.style.flexDirection = FlexDirection.Row;
            gridContainer.style.flexWrap = Wrap.Wrap;
            gridContainer.style.justifyContent = Justify.Center;
            gridContainer.AddToClassList("achievement-grid");
            gridContainer.style.width = Length.Percent(100);

            // シナリオ1-5のエンド
            for (int i = 1; i <= 5; i++)
            {
                var scenario = scenarios.Find(s => s.id == i);
                if (scenario == null) continue;

                var trueChoiceId = scenario.choices.Find(c => scenario.branches.ContainsKey(c.id) && scenario.branches[c.id].hasWord)?.id ?? -1;
                var falseChoiceId = scenario.choices.Find(c => scenario.branches.ContainsKey(c.id) && !scenario.branches[c.id].hasWord)?.id ?? -1;

                // 見たエンドを記録から取得
                bool trueEndSeen = trueChoiceId != -1 && gameManager.HasSeenEnd(i, trueChoiceId);
                bool falseEndSeen = falseChoiceId != -1 && gameManager.HasSeenEnd(i, falseChoiceId);

                var scenarioCard = CreateAchievementCard(scenario.title, trueEndSeen, falseEndSeen, true);
                gridContainer.Add(scenarioCard);
            }

            // 真実の扉のエンド
            var scenario6 = scenarios.Find(s => s.id == 6);
            if (scenario6 != null)
            {
                // 見たエンドを記録から取得（通常モード/ダークモードを区別）
                bool trueEndSeen = gameManager.HasSeenEnd(6, 2, false); // choiceId 2 はTrueエンド（通常モード）
                bool falseEndSeen = gameManager.HasSeenEnd(6, 1, false); // choiceId 1 はFalseエンド（通常モード）
                bool darkModeEnd1Seen = gameManager.HasSeenEnd(6, 1, true); // choiceId 1 はダークエンド1（ダークモード）
                bool darkModeEnd2Seen = gameManager.HasSeenEnd(6, 2, true); // choiceId 2 はダークエンド2（ダークモード）

                var scenario6Card = CreateAchievementCardForScenario6(trueEndSeen, falseEndSeen, darkModeEnd1Seen, darkModeEnd2Seen);
                gridContainer.Add(scenario6Card);
            }

            container.Add(gridContainer);
        }

        /// <summary>
        /// 通常シナリオの実績カードを作成
        /// </summary>
        private VisualElement CreateAchievementCard(string scenarioTitle, bool trueEndSeen, bool falseEndSeen, bool isNormalScenario)
        {
            var card = new VisualElement();
            card.AddToClassList("achievement-card");
            card.style.width = 300;
            card.style.marginBottom = 16;

            var titleLabel = new Label(scenarioTitle);
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 12;
            card.Add(titleLabel);

            var endsContainer = new VisualElement();
            endsContainer.style.flexDirection = FlexDirection.Column;
            endsContainer.AddToClassList("achievement-ends-container");

            // Trueエンド
            var trueEndBox = new VisualElement();
            trueEndBox.AddToClassList(trueEndSeen ? "achievement-end-unlocked" : "achievement-end-locked");
            var trueEndLabel = new Label("✨ Trueエンド");
            trueEndLabel.style.fontSize = 14;
            trueEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            trueEndLabel.style.color = trueEndSeen ? new Color(0.13f, 0.4f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            trueEndBox.Add(trueEndLabel);
            if (trueEndSeen)
            {
                var trueEndDesc = new Label("【もうひとつ】を獲得したエンド");
                trueEndDesc.style.fontSize = 12;
                trueEndDesc.style.marginTop = 4;
                trueEndBox.Add(trueEndDesc);
            }
            endsContainer.Add(trueEndBox);

            // Falseエンド
            var falseEndBox = new VisualElement();
            falseEndBox.AddToClassList(falseEndSeen ? "achievement-end-unlocked-false" : "achievement-end-locked");
            var falseEndLabel = new Label("❌ Falseエンド");
            falseEndLabel.style.fontSize = 14;
            falseEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            falseEndLabel.style.color = falseEndSeen ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.5f, 0.5f, 0.5f);
            falseEndBox.Add(falseEndLabel);
            if (falseEndSeen)
            {
                var falseEndDesc = new Label("【もうひとつ】を獲得できなかったエンド");
                falseEndDesc.style.fontSize = 12;
                falseEndDesc.style.marginTop = 4;
                falseEndBox.Add(falseEndDesc);
            }
            endsContainer.Add(falseEndBox);

            card.Add(endsContainer);
            return card;
        }

        /// <summary>
        /// シナリオ6の実績カードを作成
        /// </summary>
        private VisualElement CreateAchievementCardForScenario6(bool trueEndSeen, bool falseEndSeen, bool darkModeEnd1Seen, bool darkModeEnd2Seen)
        {
            var card = new VisualElement();
            card.AddToClassList("achievement-card");
            card.style.width = 300;
            card.style.marginBottom = 16;

            var titleLabel = new Label("真実の扉");
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 12;
            card.Add(titleLabel);

            var endsContainer = new VisualElement();
            endsContainer.style.flexDirection = FlexDirection.Column;
            endsContainer.AddToClassList("achievement-ends-container");

            // Trueエンド
            var trueEndBox = new VisualElement();
            trueEndBox.AddToClassList(trueEndSeen ? "achievement-end-unlocked" : "achievement-end-locked");
            var trueEndLabel = new Label("✨ Trueエンド");
            trueEndLabel.style.fontSize = 14;
            trueEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            trueEndLabel.style.color = trueEndSeen ? new Color(0.13f, 0.4f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            trueEndBox.Add(trueEndLabel);
            if (trueEndSeen)
            {
                var trueEndDesc = new Label("「答えを知りたかった」を選んだエンド");
                trueEndDesc.style.fontSize = 12;
                trueEndDesc.style.marginTop = 4;
                trueEndBox.Add(trueEndDesc);
            }
            endsContainer.Add(trueEndBox);

            // Falseエンド
            var falseEndBox = new VisualElement();
            falseEndBox.AddToClassList(falseEndSeen ? "achievement-end-unlocked-false" : "achievement-end-locked");
            var falseEndLabel = new Label("❌ Falseエンド");
            falseEndLabel.style.fontSize = 14;
            falseEndLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            falseEndLabel.style.color = falseEndSeen ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.5f, 0.5f, 0.5f);
            falseEndBox.Add(falseEndLabel);
            if (falseEndSeen)
            {
                var falseEndDesc = new Label("「好奇心から」を選んだエンド");
                falseEndDesc.style.fontSize = 12;
                falseEndDesc.style.marginTop = 4;
                falseEndBox.Add(falseEndDesc);
            }
            endsContainer.Add(falseEndBox);

            // ダークエンド1
            var darkEnd1Box = new VisualElement();
            darkEnd1Box.AddToClassList(darkModeEnd1Seen ? "achievement-end-dark" : "achievement-end-locked");
            var darkEnd1Label = new Label("⚠️ ダークエンド1");
            darkEnd1Label.style.fontSize = 14;
            darkEnd1Label.style.unityFontStyleAndWeight = FontStyle.Bold;
            darkEnd1Label.style.color = darkModeEnd1Seen ? new Color(1f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
            darkEnd1Box.Add(darkEnd1Label);
            if (darkModeEnd1Seen)
            {
                var darkEnd1Desc = new Label("「すみません...」と謝ったエンド");
                darkEnd1Desc.style.fontSize = 12;
                darkEnd1Desc.style.marginTop = 4;
                darkEnd1Box.Add(darkEnd1Desc);
            }
            endsContainer.Add(darkEnd1Box);

            // ダークエンド2
            var darkEnd2Box = new VisualElement();
            darkEnd2Box.AddToClassList(darkModeEnd2Seen ? "achievement-end-dark" : "achievement-end-locked");
            var darkEnd2Label = new Label("⚠️ ダークエンド2");
            darkEnd2Label.style.fontSize = 14;
            darkEnd2Label.style.unityFontStyleAndWeight = FontStyle.Bold;
            darkEnd2Label.style.color = darkModeEnd2Seen ? new Color(1f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
            darkEnd2Box.Add(darkEnd2Label);
            if (darkModeEnd2Seen)
            {
                var darkEnd2Desc = new Label("「これは何ですか？」と問うたエンド");
                darkEnd2Desc.style.fontSize = 12;
                darkEnd2Desc.style.marginTop = 4;
                darkEnd2Box.Add(darkEnd2Desc);
            }
            endsContainer.Add(darkEnd2Box);

            card.Add(endsContainer);
            return card;
        }
    }
}

