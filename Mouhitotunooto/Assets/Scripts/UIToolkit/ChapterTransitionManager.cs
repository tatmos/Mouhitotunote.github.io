using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// Chapter遷移演出を管理するクラス
    /// </summary>
    public class ChapterTransitionManager : MonoBehaviour
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
        /// Chapter C（3周目）への移行演出を表示
        /// </summary>
        public IEnumerator ShowChapterCTransition(int score, UIDocument titleScreenDocument)
        {
            audioManager.FadeOutAudioOnSceneChange();
            audioManager.FadeOutAmbientSoundForResult();
            
            // 雷のような特別な音を再生
            audioManager.PlayThunderSound();

            hideAllScreens();

            // タイトル画面のルート要素を取得
            var root = titleScreenDocument.rootVisualElement;
            if (root == null) yield break;

            // 全画面を覆うオーバーレイを作成
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = Color.black;
            overlay.style.opacity = 0;
            root.Add(overlay);

            // フェードイン（2秒）
            float fadeInDuration = 2.0f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1.0f, elapsed / fadeInDuration);
                overlay.style.opacity = alpha;
                yield return null;
            }

            // カットシーン用のラベルを作成
            var cutsceneLabel = new Label();
            cutsceneLabel.style.position = Position.Absolute;
            cutsceneLabel.style.left = 0;
            cutsceneLabel.style.right = 0;
            cutsceneLabel.style.top = Length.Percent(40);
            cutsceneLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            cutsceneLabel.style.fontSize = 36;
            cutsceneLabel.style.color = Color.white;
            cutsceneLabel.style.opacity = 0;
            overlay.Add(cutsceneLabel);

            // カットシーンテキストを生成
            string transitionText = "";
            if (score >= 7)
            {
                transitionText = "真実の扉で不正を判定されました。\n\n修正プログラムを起動します。\n\n世界が歪み始める...\n\nすべての文字が失われていく...\n\nそして、3周目が始まる。";
            }
            else
            {
                transitionText = "真実の扉を開いた。\n\nしかし、何かがおかしい。\n\n世界が歪み始める...\n\nすべての文字が失われていく...\n\nそして、3周目が始まる。";
            }

            // ラベルをフェードイン
            elapsed = 0f;
            fadeInDuration = 1.0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1.0f, elapsed / fadeInDuration);
                cutsceneLabel.style.opacity = alpha;
                yield return null;
            }

            // タイプライター効果でテキストを表示
            bool isComplete = false;
            if (typewriterEffectManager != null)
            {
                typewriterEffectManager.StartTypewriterEffect(
                    cutsceneLabel,
                    transitionText,
                    () => { isComplete = true; }
                );
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
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1.0f, 0f, elapsed / fadeDuration);
                cutsceneLabel.style.opacity = alpha;
                yield return null;
            }

            root.Remove(overlay);

            // 3周目開始（Chapter C移行）
            gameManager.TriggerThirdLoop();
            
            // タイトル画面へ（すでに暗い画面なので、フェード演出を介さず直接呼ぶ）
            showTitleScreen();
        }

        /// <summary>
        /// 3周目への移行カットシーンを外部から開始するためのメソッド
        /// </summary>
        public void TriggerThirdLoopCutscene(UIDocument titleScreenDocument)
        {
            StartCoroutine(ShowThirdLoopCutscene(titleScreenDocument));
        }

        /// <summary>
        /// 3周目への移行カットシーンを表示
        /// </summary>
        private IEnumerator ShowThirdLoopCutscene(UIDocument titleScreenDocument)
        {
            audioManager.FadeOutAudioOnSceneChange();
            audioManager.FadeOutAmbientSoundForResult();
            
            // 雷のような特別な音を再生
            audioManager.PlayThunderSound();

            hideAllScreens();

            // タイトル画面のルート要素を取得
            var root = titleScreenDocument.rootVisualElement;
            if (root == null) yield break;

            // 全画面を覆うオーバーレイを作成
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = Color.black;
            overlay.style.opacity = 0;
            root.Add(overlay);

            // フェードイン（2秒）
            float fadeInDuration = 2.0f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1.0f, elapsed / fadeInDuration);
                overlay.style.opacity = alpha;
                yield return null;
            }

            // カットシーン用のラベルを作成
            var cutsceneLabel = new Label();
            cutsceneLabel.style.position = Position.Absolute;
            cutsceneLabel.style.left = 0;
            cutsceneLabel.style.right = 0;
            cutsceneLabel.style.top = Length.Percent(40);
            cutsceneLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            cutsceneLabel.style.fontSize = 36;
            cutsceneLabel.style.color = Color.white;
            cutsceneLabel.style.opacity = 0;
            overlay.Add(cutsceneLabel);

            // カットシーンテキスト
            string transitionText = "世界が歪み始める...\n\nすべての文字が失われていく...\n\nそして、3周目が始まる。";

            // ラベルをフェードイン
            elapsed = 0f;
            fadeInDuration = 1.0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1.0f, elapsed / fadeInDuration);
                cutsceneLabel.style.opacity = alpha;
                yield return null;
            }

            // タイプライター効果でテキストを表示
            bool isComplete = false;
            if (typewriterEffectManager != null)
            {
                typewriterEffectManager.StartTypewriterEffect(
                    cutsceneLabel,
                    transitionText,
                    () => { isComplete = true; }
                );
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
            elapsed = 0f;
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
            
            // タイトル画面へ
            showTitleScreen();
        }

        /// <summary>
        /// 暗転演出を伴うChapterジャンプを実行
        /// </summary>
        public IEnumerator PerformChapterJump(string chapterId, VisualElement root, System.Action showSelectionScreen)
        {
            // 暗転オーバーレイを作成
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = Color.black;
            overlay.style.opacity = 0;
            root.Add(overlay);

            // フェードイン
            float fadeDuration = 0.5f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1.0f, elapsed / fadeDuration);
                overlay.style.opacity = alpha;
                yield return null;
            }

            // Chapterジャンプを実行
            if (ChapterManager.Instance != null)
            {
                ChapterManager.Instance.JumpToChapter(chapterId);
            }

            yield return new WaitForSeconds(0.5f);

            // フェードアウト
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1.0f, 0f, elapsed / fadeDuration);
                overlay.style.opacity = alpha;
                yield return null;
            }

            root.Remove(overlay);

            // 選択画面を表示
            showSelectionScreen();
        }

        /// <summary>
        /// 直接Chapterジャンプを実行（演出なし）
        /// </summary>
        public void JumpToChapterDirectly(string chapterId)
        {
            if (ChapterManager.Instance != null)
            {
                ChapterManager.Instance.JumpToChapter(chapterId);
            }
        }
    }
}

