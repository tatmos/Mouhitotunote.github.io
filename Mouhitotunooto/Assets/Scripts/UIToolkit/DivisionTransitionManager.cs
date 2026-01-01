using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// Division遷移演出を管理するクラス
    /// </summary>
    public class DivisionTransitionManager : MonoBehaviour
    {
        private GameManager gameManager;
        private AudioManager audioManager;
        private TypewriterEffectManager typewriterEffectManager;
        private System.Action showTitleScreen;
        private System.Action hideAllScreens;

        public void Initialize(GameManager gameManager, AudioManager audioManager, TypewriterEffectManager typewriterEffectManager, System.Action showTitleScreen, System.Action hideAllScreens)
        {
            this.gameManager = gameManager;
            this.audioManager = audioManager;
            this.typewriterEffectManager = typewriterEffectManager;
            this.showTitleScreen = showTitleScreen;
            this.hideAllScreens = hideAllScreens;
        }

        /// <summary>
        /// Division C（3周目）への移行演出を表示
        /// </summary>
        public IEnumerator ShowDivisionCTransition(int score, UIDocument titleScreenDocument)
        {
            if (audioManager != null)
            {
                audioManager.FadeOutAudioOnSceneChange();
                audioManager.FadeOutAmbientSoundForResult();
                audioManager.PlayThunderSound();
            }

            hideAllScreens?.Invoke();

            // 演出用の真っ黒なオーバーレイを作成
            if (titleScreenDocument == null)
            {
                Debug.LogError("TitleScreenDocumentがアサインされていません！演出をスキップします。");
                gameManager.TriggerThirdLoop();
                showTitleScreen?.Invoke();
                yield break;
            }

            // タイトル画面をアクティブにして、rootを取得できるようにする
            titleScreenDocument.gameObject.SetActive(true);
            var root = titleScreenDocument.rootVisualElement;
            
            if (root == null)
            {
                Debug.LogError("rootVisualElementが取得できません！演出をスキップします。");
                gameManager.TriggerThirdLoop();
                showTitleScreen?.Invoke();
                yield break;
            }

            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = Color.black;
            overlay.style.justifyContent = Justify.Center;
            overlay.style.alignItems = Align.Center;
            root.Add(overlay);

            // テキスト表示用のラベル
            var cutsceneLabel = new Label("");
            cutsceneLabel.style.fontSize = 32;
            cutsceneLabel.style.color = Color.white;
            cutsceneLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            cutsceneLabel.style.whiteSpace = WhiteSpace.Normal;
            cutsceneLabel.style.width = Length.Percent(80);
            overlay.Add(cutsceneLabel);

            yield return new WaitForSeconds(1.5f);

            // 表示するテキストを構築
            string transitionText = "不正なデータが修正されました。システムを強制再起動します";
            
            // ダークモード：失われた文字を置換
            var lostLetters = gameManager.GetLostLetters();
            if (lostLetters.Count > 0)
            {
                foreach (char lostLetter in lostLetters)
                {
                    transitionText = transitionText.Replace(lostLetter.ToString(), "※");
                }
            }

            // タイプライター表示
            bool isComplete = false;
            if (typewriterEffectManager != null)
            {
                typewriterEffectManager.StartTypewriterEffect(cutsceneLabel, transitionText, () => isComplete = true);
            }
            else
            {
                cutsceneLabel.text = transitionText;
                isComplete = true;
            }

            while (!isComplete) yield return null;

            yield return new WaitForSeconds(3.0f);

            // 長めのフェードアウト（5秒）
            float fadeDuration = 5.0f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1.0f, 0f, elapsed / fadeDuration);
                cutsceneLabel.style.opacity = alpha;
                yield return null;
            }

            root.Remove(overlay);

            // 3周目開始（Division C移行）
            gameManager.TriggerThirdLoop();
            
            // タイトル画面へ（すでに暗い画面なので、フェード演出を介さず直接呼ぶ）
            showTitleScreen?.Invoke();
        }

        /// <summary>
        /// 3周目への移行カットシーンを表示
        /// </summary>
        public IEnumerator ShowThirdLoopCutscene(UIDocument titleScreenDocument)
        {
            if (audioManager != null)
            {
                audioManager.FadeOutAudioOnSceneChange();
                audioManager.FadeOutAmbientSoundForResult();
                audioManager.PlayThunderSound();
            }

            hideAllScreens?.Invoke();

            // 演出用の真っ黒なオーバーレイを作成
            if (titleScreenDocument == null)
            {
                Debug.LogError("TitleScreenDocumentがアサインされていません！カットシーンをスキップします。");
                gameManager.TriggerThirdLoop();
                showTitleScreen?.Invoke();
                yield break;
            }

            // タイトル画面をアクティブにして、rootを取得できるようにする
            titleScreenDocument.gameObject.SetActive(true);
            var root = titleScreenDocument.rootVisualElement;
            
            if (root == null)
            {
                Debug.LogError("rootVisualElementが取得できません！カットシーンをスキップします。");
                gameManager.TriggerThirdLoop();
                showTitleScreen?.Invoke();
                yield break;
            }

            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = Color.black;
            overlay.style.justifyContent = Justify.Center;
            overlay.style.alignItems = Align.Center;
            root.Add(overlay);

            // テキスト表示用のラベル
            var cutsceneLabel = new Label("");
            cutsceneLabel.style.fontSize = 32;
            cutsceneLabel.style.color = Color.white;
            cutsceneLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            cutsceneLabel.style.whiteSpace = WhiteSpace.Normal;
            cutsceneLabel.style.width = Length.Percent(80);
            overlay.Add(cutsceneLabel);

            yield return new WaitForSeconds(1.5f);

            // タイプライター表示
            bool isComplete = false;
            if (typewriterEffectManager != null)
            {
                typewriterEffectManager.StartTypewriterEffect(cutsceneLabel, "あなたは「※※※※※」を探す使命があります。", () => isComplete = true);
            }
            else
            {
                cutsceneLabel.text = "あなたは「※※※※※」を探す使命があります。";
                isComplete = true;
            }

            while (!isComplete) yield return null;

            yield return new WaitForSeconds(3.0f);

            // フェードアウト
            float fadeDuration = 2.0f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1.0f, 0f, elapsed / fadeDuration);
                cutsceneLabel.style.opacity = alpha;
                yield return null;
            }

            root.Remove(overlay);

            // 3周目開始
            gameManager.TriggerThirdLoop();
            
            // タイトル画面へ（すでに暗い画面なので、フェード演出を介さず直接呼ぶ）
            showTitleScreen?.Invoke();
        }

        /// <summary>
        /// 暗転演出を伴うDivisionジャンプを実行
        /// </summary>
        public IEnumerator PerformDivisionJump(string divisionId, VisualElement root, System.Action showSelectionScreen)
        {
            if (root == null)
            {
                if (DivisionManager.Instance != null)
                {
                    DivisionManager.Instance.JumpToDivision(divisionId);
                }
                showSelectionScreen?.Invoke();
                yield break;
            }

            // 暗転オーバーレイ
            var blackOverlay = new VisualElement();
            blackOverlay.style.position = Position.Absolute;
            blackOverlay.style.left = 0;
            blackOverlay.style.top = 0;
            blackOverlay.style.right = 0;
            blackOverlay.style.bottom = 0;
            blackOverlay.style.backgroundColor = Color.black;
            blackOverlay.style.opacity = 0;
            root.Add(blackOverlay);

            // 音を鳴らす
            if (audioManager != null)
            {
                audioManager.PlayThunderSound();
            }

            // フェードイン
            float duration = 1.0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                blackOverlay.style.opacity = Mathf.Min(elapsed / duration, 1.0f);
                yield return null;
            }

            // ジャンプ処理
            if (DivisionManager.Instance != null)
            {
                DivisionManager.Instance.JumpToDivision(divisionId);
            }
            
            // 画面遷移
            showSelectionScreen?.Invoke();
            
            // 新しい画面が表示されるまで待つ
            yield return null;
        }
    }
}

