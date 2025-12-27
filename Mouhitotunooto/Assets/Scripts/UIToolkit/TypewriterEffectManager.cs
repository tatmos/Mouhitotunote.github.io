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
        private Coroutine currentTypewriterEffect;
        private Label clickableWordLabel = null;
        private System.Action<bool> onWordFoundCallback; // ワードが見つかった時のコールバック（bool: 見つかったかどうか）

        /// <summary>
        /// タイプライター効果を開始（1行ずつ時間差で、左から文字を表示）
        /// </summary>
        public void StartTypewriterEffect(Label label, string fullText, System.Action onComplete = null)
        {
            // 既存のタイプライター効果を停止
            if (currentTypewriterEffect != null)
            {
                StopCoroutine(currentTypewriterEffect);
            }

            // 初期状態：テキストを空にする
            label.text = "";

            // タイプライター効果開始
            currentTypewriterEffect = StartCoroutine(TypewriterEffectCoroutine(label, fullText, onComplete));
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
                            beforeLabel.text = beforeWord.Substring(0, i + 1);
                            yield return new WaitForSeconds(charDelay);
                        }
                    }
                    
                    // 「もうひとつ」をクリッカブルなLabelとして表示
                    Label clickableLabel = new Label(clickableText);
                    clickableLabel.style.fontSize = 20;
                    clickableLabel.style.whiteSpace = WhiteSpace.Normal;
                    clickableLabel.style.color = new StyleColor(new Color(0.2f, 0.6f, 1.0f)); // 青色
                    clickableLabel.AddToClassList("clickable-word");
                    clickableLabel.RegisterCallback<ClickEvent>(OnWordClicked);
                    clickableWordLabel = clickableLabel;
                    container.Add(clickableLabel);
                    
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
                            afterLabel.text = afterWord.Substring(0, i + 1);
                            yield return new WaitForSeconds(charDelay);
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
                        textLabel.text = line.Substring(0, charIndex + 1);
                        yield return new WaitForSeconds(charDelay);
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
        private IEnumerator TypewriterEffectCoroutine(Label label, string fullText, System.Action onComplete = null)
        {
            // テキストを行ごとに分割
            string[] lines = fullText.Split('\n');
            
            float charDelay = 0.03f; // 1文字あたりの遅延（秒）
            float lineDelay = 0.15f; // 行間の遅延（秒）

            string displayedText = "";

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                
                // 各行を1文字ずつ表示
                for (int charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    // 現在の行までの完全に表示されたテキスト + 現在の行の部分的なテキスト
                    string currentText = displayedText + line.Substring(0, charIndex + 1);
                    
                    label.text = currentText;
                    yield return new WaitForSeconds(charDelay);
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

