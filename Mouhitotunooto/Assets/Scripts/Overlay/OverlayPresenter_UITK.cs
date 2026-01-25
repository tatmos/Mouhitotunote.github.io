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
            
            // 要素の取得確認
            if (overlayRoot == null)
            {
                Debug.LogError("[OverlayPresenter_UITK] OverlayRootが見つかりません。UXMLが正しく読み込まれているか確認してください。");
            }
            
            // 画像要素の強制サイズ設定（UXMLの設定が反映されていない場合）
            // 背景とキャラを同じ位置に重ねて配置
            float overlayX = 1320;  // 右下配置のX座標
            float overlayY = 700;  // 右下配置のY座標
            
            if (roomImage != null)
            {
                if (roomImage.style.width.value.value == 0 || roomImage.style.height.value.value == 0)
                {
                    roomImage.style.width = 200;
                    roomImage.style.height = 150;
                }
                roomImage.style.position = Position.Absolute;
                roomImage.style.left = overlayX;
                roomImage.style.top = overlayY;
                roomImage.style.right = StyleKeyword.Auto;
                roomImage.style.bottom = StyleKeyword.Auto;
            }
            
            if (girlImage != null)
            {
                if (girlImage.style.width.value.value == 0 || girlImage.style.height.value.value == 0)
                {
                    girlImage.style.width = 200;
                    girlImage.style.height = 150;
                }
                girlImage.style.position = Position.Absolute;
                girlImage.style.left = overlayX;  // 背景と同じX座標
                girlImage.style.top = overlayY;   // 背景と同じY座標
                girlImage.style.right = StyleKeyword.Auto;
                girlImage.style.bottom = StyleKeyword.Auto;
                
                // GirlImageをRoomImageの後に配置して上に表示
                if (overlayRoot != null && roomImage != null && girlImage.parent == overlayRoot)
                {
                    overlayRoot.Remove(girlImage);
                    overlayRoot.Add(girlImage);
                }
            }
            
            // すべてのオーバーレイ要素にpickingModeをIgnoreに設定（イベントを無視するため）
            if (overlayRoot != null) overlayRoot.pickingMode = PickingMode.Ignore;
            if (roomImage != null) roomImage.pickingMode = PickingMode.Ignore;
            if (girlImage != null) girlImage.pickingMode = PickingMode.Ignore;
            if (balloonRoot != null) balloonRoot.pickingMode = PickingMode.Ignore;
            if (balloonLabel != null) balloonLabel.pickingMode = PickingMode.Ignore;
            if (thoughtBalloonRoot != null) thoughtBalloonRoot.pickingMode = PickingMode.Ignore;
            if (thoughtBalloonLabel != null) thoughtBalloonLabel.pickingMode = PickingMode.Ignore;
            if (musicNoteLayer != null) musicNoteLayer.pickingMode = PickingMode.Ignore;
            
            // 座標系解明結果に基づいて吹き出し位置を修正
            FixBalloonCoordinates();
        }

        /// <summary>
        /// 座標系解明結果に基づいて吹き出し位置を修正
        /// </summary>
        private void FixBalloonCoordinates()
        {
            // GirlImageの解明座標: left=1320, top=700
            // 吹き出しはGirlImageの左側に配置
            float balloonLeft = 1020; // GirlImageの左側（1320 - 300px）
            float balloonTop = 600;   // GirlImageより少し上
            
            if (balloonRoot != null)
            {
                balloonRoot.style.position = Position.Absolute;
                balloonRoot.style.left = balloonLeft;
                balloonRoot.style.top = balloonTop;
                balloonRoot.style.right = StyleKeyword.Auto;  // 古いright設定をクリア
                balloonRoot.style.bottom = StyleKeyword.Auto; // 古いbottom設定をクリア
                balloonRoot.style.width = 280; // 固定幅
                balloonRoot.style.maxWidth = 300;
                balloonRoot.style.minWidth = 200;
            }
            
            if (thoughtBalloonRoot != null)
            {
                thoughtBalloonRoot.style.position = Position.Absolute;
                thoughtBalloonRoot.style.left = balloonLeft;
                thoughtBalloonRoot.style.top = balloonTop;
                thoughtBalloonRoot.style.right = StyleKeyword.Auto;  // 古いright設定をクリア
                thoughtBalloonRoot.style.bottom = StyleKeyword.Auto; // 古いbottom設定をクリア
                thoughtBalloonRoot.style.width = 280; // 固定幅
                thoughtBalloonRoot.style.maxWidth = 300;
                thoughtBalloonRoot.style.minWidth = 200;
            }
        }

        /// <summary>
        /// リアクションを表示
        /// </summary>
        public void ShowReaction(ReactionPayload payload)
        {
            if (payload == null)
            {
                Debug.LogWarning("[OverlayPresenter_UITK] ShowReaction: payloadがnullです");
                return;
            }

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
            if (girlImage == null)
            {
                Debug.LogWarning("[OverlayPresenter_UITK] SetExpression: girlImageがnullです。");
                return;
            }

            Sprite sprite = OverlayAssets.GetExpressionSprite(expression);
            if (sprite != null)
            {
                girlImage.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                Debug.LogWarning($"[OverlayPresenter_UITK] 表情Spriteが見つかりません: {expression}");
            }
        }

        /// <summary>
        /// 部屋状態を設定
        /// </summary>
        private void SetRoomState(RoomState roomState)
        {
            if (roomImage == null)
            {
                Debug.LogWarning("[OverlayPresenter_UITK] SetRoomState: roomImageがnullです。");
                return;
            }

            Texture2D texture = OverlayAssets.GetRoomTexture(roomState);
            if (texture != null)
            {
                roomImage.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                Debug.LogWarning($"[OverlayPresenter_UITK] 部屋Textureが見つかりません: {roomState}");
            }
        }

        /// <summary>
        /// 通常吹き出しを表示
        /// </summary>
        private void ShowBalloon(string text, float duration)
        {
            if (balloonRoot == null || balloonLabel == null)
            {
                Debug.LogError($"[OverlayPresenter_UITK] 吹き出し要素がnull - balloonRoot: {balloonRoot != null}, balloonLabel: {balloonLabel != null}");
                return;
            }

            // 心の声を非表示
            if (thoughtBalloonRoot != null)
            {
                thoughtBalloonRoot.style.display = DisplayStyle.None;
            }

            // 通常吹き出しを表示
            balloonLabel.text = text;
            balloonRoot.style.display = DisplayStyle.Flex;
            balloonRoot.style.visibility = Visibility.Visible;
            balloonRoot.style.opacity = 1f;
            balloonRoot.MarkDirtyRepaint();

            // フェードイン
            coroutineRunner.StartCoroutine(FadeInAndOut(balloonRoot, duration));
        }

        /// <summary>
        /// 心の声（点線枠）を表示
        /// </summary>
        private void ShowThoughtBalloon(string text, float duration)
        {
            if (thoughtBalloonRoot == null || thoughtBalloonLabel == null)
            {
                Debug.LogError($"[OverlayPresenter_UITK] 心の声要素がnull - thoughtBalloonRoot: {thoughtBalloonRoot != null}, thoughtBalloonLabel: {thoughtBalloonLabel != null}");
                return;
            }

            // 通常吹き出しを非表示
            if (balloonRoot != null)
            {
                balloonRoot.style.display = DisplayStyle.None;
            }

            // 心の声を表示
            thoughtBalloonLabel.text = text;
            thoughtBalloonRoot.style.display = DisplayStyle.Flex;
            thoughtBalloonRoot.style.visibility = Visibility.Visible;
            thoughtBalloonRoot.style.opacity = 1f;
            thoughtBalloonRoot.MarkDirtyRepaint();

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
            if (overlayRoot == null)
            {
                Debug.LogWarning("[OverlayPresenter_UITK] SetVisible: overlayRootがnullです。");
                return;
            }

            if (visible)
            {
                // 表示時：すべての要素を確実に表示状態にする
                overlayRoot.style.display = DisplayStyle.Flex;
                overlayRoot.style.visibility = Visibility.Visible;
                overlayRoot.style.opacity = 1f;
                
                if (girlImage != null)
                {
                    girlImage.style.display = DisplayStyle.Flex;
                    girlImage.style.visibility = Visibility.Visible;
                    girlImage.style.opacity = 1f;
                }
                
                if (roomImage != null)
                {
                    roomImage.style.display = DisplayStyle.Flex;
                    roomImage.style.visibility = Visibility.Visible;
                    roomImage.style.opacity = 1f;
                }
                
                overlayRoot.MarkDirtyRepaint();
                if (girlImage != null) girlImage.MarkDirtyRepaint();
                if (roomImage != null) roomImage.MarkDirtyRepaint();
            }
            else
            {
                // 非表示時
                overlayRoot.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// フェーズに応じて表示を更新
        /// </summary>
        public void UpdatePhase(OverlayPhase phase)
        {
            bool visible = phase != OverlayPhase.Hidden;
            Debug.Log($"[OverlayPresenter_UITK] UpdatePhase: {phase}, visible: {visible}");
            SetVisible(visible);
            
            // PhaseがHidden以外になった時に、デフォルトの画像を設定
            if (visible)
            {
                // 背景色を透明に設定
                if (girlImage != null)
                {
                    girlImage.style.backgroundColor = Color.clear;
                }
                if (roomImage != null)
                {
                    roomImage.style.backgroundColor = Color.clear;
                }
                
                // デフォルトの表情と部屋状態を設定
                if (girlImage != null)
                {
                    var bgImage = girlImage.style.backgroundImage.value;
                    bool hasImage = bgImage != null && bgImage.texture != null;
                    if (!hasImage)
                    {
                        SetExpression(GirlExpression.Neutral);
                    }
                }
                else
                {
                    Debug.LogWarning("[OverlayPresenter_UITK] girlImageがnullです。");
                }
                
                if (roomImage != null)
                {
                    var bgImage = roomImage.style.backgroundImage.value;
                    bool hasImage = bgImage != null && bgImage.texture != null;
                    if (!hasImage)
                    {
                        SetRoomState(RoomState.CleanDay);
                    }
                }
                else
                {
                    Debug.LogWarning("[OverlayPresenter_UITK] roomImageがnullです。");
                }
                
                // 背景（RoomImage）とキャラ（GirlImage）を同じ位置に重ねて配置
                // 背景を下に、キャラを上に表示するため、両方とも同じ座標を使用
                float overlayX = 1320;  // 右下配置のX座標
                float overlayY = 700;  // 右下配置のY座標
                
                // 背景（RoomImage）を先に配置（下層）
                if (roomImage != null)
                {
                    bool needsSizeFix = (roomImage.resolvedStyle.width == 0 || roomImage.resolvedStyle.height == 0);
                    
                    if (needsSizeFix)
                    {
                        Debug.LogWarning("[OverlayPresenter_UITK] RoomImageのサイズが0x0です。座標系解明結果でサイズと座標を設定します。");
                    }
                    
                    // 背景を同じ位置に配置（下層）
                    roomImage.style.position = Position.Absolute;
                    roomImage.style.left = overlayX;
                    roomImage.style.top = overlayY;
                    roomImage.style.right = StyleKeyword.Auto;  // 古いright/bottomをクリア
                    roomImage.style.bottom = StyleKeyword.Auto;
                    
                    if (needsSizeFix)
                    {
                        roomImage.style.width = 200;
                        roomImage.style.height = 150;
                        roomImage.style.minWidth = 200;
                        roomImage.style.minHeight = 150;
                    }
                    
                    roomImage.style.display = DisplayStyle.Flex;
                    roomImage.style.visibility = Visibility.Visible;
                    roomImage.style.opacity = 1f;
                    roomImage.MarkDirtyRepaint();
                }
                
                // キャラ（GirlImage）を同じ位置に配置（上層、背景の上）
                if (girlImage != null)
                {
                    bool needsSizeFix = (girlImage.resolvedStyle.width == 0 || girlImage.resolvedStyle.height == 0);
                    
                    if (needsSizeFix)
                    {
                        Debug.LogWarning("[OverlayPresenter_UITK] GirlImageのサイズが0x0です。座標系解明結果でサイズと座標を設定します。");
                    }
                    
                    // キャラを背景と同じ位置に配置（上層）
                    girlImage.style.position = Position.Absolute;
                    girlImage.style.left = overlayX;  // 背景と同じX座標
                    girlImage.style.top = overlayY;   // 背景と同じY座標
                    girlImage.style.right = StyleKeyword.Auto;  // 古いright/bottomをクリア
                    girlImage.style.bottom = StyleKeyword.Auto;
                    
                    // z-indexを設定してキャラを上に表示（UI Toolkitでは要素の順序が重要）
                    // GirlImageをRoomImageの後に配置することで、自動的に上に表示される
                    if (overlayRoot != null && roomImage != null && girlImage.parent == overlayRoot)
                    {
                        // GirlImageをRoomImageの後に移動（DOM順序で上に表示）
                        overlayRoot.Remove(girlImage);
                        overlayRoot.Add(girlImage);
                    }
                    
                    if (needsSizeFix)
                    {
                        girlImage.style.width = 200;
                        girlImage.style.height = 150;
                        girlImage.style.minWidth = 200;
                        girlImage.style.minHeight = 150;
                    }
                    
                    girlImage.style.display = DisplayStyle.Flex;
                    girlImage.style.visibility = Visibility.Visible;
                    girlImage.style.opacity = 1f;
                    girlImage.MarkDirtyRepaint();
                }
                
                // 表示状態を確実にする
                if (girlImage != null)
                {
                    girlImage.style.display = DisplayStyle.Flex;
                    girlImage.style.visibility = Visibility.Visible;
                    girlImage.style.opacity = 1f;
                    girlImage.MarkDirtyRepaint();
                }
                if (roomImage != null)
                {
                    roomImage.style.display = DisplayStyle.Flex;
                    roomImage.style.visibility = Visibility.Visible;
                    roomImage.style.opacity = 1f;
                    roomImage.MarkDirtyRepaint();
                }
                
                // OverlayRoot自体も確実に表示状態にする
                if (overlayRoot != null)
                {
                    overlayRoot.style.display = DisplayStyle.Flex;
                    overlayRoot.style.visibility = Visibility.Visible;
                    overlayRoot.style.opacity = 1f;
                    overlayRoot.MarkDirtyRepaint();
                }
            }
        }

        /// <summary>
        /// エンドクレジット開始：歌う表情に変更し、音符エフェクトを開始
        /// </summary>
        public void StartCreditsSinging()
        {
            // 表情を歌う状態に変更
            SetExpression(GirlExpression.Singing);
            
            // 音符エフェクトレイヤーの位置とサイズをgirlImageと同じに設定
            if (musicNoteLayer != null && girlImage != null)
            {
                // girlImageの位置とサイズを取得（resolvedStyleまたはstyleから）
                float girlX = 1320f; // デフォルト値
                float girlY = 700f;  // デフォルト値
                float girlWidth = 200f;  // デフォルト値
                float girlHeight = 150f; // デフォルト値
                
                // resolvedStyleから取得を試みる
                if (girlImage.resolvedStyle.width > 0)
                {
                    girlWidth = girlImage.resolvedStyle.width;
                }
                if (girlImage.resolvedStyle.height > 0)
                {
                    girlHeight = girlImage.resolvedStyle.height;
                }
                if (girlImage.resolvedStyle.left >= 0)
                {
                    girlX = girlImage.resolvedStyle.left;
                }
                else if (girlImage.style.left.value.unit == LengthUnit.Pixel)
                {
                    girlX = girlImage.style.left.value.value;
                }
                if (girlImage.resolvedStyle.top >= 0)
                {
                    girlY = girlImage.resolvedStyle.top;
                }
                else if (girlImage.style.top.value.unit == LengthUnit.Pixel)
                {
                    girlY = girlImage.style.top.value.value;
                }
                
                // musicNoteLayerをgirlImageと同じ位置・サイズに設定
                musicNoteLayer.style.position = Position.Absolute;
                musicNoteLayer.style.left = girlX;
                musicNoteLayer.style.top = girlY;
                musicNoteLayer.style.right = StyleKeyword.Auto;
                musicNoteLayer.style.bottom = StyleKeyword.Auto;
                musicNoteLayer.style.width = girlWidth;
                musicNoteLayer.style.height = girlHeight;
                musicNoteLayer.style.display = DisplayStyle.Flex;
                musicNoteLayer.style.visibility = Visibility.Visible;
                musicNoteLayer.style.overflow = Overflow.Visible;
                
                Debug.Log($"[OverlayPresenter_UITK] MusicNoteLayerを設定: left={girlX}, top={girlY}, width={girlWidth}, height={girlHeight}");
            }
            
            // 音符エフェクトを開始
            if (musicNoteLayer != null && coroutineRunner != null)
            {
                if (musicNoteCoroutine != null)
                {
                    coroutineRunner.StopCoroutine(musicNoteCoroutine);
                }
                musicNoteCoroutine = coroutineRunner.StartCoroutine(MusicNoteEffectCoroutine());
            }
            else
            {
                Debug.LogWarning($"[OverlayPresenter_UITK] 音符エフェクトを開始できません: musicNoteLayer={musicNoteLayer != null}, coroutineRunner={coroutineRunner != null}");
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
                
                VisualElement noteElement;
                
                // ♫がフォントに含まれていない場合は、画像として表示
                if (note == "♫")
                {
                    Sprite noteSprite = OverlayAssets.GetMusicNoteSprite(note);
                    if (noteSprite != null)
                    {
                        // 画像として表示
                        var noteImage = new VisualElement();
                        noteImage.style.backgroundImage = new StyleBackground(noteSprite);
                        noteImage.style.width = 24;
                        noteImage.style.height = 24;
                        noteImage.style.position = Position.Absolute;
                        noteImage.style.backgroundColor = Color.clear;
                        noteElement = noteImage;
                    }
                    else
                    {
                        // 画像が見つからない場合は、♪に置き換える
                        note = "♪";
                        var noteLabel = new Label(note);
                        noteLabel.style.fontSize = 24;
                        noteLabel.style.color = new Color(1f, 1f, 0.8f, 1f); // 淡い黄色
                        noteLabel.style.position = Position.Absolute;
                        noteElement = noteLabel;
                    }
                }
                else
                {
                    // ♪はフォントに含まれているため、テキストとして表示
                    var noteLabel = new Label(note);
                    noteLabel.style.fontSize = 24;
                    noteLabel.style.color = new Color(1f, 1f, 0.8f, 1f); // 淡い黄色
                    noteLabel.style.position = Position.Absolute;
                    noteElement = noteLabel;
                }
                
                // 実況者の位置を基準にランダムな位置に配置
                // MusicNoteLayerのサイズを基準に、中央付近にランダムに配置
                float randomX = Random.Range(-40f, 40f); // パーセンテージでのランダムオフセット
                float randomY = Random.Range(-30f, 30f); // パーセンテージでのランダムオフセット
                noteElement.style.left = new Length(50f + randomX, LengthUnit.Percent);
                noteElement.style.bottom = new Length(50f + randomY, LengthUnit.Percent);
                noteElement.pickingMode = PickingMode.Ignore;
                
                musicNoteLayer.Add(noteElement);
                
                // 音符を上に浮かび上がらせるアニメーション
                float elapsed = 0f;
                float startY = randomY;
                
                while (elapsed < noteLifetime)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / noteLifetime;
                    
                    // 上に移動
                    float currentY = startY + (noteSpeed * elapsed);
                    noteElement.style.bottom = new Length(50f + currentY, LengthUnit.Percent);
                    
                    // フェードイン→フェードアウト
                    if (progress < 0.2f)
                    {
                        // フェードイン
                        noteElement.style.opacity = Mathf.Lerp(0f, 1f, progress / 0.2f);
                    }
                    else if (progress > 0.8f)
                    {
                        // フェードアウト
                        noteElement.style.opacity = Mathf.Lerp(1f, 0f, (progress - 0.8f) / 0.2f);
                    }
                    else
                    {
                        noteElement.style.opacity = 1f;
                    }
                    
                    // 少し拡大
                    float scale = 1f + (progress * 0.3f);
                    noteElement.style.scale = new Scale(new Vector3(scale, scale, 1f));
                    
                    yield return null;
                }
                
                // 音符を削除
                musicNoteLayer.Remove(noteElement);
                
                // 次の音符を生成するまで待機
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}

