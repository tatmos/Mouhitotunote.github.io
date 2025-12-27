using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// エンドクレジット画面の表示を管理するクラス
    /// </summary>
    public class CreditsScreenManager
    {
        /// <summary>
        /// クレジット情報を作成
        /// </summary>
        public void CreateCredits(VisualElement container)
        {
            container.Clear();

            // クレジット情報を追加
            AddCreditItem(container, "ゲームデザイン", "tatmos");
            AddCreditItem(container, "AIディレクション", "tatmos");
            AddCreditItem(container, "シナリオ", "Claude sonnet 4.5");
            AddCreditItem(container, "リードプログラマ", "Claude sonnet 4.5");
            AddCreditItem(container, "プログラマ", "tatmos");
            AddCreditItem(container, "音楽", "tatmos");
            AddCreditItem(container, "効果音", "tatmos");
            AddCreditItem(container, "グラフィック", "Chat GPT 5.2");

            // エンドクレジット楽曲セクション
            var musicSection = new VisualElement();
            musicSection.style.marginTop = 48;
            musicSection.style.paddingTop = 32;
            musicSection.style.borderTopWidth = 1;
            musicSection.style.borderTopColor = new Color(1f, 1f, 1f, 0.3f);
            musicSection.style.width = Length.Percent(100);
            musicSection.style.flexDirection = FlexDirection.Column;
            musicSection.style.alignItems = Align.Center;

            var musicTitle = new Label("エンドクレジット楽曲");
            musicTitle.style.fontSize = 36;
            musicTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            musicTitle.style.marginBottom = 24;
            musicTitle.style.color = new Color(1f, 0.84f, 0f); // yellow-300
            musicSection.Add(musicTitle);

            var songInfo = new Label("曲：「もうひとつ」 / 作曲：suno ai v5 / 作詞：Claude sonnet 4.5");
            songInfo.style.fontSize = 24;
            songInfo.style.unityFontStyleAndWeight = FontStyle.Bold;
            songInfo.style.marginBottom = 16;
            songInfo.style.whiteSpace = WhiteSpace.Normal;
            songInfo.style.maxWidth = Length.Percent(100);
            musicSection.Add(songInfo);

            AddCreditItem(musicSection, "歌", "suno ai v5");
            AddCreditItem(musicSection, "演奏", "suno ai v5");
            AddCreditItem(musicSection, "ミキシング", "suno ai v5");
            AddCreditItem(musicSection, "マスタリング", "suno ai v5");
            AddCreditItem(musicSection, "サウンドエンジニア", "tatmos");

            container.Add(musicSection);
        }

        /// <summary>
        /// クレジット項目を追加
        /// </summary>
        private void AddCreditItem(VisualElement container, string role, string name)
        {
            var item = new VisualElement();
            item.AddToClassList("credits-content-item");
            item.style.flexDirection = FlexDirection.Column;
            item.style.alignItems = Align.Center;
            item.style.marginBottom = 16;
            item.style.width = Length.Percent(100);

            var roleLabel = new Label(role);
            roleLabel.style.fontSize = 24;
            roleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            roleLabel.style.marginBottom = 8;
            roleLabel.style.color = new Color(1f, 0.84f, 0f); // yellow-300
            roleLabel.style.whiteSpace = WhiteSpace.Normal;
            roleLabel.style.maxWidth = Length.Percent(100);
            item.Add(roleLabel);

            var nameLabel = new Label(name);
            nameLabel.style.fontSize = 20;
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            nameLabel.style.maxWidth = Length.Percent(100);
            item.Add(nameLabel);

            container.Add(item);
        }
    }
}

