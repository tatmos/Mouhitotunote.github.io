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
                        yield return StartCoroutine(AnimateLetterExplosionInLabel(targetLabel, letter, root));
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
                            yield return StartCoroutine(AnimateLetterExplosionInLabel(childLabel, letter, root));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Label内の文字が物理的に弾け飛ぶアニメーション（派手な版）
        /// </summary>
        private IEnumerator AnimateLetterExplosionInLabel(Label label, char letter, VisualElement root)
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

            // 演出用コンテナ（画面全体をカバー）
            var effectContainer = new VisualElement();
            effectContainer.style.position = Position.Absolute;
            effectContainer.style.left = 0;
            effectContainer.style.top = 0;
            effectContainer.style.right = 0;
            effectContainer.style.bottom = 0;
            effectContainer.pickingMode = PickingMode.Ignore;
            root.Add(effectContainer);

            // 一時的なLabelを作成して文字を表示
            Label fallingLetter = new Label(letter.ToString());
            fallingLetter.style.position = Position.Absolute;
            fallingLetter.style.fontSize = label.style.fontSize.value;
            fallingLetter.style.color = new Color(1f, 0.3f, 0.3f, 1f); // 赤みがかった色
            fallingLetter.style.textShadow = label.style.textShadow.value;
            fallingLetter.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            // 元のLabelの位置を基準に文字の位置を概算
            float fontSize = label.style.fontSize.value.value;
            float estimatedX = label.worldBound.x + (letterIndex * fontSize * 0.6f);
            float estimatedY = label.worldBound.y;
            
            // 親要素の位置を基準に相対位置を計算
            fallingLetter.style.left = estimatedX - root.worldBound.x;
            fallingLetter.style.top = estimatedY - root.worldBound.y;
            
            effectContainer.Add(fallingLetter);
            
            // 物理的な動き（ランダムな方向に弾け飛ぶ）
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(400f, 800f);
            Vector2 velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
            float gravity = 600f; // 重力（強め）
            float rotationSpeed = Random.Range(-720f, 720f); // 回転速度（速め）
            
            Vector2 startPos = new Vector2(estimatedX - root.worldBound.x, estimatedY - root.worldBound.y);
            
            // 落下アニメーション
            float fallDuration = 2.0f;
            float elapsed = 0f;
            
            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fallDuration;
                
                // 物理的な動き（重力を考慮）
                velocity.y -= gravity * Time.deltaTime;
                Vector2 currentPos = startPos + velocity * elapsed;
                
                fallingLetter.style.left = currentPos.x;
                fallingLetter.style.top = currentPos.y;
                
                // 3D的な回転（立体的に）
                float rotation = rotationSpeed * elapsed;
                fallingLetter.style.rotate = new Rotate(new Angle(rotation));
                
                // スケールダウン（遠くに飛んでいく感じ）
                float scale = 1f - (progress * progress); // 加速的に縮小
                fallingLetter.style.scale = new Scale(new Vector2(scale, scale));
                
                // フェードアウト（加速的に）
                float opacity = 1f - (progress * progress * progress); // 三次的にフェードアウト
                fallingLetter.style.opacity = opacity;
                
                // 画面外に出たら削除
                if (currentPos.x < -200f || currentPos.x > root.worldBound.width + 200f ||
                    currentPos.y < -200f || currentPos.y > root.worldBound.height + 200f)
                {
                    break;
                }
                
                yield return null;
            }
            
            // アニメーション完了後、Labelを削除
            root.Remove(effectContainer);
            
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
