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
        private System.Action onSpecialCreditsComplete;
        private bool isSpecialVersion = false;
        private VisualElement creditsContainer;

        /// <summary>
        /// クレジット情報を作成
        /// </summary>
        public void CreateCredits(VisualElement container, ScrollView scrollView, bool isSpecial = false, System.Action onComplete = null)
        {
            container.Clear();
            
            // スクロールビューを保存
            creditsScrollView = scrollView;
            creditsContainer = container;
            isSpecialVersion = isSpecial;
            onSpecialCreditsComplete = onComplete;
            
            // 既存のスクロールコルーチンを停止
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
                scrollCoroutine = null;
            }

            // コンテナの上下に余白を追加
            container.style.paddingTop = 200f; // 上部余白
            container.style.paddingBottom = isSpecial ? 600f : 200f; // 特別版は最後にボタンを出すので広めに空ける

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

            var musicTitleText = "エンドクレジット楽曲";
            var songInfoText = "曲：「もうひとつ」 / 作曲：suno ai v5 / 作詞：Claude sonnet 4.5";

            // ダークモード演出：失われた文字を置換
            if (GameManager.Instance != null && GameManager.Instance.IsDarkMode())
            {
                var lostLetters = GameManager.Instance.GetLostLetters();
                foreach (char lostLetter in lostLetters)
                {
                    string target = lostLetter.ToString();
                    musicTitleText = musicTitleText.Replace(target, "※");
                    songInfoText = songInfoText.Replace(target, "※");
                }
            }

            var musicTitle = new Label(musicTitleText);
            musicTitle.style.fontSize = 36;
            musicTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            musicTitle.style.marginBottom = 24;
            musicTitle.style.color = new Color(1f, 0.84f, 0f); // yellow-300
            musicSection.Add(musicTitle);

            var songInfo = new Label(songInfoText);
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

            // 特別版：Thank you for playing を追加
            if (isSpecial)
            {
                var thanksLabel = new Label("Thank you for playing");
                thanksLabel.style.fontSize = 40;
                thanksLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                thanksLabel.style.marginTop = 100;
                thanksLabel.style.marginBottom = 100;
                thanksLabel.style.color = Color.white;
                thanksLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                container.Add(thanksLabel);
            }

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
        /// 自動スクロールのコルーチン
        /// </summary>
        private IEnumerator AutoScrollCoroutine()
        {
            if (creditsScrollView == null) yield break;
            
            float currentScroll = 0f;
            bool specialCompleteTriggered = false;
            
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
                    
                    // 最後までスクロールした時の処理
                    if (currentScroll > maxScroll)
                    {
                        if (isSpecialVersion)
                        {
                            // 特別版：最後まで到達したらスクロールを止めてボタンを表示
                            currentScroll = maxScroll;
                            creditsScrollView.verticalScroller.value = currentScroll;
                            
                            if (!specialCompleteTriggered)
                            {
                                specialCompleteTriggered = true;
                                ShowSpecialEndButton();
                            }
                            yield break; // ループを抜けてスクロール停止
                        }
                        else
                        {
                            // 通常版：先頭に戻す（無限ループ）
                            currentScroll = 0f;
                        }
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
        /// 特別版：最後にボタンを表示
        /// </summary>
        private void ShowSpecialEndButton()
        {
            if (creditsContainer == null) return;

            var button = new Button();
            button.text = "「もうひとつ」の世界へ";
            button.style.fontSize = 32;
            button.style.marginTop = 50;
            button.style.paddingTop = 20;
            button.style.paddingBottom = 20;
            button.style.paddingLeft = 40;
            button.style.paddingRight = 40;
            button.style.alignSelf = Align.Center;
            button.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            button.style.color = Color.white;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = Color.white;
            button.style.borderBottomColor = Color.white;
            button.style.borderLeftColor = Color.white;
            button.style.borderRightColor = Color.white;

            button.clicked += () => {
                onSpecialCreditsComplete?.Invoke();
            };

            creditsContainer.Add(button);
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

            string roleText = role;
            string nameText = name;

            // ダークモード演出：失われた文字を置換
            if (GameManager.Instance != null && GameManager.Instance.IsDarkMode())
            {
                var lostLetters = GameManager.Instance.GetLostLetters();
                foreach (char lostLetter in lostLetters)
                {
                    string target = lostLetter.ToString();
                    roleText = roleText.Replace(target, "※");
                    nameText = nameText.Replace(target, "※");
                }
            }

            var roleLabel = new Label(roleText);
            roleLabel.style.fontSize = 24;
            roleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            roleLabel.style.marginBottom = 8;
            roleLabel.style.color = new Color(1f, 0.84f, 0f); // yellow-300
            roleLabel.style.whiteSpace = WhiteSpace.Normal;
            roleLabel.style.maxWidth = Length.Percent(100);
            item.Add(roleLabel);

            var nameLabel = new Label(nameText);
            nameLabel.style.fontSize = 20;
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            nameLabel.style.maxWidth = Length.Percent(100);
            item.Add(nameLabel);

            container.Add(item);
        }
    }
}

