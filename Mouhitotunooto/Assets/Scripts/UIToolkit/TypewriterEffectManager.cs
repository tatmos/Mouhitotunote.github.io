using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// タイプライター効果とクリッカブルワード機能を管理するクラス
    /// </summary>
    public class TypewriterEffectManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioClip typewriterSound;
        [SerializeField] private float soundInterval = 0.06f; // 音を鳴らす最小間隔（秒）

        private AudioSource audioSource;
        private float lastSoundTime;
        private Coroutine currentTypewriterEffect;
        private Label clickableWordLabel = null;
        private System.Action<bool> onWordFoundCallback; // ワードが見つかった時のコールバック（bool: 見つかったかどうか）

        private void Awake()
        {
            // AudioSourceの設定
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
        }

        /// <summary>
        /// タイプライター音のクリップを設定
        /// </summary>
        public void SetTypewriterSound(AudioClip clip)
        {
            typewriterSound = clip;
        }

        /// <summary>
        /// 記号や句読点かどうかを判定
        /// </summary>
        private bool IsPunctuationOrSymbol(char c)
        {
            // 句読点、感嘆符、疑問符、および一般的な日本語の記号
            return char.IsPunctuation(c) || char.IsSymbol(c) || 
                   c == 'ー' || 
                   c == 'ぁ' || c == 'っ' || c == 'ゃ' || 
                   c == 'ァ' || c == 'ッ' || c == 'ャ' || 
                   c == '、' || c == '。' || c == '！' || c == '？' || 
                   c == '!' || c == '?' || c == '.' || 
                   c == '：' || c == ':' ||
                   c == '…' || c == '・' || c == '「' || c == '」' || 
                   c == '（' || c == '）' || c == '【' || c == '】';
        }

        /// <summary>
        /// タイプライター音を再生
        /// </summary>
        private void PlayTypewriterSound()
        {
            if (typewriterSound == null || audioSource == null) return;

            // 短い間隔で連続して鳴りすぎないように調整
            if (Time.time - lastSoundTime >= soundInterval)
            {
                audioSource.PlayOneShot(typewriterSound);
                lastSoundTime = Time.time;
            }
        }

        /// <summary>
        /// タイプライター効果を開始（1行ずつ時間差で、左から文字を表示）
        /// </summary>
        /// <param name="label">表示するラベル</param>
        /// <param name="fullText">表示するテキスト</param>
        /// <param name="onComplete">完了時のコールバック</param>
        /// <param name="speedMultiplier">速度の倍率（1.0が通常、2.0で2倍遅く）</param>
        public void StartTypewriterEffect(Label label, string fullText, System.Action onComplete = null, float speedMultiplier = 1.0f)
        {
            // 既存のタイプライター効果を停止
            if (currentTypewriterEffect != null)
            {
                StopCoroutine(currentTypewriterEffect);
            }

            // 初期状態：テキストを空にする
            label.text = "";

            // タイプライター効果開始
            currentTypewriterEffect = StartCoroutine(TypewriterEffectCoroutine(label, fullText, onComplete, speedMultiplier));
        }

        /// <summary>
        /// クリッカブルな「もうひとつ」を含むタイプライター効果を開始
        /// </summary>
        public void StartTypewriterEffectWithClickableWord(VisualElement container, string fullText, System.Action onComplete = null, System.Action<bool> onWordFound = null)
        {
            // 既存のタイプライター効果を停止
            if (currentTypewriterEffect != null)
            {
                StopCoroutine(currentTypewriterEffect);
            }

            // コールバックを設定
            onWordFoundCallback = onWordFound;

            // タイプライター効果開始
            currentTypewriterEffect = StartCoroutine(TypewriterEffectWithClickableWordCoroutine(container, fullText, onComplete));
        }

        /// <summary>
        /// クリッカブルな「もうひとつ」を含むタイプライター効果コルーチン
        /// </summary>
        private IEnumerator TypewriterEffectWithClickableWordCoroutine(VisualElement container, string fullText, System.Action onComplete = null)
        {
            // テキストを行ごとに分割
            string[] lines = fullText.Split('\n');
            
            float charDelay = 0.03f; // 1文字あたりの遅延（秒）
            float lineDelay = 0.15f; // 行間の遅延（秒）

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                
                // 行を解析して「【もうひとつ】」または「もうひとつ」の位置を検出
                int wordStartIndex = line.IndexOf("【もうひとつ】");
                int wordLength = 0;
                string clickableText = "";
                
                if (wordStartIndex >= 0)
                {
                    // 「【もうひとつ】」が見つかった場合
                    wordLength = "【もうひとつ】".Length;
                    clickableText = "もうひとつ";
                }
                else
                {
                    // 「もうひとつ」を検索（【】なし）
                    // ただし、「もうひとつ」が他の単語の一部でないことを確認
                    wordStartIndex = line.IndexOf("もうひとつ");
                    if (wordStartIndex >= 0)
                    {
                        // 前後の文字を確認して、単語の境界であることを確認
                        bool isValidWord = true;
                        if (wordStartIndex > 0)
                        {
                            char beforeChar = line[wordStartIndex - 1];
                            // ひらがな、カタカナ、漢字、英数字の場合は単語の一部の可能性がある
                            if (char.IsLetterOrDigit(beforeChar) || beforeChar == '【' || beforeChar == '「' || beforeChar == '『')
                            {
                                isValidWord = false;
                            }
                        }
                        if (wordStartIndex + "もうひとつ".Length < line.Length)
                        {
                            char afterChar = line[wordStartIndex + "もうひとつ".Length];
                            // ひらがな、カタカナ、漢字、英数字の場合は単語の一部の可能性がある
                            if (char.IsLetterOrDigit(afterChar) || afterChar == '】' || afterChar == '」' || afterChar == '』')
                            {
                                isValidWord = false;
                            }
                        }
                        
                        if (isValidWord)
                        {
                            wordLength = "もうひとつ".Length;
                            clickableText = "もうひとつ";
                        }
                    }
                }
                
                if (wordStartIndex >= 0 && wordLength > 0)
                {
                    // 「もうひとつ」または「【もうひとつ】」が見つかった場合
                    // 前の部分を通常のLabelとして表示
                    if (wordStartIndex > 0)
                    {
                        string beforeWord = line.Substring(0, wordStartIndex);
                        Label beforeLabel = new Label();
                        beforeLabel.style.fontSize = 20;
                        beforeLabel.style.whiteSpace = WhiteSpace.Normal;
                        container.Add(beforeLabel);
                        
                        for (int i = 0; i < beforeWord.Length; i++)
                        {
                            char c = beforeWord[i];
                            beforeLabel.text = beforeWord.Substring(0, i + 1);
                            
                            // 空白文字および記号・句読点以外の場合に音を鳴らす
                            if (!char.IsWhiteSpace(c) && !IsPunctuationOrSymbol(c))
                            {
                                PlayTypewriterSound();
                            }

                            // 記号や句読点の場合は待機時間を長くする
                            float delay = IsPunctuationOrSymbol(c) ? charDelay * 2.0f : charDelay;
                            yield return new WaitForSeconds(delay);
                        }
                    }
                    
                    // 「もうひとつ」をクリッカブルなLabelとして表示
                    Label clickableLabel = new Label("");
                    clickableLabel.style.fontSize = 20;
                    clickableLabel.style.whiteSpace = WhiteSpace.Normal;
                    clickableLabel.style.color = new StyleColor(new Color(0.2f, 0.6f, 1.0f)); // 青色
                    clickableLabel.AddToClassList("clickable-word");
                    clickableLabel.RegisterCallback<ClickEvent>(OnWordClicked);
                    clickableWordLabel = clickableLabel;
                    container.Add(clickableLabel);
                    
                    // クリッカブルワードを1文字ずつ表示（強調のために遅延を長くする）
                    float emphasizedCharDelay = charDelay * 10.0f; // 通常の10倍の遅延
                    for (int i = 0; i < clickableText.Length; i++)
                    {
                        char c = clickableText[i];
                        clickableLabel.text = clickableText.Substring(0, i + 1);
                        
                        // 空白文字および記号・句読点以外の場合に音を鳴らす
                        if (!char.IsWhiteSpace(c) && !IsPunctuationOrSymbol(c))
                        {
                            PlayTypewriterSound();
                        }
                        
                        yield return new WaitForSeconds(emphasizedCharDelay);
                    }
                    
                    // 後の部分を通常のLabelとして表示
                    int wordEndIndex = wordStartIndex + wordLength;
                    if (wordEndIndex < line.Length)
                    {
                        string afterWord = line.Substring(wordEndIndex);
                        Label afterLabel = new Label();
                        afterLabel.style.fontSize = 20;
                        afterLabel.style.whiteSpace = WhiteSpace.Normal;
                        container.Add(afterLabel);
                        
                        for (int i = 0; i < afterWord.Length; i++)
                        {
                            char c = afterWord[i];
                            afterLabel.text = afterWord.Substring(0, i + 1);
                            
                            // 空白文字および記号・句読点以外の場合に音を鳴らす
                            if (!char.IsWhiteSpace(c) && !IsPunctuationOrSymbol(c))
                            {
                                PlayTypewriterSound();
                            }

                            // 記号や句読点の場合は待機時間を長くする
                            float delay = IsPunctuationOrSymbol(c) ? charDelay * 2.0f : charDelay;
                            yield return new WaitForSeconds(delay);
                        }
                    }
                }
                else
                {
                    // 「もうひとつ」が見つからない場合、通常のタイプライター効果
                    Label textLabel = new Label();
                    textLabel.style.fontSize = 20;
                    textLabel.style.whiteSpace = WhiteSpace.Normal;
                    container.Add(textLabel);
                    
                    for (int charIndex = 0; charIndex < line.Length; charIndex++)
                    {
                        char c = line[charIndex];
                        textLabel.text = line.Substring(0, charIndex + 1);
                        
                        // 空白文字および記号・句読点以外の場合に音を鳴らす
                        if (!char.IsWhiteSpace(c) && !IsPunctuationOrSymbol(c))
                        {
                            PlayTypewriterSound();
                        }
                        
                        // 記号や句読点の場合は待機時間を長くする
                        float delay = IsPunctuationOrSymbol(c) ? charDelay * 2.0f : charDelay;
                        yield return new WaitForSeconds(delay);
                    }
                }
                
                // 最後の行以外は改行を追加
                if (lineIndex < lines.Length - 1)
                {
                    Label lineBreak = new Label("\n");
                    lineBreak.style.fontSize = 20;
                    container.Add(lineBreak);
                    
                    // 行間の遅延
                    yield return new WaitForSeconds(lineDelay);
                }
            }
            
            // 完了コールバックを呼び出し
            onComplete?.Invoke();
            
            currentTypewriterEffect = null;
        }

        /// <summary>
        /// 「もうひとつ」がクリックされた時の処理
        /// </summary>
        private void OnWordClicked(ClickEvent evt)
        {
            if (clickableWordLabel == null) return;
            
            // 色を変更（緑色）
            clickableWordLabel.style.color = new StyleColor(new Color(0.2f, 0.8f, 0.4f));
            clickableWordLabel.RemoveFromClassList("clickable-word");
            
            // クリックイベントを削除
            clickableWordLabel.UnregisterCallback<ClickEvent>(OnWordClicked);
            
            // コールバックを呼び出し
            onWordFoundCallback?.Invoke(true);
            
            clickableWordLabel = null;
        }

        /// <summary>
        /// タイプライター効果コルーチン
        /// </summary>
        private IEnumerator TypewriterEffectCoroutine(Label label, string fullText, System.Action onComplete = null, float speedMultiplier = 1.0f)
        {
            // テキストを行ごとに分割
            string[] lines = fullText.Split('\n');
            
            float charDelay = 0.03f * speedMultiplier; // 1文字あたりの遅延（秒）
            float lineDelay = 0.15f * speedMultiplier; // 行間の遅延（秒）

            string displayedText = "";

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                
                // 各行を1文字ずつ表示
                for (int charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    char c = line[charIndex];
                    // 現在の行までの完全に表示されたテキスト + 現在の行の部分的なテキスト
                    string currentText = displayedText + line.Substring(0, charIndex + 1);
                    
                    label.text = currentText;

                    // 空白文字および記号・句読点以外の場合に音を鳴らす
                    if (!char.IsWhiteSpace(c) && !IsPunctuationOrSymbol(c))
                    {
                        PlayTypewriterSound();
                    }

                    // 記号や句読点の場合は待機時間を長くする
                    float delay = IsPunctuationOrSymbol(c) ? charDelay * 2.0f : charDelay;
                    yield return new WaitForSeconds(delay);
                }

                // 行を完全に表示したら、displayedTextに追加
                displayedText += line;
                
                // 最後の行以外は改行を追加
                if (lineIndex < lines.Length - 1)
                {
                    displayedText += "\n";
                    label.text = displayedText; // 改行も表示
                    
                    // 行間の遅延
                    yield return new WaitForSeconds(lineDelay);
                }
            }

            // 最終的なテキストを設定（念のため）
            label.text = fullText;
            
            // 完了コールバックを呼び出し
            onComplete?.Invoke();
            
            currentTypewriterEffect = null;
        }

        /// <summary>
        /// タイプライター効果を停止
        /// </summary>
        public void StopTypewriterEffect()
        {
            if (currentTypewriterEffect != null)
            {
                StopCoroutine(currentTypewriterEffect);
                currentTypewriterEffect = null;
            }
        }
    }
}

