using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame.Overlay
{
    /// <summary>
    /// オーバーレイをドラッグ可能にするManipulator
    /// </summary>
    public class OverlayDragManipulator : Manipulator
    {
        private Vector2 startMousePosition;
        private Vector2 startOverlayRootPosition;
        private Vector2 startRoomImagePosition;
        private Vector2 startGirlImagePosition;
        private Vector2 startBalloonPosition;
        private Vector2 startThoughtBalloonPosition;
        private bool isDragging = false;
        private VisualElement roomImage;
        private VisualElement girlImage;
        private VisualElement balloonRoot;
        private VisualElement thoughtBalloonRoot;

        public OverlayDragManipulator(VisualElement roomImage, VisualElement girlImage, 
            VisualElement balloonRoot, VisualElement thoughtBalloonRoot)
        {
            this.roomImage = roomImage;
            this.girlImage = girlImage;
            this.balloonRoot = balloonRoot;
            this.thoughtBalloonRoot = thoughtBalloonRoot;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
            target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // Manipulatorが画像要素に追加されているので、このメソッドが呼ばれた時点で
            // 画像要素の上でクリックされたことが確定している
            if (evt.button == 0 && !isDragging) // 左クリックかつドラッグ中でない場合
            {
                isDragging = true;
                // マウス位置はパネル座標系（rootVisualElementを基準）
                startMousePosition = evt.mousePosition;
                
                // OverlayRootの現在位置を取得（rootを基準とした絶対位置）
                var overlayRoot = target;
                // OverlayRootは通常、位置が設定されていない（0,0から開始）
                // 実際の画像要素の位置を基準にする
                startOverlayRootPosition = Vector2.zero; // OverlayRootは常に0,0から開始
                
                // RoomImageとGirlImageの現在位置を取得
                if (roomImage != null)
                {
                    if (roomImage.style.position.value == Position.Absolute && 
                        roomImage.style.left.value.unit == LengthUnit.Pixel)
                    {
                        startRoomImagePosition = new Vector2(
                            roomImage.style.left.value.value,
                            roomImage.style.top.value.value
                        );
                    }
                    else
                    {
                        startRoomImagePosition = new Vector2(
                            roomImage.resolvedStyle.left,
                            roomImage.resolvedStyle.top
                        );
                    }
                }
                
                if (girlImage != null)
                {
                    if (girlImage.style.position.value == Position.Absolute && 
                        girlImage.style.left.value.unit == LengthUnit.Pixel)
                    {
                        startGirlImagePosition = new Vector2(
                            girlImage.style.left.value.value,
                            girlImage.style.top.value.value
                        );
                    }
                    else
                    {
                        startGirlImagePosition = new Vector2(
                            girlImage.resolvedStyle.left,
                            girlImage.resolvedStyle.top
                        );
                    }
                }
                
                // 吹き出しの現在位置を取得
                if (balloonRoot != null)
                {
                    if (balloonRoot.style.position.value == Position.Absolute && 
                        balloonRoot.style.left.value.unit == LengthUnit.Pixel)
                    {
                        startBalloonPosition = new Vector2(
                            balloonRoot.style.left.value.value,
                            balloonRoot.style.top.value.value
                        );
                    }
                    else
                    {
                        startBalloonPosition = new Vector2(
                            balloonRoot.resolvedStyle.left,
                            balloonRoot.resolvedStyle.top
                        );
                    }
                }
                
                if (thoughtBalloonRoot != null)
                {
                    if (thoughtBalloonRoot.style.position.value == Position.Absolute && 
                        thoughtBalloonRoot.style.left.value.unit == LengthUnit.Pixel)
                    {
                        startThoughtBalloonPosition = new Vector2(
                            thoughtBalloonRoot.style.left.value.value,
                            thoughtBalloonRoot.style.top.value.value
                        );
                    }
                    else
                    {
                        startThoughtBalloonPosition = new Vector2(
                            thoughtBalloonRoot.resolvedStyle.left,
                            thoughtBalloonRoot.resolvedStyle.top
                        );
                    }
                }
                
                target.CaptureMouse();
                // ドラッグ開始時のみイベントの伝播を止める
                evt.StopPropagation();
            }
            // ドラッグ開始しない場合は、イベントを伝播させて他のUI要素が反応できるようにする
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (isDragging && target.HasMouseCapture())
            {
                // マウスの移動量を計算（パネル座標系、UI座標系）
                Vector2 delta = evt.mousePosition - startMousePosition;
                
                // 位置を更新（ドロップ時にこの位置が保持される）
                UpdateElementPositions(delta);
                
                // イベントの伝播は止めない（他のUI要素も操作可能にする）
                // evt.StopPropagation(); // コメントアウト
            }
        }
        
        /// <summary>
        /// 要素の位置を更新（ドラッグ中とドロップ後の両方で使用）
        /// </summary>
        private void UpdateElementPositions(Vector2 delta)
        {
            // RoomImageとGirlImageの位置を同じオフセット分だけ更新
            if (roomImage != null)
            {
                Vector2 newRoomImagePosition = startRoomImagePosition + delta;
                roomImage.style.position = Position.Absolute;
                roomImage.style.left = newRoomImagePosition.x;
                roomImage.style.top = newRoomImagePosition.y;
                roomImage.style.right = StyleKeyword.Auto;
                roomImage.style.bottom = StyleKeyword.Auto;
                roomImage.MarkDirtyRepaint();
            }
            
            if (girlImage != null)
            {
                Vector2 newGirlImagePosition = startGirlImagePosition + delta;
                girlImage.style.position = Position.Absolute;
                girlImage.style.left = newGirlImagePosition.x;
                girlImage.style.top = newGirlImagePosition.y;
                girlImage.style.right = StyleKeyword.Auto;
                girlImage.style.bottom = StyleKeyword.Auto;
                girlImage.MarkDirtyRepaint();
            }
            
            // 吹き出しの位置も更新（GirlImageと一緒に移動）
            if (balloonRoot != null)
            {
                Vector2 newBalloonPosition = startBalloonPosition + delta;
                balloonRoot.style.position = Position.Absolute;
                balloonRoot.style.left = newBalloonPosition.x;
                balloonRoot.style.top = newBalloonPosition.y;
                balloonRoot.style.right = StyleKeyword.Auto;
                balloonRoot.style.bottom = StyleKeyword.Auto;
                balloonRoot.MarkDirtyRepaint();
            }
            
            if (thoughtBalloonRoot != null)
            {
                Vector2 newThoughtBalloonPosition = startThoughtBalloonPosition + delta;
                thoughtBalloonRoot.style.position = Position.Absolute;
                thoughtBalloonRoot.style.left = newThoughtBalloonPosition.x;
                thoughtBalloonRoot.style.top = newThoughtBalloonPosition.y;
                thoughtBalloonRoot.style.right = StyleKeyword.Auto;
                thoughtBalloonRoot.style.bottom = StyleKeyword.Auto;
                thoughtBalloonRoot.MarkDirtyRepaint();
            }
            
            target.MarkDirtyRepaint();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0 && isDragging)
            {
                // 最終位置を確定（ドロップ位置を保持）
                if (target.HasMouseCapture())
                {
                    Vector2 delta = evt.mousePosition - startMousePosition;
                    UpdateElementPositions(delta);
                    
                    // 画面外に出ている場合は画面内に戻す
                    ClampToScreenBounds();
                    
                    // 開始位置を更新（次回ドラッグ時の基準位置）
                    if (roomImage != null)
                    {
                        startRoomImagePosition = new Vector2(
                            roomImage.style.left.value.value,
                            roomImage.style.top.value.value
                        );
                    }
                    if (girlImage != null)
                    {
                        startGirlImagePosition = new Vector2(
                            girlImage.style.left.value.value,
                            girlImage.style.top.value.value
                        );
                    }
                    if (balloonRoot != null)
                    {
                        startBalloonPosition = new Vector2(
                            balloonRoot.style.left.value.value,
                            balloonRoot.style.top.value.value
                        );
                    }
                    if (thoughtBalloonRoot != null)
                    {
                        startThoughtBalloonPosition = new Vector2(
                            thoughtBalloonRoot.style.left.value.value,
                            thoughtBalloonRoot.style.top.value.value
                        );
                    }
                }
                
                // ドラッグ状態を終了（マウスキャプチャを解除）
                EndDragging();
                
                // イベントの伝播を止めない（他のUI要素も反応できるように）
                // evt.StopPropagation(); // コメントアウト
            }
        }
        
        /// <summary>
        /// マウスが要素から離れた時（ドラッグ中でもキャプチャを解除）
        /// </summary>
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            if (isDragging)
            {
                // 現在の位置を確定
                if (target.HasMouseCapture())
                {
                    Vector2 delta = evt.mousePosition - startMousePosition;
                    UpdateElementPositions(delta);
                    // 画面外に出ている場合は画面内に戻す
                    ClampToScreenBounds();
                }
                // マウスが離れた場合も位置を確定
                EndDragging();
            }
        }
        
        /// <summary>
        /// マウスキャプチャが失われた時（他の要素がキャプチャした場合など）
        /// </summary>
        private void OnMouseCaptureOut(MouseCaptureOutEvent evt)
        {
            if (isDragging)
            {
                // 現在の位置を確定（マウス位置が取得できない場合は、最後の位置を使用）
                // 画面外に出ている場合は画面内に戻す
                ClampToScreenBounds();
                // キャプチャが失われた場合も位置を確定
                EndDragging();
            }
        }
        
        /// <summary>
        /// 要素の位置を画面内に収める
        /// </summary>
        private void ClampToScreenBounds()
        {
            // 画面のサイズを取得（rootVisualElementのサイズを使用）
            VisualElement rootElement = target?.panel?.visualTree;
            if (rootElement == null) return;
            
            float screenWidth = rootElement.resolvedStyle.width;
            float screenHeight = rootElement.resolvedStyle.height;
            
            // 画面サイズが取得できない場合は、resolvedStyleから取得を試みる
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                screenWidth = rootElement.layout.width;
                screenHeight = rootElement.layout.height;
            }
            
            // デフォルト値（960x540）を使用
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                screenWidth = 960f;
                screenHeight = 540f;
            }
            
            // RoomImageとGirlImageの位置を画面内に収める
            if (roomImage != null)
            {
                float elementWidth = roomImage.resolvedStyle.width > 0 ? roomImage.resolvedStyle.width : 200f;
                float elementHeight = roomImage.resolvedStyle.height > 0 ? roomImage.resolvedStyle.height : 150f;
                
                float currentLeft = roomImage.style.left.value.value;
                float currentTop = roomImage.style.top.value.value;
                
                // 画面内に収まるように位置を調整
                float clampedLeft = Mathf.Clamp(currentLeft, 0f, screenWidth - elementWidth);
                float clampedTop = Mathf.Clamp(currentTop, 0f, screenHeight - elementHeight);
                
                if (currentLeft != clampedLeft || currentTop != clampedTop)
                {
                    roomImage.style.left = clampedLeft;
                    roomImage.style.top = clampedTop;
                    roomImage.MarkDirtyRepaint();
                }
            }
            
            if (girlImage != null)
            {
                float elementWidth = girlImage.resolvedStyle.width > 0 ? girlImage.resolvedStyle.width : 200f;
                float elementHeight = girlImage.resolvedStyle.height > 0 ? girlImage.resolvedStyle.height : 150f;
                
                float currentLeft = girlImage.style.left.value.value;
                float currentTop = girlImage.style.top.value.value;
                
                // 画面内に収まるように位置を調整
                float clampedLeft = Mathf.Clamp(currentLeft, 0f, screenWidth - elementWidth);
                float clampedTop = Mathf.Clamp(currentTop, 0f, screenHeight - elementHeight);
                
                if (currentLeft != clampedLeft || currentTop != clampedTop)
                {
                    girlImage.style.left = clampedLeft;
                    girlImage.style.top = clampedTop;
                    girlImage.MarkDirtyRepaint();
                }
            }
            
            // 吹き出しの位置も調整（画像要素と相対位置を保つ）
            // 吹き出しは画像要素の相対位置に配置されているため、
            // 画像要素の位置が調整されたら自動的に調整される
        }
        
        /// <summary>
        /// ドラッグを終了（マウスキャプチャを解除）
        /// </summary>
        private void EndDragging()
        {
            isDragging = false;
            
            // マウスキャプチャを確実に解除
            if (target.HasMouseCapture())
            {
                target.ReleaseMouse();
            }
            
            // マウスキャプチャが解除されたことを確認
            if (target.HasMouseCapture())
            {
                Debug.LogWarning("[OverlayDragManipulator] マウスキャプチャの解除に失敗しました。");
            }
        }
    }

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
        private OverlayDragManipulator dragManipulator;

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
            
            // OverlayRootの初期位置を設定（ドラッグ可能にするため）
            if (overlayRoot != null)
            {
                overlayRoot.style.position = Position.Absolute;
                // OverlayRootは画面全体をカバーするため、位置は0,0で開始
                // 実際の画像要素の位置は子要素で設定
            }
            
            // 画像要素の強制サイズ設定（UXMLの設定が反映されていない場合）
            // 背景とキャラを同じ位置に重ねて配置（右下に相対配置）
            float overlayRight = 20;  // 右端からの距離
            float overlayBottom = 20;  // 下端からの距離
            
            if (roomImage != null)
            {
                if (roomImage.style.width.value.value == 0 || roomImage.style.height.value.value == 0)
                {
                    roomImage.style.width = 200;
                    roomImage.style.height = 150;
                }
                roomImage.style.position = Position.Absolute;
                roomImage.style.right = overlayRight;
                roomImage.style.bottom = overlayBottom;
                roomImage.style.left = StyleKeyword.Auto;
                roomImage.style.top = StyleKeyword.Auto;
            }
            
            if (girlImage != null)
            {
                if (girlImage.style.width.value.value == 0 || girlImage.style.height.value.value == 0)
                {
                    girlImage.style.width = 200;
                    girlImage.style.height = 150;
                }
                girlImage.style.position = Position.Absolute;
                girlImage.style.right = overlayRight;  // 背景と同じ右端からの距離
                girlImage.style.bottom = overlayBottom;   // 背景と同じ下端からの距離
                girlImage.style.left = StyleKeyword.Auto;
                girlImage.style.top = StyleKeyword.Auto;
                
                // GirlImageをRoomImageの後に配置して上に表示
                if (overlayRoot != null && roomImage != null && girlImage.parent == overlayRoot)
                {
                    overlayRoot.Remove(girlImage);
                    overlayRoot.Add(girlImage);
                }
            }
            
            // オーバーレイ要素のpickingMode設定
            // OverlayRootはIgnoreのまま（他のUI要素へのイベントをブロックしない）
            if (overlayRoot != null)
            {
                overlayRoot.pickingMode = PickingMode.Ignore;
            }
            
            // 画像要素のみドラッグ可能にする
            // RoomImageとGirlImageの両方にManipulatorを追加（どちらをクリックしてもドラッグ可能）
            if (roomImage != null || girlImage != null)
            {
                // ドラッグManipulatorを作成（RoomImage、GirlImage、吹き出しも一緒に移動）
                dragManipulator = new OverlayDragManipulator(roomImage, girlImage, balloonRoot, thoughtBalloonRoot);
                
                // RoomImageに追加
                if (roomImage != null)
                {
                    roomImage.pickingMode = PickingMode.Position; // ドラッグ可能にする
                    roomImage.AddManipulator(dragManipulator);
                }
                
                // GirlImageにも追加（別のManipulatorインスタンスを作成）
                if (girlImage != null)
                {
                    girlImage.pickingMode = PickingMode.Position; // ドラッグ可能にする
                    // 同じManipulatorインスタンスは複数の要素に追加できないため、新しいインスタンスを作成
                    var girlDragManipulator = new OverlayDragManipulator(roomImage, girlImage, balloonRoot, thoughtBalloonRoot);
                    girlImage.AddManipulator(girlDragManipulator);
                }
            }
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
            // GirlImageは右下（right: 20, bottom: 20）に配置
            // 吹き出しはGirlImageの左側に配置（相対配置）
            float balloonRight = 240; // GirlImageの右端（right: 20）からさらに左に220px（画像幅200px + 余白20px）
            float balloonBottom = 20;   // GirlImageと同じ下端からの距離
            
            if (balloonRoot != null)
            {
                balloonRoot.style.position = Position.Absolute;
                balloonRoot.style.right = balloonRight;
                balloonRoot.style.bottom = balloonBottom;
                balloonRoot.style.left = StyleKeyword.Auto;  // 古いleft設定をクリア
                balloonRoot.style.top = StyleKeyword.Auto; // 古いtop設定をクリア
                balloonRoot.style.width = 280; // 固定幅
                balloonRoot.style.maxWidth = 300;
                balloonRoot.style.minWidth = 200;
            }
            
            if (thoughtBalloonRoot != null)
            {
                thoughtBalloonRoot.style.position = Position.Absolute;
                thoughtBalloonRoot.style.right = balloonRight;
                thoughtBalloonRoot.style.bottom = balloonBottom;
                thoughtBalloonRoot.style.left = StyleKeyword.Auto;  // 古いleft設定をクリア
                thoughtBalloonRoot.style.top = StyleKeyword.Auto; // 古いtop設定をクリア
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
                // 背景を下に、キャラを上に表示するため、両方とも同じ座標を使用（右下に相対配置）
                float overlayRight = 20;  // 右端からの距離
                float overlayBottom = 20;  // 下端からの距離
                
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
                    roomImage.style.right = overlayRight;
                    roomImage.style.bottom = overlayBottom;
                    roomImage.style.left = StyleKeyword.Auto;  // 古いleft/topをクリア
                    roomImage.style.top = StyleKeyword.Auto;
                    
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
                    girlImage.style.right = overlayRight;  // 背景と同じ右端からの距離
                    girlImage.style.bottom = overlayBottom;   // 背景と同じ下端からの距離
                    girlImage.style.left = StyleKeyword.Auto;  // 古いleft/topをクリア
                    girlImage.style.top = StyleKeyword.Auto;
                    
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
                float girlRight = 20f; // デフォルト値（右端からの距離）
                float girlBottom = 20f;  // デフォルト値（下端からの距離）
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
                // right/bottomから位置を取得
                if (girlImage.resolvedStyle.right >= 0)
                {
                    girlRight = girlImage.resolvedStyle.right;
                }
                else if (girlImage.style.right.value.unit == LengthUnit.Pixel)
                {
                    girlRight = girlImage.style.right.value.value;
                }
                if (girlImage.resolvedStyle.bottom >= 0)
                {
                    girlBottom = girlImage.resolvedStyle.bottom;
                }
                else if (girlImage.style.bottom.value.unit == LengthUnit.Pixel)
                {
                    girlBottom = girlImage.style.bottom.value.value;
                }
                
                // musicNoteLayerをgirlImageと同じ位置・サイズに設定（相対配置）
                musicNoteLayer.style.position = Position.Absolute;
                musicNoteLayer.style.right = girlRight;
                musicNoteLayer.style.bottom = girlBottom;
                musicNoteLayer.style.left = StyleKeyword.Auto;
                musicNoteLayer.style.top = StyleKeyword.Auto;
                musicNoteLayer.style.width = girlWidth;
                musicNoteLayer.style.height = girlHeight;
                musicNoteLayer.style.display = DisplayStyle.Flex;
                musicNoteLayer.style.visibility = Visibility.Visible;
                musicNoteLayer.style.overflow = Overflow.Visible;
                
                Debug.Log($"[OverlayPresenter_UITK] MusicNoteLayerを設定: right={girlRight}, bottom={girlBottom}, width={girlWidth}, height={girlHeight}");
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

