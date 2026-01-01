using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private AudioClip lostLetterSound;
        [SerializeField] private float soundInterval = 0.06f; // 音を鳴らす最小間隔（秒）

        private AudioSource audioSource;
        private float lastSoundTime;
        private Coroutine currentTypewriterEffect;
        private Dictionary<Label, Coroutine> activeLabelEffects = new Dictionary<Label, Coroutine>();
        private Label clickableWordLabel = null;
        private System.Action<bool, Vector2> onWordFoundCallback; // ワードが見つかった時のコールバック（bool: 見つかったかどうか, Vector2: クリック位置）

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
        /// 消失文字（※）用の音のクリップを設定
        /// </summary>
        public void SetLostLetterSound(AudioClip clip)
        {
            lostLetterSound = clip;
        }

        /// <summary>
        /// リッチテキストタグを考慮しながら1文字ずつ表示する（Label用、表示テキストを累積）
        /// </summary>
        /// <param name="label">表示するラベル</param>
        /// <param name="formattedText">フォーマット済みテキスト（リッチテキストタグを含む）</param>
        /// <param name="originalText">元のテキスト（音を鳴らすために使用）</param>
        /// <param name="lostLetters">失われた文字のセット</param>
        /// <param name="baseCharDelay">基本の文字遅延時間</param>
        /// <returns>累積された表示テキスト</returns>
        private IEnumerator DisplayRichTextCharacterByCharacterForLabel(Label label, string formattedText, string originalText, HashSet<char> lostLetters, float baseCharDelay, System.Action<string> onTextUpdated = null)
        {
            string displayedText = label.text; // 既存のテキストから開始
            int originalIndex = 0; // 元のテキストのインデックス
            
            for (int i = 0; i < formattedText.Length; i++)
            {
                char c = formattedText[i];
                
                // リッチテキストタグの開始（<color=#...>）
                if (c == '<' && i + 1 < formattedText.Length && formattedText[i + 1] != '/')
                {
                    // タグ全体を取得
                    int tagEnd = formattedText.IndexOf('>', i);
                    if (tagEnd > 0)
                    {
                        string tag = formattedText.Substring(i, tagEnd - i + 1);
                        displayedText += tag;
                        i = tagEnd;
                        label.text = displayedText;
                        onTextUpdated?.Invoke(displayedText);
                        continue;
                    }
                }
                // リッチテキストタグの終了（</color>）
                else if (c == '<' && i + 1 < formattedText.Length && formattedText[i + 1] == '/')
                {
                    // タグ全体を取得
                    int tagEnd = formattedText.IndexOf('>', i);
                    if (tagEnd > 0)
                    {
                        string tag = formattedText.Substring(i, tagEnd - i + 1);
                        displayedText += tag;
                        i = tagEnd;
                        label.text = displayedText;
                        onTextUpdated?.Invoke(displayedText);
                        continue;
                    }
                }
                else
                {
                    // 通常の文字
                    displayedText += c;
                    
                    // 元のテキストから対応する文字を取得（音を鳴らすために使用）
                    if (originalIndex < originalText.Length)
                    {
                        char originalChar = originalText[originalIndex];
                        char charToDisplay = lostLetters != null && lostLetters.Contains(originalChar) ? '※' : originalChar;
                        
                        // 空白文字および記号・句読点以外の場合に音を鳴らす
                        if (!char.IsWhiteSpace(originalChar) && !IsPunctuationOrSymbol(originalChar))
                        {
                            PlayTypewriterSound(charToDisplay);
                        }
                        
                        // 記号や句読点の場合は待機時間を長くする
                        float delay = IsPunctuationOrSymbol(originalChar) ? baseCharDelay * 2.0f : baseCharDelay;
                        yield return new WaitForSeconds(delay);
                        
                        originalIndex++;
                    }
                    else
                    {
                        // 元のテキストの範囲外の場合は通常の遅延
                        yield return new WaitForSeconds(baseCharDelay);
                    }
                }
                
                label.text = displayedText;
                onTextUpdated?.Invoke(displayedText);
            }
        }

        /// <summary>
        /// リッチテキストタグを考慮しながら1文字ずつ表示する
        /// </summary>
        /// <param name="label">表示するラベル</param>
        /// <param name="formattedText">フォーマット済みテキスト（リッチテキストタグを含む）</param>
        /// <param name="originalText">元のテキスト（音を鳴らすために使用）</param>
        /// <param name="lostLetters">失われた文字のセット</param>
        /// <param name="baseCharDelay">基本の文字遅延時間</param>
        private IEnumerator DisplayRichTextCharacterByCharacter(Label label, string formattedText, string originalText, HashSet<char> lostLetters, float baseCharDelay)
        {
            string currentDisplayText = "";
            int originalIndex = 0; // 元のテキストのインデックス
            
            for (int i = 0; i < formattedText.Length; i++)
            {
                char c = formattedText[i];
                
                // リッチテキストタグの開始（<color=#...>）
                if (c == '<' && i + 1 < formattedText.Length && formattedText[i + 1] != '/')
                {
                    // タグ全体を取得
                    int tagEnd = formattedText.IndexOf('>', i);
                    if (tagEnd > 0)
                    {
                        string tag = formattedText.Substring(i, tagEnd - i + 1);
                        currentDisplayText += tag;
                        i = tagEnd;
                        continue;
                    }
                }
                // リッチテキストタグの終了（</color>）
                else if (c == '<' && i + 1 < formattedText.Length && formattedText[i + 1] == '/')
                {
                    // タグ全体を取得
                    int tagEnd = formattedText.IndexOf('>', i);
                    if (tagEnd > 0)
                    {
                        string tag = formattedText.Substring(i, tagEnd - i + 1);
                        currentDisplayText += tag;
                        i = tagEnd;
                        continue;
                    }
                }
                else
                {
                    // 通常の文字
                    currentDisplayText += c;
                    
                    // 元のテキストから対応する文字を取得（音を鳴らすために使用）
                    if (originalIndex < originalText.Length)
                    {
                        char originalChar = originalText[originalIndex];
                        char charToDisplay = lostLetters != null && lostLetters.Contains(originalChar) ? '※' : originalChar;
                        
                        // 空白文字および記号・句読点以外の場合に音を鳴らす
                        if (!char.IsWhiteSpace(originalChar) && !IsPunctuationOrSymbol(originalChar))
                        {
                            PlayTypewriterSound(charToDisplay);
                        }
                        
                        // 記号や句読点の場合は待機時間を長くする
                        float delay = IsPunctuationOrSymbol(originalChar) ? baseCharDelay * 2.0f : baseCharDelay;
                        yield return new WaitForSeconds(delay);
                        
                        originalIndex++;
                    }
                    else
                    {
                        // 元のテキストの範囲外の場合は通常の遅延
                        yield return new WaitForSeconds(baseCharDelay);
                    }
                }
                
                label.text = currentDisplayText;
            }
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
        /// <param name="displayedChar">実際に表示された文字</param>
        private void PlayTypewriterSound(char displayedChar)
        {
            AudioClip clipToPlay = (displayedChar == '※') ? lostLetterSound : typewriterSound;
            if (clipToPlay == null) return;

            // 短い間隔で連続して鳴りすぎないように調整
            if (Time.time - lastSoundTime >= soundInterval)
            {
                // AudioManager経由で再生（ピッチ変動対応）
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySEWithPitchVariation(clipToPlay);
                }
                else if (audioSource != null)
                {
                    // AudioManagerが利用できない場合は通常通り再生
                    audioSource.PlayOneShot(clipToPlay);
                }
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
            // 既存のラベル単位のタイプライター効果を停止
            if (activeLabelEffects.TryGetValue(label, out Coroutine existingCoroutine))
            {
                if (existingCoroutine != null)
                {
                    StopCoroutine(existingCoroutine);
                }
                activeLabelEffects.Remove(label);
            }

            // 初期状態：テキストを空にする
            label.text = "";

            // タイプライター効果開始
            Coroutine newCoroutine = StartCoroutine(TypewriterEffectCoroutine(label, fullText, onComplete, speedMultiplier));
            activeLabelEffects[label] = newCoroutine;
        }

        private VisualElement currentContainer = null;
        private string currentFullText = "";

        /// <summary>
        /// クリッカブルな「もうひとつ」を含むタイプライター効果を開始
        /// </summary>
        /// <param name="container">表示するコンテナ</param>
        /// <param name="fullText">フォーマット後のテキスト（表示用）</param>
        /// <param name="onComplete">完了時のコールバック</param>
        /// <param name="onWordFound">ワードが見つかった時のコールバック</param>
        /// <param name="fontSize">フォントサイズ</param>
        /// <param name="isClickable">クリッカブルかどうか</param>
        /// <param name="originalText">フォーマット前の元のテキスト（パターンマッチング用、nullの場合はfullTextを使用）</param>
        public void StartTypewriterEffectWithClickableWord(VisualElement container, string fullText, System.Action onComplete = null, System.Action<bool, Vector2> onWordFound = null, int fontSize = 20, bool isClickable = true, string originalText = null)
        {
            // 既存のタイプライター効果を停止
            if (currentTypewriterEffect != null)
            {
                StopCoroutine(currentTypewriterEffect);
            }

            // 現在の状態を保存
            currentContainer = container;
            currentFullText = fullText;
            onWordFoundCallback = onWordFound;

            // 元のテキストが指定されていない場合は、fullTextを使用（後方互換性のため）
            string textForPatternMatching = originalText ?? fullText;

            // タイプライター効果開始
            currentTypewriterEffect = StartCoroutine(TypewriterEffectWithClickableWordCoroutine(container, fullText, textForPatternMatching, onComplete, fontSize, isClickable));
        }

        /// <summary>
        /// クリッカブルな「もうひとつ」を含むタイプライター効果コルーチン
        /// </summary>
        /// <param name="container">表示するコンテナ</param>
        /// <param name="fullText">フォーマット後のテキスト（表示用）</param>
        /// <param name="originalTextForPatternMatching">フォーマット前の元のテキスト（パターンマッチング用）</param>
        /// <param name="onComplete">完了時のコールバック</param>
        /// <param name="fontSize">フォントサイズ</param>
        /// <param name="isClickable">クリッカブルかどうか</param>
        private IEnumerator TypewriterEffectWithClickableWordCoroutine(VisualElement container, string fullText, string originalTextForPatternMatching, System.Action onComplete = null, int fontSize = 20, bool isClickable = true)
        {
            // ダークモードで失われた文字を取得
            var lostLetters = GameManager.Instance != null ? GameManager.Instance.GetLostLetters() : new HashSet<char>();
            // 取得済みの文字を取得
            var collectedLetters = GameManager.Instance != null ? GameManager.Instance.GetCollectedLetters() : new HashSet<char>();
            
            // 表示用テキストを行ごとに分割
            string[] lines = fullText.Split('\n');
            // パターンマッチング用の元のテキストを行ごとに分割
            string[] originalLines = originalTextForPatternMatching.Split('\n');
            
            float charDelay = 0.03f; // 1文字あたりの遅延（秒）
            float lineDelay = 0.15f; // 行間の遅延（秒）

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex]; // 表示用テキスト（フォーマット後）
                string originalLine = lineIndex < originalLines.Length ? originalLines[lineIndex] : line; // パターンマッチング用テキスト（フォーマット前）
                
                // パターンマッチングは元のテキスト（フォーマット前）に対して行う
                // これにより、3周目で「もうひとつ」が「※※※※※」に置き換えられていても、正しく検出できる
                string[] patterns = { "【もうひとつ】", "もうひとつ", "もう、ひとつ", "もう,ひとつ", "『もうひとつ』", "\"もうひとつ\"", "「もうひとつ」" };

                int wordStartIndex = -1;
                int wordLength = 0;
                string clickableText = "";
                string matchedPattern = "";

                // 元のテキスト（フォーマット前）でパターンマッチングを行う
                foreach (var pattern in patterns)
                {
                    wordStartIndex = originalLine.IndexOf(pattern);
                    if (wordStartIndex >= 0)
                    {
                        matchedPattern = pattern;
                        wordLength = pattern.Length;
                        // クリッカブルテキストを抽出（装飾文字を除去）
                        // 「もう、ひとつ」の場合は、カンマを含む「もう、ひとつ」をクリッカブルテキストとして使用
                        if (pattern.Contains("もう、ひとつ") || pattern.Contains("もう,ひとつ"))
                        {
                            clickableText = "もう、ひとつ";
                        }
                        else
                        {
                            // 装飾文字（【】、『』、「」、""）を除去
                            clickableText = pattern
                                .Replace("【", "").Replace("】", "")
                                .Replace("『", "").Replace("』", "")
                                .Replace("「", "").Replace("」", "")
                                .Replace("\"", "");
                        }
                        Debug.Log($"[TypewriterEffectManager] Pattern matched: '{pattern}' at index {wordStartIndex}, originalLine: '{originalLine}', clickableText: '{clickableText}'");
                        break;
                    }
                }
                
                // パターンマッチングが失敗した場合のデバッグログ（ランダム要素が含まれるテキストの確認用）
                if (wordStartIndex < 0 && originalLine.Contains("もう") && originalLine.Contains("ひとつ"))
                {
                    Debug.LogWarning($"[TypewriterEffectManager] 「もうひとつ」が含まれているが、パターンマッチングに失敗しました。originalLine: '{originalLine}'");
                }
                
                if (wordStartIndex >= 0 && wordLength > 0)
                {
                    // 「もうひとつ」または「【もうひとつ】」が見つかった場合
                    // 前の部分を通常のLabelとして表示
                    if (wordStartIndex > 0)
                    {
                        string beforeWord = line.Substring(0, wordStartIndex);
                        Label beforeLabel = new Label();
                        beforeLabel.style.fontSize = fontSize;
                        beforeLabel.style.whiteSpace = WhiteSpace.Normal;
                        beforeLabel.style.alignSelf = Align.FlexStart; // 左揃え
                        beforeLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                        // 明るい色を適用
                        Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                        beforeLabel.style.color = brightTextColor;
                        beforeLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        container.Add(beforeLabel);
                        
                        // テキストを事前にフォーマット（色付けと伏字化）
                        string formattedBeforeWord = TextFormatter.FormatText(beforeWord, collectedLetters, lostLetters, true);
                        
                        // リッチテキストタグを考慮しながら1文字ずつ表示
                        yield return StartCoroutine(DisplayRichTextCharacterByCharacter(beforeLabel, formattedBeforeWord, beforeWord, lostLetters, charDelay));
                    }
                    
                    // 「もうひとつ」を強調表示（必要に応じてクリッカブル）するLabelとして表示
                    Label clickableLabel = new Label("");
                    clickableLabel.style.fontSize = fontSize;
                    clickableLabel.style.whiteSpace = WhiteSpace.Normal;
                    clickableLabel.style.alignSelf = Align.FlexStart; // 左揃え
                    clickableLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                    
                    if (isClickable)
                    {
                        clickableLabel.style.color = new StyleColor(new Color(0.2f, 0.6f, 1.0f)); // 青色
                        clickableLabel.AddToClassList("clickable-word");
                        clickableLabel.RegisterCallback<ClickEvent>(OnWordClicked);
                        clickableWordLabel = clickableLabel;
                    }
                    else
                    {
                        // クリッカブルでない場合も、強調のために少し色を変える（任意）
                        // ここではシナリオに合わせるため青色にするが、クリックイベントは登録しない
                        clickableLabel.style.color = new StyleColor(new Color(0.2f, 0.6f, 1.0f)); // 青色
                    }
                    
                    container.Add(clickableLabel);
                    
                    // 強調ワードを1文字ずつ表示（強調のために遅延を長くする）
                    float emphasizedCharDelay = charDelay * 10.0f; // 通常の10倍の遅延
                    // テキストを事前にフォーマット（色付けと伏字化）
                    string formattedClickableText = TextFormatter.FormatText(clickableText, collectedLetters, lostLetters, true);
                    
                    // リッチテキストタグを考慮しながら1文字ずつ表示
                    yield return StartCoroutine(DisplayRichTextCharacterByCharacter(clickableLabel, formattedClickableText, clickableText, lostLetters, emphasizedCharDelay));
                    
                    // 後の部分を通常のLabelとして表示
                    int wordEndIndex = wordStartIndex + wordLength;
                    if (wordEndIndex < line.Length)
                    {
                        string afterWord = line.Substring(wordEndIndex);
                        Label afterLabel = new Label();
                        afterLabel.style.fontSize = fontSize;
                        afterLabel.style.whiteSpace = WhiteSpace.Normal;
                        afterLabel.style.alignSelf = Align.FlexStart; // 左揃え
                        afterLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                        // 明るい色を適用
                        Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                        afterLabel.style.color = brightTextColor;
                        afterLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        container.Add(afterLabel);
                        
                        // テキストを事前にフォーマット（色付けと伏字化）
                        string formattedAfterWord = TextFormatter.FormatText(afterWord, collectedLetters, lostLetters, true);
                        
                        // リッチテキストタグを考慮しながら1文字ずつ表示
                        yield return StartCoroutine(DisplayRichTextCharacterByCharacter(afterLabel, formattedAfterWord, afterWord, lostLetters, charDelay));
                    }
                }
                else
                {
                    // 「もうひとつ」が見つからない場合、通常のタイプライター効果
                    Label textLabel = new Label();
                    textLabel.style.fontSize = fontSize;
                    textLabel.style.whiteSpace = WhiteSpace.Normal;
                    textLabel.style.alignSelf = Align.FlexStart; // 左揃え
                    textLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                    // 明るい色を適用
                    Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                    textLabel.style.color = brightTextColor;
                    textLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                    container.Add(textLabel);
                    
                    // テキストを事前にフォーマット（色付けと伏字化）
                    string formattedLine = TextFormatter.FormatText(line, collectedLetters, lostLetters, true);
                    
                    // リッチテキストタグを考慮しながら1文字ずつ表示
                    yield return StartCoroutine(DisplayRichTextCharacterByCharacter(textLabel, formattedLine, line, lostLetters, charDelay));
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
            
            // コールバックを呼び出し（クリック位置を渡す）
            onWordFoundCallback?.Invoke(true, evt.position);
            
            // テキストを即座に全表示し、コルーチンを停止する
            SkipTypewriterWithClickableWord();
            
            clickableWordLabel = null;
        }

        /// <summary>
        /// タイプライター効果コルーチン
        /// </summary>
        private IEnumerator TypewriterEffectCoroutine(Label label, string fullText, System.Action onComplete = null, float speedMultiplier = 1.0f)
        {
            // ダークモードで失われた文字を取得
            var lostLetters = GameManager.Instance != null ? GameManager.Instance.GetLostLetters() : new HashSet<char>();
            // 取得済みの文字を取得
            var collectedLetters = GameManager.Instance != null ? GameManager.Instance.GetCollectedLetters() : new HashSet<char>();
            
            // テキストを行ごとに分割
            string[] lines = fullText.Split('\n');
            
            float charDelay = 0.03f * speedMultiplier; // 1文字あたりの遅延（秒）
            float lineDelay = 0.15f * speedMultiplier; // 行間の遅延（秒）

            string displayedText = "";

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                
                // テキストを事前にフォーマット（色付けと伏字化）
                string formattedLine = TextFormatter.FormatText(line, collectedLetters, lostLetters, true);
                
                // リッチテキストタグを考慮しながら1文字ずつ表示
                yield return StartCoroutine(DisplayRichTextCharacterByCharacterForLabel(label, formattedLine, line, lostLetters, charDelay, (updatedText) => {
                    displayedText = updatedText;
                }));
                
                // 最後の行以外は改行を追加
                if (lineIndex < lines.Length - 1)
                {
                    displayedText = label.text; // 現在のラベルのテキストを取得
                    displayedText += "\n";
                    label.text = displayedText; // 改行も表示
                    
                    // 行間の遅延
                    yield return new WaitForSeconds(lineDelay);
                }
            }

            // 完了コールバックを呼び出し
            onComplete?.Invoke();
            
            if (activeLabelEffects.ContainsKey(label))
            {
                activeLabelEffects.Remove(label);
            }
        }

        /// <summary>
        /// クリッカブルな「もうひとつ」を含むテキストを即座に全表示する
        /// </summary>
        public void SkipTypewriterWithClickableWord()
        {
            if (currentTypewriterEffect == null || currentContainer == null || string.IsNullOrEmpty(currentFullText)) return;

            // コルーチンを停止
            StopTypewriterEffect();

            // コンテナをクリア
            currentContainer.Clear();

            // ダークモードで失われた文字を取得
            var lostLetters = GameManager.Instance != null ? GameManager.Instance.GetLostLetters() : new HashSet<char>();

            // テキストを解析して一気に表示
            string[] lines = currentFullText.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                
                // ダークモード用に「も」「う」「ひ」「と」「つ」が「※」に置換されたパターンを考慮
                string[] patterns = { MouhitotsuWordManager.GetFormattedWord(), MouhitotsuWordManager.GetWord() };
                List<string> dynamicPatterns = new List<string>();
                char[] letters = MouhitotsuWordManager.GetAllLetters();
                foreach (var p in patterns)
                {
                    dynamicPatterns.Add(p);
                    foreach (var letter in letters)
                    {
                        string replaced = p.Replace(letter.ToString(), "※");
                        if (replaced != p && !dynamicPatterns.Contains(replaced)) dynamicPatterns.Add(replaced);
                    }
                    string allReplaced = p;
                    foreach (var letter in letters) allReplaced = allReplaced.Replace(letter.ToString(), "※");
                    if (allReplaced != p && !dynamicPatterns.Contains(allReplaced)) dynamicPatterns.Add(allReplaced);
                }

                int wordStartIndex = -1;
                int wordLength = 0;
                string clickableText = "";

                foreach (var pattern in dynamicPatterns)
                {
                    wordStartIndex = line.IndexOf(pattern);
                    if (wordStartIndex >= 0)
                    {
                        wordLength = pattern.Length;
                        clickableText = pattern.Replace("【", "").Replace("】", "");
                        break;
                    }
                }

                if (wordStartIndex >= 0 && wordLength > 0)
                {
                    if (wordStartIndex > 0)
                    {
                        string beforeWord = line.Substring(0, wordStartIndex);
                        string replacedBefore = "";
                        foreach (char c in beforeWord) replacedBefore += lostLetters.Contains(c) ? '※' : c;
                        
                        Label beforeLabel = new Label(replacedBefore);
                        beforeLabel.style.fontSize = 20;
                        beforeLabel.style.whiteSpace = WhiteSpace.Normal;
                        beforeLabel.style.alignSelf = Align.FlexStart; // 左揃え
                        beforeLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                        // 明るい色を適用
                        Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                        beforeLabel.style.color = brightTextColor;
                        beforeLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        currentContainer.Add(beforeLabel);
                    }

                    string replacedClickable = "";
                    foreach (char c in clickableText) replacedClickable += lostLetters.Contains(c) ? '※' : c;
                    Label clickableLabel = new Label(replacedClickable);
                    clickableLabel.style.fontSize = 20;
                    clickableLabel.style.whiteSpace = WhiteSpace.Normal;
                    clickableLabel.style.alignSelf = Align.FlexStart; // 左揃え
                    clickableLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                    clickableLabel.style.color = new StyleColor(new Color(0.2f, 0.8f, 0.4f)); // 最初から緑色（見つかった状態）
                    currentContainer.Add(clickableLabel);

                    int wordEndIndex = wordStartIndex + wordLength;
                    if (wordEndIndex < line.Length)
                    {
                        string afterWord = line.Substring(wordEndIndex);
                        string replacedAfter = "";
                        foreach (char c in afterWord) replacedAfter += lostLetters.Contains(c) ? '※' : c;
                        
                        Label afterLabel = new Label(replacedAfter);
                        afterLabel.style.fontSize = 20;
                        afterLabel.style.whiteSpace = WhiteSpace.Normal;
                        afterLabel.style.alignSelf = Align.FlexStart; // 左揃え
                        afterLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                        // 明るい色を適用
                        Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                        afterLabel.style.color = brightTextColor;
                        afterLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        currentContainer.Add(afterLabel);
                    }
                }
                else
                {
                    string replacedLine = "";
                    foreach (char c in line) replacedLine += lostLetters.Contains(c) ? '※' : c;
                    Label textLabel = new Label(replacedLine);
                    textLabel.style.fontSize = 20;
                    textLabel.style.whiteSpace = WhiteSpace.Normal;
                    textLabel.style.alignSelf = Align.FlexStart; // 左揃え
                    textLabel.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                    // 明るい色を適用
                    Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                    textLabel.style.color = brightTextColor;
                    textLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                    currentContainer.Add(textLabel);
                }

                if (lineIndex < lines.Length - 1)
                {
                    Label lineBreak = new Label("\n");
                    lineBreak.style.fontSize = 20;
                    currentContainer.Add(lineBreak);
                }
            }

            currentContainer = null;
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

