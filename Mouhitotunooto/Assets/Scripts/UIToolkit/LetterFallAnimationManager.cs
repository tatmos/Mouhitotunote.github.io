using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// 文字が落下して消えるアニメーションを管理するクラス
    /// </summary>
    public class LetterFallAnimationManager : MonoBehaviour
    {
        private GameManager gameManager;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        /// <summary>
        /// 文字が落下して消えるアニメーションを開始
        /// </summary>
        public void AnimateLetterFall(char letter, VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            StartCoroutine(AnimateLetterFallCoroutine(letter, root));
        }

        /// <summary>
        /// 文字が落下して消えるアニメーションのコルーチン
        /// </summary>
        private IEnumerator AnimateLetterFallCoroutine(char letter, VisualElement root)
        {
            // スコア表示と「もうひとつ」を含む要素を探す
            var scoreLabel = root.Q<Label>("ScoreText");
            var wordGetLabel = root.Q<Label>("WordGetText");
            var titleLabel = root.Q<Label>("TitleText");
            var mysteryVoiceText = root.Q<VisualElement>("MysteryVoiceText");

            List<VisualElement> targetElements = new List<VisualElement>();
            if (scoreLabel != null) targetElements.Add(scoreLabel);
            if (wordGetLabel != null) targetElements.Add(wordGetLabel);
            if (titleLabel != null) targetElements.Add(titleLabel);
            if (mysteryVoiceText != null) targetElements.Add(mysteryVoiceText);

            // 各要素から該当する文字を探してアニメーション
            foreach (var element in targetElements)
            {
                if (element is Label targetLabel)
                {
                    string text = targetLabel.text;
                    // リッチテキストタグを除去して検索
                    string plainText = Regex.Replace(text, "<[^>]+>", "");
                    if (plainText.Contains(letter))
                    {
                        yield return StartCoroutine(AnimateLetterFallInLabel(targetLabel, letter));
                    }
                }
                else
                {
                    // VisualElement内の子要素（Label）を探す
                    var labels = element.Query<Label>().ToList();
                    foreach (var childLabel in labels)
                    {
                        string text = childLabel.text;
                        // リッチテキストタグを除去して検索
                        string plainText = Regex.Replace(text, "<[^>]+>", "");
                        if (plainText.Contains(letter))
                        {
                            yield return StartCoroutine(AnimateLetterFallInLabel(childLabel, letter));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Label内の文字が落下して消えるアニメーション
        /// </summary>
        private IEnumerator AnimateLetterFallInLabel(Label label, char letter)
        {
            if (label == null || string.IsNullOrEmpty(label.text))
            {
                yield break;
            }

            // リッチテキストタグを除去して検索
            string text = label.text;
            string plainText = Regex.Replace(text, "<[^>]+>", "");
            
            if (!plainText.Contains(letter))
            {
                yield break;
            }

            // 文字の位置を概算（簡易版：文字数から概算）
            int letterIndex = plainText.IndexOf(letter);
            if (letterIndex < 0)
            {
                yield break;
            }

            // 元のLabelの位置とスタイルを取得
            var parent = label.parent;
            if (parent == null)
            {
                yield break;
            }

            // 一時的なLabelを作成して文字を表示
            Label fallingLetter = new Label(letter.ToString());
            fallingLetter.style.position = Position.Absolute;
            fallingLetter.style.fontSize = label.style.fontSize.value;
            fallingLetter.style.color = label.style.color.value;
            fallingLetter.style.textShadow = label.style.textShadow.value;
            
            // 元のLabelの位置を基準に文字の位置を概算
            float fontSize = label.style.fontSize.value.value;
            float estimatedX = label.worldBound.x + (letterIndex * fontSize * 0.6f);
            float estimatedY = label.worldBound.y;
            
            parent.Add(fallingLetter);
            
            // 親要素の位置を基準に相対位置を計算
            fallingLetter.style.left = estimatedX - parent.worldBound.x;
            fallingLetter.style.top = estimatedY - parent.worldBound.y;
            
            // 落下アニメーション
            float fallDuration = 1.0f;
            float elapsed = 0f;
            
            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fallDuration;
                
                // 落下（重力加速度を考慮）
                float gravity = 9.8f * 100f; // ピクセル単位に変換
                float currentY = estimatedY - parent.worldBound.y + (0.5f * gravity * progress * progress);
                fallingLetter.style.top = currentY;
                
                // フェードアウト
                float opacity = 1f - (progress * progress); // 加速的にフェードアウト
                fallingLetter.style.opacity = opacity;
                
                // 回転（オプション）
                float rotation = progress * 360f;
                fallingLetter.style.rotate = new Rotate(new Angle(rotation));
                
                yield return null;
            }
            
            // アニメーション完了後、Labelを削除
            parent.Remove(fallingLetter);
            
            // 元のテキストを更新（文字を※に置換）
            // TextFormatterを使用して更新
            if (gameManager != null)
            {
                var lostLetters = gameManager.GetLostLetters();
                var collectedLetters = gameManager.GetCollectedLetters();
                string updatedText = TextFormatter.FormatText(text, collectedLetters, lostLetters, true);
                label.text = updatedText;
            }
        }
    }
}

