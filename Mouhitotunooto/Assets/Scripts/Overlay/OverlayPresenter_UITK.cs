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
            
            // デバッグ: 要素が正しく取得できているか確認
            if (overlayRoot == null)
            {
                Debug.LogError("[OverlayPresenter_UITK] OverlayRootが見つかりません。UXMLが正しく読み込まれているか確認してください。");
            }
            else
            {
                Debug.Log($"[OverlayPresenter_UITK] OverlayRootを正常に取得しました。初期表示状態: {overlayRoot.style.display.value}");
                Debug.Log($"[OverlayPresenter_UITK] OverlayRoot位置とサイズ: left={overlayRoot.style.left.value}, top={overlayRoot.style.top.value}, width={overlayRoot.style.width.value}, height={overlayRoot.style.height.value}");
                Debug.Log($"[OverlayPresenter_UITK] OverlayRoot解決サイズ: {overlayRoot.resolvedStyle.width}x{overlayRoot.resolvedStyle.height}");
            }
            
            // 画像要素のデバッグ情報と強制サイズ設定
            if (girlImage != null)
            {
                Debug.Log($"[OverlayPresenter_UITK] GirlImage位置とサイズ: right={girlImage.style.right.value}, bottom={girlImage.style.bottom.value}, width={girlImage.style.width.value}, height={girlImage.style.height.value}");
                
                // 強制的にサイズを設定（UXMLの設定が反映されていない場合）
                if (girlImage.style.width.value.value == 0 || girlImage.style.height.value.value == 0)
                {
                    girlImage.style.width = 200;
                    girlImage.style.height = 150;
                    girlImage.style.position = Position.Absolute;
                    girlImage.style.right = 20;
                    girlImage.style.bottom = 20;
                    Debug.Log("[OverlayPresenter_UITK] GirlImageのサイズを強制設定しました: 200x150px");
                }
            }
            if (roomImage != null)
            {
                Debug.Log($"[OverlayPresenter_UITK] RoomImage位置とサイズ: right={roomImage.style.right.value}, bottom={roomImage.style.bottom.value}, width={roomImage.style.width.value}, height={roomImage.style.height.value}");
                
                // 強制的にサイズを設定（UXMLの設定が反映されていない場合）
                if (roomImage.style.width.value.value == 0 || roomImage.style.height.value.value == 0)
                {
                    roomImage.style.width = 200;
                    roomImage.style.height = 150;
                    roomImage.style.position = Position.Absolute;
                    roomImage.style.right = 20;
                    roomImage.style.bottom = 20;
                    Debug.Log("[OverlayPresenter_UITK] RoomImageのサイズを強制設定しました: 200x150px");
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
            if (girlImage == null)
            {
                Debug.LogWarning("[OverlayPresenter_UITK] SetExpression: girlImageがnullです。");
                return;
            }

            Sprite sprite = OverlayAssets.GetExpressionSprite(expression);
            if (sprite != null)
            {
                girlImage.style.backgroundImage = new StyleBackground(sprite);
                Debug.Log($"[OverlayPresenter_UITK] 表情を設定しました: {expression}");
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
                Debug.Log($"[OverlayPresenter_UITK] 部屋状態を設定しました: {roomState}");
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
            if (overlayRoot == null)
            {
                Debug.LogWarning("[OverlayPresenter_UITK] SetVisible: overlayRootがnullです。");
                return;
            }

            overlayRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log($"[OverlayPresenter_UITK] SetVisible: {visible} (display = {overlayRoot.style.display.value})");
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
                // テスト用背景色を削除し、透明背景に設定
                if (girlImage != null)
                {
                    girlImage.style.backgroundColor = Color.clear; // 透明背景
                    Debug.Log($"[OverlayPresenter_UITK] GirlImageの背景を透明に設定しました。");
                }
                if (roomImage != null)
                {
                    roomImage.style.backgroundColor = Color.clear; // 透明背景
                    Debug.Log($"[OverlayPresenter_UITK] RoomImageの背景を透明に設定しました。");
                }
                
                // デフォルトの表情と部屋状態を設定
                // Phaseが更新された時は、常に画像を設定する（表示を確実にするため）
                if (girlImage != null)
                {
                    var bgImage = girlImage.style.backgroundImage.value;
                    bool hasImage = bgImage != null && bgImage.texture != null;
                    if (!hasImage)
                    {
                        Debug.Log("[OverlayPresenter_UITK] デフォルトの表情を設定します: Neutral");
                        SetExpression(GirlExpression.Neutral);
                    }
                    else
                    {
                        Debug.Log($"[OverlayPresenter_UITK] 表情は既に設定されています: {bgImage.texture.name}");
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
                        Debug.Log("[OverlayPresenter_UITK] デフォルトの部屋状態を設定します: CleanDay");
                        SetRoomState(RoomState.CleanDay);
                    }
                    else
                    {
                        Debug.Log($"[OverlayPresenter_UITK] 部屋背景は既に設定されています: {bgImage.texture.name}");
                    }
                }
                else
                {
                    Debug.LogWarning("[OverlayPresenter_UITK] roomImageがnullです。");
                }
                
                // 座標系解明結果を適用した座標設定
                if (girlImage != null && (girlImage.resolvedStyle.width == 0 || girlImage.resolvedStyle.height == 0))
                {
                    Debug.LogWarning("[OverlayPresenter_UITK] GirlImageのサイズが0x0です。座標系解明結果でサイズと座標を設定します。");
                    girlImage.style.position = Position.Absolute;
                    girlImage.style.left = 1320;     // 座標系解明結果：完璧な右下配置
                    girlImage.style.top = 700;       // 座標系解明結果：完璧な高さ
                    girlImage.style.right = StyleKeyword.Auto;  // 古いright/bottomをクリア
                    girlImage.style.bottom = StyleKeyword.Auto;
                    girlImage.style.width = 200;
                    girlImage.style.height = 150;
                    girlImage.style.minWidth = 200;
                    girlImage.style.minHeight = 150;
                    girlImage.MarkDirtyRepaint();
                    Debug.Log("[OverlayPresenter_UITK] ✅ GirlImageに座標系解明結果適用: left=1320, top=700, 200x150px");
                }
                else if (girlImage != null)
                {
                    // サイズは正常だが、座標系解明結果を適用
                    girlImage.style.position = Position.Absolute;
                    girlImage.style.left = 1320;     // 座標系解明結果：完璧な右下配置
                    girlImage.style.top = 700;       // 座標系解明結果：完璧な高さ
                    girlImage.style.right = StyleKeyword.Auto;  // 古いright/bottomをクリア
                    girlImage.style.bottom = StyleKeyword.Auto;
                    Debug.Log($"[OverlayPresenter_UITK] GirlImageに座標系解明結果適用: left=1320, top=700 (サイズは既に正常: {girlImage.resolvedStyle.width}x{girlImage.resolvedStyle.height})");
                }
                
                if (roomImage != null && (roomImage.resolvedStyle.width == 0 || roomImage.resolvedStyle.height == 0))
                {
                    Debug.LogWarning("[OverlayPresenter_UITK] RoomImageのサイズが0x0です。座標系解明結果でサイズと座標を設定します。");
                    roomImage.style.position = Position.Absolute;
                    roomImage.style.left = 1320;     // 座標系解明結果：完璧な右下配置
                    roomImage.style.top = 700;       // 座標系解明結果：GirlImageの上
                    roomImage.style.right = StyleKeyword.Auto;  // 古いright/bottomをクリア
                    roomImage.style.bottom = StyleKeyword.Auto;
                    roomImage.style.width = 200;
                    roomImage.style.height = 150;
                    roomImage.style.minWidth = 200;
                    roomImage.style.minHeight = 150;
                    roomImage.MarkDirtyRepaint();
                    Debug.Log("[OverlayPresenter_UITK] ✅ RoomImageに座標系解明結果適用: left=1320, top=530, 200x150px");
                }
                else if (roomImage != null)
                {
                    // サイズは正常だが、座標系解明結果を適用
                    roomImage.style.position = Position.Absolute;
                    roomImage.style.left = 1320;     // 座標系解明結果：完璧な右下配置
                    roomImage.style.top = 700;       // 座標系解明結果：GirlImageの上
                    roomImage.style.right = StyleKeyword.Auto;  // 古いright/bottomをクリア
                    roomImage.style.bottom = StyleKeyword.Auto;
                    Debug.Log($"[OverlayPresenter_UITK] RoomImageに座標系解明結果適用: left=1320, top=530 (サイズは既に正常: {roomImage.resolvedStyle.width}x{roomImage.resolvedStyle.height})");
                }
                
                // 最終確認：実際の要素サイズと座標をログ出力
                if (girlImage != null)
                {
                    Debug.Log($"[OverlayPresenter_UITK] 🟣 最終確認 GirlImage: サイズ={girlImage.resolvedStyle.width}x{girlImage.resolvedStyle.height}, 座標=left:{girlImage.style.left.value.value}, top:{girlImage.style.top.value.value}");
                }
                if (roomImage != null)
                {
                    Debug.Log($"[OverlayPresenter_UITK] 🟢 最終確認 RoomImage: サイズ={roomImage.resolvedStyle.width}x{roomImage.resolvedStyle.height}, 座標=left:{roomImage.style.left.value.value}, top:{roomImage.style.top.value.value}");
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
                float randomX = Random.Range(-30f, 30f);
                float randomY = Random.Range(-20f, 20f);
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

