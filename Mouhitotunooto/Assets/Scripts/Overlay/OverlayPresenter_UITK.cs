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
        private readonly MonoBehaviour coroutineRunner;

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
            
            // すべてのオーバーレイ要素にpickingModeをIgnoreに設定（イベントを無視するため）
            if (overlayRoot != null) overlayRoot.pickingMode = PickingMode.Ignore;
            if (roomImage != null) roomImage.pickingMode = PickingMode.Ignore;
            if (girlImage != null) girlImage.pickingMode = PickingMode.Ignore;
            if (balloonRoot != null) balloonRoot.pickingMode = PickingMode.Ignore;
            if (balloonLabel != null) balloonLabel.pickingMode = PickingMode.Ignore;
            if (thoughtBalloonRoot != null) thoughtBalloonRoot.pickingMode = PickingMode.Ignore;
            if (thoughtBalloonLabel != null) thoughtBalloonLabel.pickingMode = PickingMode.Ignore;
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
    }
}

