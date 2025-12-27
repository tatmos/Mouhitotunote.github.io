using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// プロフィール画面の表示を管理するクラス
    /// </summary>
    public class ProfileScreenManager
    {
        private GameManager gameManager;
        private HashSet<int> expandedProfiles = new HashSet<int>();
        private int selectedProfileId = 1;

        public ProfileScreenManager(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        /// <summary>
        /// プロフィールカードを作成
        /// </summary>
        public void CreateProfileCards(VisualElement root)
        {
            var profileList = root.Q<VisualElement>("ProfileList");
            var profileDetail = root.Q<VisualElement>("ProfileDetail");

            if (profileList == null || profileDetail == null) return;

            // 既存の要素を削除
            profileList.Clear();
            profileDetail.Clear();

            var scenarios = gameManager.GetScenarios();
            bool isDarkMode = gameManager.IsDarkMode();
            bool scenario6Completed = gameManager.IsScenarioCompleted(6);

            // 利用可能なプロフィールIDのリストを作成
            List<int> availableProfileIds = new List<int>();

            // シナリオ1-5のプロフィール
            for (int i = 1; i <= 5; i++)
            {
                var profile = CharacterProfileManager.GetProfile(i);
                if (profile != null)
                {
                    availableProfileIds.Add(i);
                }
            }

            // シナリオ6のプロフィール（クリア後のみ表示）
            if (scenario6Completed)
            {
                var profile = CharacterProfileManager.GetProfile(6);
                if (profile != null)
                {
                    availableProfileIds.Add(6);
                }
            }

            // 選択中のプロフィールが利用可能でない場合、最初の利用可能なものを選択
            if (!availableProfileIds.Contains(selectedProfileId) && availableProfileIds.Count > 0)
            {
                selectedProfileId = availableProfileIds[0];
            }

            // 左側にプロフィールリストを作成
            foreach (int profileId in availableProfileIds)
            {
                var profile = CharacterProfileManager.GetProfile(profileId);
                if (profile == null) continue;

                var result = gameManager.GetScenarioResult(profileId);
                bool isUnlocked = result != null;

                // リストボタンを作成
                Button listButton = new Button();
                listButton.AddToClassList("profile-list-button");

                // ボタンの中身を構造化
                var buttonContent = new VisualElement();
                buttonContent.style.flexDirection = FlexDirection.Column;
                buttonContent.style.alignItems = Align.FlexStart;

                var nameLabel = new Label(isUnlocked ? profile.name : "???");
                nameLabel.style.fontSize = 16;
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.marginBottom = 4;

                var roleLabel = new Label($"({profile.role})");
                roleLabel.style.fontSize = 12;
                roleLabel.style.opacity = 0.8f;

                buttonContent.Add(nameLabel);
                buttonContent.Add(roleLabel);
                listButton.Add(buttonContent);

                if (!isUnlocked)
                {
                    listButton.AddToClassList("locked");
                }

                if (profileId == selectedProfileId && isUnlocked)
                {
                    listButton.AddToClassList("active");
                }

                int currentProfileId = profileId;
                listButton.clicked += () =>
                {
                    if (isUnlocked)
                    {
                        selectedProfileId = currentProfileId;
                        // コールバックで再生成を通知（UIManagerUIToolkitで処理）
                    }
                };

                profileList.Add(listButton);
            }

            // 右側に選択中のプロフィール詳細を表示
            if (selectedProfileId > 0)
            {
                var selectedProfile = CharacterProfileManager.GetProfile(selectedProfileId);
                if (selectedProfile != null)
                {
                    var result = gameManager.GetScenarioResult(selectedProfileId);
                    bool isUnlocked = result != null;

                    CreateProfileDetail(profileDetail, selectedProfile, result, isUnlocked, isDarkMode, scenario6Completed);
                }
            }
        }

        /// <summary>
        /// プロフィール詳細を作成
        /// </summary>
        private void CreateProfileDetail(VisualElement container, CharacterProfile profile, ScenarioResult result, bool isUnlocked, bool isDarkMode, bool scenario6Completed)
        {
            // プロフィール詳細コンテナを作成
            var detailCard = new VisualElement();

            // キャラクターごとの色分けクラスを追加
            if (isUnlocked)
            {
                switch (profile.scenarioId)
                {
                    case 1:
                        detailCard.AddToClassList("profile-card-momo");
                        break;
                    case 2:
                        detailCard.AddToClassList("profile-card-umi");
                        break;
                    case 3:
                        detailCard.AddToClassList("profile-card-hiro");
                        break;
                    case 4:
                        detailCard.AddToClassList("profile-card-toru");
                        break;
                    case 5:
                        detailCard.AddToClassList("profile-card-tsubasa");
                        break;
                    case 6:
                        detailCard.AddToClassList("profile-card-voice");
                        break;
                }
                detailCard.style.backgroundColor = profile.profileColor;
            }
            else
            {
                detailCard.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            }

            detailCard.style.paddingTop = 20;
            detailCard.style.paddingBottom = 20;
            detailCard.style.paddingLeft = 20;
            detailCard.style.paddingRight = 20;
            detailCard.style.width = Length.Percent(100);
            detailCard.style.maxWidth = Length.Percent(100);
            detailCard.style.minWidth = 0;

            // ボーダー半径を各角に設定
            detailCard.style.borderTopLeftRadius = 8;
            detailCard.style.borderTopRightRadius = 8;
            detailCard.style.borderBottomLeftRadius = 8;
            detailCard.style.borderBottomRightRadius = 8;

            // ボーダー幅を各方向に設定
            var borderColor = isUnlocked ? profile.borderColor : new Color(0.2f, 0.2f, 0.2f);
            detailCard.style.borderTopWidth = 2;
            detailCard.style.borderRightWidth = 2;
            detailCard.style.borderBottomWidth = 2;
            detailCard.style.borderLeftWidth = 2;

            // ボーダー色を各方向に設定
            detailCard.style.borderTopColor = borderColor;
            detailCard.style.borderRightColor = borderColor;
            detailCard.style.borderBottomColor = borderColor;
            detailCard.style.borderLeftColor = borderColor;

            // 名前
            var nameLabel = new Label(isUnlocked ? $"{profile.name}（{profile.role}）" : $"???（{profile.role}）");
            nameLabel.AddToClassList("profile-name");
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            nameLabel.style.maxWidth = Length.Percent(100);
            detailCard.Add(nameLabel);

            if (isUnlocked)
            {
                // 情報
                var infoLabel = new Label();
                string info = $"職業: {(isDarkMode ? "【データ欠損】" : profile.job)}\n";
                info += $"特徴: {(isDarkMode ? profile.featureDarkMode : profile.feature)}";

                if (scenario6Completed && !isDarkMode && !string.IsNullOrEmpty(profile.relationshipWithVoice))
                {
                    info += $"\n\n謎の声との関係: {profile.relationshipWithVoice}";
                }
                else if (scenario6Completed && isDarkMode && !string.IsNullOrEmpty(profile.bugDescription))
                {
                    info += $"\n\n【バグ】: {profile.bugDescription}";
                }

                infoLabel.text = info;
                infoLabel.AddToClassList("profile-info");
                infoLabel.style.whiteSpace = WhiteSpace.Normal;
                infoLabel.style.maxWidth = Length.Percent(100);
                detailCard.Add(infoLabel);

                // セリフ
                if (!string.IsNullOrEmpty(profile.quote) || !string.IsNullOrEmpty(profile.quoteDarkMode))
                {
                    var quoteLabel = new Label(isDarkMode ? profile.quoteDarkMode : profile.quote);
                    quoteLabel.AddToClassList("profile-quote");
                    quoteLabel.style.color = isDarkMode ? Color.red : profile.borderColor;
                    quoteLabel.style.whiteSpace = WhiteSpace.Normal;
                    quoteLabel.style.maxWidth = Length.Percent(100);
                    detailCard.Add(quoteLabel);
                }

                // 後日談
                if (result != null)
                {
                    var epilogueLabel = new Label(isDarkMode ? GetDarkModeEpilogue(profile.scenarioId, result.choiceId) : result.epilogue);
                    epilogueLabel.AddToClassList("profile-epilogue");
                    epilogueLabel.style.whiteSpace = WhiteSpace.Normal;
                    epilogueLabel.style.maxWidth = Length.Percent(100);
                    detailCard.Add(epilogueLabel);

                    // 後日談の後日談
                    if (result.hasWord && profile.scenarioId <= 5)
                    {
                        var scenario = gameManager.GetScenarios().Find(s => s.id == profile.scenarioId);
                        if (scenario != null && scenario.branches.ContainsKey(result.choiceId) &&
                            !string.IsNullOrEmpty(scenario.branches[result.choiceId].epilogue2))
                        {
                            bool isExpanded = expandedProfiles.Contains(profile.scenarioId);

                            var expandButton = new Button();
                            expandButton.text = isExpanded ? "▼ 後日談の後日談を隠す" : "▶ 後日談の後日談を見る";
                            expandButton.clicked += () => ToggleEpilogue2(profile.scenarioId);
                            detailCard.Add(expandButton);

                            if (isExpanded)
                            {
                                var epilogue2Label = new Label(isDarkMode ? GetDarkModeEpilogue2(profile.scenarioId) : scenario.branches[result.choiceId].epilogue2);
                                epilogue2Label.AddToClassList("profile-epilogue2");
                                epilogue2Label.style.whiteSpace = WhiteSpace.Normal;
                                epilogue2Label.style.maxWidth = Length.Percent(100);
                                detailCard.Add(epilogue2Label);
                            }
                        }
                    }

                    // ヒント
                    if (!result.hasWord && profile.scenarioId <= 5)
                    {
                        var scenario = gameManager.GetScenarios().Find(s => s.id == profile.scenarioId);
                        if (scenario != null && scenario.branches.ContainsKey(result.choiceId) &&
                            !string.IsNullOrEmpty(scenario.branches[result.choiceId].hint))
                        {
                            var hintLabel = new Label(scenario.branches[result.choiceId].hint);
                            hintLabel.AddToClassList("profile-hint");
                            hintLabel.style.whiteSpace = WhiteSpace.Normal;
                            hintLabel.style.maxWidth = Length.Percent(100);
                            detailCard.Add(hintLabel);
                        }
                    }
                }
            }
            else
            {
                var lockedLabel = new Label($"シナリオ「{GetScenarioTitle(profile.scenarioId)}」をクリアすると表示されます");
                lockedLabel.AddToClassList("profile-locked");
                detailCard.Add(lockedLabel);
            }

            container.Add(detailCard);
        }

        /// <summary>
        /// 後日談の後日談の表示/非表示を切り替え
        /// </summary>
        public void ToggleEpilogue2(int scenarioId)
        {
            if (expandedProfiles.Contains(scenarioId))
            {
                expandedProfiles.Remove(scenarioId);
            }
            else
            {
                expandedProfiles.Add(scenarioId);
            }
        }

        /// <summary>
        /// 選択中のプロフィールIDを設定
        /// </summary>
        public void SetSelectedProfileId(int profileId)
        {
            selectedProfileId = profileId;
        }

        /// <summary>
        /// 選択中のプロフィールIDを取得
        /// </summary>
        public int GetSelectedProfileId()
        {
            return selectedProfileId;
        }

        private string GetScenarioTitle(int scenarioId)
        {
            var scenarios = gameManager.GetScenarios();
            var scenario = scenarios.Find(s => s.id == scenarioId);
            return scenario != null ? scenario.title : "";
        }

        private string GetDarkModeEpilogue(int scenarioId, int choiceId)
        {
            return "【データ破損】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "のデータは完全に崩壊しました。";
        }

        private string GetDarkModeEpilogue2(int scenarioId)
        {
            return "【完全崩壊】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "は完全にデータの欠片となって消えました。";
        }
    }
}

