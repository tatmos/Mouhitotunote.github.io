using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// エンドクレジット画面の表示を管理するクラス
    /// </summary>
    public class CreditsScreenManager : MonoBehaviour
    {
        private Coroutine scrollCoroutine;
        private ScrollView creditsScrollView;
        private float scrollSpeed = 30f; // スクロール速度（ピクセル/秒）
        /// <summary>
        /// クレジット情報を作成
        /// </summary>
        public void CreateCredits(VisualElement container, ScrollView scrollView)
        {
            container.Clear();
            
            // スクロールビューを保存
            creditsScrollView = scrollView;
            
            // 既存のスクロールコルーチンを停止
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
                scrollCoroutine = null;
            }

            // コンテナの上下に余白を追加
            container.style.paddingTop = 100f; // 上部余白
            container.style.paddingBottom = 100f; // 下部余白

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


            // スクロールを開始
            if (scrollView != null)
            {
                // 少し待ってからスクロール位置を設定（レイアウトが確定するまで）
                StartCoroutine(DelayedStartScroll());
            }
        }
        
        /// <summary>
        /// スクロール開始を遅延させる
        /// </summary>
        private IEnumerator DelayedStartScroll()
        {
            yield return new WaitForSeconds(0.5f);
            
            // 初期スクロール位置を最上部（0）に設定
            if (creditsScrollView != null)
            {
                creditsScrollView.verticalScroller.value = 0f;
                yield return null;
            }
            
            StartAutoScroll();
        }
        
        /// <summary>
        /// 自動スクロールを開始
        /// </summary>
        public void StartAutoScroll()
        {
            if (creditsScrollView == null) return;
            
            // 既存のスクロールコルーチンを停止
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
            }
            
            scrollCoroutine = StartCoroutine(AutoScrollCoroutine());
        }
        
        /// <summary>
        /// 自動スクロールを停止
        /// </summary>
        public void StopAutoScroll()
        {
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
                scrollCoroutine = null;
            }
        }
        
        /// <summary>
        /// 自動スクロールのコルーチン（無限ループ）
        /// </summary>
        private IEnumerator AutoScrollCoroutine()
        {
            if (creditsScrollView == null) yield break;
            
            float currentScroll = 0f;
            
            while (true)
            {
                // スクロールビューのコンテンツの高さを取得
                var content = creditsScrollView.contentContainer;
                float contentHeight = content.layout.height;
                float viewportHeight = creditsScrollView.contentViewport.layout.height;
                
                // スクロール可能な距離
                float maxScroll = Mathf.Max(0, contentHeight - viewportHeight);
                
                if (maxScroll > 0)
                {
                    // スクロールを進める
                    currentScroll += scrollSpeed * Time.deltaTime;
                    
                    // 最後までスクロールしたら先頭に戻す
                    if (currentScroll > maxScroll)
                    {
                        currentScroll = 0f;
                    }
                    
                    creditsScrollView.verticalScroller.value = currentScroll;
                }
                else
                {
                    // スクロールできない場合は少し待つ
                    yield return new WaitForSeconds(0.1f);
                }
                
                yield return null;
            }
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

