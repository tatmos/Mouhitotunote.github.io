using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// カウントダウン機能を管理するクラス
    /// </summary>
    public class CountdownManager : MonoBehaviour
    {
        private Coroutine countdownCoroutine = null;
        private float countdownTime = 10f; // カウントダウン時間（秒）
        private bool wordFound = false; // ワードが見つかったかどうか

        /// <summary>
        /// カウントダウンを開始
        /// </summary>
        public void StartCountdown(
            Label countdownText,
            VisualElement countdownContainer,
            VisualElement wordGetContainer,
            Label wordFailedMessageLabel,
            System.Action onWordFound,
            System.Action onCountdownComplete,
            System.Action onShowBackButton)
        {
            // フラグをリセット
            wordFound = false;

            // カウントダウンコンテナを表示
            if (countdownContainer != null)
            {
                countdownContainer.style.display = DisplayStyle.Flex;
            }

            // カウントダウンを開始
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            countdownCoroutine = StartCoroutine(CountdownCoroutine(
                countdownText,
                countdownContainer,
                wordGetContainer,
                wordFailedMessageLabel,
                onWordFound,
                onCountdownComplete,
                onShowBackButton));
        }

        /// <summary>
        /// カウントダウンコルーチン
        /// </summary>
        private IEnumerator CountdownCoroutine(
            Label countdownText,
            VisualElement countdownContainer,
            VisualElement wordGetContainer,
            Label wordFailedMessageLabel,
            System.Action onWordFound,
            System.Action onCountdownComplete,
            System.Action onShowBackButton)
        {
            float remainingTime = countdownTime;

            while (remainingTime > 0 && !wordFound)
            {
                if (countdownText != null)
                {
                    int displayTime = Mathf.CeilToInt(remainingTime);
                    countdownText.text = displayTime.ToString();
                }

                remainingTime -= Time.deltaTime;
                yield return null;
            }

            // カウントダウンコンテナを非表示にする
            if (countdownContainer != null)
            {
                countdownContainer.style.display = DisplayStyle.None;
            }

            // カウントダウンが終了した場合の処理
            if (!wordFound)
            {
                // 失敗メッセージを表示
                if (wordFailedMessageLabel != null)
                {
                    wordFailedMessageLabel.style.display = DisplayStyle.Flex;
                }
            }
            else
            {
                // ワードが見つかった場合のコールバック
                onWordFound?.Invoke();
            }

            // ワードゲット表示を表示
            if (wordGetContainer != null)
            {
                wordGetContainer.style.display = DisplayStyle.Flex;
            }

            // カウントダウン完了コールバック
            onCountdownComplete?.Invoke();

            // 戻るボタンを表示
            onShowBackButton?.Invoke();

            countdownCoroutine = null;
        }

        /// <summary>
        /// ワードが見つかったことを通知
        /// </summary>
        public void NotifyWordFound()
        {
            wordFound = true;
        }

        /// <summary>
        /// カウントダウンを停止
        /// </summary>
        public void StopCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
        }

        /// <summary>
        /// カウントダウン時間を設定
        /// </summary>
        public void SetCountdownTime(float time)
        {
            countdownTime = time;
        }
    }
}

