using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// UI ToolkitベースのUIManager
    /// </summary>
    public class UIManagerUIToolkit : MonoBehaviour
    {
        public static UIManagerUIToolkit Instance { get; private set; }

        [Header("UI Documents")]
        [SerializeField] private UIDocument titleScreenDocument;
        [SerializeField] private UIDocument selectionScreenDocument;
        [SerializeField] private UIDocument scenarioScreenDocument;
        [SerializeField] private UIDocument resultScreenDocument;
        [SerializeField] private UIDocument profileScreenDocument;
        [SerializeField] private UIDocument creditsScreenDocument;
        [SerializeField] private UIDocument achievementsScreenDocument;
        [SerializeField] private UIDocument mouhitotsuScreenDocument;

        [Header("UXML Files")]
        [SerializeField] private VisualTreeAsset selectionScreenUXML;
        [SerializeField] private VisualTreeAsset scenarioScreenUXML;
        [SerializeField] private VisualTreeAsset resultScreenUXML;
        [SerializeField] private VisualTreeAsset profileScreenUXML;
        [SerializeField] private VisualTreeAsset creditsScreenUXML;
        [SerializeField] private VisualTreeAsset achievementsScreenUXML;
        [SerializeField] private VisualTreeAsset mouhitotsuScreenUXML;
        [SerializeField] private VisualTreeAsset soundSettingsPanelUXML;

        [Header("Background Images")]
        [SerializeField] private Sprite[] scenarioBackgrounds = new Sprite[6];
        [SerializeField] private Sprite selectionScreenBackground;
        [SerializeField] private Sprite profileScreenBackground;
        
        [Header("Scenario Button Images")]
        [Tooltip("クリア前のシナリオボタン画像（9-slice対応）。Unityエディタで画像を選択し、Sprite Editorで9-sliceを設定してください。")]
        [SerializeField] private Sprite scenarioButtonNormalImage; // クリア前のシナリオボタン画像（9-slice対応）
        [Tooltip("クリア後のシナリオボタン画像（9-slice対応）。Unityエディタで画像を選択し、Sprite Editorで9-sliceを設定してください。")]
        [SerializeField] private Sprite scenarioButtonCompletedImage; // クリア後のシナリオボタン画像（9-slice対応）
        
        [Header("UI Button Images")]
        [Tooltip("通常のUIボタン画像（9-slice対応）。選択肢ボタン、スタートボタン、戻るボタンなどに使用されます。")]
        [SerializeField] private Sprite uiButtonNormalImage; // 通常のUIボタン画像（9-slice対応）
        [Tooltip("ダークモード用のUIボタン画像（9-slice対応）。ダークモード時の選択肢ボタンに使用されます。")]
        [SerializeField] private Sprite uiButtonDarkImage; // ダークモード用のUIボタン画像（9-slice対応）
        [Tooltip("インディゴ系のUIボタン画像（9-slice対応）。確認ダイアログのキャンセルボタンなどに使用されます。")]
        [SerializeField] private Sprite uiButtonIndigoImage; // インディゴ系のUIボタン画像（9-slice対応）
        
        [Header("UI Element Images")]
        [Tooltip("タイトル画像（「ミニノベルゲーム」など）。")]
        [SerializeField] private Sprite titleImage; // タイトル画像
        [Tooltip("スコア表示用の背景画像（9-slice対応）。")]
        [SerializeField] private Sprite scoreDisplayBackgroundImage; // スコア表示用の背景画像
        [Tooltip("メニューボタン用の画像（9-slice対応）。登場人物、実績、もうひとつボタンなどに使用されます。")]
        [SerializeField] private Sprite menuButtonImage; // メニューボタン用の画像
        
        [Header("Effects")]
        [SerializeField] private Material distortionMaterial;
        
        // 背景テクスチャのキャッシュ（VisualElement → Texture2D）
        // 注意: DistortionEffectManagerでも使用するため、共有する必要がある場合はpublicにするか、DistortionEffectManagerに渡す
        private Dictionary<VisualElement, Texture2D> backgroundTextureCache = new Dictionary<VisualElement, Texture2D>();
        
        // 背景の明度を下げるオーバーレイ
        private VisualElement backgroundOverlay;
        private Coroutine backgroundOverlayFadeCoroutine;
        private const float BackgroundOverlayOpacity = 0.6f; // オーバーレイの不透明度（0.3 = 30%の暗さ）
        private const float BackgroundOverlayFadeDuration = 0.5f; // フェードイン時間（秒）
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] wordGetSounds; // 「もうひとつ」をゲットした時の効果音（複数からランダムに選択）
        [SerializeField] private AudioClip wordGetIncreaseSound; // ワードゲット数が増える時の効果音
        [SerializeField] private AudioClip wordGetDecreaseSound; // ワードゲット数が減る時の効果音
        [SerializeField] private AudioClip creditsBGM; // エンドクレジットBGM
        [SerializeField] private AudioClip selectionBGM; // シナリオ選択画面BGM
        [SerializeField] private AudioClip typewriterSound; // タイプライター文字表示時の効果音
        [SerializeField] private AudioClip lostLetterSound; // ダークモードで「※」が表示される時の専用効果音
        [SerializeField] private AudioClip sparkleSound; // スパークルアイコンクリック時の効果音（「きらん！」）
        [SerializeField] private AudioClip buttonHoverSound; // ボタンにマウスオーバーした時の効果音（「ぱっ」）
        [SerializeField] private AudioClip thunderSound; // 3周目移行時の雷のような音
        [SerializeField] private AudioClip truthDoorUnlockSound; // 真実の扉出現時の効果音
        [SerializeField] private AudioClip[] ambientSounds; // 各シナリオの環境音（インデックス0=シナリオ1, 1=シナリオ2, ...）
        
        
        [Header("Emoji Icons (for Web compatibility)")]
        [SerializeField] private Sprite creditsIcon; // エンドクレジット用のアイコン（🎬の代替）
        [SerializeField] private Sprite achievementsIcon; // 実績用のアイコン（🏆の代替）
        [SerializeField] private Sprite clockIcon; // カウントダウン用のアイコン（⏰の代替）
        [SerializeField] private Sprite sparkleIcon; // スパークル用のアイコン（✨の代替）
        [SerializeField] private Sprite soundIcon; // サウンド設定用のアイコン（🔊の代替）

        private GameManager gameManager;
        private AudioManager audioManager;
        private UIDocument currentDocument;
        private List<GameObject> currentButtons = new List<GameObject>();
        
        // マネージャークラスのインスタンス
        private TypewriterEffectManager typewriterEffectManager;
        private CountdownManager countdownManager;
        private ScreenTransitionManager screenTransitionManager;
        private ProfileScreenManager profileScreenManager;
        private AchievementsScreenManager achievementsScreenManager;
        private MouhitotsuScreenManager mouhitotsuScreenManager;
        private CreditsScreenManager creditsScreenManager;
        private SoundSettingsManager soundSettingsManager;
        private SelectionScreenManager selectionScreenManager;
        
        // 演出マネージャー
        private LetterFallAnimationManager letterFallAnimationManager;
        private ShakeAnimationManager shakeAnimationManager;
        private FadeEffectManager fadeEffectManager;
        private DistortionEffectManager distortionEffectManager;
        private WordGetEffectManager wordGetEffectManager;
        private ScenarioUnlockEffectManager scenarioUnlockEffectManager;
        private ChapterTransitionManager chapterTransitionManager;
        
        // プロフィール関連（ProfileScreenManagerで管理されているため、ここでは使用しない）
        
        // 「もうひとつ」関連
        private bool wordFoundInCurrentScenario = false; // 現在のシナリオで「もうひとつ」を見つけたか
        
        // スコア減少演出用
        private int previousScore = -1; // 前回のスコア（-1は初期値）
        
        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManagerが見つかりません！");
                return;
            }

            // オーディオマネージャーを取得
            audioManager = AudioManager.Instance;
            if (audioManager == null)
            {
                audioManager = gameObject.AddComponent<AudioManager>();
            }
            
            audioManager.SetAudioClips(
                wordGetSounds, 
                wordGetIncreaseSound,
                wordGetDecreaseSound,
                creditsBGM, 
                selectionBGM, 
                typewriterSound, 
                lostLetterSound, 
                sparkleSound, 
                buttonHoverSound, 
                thunderSound,
                truthDoorUnlockSound,
                ambientSounds);

            // マネージャークラスのインスタンスを作成
            typewriterEffectManager = gameObject.AddComponent<TypewriterEffectManager>();
            if (audioManager != null)
            {
                typewriterEffectManager.SetTypewriterSound(audioManager.GetTypewriterSound());
                typewriterEffectManager.SetLostLetterSound(audioManager.GetLostLetterSound());
            }
            countdownManager = gameObject.AddComponent<CountdownManager>();
            screenTransitionManager = gameObject.AddComponent<ScreenTransitionManager>();
            profileScreenManager = new ProfileScreenManager(gameManager);
            profileScreenManager.SetTypewriterEffectManager(typewriterEffectManager);
            profileScreenManager.SetOnHoverSoundCallback(PlayHoverSound);
            profileScreenManager.SetOnProfileSelectedCallback(() => {
                // プロフィールが選択されたら、プロフィール画面を再生成
                if (profileScreenDocument != null && profileScreenDocument.gameObject.activeSelf)
                {
                    var root = profileScreenDocument.rootVisualElement;
                    if (root != null)
                    {
                        profileScreenManager.CreateProfileCards(root);
                    }
                }
            });
            profileScreenManager.SetOnProfileDetailUpdateCallback(() => {
                // プロフィール詳細のみを更新（リストは再生成しない）
                if (profileScreenDocument != null && profileScreenDocument.gameObject.activeSelf)
                {
                    var root = profileScreenDocument.rootVisualElement;
                    if (root != null)
                    {
                        profileScreenManager.RefreshProfileDetail(root);
                    }
                }
            });
            achievementsScreenManager = new AchievementsScreenManager(gameManager);
            achievementsScreenManager.SetOnSparkleClickedCallback(PlaySparkleSound);
            achievementsScreenManager.SetOnHoverSoundCallback(PlayHoverSound);
            
            mouhitotsuScreenManager = new MouhitotsuScreenManager(gameManager);
            mouhitotsuScreenManager.SetOnHoverSoundCallback(PlayHoverSound);
            
            creditsScreenManager = gameObject.AddComponent<CreditsScreenManager>();

            // 演出マネージャーを初期化
            letterFallAnimationManager = gameObject.AddComponent<LetterFallAnimationManager>();
            letterFallAnimationManager.Initialize(gameManager);
            
            shakeAnimationManager = gameObject.AddComponent<ShakeAnimationManager>();
            
            fadeEffectManager = gameObject.AddComponent<FadeEffectManager>();
            
            distortionEffectManager = gameObject.AddComponent<DistortionEffectManager>();
            if (distortionMaterial != null)
            {
                distortionEffectManager.SetDistortionMaterial(distortionMaterial);
            }
            // 背景テクスチャキャッシュを共有
            distortionEffectManager.SetBackgroundTextureCache(backgroundTextureCache);
            
            wordGetEffectManager = gameObject.AddComponent<WordGetEffectManager>();
            wordGetEffectManager.Initialize(gameManager, audioManager, sparkleIcon);
            
            scenarioUnlockEffectManager = gameObject.AddComponent<ScenarioUnlockEffectManager>();
            scenarioUnlockEffectManager.Initialize(gameManager, audioManager, scenarioButtonNormalImage, OnScenarioSelected, () => PlayHoverSound());
            
            chapterTransitionManager = gameObject.AddComponent<ChapterTransitionManager>();
            chapterTransitionManager.Initialize(gameManager, audioManager, typewriterEffectManager, () => ShowTitleScreen(), () => HideAllScreens());

            gameManager.OnScoreChanged += UpdateScoreDisplay;
            gameManager.OnLetterLost += OnLetterLost;
            Debug.Log("[GameManager] プロローグを開始します。");
            ShowTitleScreen();
        }

        /// <summary>
        /// 文字が失われた時の処理
        /// </summary>
        private void OnLetterLost(char lostLetter)
        {
            if (letterFallAnimationManager != null && currentDocument != null)
            {
                var root = currentDocument.rootVisualElement;
                if (root != null)
                {
                    letterFallAnimationManager.AnimateLetterFall(lostLetter, root);
                }
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreChanged -= UpdateScoreDisplay;
                gameManager.OnLetterLost -= OnLetterLost;
            }

            // 歪み効果のクリーンアップはDistortionEffectManagerで管理されているため、ここでは不要
            
            // 背景オーバーレイのクリーンアップ
            CleanupBackgroundOverlay();
        }

        /// <summary>
        /// 暗転演出を伴ってタイトル画面を表示
        /// </summary>
        public void ShowTitleScreenWithFade()
        {
            StartCoroutine(ShowTitleScreenCoroutine());
        }

        private IEnumerator ShowTitleScreenCoroutine()
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null)
            {
                ShowTitleScreen();
                yield break;
            }

            var root = currentDocument.rootVisualElement;

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

            // フェードイン（黒画面へ）
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                blackOverlay.style.opacity = Mathf.Min(elapsed / duration, 1.0f);
                yield return null;
            }

            ShowTitleScreen();
        }

        /// <summary>
        /// タイトル画面を表示
        /// </summary>
        public void ShowTitleScreen()
        {
            FadeOutAudioOnSceneChange();
            HideAllScreens(true);
            
            if (titleScreenDocument == null)
            {
                Debug.LogError("TitleScreenDocumentがアサインされていません！");
                return;
            }

            titleScreenDocument.gameObject.SetActive(true);
            currentDocument = titleScreenDocument;
            
            var root = titleScreenDocument.rootVisualElement;
            if (root == null) return;
            
            // 背景画像を設定（シナリオ選択背景を使用）
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                    
                    // 背景テクスチャを事前にキャッシュ
                    if (selectionScreenBackground != null && selectionScreenBackground.texture != null)
                    {
                        backgroundTextureCache[backgroundImage] = selectionScreenBackground.texture;
                    }
                    
                    // ダークモード時は背景を歪ませる
                    ApplyBackgroundDistortion(backgroundImage);
                }
            }
            
            // スタートボタンの設定
            var startButton = root.Q<Button>("StartButton");
            if (startButton != null)
            {
                startButton.clicked += OnStartButtonClicked;
                startButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                
                // 3周目：スタートボタンのテキストも伏字にする
                var lostLetters = gameManager.GetLostLetters();
                if (lostLetters.Count > 0)
                {
                    string buttonText = "もうひとつを探す";
                    foreach (char lostLetter in lostLetters)
                    {
                        buttonText = buttonText.Replace(lostLetter.ToString(), "※");
                    }
                    startButton.text = buttonText;
                }
                else
                {
                    startButton.text = "もうひとつを探す";
                }
                
                // スタートボタンに画像を適用
                Color startButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(startButton, uiButtonNormalImage, startButtonTextColor);
            }
            
            // 謎の声テキストを非表示に設定
            var mysteryVoiceText = root.Q<VisualElement>("MysteryVoiceText");
            if (mysteryVoiceText != null)
            {
                mysteryVoiceText.style.display = DisplayStyle.None;
            }
            
            // バージョン情報の表示（3周目などで伏字にする）
            var versionText = root.Q<Label>("VersionText");
            if (versionText != null)
            {
                string text = "v1.7.3 (2026-01-02)";
                var lostLetters = gameManager.GetLostLetters();
                var collectedLetters = gameManager.GetCollectedLetters();
                versionText.text = TextFormatter.FormatText(text, collectedLetters, lostLetters, true);
            }
            
            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }
        
        /// <summary>
        /// スタートボタンがクリックされた時の処理
        /// </summary>
        private void OnStartButtonClicked()
        {
            if (titleScreenDocument == null) return;
            
            var root = titleScreenDocument.rootVisualElement;
            if (root == null) return;
            
            // スタートボタンを非表示
            var startButton = root.Q<Button>("StartButton");
            if (startButton != null)
            {
                startButton.style.display = DisplayStyle.None;
            }
            
            // 謎の声テキストを表示
            var mysteryVoiceText = root.Q<VisualElement>("MysteryVoiceText");
            if (mysteryVoiceText != null && typewriterEffectManager != null)
            {
                mysteryVoiceText.style.display = DisplayStyle.Flex;
                
                // 3周目の場合はテキストを変更
                string mysteryText = gameManager.IsThirdLoop()
                    ? "謎の声：あなたは「※※※※※」を探す使命を...忘れてはいけません。"
                    : "謎の声：あなたは【もうひとつ】を探す使命が与えられています。";

                // ダークモード：失われた文字を置換、取得した文字に色を付ける
                var lostLetters = gameManager.GetLostLetters();
                var collectedLetters = gameManager.GetCollectedLetters();
                mysteryText = TextFormatter.FormatText(mysteryText, collectedLetters, lostLetters, true);

                // 強調ワードを含むタイプライター効果でテキストを表示（フォントサイズ24、クリック不可、速度を考慮）
                // StartTypewriterEffectWithClickableWord 内部で speedMultiplier はまだサポートしていないが、
                // 既に強調（10倍遅延）が入っているので十分印象的になるはず。
                typewriterEffectManager.StartTypewriterEffectWithClickableWord(mysteryVoiceText, mysteryText, () =>
                {
                    // タイプライター効果完了後、テキストを3秒かけてフェードアウト
                    StartCoroutine(FadeOutTitleTextAndShowSelection(mysteryVoiceText));
                }, fontSize: 24, isClickable: false);
            }
            else
            {
                // タイプライター効果が使えない場合は即座に遷移
                ShowSelectionScreen();
            }
        }
        
        /// <summary>
        /// タイトルテキストをフェードアウトしてからシナリオ選択画面を表示
        /// </summary>
        private IEnumerator FadeOutTitleTextAndShowSelection(VisualElement titleElement)
        {
            if (titleElement == null) yield break;
            
            // 初期opacityを取得（設定されていない場合は1.0）
            float startOpacity = 1.0f;
            if (titleElement.style.opacity.value > 0f)
            {
                startOpacity = titleElement.style.opacity.value;
            }
            else
            {
                titleElement.style.opacity = startOpacity;
            }
            
            // 3秒かけてフェードアウト
            float fadeDuration = 3.0f;
            float elapsed = 0f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float opacity = Mathf.Lerp(startOpacity, 0f, t);
                titleElement.style.opacity = opacity;
                yield return null;
            }
            
            // 完全に透明になったことを確認
            titleElement.style.opacity = 0f;
            
            // シナリオ選択画面をフェードインで表示
            ShowSelectionScreen();
        }

        /// <summary>
        /// Chapter C（3周目）への移行演出を外部から開始するためのメソッド
        /// </summary>
        public void TriggerChapterCTransition(int score)
        {
            if (chapterTransitionManager != null && titleScreenDocument != null)
            {
                StartCoroutine(chapterTransitionManager.ShowChapterCTransition(score, titleScreenDocument));
            }
        }

        /// <summary>
        /// 3周目への移行カットシーンを外部から開始するためのメソッド
        /// </summary>
        public void TriggerThirdLoopCutscene()
        {
            if (chapterTransitionManager != null && titleScreenDocument != null)
            {
                chapterTransitionManager.TriggerThirdLoopCutscene(titleScreenDocument);
            }
        }


        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        private void ShowConfirmationDialog(string message, System.Action onConfirm)
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) return;
            
            var root = currentDocument.rootVisualElement;
            
            // モーダル背景
            var modalBackground = new VisualElement();
            modalBackground.style.position = Position.Absolute;
            modalBackground.style.left = 0;
            modalBackground.style.top = 0;
            modalBackground.style.right = 0;
            modalBackground.style.bottom = 0;
            modalBackground.style.backgroundColor = new Color(0, 0, 0, 0.8f);
            modalBackground.style.justifyContent = Justify.Center;
            modalBackground.style.alignItems = Align.Center;
            // zIndexが使えない場合は、rootの最後に追加することで最前面に表示される
            
            // ダイアログ本体
            var dialog = new VisualElement();
            dialog.AddToClassList("card");
            dialog.style.paddingTop = 32;
            dialog.style.paddingBottom = 32;
            dialog.style.paddingLeft = 32;
            dialog.style.paddingRight = 32;
            dialog.style.width = 500;
            dialog.style.alignItems = Align.Center;
            // 黒または濃い藍色系の半透明背景を追加
            dialog.style.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.95f); // 濃い藍色系、ほぼ不透明
            dialog.style.borderTopLeftRadius = 10;
            dialog.style.borderTopRightRadius = 10;
            dialog.style.borderBottomLeftRadius = 10;
            dialog.style.borderBottomRightRadius = 10;
            
            // メッセージ
            var label = new Label(message);
            label.style.fontSize = 20;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 30;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = Color.white; // 文字色を白に設定
            dialog.Add(label);
            
            // ボタンコンテナ
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.SpaceBetween;
            buttonContainer.style.width = Length.Percent(100);
            
            // OKボタン
            var okButton = new Button(() => {
                root.Remove(modalBackground);
                onConfirm?.Invoke();
            });
            okButton.text = "OK";
            okButton.AddToClassList("button-gradient");
            okButton.style.flexGrow = 1;
            okButton.style.marginRight = 10;
            okButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
            // OKボタンに画像を適用
            Color okButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
            ApplyButtonImage(okButton, uiButtonNormalImage, okButtonTextColor);
            buttonContainer.Add(okButton);
            
            // キャンセルボタン
            var cancelButton = new Button(() => {
                root.Remove(modalBackground);
            });
            cancelButton.text = "キャンセル";
            cancelButton.AddToClassList("button-gradient-indigo");
            cancelButton.style.flexGrow = 1;
            cancelButton.style.marginLeft = 10;
            cancelButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
            // キャンセルボタンに画像を適用（明るめの文字色に変更）
            Color cancelButtonTextColor = new Color(0.9f, 0.9f, 1f, 1f); // 明るい青白系
            ApplyButtonImage(cancelButton, uiButtonIndigoImage, cancelButtonTextColor);
            buttonContainer.Add(cancelButton);
            
            dialog.Add(buttonContainer);
            modalBackground.Add(dialog);
            root.Add(modalBackground);
        }

        /// <summary>
        /// 暗転演出を伴うChapterジャンプを実行
        /// </summary>
        private IEnumerator PerformChapterJump(string chapterId)
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null)
            {
                ChapterManager.Instance.JumpToChapter(chapterId);
                ShowSelectionScreen();
                yield break;
            }

            var root = currentDocument.rootVisualElement;

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
            audioManager.PlayThunderSound();

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
            ChapterManager.Instance.JumpToChapter(chapterId);
            
            // 画面遷移
            ShowSelectionScreen();
            
            // 新しい画面が表示されるまで待つ（ShowSelectionScreen内でHideAllScreensが呼ばれるため）
            yield return null;
            
            // ShowSelectionScreen() で currentDocument が変わるので、新しい root にオーバーレイを付け直す必要があるかもしれないが、
            // 演出としては一瞬暗くなってから新しい画面が表示されるので、これで十分か。
        }

        public bool CheckAndGoToChapterC()
        {
            // ダークモード中にシナリオを選択した際、スコアが6に戻っていればChapter Cへ強制転送
            if (gameManager.IsDarkMode() && !gameManager.isThirdLoop && gameManager.score <= 6)
            {
                ChapterManager.Instance.LogChapter("C", "文字をいくつか失った状態でシナリオ6クリア -> 3周目へ強制移行");
                
                Debug.Log("[GameManager] 不正なデータが修正されました。システムを強制再起動します。");
                // 3周目への移行
                TriggerChapterCTransition(gameManager.score);
                return true;
            }
            return false;
        }

        public void ShowSelectionScreen()
        {
            FadeOutAudioOnSceneChange();
            HideAllScreens(true);
            
            // シナリオごとのランダム要素を生成（シナリオ選択画面で1回だけ生成）
            if (gameManager != null)
            {
                gameManager.GenerateScenarioRandomData();
            }
            
            // クレジットBGMをフェードアウト停止（急な停止を避ける）
            audioManager.FadeOutCreditBGM(1.0f);

            if (selectionScreenDocument == null)
            {
                Debug.LogError("SelectionScreenDocumentがアサインされていません！");
                return;
            }
            
            if (CheckAndGoToChapterC())
            {
                return;
            }

            if (CheckAndGoToEndCredits())
            {
                return;
            }
            
            selectionScreenDocument.gameObject.SetActive(true);
            currentDocument = selectionScreenDocument;

            // シナリオ選択BGMをフェードインして再生
            StartSelectionBGM();
            
            var root = selectionScreenDocument.rootVisualElement;
            if (root == null) return;
            
            // 背景画像を設定
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                    
                    // 背景テクスチャを事前にキャッシュ
                    if (selectionScreenBackground != null && selectionScreenBackground.texture != null)
                    {
                        backgroundTextureCache[backgroundImage] = selectionScreenBackground.texture;
                    }
                    
                    // ダークモード時は背景を歪ませる
                    ApplyBackgroundDistortion(backgroundImage);
                }
            }

            // タイトルを画像に置き換え
            var titleLabel = root.Q<Label>("TitleText");
            if (titleLabel != null && titleImage != null && titleImage.texture != null)
            {
                // LabelをVisualElementに置き換えて画像を表示
                var titleContainer = titleLabel.parent;
                if (titleContainer != null)
                {
                    var titleImageElement = new VisualElement();
                    // WebGL対応: アスペクト比を維持しつつ、適切なサイズにスケール
                    float originalWidth = titleImage.texture.width;
                    float originalHeight = titleImage.texture.height;
                    float aspectRatio = originalHeight / originalWidth;
                    
                    // 最大幅を600pxに制限し、アスペクト比を維持して高さを計算
                    float maxWidth = 600f;
                    float calculatedWidth = Mathf.Min(originalWidth, maxWidth);
                    float calculatedHeight = calculatedWidth * aspectRatio;
                    
                    // 最大高さも200pxに制限（必要に応じて）
                    if (calculatedHeight > 200f)
                    {
                        calculatedHeight = 200f;
                        calculatedWidth = calculatedHeight / aspectRatio;
                    }
                    
                    titleImageElement.style.width = calculatedWidth;
                    titleImageElement.style.height = calculatedHeight;
                    titleImageElement.style.backgroundImage = new StyleBackground(titleImage.texture);
                    titleImageElement.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    titleImageElement.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    titleImageElement.style.marginBottom = 20;
                    titleContainer.Insert(titleContainer.IndexOf(titleLabel), titleImageElement);
                    titleLabel.style.display = DisplayStyle.None; // 元のLabelを非表示
                }
            }
            else if (titleLabel != null)
            {
                // 画像がない場合は従来通りテキストを表示
                string titleText = "ミニノベルゲーム";
                var lostLetters = gameManager.GetLostLetters();
                if (lostLetters.Count > 0)
                {
                    foreach (char lostLetter in lostLetters)
                    {
                        titleText = titleText.Replace(lostLetter.ToString(), "※");
                    }
                }
                titleLabel.text = titleText;
                titleLabel.AddToClassList("title-text");
            }

            // プロフィールボタンの設定
            var showProfileButton = root.Q<Button>("ShowProfileButton");
            if (showProfileButton != null)
            {
                showProfileButton.clicked += ShowProfileScreen;
                showProfileButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // メニューボタンに画像を適用
                Color menuButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(showProfileButton, menuButtonImage, menuButtonTextColor);
            }

            // エンドクレジットボタンの設定（真実の扉クリア後のみ表示）
            var showCreditsButton = root.Q<Button>("ShowCreditsButton");
            if (showCreditsButton != null)
            {
                // 絵文字を画像に置き換え
                SetupButtonWithIcon(showCreditsButton, creditsIcon, "エンドクレジットを見る");
                showCreditsButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // メニューボタンに画像を適用
                Color menuButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(showCreditsButton, menuButtonImage, menuButtonTextColor);
                
                var scenario6Result = gameManager.GetScenarioResult(6);
                if (scenario6Result != null)
                {
                    showCreditsButton.style.display = DisplayStyle.Flex;
                    showCreditsButton.clicked += () => {
                        // 3周目の場合は特別版エンドクレジットを表示
                        if (gameManager.IsThirdLoop())
                        {
                            ShowConfirmationDialog("ここから先に進むともう戻れませんがよろしいですか？", () => {
                                StartCoroutine(ShowSpecialCreditsTransition());
                            });
                        }
                        else
                        {
                            ShowCreditsScreen(false);
                        }
                    };
                }
                else
                {
                    showCreditsButton.style.display = DisplayStyle.None;
                }
            }

            // 実績ボタンの設定（常に表示）
            var showAchievementsButton = root.Q<Button>("ShowAchievementsButton");
            if (showAchievementsButton != null)
            {
                // 絵文字を画像に置き換え
                SetupButtonWithIcon(showAchievementsButton, achievementsIcon, "実績一覧を見る");
                showAchievementsButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                showAchievementsButton.style.display = DisplayStyle.Flex;
                showAchievementsButton.clicked += ShowAchievementsScreen;
                // メニューボタンに画像を適用
                Color menuButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(showAchievementsButton, menuButtonImage, menuButtonTextColor);
            }

            // 「もうひとつ」ボタンの設定（常に表示）
            var showMouhitotsuButton = root.Q<Button>("ShowMouhitotsuButton");
            if (showMouhitotsuButton != null)
            {
                showMouhitotsuButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                showMouhitotsuButton.style.display = DisplayStyle.Flex;
                showMouhitotsuButton.clicked += ShowMouhitotsuScreen;
                // メニューボタンに画像を適用
                Color menuButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(showMouhitotsuButton, menuButtonImage, menuButtonTextColor);
            }

            // サウンド設定ボタンの設定
            var soundButton = root.Q<Button>("SoundButton");
            if (soundButton != null)
            {
                if (soundIcon != null)
                {
                    SetupButtonWithIcon(soundButton, soundIcon, "");
                }
                else
                {
                    soundButton.text = "🔊";
                }

                soundButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                soundButton.clicked += () => {
                    if (soundSettingsManager == null)
                    {
                        soundSettingsManager = new SoundSettingsManager(root, soundSettingsPanelUXML, PlayHoverSound);
                    }
                    soundSettingsManager.Show(root);
                };
                
                // 背景なし、アイコンのみの設定
                soundButton.style.backgroundColor = Color.clear;
                soundButton.style.backgroundImage = null;
                soundButton.style.borderTopWidth = 0;
                soundButton.style.borderRightWidth = 0;
                soundButton.style.borderBottomWidth = 0;
                soundButton.style.borderLeftWidth = 0;
                
                // アイコンの色（設定があれば）
                if (soundIcon == null)
                {
                    soundButton.style.color = Color.white;
                }
            }

            UpdateScoreDisplay();
            UpdateStoryProgressDisplay(root);
            CreateScenarioButtons(root);
            
            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }

            // シナリオ6解放演出のチェック
            // 注意：CreateScenarioButtons() の後に呼ぶ必要がある（ボタンが生成されてから演出を開始するため）
            // CheckAndConsumeScenario6Unlocked は初回のみ true を返す（内部フラグを消費する）
            if (gameManager.CanAccessScenario(6))
            {
                // まだ演出していないがアクセス可能な場合
                if (gameManager.CheckAndConsumeScenario6Unlocked())
                {
                    Debug.Log("[UIManagerUIToolkit] 真実の扉出現演出を開始します。");
                    // 真実の扉が開いたタイミングをChapterとして記録
                    if (ChapterManager.Instance != null)
                    {
                        // 3周目の場合はPreD、それ以外はPreA
                        if (gameManager.IsThirdLoop())
                        {
                            ChapterManager.Instance.LogChapter("PreD", "真実の扉が開いた（3周目でシナリオ1-5をクリア）");
                        }
                        else
                        {
                            ChapterManager.Instance.LogChapter("PreA", "真実の扉が開いた（シナリオ1-5をクリア）");
                        }
                    }
                    // ScenarioUnlockEffectManagerを使用して演出を表示
                    if (scenarioUnlockEffectManager != null)
                    {
                        var collectedLetters = gameManager.GetCollectedLetters();
                        var lostLetters = gameManager.GetLostLetters();
                        StartCoroutine(scenarioUnlockEffectManager.ShowScenario6UnlockAnimation(
                            root,
                            (text, collected, lost) => TextFormatter.FormatText(text, collected, lost, true)
                        ));
                    }
                    else
                    {
                        Debug.LogError("[UIManagerUIToolkit] scenarioUnlockEffectManager が null です。");
                    }
                }
                else
                {
                    Debug.Log($"[UIManagerUIToolkit] 真実の扉出現演出をスキップします。IsScenario6Unlocked: {gameManager.IsScenario6Unlocked()}, CanAccessScenario(6): {gameManager.CanAccessScenario(6)}");
                }
            }
        }

        public void ShowProfileScreen()
        {
            FadeOutAudioOnSceneChange();
            // シナリオ選択BGMの音量を下げる（流したまま）
            LowerSelectionBGMVolume();
            HideAllScreens(true);
            
            if (profileScreenDocument == null)
            {
                Debug.LogError("ProfileScreenDocumentがアサインされていません！");
                return;
            }

            profileScreenDocument.gameObject.SetActive(true);
            currentDocument = profileScreenDocument;
            
            var root = profileScreenDocument.rootVisualElement;
            
            // 背景画像を設定
            if (root != null && profileScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(profileScreenBackground);
                    
                    // 背景テクスチャを事前にキャッシュ
                    if (profileScreenBackground != null && profileScreenBackground.texture != null)
                    {
                        backgroundTextureCache[backgroundImage] = profileScreenBackground.texture;
                    }
                    
                    // ダークモード時は背景を歪ませる
                    ApplyBackgroundDistortion(backgroundImage);
                }
            }

            // タイトルを設定
            var titleLabel = root.Q<Label>("ProfileSectionTitle");
            if (titleLabel != null)
            {
                bool isDarkMode = gameManager.IsDarkMode();
                titleLabel.text = isDarkMode ? "登場人物プロフィール【データ破損】" : "登場人物プロフィール";
                if (isDarkMode)
                {
                    titleLabel.AddToClassList("title-text-dark");
                }
                else
                {
                    titleLabel.AddToClassList("title-text");
                }
            }

            if (profileScreenManager != null)
            {
                profileScreenManager.CreateProfileCards(root);
            }
            
            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromProfile");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
                backButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // 戻るボタンに画像を適用
                Color backButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backButton, uiButtonNormalImage, backButtonTextColor);
            }

            // タイトル画面に戻るボタン（もしあれば）
            var backToTitleButton = root.Q<Button>("BackToTitleButtonFromProfile");
            if (backToTitleButton != null)
            {
                backToTitleButton.clicked += ShowTitleScreenWithFade;
                backToTitleButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // タイトルに戻るボタンに画像を適用
                Color backToTitleButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backToTitleButton, uiButtonNormalImage, backToTitleButtonTextColor);
            }
            
            // トランジション開始
            if (root != null && screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        public void ShowScenarioScreen()
        {
            FadeOutAudioOnSceneChange();
            // シナリオ選択BGMをフェードアウトして時刻を記録（先に実行）
            PauseSelectionBGM();
            // HideAllScreens()は後で実行（フェードアウトが開始されるまで待つ）
            HideAllScreens();
            
            if (scenarioScreenDocument == null)
            {
                Debug.LogError("ScenarioScreenDocumentがアサインされていません！");
                return;
            }

            scenarioScreenDocument.gameObject.SetActive(true);
            currentDocument = scenarioScreenDocument;
            
            var scenario = gameManager.GetCurrentScenario();
            if (scenario == null) return;
            
            // シナリオの環境音を開始
            StartAmbientSound(scenario.id);

            var root = scenarioScreenDocument.rootVisualElement;
            
            // 背景画像を設定
            SetBackgroundImage(scenario.id, true);

            // タイトルと設定テキストを設定
            var titleLabel = root.Q<Label>("ScenarioTitleText");
            if (titleLabel != null)
            {
                bool isDarkMode = gameManager.IsDarkMode();
                if (isDarkMode)
                {
                    // ダークモード時のタイトル
                    string darkTitle = scenario.id switch
                    {
                        1 => "謎の依頼【データ破損】",
                        2 => "不思議なレストラン【データ破損】",
                        3 => "タイムカプセル【データ破損】",
                        4 => "魔法学校の試験【データ破損】",
                        5 => "最後のピース【データ破損】",
                        6 => "真実の扉【ダークモード】",
                        _ => scenario.title + "【データ破損】"
                    };
                    titleLabel.text = darkTitle;
                    titleLabel.AddToClassList("title-text-dark");
                }
                else
                {
                    titleLabel.text = scenario.title;
                    titleLabel.AddToClassList("title-text");
                }
                // 明るい色を適用
                Color textColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                titleLabel.style.color = textColor;
                titleLabel.style.textShadow = new TextShadow { offset = new Vector2(2, 2), blurRadius = 4, color = new Color(0, 0, 0, 0.8f) };
            }

            var setupContainer = root.Q<VisualElement>("SetupText");
            // SetupText内のLabelに明るい色を適用
            if (setupContainer != null)
            {
                Color textColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
                foreach (var child in setupContainer.Children())
                {
                    if (child is Label label)
                    {
                        label.style.color = textColor;
                        label.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                    }
                }
            }
            var choiceButtonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
            // 明るい色を適用
            Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5
            
            // スコア表示に明るい色を適用
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel != null)
            {
                scoreLabel.style.color = brightTextColor;
                scoreLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット成功メッセージに明るい色を適用
            if (wordFoundMessageLabel != null)
            {
                wordFoundMessageLabel.style.color = brightTextColor;
                wordFoundMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット失敗メッセージに明るい色を適用
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.color = brightTextColor;
                wordFailedMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // カウントダウンテキストに明るい色を適用
            if (countdownText != null)
            {
                countdownText.style.color = brightTextColor;
                countdownText.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // 時計アイコンを設定
            var clockIcon = root.Q<Image>("ClockIcon");
            if (clockIcon != null && this.clockIcon != null)
            {
                clockIcon.sprite = this.clockIcon;
            }
            
            // フラグをリセット
            wordFoundInCurrentScenario = false;
            
            // 既存のカウントダウンを停止
            if (countdownManager != null)
            {
                countdownManager.StopCountdown();
            }
            
            // 選択肢ボタンコンテナを最初は非表示にする
            if (choiceButtonContainer != null)
            {
                choiceButtonContainer.style.display = DisplayStyle.None;
            }
            
            // メッセージラベルを非表示にする
            if (wordFoundMessageLabel != null)
            {
                wordFoundMessageLabel.style.display = DisplayStyle.None;
            }
            
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.display = DisplayStyle.None;
            }
            
            // カウントダウンコンテナを非表示にする
            if (countdownContainer != null)
            {
                countdownContainer.style.display = DisplayStyle.None;
            }
            
            if (setupContainer != null)
            {
                // 既存の子要素をクリア
                setupContainer.Clear();
                
                // ダークモード時のsetupテキストを取得
                bool isDarkMode = gameManager.IsDarkMode() && !gameManager.IsThirdLoop();
                bool isThirdLoop = gameManager.IsThirdLoop();
                string setupText = scenario.setup;
                
                if (isDarkMode)
                {
                    setupText = scenario.id switch
                    {
                        1 => $"【エラー】探偵事務所のデータが破損しています。\n写真の人物が歪み、存在が不安定になっています。\nバグの影響で「も」という文字が消失しました。\n\n{scenario.setup}",
                        2 => $"【エラー】レストランのデータが破損しています。\nメニューが文字化けし、料理のデータが読み込めません。\nバグの影響で「う」という文字が消失しました。\n\n{scenario.setup}",
                        3 => $"【エラー】タイムカプセルのデータが破損しています。\n過去の記憶が歪み、データが欠損しています。\nバグの影響で「ひ」という文字が消失しました。\n\n{scenario.setup}",
                        4 => $"【エラー】魔法学校のデータが破損しています。\n呪文のコードがエラーを起こし、魔法が機能しません。\nバグの影響で「と」という文字が消失しました。\n\n{scenario.setup}",
                        5 => $"【エラー】パズルのデータが破損しています。\nピースの整合性が失われ、完成することができません。\nバグの影響で「つ」という文字が消失しました。\n\n{scenario.setup}",
                        6 => scenario.setup,
                        _ => scenario.setup
                    };
                }
                
                // タイプライター効果で表示（完了後に選択肢ボタンを表示）
                if (typewriterEffectManager != null)
                {
                    typewriterEffectManager.StartTypewriterEffectWithClickableWord(setupContainer, setupText, () =>
                    {
                        // タイプライター効果が完了したら選択肢ボタンを順次表示
                        StartCoroutine(ShowChoicesSequentially(root));
                    }, (found, pos) => {
                        if (found)
                        {
                            wordFoundInCurrentScenario = true;
                            
                            // 効果音を再生（ワードゲット数が増える時の音 + ランダムなワードゲット音）
                            if (audioManager != null)
                            {
                                audioManager.PlayWordGetIncreaseSound();
                                audioManager.PlayWordGetSound();
                            }
                            
                            // メッセージを表示
                            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
                            if (wordFoundMessageLabel != null)
                            {
                                wordFoundMessageLabel.text = isDarkMode || isThirdLoop
                                    ? "⚠️ システムエラー：データ破損を検出 ⚠️"
                                    : "あなたは何かをみつけた気がした";
                                wordFoundMessageLabel.style.display = DisplayStyle.Flex;
                                StartCoroutine(ShakeAnimation(wordFoundMessageLabel));
                            }
                            
                            // 選択肢ボタンを順次表示
                            StartCoroutine(ShowChoicesSequentially(root));

                            // スコア表示へ光が飛んでいく演出を開始
                            if (!isDarkMode && !isThirdLoop)
                            {
                                StartCoroutine(ShowLetterGetAnimation(pos));
                            }
                        }
                    });
                }
            }
            else
            {
                // タイプライター効果がない場合は即座に選択肢ボタンを順次表示
                StartCoroutine(ShowChoicesSequentially(root));
            }

            CreateChoiceButtons(root, scenario);
            UpdateScoreDisplay();

            // トランジション開始（シナリオ画面のみスケールアニメーションあり）
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root, withScale: true);
            }
        }

        public bool CheckAndGoToEndCredits()
        {
            var currentScenario = gameManager.GetCurrentScenario();
            if (currentScenario != null && currentScenario.id == 6)
            {
                bool isThirdLoop = gameManager.IsThirdLoop();
                var result6 = gameManager.GetScenarioResult(6);

                if (isThirdLoop)
                {
                    if (result6 != null && result6.hasWord)
                    {
                        // 3周目かつシナリオ6クリア（ワード取得成功）
                        // 暗転演出を挟んでから特別版エンドクレジットを表示
                        StartCoroutine(ShowSpecialCreditsTransition());
                        return true;
                    }
                    // ワード未取得の場合は通常のリザルト画面へ（後で共通処理へ）
                }
                else
                {
                    // まだ3周目でない場合の判定
                    if (gameManager.AreAllLettersLost())
                    {
                        // ダークモードですべての文字を消失した状態でクリア
                        TriggerThirdLoopCutscene();
                        return true;
                    }
                }
            }

            return false;
        }

        public void ShowResultScreen()
        {
            FadeOutAudioOnSceneChange();
            // 環境音を長めにフェードアウト（結果画面に移行）
            FadeOutAmbientSoundForResult();
            HideAllScreens(true);
            
            if (resultScreenDocument == null)
            {
                Debug.LogError("ResultScreenDocumentがアサインされていません！");
                return;
            }

            resultScreenDocument.gameObject.SetActive(true);
            currentDocument = resultScreenDocument;
            
            var scenario = gameManager.GetCurrentScenario();
            if (scenario == null) return;

            var result = gameManager.GetScenarioResult(scenario.id);
            if (result == null) return;

            var root = resultScreenDocument.rootVisualElement;
            
            // 背景画像を設定
            SetBackgroundImage(scenario.id, false);

            bool isDarkMode = gameManager.IsDarkMode();
            
            // 明るい色を定義（メソッド全体で使用）
            Color brightTextColor = new Color(0xED / 255f, 0xD7 / 255f, 0xB5 / 255f, 1f); // #EDD7B5

            // 後日談を設定（最初は非表示）
            var epilogueContainer = root.Q<VisualElement>("EpilogueContainer");
            var epilogueLabel = root.Q<Label>("EpilogueText");
            if (epilogueContainer != null)
            {
                // 後日談コンテナを最初は非表示にする
                epilogueContainer.style.display = DisplayStyle.None;
                
                // ダークモードの場合はダークスタイルを適用
                epilogueContainer.ClearClassList();
                if (isDarkMode)
                {
                    epilogueContainer.AddToClassList("epilogue-box-dark");
                }
                else
                {
                    epilogueContainer.AddToClassList("epilogue-box");
                }
            }
            
            // 後日談テキストを準備
            string epilogueText = "";
            if (epilogueLabel != null)
            {
                // 既存のクラスをクリア
                epilogueLabel.ClearClassList();
                // 明るい色を適用
                epilogueLabel.style.color = brightTextColor;
                epilogueLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                
                if (isDarkMode)
                {
                    // シナリオごとのダークモードエピローグ
                    epilogueText = scenario.id switch
                    {
                        1 => result.choiceId == 1
                            ? "【データ破損】もも子のデータは完全に崩壊しました。\n写真から人物の姿が消え、存在が不安定になりました。\n「も」という文字が消失し、探偵事務所のデータも歪み始めています。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「も...もど...もどれない...」\n\n【エンド：文字の消失】"
                            : "【システムエラー】データの修復を試みましたが、失敗しました。\nもも子のデータは完全に破損し、修復不可能な状態です。\n写真の人物は、データの欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                        2 => result.choiceId == 1
                            ? "【データ破損】うみシェフのデータは完全に崩壊しました。\nレストランのメニューが文字化けし、料理のデータが読み込めなくなりました。\n「う」という文字が消失し、レストランの存在が不安定になっています。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「う...うみ...うみへ...」\n\n【エンド：文字の消失】"
                            : "【システムエラー】システムエラーの報告を行いましたが、無意味でした。\nうみシェフのデータは完全に破損し、レストランは機能しなくなりました。\n料理のデータが欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                        3 => result.choiceId == 1
                            ? "【データ破損】ひろのデータは完全に崩壊しました。\n過去の記憶が歪み、タイムカプセルのデータが欠損しています。\n「ひ」という文字が消失し、友情の記憶が失われました。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「ひ...ひろ...ひろが...」\n\n【エンド：文字の消失】"
                            : "【システムエラー】データの修復を試みましたが、失敗しました。\nひろのデータは完全に破損し、過去の記憶が消えてしまいました。\nタイムカプセルは、データの欠片となって崩壊しました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                        4 => result.choiceId == 1
                            ? "【データ破損】とおる試験官のデータは完全に崩壊しました。\n魔法のコードがエラーを起こし、魔法学校のシステムが停止しました。\n「と」という文字が消失し、魔法のデータが読み込めなくなりました。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「と...とおる...とおるが...」\n\n【エンド：文字の消失】"
                            : "【システムエラー】システムの整合性を確認しましたが、手遅れでした。\nとおる試験官のデータは完全に破損し、魔法学校は機能しなくなりました。\n呪文のコードが欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                        5 => result.choiceId == 1
                            ? "【データ破損】つばさのデータは完全に崩壊しました。\nパズルのピースが永遠に足りず、完成することができなくなりました。\n「つ」という文字が消失し、愛の記憶が消えつつあります。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「つ...つばさ...つばさが...」\n\n【エンド：文字の消失】"
                            : "【システムエラー】完成できないことに気づきましたが、時既に遅しでした。\nつばさのデータは完全に破損し、パズルは永遠に完成できなくなりました。\n愛の記憶が欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                        6 => result.choiceId == 1
                            ? "世界は完全に崩壊しました。\nシミュレーションの整合性は失われ、修復不可能な状態です。\n\n登場人物たちは、データの欠片となって消えていきました。\nもも子、うみ、ひろ、とおる、つばさ...\nすべてが、あなたの異常な行動の結果です。\n\nあなたは、空っぽの世界に一人取り残されました。\n「もう...戻れない...」\n\n【エンド：世界崩壊】"
                            : "あなたは、世界の真実を知ってしまいました。\nこの世界は、シミュレーションだったのです。\n\nしかし、あなたの異常な行動が、世界を破壊してしまいました。\n登場人物たちは、バグによって歪んだ姿となっています。\n\nもも子は「も」という文字を失い、\nうみは「う」という文字を失い、\nひろは「ひ」という文字を失い、\nとおるは「と」という文字を失い、\nつばさは「つ」という文字を失いました。\n\n「もうひとつ」という言葉は、永遠に失われました。\n\n【エンド：言葉の消失】",
                        _ => "【データ破損】"
                    };
                    epilogueLabel.AddToClassList("epilogue-text-dark");
                }
                else
                {
                    epilogueText = result.epilogue;
                    
                    // シナリオ4（魔法学校の試験）の場合、ワードが見つからなかった場合に動物にゆかりのある話題を追加
                    // 重要: hasWordではなく、wordFoundInCurrentScenarioを使用
                    if (scenario.id == 4 && !wordFoundInCurrentScenario)
                    {
                        // シナリオのsetupから動物名を抽出（「試験官：「{animalName}を出現させなさい」」の形式）
                        string animalName = ExtractAnimalNameFromSetup(scenario.setup);
                        
                        if (!string.IsNullOrEmpty(animalName))
                        {
                            string relatedTopic = AnimalNameManager.GetRelatedTopic(animalName);
                            if (!string.IsNullOrEmpty(relatedTopic))
                            {
                                // epilogueに既に動物の話題が含まれていない場合のみ追加
                                if (!epilogueText.Contains(relatedTopic))
                                {
                                    epilogueText += $"\n\n試験官が何か言いかけた。\n試験官：「ところで、{animalName}について...{relatedTopic}」";
                                }
                            }
                        }
                    }
                    
                    epilogueLabel.AddToClassList("epilogue-text");
                }
                
                // 取得した文字に色を付け、失われた文字を伏字化
                var collectedLetters = gameManager.GetCollectedLetters();
                var lostLetters = gameManager.GetLostLetters();
                epilogueText = TextFormatter.FormatText(epilogueText, collectedLetters, lostLetters, true);
            }

            // ワードゲット表示（最初は非表示、結果テキストのタイプライター効果が完了したら表示）
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            var wordGetLabel = root.Q<Label>("WordGetText");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
            // スコア表示に明るい色を適用（brightTextColorはメソッド先頭で定義済み）
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel != null)
            {
                scoreLabel.style.color = brightTextColor;
                scoreLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲットテキストに明るい色を適用
            if (wordGetLabel != null)
            {
                wordGetLabel.style.color = brightTextColor;
                wordGetLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット成功メッセージに明るい色を適用
            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
            if (wordFoundMessageLabel != null)
            {
                wordFoundMessageLabel.style.color = brightTextColor;
                wordFoundMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // ワードゲット失敗メッセージに明るい色を適用
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.color = brightTextColor;
                wordFailedMessageLabel.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // カウントダウンテキストに明るい色を適用
            if (countdownText != null)
            {
                countdownText.style.color = brightTextColor;
                countdownText.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }
            
            // 時計アイコンを設定
            var clockIcon = root.Q<Image>("ClockIcon");
            if (clockIcon != null && this.clockIcon != null)
            {
                clockIcon.sprite = this.clockIcon;
            }
            
            // テキストに「もうひとつ」が含まれているかどうかを自動的に検出
            // 重要: hasWordの概念は不要。テキストに「もうひとつ」が含まれていれば、自動的にワードゲット可能
            if (result != null && scenario != null)
            {
                // 選択された選択肢のテキストを取得
                if (scenario.branches.ContainsKey(result.choiceId))
                {
                    var branch = scenario.branches[result.choiceId];
                    string branchText = branch.text ?? "";
                    
                    // テキストに「もうひとつ」が含まれているかチェック（伏字になる前の元のテキストで検出）
                    string[] patterns = { "【もうひとつ】", "もうひとつ", "もう、ひとつ", "もう,ひとつ" };
                    bool containsWord = false;
                    foreach (var pattern in patterns)
                    {
                        if (branchText.Contains(pattern))
                        {
                            containsWord = true;
                            break;
                        }
                    }
                    
                    // テキストに「もうひとつ」が含まれていれば、自動的にワードゲット可能
                    if (containsWord)
                    {
                        wordFoundInCurrentScenario = true;
                        Debug.Log($"[UIManagerUIToolkit] テキストに「もうひとつ」が含まれているため、自動的にワードゲット可能に設定しました。");
                    }
                    else
                    {
                        // テキストに「もうひとつ」が含まれていない場合のみ、フラグをリセット
                        wordFoundInCurrentScenario = false;
                    }
                }
            }
            
            // 既存のカウントダウンを停止
            if (countdownManager != null)
            {
                countdownManager.StopCountdown();
            }
            
            // カウントダウンコンテナを非表示にする
            if (countdownContainer != null)
            {
                countdownContainer.style.display = DisplayStyle.None;
            }
            
            // 失敗メッセージを非表示にする
            if (wordFailedMessageLabel != null)
            {
                wordFailedMessageLabel.style.display = DisplayStyle.None;
            }
            
            // 結果テキストを設定（タイプライター効果で表示）
            var resultLabel = root.Q<Label>("ResultText");
            if (resultLabel != null)
            {
                string resultText = "";
                if (isDarkMode)
                {
                    // シナリオごとのダークモード結果テキスト
                    resultText = scenario.id switch
                    {
                        1 => result.choiceId == 1
                            ? "私：「も...もど...もどれない...」\n\nデータが歪み始める。写真から人物の姿が消えていく。\n\n壊れた声：「【エラー】もも子のデータが...破損しています...」\n「「も」という文字が...消失しました...」\n「写真の人物は...存在しません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、データが破壊されていることを理解した。"
                            : "私：「このデータは...修復できるか？」\n\n壊れた声：「修復不可能です...」\n「もも子のデータは...完全に破損しています...」\n「「も」という文字が...永遠に失われました...」\n\n写真はデータの欠片となって消えていく。\n探偵事務所の存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        2 => result.choiceId == 1
                            ? "私：「う...うみ...うみへ...」\n\nメニューが文字化けし、レストランのデータが崩壊していく。\n\n壊れた声：「【エラー】うみのデータが...破損しています...」\n「「う」という文字が...消失しました...」\n「料理のデータは...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、レストランが機能しなくなることを理解した。"
                            : "私：「システムエラーを報告する...」\n\n壊れた声：「報告は...無意味です...」\n「うみのデータは...完全に破損しています...」\n「「う」という文字が...永遠に失われました...」\n\n料理のデータは欠片となって消えていく。\nレストランの存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        3 => result.choiceId == 1
                            ? "私：「ひ...ひろ...ひろが...」\n\n過去の記憶が歪み、タイムカプセルのデータが崩壊していく。\n\n壊れた声：「【エラー】ひろのデータが...破損しています...」\n「「ひ」という文字が...消失しました...」\n「友情の記憶は...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、記憶が失われることを理解した。"
                            : "私：「データを修復しようとする...」\n\n壊れた声：「修復不可能です...」\n「ひろのデータは...完全に破損しています...」\n「「ひ」という文字が...永遠に失われました...」\n\n過去の記憶は欠片となって消えていく。\nタイムカプセルの存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        4 => result.choiceId == 1
                            ? "私：「と...とおる...とおるが...」\n\n魔法のコードがエラーを起こし、魔法学校のシステムが崩壊していく。\n\n壊れた声：「【エラー】とおるのデータが...破損しています...」\n「「と」という文字が...消失しました...」\n「魔法のデータは...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、魔法が機能しなくなることを理解した。"
                            : "私：「システムの整合性を確認する...」\n\n壊れた声：「確認は...無意味です...」\n「とおるのデータは...完全に破損しています...」\n「「と」という文字が...永遠に失われました...」\n\n呪文のコードは欠片となって消えていく。\n魔法学校の存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        5 => result.choiceId == 1
                            ? "私：「つ...つばさ...つばさが...」\n\nパズルのピースが永遠に足りず、完成のデータが崩壊していく。\n\n壊れた声：「【エラー】つばさのデータが...破損しています...」\n「「つ」という文字が...消失しました...」\n「愛の記憶は...読み込めません...」\n\n画面が歪み、文字が崩れていく。\nあなたは、パズルが完成できなくなることを理解した。"
                            : "私：「完成できないことに気づく...」\n\n壊れた声：「気づいても...もう遅い...」\n「つばさのデータは...完全に破損しています...」\n「「つ」という文字が...永遠に失われました...」\n\n愛の記憶は欠片となって消えていく。\nパズルの存在も、不安定になっていく。\n\n壊れた声：「もう...戻れない...」\n「あなたの異常な行動が...世界を壊した...」",
                        6 => result.choiceId == 1
                            ? "私：「すみません...壊してしまって...」\n\n壊れた声：「謝っても...もう遅い...」\n世界が歪み始める。\n\n壊れた声：「この世界は...シミュレーションだった...」\n「あなたの異常な行動が...世界を破壊した...」\n「もう...修復できない...」\n\n画面が歪み、文字が崩れていく。\nあなたは、自分が何をしてしまったのか理解した。"
                            : "私：「この世界は...何ですか？」\n\n壊れた声：「シミュレーション...すべてが...」\n「あなたは...バグを起こした...」\n「世界の整合性が...崩壊している...」\n\n周囲の空間が歪み、現実が崩れていく。\n登場人物たちの姿が、データの欠片となって消えていく。\n\n壊れた声：「もう...戻れない...」\n「あなたは...世界を壊した...」",
                        _ => "【データ破損】"
                    };
                }
                else
                {
                    resultText = scenario.branches[result.choiceId].text;
                    
                    // シナリオ2（不思議なレストラン）の場合、選択肢1（本日のおすすめ）を選んだ時に料理に対するセリフを追加
                    if (scenario.id == 2 && result.choiceId == 1)
                    {
                        string todayRecommendation = RestaurantMenuManager.GetTodayRecommendation();
                        string comment = RestaurantCommentManager.GetCommentForDish(todayRecommendation);
                        
                        // 結果テキストの最後にセリフを追加
                        if (!string.IsNullOrEmpty(comment))
                        {
                            resultText += $"\n\n{comment}";
                        }
                    }
                }
                
                // 結果テキストをVisualElementに変更して「もうひとつ」をクリッカブルにする
                var resultContainer = new VisualElement();
                resultContainer.style.fontSize = 18;
                resultContainer.style.whiteSpace = WhiteSpace.Normal;
                resultContainer.style.maxWidth = 800;
                resultContainer.style.marginBottom = 20;
                resultContainer.style.alignItems = Align.FlexStart; // 左揃え
                resultContainer.style.alignSelf = Align.FlexStart; // 左揃え
                resultContainer.style.width = Length.Percent(100); // 幅を100%に設定
                
                // 元のLabelを非表示にして、新しいコンテナを追加
                resultLabel.style.display = DisplayStyle.None;
                resultLabel.parent.Insert(resultLabel.parent.IndexOf(resultLabel), resultContainer);
                
                // 結果テキストに「【もうひとつ】」が含まれているか確認
                // 重要: パターンマッチングはフォーマット前の元のテキストに対して行う（リッチテキストタグが含まれる前）
                // テキストに「もうひとつ」が含まれているかチェック（すべてのパターンを考慮）
                // 重要: シングルクォート（『』）、ダブルクォート（""）、角括弧（【】）など、すべてのパターンを検出
                string originalResultText = resultText; // フォーマット前のテキストを保存
                string[] mouhitotsuPatterns = { "【もうひとつ】", "もうひとつ", "もう、ひとつ", "もう,ひとつ", "『もうひとつ』", "\"もうひとつ\"", "「もうひとつ」" };
                bool hasMouhitotsu = false;
                foreach (var pattern in mouhitotsuPatterns)
                {
                    if (originalResultText.Contains(pattern))
                    {
                        hasMouhitotsu = true;
                        Debug.Log($"[UIManagerUIToolkit] 「もうひとつ」パターンを検出: '{pattern}'");
                        break;
                    }
                }
                
                // 取得した文字に色を付け、失われた文字を伏字化（表示用のテキストをフォーマット）
                var collectedLetters = gameManager.GetCollectedLetters();
                var lostLetters = gameManager.GetLostLetters();
                string formattedResultText = TextFormatter.FormatText(originalResultText, collectedLetters, lostLetters, true);
                
                // タイプライター効果で表示
                if (typewriterEffectManager != null)
                {
                    if (hasMouhitotsu)
                    {
                        // 「もうひとつ」が含まれている場合：クリッカブルワード付きタイプライター効果
                        // フォーマット後のテキストを表示用に使用、フォーマット前のテキストをパターンマッチング用に使用
                        typewriterEffectManager.StartTypewriterEffectWithClickableWord(resultContainer, formattedResultText, () =>
                        {
                            // 既にワードが見つかっている場合は、カウントダウンの開始をスキップ
                            if (wordFoundInCurrentScenario)
                            {
                                Debug.Log("既にワードが見つかっているため、カウントダウンの開始をスキップします。");
                                return;
                            }

                            // 結果テキストのタイプライター効果が完了したらカウントダウンを開始
                            if (countdownManager != null)
                            {
                                countdownManager.StartCountdown(
                                    countdownText,
                                    countdownContainer,
                                    wordGetContainer,
                                    wordFailedMessageLabel,
                                    () => {
                                        // ワードが見つかった場合の処理
                                        wordFoundInCurrentScenario = true;
                                    },
                                    () => {
                                        // カウントダウン完了時の処理
                                        // HandleChoiceを先に呼び出して、取得した文字をcollectedLettersに反映
                                        if (wordFoundInCurrentScenario && scenario != null && result != null)
                                        {
                                            gameManager.HandleChoice(result.choiceId, true);
                                            // resultを再取得
                                            result = gameManager.GetScenarioResult(scenario.id);
                                            
                                            // wordGetLabelのテキストを設定（HandleChoiceの後なので、取得した文字が反映される）
                                            if (wordGetLabel != null)
                                            {
                                                wordGetLabel.ClearClassList();
                                                if (isDarkMode)
                                                {
                                                    wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                                                    wordGetLabel.AddToClassList("word-get-dark");
                                                }
                                                else
                                                {
                                                    // ✨を画像で置き換え（取得した文字が反映された状態でフォーマット）
                                                    SetupWordGetLabelWithSparkle(wordGetContainer, wordGetLabel, GetMaskedWordGetText());
                                                    wordGetLabel.AddToClassList("word-get-success");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // ワードが見つからなかった場合
                                            if (wordGetLabel != null)
                                            {
                                                wordGetLabel.ClearClassList();
                                                // wordGetLabel.text = "残念...【もうひとつ】は出ませんでした";
                                                // wordGetLabel.AddToClassList("word-get-failed");
                                            }
                                        }
                                    },
                                    ShowBackButton
                                );
                            }
                        }, (found, pos) => {
                        if (found)
                        {
                            wordFoundInCurrentScenario = true;
                            
                            // 効果音を再生（ワードゲット数が増える時の音 + ランダムなワードゲット音）
                            if (audioManager != null)
                            {
                                audioManager.PlayWordGetIncreaseSound();
                                audioManager.PlayWordGetSound();
                            }
                            
                            // カウントダウンを停止
                            if (countdownManager != null)
                            {
                                countdownManager.NotifyWordFound();
                            }
                            
                            // カウントダウンコンテナを非表示にする
                            if (countdownContainer != null)
                            {
                                countdownContainer.style.display = DisplayStyle.None;
                            }
                            
                            // クリッカブル判定は既に完了している（wordFoundInCurrentScenario = true）
                            // HandleChoiceを呼び出して、取得した文字をcollectedLettersに反映
                            if (scenario != null && result != null)
                            {
                                gameManager.HandleChoice(result.choiceId, true);
                                // resultを再取得
                                result = gameManager.GetScenarioResult(scenario.id);
                            }
                            
                            // 綺麗な演出とともに一呼吸してから表示
                            if (wordGetEffectManager != null)
                            {
                                StartCoroutine(wordGetEffectManager.ShowWordGetWithEffect(root, isDarkMode, pos, () =>
                                {
                                    // 演出完了後の処理
                                    // wordGetLabelのテキストを設定（HandleChoiceの後なので、取得した文字が反映される）
                                    if (wordGetLabel != null)
                                    {
                                        wordGetLabel.ClearClassList();
                                        if (isDarkMode)
                                        {
                                            wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                                            wordGetLabel.AddToClassList("word-get-dark");
                                        }
                                        else
                                        {
                                            // ✨を画像で置き換え（取得した文字が反映された状態でフォーマット）
                                            SetupWordGetLabelWithSparkle(wordGetContainer, wordGetLabel, GetMaskedWordGetText());
                                            wordGetLabel.AddToClassList("word-get-success");
                                        }
                                        
                                        // フェードインとスケールアニメーション
                                        StartCoroutine(AnimateWordGetLabelFadeIn(wordGetLabel));
                                    }
                                    
                                    // 後日談を表示
                                    if (epilogueContainer != null && epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
                                    {
                                        epilogueContainer.style.display = DisplayStyle.Flex;
                                        if (typewriterEffectManager != null)
                                        {
                                            typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText, () =>
                                            {
                                                // 後日談のタイプライター効果が完了したら戻るボタンを表示
                                                ShowBackButton();
                                            });
                                        }
                                        else
                                        {
                                            ShowBackButton();
                                        }
                                    }
                                    else
                                    {
                                        ShowBackButton();
                                    }
                                }));
                            }
                            else
                            {
                                // フォールバック：元のメソッドを使用
                                // クリッカブル判定が完了している場合は、HandleChoiceを呼び出す
                                if (wordFoundInCurrentScenario && scenario != null && result != null)
                                {
                                    gameManager.HandleChoice(result.choiceId, true);
                                    // resultを再取得
                                    result = gameManager.GetScenarioResult(scenario.id);
                                }
                                StartCoroutine(ShowWordGetWithEffect(root, isDarkMode, scenario, result, epilogueContainer, epilogueLabel, pos));
                            }
            
                            // スコア表示へ光が飛んでいく演出を開始（posがクリック位置 = 【もうひとつ】の位置）
                            if (!isDarkMode && wordGetEffectManager != null)
                            {
                                StartCoroutine(wordGetEffectManager.ShowLetterGetAnimation(pos, root));
                            }
                            else if (!isDarkMode)
                            {
                                StartCoroutine(ShowLetterGetAnimation(pos));
                            }
                        }
                    }, fontSize: 18, isClickable: true, originalText: originalResultText);
                    }
                    else
                    {
                        // 「もうひとつ」が含まれていない場合：通常のタイプライター効果
                        var resultLabelForTypewriter = new Label();
                        resultLabelForTypewriter.style.fontSize = 18;
                        resultLabelForTypewriter.style.whiteSpace = WhiteSpace.Normal;
                        resultLabelForTypewriter.style.maxWidth = 800;
                        resultLabelForTypewriter.style.marginBottom = 20;
                        resultLabelForTypewriter.style.alignSelf = Align.FlexStart; // 左揃え
                        resultLabelForTypewriter.style.unityTextAlign = TextAnchor.UpperLeft; // 左揃え
                        // 明るい色を適用
                        resultLabelForTypewriter.style.color = brightTextColor;
                        resultLabelForTypewriter.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        resultContainer.Add(resultLabelForTypewriter);
                        
                        typewriterEffectManager.StartTypewriterEffect(resultLabelForTypewriter, resultText, () =>
                        {
                            // タイプライター効果が完了したら、即座に戻るボタンを表示
                            ShowBackButton();
                        });
                    }
                    
                    // resultContainer内のすべてのLabelに明るい色を適用（タイプライター効果で追加されるLabelにも適用）
                    foreach (var child in resultContainer.Children())
                    {
                        if (child is Label label)
                        {
                            label.style.color = brightTextColor;
                            label.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
                        }
                    }
                }
            }
            else if (wordFoundInCurrentScenario && epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
            {
                // 結果テキストがない場合は即座に後日談を表示（wordFoundInCurrentScenarioがtrueの場合のみ）
                if (epilogueContainer != null)
                {
                    epilogueContainer.style.display = DisplayStyle.Flex;
                }
                if (typewriterEffectManager != null)
                {
                    typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText);
                }
            }
            if (wordGetContainer != null)
            {
                wordGetContainer.style.display = DisplayStyle.None;
            }
            
            // wordGetLabelのテキストは、カウントダウンが終了した時、または「もうひとつ」をクリックした時に設定する
            // ここでは初期化のみ（クラスをクリア）
            if (wordGetLabel != null)
            {
                wordGetLabel.ClearClassList();
                wordGetLabel.text = ""; // テキストは後で設定
            }
            
            // 後日談のタイトルも更新
            var epilogueTitle = root.Q<Label>("EpilogueTitle");
            if (epilogueTitle != null)
            {
                epilogueTitle.ClearClassList();
                if (isDarkMode)
                {
                    epilogueTitle.AddToClassList("epilogue-title-dark");
                }
                else
                {
                    epilogueTitle.AddToClassList("epilogue-title");
                }
                // 明るい色を適用（クラス追加後に適用して上書き）
                epilogueTitle.style.color = brightTextColor;
                epilogueTitle.style.textShadow = new TextShadow { offset = new Vector2(1, 1), blurRadius = 2, color = new Color(0, 0, 0, 0.8f) };
            }

            // 戻るボタン（最初は非表示）
            var backButton = root.Q<Button>("BackToSelectionButton");
            if (backButton != null)
            {
                backButton.style.display = DisplayStyle.None;
                backButton.clicked += () => {
                    // 予約されているダークモードがあれば有効化
                    gameManager.ActivatePendingDarkMode();
                    ShowSelectionScreen();
                };
                // 戻るボタンに画像を適用
                Color backButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backButton, uiButtonNormalImage, backButtonTextColor);
            }

            // タイトル画面に戻るボタン（もしあれば。最初は非表示）
            var backToTitleButton = root.Q<Button>("BackToTitleButton");
            if (backToTitleButton != null)
            {
                backToTitleButton.style.display = DisplayStyle.None;
                backToTitleButton.clicked += () => {
                    // 予約されているダークモードがあれば有効化
                    gameManager.ActivatePendingDarkMode();
                    ShowTitleScreenWithFade();
                };
                // タイトルに戻るボタンに画像を適用
                Color backToTitleButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backToTitleButton, uiButtonNormalImage, backToTitleButtonTextColor);
            }

            // トランジション開始
            UpdateScoreDisplay();
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        private void HideAllScreens(bool keepBgm = false)
        {
            // 背景オーバーレイをクリーンアップ
            CleanupBackgroundOverlay();
            
            if (titleScreenDocument != null) titleScreenDocument.gameObject.SetActive(false);
            if (selectionScreenDocument != null) selectionScreenDocument.gameObject.SetActive(false);
            if (scenarioScreenDocument != null)
            {
                // シナリオ画面を閉じる時に環境音を停止
                if (audioManager != null)
                {
                    audioManager.StopAmbientSound();
                }
                scenarioScreenDocument.gameObject.SetActive(false);
            }
            if (resultScreenDocument != null) resultScreenDocument.gameObject.SetActive(false);
            if (profileScreenDocument != null) profileScreenDocument.gameObject.SetActive(false);
            if (creditsScreenDocument != null)
            {
                // エンドクレジット画面を閉じる時にスクロールとBGMを停止
                if (creditsScreenManager != null)
                {
                    creditsScreenManager.StopAutoScroll();
                }
                if (!keepBgm && audioManager != null)
                {
                    // BGMをフェードアウト
                    audioManager.FadeOutBGM(2f);
                }
                
                // シナリオ選択BGMの一時停止時刻の記録などはAudioManager側で行われる
                if (!keepBgm && audioManager != null)
                {
                    audioManager.PauseSelectionBGM();
                }
                
                creditsScreenDocument.gameObject.SetActive(false);
            }
            if (achievementsScreenDocument != null) achievementsScreenDocument.gameObject.SetActive(false);
            if (mouhitotsuScreenDocument != null) mouhitotsuScreenDocument.gameObject.SetActive(false);
        }

        public void PlaySparkleSound()
        {
            if (audioManager != null)
            {
                audioManager.PlaySparkleSound();
            }
        }

        /// <summary>
        /// 消失文字を考慮した「ワードゲット!」テキストを取得
        /// </summary>
        private string GetMaskedWordGetText()
        {
            string text = MouhitotsuWordManager.GetFormattedWord() + "ワードゲット!";
            if (gameManager == null) return text;

            // 失われた文字と取得した文字を取得
            var lostLetters = gameManager.GetLostLetters();
            var collectedLetters = gameManager.GetCollectedLetters();
            
            // 3周目でも復活した文字を表示するため、TextFormatterを使用
            text = TextFormatter.FormatMouhitotsuWord(text, collectedLetters, lostLetters, true);
            
            return text;
        }

        /// <summary>
        /// ボタンマウスオーバー時の効果音を再生
        /// </summary>
        public void PlayHoverSound()
        {
            if (audioManager != null)
            {
                audioManager.PlayHoverSound();
            }
        }

        private void UpdateScoreDisplay()
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) return;

            int currentScore = gameManager.GetScore();
            
            // ダークモードでスコアが減った場合、演出を開始
            if (gameManager != null && gameManager.IsDarkMode() && previousScore >= 0 && currentScore < previousScore)
            {
                StartCoroutine(PlayWordLostAnimation(currentScore, previousScore));
            }
            
            previousScore = currentScore;
            
            var scoreLabel = currentDocument.rootVisualElement.Q<Label>("ScoreText");
            if (scoreLabel != null && gameManager != null)
            {
                int totalScenarios = gameManager.GetScenarios().Count;
                
                // スコア表示に背景画像を適用
                if (scoreDisplayBackgroundImage != null && scoreDisplayBackgroundImage.texture != null)
                {
                    scoreLabel.style.backgroundImage = new StyleBackground(scoreDisplayBackgroundImage.texture);
                    scoreLabel.style.backgroundColor = Color.clear;
                    scoreLabel.style.paddingTop = 8;
                    scoreLabel.style.paddingBottom = 8;
                    scoreLabel.style.paddingLeft = 16;
                    scoreLabel.style.paddingRight = 16;
                }
                
                // ダークモードで失われた文字を取得
                var lostLetters = gameManager.GetLostLetters();
                string scoreText = MouhitotsuWordManager.GetFormattedWord() + "ワードゲット数";
                
                // 3周目でも復活した文字を表示するため、TextFormatterを使用
                var collectedLetters = gameManager.GetCollectedLetters();
                scoreText = TextFormatter.FormatMouhitotsuWord(scoreText, collectedLetters, lostLetters, true);
                
                scoreLabel.text = $"{scoreText}: {currentScore} / {totalScenarios}";
                
                // 異常なスコアの場合はスタイルを適用
                scoreLabel.ClearClassList();
                if (currentScore > totalScenarios || lostLetters.Count > 0)
                {
                    scoreLabel.AddToClassList("score-text-anomaly");
                }
                else
                {
                    // 選択画面以外（シナリオ、リザルト）でも適切なスタイルが適用されるようにする
                    // USSで .score-text が定義されている
                }
            }
            
            // 選択画面の場合、物語の解明度も更新
            if (currentDocument == selectionScreenDocument)
            {
                UpdateStoryProgressDisplay();
            }
        }

        /// <summary>
        /// 物語の解明度表示を更新（選択画面のみ）
        /// </summary>
        private void UpdateStoryProgressDisplay(VisualElement root = null)
        {
            // 選択画面でない場合は何もしない
            if (currentDocument != selectionScreenDocument) return;
            
            if (root == null)
            {
                if (currentDocument == null || currentDocument.rootVisualElement == null) return;
                root = currentDocument.rootVisualElement;
            }

            // 物語の解明度表示がOFFの場合は非表示にする
            if (!gameManager.GetShowStoryProgress())
            {
                var existingProgress = root.Q<Label>("StoryProgressLabel");
                if (existingProgress != null)
                {
                    existingProgress.style.display = DisplayStyle.None;
                }
                return;
            }

            // 既存の物語の解明度ラベルを探す
            var storyProgressLabel = root.Q<Label>("StoryProgressLabel");
            if (storyProgressLabel == null)
            {
                // ラベルが存在しない場合は作成
                storyProgressLabel = new Label();
                storyProgressLabel.name = "StoryProgressLabel";
                storyProgressLabel.style.fontSize = 20;
                storyProgressLabel.style.marginBottom = 20;
                storyProgressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                storyProgressLabel.style.color = new Color(0.8f, 0.8f, 1f);

                // ScoreTextの下に挿入
                var scoreLabel = root.Q<Label>("ScoreText");
                if (scoreLabel != null && scoreLabel.parent != null)
                {
                    int insertIndex = scoreLabel.parent.IndexOf(scoreLabel) + 1;
                    scoreLabel.parent.Insert(insertIndex, storyProgressLabel);
                }
            }

            // 物語の解明度を更新
            int percentage = gameManager.GetStoryProgressPercentage();
            storyProgressLabel.text = $"物語の解明度: {percentage}%";
            storyProgressLabel.style.display = DisplayStyle.Flex;
        }

        private IEnumerator AnimateScoreCountUp(Label label, string baseText, int start, int end, int total)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                int current = (int)Mathf.Lerp(start, end, elapsed / duration);
                label.text = $"{baseText}: {current} / {total}";
                yield return null;
            }
            label.text = $"{baseText}: {end} / {total}";
        }

        /// <summary>
        /// UIボタンに画像を適用するヘルパーメソッド
        /// </summary>
        private void ApplyButtonImage(Button button, Sprite image, Color textColor)
        {
            if (button == null) return;
            
            if (image != null && image.texture != null)
            {
                button.style.backgroundImage = new StyleBackground(image.texture);
                button.style.backgroundColor = Color.clear; // 背景色をクリア
            }
            
            // ボーダーを削除（背景画像を使用する場合はボーダーは不要）
            button.style.borderTopWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            
            // ボタン内のテキストの色を設定
            if (button.text != null)
            {
                button.style.color = textColor;
            }
            
            // ボタン内の子要素（Labelなど）の色も設定
            var children = button.Children();
            foreach (var child in children)
            {
                if (child is Label label)
                {
                    label.style.color = textColor;
                }
            }
            
            // ホバー効果を追加（マウスオーバー時に数ピクセルずれる）
            AddButtonHoverEffect(button);
        }
        
        /// <summary>
        /// ボタンにホバー効果を追加（マウスオーバー時に数ピクセルずれる）
        /// </summary>
        private void AddButtonHoverEffect(Button button)
        {
            if (button == null) return;
            
            const float hoverOffset = 3f; // ホバー時のずれ量（ピクセル）
            
            // マウスオーバー時：少し上にずれる（marginで実現）
            button.RegisterCallback<MouseEnterEvent>(evt => {
                button.style.marginTop = -hoverOffset;
                button.style.marginLeft = -hoverOffset;
                button.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f, TimeUnit.Second) };
            });
            
            // マウスアウト時：元の位置に戻る
            button.RegisterCallback<MouseLeaveEvent>(evt => {
                button.style.marginTop = 0;
                button.style.marginLeft = 0;
            });
            
            // クリック時：さらに下に押し込む
            button.RegisterCallback<MouseDownEvent>(evt => {
                button.style.marginTop = hoverOffset;
                button.style.marginLeft = hoverOffset;
            });
            
            // クリック解除時：ホバー位置に戻る
            button.RegisterCallback<MouseUpEvent>(evt => {
                button.style.marginTop = -hoverOffset;
                button.style.marginLeft = -hoverOffset;
            });
        }

        private void CreateScenarioButtons(VisualElement root)
        {
            var buttonContainer = root.Q<VisualElement>("ScenarioButtonContainer");
            if (buttonContainer == null) return;

            // 既存のボタンを削除
            buttonContainer.Clear();

            var scenarios = gameManager.GetScenarios();
            var lostLetters = gameManager.GetLostLetters();

            foreach (var scenario in scenarios)
            {
                // シナリオ6は最初の5つをクリアするまで表示しない
                if (scenario.id == 6)
                {
                    if (!gameManager.CanAccessScenario(6))
                    {
                        continue;
                    }
                    
                    // 解放直後の演出中はここでは作成しない（ShowScenario6UnlockAnimation内で作成される）
                    // ただし、演出が完了した後は普通に表示される必要がある。
                    // 演出中かどうかを判断するために、GameManagerのフラグを見る。
                    // 解放済みだが「まだ演出を消費していない」場合は、ここではスキップ。
                    // (演出終了後に再描画はしないが、演出内でボタンを追加している)
                    // ただし、このメソッドが演出の前に呼ばれることを想定。
                    if (!gameManager.IsScenario6Unlocked()) // まだ演出をしていないならスキップ
                    {
                        continue;
                    }
                }

                // ボタンを作成
                Button button = new Button();
                
                // グリッド用のスタイルを適用
                button.AddToClassList("scenario-button");
                
                // ボタンの内容を構造化
                var buttonContent = new VisualElement();
                buttonContent.style.flexDirection = FlexDirection.Column;
                buttonContent.style.alignItems = Align.FlexStart;
                buttonContent.style.width = Length.Percent(100);
                buttonContent.style.height = Length.Percent(100);
                buttonContent.style.flexGrow = 1;
                
                string scenarioTitleText = scenario.title;
                string scenarioDescriptionText = scenario.setup;

                // ダークモード：失われた文字を置換
                // 失われた文字を※に置き換え、取得した文字に色を付ける
                var collectedLetters = gameManager.GetCollectedLetters();
                scenarioTitleText = TextFormatter.FormatText(scenarioTitleText, collectedLetters, lostLetters, true);
                scenarioDescriptionText = TextFormatter.FormatText(scenarioDescriptionText, collectedLetters, lostLetters, true);

                // 文字色の定義
                Color normalTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                Color completedTextColor = new Color(0x1A / 255f, 0x1A / 255f, 0x1A / 255f, 1f); // #1A1A1A（黒寄り）

                var titleLabel = new Label(scenarioTitleText);
                titleLabel.style.fontSize = 20;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.whiteSpace = WhiteSpace.Normal;
                titleLabel.style.marginBottom = 5;
                buttonContent.Add(titleLabel);
                
                // シナリオの説明を追加（3行まで）
                var descriptionLabel = new Label(scenarioDescriptionText);
                descriptionLabel.style.fontSize = 14;
                descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
                descriptionLabel.style.opacity = 0.9f;
                descriptionLabel.style.maxHeight = 120; // 3行分の高さに制限（ボタンの高さに合わせて拡張）
                descriptionLabel.style.flexGrow = 1; // 利用可能なスペースを埋める
                descriptionLabel.style.overflow = Overflow.Hidden;
                buttonContent.Add(descriptionLabel);
                
                button.Add(buttonContent);
                
                bool isCompleted = gameManager.IsScenarioCompleted(scenario.id);
                bool isLocked = !gameManager.CanAccessScenario(scenario.id);

                if (isLocked)
                {
                    button.SetEnabled(false);
                    var lockLabel = new Label("🔒 ロック");
                    lockLabel.style.fontSize = 12;
                    lockLabel.style.marginTop = 5;
                    buttonContent.Add(lockLabel);
                    button.AddToClassList("scenario-button-locked");
                    // ロック状態の文字色も設定
                    titleLabel.style.color = normalTextColor;
                    descriptionLabel.style.color = normalTextColor;
                    lockLabel.style.color = normalTextColor;
                }
                else if (isCompleted)
                {
                    button.AddToClassList("scenario-button-completed");
                    // クリア後の画像を設定（9-slice対応）
                    if (scenarioButtonCompletedImage != null && scenarioButtonCompletedImage.texture != null)
                    {
                        button.style.backgroundImage = new StyleBackground(scenarioButtonCompletedImage.texture);
                        button.style.backgroundColor = Color.clear; // 背景色をクリア
                    }
                    // 完了マークを追加
                    var completedMark = new Label("✓");
                    completedMark.style.fontSize = 16;
                    completedMark.style.position = Position.Absolute;
                    completedMark.style.top = 5;
                    completedMark.style.right = 5;
                    completedMark.style.color = completedTextColor;
                    button.Add(completedMark);
                    // クリア後の文字色を設定
                    titleLabel.style.color = completedTextColor;
                    descriptionLabel.style.color = completedTextColor;
                }
                else
                {
                    button.AddToClassList("scenario-button-normal");
                    // クリア前の画像を設定（9-slice対応）
                    if (scenarioButtonNormalImage != null && scenarioButtonNormalImage.texture != null)
                    {
                        button.style.backgroundImage = new StyleBackground(scenarioButtonNormalImage.texture);
                        button.style.backgroundColor = Color.clear; // 背景色をクリア
                    }
                    // クリア前の文字色を設定
                    titleLabel.style.color = normalTextColor;
                    descriptionLabel.style.color = normalTextColor;
                }

                int scenarioId = scenario.id;
                button.clicked += () => OnScenarioSelected(scenarioId);
                
                // マウスオーバー時の音を設定
                button.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());

                buttonContainer.Add(button);
            }
        }

        private void OnScenarioSelected(int scenarioId)
        {
            gameManager.SetCurrentScenario(scenarioId);
            ShowScenarioScreen();
        }

        private void CreateChoiceButtons(VisualElement root, Scenario scenario)
        {
            var buttonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            if (buttonContainer == null) return;

            // 既存のボタンを削除
            buttonContainer.Clear();

            bool isDarkMode = gameManager.IsDarkMode();
            // ScenarioChoiceProviderを使用して選択肢を取得
            List<Choice> choices = ScenarioChoiceProvider.GetChoices(scenario, isDarkMode);

            var lostLetters = gameManager.GetLostLetters();

            foreach (var choice in choices)
            {
                // ボタンを作成
                Button button = new Button();
                
                // ダークモードの場合はダークスタイルを適用
                Color choiceTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                if (isDarkMode)
                {
                    button.AddToClassList("choice-button-dark");
                    // ダークモード用の画像を適用
                    if (uiButtonDarkImage != null && uiButtonDarkImage.texture != null)
                    {
                        button.style.backgroundImage = new StyleBackground(uiButtonDarkImage.texture);
                        button.style.backgroundColor = Color.clear;
                    }
                    choiceTextColor = Color.white; // ダークモード時は白文字
                }
                else
                {
                    button.AddToClassList("choice-button");
                    // 通常の画像を適用
                    if (uiButtonNormalImage != null && uiButtonNormalImage.texture != null)
                    {
                        button.style.backgroundImage = new StyleBackground(uiButtonNormalImage.texture);
                        button.style.backgroundColor = Color.clear;
                    }
                }
                
                string choiceLabelText = $"選択肢{choice.id}：{choice.text}";
                string previewLabelText = choice.preview;

                // 失われた文字を※に置き換え、取得した文字に色を付ける
                var collectedLetters = gameManager.GetCollectedLetters();
                choiceLabelText = TextFormatter.FormatText(choiceLabelText, collectedLetters, lostLetters, true);
                previewLabelText = TextFormatter.FormatText(previewLabelText, collectedLetters, lostLetters, true);

                // ボタンの中にテキストを配置
                var buttonText = new Label(choiceLabelText);
                buttonText.style.fontSize = 18;
                buttonText.style.whiteSpace = WhiteSpace.Normal;
                buttonText.style.unityFontStyleAndWeight = FontStyle.Bold;
                
                var previewText = new Label(previewLabelText);
                previewText.style.fontSize = 14;
                previewText.style.opacity = 0.8f;
                previewText.style.whiteSpace = WhiteSpace.Normal;
                
                // 文字色を設定（buttonTextとpreviewTextが作成された後）
                buttonText.style.color = choiceTextColor;
                previewText.style.color = choiceTextColor;

                var buttonContent = new VisualElement();
                buttonContent.style.flexDirection = FlexDirection.Column;
                buttonContent.style.alignItems = Align.FlexStart;
                buttonContent.Add(buttonText);
                buttonContent.Add(previewText);
                button.Add(buttonContent);

                int choiceId = choice.id;
                button.clicked += () => OnChoiceSelected(choiceId);
                
                // マウスオーバー時の音を設定
                button.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());

                // 最初は非表示
                button.style.display = DisplayStyle.None;
                buttonContainer.Add(button);
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromScenario");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
                // マウスオーバー時の音を設定
                backButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // 戻るボタンに画像を適用
                Color backButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backButton, uiButtonNormalImage, backButtonTextColor);
                // 最初は非表示
                backButton.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// 選択肢を順次表示するコルーチン
        /// </summary>
        private IEnumerator ShowChoicesSequentially(VisualElement root)
        {
            var buttonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            if (buttonContainer == null) yield break;

            // コンテナを表示
            buttonContainer.style.display = DisplayStyle.Flex;

            // 選択肢ボタンを一つずつ表示
            foreach (var child in buttonContainer.Children())
            {
                if (child is Button button)
                {
                    button.style.display = DisplayStyle.Flex;
                    // フェードインやアニメーションを追加することも可能ですが、
                    // まずは単純に表示を切り替えます
                    yield return new WaitForSeconds(0.5f);
                }
            }

            // すべての選択肢が表示された後に戻るボタンを表示
            var backButtonFromScenario = root.Q<Button>("BackToSelectionButtonFromScenario");
            if (backButtonFromScenario != null)
            {
                yield return new WaitForSeconds(0.3f);
                backButtonFromScenario.style.display = DisplayStyle.Flex;
                // ここでも一応予約の有効化を考慮（基本はリザルト画面経由だが）
                backButtonFromScenario.clicked += () => {
                    gameManager.ActivatePendingDarkMode();
                    ShowSelectionScreen();
                };
            }

            var backToTitleButtonFromScenario = root.Q<Button>("BackToTitleButtonFromScenario");
            if (backToTitleButtonFromScenario != null)
            {
                backToTitleButtonFromScenario.style.display = DisplayStyle.Flex;
                backToTitleButtonFromScenario.clicked += () => {
                    gameManager.ActivatePendingDarkMode();
                    ShowTitleScreenWithFade();
                };
                // タイトルに戻るボタンに画像を適用
                Color backToTitleButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backToTitleButtonFromScenario, uiButtonNormalImage, backToTitleButtonTextColor);
            }
        }

        private void OnChoiceSelected(int choiceId)
        {
            // wordFoundInCurrentScenarioフラグをhasWordとして使用
            gameManager.HandleChoice(choiceId, wordFoundInCurrentScenario);
            ShowResultScreen();
        }



        private string GetScenarioTitle(int scenarioId)
        {
            var scenarios = gameManager.GetScenarios();
            var scenario = scenarios.Find(s => s.id == scenarioId);
            return scenario != null ? scenario.title : "";
        }

        private string GetDarkModeEpilogue(int scenarioId, int choiceId)
        {
            return "【データ破損】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "のデータは完全に崩壊しました。";
        }

        private string GetDarkModeEpilogue2(int scenarioId)
        {
            return "【完全崩壊】\n" + CharacterProfileManager.GetProfile(scenarioId)?.name + "は完全にデータの欠片となって消えました。";
        }

        private void SetBackgroundImage(int scenarioId, bool isScenarioScreen)
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) return;

            int backgroundIndex = scenarioId - 1;
            
            if (backgroundIndex >= 0 && backgroundIndex < scenarioBackgrounds.Length && scenarioBackgrounds[backgroundIndex] != null)
            {
                var backgroundImage = currentDocument.rootVisualElement.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    var scenarioBg = scenarioBackgrounds[backgroundIndex];
                    backgroundImage.style.backgroundImage = new StyleBackground(scenarioBg);
                    
                    // 背景テクスチャを事前にキャッシュ
                    if (scenarioBg != null && scenarioBg.texture != null)
                    {
                        backgroundTextureCache[backgroundImage] = scenarioBg.texture;
                    }
                    
                    // ダークモード時は背景を歪ませる
                    ApplyBackgroundDistortion(backgroundImage);
                    
                    // シナリオ画面またはリザルト画面の場合、背景の明度を下げるオーバーレイを追加
                    if (isScenarioScreen || scenarioId > 0) // リザルト画面もシナリオIDが0より大きい場合
                    {
                        SetupBackgroundOverlay(backgroundImage);
                    }
                }
            }
        }
        
        /// <summary>
        /// 背景の明度を下げるオーバーレイを設定
        /// </summary>
        private void SetupBackgroundOverlay(VisualElement backgroundImage)
        {
            if (backgroundImage == null || backgroundImage.parent == null) return;
            
            // 既存のオーバーレイを削除
            if (backgroundOverlay != null)
            {
                if (backgroundOverlay.parent != null)
                {
                    backgroundOverlay.parent.Remove(backgroundOverlay);
                }
                backgroundOverlay = null;
            }
            
            // 既存のフェードコルーチンを停止
            if (backgroundOverlayFadeCoroutine != null)
            {
                StopCoroutine(backgroundOverlayFadeCoroutine);
                backgroundOverlayFadeCoroutine = null;
            }
            
            // オーバーレイを作成
            backgroundOverlay = new VisualElement();
            backgroundOverlay.name = "BackgroundOverlay";
            
            // オーバーレイのスタイル設定
            backgroundOverlay.style.position = Position.Absolute;
            backgroundOverlay.style.left = 0;
            backgroundOverlay.style.top = 0;
            backgroundOverlay.style.right = 0;
            backgroundOverlay.style.bottom = 0;
            backgroundOverlay.style.backgroundColor = Color.black;
            backgroundOverlay.style.opacity = 0; // 最初は透明
            
            // 背景画像の直後に挿入（背景画像の上、他の要素の下）
            var parent = backgroundImage.parent;
            int backgroundIndex = parent.IndexOf(backgroundImage);
            parent.Insert(backgroundIndex + 1, backgroundOverlay);
            
            // フェードインを開始
            backgroundOverlayFadeCoroutine = StartCoroutine(FadeInBackgroundOverlay());
        }
        
        /// <summary>
        /// 背景オーバーレイをフェードインで表示
        /// </summary>
        private IEnumerator FadeInBackgroundOverlay()
        {
            if (backgroundOverlay == null) yield break;
            
            float elapsed = 0f;
            float startOpacity = 0f;
            float targetOpacity = BackgroundOverlayOpacity;
            
            while (elapsed < BackgroundOverlayFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / BackgroundOverlayFadeDuration);
                float currentOpacity = Mathf.Lerp(startOpacity, targetOpacity, t);
                
                if (backgroundOverlay != null)
                {
                    backgroundOverlay.style.opacity = currentOpacity;
                }
                
                yield return null;
            }
            
            // 最終的な不透明度を設定
            if (backgroundOverlay != null)
            {
                backgroundOverlay.style.opacity = targetOpacity;
            }
            
            backgroundOverlayFadeCoroutine = null;
        }
        
        /// <summary>
        /// 背景オーバーレイをクリーンアップ
        /// </summary>
        private void CleanupBackgroundOverlay()
        {
            // フェードコルーチンを停止
            if (backgroundOverlayFadeCoroutine != null)
            {
                StopCoroutine(backgroundOverlayFadeCoroutine);
                backgroundOverlayFadeCoroutine = null;
            }
            
            // オーバーレイを削除
            if (backgroundOverlay != null)
            {
                if (backgroundOverlay.parent != null)
                {
                    backgroundOverlay.parent.Remove(backgroundOverlay);
                }
                backgroundOverlay = null;
            }
        }

        /// <summary>
        /// 背景の歪み演出を適用または解除する
        /// </summary>
        private void ApplyBackgroundDistortion(VisualElement backgroundImage)
        {
            if (distortionEffectManager != null)
            {
                distortionEffectManager.ApplyBackgroundDistortion(backgroundImage, gameManager != null && gameManager.IsDarkMode());
            }
        }



        public void ShowAchievementsScreen()
        {
            FadeOutAudioOnSceneChange();
            // シナリオ選択BGMの音量を下げる（流したまま）
            LowerSelectionBGMVolume();
            HideAllScreens();
            
            if (achievementsScreenDocument == null)
            {
                Debug.LogError("AchievementsScreenDocumentがアサインされていません！");
                return;
            }

            achievementsScreenDocument.gameObject.SetActive(true);
            currentDocument = achievementsScreenDocument;
            
            var root = achievementsScreenDocument.rootVisualElement;
            if (root == null) return;

            // 背景画像を設定（選択画面と同じ背景を使用）
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                    
                    // 背景テクスチャを事前にキャッシュ
                    if (selectionScreenBackground != null && selectionScreenBackground.texture != null)
                    {
                        backgroundTextureCache[backgroundImage] = selectionScreenBackground.texture;
                    }
                    
                    // ダークモード時は背景を歪ませる
                    ApplyBackgroundDistortion(backgroundImage);
                }
            }
            
            // スパークルアイコンを設定
            if (achievementsScreenManager != null && sparkleIcon != null)
            {
                achievementsScreenManager.SetSparkleIcon(sparkleIcon);
            }

            var achievementsContainer = root.Q<VisualElement>("AchievementsContainer");
            if (achievementsContainer == null) return;

            if (achievementsScreenManager != null)
            {
                achievementsScreenManager.CreateAchievements(achievementsContainer);
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromAchievements");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
                backButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // 戻るボタンに画像を適用
                Color backButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backButton, uiButtonNormalImage, backButtonTextColor);
            }

            // タイトル画面に戻るボタン（もしあれば）
            var backToTitleButton = root.Q<Button>("BackToTitleButtonFromAchievements");
            if (backToTitleButton != null)
            {
                backToTitleButton.clicked += ShowTitleScreenWithFade;
                backToTitleButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // タイトルに戻るボタンに画像を適用
                Color backToTitleButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backToTitleButton, uiButtonNormalImage, backToTitleButtonTextColor);
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        public void ShowMouhitotsuScreen()
        {
            FadeOutAudioOnSceneChange();
            LowerSelectionBGMVolume();
            HideAllScreens();
            
            if (mouhitotsuScreenDocument == null)
            {
                Debug.LogError("MouhitotsuScreenDocumentがアサインされていません！");
                return;
            }

            mouhitotsuScreenDocument.gameObject.SetActive(true);
            currentDocument = mouhitotsuScreenDocument;
            
            var root = mouhitotsuScreenDocument.rootVisualElement;
            if (root == null) return;

            // 背景画像を設定
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                    
                    // 背景テクスチャを事前にキャッシュ
                    if (selectionScreenBackground != null && selectionScreenBackground.texture != null)
                    {
                        backgroundTextureCache[backgroundImage] = selectionScreenBackground.texture;
                    }
                    
                    // ダークモード時は背景を歪ませる
                    ApplyBackgroundDistortion(backgroundImage);
                }
            }
            
            var mouhitotsuContainer = root.Q<VisualElement>("MouhitotsuContainer");
            if (mouhitotsuContainer != null && mouhitotsuScreenManager != null)
            {
                mouhitotsuScreenManager.SetOnChapterJumpCallback(chapterId => {
                    ShowConfirmationDialog("現在の状況が消えてしまいますがよろしいですか？", () => {
                        StartCoroutine(PerformChapterJump(chapterId));
                    });
                });
                mouhitotsuScreenManager.SetOnShowConfirmationDialogCallback((message, onConfirm) => {
                    ShowConfirmationDialog(message, onConfirm);
                });
                mouhitotsuScreenManager.CreateRetryButtons(root);
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromMouhitotsu");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
                backButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // 戻るボタンに画像を適用
                Color backButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backButton, uiButtonNormalImage, backButtonTextColor);
            }

            // タイトル画面に戻るボタン（もしあれば）
            var backToTitleButton = root.Q<Button>("BackToTitleButtonFromMouhitotsu");
            if (backToTitleButton != null)
            {
                backToTitleButton.clicked += ShowTitleScreenWithFade;
                backToTitleButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                // タイトルに戻るボタンに画像を適用
                Color backToTitleButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backToTitleButton, uiButtonNormalImage, backToTitleButtonTextColor);
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }


        /// <summary>
        /// 特別版エンドクレジットへの暗転演出
        /// </summary>
        private IEnumerator ShowSpecialCreditsTransition()
        {
            FadeOutAudioOnSceneChange();
            FadeOutAmbientSoundForResult();
            HideAllScreens();

            // 演出用の真っ黒なオーバーレイを作成
            if (creditsScreenDocument == null)
            {
                Debug.LogError("CreditsScreenDocumentがアサインされていません！演出をスキップします。");
                ShowCreditsScreen(isSpecial: true);
                yield break;
            }

            creditsScreenDocument.gameObject.SetActive(true);
            var root = creditsScreenDocument.rootVisualElement;
            
            if (root == null)
            {
                Debug.LogError("rootVisualElementが取得できません！演出をスキップします。");
                ShowCreditsScreen(isSpecial: true);
                yield break;
            }

            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = Color.black;
            overlay.style.opacity = 0f;
            root.Add(overlay);

            // フェードイン（黒画面へ）
            float fadeDuration = 1.5f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                overlay.style.opacity = Mathf.Min(elapsed / fadeDuration, 1.0f);
                yield return null;
            }

            yield return new WaitForSeconds(1.0f);

            // クレジット画面を表示
            ShowCreditsScreen(isSpecial: true);
            
            // オーバーレイを削除
            root.Remove(overlay);
        }

        public void ShowCreditsScreen(bool isSpecial = false)
        {
            HideAllScreens();
            
            if (creditsScreenDocument == null)
            {
                Debug.LogError("CreditsScreenDocumentがアサインされていません！");
                return;
            }

            creditsScreenDocument.gameObject.SetActive(true);
            currentDocument = creditsScreenDocument;
            
            var root = creditsScreenDocument.rootVisualElement;
            if (root == null) return;

            // 背景画像を設定（選択画面と同じ背景を使用）
            if (selectionScreenBackground != null)
            {
                var backgroundImage = root.Q<VisualElement>("BackgroundImage");
                if (backgroundImage != null)
                {
                    backgroundImage.style.backgroundImage = new StyleBackground(selectionScreenBackground);
                    
                    // 背景テクスチャを事前にキャッシュ
                    if (selectionScreenBackground != null && selectionScreenBackground.texture != null)
                    {
                        backgroundTextureCache[backgroundImage] = selectionScreenBackground.texture;
                    }
                    
                    // ダークモード時は背景を歪ませる
                    ApplyBackgroundDistortion(backgroundImage);
                }
            }

            var creditsContent = root.Q<VisualElement>("CreditsContent");
            var creditsScrollView = root.Q<ScrollView>("CreditsScrollView");
            if (creditsContent == null || creditsScrollView == null) return;

            if (creditsScreenManager != null)
            {
                creditsScreenManager.CreateCredits(creditsContent, creditsScrollView, isSpecial, () => {
                    // 特別版クレジット終了後の処理（「もうひとつ」の世界へボタンが押された時）
                    StartCoroutine(EndGameRoutine());
                });
            }
            
            // BGMを再生
            audioManager.PlayCreditsBGM();

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromCredits");
            if (backButton != null)
            {
                // 特別版の場合は非表示
                backButton.style.display = isSpecial ? DisplayStyle.None : DisplayStyle.Flex;
                backButton.clicked += ShowSelectionScreen;
                // 戻るボタンに画像を適用
                Color backButtonTextColor = new Color(0x2B / 255f, 0x1F / 255f, 0x18 / 255f, 1f); // #2B1F18（濃茶）
                ApplyButtonImage(backButton, uiButtonNormalImage, backButtonTextColor);
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        /// <summary>
        /// ゲーム終了演出（画面暗転、音楽のみ）
        /// </summary>
        private IEnumerator EndGameRoutine()
        {
            // すべてのスクリーンを隠す（BGMは維持）
            HideAllScreens(true);
            
            // 真っ黒な画面を作成
            if (creditsScreenDocument == null)
            {
                Debug.LogError("CreditsScreenDocumentがアサインされていません！");
                yield break;
            }

            // クレジット画面をアクティブにして、rootを取得できるようにする
            creditsScreenDocument.gameObject.SetActive(true);
            var root = creditsScreenDocument.rootVisualElement;
            
            if (root == null)
            {
                Debug.LogError("rootVisualElementが取得できません！");
                yield break;
            }

            var blackOverlay = new VisualElement();
            blackOverlay.style.position = Position.Absolute;
            blackOverlay.style.left = 0;
            blackOverlay.style.top = 0;
            blackOverlay.style.right = 0;
            blackOverlay.style.bottom = 0;
            blackOverlay.style.backgroundColor = Color.black;
            blackOverlay.style.justifyContent = Justify.Center;
            blackOverlay.style.alignItems = Align.Center;
            root.Add(blackOverlay);
            
            // chapter E の場合は最後に「（おや？）」を表示
            if (gameManager.IsThirdLoop() && gameManager.GetScore() >= 7)
            {
                var oyaLabel = new Label("（おや？）");
                oyaLabel.style.color = Color.white;
                oyaLabel.style.fontSize = 24;
                blackOverlay.Add(oyaLabel);
            }
            
            // 音楽だけが流れている状態
            Debug.Log("[UIManager] ゲーム終了。暗転状態で音楽のみ再生中。");
            
            // クリア日時と時間を記録
            gameManager.SetGameEndTime(System.DateTime.Now);

            // 30秒待機してから進捗度を表示
            yield return new WaitForSeconds(30f);

            int percentage = gameManager.GetStoryProgressPercentage();

            var progressLabel = new Label($"物語の解明度: {percentage}%");
            progressLabel.name = "EndGameProgressLabel";
            progressLabel.style.position = Position.Absolute;
            progressLabel.style.right = 20;
            progressLabel.style.bottom = 20;
            progressLabel.style.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 薄く表示
            progressLabel.style.fontSize = 14;
            
            // クリッカブルにする設定
            progressLabel.pickingMode = PickingMode.Position;
            progressLabel.RegisterCallback<ClickEvent>(evt => {
                string playTime = gameManager.GetPlayTimeDisplay();
                string clearDate = gameManager.GetGameEndTime().ToString("yyyy/MM/dd HH:mm:ss");
                Debug.Log($"[EndGame] もうおしまいです。もうひとつのゲームを追いかけてみてください (クリア時間: {playTime}, クリア日時: {clearDate})");
            });

            blackOverlay.Add(progressLabel);

            // 徐々に表示
            float fadeDuration = 3.0f;
            float elapsed = 0f;
            progressLabel.style.opacity = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                progressLabel.style.opacity = elapsed / fadeDuration;
                yield return null;
            }
            progressLabel.style.opacity = 1f;
        }


        /// <summary>
        /// 戻るボタンを表示
        /// </summary>
        private void ShowBackButton()
        {
            VisualElement root = null;
            if (resultScreenDocument != null && resultScreenDocument.gameObject.activeSelf)
            {
                root = resultScreenDocument.rootVisualElement;
            }
            
            if (root != null)
            {
                var backButton = root.Q<Button>("BackToSelectionButton");
                if (backButton != null)
                {
                    backButton.style.display = DisplayStyle.Flex;
                }

                var backToTitleButton = root.Q<Button>("BackToTitleButton");
                if (backToTitleButton != null)
                {
                    backToTitleButton.style.display = DisplayStyle.Flex;
                }
            }
        }

        /// <summary>
        /// シェイクアニメーション
        /// </summary>
        private IEnumerator ShakeAnimation(Label label)
        {
            if (shakeAnimationManager != null)
            {
                yield return StartCoroutine(shakeAnimationManager.ShakeAnimation(label));
            }
        }

        /// <summary>
        /// ワードゲットラベルのフェードインアニメーション
        /// </summary>
        private IEnumerator AnimateWordGetLabelFadeIn(Label wordGetLabel)
        {
            if (wordGetLabel == null) yield break;
            
            wordGetLabel.style.opacity = 0f;
            wordGetLabel.style.scale = new Scale(new Vector2(0.8f, 0.8f));
            
            float fadeDuration = 0.5f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                wordGetLabel.style.opacity = t;
                float scale = Mathf.Lerp(0.8f, 1f, t);
                wordGetLabel.style.scale = new Scale(new Vector2(scale, scale));
                yield return null;
            }
            
            wordGetLabel.style.opacity = 1f;
            wordGetLabel.style.scale = new Scale(new Vector2(1f, 1f));
        }
        
        /// <summary>
        /// ワードゲット時の綺麗な演出を表示
        /// </summary>
        private IEnumerator ShowWordGetWithEffect(VisualElement root, bool isDarkMode, Scenario scenario, ScenarioResult result, VisualElement epilogueContainer, Label epilogueLabel, Vector2 clickPosition = default)
        {
            // 演出用のオーバーレイを作成（生成り系に変更）
            var effectOverlay = new VisualElement();
            effectOverlay.style.position = Position.Absolute;
            effectOverlay.style.left = 0;
            effectOverlay.style.top = 0;
            effectOverlay.style.right = 0;
            effectOverlay.style.bottom = 0;
            effectOverlay.style.backgroundColor = new Color(0.93f, 0.84f, 0.71f, 0f); // 生成り系
            
            // clickPositionが指定されていない場合のみ中央揃えにする
            if (clickPosition == default)
            {
                effectOverlay.style.justifyContent = Justify.Center;
                effectOverlay.style.alignItems = Align.Center;
            }
            
            root.Add(effectOverlay);
            
            // 光るエフェクト（円形のグラデーション風）
            var glowEffect = new VisualElement();
            glowEffect.style.width = 200f;
            glowEffect.style.height = 200f;
            // 円形にするため、すべての角に同じ値を設定
            float borderRadius = 100f;
            glowEffect.style.borderTopLeftRadius = borderRadius;
            glowEffect.style.borderTopRightRadius = borderRadius;
            glowEffect.style.borderBottomLeftRadius = borderRadius;
            glowEffect.style.borderBottomRightRadius = borderRadius;
            glowEffect.style.backgroundColor = new Color(1f, 0.84f, 0f, 0f); // 黄色
            glowEffect.style.position = Position.Absolute;

            // クリック位置が指定されている場合は、その位置にエフェクトを表示
            if (clickPosition != default)
            {
                // UI Toolkitの座標系にあわせる
                // effectOverlayが(0,0,root.width,root.height)なので、その中での相対座標として設定
                glowEffect.style.left = clickPosition.x - 100f;
                glowEffect.style.top = clickPosition.y - 100f;
            }

            effectOverlay.Add(glowEffect);
            
            // エフェクトアニメーション（拡大してフェードアウト）
            float effectDuration = 1.0f;
            float elapsed = 0f;
            float startScale = 0.5f;
            float endScale = 2.0f;
            
            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / effectDuration;
                
                // スケールアニメーション
                float currentScale = Mathf.Lerp(startScale, endScale, t);
                float currentWidth = 200f * currentScale;
                float currentHeight = 200f * currentScale;
                glowEffect.style.width = currentWidth;
                glowEffect.style.height = currentHeight;

                // 位置の調整（中心を維持するため）
                if (clickPosition != default)
                {
                    glowEffect.style.left = clickPosition.x - (currentWidth / 2f);
                    glowEffect.style.top = clickPosition.y - (currentHeight / 2f);
                }

                // 円形を維持するため、すべての角に同じ値を設定
                float currentBorderRadius = (currentWidth / 2f);
                glowEffect.style.borderTopLeftRadius = currentBorderRadius;
                glowEffect.style.borderTopRightRadius = currentBorderRadius;
                glowEffect.style.borderBottomLeftRadius = currentBorderRadius;
                glowEffect.style.borderBottomRightRadius = currentBorderRadius;
                
                // フェードアウト
                float alpha = Mathf.Lerp(0.8f, 0f, t);
                glowEffect.style.backgroundColor = new Color(1f, 0.84f, 0f, alpha);
                
                // 背景も少し明るく（生成り系に変更）
                float bgAlpha = Mathf.Lerp(0f, 0.3f, Mathf.Sin(t * Mathf.PI));
                effectOverlay.style.backgroundColor = new Color(0.93f, 0.84f, 0.71f, bgAlpha); // 生成り系
                
                yield return null;
            }
            
            // エフェクトを削除
            root.Remove(effectOverlay);
            
            // 一呼吸（0.5秒待つ）
            yield return new WaitForSeconds(0.5f);
            
            // ワードゲット表示を表示
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            if (wordGetContainer != null)
            {
                wordGetContainer.style.display = DisplayStyle.Flex;
            }
            
            // 注意: ShowWordGetWithEffectメソッドは、クリッカブル判定の後に呼ばれる場合と直接呼ばれる場合がある
            // クリッカブル判定が完了している場合は、呼び出し側でHandleChoiceを呼び出す必要がある
            // このメソッド内では、HandleChoiceを呼び出さない（呼び出し側で処理する）
            
            // wordGetLabelのテキストを設定
            var wordGetLabel = root.Q<Label>("WordGetText");
            if (wordGetLabel != null)
            {
                wordGetLabel.ClearClassList();
                if (isDarkMode)
                {
                    wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                    wordGetLabel.AddToClassList("word-get-dark");
                }
                else
                {
                    // ✨を画像で置き換え（取得した文字が反映された状態でフォーマット）
                    SetupWordGetLabelWithSparkle(wordGetContainer, wordGetLabel, GetMaskedWordGetText());
                    wordGetLabel.AddToClassList("word-get-success");
                }
                
                // フェードインとスケールアニメーション
                wordGetLabel.style.opacity = 0f;
                wordGetLabel.style.scale = new Scale(new Vector2(0.8f, 0.8f));
                
                float fadeDuration = 0.5f;
                elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / fadeDuration;
                    wordGetLabel.style.opacity = t;
                    float scale = Mathf.Lerp(0.8f, 1f, t);
                    wordGetLabel.style.scale = new Scale(new Vector2(scale, scale));
                    yield return null;
                }
                
                wordGetLabel.style.opacity = 1f;
                wordGetLabel.style.scale = new Scale(new Vector2(1f, 1f));
            }
            
            // 後日談を表示
            if (epilogueContainer != null)
            {
                epilogueContainer.style.display = DisplayStyle.Flex;
                
                // 後日談テキストを取得
                string epilogueText = "";
                if (result != null)
                {
                    if (isDarkMode)
                    {
                        // シナリオごとのダークモードエピローグ（ShowResultScreenと同じロジック）
                        epilogueText = scenario.id switch
                        {
                            1 => result.choiceId == 1
                                ? "【データ破損】もも子のデータは完全に崩壊しました。\n写真から人物の姿が消え、存在が不安定になりました。\n「も」という文字が消失し、探偵事務所のデータも歪み始めています。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「も...もど...もどれない...」\n\n【エンド：文字の消失】"
                                : "【システムエラー】データの修復を試みましたが、失敗しました。\nもも子のデータは完全に破損し、修復不可能な状態です。\n写真の人物は、データの欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                            2 => result.choiceId == 1
                                ? "【データ破損】うみシェフのデータは完全に崩壊しました。\nレストランのメニューが文字化けし、料理のデータが読み込めなくなりました。\n「う」という文字が消失し、レストランの存在が不安定になっています。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「う...うみ...うみへ...」\n\n【エンド：文字の消失】"
                                : "【システムエラー】システムエラーの報告を行いましたが、無意味でした。\nうみシェフのデータは完全に破損し、レストランは機能しなくなりました。\n料理のデータが欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                            3 => result.choiceId == 1
                                ? "【データ破損】ひろのデータは完全に崩壊しました。\n過去の記憶が歪み、タイムカプセルのデータが欠損しています。\n「ひ」という文字が消失し、友情の記憶が失われました。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「ひ...ひろ...ひろが...」\n\n【エンド：文字の消失】"
                                : "【システムエラー】データの修復を試みましたが、失敗しました。\nひろのデータは完全に破損し、過去の記憶が消えてしまいました。\nタイムカプセルは、データの欠片となって崩壊しました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                            4 => result.choiceId == 1
                                ? "【データ破損】とおる試験官のデータは完全に崩壊しました。\n魔法のコードがエラーを起こし、魔法学校のシステムが停止しました。\n「と」という文字が消失し、魔法のデータが読み込めなくなりました。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「と...とおる...とおるが...」\n\n【エンド：文字の消失】"
                                : "【システムエラー】システムの整合性を確認しましたが、手遅れでした。\nとおる試験官のデータは完全に破損し、魔法学校は機能しなくなりました。\n呪文のコードが欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                            5 => result.choiceId == 1
                                ? "【データ破損】つばさのデータは完全に崩壊しました。\nパズルのピースが永遠に足りず、完成することができなくなりました。\n「つ」という文字が消失し、愛の記憶が消えつつあります。\n\nあなたの異常な行動が、世界の一部を破壊してしまいました。\n「つ...つばさ...つばさが...」\n\n【エンド：文字の消失】"
                                : "【システムエラー】完成できないことに気づきましたが、時既に遅しでした。\nつばさのデータは完全に破損し、パズルは永遠に完成できなくなりました。\n愛の記憶が欠片となって消えていきました。\n\n「もう...戻れない...」\n\n【エンド：修復不可能】",
                            6 => result.choiceId == 1
                                ? "世界は完全に崩壊しました。\nシミュレーションの整合性は失われ、修復不可能な状態です。\n\n登場人物たちは、データの欠片となって消えていきました。\nもも子、うみ、ひろ、とおる、つばさ...\nすべてが、あなたの異常な行動の結果です。\n\nあなたは、空っぽの世界に一人取り残されました。\n「もう...戻れない...」\n\n【エンド：世界崩壊】"
                                : "あなたは、世界の真実を知ってしまいました。\nこの世界は、シミュレーションだったのです。\n\nしかし、あなたの異常な行動が、世界を破壊してしまいました。\n登場人物たちは、バグによって歪んだ姿となっています。\n\nもも子は「も」という文字を失い、\nうみは「う」という文字を失い、\nひろは「ひ」という文字を失い、\nとおるは「と」という文字を失い、\nつばさは「つ」という文字を失いました。\n\n「もうひとつ」という言葉は、永遠に失われました。\n\n【エンド：言葉の消失】",
                            _ => "【データ破損】"
                        };
                    }
                    else
                    {
                        epilogueText = result.epilogue;
                    }
                }
                
                // 取得した文字に色を付け、失われた文字を伏字化
                var collectedLetters = gameManager.GetCollectedLetters();
                var lostLetters = gameManager.GetLostLetters();
                epilogueText = TextFormatter.FormatText(epilogueText, collectedLetters, lostLetters, true);
                
                // 後日談のタイプライター効果を開始
                if (epilogueLabel != null && !string.IsNullOrEmpty(epilogueText))
                {
                    if (typewriterEffectManager != null)
                    {
                        typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText, () =>
                        {
                            // 後日談のタイプライター効果が完了したら戻るボタンを表示
                            ShowBackButton();
                        });
                    }
                    else
                    {
                        ShowBackButton();
                    }
                }
                else
                {
                    ShowBackButton();
                }
            }
            else
            {
                ShowBackButton();
            }
        }

        /// <summary>
        /// ダークモードでワードが奪われる演出（文字が消えていくアニメーション）
        /// </summary>
        private System.Collections.IEnumerator PlayWordLostAnimation(int newScore, int oldScore)
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) yield break;
            if (audioManager == null) yield break;
            
            var scoreLabel = currentDocument.rootVisualElement.Q<Label>("ScoreText");
            if (scoreLabel == null) yield break;
            
            // 逆再生の効果音を再生
            audioManager.PlayWordGetSoundReversed();
            
            // 文字が奪われていく演出
            // 「もうひとつ」の各文字を順番に消していく
            char[] allLetters = MouhitotsuWordManager.GetAllLetters();
            string[] characters = new string[allLetters.Length];
            for (int i = 0; i < allLetters.Length; i++)
            {
                characters[i] = allLetters[i].ToString();
            }
            string baseText = $"{MouhitotsuWordManager.GetFormattedWord()}ワードゲット数";
            
            // 失われた文字を取得
            var lostLetters = gameManager.GetLostLetters();
            
            // 各文字を順番に消していく（0.15秒間隔）
            for (int i = 0; i < characters.Length; i++)
            {
                // 既に失われている文字はスキップ
                if (lostLetters.Contains(characters[i][0]))
                {
                    continue;
                }
                
                // 文字を「※」に置き換え
                baseText = baseText.Replace(characters[i], "※");
                
                // スコア表示を更新（徐々に減らしていく）
                int totalScenarios = gameManager.GetScenarios().Count;
                int displayScore = oldScore - (i + 1);
                scoreLabel.text = $"{baseText}: {displayScore} / {totalScenarios}";
                
                // 文字が消えるアニメーション（スケールダウン + 揺れ）
                // UI Toolkitではstyle.translateとstyle.scaleを使用
                float shakeDuration = 0.15f;
                float shakeAmount = 3f;
                float originalScale = 1.0f;
                float elapsed = 0f;
                
                while (elapsed < shakeDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / shakeDuration;
                    
                    // 揺れ効果
                    float offsetX = Mathf.Sin(progress * Mathf.PI * 4) * shakeAmount * (1f - progress);
                    float offsetY = Mathf.Cos(progress * Mathf.PI * 4) * shakeAmount * (1f - progress);
                    scoreLabel.style.translate = new StyleTranslate(new Translate(offsetX, offsetY));
                    
                    // スケールダウン効果
                    float scale = Mathf.Lerp(originalScale, 0.95f, progress);
                    scoreLabel.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
                    
                    yield return null;
                }
                
                // 元の位置とスケールに戻す
                scoreLabel.style.translate = new StyleTranslate(new Translate(0, 0));
                scoreLabel.style.scale = new StyleScale(new Scale(Vector3.one));
                
                // 次の文字まで待機
                yield return new WaitForSeconds(0.1f);
            }
            
            // 最終的なスコア表示を更新（UpdateScoreDisplayが呼ばれるので、ここでは不要かもしれないが念のため）
        }

        /// <summary>
        /// 「もうひとつ」をゲットした時の効果音を再生（複数からランダムに選択）
        /// </summary>
        private void PlayWordGetSound()
        {
            if (audioManager != null)
            {
                audioManager.PlayWordGetSound();
            }
        }
        
        
        /// <summary>
        /// シーン切り替え時にオーディオをフェードアウト（効果音用）
        /// </summary>
        private void FadeOutAudioOnSceneChange()
        {
            if (audioManager != null)
            {
                audioManager.FadeOutAudioOnSceneChange();
            }
        }
        
        
        /// <summary>
        /// シナリオ選択BGMを開始（フェードイン）
        /// </summary>
        private void StartSelectionBGM()
        {
            if (audioManager != null)
            {
                audioManager.StartSelectionBGM();
            }
        }
        
        /// <summary>
        /// シナリオ選択BGMを一時停止（フェードアウトして時刻を記録）
        /// </summary>
        private void PauseSelectionBGM()
        {
            if (audioManager != null)
            {
                audioManager.PauseSelectionBGM();
            }
        }
        
        
        /// <summary>
        /// シナリオ選択BGMの音量を下げる（プロフィール/実績画面用）
        /// 通常時はローパスフィルターを適用し、ダークモード時はピッチを下げる
        /// </summary>
        private void LowerSelectionBGMVolume()
        {
            if (audioManager != null)
            {
                audioManager.LowerSelectionBGMVolume();
            }
        }

        
        /// <summary>
        /// シナリオの環境音を開始
        /// </summary>
        private void StartAmbientSound(int scenarioId)
        {
            if (audioManager != null)
            {
                audioManager.StartAmbientSound(scenarioId);
            }
        }
        
        /// <summary>
        /// 環境音を停止
        /// </summary>
        private void StopAmbientSound()
        {
            if (audioManager != null)
            {
                audioManager.StopAmbientSound();
            }
        }
        
        /// <summary>
        /// 環境音をフェードアウト
        /// </summary>
        private void FadeOutAmbientSound()
        {
            if (audioManager != null)
            {
                audioManager.FadeOutAmbientSound();
            }
        }
        
        /// <summary>
        /// 環境音を結果画面用に長めにフェードアウト
        /// </summary>
        private void FadeOutAmbientSoundForResult()
        {
            if (audioManager != null)
            {
                audioManager.FadeOutAmbientSoundForResult();
            }
        }
        
        
        /// <summary>
        /// ボタンにアイコンとテキストを設定（絵文字の代替）
        /// </summary>
        private void SetupButtonWithIcon(Button button, Sprite icon, string text)
        {
            if (button == null) return;
            
            // 既存の内容をクリア
            button.Clear();
            
            // ボタンのテキストを空にする
            button.text = "";
            
            // 水平レイアウトコンテナを作成
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            container.style.flexGrow = 1;
            container.pickingMode = PickingMode.Ignore;
            
            // アイコンを追加（画像が設定されている場合）
            if (icon != null)
            {
                var iconImage = new Image();
                iconImage.sprite = icon;
                iconImage.style.width = 32f; // 少し大きく
                iconImage.style.height = 32f;
                if (!string.IsNullOrEmpty(text))
                {
                    iconImage.style.marginRight = 8f;
                }
                iconImage.pickingMode = PickingMode.Ignore;
                container.Add(iconImage);
            }
            
            // テキストラベルを追加
            if (!string.IsNullOrEmpty(text))
            {
                var label = new Label(text);
                label.style.fontSize = 16f;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                label.pickingMode = PickingMode.Ignore;
                container.Add(label);
            }
            
            // コンテナをボタンに追加
            button.Add(container);
        }
        
        /// <summary>
        /// ワードゲットラベルにスパークルアイコンを追加
        /// </summary>
        private void SetupWordGetLabelWithSparkle(VisualElement container, Label label, string text)
        {
            if (container == null || label == null) return;
            
            // コンテナをクリア
            container.Clear();
            
            // 水平レイアウトコンテナを作成
            var horizontalContainer = new VisualElement();
            horizontalContainer.style.flexDirection = FlexDirection.Row;
            horizontalContainer.style.alignItems = Align.Center;
            horizontalContainer.style.justifyContent = Justify.Center;
            horizontalContainer.style.width = Length.Percent(100);
            
            // 左側のスパークルアイコン
            if (sparkleIcon != null)
            {
                var leftSparkle = new Image();
                leftSparkle.sprite = sparkleIcon;
                leftSparkle.style.width = 24f;
                leftSparkle.style.height = 24f;
                leftSparkle.style.marginRight = 8f;
                leftSparkle.RegisterCallback<ClickEvent>(evt => PlaySparkleSound());
                horizontalContainer.Add(leftSparkle);
            }
            
            // テキストラベル
            label.text = text;
            horizontalContainer.Add(label);
            
            // 右側のスパークルアイコン
            if (sparkleIcon != null)
            {
                var rightSparkle = new Image();
                rightSparkle.sprite = sparkleIcon;
                rightSparkle.style.width = 24f;
                rightSparkle.style.height = 24f;
                rightSparkle.style.marginLeft = 8f;
                rightSparkle.RegisterCallback<ClickEvent>(evt => PlaySparkleSound());
                horizontalContainer.Add(rightSparkle);
            }
            
            container.Add(horizontalContainer);
        }

        /// <summary>
        /// ワード取得時に、結果画面からスコア表示へ光が飛んでいく演出
        /// </summary>
        private IEnumerator ShowLetterGetAnimation(Vector2 startPos)
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) yield break;
            var root = currentDocument.rootVisualElement;

            // スコア表示（ScoreText）を探す
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel == null) yield break;

            // 座標確定待ち
            yield return null;
            Vector2 endPos = scoreLabel.worldBound.center;

            // 演出用コンテナ
            var effectContainer = new VisualElement();
            effectContainer.style.position = Position.Absolute;
            effectContainer.style.left = 0;
            effectContainer.style.top = 0;
            effectContainer.style.right = 0;
            effectContainer.style.bottom = 0;
            effectContainer.pickingMode = PickingMode.Ignore;
            root.Add(effectContainer);

            // 光の粒子演出
            int particleCount = 10;
            List<VisualElement> particles = new List<VisualElement>();
            for (int i = 0; i < particleCount; i++)
            {
                var p = new VisualElement();
                p.style.position = Position.Absolute;
                p.style.width = 8;
                p.style.height = 8;
                p.style.backgroundColor = Color.white;
                p.style.borderTopLeftRadius = 4;
                p.style.borderTopRightRadius = 4;
                p.style.borderBottomLeftRadius = 4;
                p.style.borderBottomRightRadius = 4;
                p.style.left = startPos.x;
                p.style.top = startPos.y;
                effectContainer.Add(p);
                particles.Add(p);
            }

            audioManager.PlaySparkleSound();

            float duration = 0.8f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easeT = Mathf.SmoothStep(0, 1, t);

                for (int i = 0; i < particleCount; i++)
                {
                    float angle = (i / (float)particleCount) * Mathf.PI * 2 + (t * 5f);
                    float spread = (1 - t) * 30f;
                    Vector2 currentPos = Vector2.Lerp(startPos, endPos, easeT);
                    particles[i].style.left = currentPos.x + Mathf.Cos(angle) * spread;
                    particles[i].style.top = currentPos.y + Mathf.Sin(angle) * spread;
                    particles[i].style.opacity = 1.0f - (t * 0.3f);
                }
                yield return null;
            }

            // 到着時の小さなフラッシュ
            var flash = new VisualElement();
            flash.style.position = Position.Absolute;
            flash.style.left = endPos.x - 20;
            flash.style.top = endPos.y - 20;
            flash.style.width = 40;
            flash.style.height = 40;
            flash.style.backgroundColor = Color.white;
            flash.style.borderTopLeftRadius = 20;
            flash.style.borderTopRightRadius = 20;
            flash.style.borderBottomLeftRadius = 20;
            flash.style.borderBottomRightRadius = 20;
            effectContainer.Add(flash);

            float flashDuration = 0.2f;
            elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                flash.style.opacity = 1.0f - (elapsed / flashDuration);
                flash.style.scale = new StyleScale(new Scale(Vector3.one * (1.0f + elapsed * 5f)));
                yield return null;
            }

            root.Remove(effectContainer);
            
            // スコア表示の更新（カウントアップ開始）
            UpdateScoreDisplay();
        }
        
        /// <summary>
        /// シナリオのsetupテキストから動物名を抽出（保存された値を使用）
        /// </summary>
        /// <param name="setupText">シナリオのsetupテキスト</param>
        /// <returns>抽出された動物名。見つからない場合は保存された値を使用</returns>
        private string ExtractAnimalNameFromSetup(string setupText)
        {
            // まず保存された値を取得
            if (gameManager != null)
            {
                string savedName = gameManager.GetScenarioRandomData(4, "animalName");
                if (!string.IsNullOrEmpty(savedName))
                {
                    return savedName;
                }
            }
            
            // 保存されていない場合はテキストから抽出を試みる
            if (string.IsNullOrEmpty(setupText))
            {
                return "";
            }
            
            // 「試験官：「{animalName}を出現させなさい」」の形式から動物名を抽出
            int startIndex = setupText.IndexOf("「");
            if (startIndex < 0) return "";
            
            int endIndex = setupText.IndexOf("を出現させなさい", startIndex);
            if (endIndex < 0) return "";
            
            // 「の後の文字列を取得
            string animalName = setupText.Substring(startIndex + 1, endIndex - startIndex - 1);
            
            // すべての動物名リストと照合して、正確な動物名を返す
            var allAnimalNames = AnimalNameManager.GetAllAnimalNames();
            foreach (string name in allAnimalNames)
            {
                if (animalName.Contains(name))
                {
                    return name;
                }
            }
            
            // 見つからない場合は抽出した文字列をそのまま返す
            return animalName.Trim();
        }
    }
}

