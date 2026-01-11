using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace NovelGame
{
    /// <summary>
    /// 歌詞データ構造
    /// </summary>
    [Serializable]
    public class LyricItem
    {
        public float startTime;
        public float endTime;
        public string text;
    }

    /// <summary>
    /// 歌詞データ配列
    /// </summary>
    [Serializable]
    public class LyricData
    {
        public List<LyricItem> lyrics;
    }

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
        private Button specialEndButton; // 特別版の「もうひとつ」の世界へボタン
        
        // 歌詞表示関連
        private Label lyricLabel;
        private Coroutine lyricDisplayCoroutine;
        private List<LyricItem> lyricItems = new List<LyricItem>();
        private int currentLyricIndex = -1;

        /// <summary>
        /// クレジット情報を作成
        /// </summary>
        public void CreateCredits(VisualElement container, ScrollView scrollView, bool isSpecial = false, System.Action onComplete = null, VisualElement lyricDisplayContainer = null)
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
            
            // 既存の歌詞表示コルーチンを停止
            if (lyricDisplayCoroutine != null)
            {
                StopCoroutine(lyricDisplayCoroutine);
                lyricDisplayCoroutine = null;
            }
            
            // 歌詞表示用のLabelを作成
            if (lyricDisplayContainer != null)
            {
                CreateLyricDisplayLabel(lyricDisplayContainer);
            }
            
            // 歌詞データを読み込む
            LoadLyricData();

            // コンテナの上下に余白を追加
            container.style.paddingTop = 200f; // 上部余白
            // 下部に歌詞表示用の隙間を追加（歌詞表示エリア + ボタンエリア）
            container.style.paddingBottom = isSpecial ? 600f : 250f; // 特別版は最後にボタンを出すので広めに空ける、通常版は歌詞表示用に250px

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

            // 物語の解明度を表示（通常版・特別版共通）
            if (GameManager.Instance != null)
            {
                int storyProgress = GameManager.Instance.GetStoryProgressPercentage();
                var progressLabel = new Label($"物語の解明度: {storyProgress}%");
                progressLabel.style.fontSize = 32;
                progressLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                progressLabel.style.marginTop = isSpecial ? 30 : 100; // 特別版の場合はThank you for playingの下、通常版は音楽セクションの下
                progressLabel.style.marginBottom = isSpecial ? 100 : 50;
                progressLabel.style.color = new Color(0.2f, 0.6f, 1.0f); // 青色
                progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                container.Add(progressLabel);
            }

            // 特別版：Thank you for playing とボタンを追加
            if (isSpecial)
            {
                var thanksLabel = new Label("Thank you for playing");
                thanksLabel.style.fontSize = 40;
                thanksLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                thanksLabel.style.marginTop = 100;
                thanksLabel.style.marginBottom = 50;
                thanksLabel.style.color = Color.white;
                thanksLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                container.Add(thanksLabel);
                
                // ボタンを最初から追加
                CreateSpecialEndButton(container);
            }

            // スクロールを開始
            if (scrollView != null)
            {
                // 少し待ってからスクロール位置を設定（レイアウトが確定するまで）
                StartCoroutine(DelayedStartScroll());
            }
            
            // 歌詞表示を開始
            if (lyricItems.Count > 0 && lyricLabel != null)
            {
                StartLyricDisplay();
            }
        }
        
        /// <summary>
        /// 歌詞表示用のLabelを作成
        /// </summary>
        public void CreateLyricDisplayLabel(VisualElement container)
        {
            if (lyricLabel != null)
            {
                lyricLabel.RemoveFromHierarchy();
            }
            
            lyricLabel = new Label();
            lyricLabel.name = "LyricDisplayLabel";
            lyricLabel.text = "";
            lyricLabel.AddToClassList("lyric-display");
            lyricLabel.style.position = Position.Absolute;
            // スクロールエリアの下の隙間に表示（ボタンの上、下から100pxの位置）
            lyricLabel.style.bottom = 100f;
            lyricLabel.style.left = Length.Percent(50);
            lyricLabel.style.width = Length.Percent(90);
            lyricLabel.style.maxWidth = 1200f;
            lyricLabel.style.transformOrigin = new TransformOrigin(Length.Percent(50), 0);
            lyricLabel.style.translate = new Translate(Length.Percent(-50), 0);
            lyricLabel.style.fontSize = 32;
            lyricLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            lyricLabel.style.color = Color.white;
            lyricLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            lyricLabel.style.whiteSpace = WhiteSpace.Normal;
            lyricLabel.style.textShadow = new TextShadow
            {
                offset = new Vector2(2, 2),
                blurRadius = 4,
                color = new Color(0, 0, 0, 0.8f)
            };
            lyricLabel.style.opacity = 0f;
            lyricLabel.style.display = DisplayStyle.Flex;
            
            container.Add(lyricLabel);
        }
        
        /// <summary>
        /// 歌詞データを読み込む
        /// </summary>
        private void LoadLyricData()
        {
            lyricItems.Clear();
            currentLyricIndex = -1;
            
            try
            {
                // ResourcesフォルダからJSONを読み込む
                TextAsset jsonAsset = Resources.Load<TextAsset>("Lyric/creditsBGM");
                if (jsonAsset == null)
                {
                    Debug.LogWarning("creditsBGM.jsonが見つかりません。Resources/Lyric/creditsBGMに配置してください。");
                    return;
                }
                
                // JSONをデシリアライズ
                string jsonText = jsonAsset.text.Trim();
                
                // UnityのJsonUtilityは配列を直接読み込めないため、ラッパークラスで読み込む
                // JSONの先頭と末尾を修正してラッパークラスで読み込む
                if (jsonText.StartsWith("["))
                {
                    jsonText = "{\"items\":" + jsonText + "}";
                }
                
                LyricDataWrapper wrapper = JsonUtility.FromJson<LyricDataWrapper>(jsonText);
                if (wrapper != null && wrapper.items != null && wrapper.items.Length > 0)
                {
                    lyricItems.AddRange(wrapper.items);
                    Debug.Log($"歌詞データを読み込みました: {lyricItems.Count}件");
                }
                else
                {
                    Debug.LogWarning("歌詞データの形式が正しくありません。");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"歌詞データの読み込みに失敗しました: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// JSON配列を読み込むためのラッパークラス
        /// </summary>
        [Serializable]
        private class LyricDataWrapper
        {
            public LyricItem[] items;
        }
        
        /// <summary>
        /// 歌詞表示を開始
        /// </summary>
        public void StartLyricDisplay()
        {
            if (lyricDisplayCoroutine != null)
            {
                StopCoroutine(lyricDisplayCoroutine);
            }
            
            currentLyricIndex = -1;
            lyricDisplayCoroutine = StartCoroutine(LyricDisplayCoroutine());
        }
        
        /// <summary>
        /// 歌詞表示のコルーチン（BGMの再生時間に合わせて表示）
        /// </summary>
        private IEnumerator LyricDisplayCoroutine()
        {
            if (lyricLabel == null || lyricItems.Count == 0) yield break;
            
            AudioSource creditBgmAudioSource = GetCreditBgmAudioSource();
            if (creditBgmAudioSource == null)
            {
                Debug.LogWarning("エンドクレジットBGMのAudioSourceが見つかりません。");
                yield break;
            }
            
            // BGMの再生を待つ
            while (!creditBgmAudioSource.isPlaying)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            // BGMの再生時間に合わせて歌詞を表示
            while (creditBgmAudioSource.isPlaying && lyricItems.Count > 0)
            {
                float currentTime = creditBgmAudioSource.time;
                
                // 現在の時間に該当する歌詞を探す
                int targetIndex = -1;
                for (int i = 0; i < lyricItems.Count; i++)
                {
                    if (currentTime >= lyricItems[i].startTime && currentTime <= lyricItems[i].endTime)
                    {
                        targetIndex = i;
                        break;
                    }
                }
                
                // 新しい歌詞が表示されるべき場合
                if (targetIndex != -1 && targetIndex != currentLyricIndex)
                {
                    currentLyricIndex = targetIndex;
                    ShowLyric(lyricItems[targetIndex]);
                }
                // 歌詞が範囲外の場合、非表示にする
                else if (targetIndex == -1 && currentLyricIndex != -1)
                {
                    // 次の歌詞が近い場合は非表示にしない（滑らかな表示のため）
                    bool nextLyricNearby = false;
                    if (currentLyricIndex + 1 < lyricItems.Count)
                    {
                        float timeUntilNext = lyricItems[currentLyricIndex + 1].startTime - currentTime;
                        if (timeUntilNext <= 0.5f)
                        {
                            nextLyricNearby = true;
                        }
                    }
                    
                    if (!nextLyricNearby)
                    {
                        HideLyric();
                        currentLyricIndex = -1;
                    }
                }
                
                yield return new WaitForSeconds(0.1f); // 0.1秒ごとにチェック
            }
            
            // BGMが終了したら歌詞を非表示にする
            HideLyric();
            currentLyricIndex = -1;
        }
        
        /// <summary>
        /// エンドクレジットBGMのAudioSourceを取得
        /// </summary>
        private AudioSource GetCreditBgmAudioSource()
        {
            if (AudioManager.Instance != null)
            {
                // AudioManagerからcreditBgmAudioSourceを取得する方法を確認する必要がある
                // 直接アクセスできない場合、AudioManagerにメソッドを追加する必要がある
                // ここでは、リフレクションを使うか、AudioManagerに公開メソッドを追加する必要がある
                return AudioManager.Instance.GetCreditBgmAudioSource();
            }
            return null;
        }
        
        /// <summary>
        /// 歌詞を表示
        /// </summary>
        private void ShowLyric(LyricItem lyricItem)
        {
            if (lyricLabel == null) return;
            
            lyricLabel.text = lyricItem.text;
            
            // フェードインアニメーション
            StartCoroutine(FadeInLyric());
        }
        
        /// <summary>
        /// 歌詞を非表示
        /// </summary>
        private void HideLyric()
        {
            if (lyricLabel == null) return;
            
            // フェードアウトアニメーション
            StartCoroutine(FadeOutLyric());
        }
        
        /// <summary>
        /// 歌詞のフェードイン
        /// </summary>
        private IEnumerator FadeInLyric()
        {
            if (lyricLabel == null) yield break;
            
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                lyricLabel.style.opacity = alpha;
                yield return null;
            }
            
            lyricLabel.style.opacity = 1f;
        }
        
        /// <summary>
        /// 歌詞のフェードアウト
        /// </summary>
        private IEnumerator FadeOutLyric()
        {
            if (lyricLabel == null) yield break;
            
            float duration = 0.3f;
            float elapsed = 0f;
            float startOpacity = lyricLabel.style.opacity.value;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startOpacity, 0f, elapsed / duration);
                lyricLabel.style.opacity = alpha;
                yield return null;
            }
            
            lyricLabel.style.opacity = 0f;
            lyricLabel.text = "";
        }
        
        /// <summary>
        /// 歌詞表示を停止
        /// </summary>
        public void StopLyricDisplay()
        {
            if (lyricDisplayCoroutine != null)
            {
                StopCoroutine(lyricDisplayCoroutine);
                lyricDisplayCoroutine = null;
            }
            
            // 即座に歌詞表示を非表示にする（フェードアウトアニメーションをスキップ）
            if (lyricLabel != null)
            {
                lyricLabel.style.opacity = 0f;
                lyricLabel.text = "";
            }
            
            currentLyricIndex = -1;
        }
        
        /// <summary>
        /// 歌詞表示を指定した時間でフェードアウトする
        /// </summary>
        public Coroutine FadeOutLyricDisplay(float duration)
        {
            if (lyricDisplayCoroutine != null)
            {
                StopCoroutine(lyricDisplayCoroutine);
                lyricDisplayCoroutine = null;
            }
            
            return StartCoroutine(FadeOutLyricCoroutine(duration));
        }
        
        /// <summary>
        /// 歌詞表示のフェードアウトコルーチン（指定時間版）
        /// </summary>
        private IEnumerator FadeOutLyricCoroutine(float duration)
        {
            if (lyricLabel == null) yield break;
            
            float elapsed = 0f;
            float startOpacity = lyricLabel.style.opacity.value;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startOpacity, 0f, elapsed / duration);
                lyricLabel.style.opacity = alpha;
                yield return null;
            }
            
            lyricLabel.style.opacity = 0f;
            lyricLabel.text = "";
            currentLyricIndex = -1;
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
            
            // 自動スクロール中はスクロールバーを非表示にする
            if (creditsScrollView.verticalScroller != null)
            {
                creditsScrollView.verticalScroller.style.display = DisplayStyle.None;
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
            
            // スクロールバーを再表示する
            if (creditsScrollView != null && creditsScrollView.verticalScroller != null)
            {
                creditsScrollView.verticalScroller.style.display = DisplayStyle.Flex;
            }
            
            // 歌詞表示も停止
            StopLyricDisplay();
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
                    
                    // 特別版の場合、「Thank you for playing」とボタンが見える位置でスクロールを止める
                    if (isSpecialVersion && specialEndButton != null)
                    {
                        // ボタンの位置を計算（レイアウトが確定している場合のみ）
                        Rect buttonLayout = specialEndButton.layout;
                        if (buttonLayout.height > 0) // レイアウトが確定しているかチェック
                        {
                            float buttonY = buttonLayout.y;
                            
                            // 「Thank you for playing」とボタンが見える位置を計算
                            // ビューポートの下部30%の位置にボタンが来るようにする
                            float targetScroll = buttonY - viewportHeight * 0.7f; // ビューポートの70%上から
                            targetScroll = Mathf.Clamp(targetScroll, 0f, maxScroll);
                            
                            // 目標位置に到達したらスクロールを止める
                            if (currentScroll >= targetScroll)
                            {
                                currentScroll = targetScroll;
                                creditsScrollView.verticalScroller.value = currentScroll;
                                
                                if (!specialCompleteTriggered)
                                {
                                    specialCompleteTriggered = true;
                                }
                                
                                // スクロールバーを再表示する
                                if (creditsScrollView.verticalScroller != null)
                                {
                                    creditsScrollView.verticalScroller.style.display = DisplayStyle.Flex;
                                }
                                
                                yield break; // ループを抜けてスクロール停止
                            }
                        }
                    }
                    
                    // 最後までスクロールした時の処理
                    if (currentScroll > maxScroll)
                    {
                        if (isSpecialVersion)
                        {
                            // 特別版：最後まで到達した場合のフォールバック
                            currentScroll = maxScroll;
                            creditsScrollView.verticalScroller.value = currentScroll;
                            
                            if (!specialCompleteTriggered)
                            {
                                specialCompleteTriggered = true;
                            }
                            
                            // スクロールバーを再表示する
                            if (creditsScrollView.verticalScroller != null)
                            {
                                creditsScrollView.verticalScroller.style.display = DisplayStyle.Flex;
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
        /// 特別版：ボタンを作成（最初から追加）
        /// </summary>
        private void CreateSpecialEndButton(VisualElement container)
        {
            if (container == null) return;

            specialEndButton = new Button();
            specialEndButton.text = "「もうひとつ」の世界へ";
            specialEndButton.style.fontSize = 32;
            specialEndButton.style.marginTop = 50;
            specialEndButton.style.paddingTop = 20;
            specialEndButton.style.paddingBottom = 20;
            specialEndButton.style.paddingLeft = 40;
            specialEndButton.style.paddingRight = 40;
            specialEndButton.style.alignSelf = Align.Center;
            specialEndButton.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            specialEndButton.style.color = Color.white;
            specialEndButton.style.borderTopWidth = 1;
            specialEndButton.style.borderBottomWidth = 1;
            specialEndButton.style.borderLeftWidth = 1;
            specialEndButton.style.borderRightWidth = 1;
            specialEndButton.style.borderTopColor = Color.white;
            specialEndButton.style.borderBottomColor = Color.white;
            specialEndButton.style.borderLeftColor = Color.white;
            specialEndButton.style.borderRightColor = Color.white;

            specialEndButton.clicked += () => {
                onSpecialCreditsComplete?.Invoke();
            };

            // マウスオーバー時の音を追加
            specialEndButton.RegisterCallback<MouseEnterEvent>(evt => {
                UIManagerUIToolkit.Instance?.PlayHoverSound();
            });

            container.Add(specialEndButton);
        }
        
        /// <summary>
        /// 特別版：最後にボタンを表示（非推奨、互換性のため残存）
        /// </summary>
        private void ShowSpecialEndButton()
        {
            // ボタンは最初から追加されているので、何もしない
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

