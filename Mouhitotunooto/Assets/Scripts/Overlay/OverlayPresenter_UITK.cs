using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame.Overlay
{
    /// <summary>
    /// UI Toolkitへ反映（画像・テキスト・フェード・アニメ）
    /// </summary>
    public class OverlayPresenter_UITK
    {
        private readonly VisualElement root;
        private readonly VisualElement overlayRoot;
        private readonly VisualElement roomImage;
        private readonly VisualElement girlImage;
        private readonly VisualElement balloonRoot;
        private readonly Label balloonLabel;
        private readonly VisualElement thoughtBalloonRoot;
        private readonly Label thoughtBalloonLabel;
        private readonly VisualElement musicNoteLayer;
        private readonly MonoBehaviour coroutineRunner;
        private Coroutine musicNoteCoroutine;

        public OverlayPresenter_UITK(VisualElement root, MonoBehaviour coroutineRunner)
        {
            this.root = root;
            this.coroutineRunner = coroutineRunner;

            overlayRoot = OverlayViewBindings.GetElement<VisualElement>(root, OverlayViewBindings.OverlayRoot);
            roomImage = OverlayViewBindings.GetElement<VisualElement>(root, OverlayViewBindings.RoomImage);
            girlImage = OverlayViewBindings.GetElement<VisualElement>(root, OverlayViewBindings.GirlImage);
            balloonRoot = OverlayViewBindings.GetElement<VisualElement>(root, OverlayViewBindings.BalloonRoot);
            balloonLabel = OverlayViewBindings.GetElement<Label>(root, OverlayViewBindings.BalloonLabel);
            thoughtBalloonRoot = OverlayViewBindings.GetElement<VisualElement>(root, OverlayViewBindings.ThoughtBalloonRoot);
            thoughtBalloonLabel = OverlayViewBindings.GetElement<Label>(root, OverlayViewBindings.ThoughtBalloonLabel);
            musicNoteLayer = OverlayViewBindings.GetElement<VisualElement>(root, OverlayViewBindings.MusicNoteLayer);
            
            // すべてのオーバーレイ要素にpickingModeをIgnoreに設定（イベントを無視するため）
            if (overlayRoot != null) overlayRoot.pickingMode = PickingMode.Ignore;
            if (roomImage != null) roomImage.pickingMode = PickingMode.Ignore;
            if (girlImage != null) girlImage.pickingMode = PickingMode.Ignore;
            if (balloonRoot != null) balloonRoot.pickingMode = PickingMode.Ignore;
            if (balloonLabel != null) balloonLabel.pickingMode = PickingMode.Ignore;
            if (thoughtBalloonRoot != null) thoughtBalloonRoot.pickingMode = PickingMode.Ignore;
            if (thoughtBalloonLabel != null) thoughtBalloonLabel.pickingMode = PickingMode.Ignore;
            if (musicNoteLayer != null) musicNoteLayer.pickingMode = PickingMode.Ignore;
        }

        /// <summary>
        /// リアクションを表示
        /// </summary>
        public void ShowReaction(ReactionPayload payload)
        {
            if (payload == null) return;

            // 表情を設定
            SetExpression(payload.Expression);

            // 部屋状態を設定
            SetRoomState(payload.RoomState);

            // 吹き出しを表示
            if (payload.IsThought)
            {
                ShowThoughtBalloon(payload.Text, payload.DisplayDuration);
            }
            else
            {
                ShowBalloon(payload.Text, payload.DisplayDuration);
            }
        }

        /// <summary>
        /// 表情を設定
        /// </summary>
        private void SetExpression(GirlExpression expression)
        {
            if (girlImage == null) return;

            Sprite sprite = OverlayAssets.GetExpressionSprite(expression);
            if (sprite != null)
            {
                girlImage.style.backgroundImage = new StyleBackground(sprite);
            }
        }

        /// <summary>
        /// 部屋状態を設定
        /// </summary>
        private void SetRoomState(RoomState roomState)
        {
            if (roomImage == null) return;

            Texture2D texture = OverlayAssets.GetRoomTexture(roomState);
            if (texture != null)
            {
                roomImage.style.backgroundImage = new StyleBackground(texture);
            }
        }

        /// <summary>
        /// 通常吹き出しを表示
        /// </summary>
        private void ShowBalloon(string text, float duration)
        {
            if (balloonRoot == null || balloonLabel == null) return;

            // 心の声を非表示
            if (thoughtBalloonRoot != null)
            {
                thoughtBalloonRoot.style.display = DisplayStyle.None;
            }

            // 通常吹き出しを表示
            balloonLabel.text = text;
            balloonRoot.style.display = DisplayStyle.Flex;

            // フェードイン
            coroutineRunner.StartCoroutine(FadeInAndOut(balloonRoot, duration));
        }

        /// <summary>
        /// 心の声（点線枠）を表示
        /// </summary>
        private void ShowThoughtBalloon(string text, float duration)
        {
            if (thoughtBalloonRoot == null || thoughtBalloonLabel == null) return;

            // 通常吹き出しを非表示
            if (balloonRoot != null)
            {
                balloonRoot.style.display = DisplayStyle.None;
            }

            // 心の声を表示
            thoughtBalloonLabel.text = text;
            thoughtBalloonRoot.style.display = DisplayStyle.Flex;

            // フェードイン
            coroutineRunner.StartCoroutine(FadeInAndOut(thoughtBalloonRoot, duration));
        }

        /// <summary>
        /// フェードイン→表示→フェードアウト
        /// </summary>
        private IEnumerator FadeInAndOut(VisualElement element, float duration)
        {
            if (element == null) yield break;

            // フェードイン（0.3秒）
            float fadeInTime = 0.3f;
            float elapsed = 0f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeInTime);
                element.style.opacity = alpha;
                yield return null;
            }
            element.style.opacity = 1f;

            // 表示時間（duration秒）
            yield return new WaitForSeconds(duration);

            // フェードアウト（0.3秒）
            float fadeOutTime = 0.3f;
            elapsed = 0f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
                element.style.opacity = alpha;
                yield return null;
            }
            element.style.opacity = 0f;

            // 非表示
            element.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Overlay全体の表示/非表示
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (overlayRoot == null) return;

            overlayRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// フェーズに応じて表示を更新
        /// </summary>
        public void UpdatePhase(OverlayPhase phase)
        {
            bool visible = phase != OverlayPhase.Hidden;
            SetVisible(visible);
        }

        /// <summary>
        /// エンドクレジット開始：歌う表情に変更し、音符エフェクトを開始
        /// </summary>
        public void StartCreditsSinging()
        {
            // 表情を歌う状態に変更
            SetExpression(GirlExpression.Singing);
            
            // 音符エフェクトを開始
            if (musicNoteLayer != null && coroutineRunner != null)
            {
                if (musicNoteCoroutine != null)
                {
                    coroutineRunner.StopCoroutine(musicNoteCoroutine);
                }
                musicNoteCoroutine = coroutineRunner.StartCoroutine(MusicNoteEffectCoroutine());
            }
        }

        /// <summary>
        /// エンドクレジット終了：音符エフェクトを停止
        /// </summary>
        public void StopCreditsSinging()
        {
            // 音符エフェクトを停止
            if (musicNoteCoroutine != null && coroutineRunner != null)
            {
                coroutineRunner.StopCoroutine(musicNoteCoroutine);
                musicNoteCoroutine = null;
            }
            
            // 音符レイヤーをクリア
            if (musicNoteLayer != null)
            {
                musicNoteLayer.Clear();
            }
        }

        /// <summary>
        /// 音符エフェクトのコルーチン
        /// </summary>
        private IEnumerator MusicNoteEffectCoroutine()
        {
            if (musicNoteLayer == null || girlImage == null) yield break;

            // 音符の文字配列
            // 注意: ♫が文字化けする場合は、ZenMaruGothic-Regularフォントアセットに♫（U+266B）を追加してください
            // 詳細は MUSIC_NOTE_FONT_GUIDE.md を参照
            string[] musicNotes = { "♪", "♫", "♪", "♫", "♪" };
            float spawnInterval = 0.5f; // 音符を生成する間隔（秒）
            float noteLifetime = 2.0f; // 音符の生存時間（秒）
            float noteSpeed = 50f; // 音符が上に移動する速度（ピクセル/秒）

            while (true)
            {
                // ランダムな音符を選択
                string note = musicNotes[Random.Range(0, musicNotes.Length)];
                
                // 音符のラベルを作成
                Label noteLabel = new Label(note);
                noteLabel.style.fontSize = 24;
                noteLabel.style.color = new Color(1f, 1f, 0.8f, 1f); // 淡い黄色
                noteLabel.style.position = Position.Absolute;
                
                // 注意: ♫が文字化けする場合は、ZenMaruGothic-Regularフォントアセットに♫（U+266B）を追加してください
                // フォントアセットに♫が含まれていれば、特別な処理は不要です
                // 詳細は MUSIC_NOTE_FONT_GUIDE.md を参照
                
                // 実況者の位置を基準にランダムな位置に配置
                float randomX = Random.Range(-30f, 30f);
                float randomY = Random.Range(-20f, 20f);
                noteLabel.style.left = new Length(50f + randomX, LengthUnit.Percent);
                noteLabel.style.bottom = new Length(50f + randomY, LengthUnit.Percent);
                noteLabel.pickingMode = PickingMode.Ignore;
                
                musicNoteLayer.Add(noteLabel);
                
                // 音符を上に浮かび上がらせるアニメーション
                float elapsed = 0f;
                float startY = randomY;
                
                while (elapsed < noteLifetime)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / noteLifetime;
                    
                    // 上に移動
                    float currentY = startY + (noteSpeed * elapsed);
                    noteLabel.style.bottom = new Length(50f + currentY, LengthUnit.Percent);
                    
                    // フェードイン→フェードアウト
                    if (progress < 0.2f)
                    {
                        // フェードイン
                        noteLabel.style.opacity = Mathf.Lerp(0f, 1f, progress / 0.2f);
                    }
                    else if (progress > 0.8f)
                    {
                        // フェードアウト
                        noteLabel.style.opacity = Mathf.Lerp(1f, 0f, (progress - 0.8f) / 0.2f);
                    }
                    else
                    {
                        noteLabel.style.opacity = 1f;
                    }
                    
                    // 少し拡大
                    float scale = 1f + (progress * 0.3f);
                    noteLabel.style.scale = new Scale(new Vector3(scale, scale, 1f));
                    
                    yield return null;
                }
                
                // 音符を削除
                musicNoteLayer.Remove(noteLabel);
                
                // 次の音符を生成するまで待機
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}

