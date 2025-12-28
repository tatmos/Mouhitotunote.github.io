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
        [Header("UI Documents")]
        [SerializeField] private UIDocument titleScreenDocument;
        [SerializeField] private UIDocument selectionScreenDocument;
        [SerializeField] private UIDocument scenarioScreenDocument;
        [SerializeField] private UIDocument resultScreenDocument;
        [SerializeField] private UIDocument profileScreenDocument;
        [SerializeField] private UIDocument creditsScreenDocument;
        [SerializeField] private UIDocument achievementsScreenDocument;

        [Header("UXML Files")]
        [SerializeField] private VisualTreeAsset selectionScreenUXML;
        [SerializeField] private VisualTreeAsset scenarioScreenUXML;
        [SerializeField] private VisualTreeAsset resultScreenUXML;
        [SerializeField] private VisualTreeAsset profileScreenUXML;
        [SerializeField] private VisualTreeAsset creditsScreenUXML;
        [SerializeField] private VisualTreeAsset achievementsScreenUXML;

        [Header("Background Images")]
        [SerializeField] private Sprite[] scenarioBackgrounds = new Sprite[6];
        [SerializeField] private Sprite selectionScreenBackground;
        [SerializeField] private Sprite profileScreenBackground;
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] wordGetSounds; // 「もうひとつ」をゲットした時の効果音（複数からランダムに選択）
        [SerializeField] private AudioClip creditsBGM; // エンドクレジットBGM
        [SerializeField] private AudioClip selectionBGM; // シナリオ選択画面BGM
        [SerializeField] private AudioClip typewriterSound; // タイプライター文字表示時の効果音
        [SerializeField] private AudioClip sparkleSound; // スパークルアイコンクリック時の効果音（「きらん！」）
        [SerializeField] private AudioClip buttonHoverSound; // ボタンにマウスオーバーした時の効果音（「ぱっ」）
        [SerializeField] private AudioClip[] ambientSounds; // 各シナリオの環境音（インデックス0=シナリオ1, 1=シナリオ2, ...）
        
        [Header("Emoji Icons (for Web compatibility)")]
        [SerializeField] private Sprite creditsIcon; // エンドクレジット用のアイコン（🎬の代替）
        [SerializeField] private Sprite achievementsIcon; // 実績用のアイコン（🏆の代替）
        [SerializeField] private Sprite clockIcon; // カウントダウン用のアイコン（⏰の代替）
        [SerializeField] private Sprite sparkleIcon; // スパークル用のアイコン（✨の代替）

        private GameManager gameManager;
        private UIDocument currentDocument;
        private List<GameObject> currentButtons = new List<GameObject>();
        
        // マネージャークラスのインスタンス
        private TypewriterEffectManager typewriterEffectManager;
        private CountdownManager countdownManager;
        private ScreenTransitionManager screenTransitionManager;
        private ProfileScreenManager profileScreenManager;
        private AchievementsScreenManager achievementsScreenManager;
        private CreditsScreenManager creditsScreenManager;
        
        // プロフィール関連（ProfileScreenManagerで管理されているため、ここでは使用しない）
        
        // 「もうひとつ」関連
        private bool wordFoundInCurrentScenario = false; // 現在のシナリオで「もうひとつ」を見つけたか
        
        // オーディオ関連
        private AudioSource bgmAudioSource; // BGM再生用のAudioSource
        private AudioSource sfxAudioSource; // 効果音再生用のAudioSource
        private AudioSource ambientAudioSource; // 環境音再生用のAudioSource
        private Coroutine fadeOutCoroutine; // BGMフェードアウト用のコルーチン
        private Coroutine fadeInCoroutine; // フェードイン用のコルーチン
        private Coroutine sfxFadeOutCoroutine; // 効果音フェードアウト用のコルーチン
        private Coroutine ambientFadeOutCoroutine; // 環境音フェードアウト用のコルーチン
        private Coroutine ambientFadeInCoroutine; // 環境音フェードイン用のコルーチン
        private float selectionBGMPausedTime = 0f; // シナリオ選択BGMの一時停止時刻
        private bool isSelectionBGMPlaying = false; // シナリオ選択BGMが再生中かどうか
        private int currentAmbientScenarioId = -1; // 現在再生中の環境音のシナリオID
        private float selectionBGMNormalVolume = 1.0f; // シナリオ選択BGMの通常音量
        private float selectionBGMLoweredVolume = 0.5f; // プロフィール/実績画面でのBGM音量（通常の50%）
        
        // ローパスフィルター関連
        private AudioLowPassFilter bgmLowPassFilter; // BGM用のローパスフィルター
        private Coroutine lowPassFadeCoroutine; // ローパスフィルターのフェード用コルーチン
        private const float LowPassNormalCutoff = 22000f; // 通常時のカットオフ周波数
        private const float LowPassMuffledCutoff = 1000f; // モヤがかった時のカットオフ周波数
        
        // ピッチ関連
        private Coroutine pitchFadeCoroutine; // ピッチのフェード用コルーチン
        private const float NormalPitch = 1.0f; // 通常時のピッチ
        private const float LoweredPitch = 0.5f; // ダークモード時の下げたピッチ

        private void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManagerが見つかりません！");
                return;
            }

            // マネージャークラスのインスタンスを作成
            typewriterEffectManager = gameObject.AddComponent<TypewriterEffectManager>();
            if (typewriterSound != null)
            {
                typewriterEffectManager.SetTypewriterSound(typewriterSound);
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
            creditsScreenManager = gameObject.AddComponent<CreditsScreenManager>();
            
            // BGM専用のGameObjectを作成し、ローパスフィルターがBGMだけに掛かるようにする
            GameObject bgmObject = new GameObject("BGMPlayer");
            bgmObject.transform.SetParent(this.transform);
            bgmAudioSource = bgmObject.AddComponent<AudioSource>();
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.volume = 1f; // BGMの初期音量
            
            // BGM用のローパスフィルターを追加
            bgmLowPassFilter = bgmObject.AddComponent<AudioLowPassFilter>();
            bgmLowPassFilter.cutoffFrequency = LowPassNormalCutoff;
            bgmLowPassFilter.enabled = true;
            
            // 効果音専用のGameObject
            GameObject sfxObject = new GameObject("SFXPlayer");
            sfxObject.transform.SetParent(this.transform);
            sfxAudioSource = sfxObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.volume = 1f; // 効果音の初期音量（必要に応じて調整可能）
            
            // 環境音専用のGameObject
            GameObject ambientObject = new GameObject("AmbientPlayer");
            ambientObject.transform.SetParent(this.transform);
            ambientAudioSource = ambientObject.AddComponent<AudioSource>();
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.volume = 0.5f; // 環境音の初期音量（必要に応じて調整可能）
            ambientAudioSource.loop = true; // 環境音はループ再生

            gameManager.OnScoreChanged += UpdateScoreDisplay;
            ShowTitleScreen();
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreChanged -= UpdateScoreDisplay;
            }
        }

        /// <summary>
        /// タイトル画面を表示
        /// </summary>
        public void ShowTitleScreen()
        {
            FadeOutAudioOnSceneChange();
            HideAllScreens();
            
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
                }
            }
            
            // スタートボタンの設定
            var startButton = root.Q<Button>("StartButton");
            if (startButton != null)
            {
                startButton.clicked += OnStartButtonClicked;
                startButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
            }
            
            // 謎の声テキストを非表示に設定
            var mysteryVoiceText = root.Q<Label>("MysteryVoiceText");
            if (mysteryVoiceText != null)
            {
                mysteryVoiceText.style.display = DisplayStyle.None;
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
            var mysteryVoiceText = root.Q<Label>("MysteryVoiceText");
            if (mysteryVoiceText != null && typewriterEffectManager != null)
            {
                mysteryVoiceText.style.display = DisplayStyle.Flex;
                
                // タイプライター効果でテキストを表示（速度を2倍遅く）
                string mysteryText = "謎の声：あなたは【もうひとつ】を探す使命が与えられています。";
                typewriterEffectManager.StartTypewriterEffect(mysteryVoiceText, mysteryText, () =>
                {
                    // タイプライター効果完了後、テキストを3秒かけてフェードアウト
                    StartCoroutine(FadeOutTitleTextAndShowSelection(mysteryVoiceText));
                }, speedMultiplier: 2.0f);
            }
            else
            {
                // タイプライター効果が使えない場合は即座に遷移
                StartCoroutine(DelayedShowSelectionScreen(1.5f));
            }
        }
        
        /// <summary>
        /// タイトルテキストをフェードアウトしてからシナリオ選択画面を表示
        /// </summary>
        private IEnumerator FadeOutTitleTextAndShowSelection(Label titleText)
        {
            if (titleText == null) yield break;
            
            // 初期opacityを取得（設定されていない場合は1.0）
            float startOpacity = 1.0f;
            if (titleText.style.opacity.value > 0f)
            {
                startOpacity = titleText.style.opacity.value;
            }
            else
            {
                titleText.style.opacity = startOpacity;
            }
            
            // 3秒かけてフェードアウト
            float fadeDuration = 3.0f;
            float elapsed = 0f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float opacity = Mathf.Lerp(startOpacity, 0f, t);
                titleText.style.opacity = opacity;
                yield return null;
            }
            
            // 完全に透明になったことを確認
            titleText.style.opacity = 0f;
            
            // シナリオ選択画面をフェードインで表示
            ShowSelectionScreenWithFadeIn();
        }
        
        /// <summary>
        /// 遅延してシナリオ選択画面を表示
        /// </summary>
        private IEnumerator DelayedShowSelectionScreen(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowSelectionScreen();
        }

        /// <summary>
        /// シナリオ選択画面をフェードインで表示（タイトル画面からの遷移用）
        /// </summary>
        private void ShowSelectionScreenWithFadeIn()
        {
            FadeOutAudioOnSceneChange();
            HideAllScreens();
            
            if (selectionScreenDocument == null)
            {
                Debug.LogError("SelectionScreenDocumentがアサインされていません！");
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
                }
            }
            
            // タイトルを設定
            var titleLabel = root.Q<Label>("TitleText");
            if (titleLabel != null)
            {
                titleLabel.text = "ミニノベルゲーム";
                titleLabel.AddToClassList("title-text");
            }

            // プロフィールボタンの設定
            var showProfileButton = root.Q<Button>("ShowProfileButton");
            if (showProfileButton != null)
            {
                showProfileButton.clicked += ShowProfileScreen;
            }

            // エンドクレジットボタンの設定（真実の扉クリア後のみ表示）
            var showCreditsButton = root.Q<Button>("ShowCreditsButton");
            if (showCreditsButton != null)
            {
                // 絵文字を画像に置き換え
                SetupButtonWithIcon(showCreditsButton, creditsIcon, "エンドクレジットを見る");
                
                var scenario6Result = gameManager.GetScenarioResult(6);
                if (scenario6Result != null)
                {
                    showCreditsButton.style.display = DisplayStyle.Flex;
                    showCreditsButton.clicked += ShowCreditsScreen;
                }
                else
                {
                    showCreditsButton.style.display = DisplayStyle.None;
                }
            }

            // 実績ボタンの設定（全シナリオクリア後のみ表示）
            var showAchievementsButton = root.Q<Button>("ShowAchievementsButton");
            if (showAchievementsButton != null)
            {
                // 絵文字を画像に置き換え
                SetupButtonWithIcon(showAchievementsButton, achievementsIcon, "実績一覧を見る");
                
                var scenarios = gameManager.GetScenarios();
                int totalCompleted = 0;
                foreach (var scenario in scenarios)
                {
                    if (gameManager.IsScenarioCompleted(scenario.id))
                    {
                        totalCompleted++;
                    }
                }
                
                if (totalCompleted >= scenarios.Count)
                {
                    showAchievementsButton.style.display = DisplayStyle.Flex;
                    showAchievementsButton.clicked += ShowAchievementsScreen;
                }
                else
                {
                    showAchievementsButton.style.display = DisplayStyle.None;
                }
            }

            // スコア表示を更新
            UpdateScoreDisplay();

            // シナリオボタンを作成
            var scenarioButtonContainer = root.Q<VisualElement>("ScenarioButtonContainer");
            if (scenarioButtonContainer != null)
            {
                scenarioButtonContainer.Clear();
                CreateScenarioButtons(scenarioButtonContainer);
            }
            
            // トランジション開始（フェードイン）
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        public void ShowSelectionScreen()
        {
            FadeOutAudioOnSceneChange();
            HideAllScreens();
            
            if (selectionScreenDocument == null)
            {
                Debug.LogError("SelectionScreenDocumentがアサインされていません！");
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
                }
            }

            // タイトルを設定
            var titleLabel = root.Q<Label>("TitleText");
            if (titleLabel != null)
            {
                titleLabel.text = "ミニノベルゲーム";
                titleLabel.AddToClassList("title-text");
            }

            // プロフィールボタンの設定
            var showProfileButton = root.Q<Button>("ShowProfileButton");
            if (showProfileButton != null)
            {
                showProfileButton.clicked += ShowProfileScreen;
                showProfileButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
            }

            // エンドクレジットボタンの設定（真実の扉クリア後のみ表示）
            var showCreditsButton = root.Q<Button>("ShowCreditsButton");
            if (showCreditsButton != null)
            {
                // 絵文字を画像に置き換え
                SetupButtonWithIcon(showCreditsButton, creditsIcon, "エンドクレジットを見る");
                showCreditsButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                
                var scenario6Result = gameManager.GetScenarioResult(6);
                if (scenario6Result != null)
                {
                    showCreditsButton.style.display = DisplayStyle.Flex;
                    showCreditsButton.clicked += ShowCreditsScreen;
                }
                else
                {
                    showCreditsButton.style.display = DisplayStyle.None;
                }
            }

            // 実績ボタンの設定（全シナリオクリア後のみ表示）
            var showAchievementsButton = root.Q<Button>("ShowAchievementsButton");
            if (showAchievementsButton != null)
            {
                // 絵文字を画像に置き換え
                SetupButtonWithIcon(showAchievementsButton, achievementsIcon, "実績一覧を見る");
                showAchievementsButton.RegisterCallback<PointerEnterEvent>(evt => PlayHoverSound());
                
                var scenarios = gameManager.GetScenarios();
                int totalCompleted = 0;
                foreach (var scenario in scenarios)
                {
                    if (gameManager.IsScenarioCompleted(scenario.id))
                    {
                        totalCompleted++;
                    }
                }
                
                if (totalCompleted >= scenarios.Count)
                {
                    showAchievementsButton.style.display = DisplayStyle.Flex;
                    showAchievementsButton.clicked += ShowAchievementsScreen;
                }
                else
                {
                    showAchievementsButton.style.display = DisplayStyle.None;
                }
            }

            UpdateScoreDisplay();
            CreateScenarioButtons(root);
            
            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        public void ShowProfileScreen()
        {
            FadeOutAudioOnSceneChange();
            // シナリオ選択BGMの音量を下げる（流したまま）
            LowerSelectionBGMVolume();
            HideAllScreens();
            
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
            }

            var setupContainer = root.Q<VisualElement>("SetupText");
            var choiceButtonContainer = root.Q<VisualElement>("ChoiceButtonContainer");
            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
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
                bool isDarkMode = gameManager.IsDarkMode();
                string setupText = scenario.setup;
                
                if (isDarkMode)
                {
                    string originalSetup = scenario.setup;
                    // 失われた文字を※に置換
                    var lostLetters = gameManager.GetLostLetters();
                    foreach (char lostLetter in lostLetters)
                    {
                        originalSetup = originalSetup.Replace(lostLetter.ToString(), "※");
                    }

                    setupText = scenario.id switch
                    {
                        1 => $"【エラー】探偵事務所のデータが破損しています。\n写真の人物が歪み、存在が不安定になっています。\nバグの影響で「も」という文字が消失しました。\n\n{originalSetup}",
                        2 => $"【エラー】レストランのデータが破損しています。\nメニューが文字化けし、料理のデータが読み込めません。\nバグの影響で「う」という文字が消失しました。\n\n{originalSetup}",
                        3 => $"【エラー】タイムカプセルのデータが破損しています。\n過去の記憶が歪み、データが欠損しています。\nバグの影響で「ひ」という文字が消失しました。\n\n{originalSetup}",
                        4 => $"【エラー】魔法学校のデータが破損しています。\n呪文のコードがエラーを起こし、魔法が機能しません。\nバグの影響で「と」という文字が消失しました。\n\n{originalSetup}",
                        5 => $"【エラー】パズルのデータが破損しています。\nピースの整合性が失われ、完成することができません。\nバグの影響で「つ」という文字が消失しました。\n\n{originalSetup}",
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
                    }, (found) => {
                        if (found)
                        {
                            wordFoundInCurrentScenario = true;
                            
                            // 効果音を再生
                            PlayWordGetSound();
                            
                            // メッセージを表示
                            var wordFoundMessageLabel = root.Q<Label>("WordFoundMessage");
                            if (wordFoundMessageLabel != null)
                            {
                                wordFoundMessageLabel.text = isDarkMode 
                                    ? "⚠️ システムエラー：データ破損を検出 ⚠️"
                                    : "あなたは何かをみつけた気がした";
                                wordFoundMessageLabel.style.display = DisplayStyle.Flex;
                                StartCoroutine(ShakeAnimation(wordFoundMessageLabel));
                            }
                            
                            // 選択肢ボタンを順次表示
                            StartCoroutine(ShowChoicesSequentially(root));
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

            // トランジション開始（シナリオ画面のみスケールアニメーションあり）
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root, withScale: true);
            }
        }

        public void ShowResultScreen()
        {
            FadeOutAudioOnSceneChange();
            // 環境音を長めにフェードアウト（結果画面に移行）
            FadeOutAmbientSoundForResult();
            HideAllScreens();
            
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
                    epilogueLabel.AddToClassList("epilogue-text");
                }
            }

            // ワードゲット表示（最初は非表示、結果テキストのタイプライター効果が完了したら表示）
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            var wordGetLabel = root.Q<Label>("WordGetText");
            var wordFailedMessageLabel = root.Q<Label>("WordFailedMessage");
            var countdownContainer = root.Q<VisualElement>("CountdownContainer");
            var countdownText = root.Q<Label>("CountdownText");
            
            // 時計アイコンを設定
            var clockIcon = root.Q<Image>("ClockIcon");
            if (clockIcon != null && this.clockIcon != null)
            {
                clockIcon.sprite = this.clockIcon;
            }
            
            // フラグをリセット（結果画面で「もうひとつ」を探すため）
            // ただし、すでにHandleChoiceでhasWord=trueになっている場合は、そのまま保持
            if (result != null && !result.hasWord)
            {
                wordFoundInCurrentScenario = false;
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
                }
                
                // 結果テキストをVisualElementに変更して「もうひとつ」をクリッカブルにする
                var resultContainer = new VisualElement();
                resultContainer.style.fontSize = 18;
                resultContainer.style.whiteSpace = WhiteSpace.Normal;
                resultContainer.style.maxWidth = 800;
                resultContainer.style.marginBottom = 20;
                
                // 元のLabelを非表示にして、新しいコンテナを追加
                resultLabel.style.display = DisplayStyle.None;
                resultLabel.parent.Insert(resultLabel.parent.IndexOf(resultLabel), resultContainer);
                
                // 結果テキストに「【もうひとつ】」が含まれているか確認
                bool hasMouhitotsu = resultText.Contains("【もうひとつ】");
                
                // タイプライター効果で表示
                if (typewriterEffectManager != null)
                {
                    if (hasMouhitotsu)
                    {
                        // 「もうひとつ」が含まれている場合：クリッカブルワード付きタイプライター効果
                        typewriterEffectManager.StartTypewriterEffectWithClickableWord(resultContainer, resultText, () =>
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
                                        // wordGetLabelのテキストを設定
                                        if (wordGetLabel != null)
                                        {
                                            wordGetLabel.ClearClassList();
                                            if (isDarkMode)
                                            {
                                                wordGetLabel.text = "⚠️ 【システムエラー】世界崩壊 ⚠️";
                                                wordGetLabel.AddToClassList("word-get-dark");
                                            }
                                            else if (wordFoundInCurrentScenario)
                                            {
                                                // ✨を画像で置き換え
                                                SetupWordGetLabelWithSparkle(wordGetContainer, wordGetLabel, "【もうひとつ】ワードゲット!");
                                                wordGetLabel.AddToClassList("word-get-success");
                                            }
                                            else
                                            {
                                                // wordGetLabel.text = "残念...【もうひとつ】は出ませんでした";
                                                // wordGetLabel.AddToClassList("word-get-failed");
                                            }
                                        }
                                        
                                        // if (wordFoundInCurrentScenario && epilogueContainer != null)
                                        // {
                                        //     epilogueContainer.style.display = DisplayStyle.Flex;
                                        //     if (epilogueLabel != null && !string.IsNullOrEmpty(epilogueText) && typewriterEffectManager != null)
                                        //     {
                                        //         typewriterEffectManager.StartTypewriterEffect(epilogueLabel, epilogueText, () =>
                                        //         {
                                        //             ShowBackButton();
                                        //         });
                                        //     }
                                        //     else
                                        //     {
                                        //         ShowBackButton();
                                        //     }
                                        // }
                                        // else
                                        // {
                                        //     ShowBackButton();
                                        // }
                                    },
                                    ShowBackButton
                                );
                            }
                        }, (found) => {
                        if (found)
                        {
                            wordFoundInCurrentScenario = true;
                            
                            // 効果音を再生
                            PlayWordGetSound();
                            
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
                            
                            // 綺麗な演出とともに一呼吸してから表示
                            StartCoroutine(ShowWordGetWithEffect(root, isDarkMode, scenario, result, epilogueContainer, epilogueLabel));
                        }
                    });
                    }
                    else
                    {
                        // 「もうひとつ」が含まれていない場合：通常のタイプライター効果
                        var resultLabelForTypewriter = new Label();
                        resultLabelForTypewriter.style.fontSize = 18;
                        resultLabelForTypewriter.style.whiteSpace = WhiteSpace.Normal;
                        resultLabelForTypewriter.style.maxWidth = 800;
                        resultLabelForTypewriter.style.marginBottom = 20;
                        resultContainer.Add(resultLabelForTypewriter);
                        
                        typewriterEffectManager.StartTypewriterEffect(resultLabelForTypewriter, resultText, () =>
                        {
                            // タイプライター効果が完了したら、即座に戻るボタンを表示
                            ShowBackButton();
                        });
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
            }

            // 戻るボタン（最初は非表示）
            var backButton = root.Q<Button>("BackToSelectionButton");
            if (backButton != null)
            {
                backButton.style.display = DisplayStyle.None;
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }

        private void HideAllScreens()
        {
            if (titleScreenDocument != null) titleScreenDocument.gameObject.SetActive(false);
            if (selectionScreenDocument != null) selectionScreenDocument.gameObject.SetActive(false);
            if (scenarioScreenDocument != null)
            {
                // シナリオ画面を閉じる時に環境音を停止
                StopAmbientSound();
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
                if (bgmAudioSource != null && bgmAudioSource.clip == creditsBGM)
                {
                    // BGMをフェードアウト
                    fadeOutCoroutine = StartCoroutine(FadeOutAudio(2f));
                }
                // シナリオ選択BGMが再生中の場合は一時停止時刻を記録
                if (bgmAudioSource != null && bgmAudioSource.clip == selectionBGM && bgmAudioSource.isPlaying)
                {
                    selectionBGMPausedTime = bgmAudioSource.time;
                    // bgmAudioSource.Pause();
                    // isSelectionBGMPlaying = false;
                }
                creditsScreenDocument.gameObject.SetActive(false);
            }
            if (achievementsScreenDocument != null) achievementsScreenDocument.gameObject.SetActive(false);
        }

        /// <summary>
        /// スパークルアイコンクリック時の効果音を再生
        /// </summary>
        private void PlaySparkleSound()
        {
            if (sparkleSound != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(sparkleSound);
            }
        }

        /// <summary>
        /// ボタンマウスオーバー時の効果音を再生
        /// </summary>
        public void PlayHoverSound()
        {
            if (buttonHoverSound != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(buttonHoverSound);
            }
        }

        private void UpdateScoreDisplay()
        {
            if (currentDocument == null || currentDocument.rootVisualElement == null) return;

            var scoreLabel = currentDocument.rootVisualElement.Q<Label>("ScoreText");
            if (scoreLabel != null && gameManager != null)
            {
                int score = gameManager.GetScore();
                int totalScenarios = gameManager.GetScenarios().Count;
                
                // ダークモードで失われた文字を取得
                var lostLetters = gameManager.GetLostLetters();
                string scoreText = "【もうひとつ】ワードゲット数";
                
                // 失われた文字を※に置き換え
                if (lostLetters.Count > 0)
                {
                    List<string> lostLettersList = new List<string>();
                    foreach (char c in lostLetters) lostLettersList.Add(c.ToString());
                    Debug.Log($"[UpdateScoreDisplay] 失われた文字数: {lostLetters.Count}, 文字: {string.Join(", ", lostLettersList.ToArray())}");
                    Debug.Log($"[UpdateScoreDisplay] 置き換え前: {scoreText}");
                    foreach (char lostLetter in lostLetters)
                    {
                        scoreText = scoreText.Replace(lostLetter.ToString(), "※");
                    }
                    Debug.Log($"[UpdateScoreDisplay] 置き換え後: {scoreText}");
                }
                
                scoreLabel.text = $"{scoreText}: {score} / {totalScenarios}";
                Debug.Log($"[UpdateScoreDisplay] 最終表示テキスト: {scoreLabel.text}");
                
                // 異常なスコアの場合はスタイルを適用
                scoreLabel.ClearClassList();
                if (score > totalScenarios || lostLetters.Count > 0)
                {
                    scoreLabel.AddToClassList("score-text-anomaly");
                }
                else
                {
                    scoreLabel.AddToClassList("score-text");
                }
            }
            
            // 選択画面のスコアも更新
            if (selectionScreenDocument != null && selectionScreenDocument.gameObject.activeSelf)
            {
                var root = selectionScreenDocument.rootVisualElement;
                if (root != null)
                {
                    var selectionScoreLabel = root.Q<Label>("ScoreText");
                    if (selectionScoreLabel != null && gameManager != null)
                    {
                        int score = gameManager.GetScore();
                        int totalScenarios = gameManager.GetScenarios().Count;
                        
                        // ダークモードで失われた文字を取得
                        var lostLetters = gameManager.GetLostLetters();
                        string scoreText = "【もうひとつ】ワードゲット数";
                        
                        // 失われた文字を※に置き換え
                        if (lostLetters.Count > 0)
                        {
                            List<string> lostLettersList = new List<string>();
                            foreach (char c in lostLetters) lostLettersList.Add(c.ToString());
                            Debug.Log($"[UpdateScoreDisplay-Selection] 失われた文字数: {lostLetters.Count}, 文字: {string.Join(", ", lostLettersList.ToArray())}");
                            Debug.Log($"[UpdateScoreDisplay-Selection] 置き換え前: {scoreText}");
                            foreach (char lostLetter in lostLetters)
                            {
                                scoreText = scoreText.Replace(lostLetter.ToString(), "※");
                            }
                            Debug.Log($"[UpdateScoreDisplay-Selection] 置き換え後: {scoreText}");
                        }
                        
                        selectionScoreLabel.text = $"{scoreText}: {score} / {totalScenarios}";
                        Debug.Log($"[UpdateScoreDisplay-Selection] 最終表示テキスト: {selectionScoreLabel.text}");
                        
                        // 異常なスコアの場合はスタイルを適用
                        selectionScoreLabel.ClearClassList();
                        if (score > totalScenarios || lostLetters.Count > 0)
                        {
                            selectionScoreLabel.AddToClassList("score-text-anomaly");
                        }
                        else
                        {
                            selectionScoreLabel.AddToClassList("score-text");
                        }
                    }
                }
            }
        }

        private void CreateScenarioButtons(VisualElement root)
        {
            var buttonContainer = root.Q<VisualElement>("ScenarioButtonContainer");
            if (buttonContainer == null) return;

            // 既存のボタンを削除
            buttonContainer.Clear();

            var scenarios = gameManager.GetScenarios();
            foreach (var scenario in scenarios)
            {
                // シナリオ6は最初の5つをクリアするまで表示しない
                if (scenario.id == 6 && !gameManager.CanAccessScenario(6))
                {
                    continue;
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
                
                var titleLabel = new Label(scenario.title);
                titleLabel.style.fontSize = 20;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.whiteSpace = WhiteSpace.Normal;
                titleLabel.style.marginBottom = 5;
                buttonContent.Add(titleLabel);
                
                // シナリオの説明を追加（2行まで）
                var descriptionLabel = new Label(scenario.setup);
                descriptionLabel.style.fontSize = 14;
                descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
                descriptionLabel.style.opacity = 0.9f;
                descriptionLabel.style.maxHeight = 40; // 2行分の高さに制限
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
                }
                else if (isCompleted)
                {
                    button.AddToClassList("scenario-button-completed");
                    // 完了マークを追加
                    var completedMark = new Label("✓");
                    completedMark.style.fontSize = 16;
                    completedMark.style.position = Position.Absolute;
                    completedMark.style.top = 5;
                    completedMark.style.right = 5;
                    button.Add(completedMark);
                }
                else
                {
                    button.AddToClassList("scenario-button-normal");
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
            List<Choice> choices;
            
            if (isDarkMode)
            {
                // ダークモード時の選択肢
                choices = scenario.id switch
                {
                    1 => new List<Choice> 
                    { 
                        new Choice { id = 1, text = "「データが壊れている...」と呟く", preview = "私：「も...もど...」" },
                        new Choice { id = 2, text = "「修復できるか？」と試みる", preview = "私：「この世界は...何が起きている...」" }
                    },
                    2 => new List<Choice> 
                    { 
                        new Choice { id = 1, text = "「メニューが読めない...」と困惑する", preview = "私：「う...うみ...？」" },
                        new Choice { id = 2, text = "「システムエラーを報告する」", preview = "私：「データが...崩壊している...」" }
                    },
                    3 => new List<Choice> 
                    { 
                        new Choice { id = 1, text = "「記憶が歪んでいる...」と気づく", preview = "私：「ひ...ひろ...？」" },
                        new Choice { id = 2, text = "「データを修復しようとする」", preview = "私：「過去のデータが...消えていく...」" }
                    },
                    4 => new List<Choice> 
                    { 
                        new Choice { id = 1, text = "「魔法が機能しない...」と混乱する", preview = "私：「と...とおる...？」" },
                        new Choice { id = 2, text = "「システムの整合性を確認する」", preview = "私：「コードが...エラーを起こしている...」" }
                    },
                    5 => new List<Choice> 
                    { 
                        new Choice { id = 1, text = "「ピースが足りない...」と絶望する", preview = "私：「つ...つばさ...？」" },
                        new Choice { id = 2, text = "「完成できないことに気づく」", preview = "私：「永遠に...完成できない...」" }
                    },
                    6 => new List<Choice> 
                    { 
                        new Choice { id = 1, text = "「すみません...」と謝る", preview = "私：「壊してしまって..." },
                        new Choice { id = 2, text = "「これは何ですか？」と問う", preview = "私：「この世界は..." }
                    },
                    _ => scenario.choices
                };
            }
            else
            {
                choices = scenario.choices;
            }

            foreach (var choice in choices)
            {
                // ボタンを作成
                Button button = new Button();
                
                // ダークモードの場合はダークスタイルを適用
                if (isDarkMode)
                {
                    button.AddToClassList("choice-button-dark");
                }
                else
                {
                    button.AddToClassList("choice-button");
                }
                
                // ボタンの中にテキストを配置
                var buttonText = new Label($"選択肢{choice.id}：{choice.text}");
                buttonText.style.fontSize = 18;
                buttonText.style.whiteSpace = WhiteSpace.Normal;
                buttonText.style.unityFontStyleAndWeight = FontStyle.Bold;
                
                var previewText = new Label(choice.preview);
                previewText.style.fontSize = 14;
                previewText.style.opacity = 0.8f;
                previewText.style.whiteSpace = WhiteSpace.Normal;

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
            var backButton = root.Q<Button>("BackToSelectionButtonFromScenario");
            if (backButton != null)
            {
                yield return new WaitForSeconds(0.3f);
                backButton.style.display = DisplayStyle.Flex;
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
                    backgroundImage.style.backgroundImage = new StyleBackground(scenarioBackgrounds[backgroundIndex]);
                }
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
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
        }


        public void ShowCreditsScreen()
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
                }
            }

            var creditsContent = root.Q<VisualElement>("CreditsContent");
            var creditsScrollView = root.Q<ScrollView>("CreditsScrollView");
            if (creditsContent == null || creditsScrollView == null) return;

            if (creditsScreenManager != null)
            {
                creditsScreenManager.CreateCredits(creditsContent, creditsScrollView);
            }
            
            // BGMを再生
            if (creditsBGM != null && bgmAudioSource != null)
            {
                // BGMが再生されたので環境音をフェードアウト
                FadeOutAmbientSound();
                
                bgmAudioSource.clip = creditsBGM;
                bgmAudioSource.loop = true;
                bgmAudioSource.Play();
            }

            // 戻るボタン
            var backButton = root.Q<Button>("BackToSelectionButtonFromCredits");
            if (backButton != null)
            {
                backButton.clicked += ShowSelectionScreen;
            }

            // トランジション開始
            if (screenTransitionManager != null)
            {
                screenTransitionManager.StartScreenTransition(root);
            }
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
            }
        }

        /// <summary>
        /// シェイクアニメーション
        /// </summary>
        private IEnumerator ShakeAnimation(Label label)
        {
            float duration = 0.5f;
            float shakeIntensity = 10f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float offsetX = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                float offsetY = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                
                label.style.translate = new Translate(offsetX, offsetY, 0);
                
                yield return null;
            }
            
            // 元の位置に戻す
            label.style.translate = new Translate(0, 0, 0);
        }
        
        /// <summary>
        /// ワードゲット時の綺麗な演出を表示
        /// </summary>
        private IEnumerator ShowWordGetWithEffect(VisualElement root, bool isDarkMode, Scenario scenario, ScenarioResult result, VisualElement epilogueContainer, Label epilogueLabel)
        {
            // 演出用のオーバーレイを作成
            var effectOverlay = new VisualElement();
            effectOverlay.style.position = Position.Absolute;
            effectOverlay.style.left = 0;
            effectOverlay.style.top = 0;
            effectOverlay.style.right = 0;
            effectOverlay.style.bottom = 0;
            effectOverlay.style.backgroundColor = new Color(1f, 1f, 1f, 0f);
            effectOverlay.style.justifyContent = Justify.Center;
            effectOverlay.style.alignItems = Align.Center;
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
                glowEffect.style.width = 200f * currentScale;
                glowEffect.style.height = 200f * currentScale;
                // 円形を維持するため、すべての角に同じ値を設定
                float currentBorderRadius = 100f * currentScale;
                glowEffect.style.borderTopLeftRadius = currentBorderRadius;
                glowEffect.style.borderTopRightRadius = currentBorderRadius;
                glowEffect.style.borderBottomLeftRadius = currentBorderRadius;
                glowEffect.style.borderBottomRightRadius = currentBorderRadius;
                
                // フェードアウト
                float alpha = Mathf.Lerp(0.8f, 0f, t);
                glowEffect.style.backgroundColor = new Color(1f, 0.84f, 0f, alpha);
                
                // 背景も少し明るく
                float bgAlpha = Mathf.Lerp(0f, 0.3f, Mathf.Sin(t * Mathf.PI));
                effectOverlay.style.backgroundColor = new Color(1f, 1f, 1f, bgAlpha);
                
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
                    // ✨を画像で置き換え
                    SetupWordGetLabelWithSparkle(wordGetContainer, wordGetLabel, "【もうひとつ】ワードゲット!");
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
            
            // HandleChoiceを再度呼び出して、hasWordをtrueに更新
            if (scenario != null && result != null)
            {
                gameManager.HandleChoice(result.choiceId, true);
                // resultを再取得
                result = gameManager.GetScenarioResult(scenario.id);
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
        /// 「もうひとつ」をゲットした時の効果音を再生（複数からランダムに選択）
        /// </summary>
        private void PlayWordGetSound()
        {
            if (wordGetSounds != null && wordGetSounds.Length > 0 && sfxAudioSource != null)
            {
                // 配列からランダムに1つ選択
                int randomIndex = Random.Range(0, wordGetSounds.Length);
                AudioClip selectedSound = wordGetSounds[randomIndex];
                
                if (selectedSound != null)
                {
                    sfxAudioSource.PlayOneShot(selectedSound);
                    // 効果音が再生されたので環境音をフェードアウト
                    FadeOutAmbientSound();
                }
            }
        }
        
        /// <summary>
        /// オーディオをフェードアウト（BGM用）
        /// </summary>
        private IEnumerator FadeOutAudio(float duration)
        {
            if (bgmAudioSource == null) yield break;
            
            // 既存のフェードアウトコルーチンを停止
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
            
            float startVolume = bgmAudioSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }
            
            // フェードアウト完了後、停止して音量をリセット
            bgmAudioSource.Stop();
            bgmAudioSource.volume = startVolume;
            fadeOutCoroutine = null;
            
            // BGMが停止したら環境音をフェードイン
            StartCoroutine(CheckAndFadeInAmbientAfterBGM());
        }
        
        /// <summary>
        /// BGMが停止したら環境音をフェードイン
        /// </summary>
        private IEnumerator CheckAndFadeInAmbientAfterBGM()
        {
            // BGMのフェードアウトが完了するまで少し待つ
            yield return new WaitForSeconds(0.1f);
            
            // 効果音やBGMが再生中でない場合、環境音をフェードイン
            if (!IsAnyAudioPlaying() && ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientFadeInCoroutine = StartCoroutine(FadeInAmbientSound(1f));
            }
        }
        
        /// <summary>
        /// シーン切り替え時にオーディオをフェードアウト（効果音用）
        /// </summary>
        private void FadeOutAudioOnSceneChange()
        {
            // 効果音が再生中の場合はフェードアウト（0.5秒）
            if (sfxAudioSource != null && sfxAudioSource.isPlaying)
            {
                // 既存の効果音フェードアウトコルーチンを停止
                if (sfxFadeOutCoroutine != null)
                {
                    StopCoroutine(sfxFadeOutCoroutine);
                }
                sfxFadeOutCoroutine = StartCoroutine(FadeOutSfxAudio(0.5f));
            }
            
            // ローパスフィルターをリセット（通常の状態に戻す）
            if (bgmLowPassFilter != null)
            {
                if (lowPassFadeCoroutine != null)
                {
                    StopCoroutine(lowPassFadeCoroutine);
                    lowPassFadeCoroutine = null;
                }
                bgmLowPassFilter.cutoffFrequency = LowPassNormalCutoff;
            }

            // 効果音が停止したら環境音をフェードイン
            StartCoroutine(CheckAndFadeInAmbientAfterSfx());
            
            // ピッチをリセット（通常の状態に戻す）
            if (bgmAudioSource != null)
            {
                if (pitchFadeCoroutine != null)
                {
                    StopCoroutine(pitchFadeCoroutine);
                    pitchFadeCoroutine = null;
                }
                bgmAudioSource.pitch = NormalPitch;
            }
        }
        
        /// <summary>
        /// 効果音をフェードアウト
        /// </summary>
        private IEnumerator FadeOutSfxAudio(float duration)
        {
            if (sfxAudioSource == null) yield break;
            
            float startVolume = sfxAudioSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                sfxAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }
            
            // フェードアウト完了後、停止して音量をリセット
            sfxAudioSource.Stop();
            sfxAudioSource.volume = startVolume;
            sfxFadeOutCoroutine = null;
            
            // 効果音が停止したら環境音をフェードイン
            StartCoroutine(CheckAndFadeInAmbientAfterSfx());
        }
        
        /// <summary>
        /// シナリオ選択BGMを開始（フェードイン）
        /// </summary>
        private void StartSelectionBGM()
        {
            if (selectionBGM == null || bgmAudioSource == null) return;
            
            // 既存のフェードインコルーチンを停止
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
            }
            
            // 既存のフェードアウトコルーチンを停止
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
            
            // BGMが既に再生中でない場合、または別のBGMが再生中の場合は開始
            if (!isSelectionBGMPlaying || bgmAudioSource.clip != selectionBGM)
            {
                bgmAudioSource.clip = selectionBGM;
                bgmAudioSource.loop = true;
                bgmAudioSource.time = selectionBGMPausedTime; // 一時停止した時刻から再生
                bgmAudioSource.volume = 0f; // フェードイン開始前に音量を0に設定
                bgmAudioSource.Play();
                isSelectionBGMPlaying = true;
            }
            else if (!bgmAudioSource.isPlaying && bgmAudioSource.clip == selectionBGM)
            {
                // 一時停止中の場合は再開（isPlayingがfalseで、clipがselectionBGMの場合）
                bgmAudioSource.time = selectionBGMPausedTime; // 一時停止した時刻から再生
                bgmAudioSource.volume = 0f; // フェードイン開始前に音量を0に設定
                bgmAudioSource.Play();
            }
            else if (bgmAudioSource.isPlaying && bgmAudioSource.clip == selectionBGM)
            {
                // 既に再生中の場合は、現在の音量から通常音量にフェードイン
                // 音量が既に通常音量に近い場合は、そのまま維持
                if (bgmAudioSource.volume < selectionBGMNormalVolume - 0.1f)
                {
                    // 音量が低い場合のみフェードインして通常音量に戻す
                    fadeInCoroutine = StartCoroutine(FadeInAudioToNormalVolume(3f));
                }
                else if (bgmAudioSource.volume < selectionBGMNormalVolume)
                {
                    // 少し低い場合は即座に通常音量に戻す
                    bgmAudioSource.volume = selectionBGMNormalVolume;
                }

                // ローパスフィルターを解除（既に再生中の場合も必要）
                if (lowPassFadeCoroutine != null)
                {
                    StopCoroutine(lowPassFadeCoroutine);
                }
                lowPassFadeCoroutine = StartCoroutine(FadeLowPassFilter(LowPassNormalCutoff, 2.0f));

                return;
            }
            
            // フェードイン（音量を通常音量に戻す）
            fadeInCoroutine = StartCoroutine(FadeInAudioToNormalVolume(3f));

            // ローパスフィルターとピッチを解除（通常の状態に戻す）
            bool isDarkMode = gameManager.IsDarkMode();
            
            if (lowPassFadeCoroutine != null)
            {
                StopCoroutine(lowPassFadeCoroutine);
            }
            lowPassFadeCoroutine = StartCoroutine(FadeLowPassFilter(LowPassNormalCutoff, 2.0f));

            if (pitchFadeCoroutine != null)
            {
                StopCoroutine(pitchFadeCoroutine);
            }
            pitchFadeCoroutine = StartCoroutine(FadePitch(NormalPitch, 2.0f));
        }
        
        /// <summary>
        /// シナリオ選択BGMを一時停止（フェードアウトして時刻を記録）
        /// </summary>
        private void PauseSelectionBGM()
        {
            if (bgmAudioSource == null || bgmAudioSource.clip != selectionBGM) return;
            
            // 既存のフェードインコルーチンを停止
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }
            
            // 既存のフェードアウトコルーチンを停止（念のため）
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
            
            // 再生中でない場合は、現在の時刻を記録して終了
            if (!bgmAudioSource.isPlaying)
            {
                // フェードイン中に停止された場合など、再生時刻を記録
                if (bgmAudioSource.clip == selectionBGM)
                {
                    selectionBGMPausedTime = bgmAudioSource.time;
                    isSelectionBGMPlaying = false;
                }
                return;
            }
            
            // フェードアウトして時刻を記録（1.5秒）
            // 重要：フェードアウトが完了してからPause()を呼ぶ
            fadeOutCoroutine = StartCoroutine(FadeOutAndPauseSelectionBGM(1.5f));
        }
        
        /// <summary>
        /// オーディオをフェードイン（BGM用）
        /// </summary>
        private IEnumerator FadeInAudio(float duration)
        {
            if (bgmAudioSource == null) yield break;
            
            float targetVolume = 1f; // 目標音量（必要に応じて調整可能）
            float startVolume = bgmAudioSource.volume;
            float elapsed = 0f;
            
            // 最初の音量を0に設定
            bgmAudioSource.volume = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bgmAudioSource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }
            
            bgmAudioSource.volume = targetVolume;
            fadeInCoroutine = null;
        }
        
        /// <summary>
        /// シナリオ選択BGMの音量を下げる（プロフィール/実績画面用）
        /// 通常時はローパスフィルターを適用し、ダークモード時はピッチを下げる
        /// </summary>
        private void LowerSelectionBGMVolume()
        {
            if (bgmAudioSource == null || bgmAudioSource.clip != selectionBGM || !bgmAudioSource.isPlaying) return;
            
            // 既存のフェードイン/フェードアウトコルーチンを停止
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }
            
            if (gameManager.IsDarkMode())
            {
                // ダークモード時はピッチを下げる
                if (pitchFadeCoroutine != null)
                {
                    StopCoroutine(pitchFadeCoroutine);
                }
                pitchFadeCoroutine = StartCoroutine(FadePitch(LoweredPitch, 2.0f));
            }
            else
            {
                // 通常時はローパスフィルターを適用
                if (lowPassFadeCoroutine != null)
                {
                    StopCoroutine(lowPassFadeCoroutine);
                }
                lowPassFadeCoroutine = StartCoroutine(FadeLowPassFilter(LowPassMuffledCutoff, 2.0f));
            }
        }

        /// <summary>
        /// ローパスフィルターのカットオフ周波数をフェードさせる
        /// </summary>
        private IEnumerator FadeLowPassFilter(float targetCutoff, float duration)
        {
            if (bgmLowPassFilter == null) yield break;

            float startCutoff = bgmLowPassFilter.cutoffFrequency;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // 周波数の変化は対数的（Logarithmic）に感じられるため、Lerpよりも適切な補間があるが、
                // 今回はシンプルにLerpを使用する
                bgmLowPassFilter.cutoffFrequency = Mathf.Lerp(startCutoff, targetCutoff, t);
                yield return null;
            }

            bgmLowPassFilter.cutoffFrequency = targetCutoff;
            lowPassFadeCoroutine = null;
        }

        /// <summary>
        /// BGMのピッチをフェードさせる
        /// </summary>
        private IEnumerator FadePitch(float targetPitch, float duration)
        {
            if (bgmAudioSource == null) yield break;

            float startPitch = bgmAudioSource.pitch;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bgmAudioSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);
                yield return null;
            }

            bgmAudioSource.pitch = targetPitch;
            pitchFadeCoroutine = null;
        }
        
        /// <summary>
        /// シナリオ選択BGMの音量をフェードで変更
        /// </summary>
        private IEnumerator FadeSelectionBGMVolume(float fromVolume, float toVolume, float duration)
        {
            if (bgmAudioSource == null || bgmAudioSource.clip != selectionBGM) yield break;
            
            float elapsed = 0f;
            float startVolume = bgmAudioSource.volume;
            
            while (elapsed < duration)
            {
                if (bgmAudioSource == null || bgmAudioSource.clip != selectionBGM) yield break;
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, toVolume, t);
                yield return null;
            }
            
            if (bgmAudioSource != null && bgmAudioSource.clip == selectionBGM)
            {
                bgmAudioSource.volume = toVolume;
            }
        }
        
        /// <summary>
        /// オーディオをフェードインして通常音量に戻す（BGM用）
        /// </summary>
        private IEnumerator FadeInAudioToNormalVolume(float duration)
        {
            if (bgmAudioSource == null) yield break;
            
            float targetVolume = selectionBGMNormalVolume; // 通常音量
            float startVolume = bgmAudioSource.volume;
            float elapsed = 0f;
            
            // 最初の音量を0に設定（新規再生の場合）
            if (startVolume < 0.01f)
            {
                bgmAudioSource.volume = 0f;
                startVolume = 0f;
            }
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }
            
            bgmAudioSource.volume = targetVolume;
            fadeInCoroutine = null;
        }
        
        /// <summary>
        /// シナリオ選択BGMをフェードアウトして一時停止時刻を記録
        /// </summary>
        private IEnumerator FadeOutAndPauseSelectionBGM(float duration)
        {
            if (bgmAudioSource == null) yield break;
            
            float startVolume = bgmAudioSource.volume;
            float elapsed = 0f;
            
            // フェードアウト中は再生を続ける（音量を下げるだけ）
            while (elapsed < duration)
            {
                // bgmAudioSourceが破棄された場合は終了
                if (bgmAudioSource == null)
                {
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // clipが変更されても、フェードアウトは続行（音量を0にする）
                if (bgmAudioSource.clip == selectionBGM)
                {
                    bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                }
                else
                {
                    // clipが変更された場合は、現在の音量を0に向かって下げる
                    bgmAudioSource.volume = Mathf.Lerp(bgmAudioSource.volume, 0f, t);
                }
                
                yield return null;
            }
            
            // フェードアウト完了後、音量を0に設定
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = 0f;
            }
            
            // フェードアウト完了後、現在の再生時刻を記録して一時停止
            if (bgmAudioSource != null && bgmAudioSource.clip == selectionBGM && bgmAudioSource.isPlaying)
            {
                selectionBGMPausedTime = bgmAudioSource.time;
                // bgmAudioSource.Pause(); // 停止ではなく一時停止（再生時刻を保持）
                isSelectionBGMPlaying = false;
            }
            
            fadeOutCoroutine = null;
        }
        
        /// <summary>
        /// シナリオの環境音を開始
        /// </summary>
        private void StartAmbientSound(int scenarioId)
        {
            if (ambientSounds == null || ambientSounds.Length == 0 || ambientAudioSource == null) return;
            
            // シナリオIDは1-6なので、インデックスは0-5
            int index = scenarioId - 1;
            if (index < 0 || index >= ambientSounds.Length) return;
            
            AudioClip ambientClip = ambientSounds[index];
            if (ambientClip == null) return;
            
            // 既に同じ環境音が再生中の場合は何もしない
            if (ambientAudioSource.isPlaying && ambientAudioSource.clip == ambientClip && currentAmbientScenarioId == scenarioId)
            {
                return;
            }
            
            // 既存のフェードイン/フェードアウトコルーチンを停止
            if (ambientFadeInCoroutine != null)
            {
                StopCoroutine(ambientFadeInCoroutine);
            }
            // 開始時にまだフェードアウトしているものがあれば停止
            if (ambientFadeOutCoroutine != null)
            {
                StopCoroutine(ambientFadeOutCoroutine);
            }
            
            // 環境音を開始
            ambientAudioSource.clip = ambientClip;
            ambientAudioSource.loop = true;
            ambientAudioSource.volume = 0f; // フェードイン開始前に音量を0に設定
            ambientAudioSource.Play();
            currentAmbientScenarioId = scenarioId;
            
            // 効果音やBGMが再生中でない場合のみフェードイン
            if (!IsAnyAudioPlaying())
            {
                ambientFadeInCoroutine = StartCoroutine(FadeInAmbientSound(1f));
            }
        }
        
        /// <summary>
        /// 環境音を停止
        /// </summary>
        private void StopAmbientSound()
        {
            if (ambientAudioSource == null) return;
            
            // 既存のフェードイン/フェードアウトコルーチンを停止
            if (ambientFadeInCoroutine != null)
            {
                StopCoroutine(ambientFadeInCoroutine);
            }
            // フェードアウト中のものは止めない（フェードアウトを続行させる）
            // if (ambientFadeOutCoroutine != null)
            // {
            //     StopCoroutine(ambientFadeOutCoroutine);
            // }
            
            // 
            currentAmbientScenarioId = -1;
        }
        
        /// <summary>
        /// 環境音をフェードアウト
        /// </summary>
        private void FadeOutAmbientSound()
        {
            if (ambientAudioSource == null || !ambientAudioSource.isPlaying) return;
            
            // 既存のフェードインコルーチンを停止
            if (ambientFadeInCoroutine != null)
            {
                StopCoroutine(ambientFadeInCoroutine);
            }
            
            // 既存のフェードアウトコルーチンを停止
            if (ambientFadeOutCoroutine != null)
            {
                StopCoroutine(ambientFadeOutCoroutine);
            }
            
            ambientFadeOutCoroutine = StartCoroutine(FadeOutAmbientSoundCoroutine(0.5f));
        }
        
        /// <summary>
        /// 環境音を結果画面用に長めにフェードアウト
        /// </summary>
        private void FadeOutAmbientSoundForResult()
        {
            if (ambientAudioSource == null || !ambientAudioSource.isPlaying) return;
            
            // 既存のフェードインコルーチンを停止
            if (ambientFadeInCoroutine != null)
            {
                StopCoroutine(ambientFadeInCoroutine);
            }
            
            // 既存のフェードアウトコルーチンを停止
            if (ambientFadeOutCoroutine != null)
            {
                StopCoroutine(ambientFadeOutCoroutine);
            }
            
            // 結果画面に移行する時は長めにフェードアウト（2秒）
            ambientFadeOutCoroutine = StartCoroutine(FadeOutAmbientSoundCoroutine(2f));
        }
        
        /// <summary>
        /// 環境音をフェードイン
        /// </summary>
        private IEnumerator FadeInAmbientSound(float duration)
        {
            if (ambientAudioSource == null || !ambientAudioSource.isPlaying) yield break;
            
            float targetVolume = 0.5f; // 環境音の目標音量
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                // 効果音やBGMが再生開始されたらフェードインを中断
                if (IsAnyAudioPlaying())
                {
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                ambientAudioSource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }
            
            ambientAudioSource.volume = targetVolume;
            ambientFadeInCoroutine = null;
        }
        
        /// <summary>
        /// 環境音をフェードアウト（コルーチン）
        /// </summary>
        private IEnumerator FadeOutAmbientSoundCoroutine(float duration)
        {
            if (ambientAudioSource == null) yield break;
            
            float startVolume = ambientAudioSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                ambientAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }
            
            ambientAudioSource.volume = 0f;

            // ボリュームが0になったら停止する
            ambientAudioSource.Stop();
            ambientFadeOutCoroutine = null;
        }
        
        /// <summary>
        /// 効果音が再生中かどうかをチェック
        /// </summary>
        private bool IsAnyAudioPlaying()
        {
            
            // 効果音が再生中かチェック
            if (sfxAudioSource != null && sfxAudioSource.isPlaying && sfxAudioSource.volume > 0.01f)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 効果音が停止したら環境音をフェードイン
        /// </summary>
        private IEnumerator CheckAndFadeInAmbientAfterSfx()
        {
            // 効果音のフェードアウトが完了するまで少し待つ
            yield return new WaitForSeconds(0.6f);
            
            // 効果音やBGMが再生中でない場合、環境音をフェードイン
            if (!IsAnyAudioPlaying() && ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientFadeInCoroutine = StartCoroutine(FadeInAmbientSound(1f));
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
            
            // アイコンを追加（画像が設定されている場合）
            if (icon != null)
            {
                var iconImage = new Image();
                iconImage.sprite = icon;
                iconImage.style.width = 24f;
                iconImage.style.height = 24f;
                iconImage.style.marginRight = 8f;
                container.Add(iconImage);
            }
            
            // テキストラベルを追加
            var label = new Label(text);
            label.style.fontSize = 16f;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(label);
            
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
                horizontalContainer.Add(rightSparkle);
            }
            
            container.Add(horizontalContainer);
        }
    }
}

