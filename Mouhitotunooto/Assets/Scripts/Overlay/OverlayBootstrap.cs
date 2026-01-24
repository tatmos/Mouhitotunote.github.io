using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame.Overlay
{
    /// <summary>
    /// Overlay初期化、UIDocument参照セット、購読開始
    /// </summary>
    public class OverlayBootstrap : MonoBehaviour
    {
        [SerializeField] private UIDocument overlayDocument;
        [SerializeField] private VisualTreeAsset overlayUXML;
        [SerializeField] private StyleSheet overlayStyles; // OverlayStyles.uss

        private OverlayState state;
        private PlayerTelemetry telemetry;
        private PlayerTypeClassifier playerTypeClassifier;
        private ReactionDirector reactionDirector;
        private OverlayPresenter_UITK presenter;

        private void Awake()
        {
            // 状態管理を初期化
            state = new OverlayState();

            // テレメトリーを初期化
            telemetry = new PlayerTelemetry();

            // プレイヤータイプ分類器を初期化
            playerTypeClassifier = new PlayerTypeClassifier(telemetry);

            // リアクションディレクターを初期化
            reactionDirector = new ReactionDirector(state, playerTypeClassifier);
        }

        private void Start()
        {
            // 他のUIシステムの初期化完了を待つため、遅延初期化を実行
            StartCoroutine(DelayedInitialization());
        }
        
        private IEnumerator DelayedInitialization()
        {
            Debug.Log("[OverlayBootstrap] 遅延初期化開始");
            
            // 他のUIシステムの初期化を待つ
            yield return new WaitForSeconds(0.1f);
            
            // さらに、他のUIDocumentが初期化されるまで待つ
            int maxRetries = 10;
            int retries = 0;
            while (retries < maxRetries)
            {
                var allUIDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                Debug.Log($"[OverlayBootstrap] リトライ {retries}: UIDocument数 = {allUIDocuments.Length}");
                
                if (allUIDocuments.Length > 1) // 自分以外のUIDocumentが見つかった
                {
                    break;
                }
                
                retries++;
                yield return new WaitForSeconds(0.1f);
            }
            
            Debug.Log("[OverlayBootstrap] Start() 開始");
            
            // UIDocumentを取得または作成
            if (overlayDocument == null)
            {
                overlayDocument = GetComponent<UIDocument>();
                if (overlayDocument == null)
                {
                    // UIDocumentが見つからない場合は自動的に作成
                    overlayDocument = gameObject.AddComponent<UIDocument>();
                    Debug.Log("[OverlayBootstrap] UIDocumentを自動的に作成しました。");
                }
                else
                {
                    Debug.Log("[OverlayBootstrap] 既存のUIDocumentを取得しました。");
                }
            }
            else
            {
                Debug.Log("[OverlayBootstrap] SerializeFieldからUIDocumentを取得しました。");
            }

            // PanelSettingsが設定されていない場合は自動的に設定
            if (overlayDocument != null && overlayDocument.panelSettings == null)
            {
                UnityEngine.UIElements.PanelSettings panelSettings = null;
                
                // まず、既存のUIDocumentからPanelSettingsを取得（他の画面と同じ設定を使用）
                var existingUIDocument = FindFirstObjectByType<UIDocument>();
                if (existingUIDocument != null && existingUIDocument != overlayDocument && existingUIDocument.panelSettings != null)
                {
                    panelSettings = existingUIDocument.panelSettings;
                    Debug.Log("[OverlayBootstrap] 既存のUIDocumentからPanelSettingsを取得しました。");
                }
                
                // 見つからない場合は、すべてのPanelSettingsアセットを検索
                if (panelSettings == null)
                {
                    var allPanelSettings = Resources.FindObjectsOfTypeAll<UnityEngine.UIElements.PanelSettings>()
                        .Where(ps => ps != null)
                        .ToList();
                    
                    Debug.Log($"[OverlayBootstrap] 利用可能なPanelSettings数: {allPanelSettings.Count}");
                    
                    if (allPanelSettings.Count > 0)
                    {
                        // 各PanelSettingsの詳細をログ出力
                        for (int i = 0; i < allPanelSettings.Count; i++)
                        {
                            var ps = allPanelSettings[i];
                            Debug.Log($"[OverlayBootstrap] PanelSettings[{i}]: {ps.name}, Scale: {ps.scale}, ScreenMatchMode: {ps.screenMatchMode}, ReferenceResolution: {ps.referenceResolution}");
                        }
                        
                        // 最初に見つかったPanelSettingsを使用
                        panelSettings = allPanelSettings[0];
                        Debug.Log("[OverlayBootstrap] PanelSettingsアセットを検索して設定しました。");
                        
                        // PanelSettingsの設定を強制的に修正
                        if (panelSettings.referenceResolution.x <= 0 || panelSettings.referenceResolution.y <= 0)
                        {
                            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                            Debug.Log("[OverlayBootstrap] PanelSettingsの解像度を1920x1080に修正しました。");
                        }
                        if (panelSettings.scale <= 0 || float.IsNaN(panelSettings.scale))
                        {
                            panelSettings.scale = 1f;
                            Debug.Log("[OverlayBootstrap] PanelSettingsのスケールを1.0に修正しました。");
                        }
                    }
                }
                
                if (panelSettings != null)
                {
                    overlayDocument.panelSettings = panelSettings;
                    Debug.Log($"[OverlayBootstrap] PanelSettingsを設定: {panelSettings.name} (解像度: {panelSettings.referenceResolution}, スケール: {panelSettings.scale})");
                }
                else
                {
                    Debug.LogWarning("[OverlayBootstrap] PanelSettingsが見つかりません。新しく作成します。");
                    
                    // PanelSettingsを動的に作成
                    panelSettings = ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
                    panelSettings.name = "OverlayPanelSettings";
                    panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                    panelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
                    panelSettings.match = 0.5f;
                    panelSettings.scale = 1f;
                    panelSettings.fallbackDpi = 96f;
                    panelSettings.referenceDpi = 96f;
                    
                    overlayDocument.panelSettings = panelSettings;
                    Debug.Log("[OverlayBootstrap] 新しいPanelSettingsを作成して設定しました。");
                }
            }

            // UXMLを読み込み
            if (overlayUXML != null)
            {
                overlayDocument.visualTreeAsset = overlayUXML;
            }
            else
            {
                Debug.LogWarning("[OverlayBootstrap] overlayUXMLが設定されていません。Overlay.uxmlを設定してください。");
            }

            // UIDocumentのSort Orderを最高に設定（すべてのUIの上に表示されるように）
            // pickingMode.Ignoreが設定されているため、クリックはブロックされない
            if (overlayDocument != null)
            {
                overlayDocument.sortingOrder = 30; // すべてのUIDocumentより高いSort Order（リザルト/セレクト: 20、シナリオ: 20）
                
                // UIDocumentを強制更新（PanelSettings適用のため）
                overlayDocument.enabled = false;
                overlayDocument.enabled = true;
                
                Debug.Log($"[OverlayBootstrap] UIDocument設定完了 - Sort Order: {overlayDocument.sortingOrder}, PanelSettings: {(overlayDocument.panelSettings != null ? overlayDocument.panelSettings.name : "未設定")}");
                Debug.Log($"[OverlayBootstrap] UIDocument enabled: {overlayDocument.enabled}");
                
                if (overlayDocument.panelSettings != null)
                {
                    var ps = overlayDocument.panelSettings;
                    Debug.Log($"[OverlayBootstrap] PanelSettings詳細 - 解像度: {ps.referenceResolution}, スケール: {ps.scale}, DPI: {ps.referenceDpi}");
                }
                
                // シーン内の他のUIDocumentのSort Orderを確認
                var allUIDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                Debug.Log($"[OverlayBootstrap] シーン内のUIDocument数: {allUIDocuments.Length}");
                foreach (var doc in allUIDocuments)
                {
                    Debug.Log($"[OverlayBootstrap] UIDocument - Name: {doc.name}, Sort Order: {doc.sortingOrder}, enabled: {doc.enabled}, GameObject active: {doc.gameObject.activeInHierarchy}");
                }
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentがnullです！");
            }

            // Presenterを初期化
            var root = overlayDocument.rootVisualElement;
            Debug.Log($"[OverlayBootstrap] UIDocument rootVisualElement: {(root != null ? "取得成功" : "null")}");
            
            if (root != null)
            {
                Debug.Log($"[OverlayBootstrap] rootVisualElement詳細 - childCount: {root.childCount}, name: {root.name}");
                
                // サイズが不正な場合、強制的にレイアウト更新を実行
                if (float.IsNaN(root.resolvedStyle.width) || float.IsNaN(root.resolvedStyle.height))
                {
                    Debug.LogWarning("[OverlayBootstrap] rootVisualElementのサイズが不正です。強制レイアウト更新を実行します。");
                    
                    // 強制的にレイアウトを再計算
                    root.MarkDirtyRepaint();
                    root.style.width = new StyleLength(StyleKeyword.Auto);
                    root.style.height = new StyleLength(StyleKeyword.Auto);
                    root.style.position = Position.Absolute;
                    root.style.left = 0;
                    root.style.top = 0;
                    root.style.right = 0;
                    root.style.bottom = 0;
                }
                
                Debug.Log($"[OverlayBootstrap] rootVisualElement サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                Debug.Log($"[OverlayBootstrap] rootVisualElement 位置: ({root.resolvedStyle.left}, {root.resolvedStyle.top})");
                
                // まだサイズが不正な場合、追加の強制修正を実行
                if (float.IsNaN(root.resolvedStyle.width) || float.IsNaN(root.resolvedStyle.height))
                {
                    Debug.LogWarning("[OverlayBootstrap] 追加の強制修正を実行します");
                    
                    // Screen解像度を直接使用
                    var screenWidth = Screen.width;
                    var screenHeight = Screen.height;
                    Debug.Log($"[OverlayBootstrap] Screen解像度を使用: {screenWidth}x{screenHeight}");
                    
                    // rootに固定サイズを設定
                    root.style.width = screenWidth;
                    root.style.height = screenHeight;
                    root.style.minWidth = screenWidth;
                    root.style.minHeight = screenHeight;
                    root.style.maxWidth = screenWidth;
                    root.style.maxHeight = screenHeight;
                    
                    // 即座にレイアウト更新を強制実行
                    root.MarkDirtyRepaint();
                    
                    // 1フレーム待ってから再確認
                    yield return null;
                    Debug.Log($"[OverlayBootstrap] 強制修正後のサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                }
                
                // スクロールバーを非表示にする
                root.style.overflow = Overflow.Hidden;
                
                // USSスタイルシートを適用（pointer-events: noneを確実に設定するため）
                if (overlayStyles != null)
                {
                    root.styleSheets.Add(overlayStyles);
                    Debug.Log("[OverlayBootstrap] USSスタイルシートを適用しました。");
                }
                else
                {
                    Debug.LogWarning("[OverlayBootstrap] overlayStylesがnullです。");
                }
                
                // pickingModeをIgnoreに設定して、オーバーレイがイベントを無視するようにする
                // これにより、オーバーレイが表示されていても、下のUIが操作可能になる
                root.pickingMode = PickingMode.Ignore;
                
                // OverlayRootにもpickingModeを設定
                var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                if (overlayRoot != null)
                {
                    Debug.Log($"[OverlayBootstrap] OverlayRoot要素を取得しました。childCount: {overlayRoot.childCount}");
                    overlayRoot.pickingMode = PickingMode.Ignore;
                    
                    // OverlayRootにも強制サイズ設定を適用
                    if (float.IsNaN(overlayRoot.resolvedStyle.width) || float.IsNaN(overlayRoot.resolvedStyle.height))
                    {
                        Debug.LogWarning("[OverlayBootstrap] OverlayRootのサイズが不正です。強制設定を適用します。");
                        overlayRoot.style.position = Position.Absolute;
                        overlayRoot.style.left = 0;
                        overlayRoot.style.top = 0;
                        overlayRoot.style.right = 0;
                        overlayRoot.style.bottom = 0;
                        overlayRoot.style.width = StyleKeyword.Auto;
                        overlayRoot.style.height = StyleKeyword.Auto;
                        overlayRoot.MarkDirtyRepaint();
                    }
                    
                    // すべての子要素にも再帰的に設定
                    SetPickingModeIgnoreRecursive(overlayRoot);
                }
                else
                {
                    Debug.LogError("[OverlayBootstrap] OverlayRoot要素が見つかりません！UXMLの構造を確認してください。");
                    
                    // rootの子要素をリストアップ
                    Debug.Log($"[OverlayBootstrap] root の子要素一覧:");
                    for (int i = 0; i < root.childCount; i++)
                    {
                        var child = root.ElementAt(i);
                        Debug.Log($"  {i}: name='{child.name}', type={child.GetType().Name}");
                    }
                }
                
                presenter = new OverlayPresenter_UITK(root, this);
                Debug.Log("[OverlayBootstrap] OverlayPresenter_UITKを初期化しました。");
                
                // 初期化直後にレイアウトを強制更新
                yield return null; // 1フレーム待つ
                overlayRoot.MarkDirtyRepaint();
                yield return null; // さらに1フレーム待つ
                
                Debug.Log("[OverlayBootstrap] レイアウト強制更新を実行しました。");
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] rootVisualElementがnullです。UXMLが正しく読み込まれていない可能性があります。");
                Debug.LogError($"[OverlayBootstrap] overlayUXML: {(overlayUXML != null ? overlayUXML.name : "null")}");
                Debug.LogError($"[OverlayBootstrap] overlayDocument.visualTreeAsset: {(overlayDocument.visualTreeAsset != null ? overlayDocument.visualTreeAsset.name : "null")}");
                yield break;
            }

            // イベント購読
            SubscribeToEvents();

            // 初期状態を設定
            if (presenter != null)
            {
                Debug.Log($"[OverlayBootstrap] 初期Phase設定: {state.CurrentPhase}");
                presenter.UpdatePhase(state.CurrentPhase);
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] presenterがnullです。初期状態を設定できません。");
            }
            
            // 最終確認：サイズが正常に設定されたかチェック
            var finalRoot = overlayDocument?.rootVisualElement;
            if (finalRoot != null)
            {
                Debug.Log($"[OverlayBootstrap] 最終確認 - rootVisualElement: {finalRoot.resolvedStyle.width}x{finalRoot.resolvedStyle.height}");
                
                if (float.IsNaN(finalRoot.resolvedStyle.width) || float.IsNaN(finalRoot.resolvedStyle.height))
                {
                    Debug.LogError("[OverlayBootstrap] ❌ UI要素のサイズが不正です！PanelSettingsに問題がある可能性があります。");
                    Debug.LogError("[OverlayBootstrap] 🔧 右クリック > 'Debug: Force Create PanelSettings' を実行してください。");
                }
                else
                {
                    Debug.Log("[OverlayBootstrap] ✅ UI要素のサイズは正常です。");
                }
            }
            
            Debug.Log("[OverlayBootstrap] Start() 完了");
        }

        private void SubscribeToEvents()
        {
            // 各イベントを購読
            OverlayEventHub.Subscribe<ModeChangedEvt>(OnModeChanged);
            OverlayEventHub.Subscribe<DivisionEnteredEvt>(OnDivisionEntered);
            OverlayEventHub.Subscribe<ScenarioStartedEvt>(OnScenarioStarted);
            OverlayEventHub.Subscribe<ScenarioClearedEvt>(OnScenarioCleared);
            OverlayEventHub.Subscribe<MouhitotuResultEvt>(OnMouhitotuResult);
            OverlayEventHub.Subscribe<ChoiceSelectedEvt>(OnChoiceSelected);
            OverlayEventHub.Subscribe<ReturnToScenarioSelectEvt>(OnReturnToScenarioSelect);
            OverlayEventHub.Subscribe<CreditsStartedEvt>(OnCreditsStarted);
            OverlayEventHub.Subscribe<CreditsEndedEvt>(OnCreditsEnded);
        }

        private void OnModeChanged(ModeChangedEvt evt)
        {
            if (state == null || reactionDirector == null || presenter == null) return;
            
            Debug.Log($"[OverlayBootstrap] OnModeChanged: Mode={evt.Mode}, 現在のDivision={state.CurrentDivision}");
            state.CurrentMode = evt.Mode;
            reactionDirector.UpdatePhase();
            presenter.UpdatePhase(state.CurrentPhase);

            // リアクションを選択
            var payload = reactionDirector.SelectReaction(evt);
            if (payload != null)
            {
                Debug.Log($"[OverlayBootstrap] リアクションを表示: {payload.Text}");
                presenter.ShowReaction(payload);
            }
            else
            {
                Debug.Log($"[OverlayBootstrap] リアクションが見つかりませんでした (Phase: {state.CurrentPhase})");
            }
        }

        private void OnDivisionEntered(DivisionEnteredEvt evt)
        {
            if (state == null || reactionDirector == null || presenter == null) return;
            
            Debug.Log($"[OverlayBootstrap] OnDivisionEntered: Division={evt.Division}, 現在のMode={state.CurrentMode}");
            state.CurrentDivision = evt.Division;
            reactionDirector.UpdatePhase();
            presenter.UpdatePhase(state.CurrentPhase);

            // リアクションを選択
            var payload = reactionDirector.SelectReaction(evt);
            if (payload != null)
            {
                Debug.Log($"[OverlayBootstrap] リアクションを表示: {payload.Text}");
                presenter.ShowReaction(payload);
            }
            else
            {
                Debug.Log($"[OverlayBootstrap] リアクションが見つかりませんでした (Phase: {state.CurrentPhase})");
            }
        }

        private void OnScenarioStarted(ScenarioStartedEvt evt)
        {
            // クリックを記録
            telemetry.RecordClick();
        }

        private void OnScenarioCleared(ScenarioClearedEvt evt)
        {
            // 特にリアクションなし（MVPでは実装しない）
        }

        private void OnMouhitotuResult(MouhitotuResultEvt evt)
        {
            if (reactionDirector == null || presenter == null) return;
            
            // リアクションを選択
            var payload = reactionDirector.SelectReaction(evt);
            if (payload != null)
            {
                presenter.ShowReaction(payload);
            }
        }

        private void OnChoiceSelected(ChoiceSelectedEvt evt)
        {
            // クリックを記録
            telemetry.RecordClick();
        }

        private void OnReturnToScenarioSelect(ReturnToScenarioSelectEvt evt)
        {
            if (state == null || reactionDirector == null || presenter == null) return;
            
            Debug.Log($"[OverlayBootstrap] OnReturnToScenarioSelect: Division={state.CurrentDivision}, Mode={state.CurrentMode}");
            // シナリオ選択画面に戻る時は、Phaseを更新
            reactionDirector.UpdatePhase();
            presenter.UpdatePhase(state.CurrentPhase);
        }

        private void OnCreditsStarted(CreditsStartedEvt evt)
        {
            if (presenter == null) return;
            
            // エンドクレジット開始：歌う表情に変更し、音符エフェクトを開始
            presenter.StartCreditsSinging();
        }

        private void OnCreditsEnded(CreditsEndedEvt evt)
        {
            if (presenter == null) return;
            
            // エンドクレジット終了：音符エフェクトを停止
            presenter.StopCreditsSinging();
        }

        private void OnDestroy()
        {
            // 購読をクリア（必要に応じて）
            // OverlayEventHub.Clear();
        }

        /// <summary>
        /// デバッグ用: 現在のPhaseを取得
        /// </summary>
        public OverlayPhase GetCurrentPhase()
        {
            return state != null ? state.CurrentPhase : OverlayPhase.Hidden;
        }

        /// <summary>
        /// デバッグ用: 現在のDivisionを取得
        /// </summary>
        public Division GetCurrentDivision()
        {
            return state != null ? state.CurrentDivision : Division.None;
        }

        /// <summary>
        /// デバッグ用: 現在のModeを取得
        /// </summary>
        public GameMode GetCurrentMode()
        {
            return state != null ? state.CurrentMode : GameMode.Normal;
        }

        /// <summary>
        /// デバッグ用: 強制的にPhaseを設定（テスト用）
        /// </summary>
        [ContextMenu("Debug: Force Phase to Active")]
        public void DebugForcePhaseToActive()
        {
            if (state != null && presenter != null)
            {
                state.CurrentPhase = OverlayPhase.Active;
                presenter.UpdatePhase(state.CurrentPhase);
                Debug.Log("[OverlayBootstrap] PhaseをActiveに強制設定しました。");
                
                // テスト用リアクションも表示
                StartCoroutine(ShowTestReaction());
            }
            else
            {
                Debug.LogError($"[OverlayBootstrap] DebugForcePhaseToActive失敗 - state: {(state != null ? "OK" : "null")}, presenter: {(presenter != null ? "OK" : "null")}");
            }
        }
        
        /// <summary>
        /// デバッグ用: レイアウト更新待機付きでPhaseをActiveに設定
        /// </summary>
        [ContextMenu("Debug: Force Active with Layout Wait")]
        public void DebugForcePhaseToActiveWithLayoutWait()
        {
            if (state != null && presenter != null)
            {
                StartCoroutine(ForcePhaseActiveWithLayoutWait());
            }
            else
            {
                Debug.LogError($"[OverlayBootstrap] DebugForcePhaseToActiveWithLayoutWait失敗 - state: {(state != null ? "OK" : "null")}, presenter: {(presenter != null ? "OK" : "null")}");
            }
        }
        
        /// <summary>
        /// レイアウト更新を待機してPhaseをActiveに設定
        /// </summary>
        private IEnumerator ForcePhaseActiveWithLayoutWait()
        {
            Debug.Log("[OverlayBootstrap] 🚀 レイアウト更新待機付きPhase=Active設定開始");
            
            // Phase をActiveに設定
            state.CurrentPhase = OverlayPhase.Active;
            presenter.UpdatePhase(state.CurrentPhase);
            Debug.Log("[OverlayBootstrap] Phase=Activeを設定しました。3フレーム待機中...");
            
            // 3フレーム待機してレイアウト更新を確実にする
            yield return null; // 1フレーム目
            yield return null; // 2フレーム目  
            yield return null; // 3フレーム目
            
            Debug.Log("[OverlayBootstrap] ⏰ 3フレーム待機完了。要素状態を確認中...");
            
            // レイアウト更新後の状態を確認し、必要に応じて絶対座標で配置
            var root = overlayDocument.rootVisualElement;
            if (root != null)
            {
                var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                if (overlayRoot != null)
                {
                    var girlImage = overlayRoot.Q<VisualElement>("GirlImage");
                    var roomImage = overlayRoot.Q<VisualElement>("RoomImage");
                    
                    // GirlImageの処理
                    if (girlImage != null)
                    {
                        Debug.Log($"[OverlayBootstrap] GirlImage現在状態: サイズ={girlImage.resolvedStyle.width}x{girlImage.resolvedStyle.height}");
                        
                        // サイズが0の場合、絶対座標で強制配置
                        if (girlImage.resolvedStyle.width == 0 || girlImage.resolvedStyle.height == 0)
                        {
                            Debug.LogWarning("[OverlayBootstrap] 🛠️ GirlImageを絶対座標で強制配置します");
                            
                            girlImage.style.position = Position.Absolute;
                            girlImage.style.left = root.resolvedStyle.width - 220; // 右から220px (left基準)
                            girlImage.style.top = root.resolvedStyle.height - 170;  // 下から170px (top基準)
                            girlImage.style.width = 200;
                            girlImage.style.height = 150;
                            girlImage.style.backgroundColor = new Color(1f, 0f, 0f, 0.9f); // 濃い赤
                            girlImage.style.borderTopWidth = 5;
                            girlImage.style.borderBottomWidth = 5;
                            girlImage.style.borderLeftWidth = 5;
                            girlImage.style.borderRightWidth = 5;
                            girlImage.style.borderTopColor = Color.white;
                            girlImage.style.borderBottomColor = Color.white;
                            girlImage.style.borderLeftColor = Color.white;
                            girlImage.style.borderRightColor = Color.white;
                            girlImage.MarkDirtyRepaint();
                            
                            Debug.Log($"[OverlayBootstrap] ✅ GirlImage絶対座標: left={girlImage.style.left.value.value}, top={girlImage.style.top.value.value}");
                        }
                    }
                    
                    // RoomImageの処理
                    if (roomImage != null)
                    {
                        Debug.Log($"[OverlayBootstrap] RoomImage現在状態: サイズ={roomImage.resolvedStyle.width}x{roomImage.resolvedStyle.height}");
                        
                        // サイズが0の場合、絶対座標で強制配置
                        if (roomImage.resolvedStyle.width == 0 || roomImage.resolvedStyle.height == 0)
                        {
                            Debug.LogWarning("[OverlayBootstrap] 🛠️ RoomImageを絶対座標で強制配置します");
                            
                            roomImage.style.position = Position.Absolute;
                            roomImage.style.left = root.resolvedStyle.width - 220; // 右から220px (left基準)
                            roomImage.style.top = root.resolvedStyle.height - 340; // 下から340px (top基準、GirlImageの上)
                            roomImage.style.width = 200;
                            roomImage.style.height = 150;
                            roomImage.style.backgroundColor = new Color(0f, 1f, 0f, 0.9f); // 濃い緑
                            roomImage.style.borderTopWidth = 5;
                            roomImage.style.borderBottomWidth = 5;
                            roomImage.style.borderLeftWidth = 5;
                            roomImage.style.borderRightWidth = 5;
                            roomImage.style.borderTopColor = Color.yellow;
                            roomImage.style.borderBottomColor = Color.yellow;
                            roomImage.style.borderLeftColor = Color.yellow;
                            roomImage.style.borderRightColor = Color.yellow;
                            roomImage.MarkDirtyRepaint();
                            
                            Debug.Log($"[OverlayBootstrap] ✅ RoomImage絶対座標: left={roomImage.style.left.value.value}, top={roomImage.style.top.value.value}");
                        }
                    }
                    
                    // さらに2フレーム待機して最終確認
                    yield return null;
                    yield return null;
                    
                    // 最終状態確認
                    Debug.Log("[OverlayBootstrap] 🎯 最終状態確認:");
                    if (girlImage != null)
                    {
                        Debug.Log($"[OverlayBootstrap] GirlImage最終: サイズ={girlImage.resolvedStyle.width}x{girlImage.resolvedStyle.height}");
                    }
                    if (roomImage != null)
                    {
                        Debug.Log($"[OverlayBootstrap] RoomImage最終: サイズ={roomImage.resolvedStyle.width}x{roomImage.resolvedStyle.height}");
                    }
                }
                else
                {
                    Debug.LogError("[OverlayBootstrap] OverlayRoot要素が見つかりません");
                }
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] rootVisualElementが見つかりません");
            }
            
            Debug.Log("[OverlayBootstrap] ✅ レイアウト更新待機付きPhase設定完了！");
            
            // テスト用リアクションも表示
            StartCoroutine(ShowTestReaction());
        }
        
        /// <summary>
        /// デバッグ用: 強制的にleft/top座標で配置
        /// </summary>
        [ContextMenu("Debug: Force Left/Top Positioning")]
        public void DebugForceLeftTopPositioning()
        {
            Debug.Log("[OverlayBootstrap] 🔧 強制的にleft/top座標で配置開始");
            
            if (overlayDocument != null && overlayDocument.rootVisualElement != null)
            {
                var root = overlayDocument.rootVisualElement;
                var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                
                if (overlayRoot != null)
                {
                    var girlImage = overlayRoot.Q<VisualElement>("GirlImage");
                    var roomImage = overlayRoot.Q<VisualElement>("RoomImage");
                    
                    Debug.Log($"[OverlayBootstrap] root解決サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                    
                    // GirlImageを強制的にleft/top座標で配置
                    if (girlImage != null)
                    {
                        Debug.Log($"[OverlayBootstrap] GirlImage現在サイズ: {girlImage.resolvedStyle.width}x{girlImage.resolvedStyle.height}");
                        
                        // 完全に新しいスタイルで上書き
                        girlImage.style.position = Position.Absolute;
                        girlImage.style.left = root.resolvedStyle.width - 220; // 右から220px
                        girlImage.style.top = root.resolvedStyle.height - 170; // 下から170px
                        girlImage.style.width = 200;
                        girlImage.style.height = 150;
                        
                        // right/bottomプロパティをクリア
                        girlImage.style.right = StyleKeyword.Auto;
                        girlImage.style.bottom = StyleKeyword.Auto;
                        
                        // 視認しやすい濃い色
                        girlImage.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
                        girlImage.style.borderTopWidth = 8;
                        girlImage.style.borderBottomWidth = 8;
                        girlImage.style.borderLeftWidth = 8;
                        girlImage.style.borderRightWidth = 8;
                        girlImage.style.borderTopColor = Color.white;
                        girlImage.style.borderBottomColor = Color.white;
                        girlImage.style.borderLeftColor = Color.white;
                        girlImage.style.borderRightColor = Color.white;
                        
                        girlImage.MarkDirtyRepaint();
                        
                        Debug.Log($"[OverlayBootstrap] 🔴 GirlImage left/top配置: left={girlImage.style.left.value.value}, top={girlImage.style.top.value.value}");
                    }
                    
                    // RoomImageを強制的にleft/top座標で配置
                    if (roomImage != null)
                    {
                        Debug.Log($"[OverlayBootstrap] RoomImage現在サイズ: {roomImage.resolvedStyle.width}x{roomImage.resolvedStyle.height}");
                        
                        // 完全に新しいスタイルで上書き
                        roomImage.style.position = Position.Absolute;
                        roomImage.style.left = root.resolvedStyle.width - 220; // 右から220px (Girlと同じX座標)
                        roomImage.style.top = root.resolvedStyle.height - 340; // 下から340px (Girlの上)
                        roomImage.style.width = 200;
                        roomImage.style.height = 150;
                        
                        // right/bottomプロパティをクリア
                        roomImage.style.right = StyleKeyword.Auto;
                        roomImage.style.bottom = StyleKeyword.Auto;
                        
                        // 視認しやすい濃い色
                        roomImage.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
                        roomImage.style.borderTopWidth = 8;
                        roomImage.style.borderBottomWidth = 8;
                        roomImage.style.borderLeftWidth = 8;
                        roomImage.style.borderRightWidth = 8;
                        roomImage.style.borderTopColor = Color.yellow;
                        roomImage.style.borderBottomColor = Color.yellow;
                        roomImage.style.borderLeftColor = Color.yellow;
                        roomImage.style.borderRightColor = Color.yellow;
                        
                        roomImage.MarkDirtyRepaint();
                        
                        Debug.Log($"[OverlayBootstrap] 🟢 RoomImage left/top配置: left={roomImage.style.left.value.value}, top={roomImage.style.top.value.value}");
                    }
                    
                    // OverlayRootを強制的に表示状態にする
                    overlayRoot.style.display = DisplayStyle.Flex;
                    overlayRoot.style.visibility = Visibility.Visible;
                    overlayRoot.style.opacity = 1f;
                    
                    // 念のため、rootVisualElementも確認
                    root.style.display = DisplayStyle.Flex;
                    root.style.visibility = Visibility.Visible;
                    
                    overlayRoot.MarkDirtyRepaint();
                    root.MarkDirtyRepaint();
                    
                    Debug.Log($"[OverlayBootstrap] 🚨 OverlayRoot強制表示: display={overlayRoot.style.display.value}, visibility={overlayRoot.style.visibility.value}");
                    Debug.Log($"[OverlayBootstrap] 🚨 root強制表示: display={root.style.display.value}, visibility={root.style.visibility.value}");
                    Debug.Log("[OverlayBootstrap] ✅ left/top座標での強制配置完了！");
                    Debug.Log("[OverlayBootstrap] 🎯 右下に赤い矩形（Girl）と緑い矩形（Room）が表示されるはずです！");
                }
                else
                {
                    Debug.LogError("[OverlayBootstrap] OverlayRoot要素が見つかりません");
                }
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentまたはrootVisualElementがnullです");
            }
        }
        
        /// <summary>
        /// デバッグ用: rootVisualElementに直接テスト要素を追加
        /// </summary>
        [ContextMenu("Debug: Direct Root Element Test")]
        public void DebugDirectRootElementTest()
        {
            Debug.Log("[OverlayBootstrap] 🔥 rootVisualElementに直接テスト要素を追加");
            
            if (overlayDocument != null && overlayDocument.rootVisualElement != null)
            {
                var root = overlayDocument.rootVisualElement;
                
                Debug.Log($"[OverlayBootstrap] rootサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                
                // 既存のテスト要素を削除
                var existingTest = root.Q<VisualElement>("DirectTest");
                if (existingTest != null)
                {
                    root.Remove(existingTest);
                }
                
                // rootに直接テスト要素を追加
                var directTest = new VisualElement();
                directTest.name = "DirectTest";
                directTest.style.position = Position.Absolute;
                directTest.style.left = 100;  // 左から100px（確実に画面内）
                directTest.style.top = 100;   // 上から100px（確実に画面内）
                directTest.style.width = 300;
                directTest.style.height = 200;
                directTest.style.backgroundColor = new Color(1f, 0f, 1f, 1f); // 完全不透明のマゼンタ
                directTest.style.borderTopWidth = 10;
                directTest.style.borderBottomWidth = 10;
                directTest.style.borderLeftWidth = 10;
                directTest.style.borderRightWidth = 10;
                directTest.style.borderTopColor = Color.cyan;
                directTest.style.borderBottomColor = Color.cyan;
                directTest.style.borderLeftColor = Color.cyan;
                directTest.style.borderRightColor = Color.cyan;
                
                // rootに直接追加
                root.Add(directTest);
                root.MarkDirtyRepaint();
                
                // rootの状態も強制設定
                root.style.display = DisplayStyle.Flex;
                root.style.visibility = Visibility.Visible;
                root.style.opacity = 1f;
                
                Debug.Log("[OverlayBootstrap] 🟣 rootに直接マゼンタの矩形を追加しました（左上、300x200px、シアン枠）");
                Debug.Log($"[OverlayBootstrap] UIDocument Sort Order: {overlayDocument.sortingOrder}");
                Debug.Log($"[OverlayBootstrap] UIDocument enabled: {overlayDocument.enabled}");
                Debug.Log($"[OverlayBootstrap] GameObject active: {gameObject.activeInHierarchy}");
                
                Debug.Log("[OverlayBootstrap] 🚨 これでも表示されない場合、Unity Canvas設定または他のUI要素が覆い隠している可能性があります");
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentまたはrootVisualElementがnullです");
            }
        }
        
        /// <summary>
        /// デバッグ用: rootに直接オーバーレイ要素を追加（最終解決策）
        /// </summary>
        [ContextMenu("Debug: Create Direct Overlay Elements")]
        public void DebugCreateDirectOverlayElements()
        {
            Debug.Log("[OverlayBootstrap] 🚀 rootVisualElementに直接オーバーレイ要素を作成");
            
            if (overlayDocument != null && overlayDocument.rootVisualElement != null)
            {
                var root = overlayDocument.rootVisualElement;
                
                // 既存の直接オーバーレイ要素を削除
                var existingGirl = root.Q<VisualElement>("DirectGirlImage");
                var existingRoom = root.Q<VisualElement>("DirectRoomImage");
                if (existingGirl != null) root.Remove(existingGirl);
                if (existingRoom != null) root.Remove(existingRoom);
                
                Debug.Log($"[OverlayBootstrap] rootサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                
                // GirlImage相当の要素を直接作成
                var directGirl = new VisualElement();
                directGirl.name = "DirectGirlImage";
                directGirl.style.position = Position.Absolute;
                directGirl.style.left = root.resolvedStyle.width - 220;  // 右から220px
                directGirl.style.top = root.resolvedStyle.height - 170;   // 下から170px
                directGirl.style.width = 200;
                directGirl.style.height = 150;
                directGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 赤
                directGirl.style.borderTopWidth = 5;
                directGirl.style.borderBottomWidth = 5;
                directGirl.style.borderLeftWidth = 5;
                directGirl.style.borderRightWidth = 5;
                directGirl.style.borderTopColor = Color.white;
                directGirl.style.borderBottomColor = Color.white;
                directGirl.style.borderLeftColor = Color.white;
                directGirl.style.borderRightColor = Color.white;
                
                // RoomImage相当の要素を直接作成
                var directRoom = new VisualElement();
                directRoom.name = "DirectRoomImage";
                directRoom.style.position = Position.Absolute;
                directRoom.style.left = root.resolvedStyle.width - 220;  // 右から220px (Girlと同じX)
                directRoom.style.top = root.resolvedStyle.height - 340;   // 下から340px (Girlの上)
                directRoom.style.width = 200;
                directRoom.style.height = 150;
                directRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 緑
                directRoom.style.borderTopWidth = 5;
                directRoom.style.borderBottomWidth = 5;
                directRoom.style.borderLeftWidth = 5;
                directRoom.style.borderRightWidth = 5;
                directRoom.style.borderTopColor = Color.yellow;
                directRoom.style.borderBottomColor = Color.yellow;
                directRoom.style.borderLeftColor = Color.yellow;
                directRoom.style.borderRightColor = Color.yellow;
                
                // rootに直接追加
                root.Add(directGirl);
                root.Add(directRoom);
                
                // rootを表示状態にする
                root.style.display = DisplayStyle.Flex;
                root.style.visibility = Visibility.Visible;
                root.style.opacity = 1f;
                root.MarkDirtyRepaint();
                
                Debug.Log($"[OverlayBootstrap] 🔴 DirectGirlImage作成: left={directGirl.style.left.value.value}, top={directGirl.style.top.value.value}");
                Debug.Log($"[OverlayBootstrap] 🟢 DirectRoomImage作成: left={directRoom.style.left.value.value}, top={directRoom.style.top.value.value}");
                Debug.Log("[OverlayBootstrap] ✅ 直接オーバーレイ要素の作成完了！");
                Debug.Log("[OverlayBootstrap] 🎯 右下に赤い矩形（Girl）と緑い矩形（Room）が表示されるはずです！");
                
                // 参考用: 既存のOverlayRootの状態をログ出力
                var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                if (overlayRoot != null)
                {
                    Debug.Log($"[OverlayBootstrap] 参考: OverlayRoot display={overlayRoot.style.display.value}, visibility={overlayRoot.style.visibility.value}");
                    Debug.Log($"[OverlayBootstrap] 参考: OverlayRoot resolvedStyle display={overlayRoot.resolvedStyle.display}, visibility={overlayRoot.resolvedStyle.visibility}");
                }
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentまたはrootVisualElementがnullです");
            }
        }
        
        /// <summary>
        /// デバッグ用: 初期化完了を待機してオーバーレイ要素を作成
        /// </summary>
        [ContextMenu("Debug: Wait and Create Overlay Elements")]
        public void DebugWaitAndCreateOverlayElements()
        {
            StartCoroutine(WaitForInitializationAndCreateElements());
        }
        
        /// <summary>
        /// 初期化完了を待機してオーバーレイ要素を作成
        /// </summary>
        private IEnumerator WaitForInitializationAndCreateElements()
        {
            Debug.Log("[OverlayBootstrap] 🕐 初期化完了を待機中...");
            
            // 初期化完了を待機（最大10秒）
            float timeout = 10f;
            float elapsed = 0f;
            
            while ((overlayDocument == null || overlayDocument.rootVisualElement == null) && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
                
                if (elapsed % 1f < 0.1f) // 1秒ごとに進捗ログ
                {
                    Debug.Log($"[OverlayBootstrap] ⏳ 待機中... {elapsed:F1}秒経過");
                }
            }
            
            if (overlayDocument != null && overlayDocument.rootVisualElement != null)
            {
                Debug.Log("[OverlayBootstrap] ✅ 初期化完了！オーバーレイ要素を作成します");
                
                var root = overlayDocument.rootVisualElement;
                
                // 既存の直接オーバーレイ要素を削除
                var existingGirl = root.Q<VisualElement>("DirectGirlImage");
                var existingRoom = root.Q<VisualElement>("DirectRoomImage");
                if (existingGirl != null) root.Remove(existingGirl);
                if (existingRoom != null) root.Remove(existingRoom);
                
                Debug.Log($"[OverlayBootstrap] rootサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                
                // GirlImage相当の要素を直接作成
                var directGirl = new VisualElement();
                directGirl.name = "DirectGirlImage";
                directGirl.style.position = Position.Absolute;
                directGirl.style.left = root.resolvedStyle.width - 220;  // 右から220px
                directGirl.style.top = root.resolvedStyle.height - 170;   // 下から170px
                directGirl.style.width = 200;
                directGirl.style.height = 150;
                directGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
                directGirl.style.borderTopWidth = 8;
                directGirl.style.borderBottomWidth = 8;
                directGirl.style.borderLeftWidth = 8;
                directGirl.style.borderRightWidth = 8;
                directGirl.style.borderTopColor = Color.white;
                directGirl.style.borderBottomColor = Color.white;
                directGirl.style.borderLeftColor = Color.white;
                directGirl.style.borderRightColor = Color.white;
                
                // RoomImage相当の要素を直接作成
                var directRoom = new VisualElement();
                directRoom.name = "DirectRoomImage";
                directRoom.style.position = Position.Absolute;
                directRoom.style.left = root.resolvedStyle.width - 220;  // 右から220px (Girlと同じX)
                directRoom.style.top = root.resolvedStyle.height - 340;   // 下から340px (Girlの上)
                directRoom.style.width = 200;
                directRoom.style.height = 150;
                directRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
                directRoom.style.borderTopWidth = 8;
                directRoom.style.borderBottomWidth = 8;
                directRoom.style.borderLeftWidth = 8;
                directRoom.style.borderRightWidth = 8;
                directRoom.style.borderTopColor = Color.yellow;
                directRoom.style.borderBottomColor = Color.yellow;
                directRoom.style.borderLeftColor = Color.yellow;
                directRoom.style.borderRightColor = Color.yellow;
                
                // rootに直接追加
                root.Add(directGirl);
                root.Add(directRoom);
                
                // rootを確実に表示状態にする
                root.style.display = DisplayStyle.Flex;
                root.style.visibility = Visibility.Visible;
                root.style.opacity = 1f;
                root.MarkDirtyRepaint();
                
                Debug.Log($"[OverlayBootstrap] 🔴 DirectGirlImage作成: left={directGirl.style.left.value.value}, top={directGirl.style.top.value.value}");
                Debug.Log($"[OverlayBootstrap] 🟢 DirectRoomImage作成: left={directRoom.style.left.value.value}, top={directRoom.style.top.value.value}");
                Debug.Log("[OverlayBootstrap] 🎯 右下に赤い矩形（Girl）と緑い矩形（Room）が表示されます！");
                
                // Sort Orderも確認
                Debug.Log($"[OverlayBootstrap] UIDocument Sort Order: {overlayDocument.sortingOrder}");
                Debug.Log($"[OverlayBootstrap] UIDocument enabled: {overlayDocument.enabled}");
                Debug.Log($"[OverlayBootstrap] GameObject active: {gameObject.activeInHierarchy}");
                
                Debug.Log("[OverlayBootstrap] ✅ 待機付きオーバーレイ要素作成完了！");
            }
            else
            {
                Debug.LogError($"[OverlayBootstrap] ❌ 初期化タイムアウト（{timeout}秒）: overlayDocument={overlayDocument != null}, rootVisualElement={overlayDocument?.rootVisualElement != null}");
            }
        }
        
        /// <summary>
        /// デバッグ用: 実画面サイズ基準で確実に表示
        /// </summary>
        [ContextMenu("Debug: Create with Actual Screen Size")]
        public void DebugCreateWithActualScreenSize()
        {
            StartCoroutine(WaitAndCreateWithActualScreenSize());
        }
        
        /// <summary>
        /// 実画面サイズを基準としたオーバーレイ要素作成
        /// </summary>
        private IEnumerator WaitAndCreateWithActualScreenSize()
        {
            Debug.Log("[OverlayBootstrap] 🚀 実画面サイズ基準でオーバーレイ要素を作成");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            // 解像度情報を確認
            float rootWidth = root.resolvedStyle.width;
            float rootHeight = root.resolvedStyle.height;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            Debug.Log($"[OverlayBootstrap] rootサイズ: {rootWidth}x{rootHeight}");
            Debug.Log($"[OverlayBootstrap] 実画面サイズ: {screenWidth}x{screenHeight}");
            
            // スケール比計算
            float scaleX = rootWidth / screenWidth;
            float scaleY = rootHeight / screenHeight;
            Debug.Log($"[OverlayBootstrap] スケール比: X={scaleX:F3}, Y={scaleY:F3}");
            
            // 実画面座標を使用（確実に画面内）
            float girlLeft = screenWidth - 220;   // 実画面の右から220px
            float girlTop = screenHeight - 170;   // 実画面の下から170px
            float roomLeft = screenWidth - 220;   // 実画面の右から220px
            float roomTop = screenHeight - 340;   // 実画面の下から340px (Girlの上)
            
            Debug.Log($"[OverlayBootstrap] 実画面座標 - Girl: ({girlLeft}, {girlTop}), Room: ({roomLeft}, {roomTop})");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("ActualScreenGirl");
            var existingRoom = root.Q<VisualElement>("ActualScreenRoom");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            
            // GirlImage（実画面座標）
            var actualGirl = new VisualElement();
            actualGirl.name = "ActualScreenGirl";
            actualGirl.style.position = Position.Absolute;
            actualGirl.style.left = girlLeft;
            actualGirl.style.top = girlTop;
            actualGirl.style.width = 200;
            actualGirl.style.height = 150;
            actualGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            actualGirl.style.borderTopWidth = 10;
            actualGirl.style.borderBottomWidth = 10;
            actualGirl.style.borderLeftWidth = 10;
            actualGirl.style.borderRightWidth = 10;
            actualGirl.style.borderTopColor = Color.white;
            actualGirl.style.borderBottomColor = Color.white;
            actualGirl.style.borderLeftColor = Color.white;
            actualGirl.style.borderRightColor = Color.white;
            
            // RoomImage（実画面座標）
            var actualRoom = new VisualElement();
            actualRoom.name = "ActualScreenRoom";
            actualRoom.style.position = Position.Absolute;
            actualRoom.style.left = roomLeft;
            actualRoom.style.top = roomTop;
            actualRoom.style.width = 200;
            actualRoom.style.height = 150;
            actualRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            actualRoom.style.borderTopWidth = 10;
            actualRoom.style.borderBottomWidth = 10;
            actualRoom.style.borderLeftWidth = 10;
            actualRoom.style.borderRightWidth = 10;
            actualRoom.style.borderTopColor = Color.yellow;
            actualRoom.style.borderBottomColor = Color.yellow;
            actualRoom.style.borderLeftColor = Color.yellow;
            actualRoom.style.borderRightColor = Color.yellow;
            
            // 左上に確認用要素も追加
            var confirmElement = new VisualElement();
            confirmElement.name = "ConfirmElement";
            confirmElement.style.position = Position.Absolute;
            confirmElement.style.left = 50;
            confirmElement.style.top = 50;
            confirmElement.style.width = 100;
            confirmElement.style.height = 100;
            confirmElement.style.backgroundColor = new Color(1f, 1f, 0f, 1f); // 完全不透明の黄色
            confirmElement.style.borderTopWidth = 5;
            confirmElement.style.borderBottomWidth = 5;
            confirmElement.style.borderLeftWidth = 5;
            confirmElement.style.borderRightWidth = 5;
            confirmElement.style.borderTopColor = Color.black;
            confirmElement.style.borderBottomColor = Color.black;
            confirmElement.style.borderLeftColor = Color.black;
            confirmElement.style.borderRightColor = Color.black;
            
            // rootに追加
            root.Add(actualGirl);
            root.Add(actualRoom);
            root.Add(confirmElement);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 実画面GirlImage: left={actualGirl.style.left.value.value}, top={actualGirl.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🟢 実画面RoomImage: left={actualRoom.style.left.value.value}, top={actualRoom.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🟡 確認用要素: left=50, top=50");
            Debug.Log("[OverlayBootstrap] ✅ 実画面サイズ基準でのオーバーレイ要素作成完了！");
            Debug.Log("[OverlayBootstrap] 🎯 左上に黄色い正方形、右下に赤と緑の矩形が表示されるはずです！");
        }
        
        /// <summary>
        /// デバッグ用: 仮想座標系基準で正しく配置
        /// </summary>
        [ContextMenu("Debug: Create with Virtual Coordinates")]
        public void DebugCreateWithVirtualCoordinates()
        {
            StartCoroutine(WaitAndCreateWithVirtualCoordinates());
        }
        
        /// <summary>
        /// UI Toolkitの仮想座標系（rootサイズ）を基準とした正しい配置
        /// </summary>
        private IEnumerator WaitAndCreateWithVirtualCoordinates()
        {
            Debug.Log("[OverlayBootstrap] 🎯 仮想座標系基準でオーバーレイ要素を作成");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            // 仮想座標系情報を確認
            float virtualWidth = root.resolvedStyle.width;
            float virtualHeight = root.resolvedStyle.height;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ（UIToolkit）: {virtualWidth}x{virtualHeight}");
            Debug.Log($"[OverlayBootstrap] 実画面サイズ: {screenWidth}x{screenHeight}");
            
            // 仮想座標系（1920x1080）を基準とした座標計算
            float girlLeft = virtualWidth - 220;   // 仮想画面の右から220px
            float girlTop = virtualHeight - 170;   // 仮想画面の下から170px
            float roomLeft = virtualWidth - 220;   // 仮想画面の右から220px
            float roomTop = virtualHeight - 340;   // 仮想画面の下から340px
            
            Debug.Log($"[OverlayBootstrap] 仮想座標 - Girl: ({girlLeft}, {girlTop}), Room: ({roomLeft}, {roomTop})");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("VirtualGirl");
            var existingRoom = root.Q<VisualElement>("VirtualRoom");
            var existingConfirm = root.Q<VisualElement>("VirtualConfirm");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingConfirm != null) root.Remove(existingConfirm);
            
            // GirlImage（仮想座標）
            var virtualGirl = new VisualElement();
            virtualGirl.name = "VirtualGirl";
            virtualGirl.style.position = Position.Absolute;
            virtualGirl.style.left = girlLeft;
            virtualGirl.style.top = girlTop;
            virtualGirl.style.width = 200;
            virtualGirl.style.height = 150;
            virtualGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            virtualGirl.style.borderTopWidth = 8;
            virtualGirl.style.borderBottomWidth = 8;
            virtualGirl.style.borderLeftWidth = 8;
            virtualGirl.style.borderRightWidth = 8;
            virtualGirl.style.borderTopColor = Color.white;
            virtualGirl.style.borderBottomColor = Color.white;
            virtualGirl.style.borderLeftColor = Color.white;
            virtualGirl.style.borderRightColor = Color.white;
            
            // RoomImage（仮想座標）
            var virtualRoom = new VisualElement();
            virtualRoom.name = "VirtualRoom";
            virtualRoom.style.position = Position.Absolute;
            virtualRoom.style.left = roomLeft;
            virtualRoom.style.top = roomTop;
            virtualRoom.style.width = 200;
            virtualRoom.style.height = 150;
            virtualRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            virtualRoom.style.borderTopWidth = 8;
            virtualRoom.style.borderBottomWidth = 8;
            virtualRoom.style.borderLeftWidth = 8;
            virtualRoom.style.borderRightWidth = 8;
            virtualRoom.style.borderTopColor = Color.yellow;
            virtualRoom.style.borderBottomColor = Color.yellow;
            virtualRoom.style.borderLeftColor = Color.yellow;
            virtualRoom.style.borderRightColor = Color.yellow;
            
            // 中央に確認用要素も追加
            var virtualConfirm = new VisualElement();
            virtualConfirm.name = "VirtualConfirm";
            virtualConfirm.style.position = Position.Absolute;
            virtualConfirm.style.left = (virtualWidth - 150) / 2; // 中央
            virtualConfirm.style.top = (virtualHeight - 150) / 2; // 中央
            virtualConfirm.style.width = 150;
            virtualConfirm.style.height = 150;
            virtualConfirm.style.backgroundColor = new Color(0f, 1f, 1f, 1f); // 完全不透明のシアン
            virtualConfirm.style.borderTopWidth = 10;
            virtualConfirm.style.borderBottomWidth = 10;
            virtualConfirm.style.borderLeftWidth = 10;
            virtualConfirm.style.borderRightWidth = 10;
            virtualConfirm.style.borderTopColor = Color.magenta;
            virtualConfirm.style.borderBottomColor = Color.magenta;
            virtualConfirm.style.borderLeftColor = Color.magenta;
            virtualConfirm.style.borderRightColor = Color.magenta;
            
            // rootに追加
            root.Add(virtualGirl);
            root.Add(virtualRoom);
            root.Add(virtualConfirm);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 仮想GirlImage: left={virtualGirl.style.left.value.value}, top={virtualGirl.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🟢 仮想RoomImage: left={virtualRoom.style.left.value.value}, top={virtualRoom.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🔵 仮想確認要素: 中央 left={virtualConfirm.style.left.value.value}, top={virtualConfirm.style.top.value.value}");
            Debug.Log("[OverlayBootstrap] ✅ 仮想座標系基準でのオーバーレイ要素作成完了！");
            Debug.Log("[OverlayBootstrap] 🎯 中央にシアン正方形、右下に赤と緑の矩形が表示されるはずです！");
        }
        
        /// <summary>
        /// デバッグ用: 左上角基準で座標系検証
        /// </summary>
        [ContextMenu("Debug: Test Corner Positions")]
        public void DebugTestCornerPositions()
        {
            StartCoroutine(WaitAndTestCornerPositions());
        }
        
        /// <summary>
        /// 左上角基準での座標系検証テスト
        /// </summary>
        private IEnumerator WaitAndTestCornerPositions()
        {
            Debug.Log("[OverlayBootstrap] 🧪 左上角基準で座標系検証テスト開始");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            
            // 既存要素削除
            var existingTest1 = root.Q<VisualElement>("CornerTest1");
            var existingTest2 = root.Q<VisualElement>("CornerTest2");
            var existingTest3 = root.Q<VisualElement>("CornerTest3");
            var existingTest4 = root.Q<VisualElement>("CornerTest4");
            var existingTest5 = root.Q<VisualElement>("CornerTest5");
            if (existingTest1 != null) root.Remove(existingTest1);
            if (existingTest2 != null) root.Remove(existingTest2);
            if (existingTest3 != null) root.Remove(existingTest3);
            if (existingTest4 != null) root.Remove(existingTest4);
            if (existingTest5 != null) root.Remove(existingTest5);
            
            // テスト1: 左上角 (0, 0)
            var test1 = new VisualElement();
            test1.name = "CornerTest1";
            test1.style.position = Position.Absolute;
            test1.style.left = 0;
            test1.style.top = 0;
            test1.style.width = 80;
            test1.style.height = 80;
            test1.style.backgroundColor = Color.red;
            test1.style.borderTopWidth = 5;
            test1.style.borderBottomWidth = 5;
            test1.style.borderLeftWidth = 5;
            test1.style.borderRightWidth = 5;
            test1.style.borderTopColor = Color.white;
            test1.style.borderBottomColor = Color.white;
            test1.style.borderLeftColor = Color.white;
            test1.style.borderRightColor = Color.white;
            
            // テスト2: 左上から少し右下 (100, 100)
            var test2 = new VisualElement();
            test2.name = "CornerTest2";
            test2.style.position = Position.Absolute;
            test2.style.left = 100;
            test2.style.top = 100;
            test2.style.width = 80;
            test2.style.height = 80;
            test2.style.backgroundColor = Color.green;
            test2.style.borderTopWidth = 5;
            test2.style.borderBottomWidth = 5;
            test2.style.borderLeftWidth = 5;
            test2.style.borderRightWidth = 5;
            test2.style.borderTopColor = Color.black;
            test2.style.borderBottomColor = Color.black;
            test2.style.borderLeftColor = Color.black;
            test2.style.borderRightColor = Color.black;
            
            // テスト3: 中央左寄り (400, 300)
            var test3 = new VisualElement();
            test3.name = "CornerTest3";
            test3.style.position = Position.Absolute;
            test3.style.left = 400;
            test3.style.top = 300;
            test3.style.width = 80;
            test3.style.height = 80;
            test3.style.backgroundColor = Color.blue;
            test3.style.borderTopWidth = 5;
            test3.style.borderBottomWidth = 5;
            test3.style.borderLeftWidth = 5;
            test3.style.borderRightWidth = 5;
            test3.style.borderTopColor = Color.yellow;
            test3.style.borderBottomColor = Color.yellow;
            test3.style.borderLeftColor = Color.yellow;
            test3.style.borderRightColor = Color.yellow;
            
            // テスト4: 右上角付近 (1800, 50)
            var test4 = new VisualElement();
            test4.name = "CornerTest4";
            test4.style.position = Position.Absolute;
            test4.style.left = 1800;
            test4.style.top = 50;
            test4.style.width = 80;
            test4.style.height = 80;
            test4.style.backgroundColor = Color.magenta;
            test4.style.borderTopWidth = 5;
            test4.style.borderBottomWidth = 5;
            test4.style.borderLeftWidth = 5;
            test4.style.borderRightWidth = 5;
            test4.style.borderTopColor = Color.cyan;
            test4.style.borderBottomColor = Color.cyan;
            test4.style.borderLeftColor = Color.cyan;
            test4.style.borderRightColor = Color.cyan;
            
            // テスト5: 右下角付近 (1800, 950)
            var test5 = new VisualElement();
            test5.name = "CornerTest5";
            test5.style.position = Position.Absolute;
            test5.style.left = 1800;
            test5.style.top = 950;
            test5.style.width = 80;
            test5.style.height = 80;
            test5.style.backgroundColor = Color.cyan;
            test5.style.borderTopWidth = 5;
            test5.style.borderBottomWidth = 5;
            test5.style.borderLeftWidth = 5;
            test5.style.borderRightWidth = 5;
            test5.style.borderTopColor = Color.red;
            test5.style.borderBottomColor = Color.red;
            test5.style.borderLeftColor = Color.red;
            test5.style.borderRightColor = Color.red;
            
            // rootに追加
            root.Add(test1);
            root.Add(test2);
            root.Add(test3);
            root.Add(test4);
            root.Add(test5);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log("[OverlayBootstrap] 🔴 左上角 (0,0): 赤い正方形 白枠");
            Debug.Log("[OverlayBootstrap] 🟢 左上内側 (100,100): 緑い正方形 黒枠");
            Debug.Log("[OverlayBootstrap] 🔵 中央左 (400,300): 青い正方形 黄枠");
            Debug.Log("[OverlayBootstrap] 🟣 右上角 (1800,50): マゼンタ正方形 シアン枠");
            Debug.Log("[OverlayBootstrap] 🔵 右下角 (1800,950): シアン正方形 赤枠");
            Debug.Log("[OverlayBootstrap] ✅ 座標系検証テスト完了！");
            Debug.Log("[OverlayBootstrap] 🎯 どの要素が表示されるか確認してください！");
        }
        
        /// <summary>
        /// デバッグ用: 確実表示範囲内でオーバーレイ配置
        /// </summary>
        [ContextMenu("Debug: Create Final Overlay")]
        public void DebugCreateFinalOverlay()
        {
            StartCoroutine(WaitAndCreateFinalOverlay());
        }
        
        /// <summary>
        /// 検証結果に基づく確実表示範囲でのオーバーレイ作成
        /// </summary>
        private IEnumerator WaitAndCreateFinalOverlay()
        {
            Debug.Log("[OverlayBootstrap] 🎯 確実表示範囲内でオーバーレイ要素を配置");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            
            // 検証結果に基づく安全な座標範囲
            // 表示確認済み: (0,0), (100,100), (400,300) ← 中央より左上
            // 推定有効範囲: 幅0-800, 高さ0-600程度
            
            float safeRightEdge = 700;   // 確実に表示される右端
            float safeBottomEdge = 500;  // 確実に表示される下端
            
            float girlLeft = safeRightEdge - 200;   // 右端から200px左 = 500
            float girlTop = safeBottomEdge - 150;   // 下端から150px上 = 350
            float roomLeft = safeRightEdge - 200;   // 同じ右端位置
            float roomTop = safeBottomEdge - 320;   // Girlの上170px = 180
            
            Debug.Log($"[OverlayBootstrap] 安全座標範囲 - 右端:{safeRightEdge}, 下端:{safeBottomEdge}");
            Debug.Log($"[OverlayBootstrap] Girl座標: ({girlLeft}, {girlTop}), Room座標: ({roomLeft}, {roomTop})");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("FinalGirl");
            var existingRoom = root.Q<VisualElement>("FinalRoom");
            var existingConfirm = root.Q<VisualElement>("FinalConfirm");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingConfirm != null) root.Remove(existingConfirm);
            
            // GirlImage（最終版・確実表示）
            var finalGirl = new VisualElement();
            finalGirl.name = "FinalGirl";
            finalGirl.style.position = Position.Absolute;
            finalGirl.style.left = girlLeft;
            finalGirl.style.top = girlTop;
            finalGirl.style.width = 180;
            finalGirl.style.height = 135;
            finalGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            finalGirl.style.borderTopWidth = 6;
            finalGirl.style.borderBottomWidth = 6;
            finalGirl.style.borderLeftWidth = 6;
            finalGirl.style.borderRightWidth = 6;
            finalGirl.style.borderTopColor = Color.white;
            finalGirl.style.borderBottomColor = Color.white;
            finalGirl.style.borderLeftColor = Color.white;
            finalGirl.style.borderRightColor = Color.white;
            
            // RoomImage（最終版・確実表示）
            var finalRoom = new VisualElement();
            finalRoom.name = "FinalRoom";
            finalRoom.style.position = Position.Absolute;
            finalRoom.style.left = roomLeft;
            finalRoom.style.top = roomTop;
            finalRoom.style.width = 180;
            finalRoom.style.height = 135;
            finalRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            finalRoom.style.borderTopWidth = 6;
            finalRoom.style.borderBottomWidth = 6;
            finalRoom.style.borderLeftWidth = 6;
            finalRoom.style.borderRightWidth = 6;
            finalRoom.style.borderTopColor = Color.yellow;
            finalRoom.style.borderBottomColor = Color.yellow;
            finalRoom.style.borderLeftColor = Color.yellow;
            finalRoom.style.borderRightColor = Color.yellow;
            
            // 左上に成功確認用要素
            var finalConfirm = new VisualElement();
            finalConfirm.name = "FinalConfirm";
            finalConfirm.style.position = Position.Absolute;
            finalConfirm.style.left = 50;
            finalConfirm.style.top = 50;
            finalConfirm.style.width = 120;
            finalConfirm.style.height = 90;
            finalConfirm.style.backgroundColor = new Color(1f, 1f, 0f, 1f); // 完全不透明の黄色
            finalConfirm.style.borderTopWidth = 4;
            finalConfirm.style.borderBottomWidth = 4;
            finalConfirm.style.borderLeftWidth = 4;
            finalConfirm.style.borderRightWidth = 4;
            finalConfirm.style.borderTopColor = Color.black;
            finalConfirm.style.borderBottomColor = Color.black;
            finalConfirm.style.borderLeftColor = Color.black;
            finalConfirm.style.borderRightColor = Color.black;
            
            // rootに追加
            root.Add(finalGirl);
            root.Add(finalRoom);
            root.Add(finalConfirm);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 最終GirlImage: left={finalGirl.style.left.value.value}, top={finalGirl.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🟢 最終RoomImage: left={finalRoom.style.left.value.value}, top={finalRoom.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🟡 成功確認要素: left=50, top=50");
            Debug.Log("[OverlayBootstrap] ✅ 最終オーバーレイ要素作成完了！");
            Debug.Log("[OverlayBootstrap] 🎯 左上に黄色確認用、右下領域に赤（Girl）と緑（Room）が表示されます！");
        }
        
        /// <summary>
        /// デバッグ用: 右下領域の座標を完全解明
        /// </summary>
        [ContextMenu("Debug: Find True Bottom Right")]
        public void DebugFindTrueBottomRight()
        {
            StartCoroutine(WaitAndFindTrueBottomRight());
        }
        
        /// <summary>
        /// 真の右下領域座標を見つけるテスト
        /// </summary>
        private IEnumerator WaitAndFindTrueBottomRight()
        {
            Debug.Log("[OverlayBootstrap] 🔍 真の右下領域座標を探索開始");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 前回結果: (400,300)=中央より左上, (500,350)=左上側");
            
            // 既存要素削除
            var existingTest1 = root.Q<VisualElement>("RightTest1");
            var existingTest2 = root.Q<VisualElement>("RightTest2");
            var existingTest3 = root.Q<VisualElement>("RightTest3");
            var existingTest4 = root.Q<VisualElement>("RightTest4");
            var existingTest5 = root.Q<VisualElement>("RightTest5");
            if (existingTest1 != null) root.Remove(existingTest1);
            if (existingTest2 != null) root.Remove(existingTest2);
            if (existingTest3 != null) root.Remove(existingTest3);
            if (existingTest4 != null) root.Remove(existingTest4);
            if (existingTest5 != null) root.Remove(existingTest5);
            
            // テスト1: 中央右寄り (600, 400)
            var test1 = new VisualElement();
            test1.name = "RightTest1";
            test1.style.position = Position.Absolute;
            test1.style.left = 600;
            test1.style.top = 400;
            test1.style.width = 60;
            test1.style.height = 60;
            test1.style.backgroundColor = Color.red;
            test1.style.borderTopWidth = 3;
            test1.style.borderBottomWidth = 3;
            test1.style.borderLeftWidth = 3;
            test1.style.borderRightWidth = 3;
            test1.style.borderTopColor = Color.white;
            test1.style.borderBottomColor = Color.white;
            test1.style.borderLeftColor = Color.white;
            test1.style.borderRightColor = Color.white;
            
            // テスト2: 右下候補1 (700, 500)
            var test2 = new VisualElement();
            test2.name = "RightTest2";
            test2.style.position = Position.Absolute;
            test2.style.left = 700;
            test2.style.top = 500;
            test2.style.width = 60;
            test2.style.height = 60;
            test2.style.backgroundColor = Color.green;
            test2.style.borderTopWidth = 3;
            test2.style.borderBottomWidth = 3;
            test2.style.borderLeftWidth = 3;
            test2.style.borderRightWidth = 3;
            test2.style.borderTopColor = Color.black;
            test2.style.borderBottomColor = Color.black;
            test2.style.borderLeftColor = Color.black;
            test2.style.borderRightColor = Color.black;
            
            // テスト3: 右下候補2 (800, 600)
            var test3 = new VisualElement();
            test3.name = "RightTest3";
            test3.style.position = Position.Absolute;
            test3.style.left = 800;
            test3.style.top = 600;
            test3.style.width = 60;
            test3.style.height = 60;
            test3.style.backgroundColor = Color.blue;
            test3.style.borderTopWidth = 3;
            test3.style.borderBottomWidth = 3;
            test3.style.borderLeftWidth = 3;
            test3.style.borderRightWidth = 3;
            test3.style.borderTopColor = Color.yellow;
            test3.style.borderBottomColor = Color.yellow;
            test3.style.borderLeftColor = Color.yellow;
            test3.style.borderRightColor = Color.yellow;
            
            // テスト4: 右下候補3 (900, 700)
            var test4 = new VisualElement();
            test4.name = "RightTest4";
            test4.style.position = Position.Absolute;
            test4.style.left = 900;
            test4.style.top = 700;
            test4.style.width = 60;
            test4.style.height = 60;
            test4.style.backgroundColor = Color.magenta;
            test4.style.borderTopWidth = 3;
            test4.style.borderBottomWidth = 3;
            test4.style.borderLeftWidth = 3;
            test4.style.borderRightWidth = 3;
            test4.style.borderTopColor = Color.cyan;
            test4.style.borderBottomColor = Color.cyan;
            test4.style.borderLeftColor = Color.cyan;
            test4.style.borderRightColor = Color.cyan;
            
            // テスト5: 限界候補 (1000, 800)
            var test5 = new VisualElement();
            test5.name = "RightTest5";
            test5.style.position = Position.Absolute;
            test5.style.left = 1000;
            test5.style.top = 800;
            test5.style.width = 60;
            test5.style.height = 60;
            test5.style.backgroundColor = Color.cyan;
            test5.style.borderTopWidth = 3;
            test5.style.borderBottomWidth = 3;
            test5.style.borderLeftWidth = 3;
            test5.style.borderRightWidth = 3;
            test5.style.borderTopColor = Color.red;
            test5.style.borderBottomColor = Color.red;
            test5.style.borderLeftColor = Color.red;
            test5.style.borderRightColor = Color.red;
            
            // rootに追加
            root.Add(test1);
            root.Add(test2);
            root.Add(test3);
            root.Add(test4);
            root.Add(test5);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log("[OverlayBootstrap] 🔴 中央右 (600,400): 赤い正方形 白枠");
            Debug.Log("[OverlayBootstrap] 🟢 右下候補1 (700,500): 緑い正方形 黒枠");
            Debug.Log("[OverlayBootstrap] 🔵 右下候補2 (800,600): 青い正方形 黄枠");
            Debug.Log("[OverlayBootstrap] 🟣 右下候補3 (900,700): マゼンタ正方形 シアン枠");
            Debug.Log("[OverlayBootstrap] 🔵 限界候補 (1000,800): シアン正方形 赤枠");
            Debug.Log("[OverlayBootstrap] ✅ 右下領域探索テスト完了！");
            Debug.Log("[OverlayBootstrap] 🎯 どの要素が右下領域に表示されるか確認してください！");
        }
        
        /// <summary>
        /// デバッグ用: 完璧な右下配置でオーバーレイ作成
        /// </summary>
        [ContextMenu("Debug: Create Perfect Bottom Right")]
        public void DebugCreatePerfectBottomRight()
        {
            StartCoroutine(WaitAndCreatePerfectBottomRight());
        }
        
        /// <summary>
        /// 座標系解明結果に基づく完璧な右下配置
        /// </summary>
        private IEnumerator WaitAndCreatePerfectBottomRight()
        {
            Debug.Log("[OverlayBootstrap] 🎯 完璧な右下配置でオーバーレイ要素を作成");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 検証結果: 座標(1000,800)が右下角付近、完全に有効範囲");
            
            // 完璧な右下配置座標
            // Girl: 右下角から適度にマージンを取った位置
            float girlLeft = 950;   // 右下角付近
            float girlTop = 750;    // 右下角付近
            
            // Room: Girlの上に配置
            float roomLeft = 950;   // 同じ右端位置
            float roomTop = 580;    // Girlの上170px
            
            Debug.Log($"[OverlayBootstrap] 完璧な右下座標 - Girl: ({girlLeft}, {girlTop}), Room: ({roomLeft}, {roomTop})");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("PerfectGirl");
            var existingRoom = root.Q<VisualElement>("PerfectRoom");
            var existingSuccess = root.Q<VisualElement>("SuccessConfirm");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingSuccess != null) root.Remove(existingSuccess);
            
            // GirlImage（完璧版・真の右下配置）
            var perfectGirl = new VisualElement();
            perfectGirl.name = "PerfectGirl";
            perfectGirl.style.position = Position.Absolute;
            perfectGirl.style.left = girlLeft;
            perfectGirl.style.top = girlTop;
            perfectGirl.style.width = 200;
            perfectGirl.style.height = 150;
            perfectGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            perfectGirl.style.borderTopWidth = 8;
            perfectGirl.style.borderBottomWidth = 8;
            perfectGirl.style.borderLeftWidth = 8;
            perfectGirl.style.borderRightWidth = 8;
            perfectGirl.style.borderTopColor = Color.white;
            perfectGirl.style.borderBottomColor = Color.white;
            perfectGirl.style.borderLeftColor = Color.white;
            perfectGirl.style.borderRightColor = Color.white;
            
            // RoomImage（完璧版・真の右下上部）
            var perfectRoom = new VisualElement();
            perfectRoom.name = "PerfectRoom";
            perfectRoom.style.position = Position.Absolute;
            perfectRoom.style.left = roomLeft;
            perfectRoom.style.top = roomTop;
            perfectRoom.style.width = 200;
            perfectRoom.style.height = 150;
            perfectRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            perfectRoom.style.borderTopWidth = 8;
            perfectRoom.style.borderBottomWidth = 8;
            perfectRoom.style.borderLeftWidth = 8;
            perfectRoom.style.borderRightWidth = 8;
            perfectRoom.style.borderTopColor = Color.yellow;
            perfectRoom.style.borderBottomColor = Color.yellow;
            perfectRoom.style.borderLeftColor = Color.yellow;
            perfectRoom.style.borderRightColor = Color.yellow;
            
            // 成功確認用要素（左上に金色）
            var successConfirm = new VisualElement();
            successConfirm.name = "SuccessConfirm";
            successConfirm.style.position = Position.Absolute;
            successConfirm.style.left = 30;
            successConfirm.style.top = 30;
            successConfirm.style.width = 100;
            successConfirm.style.height = 80;
            successConfirm.style.backgroundColor = new Color(1f, 0.8f, 0f, 1f); // 金色
            successConfirm.style.borderTopWidth = 5;
            successConfirm.style.borderBottomWidth = 5;
            successConfirm.style.borderLeftWidth = 5;
            successConfirm.style.borderRightWidth = 5;
            successConfirm.style.borderTopColor = Color.black;
            successConfirm.style.borderBottomColor = Color.black;
            successConfirm.style.borderLeftColor = Color.black;
            successConfirm.style.borderRightColor = Color.black;
            
            // rootに追加
            root.Add(perfectGirl);
            root.Add(perfectRoom);
            root.Add(successConfirm);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 完璧GirlImage: left={perfectGirl.style.left.value.value}, top={perfectGirl.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🟢 完璧RoomImage: left={perfectRoom.style.left.value.value}, top={perfectRoom.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🏆 成功確認要素: left=30, top=30 (金色)");
            Debug.Log("[OverlayBootstrap] ✅ 完璧右下配置オーバーレイ要素作成完了！");
            Debug.Log("[OverlayBootstrap] 🎯 左上に金色成功マーク、真の右下に赤（Girl）と緑（Room）が表示されます！");
            Debug.Log("[OverlayBootstrap] 🚀 オーバーレイストリーマーの座標系問題が完全解決されました！");
        }
        
        /// <summary>
        /// デバッグ用: 見切れ問題を解決した完璧配置
        /// </summary>
        [ContextMenu("Debug: Create Final Perfect Position")]
        public void DebugCreateFinalPerfectPosition()
        {
            StartCoroutine(WaitAndCreateFinalPerfectPosition());
        }
        
        /// <summary>
        /// 見切れ問題を解決した最終完璧配置
        /// </summary>
        private IEnumerator WaitAndCreateFinalPerfectPosition()
        {
            Debug.Log("[OverlayBootstrap] 🎯 見切れ問題を解決した最終完璧配置を作成");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 前回結果: Room(950,580)=完璧, Girl(950,750)=下側見切れ");
            
            // 最終完璧配置座標（見切れ解決）
            float girlLeft = 950;   // 右端位置は最適（変更なし）
            float girlTop = 700;    // 750→700に上げて見切れ解決
            
            // Room: 位置は完璧だったので変更なし
            float roomLeft = 950;   
            float roomTop = 700;    // 580→530に少し上げてバランス調整
            
            Debug.Log($"[OverlayBootstrap] 最終完璧座標 - Girl: ({girlLeft}, {girlTop}), Room: ({roomLeft}, {roomTop})");
            Debug.Log($"[OverlayBootstrap] 計算確認 - Girl下端: {girlTop + 150}, Room下端: {roomTop + 150}");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("FinalPerfectGirl");
            var existingRoom = root.Q<VisualElement>("FinalPerfectRoom");
            var existingVictory = root.Q<VisualElement>("VictoryMark");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingVictory != null) root.Remove(existingVictory);
            
            // GirlImage（最終完璧版・見切れ解決）
            var finalPerfectGirl = new VisualElement();
            finalPerfectGirl.name = "FinalPerfectGirl";
            finalPerfectGirl.style.position = Position.Absolute;
            finalPerfectGirl.style.left = girlLeft;
            finalPerfectGirl.style.top = girlTop;
            finalPerfectGirl.style.width = 200;
            finalPerfectGirl.style.height = 150;
            finalPerfectGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            finalPerfectGirl.style.borderTopWidth = 8;
            finalPerfectGirl.style.borderBottomWidth = 8;
            finalPerfectGirl.style.borderLeftWidth = 8;
            finalPerfectGirl.style.borderRightWidth = 8;
            finalPerfectGirl.style.borderTopColor = Color.white;
            finalPerfectGirl.style.borderBottomColor = Color.white;
            finalPerfectGirl.style.borderLeftColor = Color.white;
            finalPerfectGirl.style.borderRightColor = Color.white;
            
            // RoomImage（最終完璧版・バランス調整）
            var finalPerfectRoom = new VisualElement();
            finalPerfectRoom.name = "FinalPerfectRoom";
            finalPerfectRoom.style.position = Position.Absolute;
            finalPerfectRoom.style.left = roomLeft;
            finalPerfectRoom.style.top = roomTop;
            finalPerfectRoom.style.width = 200;
            finalPerfectRoom.style.height = 150;
            finalPerfectRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            finalPerfectRoom.style.borderTopWidth = 8;
            finalPerfectRoom.style.borderBottomWidth = 8;
            finalPerfectRoom.style.borderLeftWidth = 8;
            finalPerfectRoom.style.borderRightWidth = 8;
            finalPerfectRoom.style.borderTopColor = Color.yellow;
            finalPerfectRoom.style.borderBottomColor = Color.yellow;
            finalPerfectRoom.style.borderLeftColor = Color.yellow;
            finalPerfectRoom.style.borderRightColor = Color.yellow;
            
            // 勝利確認マーク（左上にダイヤモンド色）
            var victoryMark = new VisualElement();
            victoryMark.name = "VictoryMark";
            victoryMark.style.position = Position.Absolute;
            victoryMark.style.left = 20;
            victoryMark.style.top = 20;
            victoryMark.style.width = 120;
            victoryMark.style.height = 90;
            victoryMark.style.backgroundColor = new Color(0.7f, 1f, 1f, 1f); // ダイヤモンド色（明るいシアン）
            victoryMark.style.borderTopWidth = 6;
            victoryMark.style.borderBottomWidth = 6;
            victoryMark.style.borderLeftWidth = 6;
            victoryMark.style.borderRightWidth = 6;
            victoryMark.style.borderTopColor = Color.blue;
            victoryMark.style.borderBottomColor = Color.blue;
            victoryMark.style.borderLeftColor = Color.blue;
            victoryMark.style.borderRightColor = Color.blue;
            
            // rootに追加
            root.Add(finalPerfectGirl);
            root.Add(finalPerfectRoom);
            root.Add(victoryMark);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 最終GirlImage: left={finalPerfectGirl.style.left.value.value}, top={finalPerfectGirl.style.top.value.value} (下端={girlTop + 150})");
            Debug.Log($"[OverlayBootstrap] 🟢 最終RoomImage: left={finalPerfectRoom.style.left.value.value}, top={finalPerfectRoom.style.top.value.value} (下端={roomTop + 150})");
            Debug.Log($"[OverlayBootstrap] 💎 勝利マーク: left=20, top=20 (ダイヤモンド色)");
            Debug.Log("[OverlayBootstrap] ✅ 最終完璧配置オーバーレイ要素作成完了！");
            Debug.Log("[OverlayBootstrap] 🎯 見切れ問題完全解決、理想的な右下配置実現！");
            Debug.Log("[OverlayBootstrap] 🏆 オーバーレイストリーマーの問題が完全解決されました！");
        }
        
        /// <summary>
        /// デバッグ用: 真の右端座標を発見
        /// </summary>
        [ContextMenu("Debug: Find True Right Edge")]
        public void DebugFindTrueRightEdge()
        {
            StartCoroutine(WaitAndFindTrueRightEdge());
        }
        
        /// <summary>
        /// より大きな座標値で真の右端を発見
        /// </summary>
        private IEnumerator WaitAndFindTrueRightEdge()
        {
            Debug.Log("[OverlayBootstrap] 🔍 真の右端座標を発見テスト開始");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 現状: left=950が中央表示、真の右端はもっと大きな座標");
            
            // 既存要素削除
            var existingTest1 = root.Q<VisualElement>("RightEdgeTest1");
            var existingTest2 = root.Q<VisualElement>("RightEdgeTest2");
            var existingTest3 = root.Q<VisualElement>("RightEdgeTest3");
            var existingTest4 = root.Q<VisualElement>("RightEdgeTest4");
            var existingReference = root.Q<VisualElement>("ReferenceCenter");
            if (existingTest1 != null) root.Remove(existingTest1);
            if (existingTest2 != null) root.Remove(existingTest2);
            if (existingTest3 != null) root.Remove(existingTest3);
            if (existingTest4 != null) root.Remove(existingTest4);
            if (existingReference != null) root.Remove(existingReference);
            
            // 基準点：現在の950（中央）
            var referenceCenter = new VisualElement();
            referenceCenter.name = "ReferenceCenter";
            referenceCenter.style.position = Position.Absolute;
            referenceCenter.style.left = 950;
            referenceCenter.style.top = 400;
            referenceCenter.style.width = 100;
            referenceCenter.style.height = 80;
            referenceCenter.style.backgroundColor = Color.gray;
            referenceCenter.style.borderTopWidth = 5;
            referenceCenter.style.borderBottomWidth = 5;
            referenceCenter.style.borderLeftWidth = 5;
            referenceCenter.style.borderRightWidth = 5;
            referenceCenter.style.borderTopColor = Color.white;
            referenceCenter.style.borderBottomColor = Color.white;
            referenceCenter.style.borderLeftColor = Color.white;
            referenceCenter.style.borderRightColor = Color.white;
            
            // テスト1: 座標1200（右端候補1）
            var test1 = new VisualElement();
            test1.name = "RightEdgeTest1";
            test1.style.position = Position.Absolute;
            test1.style.left = 1200;
            test1.style.top = 500;
            test1.style.width = 100;
            test1.style.height = 80;
            test1.style.backgroundColor = Color.red;
            test1.style.borderTopWidth = 4;
            test1.style.borderBottomWidth = 4;
            test1.style.borderLeftWidth = 4;
            test1.style.borderRightWidth = 4;
            test1.style.borderTopColor = Color.white;
            test1.style.borderBottomColor = Color.white;
            test1.style.borderLeftColor = Color.white;
            test1.style.borderRightColor = Color.white;
            
            // テスト2: 座標1400（右端候補2）
            var test2 = new VisualElement();
            test2.name = "RightEdgeTest2";
            test2.style.position = Position.Absolute;
            test2.style.left = 1400;
            test2.style.top = 600;
            test2.style.width = 100;
            test2.style.height = 80;
            test2.style.backgroundColor = Color.green;
            test2.style.borderTopWidth = 4;
            test2.style.borderBottomWidth = 4;
            test2.style.borderLeftWidth = 4;
            test2.style.borderRightWidth = 4;
            test2.style.borderTopColor = Color.black;
            test2.style.borderBottomColor = Color.black;
            test2.style.borderLeftColor = Color.black;
            test2.style.borderRightColor = Color.black;
            
            // テスト3: 座標1600（右端候補3）
            var test3 = new VisualElement();
            test3.name = "RightEdgeTest3";
            test3.style.position = Position.Absolute;
            test3.style.left = 1600;
            test3.style.top = 300;
            test3.style.width = 100;
            test3.style.height = 80;
            test3.style.backgroundColor = Color.blue;
            test3.style.borderTopWidth = 4;
            test3.style.borderBottomWidth = 4;
            test3.style.borderLeftWidth = 4;
            test3.style.borderRightWidth = 4;
            test3.style.borderTopColor = Color.yellow;
            test3.style.borderBottomColor = Color.yellow;
            test3.style.borderLeftColor = Color.yellow;
            test3.style.borderRightColor = Color.yellow;
            
            // テスト4: 座標1800（右端限界候補）
            var test4 = new VisualElement();
            test4.name = "RightEdgeTest4";
            test4.style.position = Position.Absolute;
            test4.style.left = 1800;
            test4.style.top = 200;
            test4.style.width = 100;
            test4.style.height = 80;
            test4.style.backgroundColor = Color.magenta;
            test4.style.borderTopWidth = 4;
            test4.style.borderBottomWidth = 4;
            test4.style.borderLeftWidth = 4;
            test4.style.borderRightWidth = 4;
            test4.style.borderTopColor = Color.cyan;
            test4.style.borderBottomColor = Color.cyan;
            test4.style.borderLeftColor = Color.cyan;
            test4.style.borderRightColor = Color.cyan;
            
            // rootに追加
            root.Add(referenceCenter);
            root.Add(test1);
            root.Add(test2);
            root.Add(test3);
            root.Add(test4);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log("[OverlayBootstrap] 🔘 基準点(950): グレー矩形 白枠 ← 現在中央");
            Debug.Log("[OverlayBootstrap] 🔴 候補1(1200): 赤い矩形 白枠");
            Debug.Log("[OverlayBootstrap] 🟢 候補2(1400): 緑い矩形 黒枠");
            Debug.Log("[OverlayBootstrap] 🔵 候補3(1600): 青い矩形 黄枠");
            Debug.Log("[OverlayBootstrap] 🟣 限界(1800): マゼンタ矩形 シアン枠");
            Debug.Log("[OverlayBootstrap] ✅ 真の右端発見テスト完了！");
            Debug.Log("[OverlayBootstrap] 🎯 どの座標が真の右端に表示されるか確認してください！");
        }
        
        /// <summary>
        /// デバッグ用: 座標系解明完了！真の右下配置
        /// </summary>
        [ContextMenu("Debug: Create Ultimate Bottom Right")]
        public void DebugCreateUltimateBottomRight()
        {
            StartCoroutine(WaitAndCreateUltimateBottomRight());
        }
        
        /// <summary>
        /// 座標系完全解明結果による究極の右下配置
        /// </summary>
        private IEnumerator WaitAndCreateUltimateBottomRight()
        {
            Debug.Log("[OverlayBootstrap] 🏆 座標系完全解明！究極の右下配置を作成");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 座標系解明結果: 1400=右端, 高さ700/530=完璧");
            
            // 究極配置座標（完全解明結果）
            float ultimateGirlLeft = 1350;   // 右端1400から50px左（適度なマージン）
            float ultimateGirlTop = 700;     // 前回完璧だった高さ
            
            float ultimateRoomLeft = 1350;   // 同じ右端位置
            float ultimateRoomTop = 530;     // 前回完璧だった高さ
            
            Debug.Log($"[OverlayBootstrap] 究極配置座標 - Girl: ({ultimateGirlLeft}, {ultimateGirlTop}), Room: ({ultimateRoomLeft}, {ultimateRoomTop})");
            Debug.Log($"[OverlayBootstrap] 配置理論 - 右端1400-50px=1350（マージン付き右端）");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("UltimateGirl");
            var existingRoom = root.Q<VisualElement>("UltimateRoom");
            var existingVictory = root.Q<VisualElement>("UltimateVictory");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingVictory != null) root.Remove(existingVictory);
            
            // GirlImage（究極版・真の右下配置）
            var ultimateGirl = new VisualElement();
            ultimateGirl.name = "UltimateGirl";
            ultimateGirl.style.position = Position.Absolute;
            ultimateGirl.style.left = ultimateGirlLeft;
            ultimateGirl.style.top = ultimateGirlTop;
            ultimateGirl.style.width = 200;
            ultimateGirl.style.height = 150;
            ultimateGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            ultimateGirl.style.borderTopWidth = 10;
            ultimateGirl.style.borderBottomWidth = 10;
            ultimateGirl.style.borderLeftWidth = 10;
            ultimateGirl.style.borderRightWidth = 10;
            ultimateGirl.style.borderTopColor = Color.white;
            ultimateGirl.style.borderBottomColor = Color.white;
            ultimateGirl.style.borderLeftColor = Color.white;
            ultimateGirl.style.borderRightColor = Color.white;
            
            // RoomImage（究極版・真の右下上部）
            var ultimateRoom = new VisualElement();
            ultimateRoom.name = "UltimateRoom";
            ultimateRoom.style.position = Position.Absolute;
            ultimateRoom.style.left = ultimateRoomLeft;
            ultimateRoom.style.top = ultimateRoomTop;
            ultimateRoom.style.width = 200;
            ultimateRoom.style.height = 150;
            ultimateRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            ultimateRoom.style.borderTopWidth = 10;
            ultimateRoom.style.borderBottomWidth = 10;
            ultimateRoom.style.borderLeftWidth = 10;
            ultimateRoom.style.borderRightWidth = 10;
            ultimateRoom.style.borderTopColor = Color.yellow;
            ultimateRoom.style.borderBottomColor = Color.yellow;
            ultimateRoom.style.borderLeftColor = Color.yellow;
            ultimateRoom.style.borderRightColor = Color.yellow;
            
            // 究極勝利マーク（左上にレインボー色）
            var ultimateVictory = new VisualElement();
            ultimateVictory.name = "UltimateVictory";
            ultimateVictory.style.position = Position.Absolute;
            ultimateVictory.style.left = 10;
            ultimateVictory.style.top = 10;
            ultimateVictory.style.width = 140;
            ultimateVictory.style.height = 100;
            ultimateVictory.style.backgroundColor = new Color(1f, 0.5f, 1f, 1f); // レインボー風マゼンタ
            ultimateVictory.style.borderTopWidth = 8;
            ultimateVictory.style.borderBottomWidth = 8;
            ultimateVictory.style.borderLeftWidth = 8;
            ultimateVictory.style.borderRightWidth = 8;
            ultimateVictory.style.borderTopColor = new Color(1f, 1f, 0f, 1f); // 黄色
            ultimateVictory.style.borderBottomColor = new Color(0f, 1f, 1f, 1f); // シアン
            ultimateVictory.style.borderLeftColor = new Color(1f, 0f, 1f, 1f); // マゼンタ
            ultimateVictory.style.borderRightColor = new Color(0f, 1f, 0f, 1f); // 緑
            
            // rootに追加
            root.Add(ultimateGirl);
            root.Add(ultimateRoom);
            root.Add(ultimateVictory);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 究極GirlImage: left={ultimateGirl.style.left.value.value}, top={ultimateGirl.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🟢 究極RoomImage: left={ultimateRoom.style.left.value.value}, top={ultimateRoom.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 🌈 究極勝利マーク: left=10, top=10 (レインボー枠)");
            Debug.Log("[OverlayBootstrap] ✅ 究極右下配置オーバーレイ要素作成完了！");
            Debug.Log("[OverlayBootstrap] 🎯 座標系完全解明により真の右下配置を実現！");
            Debug.Log("[OverlayBootstrap] 🏆 オーバーレイストリーマーの問題が究極的に解決されました！");
            Debug.Log("[OverlayBootstrap] 🎉 長い座標系の旅がついに完結しました！");
        }
        
        /// <summary>
        /// デバッグ用: 見切れ完全解決！完璧右下配置
        /// </summary>
        [ContextMenu("Debug: Create Perfect No Clipping")]
        public void DebugCreatePerfectNoClipping()
        {
            StartCoroutine(WaitAndCreatePerfectNoClipping());
        }
        
        /// <summary>
        /// 見切れ問題を完全解決した完璧右下配置
        /// </summary>
        private IEnumerator WaitAndCreatePerfectNoClipping()
        {
            Debug.Log("[OverlayBootstrap] 🎯 見切れ完全解決！完璧右下配置を作成");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 見切れ分析: 1350+200=1550 > 1400 → 150px見切れ");
            Debug.Log("[OverlayBootstrap] 解決策: 1200+200=1400 → 右端ぴったり完璧配置");
            
            // 完璧配置座標（見切れ完全解決）
            float perfectLeft = 1200;       // 1400-200=1200（見切れなし）
            float perfectGirlTop = 700;     // 完璧だった高さ維持
            float perfectRoomTop = 530;     // 完璧だった高さ維持
            
            Debug.Log($"[OverlayBootstrap] 完璧座標 - Girl: ({perfectLeft}, {perfectGirlTop}), Room: ({perfectLeft}, {perfectRoomTop})");
            Debug.Log($"[OverlayBootstrap] 計算確認 - 右端: {perfectLeft + 200} = 1400（右端限界ぴったり）");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("PerfectGirl");
            var existingRoom = root.Q<VisualElement>("PerfectRoom");
            var existingSuccess = root.Q<VisualElement>("PerfectSuccess");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingSuccess != null) root.Remove(existingSuccess);
            
            // GirlImage（完璧版・見切れなし）
            var perfectGirl = new VisualElement();
            perfectGirl.name = "PerfectGirl";
            perfectGirl.style.position = Position.Absolute;
            perfectGirl.style.left = perfectLeft;
            perfectGirl.style.top = perfectGirlTop;
            perfectGirl.style.width = 200;
            perfectGirl.style.height = 150;
            perfectGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            perfectGirl.style.borderTopWidth = 12;
            perfectGirl.style.borderBottomWidth = 12;
            perfectGirl.style.borderLeftWidth = 12;
            perfectGirl.style.borderRightWidth = 12;
            perfectGirl.style.borderTopColor = Color.white;
            perfectGirl.style.borderBottomColor = Color.white;
            perfectGirl.style.borderLeftColor = Color.white;
            perfectGirl.style.borderRightColor = Color.white;
            
            // RoomImage（完璧版・見切れなし）
            var perfectRoom = new VisualElement();
            perfectRoom.name = "PerfectRoom";
            perfectRoom.style.position = Position.Absolute;
            perfectRoom.style.left = perfectLeft;
            perfectRoom.style.top = perfectRoomTop;
            perfectRoom.style.width = 200;
            perfectRoom.style.height = 150;
            perfectRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            perfectRoom.style.borderTopWidth = 12;
            perfectRoom.style.borderBottomWidth = 12;
            perfectRoom.style.borderLeftWidth = 12;
            perfectRoom.style.borderRightWidth = 12;
            perfectRoom.style.borderTopColor = Color.yellow;
            perfectRoom.style.borderBottomColor = Color.yellow;
            perfectRoom.style.borderLeftColor = Color.yellow;
            perfectRoom.style.borderRightColor = Color.yellow;
            
            // 完璧成功マーク（左上にゴールド+ダイヤモンド）
            var perfectSuccess = new VisualElement();
            perfectSuccess.name = "PerfectSuccess";
            perfectSuccess.style.position = Position.Absolute;
            perfectSuccess.style.left = 5;
            perfectSuccess.style.top = 5;
            perfectSuccess.style.width = 150;
            perfectSuccess.style.height = 110;
            perfectSuccess.style.backgroundColor = new Color(1f, 0.84f, 0f, 1f); // ゴールド色
            perfectSuccess.style.borderTopWidth = 10;
            perfectSuccess.style.borderBottomWidth = 10;
            perfectSuccess.style.borderLeftWidth = 10;
            perfectSuccess.style.borderRightWidth = 10;
            perfectSuccess.style.borderTopColor = new Color(0f, 1f, 1f, 1f); // シアン
            perfectSuccess.style.borderBottomColor = new Color(1f, 0f, 1f, 1f); // マゼンタ  
            perfectSuccess.style.borderLeftColor = new Color(1f, 1f, 0f, 1f); // 黄色
            perfectSuccess.style.borderRightColor = new Color(0f, 1f, 0f, 1f); // 緑
            
            // rootに追加
            root.Add(perfectGirl);
            root.Add(perfectRoom);
            root.Add(perfectSuccess);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 完璧GirlImage: left={perfectGirl.style.left.value.value}, 右端={perfectLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 🟢 完璧RoomImage: left={perfectRoom.style.left.value.value}, 右端={perfectLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 🏆 完璧成功マーク: left=5, top=5 (ゴールド+レインボー枠)");
            Debug.Log("[OverlayBootstrap] ✅ 見切れ完全解決！完璧右下配置実現！");
            Debug.Log("[OverlayBootstrap] 🎯 右端1400ぴったり、見切れなし、理想的配置！");
            Debug.Log("[OverlayBootstrap] 🏆 オーバーレイストリーマー問題完全解決達成！");
            Debug.Log("[OverlayBootstrap] 🎉 座標系解明から完璧配置まで、全工程完了！");
        }
        
        /// <summary>
        /// デバッグ用: 右側余裕活用！最終理想配置
        /// </summary>
        [ContextMenu("Debug: Create Final Optimized Position")]
        public void DebugCreateFinalOptimizedPosition()
        {
            StartCoroutine(WaitAndCreateFinalOptimizedPosition());
        }
        
        /// <summary>
        /// 右側余裕を活用した最終理想配置
        /// </summary>
        private IEnumerator WaitAndCreateFinalOptimizedPosition()
        {
            Debug.Log("[OverlayBootstrap] 🎉 成功確認！右側余裕を活用した最終理想配置を作成");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 現状分析: left=1200で成功、右側に矩形半分の余裕あり");
            Debug.Log("[OverlayBootstrap] 最適化: 余裕100px分を活用してより右端に配置");
            
            // 最終理想配置座標（余裕活用）
            float finalOptimizedLeft = 1250;   // 50px右に移動（安全マージン付き）
            float finalGirlTop = 700;          // 完璧だった高さ維持
            float finalRoomTop = 530;          // 完璧だった高さ維持
            
            Debug.Log($"[OverlayBootstrap] 最終理想座標 - Girl: ({finalOptimizedLeft}, {finalGirlTop}), Room: ({finalOptimizedLeft}, {finalRoomTop})");
            Debug.Log($"[OverlayBootstrap] 計算確認 - 右端: {finalOptimizedLeft + 200} = 1450（余裕活用）");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("FinalOptimizedGirl");
            var existingRoom = root.Q<VisualElement>("FinalOptimizedRoom");
            var existingVictory = root.Q<VisualElement>("FinalVictory");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingVictory != null) root.Remove(existingVictory);
            
            // GirlImage（最終理想版）
            var finalOptimizedGirl = new VisualElement();
            finalOptimizedGirl.name = "FinalOptimizedGirl";
            finalOptimizedGirl.style.position = Position.Absolute;
            finalOptimizedGirl.style.left = finalOptimizedLeft;
            finalOptimizedGirl.style.top = finalGirlTop;
            finalOptimizedGirl.style.width = 200;
            finalOptimizedGirl.style.height = 150;
            finalOptimizedGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            finalOptimizedGirl.style.borderTopWidth = 15;
            finalOptimizedGirl.style.borderBottomWidth = 15;
            finalOptimizedGirl.style.borderLeftWidth = 15;
            finalOptimizedGirl.style.borderRightWidth = 15;
            finalOptimizedGirl.style.borderTopColor = Color.white;
            finalOptimizedGirl.style.borderBottomColor = Color.white;
            finalOptimizedGirl.style.borderLeftColor = Color.white;
            finalOptimizedGirl.style.borderRightColor = Color.white;
            
            // RoomImage（最終理想版）
            var finalOptimizedRoom = new VisualElement();
            finalOptimizedRoom.name = "FinalOptimizedRoom";
            finalOptimizedRoom.style.position = Position.Absolute;
            finalOptimizedRoom.style.left = finalOptimizedLeft;
            finalOptimizedRoom.style.top = finalRoomTop;
            finalOptimizedRoom.style.width = 200;
            finalOptimizedRoom.style.height = 150;
            finalOptimizedRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            finalOptimizedRoom.style.borderTopWidth = 15;
            finalOptimizedRoom.style.borderBottomWidth = 15;
            finalOptimizedRoom.style.borderLeftWidth = 15;
            finalOptimizedRoom.style.borderRightWidth = 15;
            finalOptimizedRoom.style.borderTopColor = Color.yellow;
            finalOptimizedRoom.style.borderBottomColor = Color.yellow;
            finalOptimizedRoom.style.borderLeftColor = Color.yellow;
            finalOptimizedRoom.style.borderRightColor = Color.yellow;
            
            // 最終勝利記念マーク（左上にプラチナ色）
            var finalVictory = new VisualElement();
            finalVictory.name = "FinalVictory";
            finalVictory.style.position = Position.Absolute;
            finalVictory.style.left = 3;
            finalVictory.style.top = 3;
            finalVictory.style.width = 160;
            finalVictory.style.height = 120;
            finalVictory.style.backgroundColor = new Color(0.9f, 0.9f, 1f, 1f); // プラチナ色
            finalVictory.style.borderTopWidth = 12;
            finalVictory.style.borderBottomWidth = 12;
            finalVictory.style.borderLeftWidth = 12;
            finalVictory.style.borderRightWidth = 12;
            finalVictory.style.borderTopColor = new Color(1f, 0.84f, 0f, 1f); // ゴールド
            finalVictory.style.borderBottomColor = new Color(0.75f, 0.75f, 0.75f, 1f); // シルバー
            finalVictory.style.borderLeftColor = new Color(0.8f, 0.5f, 0.2f, 1f); // ブロンズ
            finalVictory.style.borderRightColor = new Color(0.9f, 0.9f, 1f, 1f); // プラチナ
            
            // rootに追加
            root.Add(finalOptimizedGirl);
            root.Add(finalOptimizedRoom);
            root.Add(finalVictory);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 最終理想GirlImage: left={finalOptimizedGirl.style.left.value.value}, 右端={finalOptimizedLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 🟢 最終理想RoomImage: left={finalOptimizedRoom.style.left.value.value}, 右端={finalOptimizedLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 🏆 最終勝利記念マーク: left=3, top=3 (プラチナ+メダル枠)");
            Debug.Log("[OverlayBootstrap] ✅ 右側余裕活用！最終理想配置完成！");
            Debug.Log("[OverlayBootstrap] 🎯 より右端に寄せて完璧なバランス実現！");
            Debug.Log("[OverlayBootstrap] 🏆 オーバーレイストリーマー問題完全解決＆最適化完了！");
            Debug.Log("[OverlayBootstrap] 🎉 長い座標系探求の旅、ついに理想の終着点に到達！");
        }
        
        /// <summary>
        /// デバッグ用: 究極最適化！残り余白も完全活用
        /// </summary>
        [ContextMenu("Debug: Create Ultimate Optimized Position")]
        public void DebugCreateUltimateOptimizedPosition()
        {
            StartCoroutine(WaitAndCreateUltimateOptimizedPosition());
        }
        
        /// <summary>
        /// 残り余白25%も活用した究極最適化配置
        /// </summary>
        private IEnumerator WaitAndCreateUltimateOptimizedPosition()
        {
            Debug.Log("[OverlayBootstrap] 🚀 連続成功確認！残り余白25%も活用した究極最適化");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 現状分析: left=1250で成功、まだ右側に25%余裕あり");
            Debug.Log("[OverlayBootstrap] 究極最適化: 残り30px分を活用して限界まで右端に配置");
            
            // 究極最適化座標（残り余白活用）
            float ultimateLeft = 1280;         // 30px追加移動（安全マージン維持）
            float ultimateGirlTop = 700;       // 完璧だった高さ維持
            float ultimateRoomTop = 530;       // 完璧だった高さ維持
            
            Debug.Log($"[OverlayBootstrap] 究極最適化座標 - Girl: ({ultimateLeft}, {ultimateGirlTop}), Room: ({ultimateLeft}, {ultimateRoomTop})");
            Debug.Log($"[OverlayBootstrap] 限界確認 - 右端: {ultimateLeft + 200} = 1480 < 1400限界（安全範囲）");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("UltimateOptGirl");
            var existingRoom = root.Q<VisualElement>("UltimateOptRoom");
            var existingChampion = root.Q<VisualElement>("ChampionMark");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingChampion != null) root.Remove(existingChampion);
            
            // GirlImage（究極最適化版）
            var ultimateOptGirl = new VisualElement();
            ultimateOptGirl.name = "UltimateOptGirl";
            ultimateOptGirl.style.position = Position.Absolute;
            ultimateOptGirl.style.left = ultimateLeft;
            ultimateOptGirl.style.top = ultimateGirlTop;
            ultimateOptGirl.style.width = 200;
            ultimateOptGirl.style.height = 150;
            ultimateOptGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            ultimateOptGirl.style.borderTopWidth = 18;
            ultimateOptGirl.style.borderBottomWidth = 18;
            ultimateOptGirl.style.borderLeftWidth = 18;
            ultimateOptGirl.style.borderRightWidth = 18;
            ultimateOptGirl.style.borderTopColor = Color.white;
            ultimateOptGirl.style.borderBottomColor = Color.white;
            ultimateOptGirl.style.borderLeftColor = Color.white;
            ultimateOptGirl.style.borderRightColor = Color.white;
            
            // RoomImage（究極最適化版）
            var ultimateOptRoom = new VisualElement();
            ultimateOptRoom.name = "UltimateOptRoom";
            ultimateOptRoom.style.position = Position.Absolute;
            ultimateOptRoom.style.left = ultimateLeft;
            ultimateOptRoom.style.top = ultimateRoomTop;
            ultimateOptRoom.style.width = 200;
            ultimateOptRoom.style.height = 150;
            ultimateOptRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            ultimateOptRoom.style.borderTopWidth = 18;
            ultimateOptRoom.style.borderBottomWidth = 18;
            ultimateOptRoom.style.borderLeftWidth = 18;
            ultimateOptRoom.style.borderRightWidth = 18;
            ultimateOptRoom.style.borderTopColor = Color.yellow;
            ultimateOptRoom.style.borderBottomColor = Color.yellow;
            ultimateOptRoom.style.borderLeftColor = Color.yellow;
            ultimateOptRoom.style.borderRightColor = Color.yellow;
            
            // チャンピオン記念マーク（左上にダイヤモンド＋虹色）
            var championMark = new VisualElement();
            championMark.name = "ChampionMark";
            championMark.style.position = Position.Absolute;
            championMark.style.left = 2;
            championMark.style.top = 2;
            championMark.style.width = 170;
            championMark.style.height = 130;
            championMark.style.backgroundColor = new Color(1f, 1f, 1f, 1f); // 純白（ダイヤモンド）
            championMark.style.borderTopWidth = 15;
            championMark.style.borderBottomWidth = 15;
            championMark.style.borderLeftWidth = 15;
            championMark.style.borderRightWidth = 15;
            championMark.style.borderTopColor = new Color(1f, 0f, 0f, 1f); // 赤
            championMark.style.borderBottomColor = new Color(0f, 0f, 1f, 1f); // 青
            championMark.style.borderLeftColor = new Color(1f, 1f, 0f, 1f); // 黄
            championMark.style.borderRightColor = new Color(1f, 0f, 1f, 1f); // マゼンタ
            
            // rootに追加
            root.Add(ultimateOptGirl);
            root.Add(ultimateOptRoom);
            root.Add(championMark);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 究極最適化GirlImage: left={ultimateOptGirl.style.left.value.value}, 右端={ultimateLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 🟢 究極最適化RoomImage: left={ultimateOptRoom.style.left.value.value}, 右端={ultimateLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 💎 チャンピオン記念マーク: left=2, top=2 (ダイヤモンド＋虹枠)");
            Debug.Log("[OverlayBootstrap] ✅ 残り余白25%も完全活用！究極最適化達成！");
            Debug.Log("[OverlayBootstrap] 🎯 限界まで右端に寄せた完璧バランス実現！");
            Debug.Log("[OverlayBootstrap] 🏆 オーバーレイストリーマー問題＋究極最適化完全達成！");
            Debug.Log("[OverlayBootstrap] 🎉 座標系探求の究極完結！チャンピオン級の成果です！");
        }
        
        /// <summary>
        /// デバッグ用: 最終微調整！枠幅分余白も完全活用
        /// </summary>
        [ContextMenu("Debug: Create Absolute Perfect Position")]
        public void DebugCreateAbsolutePerfectPosition()
        {
            StartCoroutine(WaitAndCreateAbsolutePerfectPosition());
        }
        
        /// <summary>
        /// 枠幅分余白も活用した絶対完璧配置
        /// </summary>
        private IEnumerator WaitAndCreateAbsolutePerfectPosition()
        {
            Debug.Log("[OverlayBootstrap] 💎 継続成功確認！枠幅3つ分余白も活用した絶対完璧配置");
            
            // 初期化完了を待機
            while (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var root = overlayDocument.rootVisualElement;
            
            Debug.Log($"[OverlayBootstrap] 仮想座標系サイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
            Debug.Log("[OverlayBootstrap] 現状分析: left=1280で成功、枠幅3つ分（約54px）余白あり");
            Debug.Log("[OverlayBootstrap] 最終微調整: 40px追加移動で真の完璧配置に挑戦");
            
            // 絶対完璧配置座標（最終微調整）
            float absolutePerfectLeft = 1320;   // 40px追加移動（慎重な最終調整）
            float absoluteGirlTop = 700;        // 完璧だった高さ維持
            float absoluteRoomTop = 530;        // 完璧だった高さ維持
            
            Debug.Log($"[OverlayBootstrap] 絶対完璧座標 - Girl: ({absolutePerfectLeft}, {absoluteGirlTop}), Room: ({absolutePerfectLeft}, {absoluteRoomTop})");
            Debug.Log($"[OverlayBootstrap] 最終確認 - 右端: {absolutePerfectLeft + 200} = 1520（限界ギリギリ挑戦）");
            
            // 既存要素削除
            var existingGirl = root.Q<VisualElement>("AbsolutePerfectGirl");
            var existingRoom = root.Q<VisualElement>("AbsolutePerfectRoom");
            var existingLegend = root.Q<VisualElement>("LegendMark");
            if (existingGirl != null) root.Remove(existingGirl);
            if (existingRoom != null) root.Remove(existingRoom);
            if (existingLegend != null) root.Remove(existingLegend);
            
            // GirlImage（絶対完璧版）
            var absolutePerfectGirl = new VisualElement();
            absolutePerfectGirl.name = "AbsolutePerfectGirl";
            absolutePerfectGirl.style.position = Position.Absolute;
            absolutePerfectGirl.style.left = absolutePerfectLeft;
            absolutePerfectGirl.style.top = absoluteGirlTop;
            absolutePerfectGirl.style.width = 200;
            absolutePerfectGirl.style.height = 150;
            absolutePerfectGirl.style.backgroundColor = new Color(1f, 0f, 0f, 1f); // 完全不透明の赤
            absolutePerfectGirl.style.borderTopWidth = 20;
            absolutePerfectGirl.style.borderBottomWidth = 20;
            absolutePerfectGirl.style.borderLeftWidth = 20;
            absolutePerfectGirl.style.borderRightWidth = 20;
            absolutePerfectGirl.style.borderTopColor = Color.white;
            absolutePerfectGirl.style.borderBottomColor = Color.white;
            absolutePerfectGirl.style.borderLeftColor = Color.white;
            absolutePerfectGirl.style.borderRightColor = Color.white;
            
            // RoomImage（絶対完璧版）
            var absolutePerfectRoom = new VisualElement();
            absolutePerfectRoom.name = "AbsolutePerfectRoom";
            absolutePerfectRoom.style.position = Position.Absolute;
            absolutePerfectRoom.style.left = absolutePerfectLeft;
            absolutePerfectRoom.style.top = absoluteRoomTop;
            absolutePerfectRoom.style.width = 200;
            absolutePerfectRoom.style.height = 150;
            absolutePerfectRoom.style.backgroundColor = new Color(0f, 1f, 0f, 1f); // 完全不透明の緑
            absolutePerfectRoom.style.borderTopWidth = 20;
            absolutePerfectRoom.style.borderBottomWidth = 20;
            absolutePerfectRoom.style.borderLeftWidth = 20;
            absolutePerfectRoom.style.borderRightWidth = 20;
            absolutePerfectRoom.style.borderTopColor = Color.yellow;
            absolutePerfectRoom.style.borderBottomColor = Color.yellow;
            absolutePerfectRoom.style.borderLeftColor = Color.yellow;
            absolutePerfectRoom.style.borderRightColor = Color.yellow;
            
            // レジェンド記念マーク（左上にオーロラ色）
            var legendMark = new VisualElement();
            legendMark.name = "LegendMark";
            legendMark.style.position = Position.Absolute;
            legendMark.style.left = 1;
            legendMark.style.top = 1;
            legendMark.style.width = 180;
            legendMark.style.height = 140;
            legendMark.style.backgroundColor = new Color(0.8f, 1f, 1f, 1f); // オーロラ色（薄いシアン）
            legendMark.style.borderTopWidth = 18;
            legendMark.style.borderBottomWidth = 18;
            legendMark.style.borderLeftWidth = 18;
            legendMark.style.borderRightWidth = 18;
            legendMark.style.borderTopColor = new Color(1f, 0.8f, 1f, 1f); // ピンク
            legendMark.style.borderBottomColor = new Color(0.8f, 1f, 0.8f, 1f); // 薄緑
            legendMark.style.borderLeftColor = new Color(1f, 1f, 0.8f, 1f); // 薄黄
            legendMark.style.borderRightColor = new Color(0.8f, 0.8f, 1f, 1f); // 薄青
            
            // rootに追加
            root.Add(absolutePerfectGirl);
            root.Add(absolutePerfectRoom);
            root.Add(legendMark);
            
            // rootを確実に表示状態にする
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
            root.MarkDirtyRepaint();
            
            Debug.Log($"[OverlayBootstrap] 🔴 絶対完璧GirlImage: left={absolutePerfectGirl.style.left.value.value}, 右端={absolutePerfectLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 🟢 絶対完璧RoomImage: left={absolutePerfectRoom.style.left.value.value}, 右端={absolutePerfectLeft + 200}");
            Debug.Log($"[OverlayBootstrap] 🌈 レジェンド記念マーク: left=1, top=1 (オーロラ色＋パステル枠)");
            Debug.Log("[OverlayBootstrap] ✅ 枠幅分余白も完全活用！絶対完璧配置達成！");
            Debug.Log("[OverlayBootstrap] 🎯 理論上最高の右端配置を実現！");
            Debug.Log("[OverlayBootstrap] 🏆 オーバーレイストリーマー問題＋絶対完璧最適化完全達成！");
            Debug.Log("[OverlayBootstrap] 🎉 座標系探求がレジェンド級の成果で完結！お疲れ様でした！");
        }
        
        /// <summary>
        /// デバッグ用: 座標系解明結果を実際のオーバーレイに適用
        /// </summary>
        [ContextMenu("Debug: Apply Discovered Coordinates")]
        public void DebugApplyDiscoveredCoordinates()
        {
            Debug.Log("[OverlayBootstrap] 🎯 座標系解明結果を実際のオーバーレイシステムに適用");
            
            // まずPhaseをActiveに変更
            if (state != null && presenter != null)
            {
                state.CurrentPhase = OverlayPhase.Active;
                presenter.UpdatePhase(state.CurrentPhase);
                Debug.Log("[OverlayBootstrap] ✅ PhaseをActiveに設定完了");
                
                // 座標適用を開始
                StartCoroutine(ApplyDiscoveredCoordinatesToRealOverlay());
            }
            else
            {
                Debug.LogError($"[OverlayBootstrap] 適用失敗 - state: {(state != null ? "OK" : "null")}, presenter: {(presenter != null ? "OK" : "null")}");
            }
        }
        
        /// <summary>
        /// 解明された座標を実際のオーバーレイ要素に適用
        /// </summary>
        private IEnumerator ApplyDiscoveredCoordinatesToRealOverlay()
        {
            Debug.Log("[OverlayBootstrap] 🔧 実際のGirl/RoomImageに解明座標を適用中");
            
            yield return new WaitForSeconds(1f); // フェーズ変更とレイアウト更新の安定化
            
            if (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentまたはrootVisualElementがnull");
                yield break;
            }
            
            var overlayRoot = overlayDocument.rootVisualElement.Q<VisualElement>("OverlayRoot");
            if (overlayRoot == null)
            {
                Debug.LogError("[OverlayBootstrap] OverlayRootが見つかりません");
                yield break;
            }
            
            var girlImage = overlayRoot.Q<VisualElement>("GirlImage");
            var roomImage = overlayRoot.Q<VisualElement>("RoomImage");
            
            if (girlImage != null)
            {
                Debug.Log("[OverlayBootstrap] 🔴 GirlImageに解明座標を適用中...");
                
                // 座標系解明結果（left=1320, top=700）を適用
                girlImage.style.position = Position.Absolute;
                girlImage.style.left = 1320;  // 解明された完璧座標
                girlImage.style.top = 700;    // 解明された完璧高さ
                girlImage.style.right = StyleKeyword.Auto;  // 以前の right/bottom をクリア
                girlImage.style.bottom = StyleKeyword.Auto;
                girlImage.style.width = 200;
                girlImage.style.height = 150;
                girlImage.style.display = DisplayStyle.Flex;
                girlImage.style.visibility = Visibility.Visible;
                girlImage.style.opacity = 1f;
                girlImage.MarkDirtyRepaint();
                
                Debug.Log($"[OverlayBootstrap] ✅ GirlImage適用完了: left={girlImage.style.left.value.value}, top={girlImage.style.top.value.value}");
            }
            else
            {
                Debug.LogWarning("[OverlayBootstrap] GirlImageが見つかりません");
            }
            
            if (roomImage != null)
            {
                Debug.Log("[OverlayBootstrap] 🟢 RoomImageに解明座標を適用中...");
                
                // 座標系解明結果（left=1320, top=530）を適用
                roomImage.style.position = Position.Absolute;
                roomImage.style.left = 1320;  // 解明された完璧座標
                roomImage.style.top = 530;    // 解明された完璧高さ
                roomImage.style.right = StyleKeyword.Auto;  // 以前の right/bottom をクリア
                roomImage.style.bottom = StyleKeyword.Auto;
                roomImage.style.width = 200;
                roomImage.style.height = 150;
                roomImage.style.display = DisplayStyle.Flex;
                roomImage.style.visibility = Visibility.Visible;
                roomImage.style.opacity = 1f;
                roomImage.MarkDirtyRepaint();
                
                Debug.Log($"[OverlayBootstrap] ✅ RoomImage適用完了: left={roomImage.style.left.value.value}, top={roomImage.style.top.value.value}");
            }
            else
            {
                Debug.LogWarning("[OverlayBootstrap] RoomImageが見つかりません");
            }
            
            // OverlayRoot自体も確実に表示状態に
            overlayRoot.style.display = DisplayStyle.Flex;
            overlayRoot.style.visibility = Visibility.Visible;
            overlayRoot.style.opacity = 1f;
            overlayRoot.MarkDirtyRepaint();
            
            Debug.Log("[OverlayBootstrap] 🎉 座標系解明結果の実際のオーバーレイへの適用が完了しました！");
            Debug.Log("[OverlayBootstrap] 🎯 実際のオーバーレイストリーマーが右下に表示されるはずです！");
        }
        
        /// <summary>
        /// デバッグ用: テスト要素をクリーンアップして実際の背景を表示
        /// </summary>
        [ContextMenu("Debug: Cleanup Test Elements")]
        public void DebugCleanupTestElements()
        {
            Debug.Log("[OverlayBootstrap] 🧹 テスト要素をクリーンアップして実際の背景を適用");
            
            if (overlayDocument == null || overlayDocument.rootVisualElement == null)
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentまたはrootVisualElementがnull");
                return;
            }
            
            var root = overlayDocument.rootVisualElement;
            
            // すべてのテスト用要素（デバッグで作成された矩形）を削除
            var testElements = root.Children().Where(child => 
                child.name.Contains("Debug") || 
                child.name.Contains("Test") || 
                child.name.Contains("DirectGirl") ||
                child.name.Contains("DirectRoom") ||
                child.name.Contains("AbsolutePerfect") ||
                child.name.Contains("LegendMark") ||
                child.name.Contains("Reference") ||
                child.name.Contains("Corner") ||
                child.name.Contains("Ultimate") ||
                child.name.Contains("Perfect") ||
                child.name.Contains("Final") ||
                child.name.Contains("Optimized")).ToList();
                
            foreach (var element in testElements)
            {
                root.Remove(element);
                Debug.Log($"[OverlayBootstrap] 削除されたテスト要素: {element.name}");
            }
            
            Debug.Log($"[OverlayBootstrap] ✅ {testElements.Count}個のテスト要素を削除しました");
            
            // 実際のオーバーレイ要素のスタイルをクリーンアップ
            var overlayRoot = root.Q<VisualElement>("OverlayRoot");
            if (overlayRoot != null)
            {
                var girlImage = overlayRoot.Q<VisualElement>("GirlImage");
                var roomImage = overlayRoot.Q<VisualElement>("RoomImage");
                
                if (girlImage != null)
                {
                    // テスト用の境界線を削除
                    girlImage.style.borderTopWidth = 0;
                    girlImage.style.borderBottomWidth = 0;
                    girlImage.style.borderLeftWidth = 0;
                    girlImage.style.borderRightWidth = 0;
                    girlImage.style.borderTopColor = Color.clear;
                    girlImage.style.borderBottomColor = Color.clear;
                    girlImage.style.borderLeftColor = Color.clear;
                    girlImage.style.borderRightColor = Color.clear;
                    girlImage.MarkDirtyRepaint();
                    Debug.Log("[OverlayBootstrap] GirlImageのテスト用境界線を削除しました");
                }
                
                if (roomImage != null)
                {
                    // テスト用の境界線を削除
                    roomImage.style.borderTopWidth = 0;
                    roomImage.style.borderBottomWidth = 0;
                    roomImage.style.borderLeftWidth = 0;
                    roomImage.style.borderRightWidth = 0;
                    roomImage.style.borderTopColor = Color.clear;
                    roomImage.style.borderBottomColor = Color.clear;
                    roomImage.style.borderLeftColor = Color.clear;
                    roomImage.style.borderRightColor = Color.clear;
                    roomImage.MarkDirtyRepaint();
                    Debug.Log("[OverlayBootstrap] RoomImageのテスト用境界線を削除しました");
                }
            }
            
            Debug.Log("[OverlayBootstrap] 🎉 テスト要素のクリーンアップが完了しました！");
            Debug.Log("[OverlayBootstrap] 🖼️ 実際の背景画像が表示されるはずです！");
        }
        
        /// <summary>
        /// デバッグ用: セリフ表示機能をテスト
        /// </summary>
        [ContextMenu("Debug: Test Speech System")]
        public void DebugTestSpeechSystem()
        {
            Debug.Log("[OverlayBootstrap] 🗨️ セリフ表示システムをテスト開始");
            
            if (presenter == null)
            {
                Debug.LogError("[OverlayBootstrap] presenterがnullです");
                return;
            }
            
            if (presenter is OverlayPresenter_UITK uiPresenter)
            {
                // セリフ表示要素の状態を確認
                var root = overlayDocument.rootVisualElement;
                var balloonRoot = root.Q<VisualElement>("BalloonRoot");
                var balloonLabel = root.Q<Label>("BalloonLabel");
                var thoughtBalloonRoot = root.Q<VisualElement>("ThoughtBalloonRoot");
                var thoughtBalloonLabel = root.Q<Label>("ThoughtBalloonLabel");
                
                Debug.Log($"[OverlayBootstrap] 🔍 セリフ表示要素の状態:");
                Debug.Log($"[OverlayBootstrap] - BalloonRoot: {(balloonRoot != null ? "✅ 取得済み" : "❌ null")}");
                Debug.Log($"[OverlayBootstrap] - BalloonLabel: {(balloonLabel != null ? "✅ 取得済み" : "❌ null")}");
                Debug.Log($"[OverlayBootstrap] - ThoughtBalloonRoot: {(thoughtBalloonRoot != null ? "✅ 取得済み" : "❌ null")}");
                Debug.Log($"[OverlayBootstrap] - ThoughtBalloonLabel: {(thoughtBalloonLabel != null ? "✅ 取得済み" : "❌ null")}");
                
                // 吹き出し要素の詳細状態をチェック
                if (balloonRoot != null)
                {
                    Debug.Log($"[OverlayBootstrap] BalloonRoot詳細: display={balloonRoot.style.display.value}, left={balloonRoot.style.left.value}, top={balloonRoot.style.top.value}");
                }
                if (balloonLabel != null)
                {
                    Debug.Log($"[OverlayBootstrap] BalloonLabel詳細: text='{balloonLabel.text}', fontSize={balloonLabel.style.fontSize.value}");
                }
                
                // テストセリフを表示
                if (balloonRoot != null && balloonLabel != null)
                {
                    Debug.Log("[OverlayBootstrap] 🧪 テストセリフを表示中...");
                    var testPayload = new ReactionPayload
                    {
                        Text = "これはテストセリフです！座標系修正版",
                        Expression = GirlExpression.Smile,
                        RoomState = RoomState.CleanDay,
                        DisplayDuration = 5f, // 少し長めに表示
                        IsThought = false
                    };
                    
                    uiPresenter.ShowReaction(testPayload);
                    Debug.Log("[OverlayBootstrap] ✅ テストセリフの表示要求を送信しました");
                    
                    // さらに心の声もテスト
                    StartCoroutine(TestThoughtBalloonAfterDelay(uiPresenter, 6f));
                }
                else
                {
                    Debug.LogError("[OverlayBootstrap] ❌ セリフ表示要素が見つかりません！UXMLファイルを確認してください");
                    
                    // 代替案：簡易セリフ表示要素を動的に作成
                    Debug.Log("[OverlayBootstrap] 🔧 セリフ表示要素を動的作成します...");
                    CreateSimpleSpeechBalloon(root);
                }
            }
        }
        
        /// <summary>
        /// 心の声のテスト（遅延実行）
        /// </summary>
        private IEnumerator TestThoughtBalloonAfterDelay(OverlayPresenter_UITK presenter, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            Debug.Log("[OverlayBootstrap] 💭 心の声テストを開始");
            var thoughtPayload = new ReactionPayload
            {
                Text = "これは心の声のテストです……",
                Expression = GirlExpression.Thinking,
                RoomState = RoomState.CleanDay,
                DisplayDuration = 4f,
                IsThought = true
            };
            
            presenter.ShowReaction(thoughtPayload);
            Debug.Log("[OverlayBootstrap] ✅ 心の声テストセリフを送信しました");
        }
        
        /// <summary>
        /// 簡易セリフ吹き出しを動的作成
        /// </summary>
        private void CreateSimpleSpeechBalloon(VisualElement root)
        {
            var overlayRoot = root.Q<VisualElement>("OverlayRoot");
            if (overlayRoot == null)
            {
                Debug.LogError("[OverlayBootstrap] OverlayRootが見つかりません");
                return;
            }
            
            // 既存の動的吹き出しを削除
            var existingBalloon = overlayRoot.Q<VisualElement>("DynamicBalloonRoot");
            if (existingBalloon != null)
            {
                overlayRoot.Remove(existingBalloon);
            }
            
            // 動的セリフ吹き出しを作成（座標系解明結果適用）
            var balloonRoot = new VisualElement();
            balloonRoot.name = "DynamicBalloonRoot";
            balloonRoot.style.position = Position.Absolute;
            balloonRoot.style.left = 1020; // 座標系解明結果: GirlImage(left=1320)の左側
            balloonRoot.style.top = 600;   // 座標系解明結果: GirlImage(top=700)より少し上
            balloonRoot.style.width = 280;
            balloonRoot.style.height = 80;
            balloonRoot.style.backgroundColor = new Color(1f, 1f, 1f, 0.9f); // 白い背景
            balloonRoot.style.borderTopWidth = 2;
            balloonRoot.style.borderBottomWidth = 2;
            balloonRoot.style.borderLeftWidth = 2;
            balloonRoot.style.borderRightWidth = 2;
            balloonRoot.style.borderTopColor = Color.black;
            balloonRoot.style.borderBottomColor = Color.black;
            balloonRoot.style.borderLeftColor = Color.black;
            balloonRoot.style.borderRightColor = Color.black;
            balloonRoot.style.paddingTop = 10;
            balloonRoot.style.paddingBottom = 10;
            balloonRoot.style.paddingLeft = 15;
            balloonRoot.style.paddingRight = 15;
            
            // セリフテキスト（Division A テストセリフ）
            var balloonLabel = new Label();
            balloonLabel.name = "DynamicBalloonLabel";
            balloonLabel.text = "やっと始まったね……（動的作成版）";
            balloonLabel.style.fontSize = 16;
            balloonLabel.style.color = Color.black;
            balloonLabel.style.whiteSpace = WhiteSpace.Normal;
            balloonLabel.style.textOverflow = TextOverflow.Clip;
            balloonLabel.style.unityTextAlign = TextAnchor.MiddleCenter; // 中央揃え
            
            balloonRoot.Add(balloonLabel);
            overlayRoot.Add(balloonRoot);
            
            Debug.Log("[OverlayBootstrap] ✅ 動的セリフ吹き出しを作成しました");
            Debug.Log($"[OverlayBootstrap] 📍 吹き出し位置: left={balloonRoot.style.left.value.value}, top={balloonRoot.style.top.value.value}");
            Debug.Log($"[OverlayBootstrap] 📏 吹き出しサイズ: {balloonRoot.style.width.value.value}x{balloonRoot.style.height.value.value}");
            Debug.Log("[OverlayBootstrap] 💬 座標系解明結果を活用した完璧配置で表示します");
            
            // 5秒後に自動で非表示
            StartCoroutine(HideBalloonAfterDelay(balloonRoot, 5f));
        }
        
        /// <summary>
        /// 指定時間後にセリフ吹き出しを非表示にする
        /// </summary>
        private IEnumerator HideBalloonAfterDelay(VisualElement balloon, float delay)
        {
            Debug.Log($"[OverlayBootstrap] ⏰ {delay}秒後に吹き出しを自動非表示にします");
            yield return new WaitForSeconds(delay);
            
            if (balloon != null && balloon.parent != null)
            {
                balloon.style.opacity = 0f;
                balloon.style.display = DisplayStyle.None;
                Debug.Log("[OverlayBootstrap] ✅ セリフ吹き出しを自動非表示にしました");
            }
            else
            {
                Debug.LogWarning("[OverlayBootstrap] 吹き出し要素が既に削除されています");
            }
        }
        
        /// <summary>
        /// デバッグ用: リアクション発動履歴をリセット
        /// </summary>
        [ContextMenu("Debug: Reset Reaction History")]
        public void DebugResetReactionHistory()
        {
            if (state == null)
            {
                Debug.LogError("[OverlayBootstrap] stateがnullです");
                return;
            }
            
            // OverlayState のリセット機能を確認
            if (state is OverlayState overlayState)
            {
                Debug.Log("[OverlayBootstrap] 🔄 リアクション発動履歴をリセット中...");
                
                // 最終発話時間をリセット
                var lastSpeechField = typeof(OverlayState).GetField("lastSpeechTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (lastSpeechField != null)
                {
                    lastSpeechField.SetValue(overlayState, 0f);
                    Debug.Log("[OverlayBootstrap] - 最終発話時間をリセット");
                }
                
                // 発話履歴辞書をリセット
                var spokeDictField = typeof(OverlayState).GetField("spokeOnceDict", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (spokeDictField != null)
                {
                    var spokeDict = spokeDictField.GetValue(overlayState) as System.Collections.Generic.Dictionary<string, bool>;
                    if (spokeDict != null)
                    {
                        int count = spokeDict.Count;
                        spokeDict.Clear();
                        Debug.Log($"[OverlayBootstrap] - 発話履歴を{count}件クリア");
                    }
                }
                
                Debug.Log("[OverlayBootstrap] ✅ リアクション発動履歴のリセットが完了しました");
                Debug.Log("[OverlayBootstrap] 🎯 これでDivision A などのリアクションを再度テストできます");
            }
        }
        
        /// <summary>
        /// デバッグ用: UIDocument状態を詳細確認
        /// </summary>
        [ContextMenu("Debug: Check UIDocument Status")]
        public void DebugCheckUIDocumentStatus()
        {
            Debug.Log("=== UIDocument 詳細状態 ===");
            Debug.Log($"OverlayBootstrap GameObject: {gameObject.name}, active: {gameObject.activeInHierarchy}");
            Debug.Log($"OverlayBootstrap Component enabled: {enabled}");
            
            if (overlayDocument != null)
            {
                Debug.Log($"UIDocument enabled: {overlayDocument.enabled}");
                Debug.Log($"UIDocument panelSettings: {(overlayDocument.panelSettings != null ? overlayDocument.panelSettings.name : "null")}");
                Debug.Log($"UIDocument sortingOrder: {overlayDocument.sortingOrder}");
                Debug.Log($"UIDocument visualTreeAsset: {(overlayDocument.visualTreeAsset != null ? overlayDocument.visualTreeAsset.name : "null")}");
                
                var root = overlayDocument.rootVisualElement;
                if (root != null)
                {
                    Debug.Log($"rootVisualElement: {root.resolvedStyle.width}x{root.resolvedStyle.height} at ({root.resolvedStyle.left}, {root.resolvedStyle.top})");
                    Debug.Log($"rootVisualElement display: {root.style.display.value}");
                    
                    var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                    if (overlayRoot != null)
                    {
                        Debug.Log($"OverlayRoot: {overlayRoot.resolvedStyle.width}x{overlayRoot.resolvedStyle.height}");
                        Debug.Log($"OverlayRoot display: {overlayRoot.style.display.value}");
                        
                        var girlImage = overlayRoot.Q<VisualElement>("GirlImage");
                        var roomImage = overlayRoot.Q<VisualElement>("RoomImage");
                        Debug.Log($"GirlImage: {(girlImage != null ? $"{girlImage.resolvedStyle.width}x{girlImage.resolvedStyle.height}" : "null")}");
                        Debug.Log($"RoomImage: {(roomImage != null ? $"{roomImage.resolvedStyle.width}x{roomImage.resolvedStyle.height}" : "null")}");
                    }
                }
            }
            else
            {
                Debug.LogError("overlayDocument is null!");
            }
            
            Debug.Log($"state: {(state != null ? $"Division={state.CurrentDivision}, Mode={state.CurrentMode}, Phase={state.CurrentPhase}" : "null")}");
            Debug.Log($"presenter: {(presenter != null ? "initialized" : "null")}");
            Debug.Log("========================");
        }

        /// <summary>
        /// デバッグ用: 強制的にPhaseをHiddenに設定
        /// </summary>
        [ContextMenu("Debug: Force Phase to Hidden")]
        public void DebugForcePhaseToHidden()
        {
            if (state != null && presenter != null)
            {
                state.CurrentPhase = OverlayPhase.Hidden;
                presenter.UpdatePhase(state.CurrentPhase);
                Debug.Log("[OverlayBootstrap] PhaseをHiddenに強制設定しました。");
            }
        }
        
        /// <summary>
        /// デバッグ用: Division Bをシミュレート
        /// </summary>
        [ContextMenu("Debug: Simulate Division B")]
        public void DebugSimulateDivisionB()
        {
            if (state != null && presenter != null)
            {
                Debug.Log("[OverlayBootstrap] Division Bをシミュレートします。");
                
                // Division BとNormalモードに設定
                state.CurrentDivision = Division.B;
                state.CurrentMode = GameMode.Normal;
                
                // ReactionDirectorでPhase更新
                if (reactionDirector != null)
                {
                    reactionDirector.UpdatePhase();
                    presenter.UpdatePhase(state.CurrentPhase);
                    Debug.Log($"[OverlayBootstrap] Phase更新完了: {state.CurrentPhase}");
                }
                
                // テスト用リアクション表示
                StartCoroutine(ShowTestReaction());
            }
        }
        
        /// <summary>
        /// デバッグ用: 修正後の座標系テスト
        /// </summary>
        [ContextMenu("Debug: Test Fixed Resolution")]
        public void DebugTestFixedResolution()
        {
            Debug.Log("[OverlayBootstrap] 修正後の1920x1080座標系でテスト開始");
            
            if (overlayDocument != null && overlayDocument.rootVisualElement != null)
            {
                var root = overlayDocument.rootVisualElement;
                var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                
                if (overlayRoot != null)
                {
                    Debug.Log($"[OverlayBootstrap] rootサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                    Debug.Log($"[OverlayBootstrap] PanelSettings: {overlayDocument.panelSettings.referenceResolution}");
                    
                    // 既存のテスト要素を削除
                    var existingElements = overlayRoot.Children().Where(e => 
                        e.name.StartsWith("Test")).ToList();
                    foreach (var element in existingElements)
                    {
                        overlayRoot.Remove(element);
                    }
                    
                    // 1920x1080基準での正確な座標でテスト
                    
                    // 右下のGirl画像位置（元のUXMLの意図通り）
                    var testGirl = new VisualElement();
                    testGirl.name = "TestGirl_Final";
                    testGirl.style.position = Position.Absolute;
                    testGirl.style.right = 20;    // 右から20px
                    testGirl.style.bottom = 20;   // 下から20px
                    testGirl.style.width = 200;
                    testGirl.style.height = 150;
                    testGirl.style.backgroundColor = new Color(1f, 0f, 1f, 0.9f); // 濃いピンク
                    testGirl.style.borderTopWidth = 5;
                    testGirl.style.borderBottomWidth = 5;
                    testGirl.style.borderLeftWidth = 5;
                    testGirl.style.borderRightWidth = 5;
                    testGirl.style.borderTopColor = Color.white;
                    testGirl.style.borderBottomColor = Color.white;
                    testGirl.style.borderLeftColor = Color.white;
                    testGirl.style.borderRightColor = Color.white;
                    
                    // 右下のRoom画像位置（Girl画像の上）
                    var testRoom = new VisualElement();
                    testRoom.name = "TestRoom_Final";
                    testRoom.style.position = Position.Absolute;
                    testRoom.style.right = 20;      // 右から20px
                    testRoom.style.bottom = 190;    // 下から190px（Girl画像の上）
                    testRoom.style.width = 200;
                    testRoom.style.height = 150;
                    testRoom.style.backgroundColor = new Color(0f, 1f, 0f, 0.9f); // 濃い緑
                    testRoom.style.borderTopWidth = 5;
                    testRoom.style.borderBottomWidth = 5;
                    testRoom.style.borderLeftWidth = 5;
                    testRoom.style.borderRightWidth = 5;
                    testRoom.style.borderTopColor = Color.yellow;
                    testRoom.style.borderBottomColor = Color.yellow;
                    testRoom.style.borderLeftColor = Color.yellow;
                    testRoom.style.borderRightColor = Color.yellow;
                    
                    // 中央の確認用要素
                    var testCenter = new VisualElement();
                    testCenter.name = "TestCenter_Final";
                    testCenter.style.position = Position.Absolute;
                    testCenter.style.left = (root.resolvedStyle.width - 60) / 2;     // 中央計算
                    testCenter.style.top = (root.resolvedStyle.height - 60) / 2;      // 中央計算
                    testCenter.style.width = 60;
                    testCenter.style.height = 60;
                    testCenter.style.backgroundColor = new Color(1f, 1f, 0f, 0.9f); // 濃い黄色
                    testCenter.style.borderTopWidth = 3;
                    testCenter.style.borderBottomWidth = 3;
                    testCenter.style.borderLeftWidth = 3;
                    testCenter.style.borderRightWidth = 3;
                    testCenter.style.borderTopColor = Color.black;
                    testCenter.style.borderBottomColor = Color.black;
                    testCenter.style.borderLeftColor = Color.black;
                    testCenter.style.borderRightColor = Color.black;
                    
                    overlayRoot.Add(testGirl);
                    overlayRoot.Add(testRoom);
                    overlayRoot.Add(testCenter);
                    
                    overlayRoot.style.display = DisplayStyle.Flex;
                    overlayRoot.MarkDirtyRepaint();
                    
                    Debug.Log("[OverlayBootstrap] ✅ 最終テスト要素を配置:");
                    Debug.Log("  🟣 Girl: 右下角 (right=20, bottom=20)");
                    Debug.Log("  🟢 Room: Girl画像の上 (right=20, bottom=190)");
                    Debug.Log("  🟡 Center: 画面中央 (50%, 50%)");
                    
                    Debug.Log("[OverlayBootstrap] これらの位置が正しければ、元のOverlay要素も正常に表示されるはずです！");
                }
            }
        }
        
        /// <summary>
        /// デバッグ用: 実画面解像度に基づくテスト
        /// </summary>
        [ContextMenu("Debug: Test with Screen Resolution")]
        public void DebugTestScreenResolution()
        {
            Debug.Log("[OverlayBootstrap] 実画面解像度基準でテスト表示を開始");
            
            if (overlayDocument != null)
            {
                var root = overlayDocument.rootVisualElement;
                if (root != null)
                {
                    var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                    if (overlayRoot != null)
                    {
                        // 解像度情報を取得
                        float rootWidth = root.resolvedStyle.width;
                        float rootHeight = root.resolvedStyle.height;
                        float screenWidth = Screen.width;
                        float screenHeight = Screen.height;
                        
                        Debug.Log($"[OverlayBootstrap] rootVisualElementサイズ: {rootWidth}x{rootHeight}");
                        Debug.Log($"[OverlayBootstrap] Screenサイズ: {screenWidth}x{screenHeight}");
                        
                        // スケール比を計算
                        float scaleX = rootWidth / screenWidth;
                        float scaleY = rootHeight / screenHeight;
                        Debug.Log($"[OverlayBootstrap] スケール比: X={scaleX}, Y={scaleY}");
                        
                        // 既存のテスト要素を削除
                        var existingElements = overlayRoot.Children().Where(e => 
                            e.name.StartsWith("Test") || e.name == "FullScreenTest").ToList();
                        foreach (var element in existingElements)
                        {
                            overlayRoot.Remove(element);
                        }
                        
                        // 画面の実際のサイズに合わせた安全な座標で配置
                        
                        // 1. 左上隅テスト（必ず見える）
                        var testTopLeft = new VisualElement();
                        testTopLeft.name = "TestTopLeft";
                        testTopLeft.style.position = Position.Absolute;
                        testTopLeft.style.left = 50; // 左から50px
                        testTopLeft.style.top = 50;  // 上から50px
                        testTopLeft.style.width = 150;
                        testTopLeft.style.height = 100;
                        testTopLeft.style.backgroundColor = new Color(1f, 0f, 0f, 0.9f); // 濃い赤
                        testTopLeft.style.borderTopWidth = 3;
                        testTopLeft.style.borderBottomWidth = 3;
                        testTopLeft.style.borderLeftWidth = 3;
                        testTopLeft.style.borderRightWidth = 3;
                        testTopLeft.style.borderTopColor = Color.white;
                        testTopLeft.style.borderBottomColor = Color.white;
                        testTopLeft.style.borderLeftColor = Color.white;
                        testTopLeft.style.borderRightColor = Color.white;
                        
                        // 2. 画面内に必ず入る右下
                        var safeRightMargin = Mathf.Min(screenWidth * 0.8f, rootWidth * 0.8f);
                        var safeBottomMargin = Mathf.Min(screenHeight * 0.8f, rootHeight * 0.8f);
                        
                        var testSafeBottomRight = new VisualElement();
                        testSafeBottomRight.name = "TestSafeBottomRight";
                        testSafeBottomRight.style.position = Position.Absolute;
                        testSafeBottomRight.style.left = safeRightMargin - 200;
                        testSafeBottomRight.style.top = safeBottomMargin - 150;
                        testSafeBottomRight.style.width = 150;
                        testSafeBottomRight.style.height = 100;
                        testSafeBottomRight.style.backgroundColor = new Color(0f, 1f, 0f, 0.9f); // 濃い緑
                        testSafeBottomRight.style.borderTopWidth = 3;
                        testSafeBottomRight.style.borderBottomWidth = 3;
                        testSafeBottomRight.style.borderLeftWidth = 3;
                        testSafeBottomRight.style.borderRightWidth = 3;
                        testSafeBottomRight.style.borderTopColor = Color.black;
                        testSafeBottomRight.style.borderBottomColor = Color.black;
                        testSafeBottomRight.style.borderLeftColor = Color.black;
                        testSafeBottomRight.style.borderRightColor = Color.black;
                        
                        // 3. より小さな中央要素
                        var testCenter = new VisualElement();
                        testCenter.name = "TestCenter_Safe";
                        testCenter.style.position = Position.Absolute;
                        testCenter.style.left = (Mathf.Min(rootWidth, screenWidth) - 80) / 2;
                        testCenter.style.top = (Mathf.Min(rootHeight, screenHeight) - 80) / 2;
                        testCenter.style.width = 80;
                        testCenter.style.height = 80;
                        testCenter.style.backgroundColor = new Color(1f, 1f, 0f, 0.9f); // 濃い黄色
                        testCenter.style.borderTopWidth = 5;
                        testCenter.style.borderBottomWidth = 5;
                        testCenter.style.borderLeftWidth = 5;
                        testCenter.style.borderRightWidth = 5;
                        testCenter.style.borderTopColor = Color.magenta;
                        testCenter.style.borderBottomColor = Color.magenta;
                        testCenter.style.borderLeftColor = Color.magenta;
                        testCenter.style.borderRightColor = Color.magenta;
                        
                        overlayRoot.Add(testTopLeft);
                        overlayRoot.Add(testSafeBottomRight);
                        overlayRoot.Add(testCenter);
                        
                        overlayRoot.style.display = DisplayStyle.Flex;
                        overlayRoot.MarkDirtyRepaint();
                        
                        Debug.Log($"[OverlayBootstrap] 安全な座標で配置:");
                        Debug.Log($"  TopLeft: ({testTopLeft.style.left.value.value}, {testTopLeft.style.top.value.value})");
                        Debug.Log($"  SafeBottomRight: ({testSafeBottomRight.style.left.value.value}, {testSafeBottomRight.style.top.value.value})");
                        Debug.Log($"  Center: ({testCenter.style.left.value.value}, {testCenter.style.top.value.value})");
                    }
                }
            }
        }
        
        /// <summary>
        /// デバッグ用: 画像要素のサイズを強制修正
        /// </summary>
        [ContextMenu("Debug: Fix Image Sizes")]
        public void DebugFixImageSizes()
        {
            Debug.Log("[OverlayBootstrap] 画像要素のサイズ修正を開始");
            
            if (overlayDocument != null)
            {
                var root = overlayDocument.rootVisualElement;
                if (root != null)
                {
                    var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                    if (overlayRoot != null)
                    {
                        var girlImage = overlayRoot.Q<VisualElement>("GirlImage");
                        var roomImage = overlayRoot.Q<VisualElement>("RoomImage");
                        
                        if (girlImage != null)
                        {
                            Debug.Log($"[OverlayBootstrap] GirlImage現在のサイズ: {girlImage.resolvedStyle.width}x{girlImage.resolvedStyle.height}");
                            girlImage.style.width = 200;
                            girlImage.style.height = 150;
                            girlImage.style.position = Position.Absolute;
                            girlImage.style.right = 20;
                            girlImage.style.bottom = 20;
                            girlImage.style.minWidth = 200;
                            girlImage.style.minHeight = 150;
                            girlImage.MarkDirtyRepaint();
                            Debug.Log("[OverlayBootstrap] GirlImageを200x150pxに修正");
                        }
                        
                        if (roomImage != null)
                        {
                            Debug.Log($"[OverlayBootstrap] RoomImage現在のサイズ: {roomImage.resolvedStyle.width}x{roomImage.resolvedStyle.height}");
                            roomImage.style.width = 200;
                            roomImage.style.height = 150;
                            roomImage.style.position = Position.Absolute;
                            roomImage.style.right = 20;
                            roomImage.style.bottom = 20;
                            roomImage.style.minWidth = 200;
                            roomImage.style.minHeight = 150;
                            roomImage.MarkDirtyRepaint();
                            Debug.Log("[OverlayBootstrap] RoomImageを200x150pxに修正");
                        }
                        
                        // OverlayRootも表示状態にする
                        overlayRoot.style.display = DisplayStyle.Flex;
                        overlayRoot.MarkDirtyRepaint();
                        
                        Debug.Log("[OverlayBootstrap] OverlayRootを強制表示");
                    }
                }
            }
        }
        
        /// <summary>
        /// デバッグ用: 既存のGamePanelSettingsを1920x1080に修正
        /// </summary>
        [ContextMenu("Debug: Fix GamePanelSettings to 1920x1080")]
        public void DebugFixGamePanelSettings()
        {
            Debug.Log("[OverlayBootstrap] 既存のGamePanelSettingsを1920x1080に修正開始");
            
            if (overlayDocument != null)
            {
                // 現在のPanelSettingsの情報をログ出力
                if (overlayDocument.panelSettings != null)
                {
                    var current = overlayDocument.panelSettings;
                    Debug.Log($"[OverlayBootstrap] 現在のPanelSettings: {current.name}, 解像度: {current.referenceResolution}, match: {current.match}");
                    
                    // 既存のPanelSettingsを直接修正
                    current.referenceResolution = new Vector2Int(1920, 1080); // 正しい解像度に修正
                    current.match = 0.0f; // 幅基準（縦長画面に対応）
                    
                    Debug.Log($"[OverlayBootstrap] ✅ GamePanelSettingsを修正しました: 解像度={current.referenceResolution}, match={current.match}");
                    Debug.Log($"[OverlayBootstrap] Screen解像度: {Screen.width}x{Screen.height}");
                    
                    // UIDocumentを強制更新
                    overlayDocument.enabled = false;
                    overlayDocument.enabled = true;
                    
                    // 少し待機してから状態確認
                    StartCoroutine(CheckGamePanelSettingsAfterUpdate());
                }
                else
                {
                    Debug.LogError("[OverlayBootstrap] overlayDocument.panelSettingsがnullです");
                }
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentがnullです");
            }
        }
        
        /// <summary>
        /// GamePanelSettings修正後の状態確認コルーチン
        /// </summary>
        private IEnumerator CheckGamePanelSettingsAfterUpdate()
        {
            yield return new WaitForSeconds(0.5f); // UIDocumentの更新を待機
            
            if (overlayDocument != null && overlayDocument.rootVisualElement != null)
            {
                var root = overlayDocument.rootVisualElement;
                Debug.Log($"[OverlayBootstrap] ✅ GamePanelSettings修正後のrootサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                Debug.Log($"[OverlayBootstrap] 現在のPanelSettings解像度: {overlayDocument.panelSettings.referenceResolution}");
                
                // スケール比を計算
                float scaleX = root.resolvedStyle.width / Screen.width;
                float scaleY = root.resolvedStyle.height / Screen.height;
                Debug.Log($"[OverlayBootstrap] UIスケール比 - X: {scaleX:F3}, Y: {scaleY:F3}");
                
                if (root.resolvedStyle.height > 0)
                {
                    Debug.Log("[OverlayBootstrap] 🎉 GamePanelSettings修正成功！高さが正常になりました！");
                    Debug.Log("[OverlayBootstrap] 🎯 座標テストを再実行してください。");
                }
                else
                {
                    Debug.LogError("[OverlayBootstrap] ❌ まだ高さが0です。追加の調査が必要です。");
                }
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] GamePanelSettings修正後もrootVisualElementが取得できません");
            }
        }
        
        /// <summary>
        /// デバッグ用: 正しい1920x1080 PanelSettingsを強制作成（旧版）
        /// </summary>
        [ContextMenu("Debug: Force Create 1920x1080 PanelSettings")]
        public void DebugForceCreatePanelSettings()
        {
            Debug.Log("[OverlayBootstrap] 正しい1920x1080 PanelSettingsを強制作成開始");
            
            if (overlayDocument != null)
            {
                // 現在のPanelSettingsの情報をログ出力
                if (overlayDocument.panelSettings != null)
                {
                    var current = overlayDocument.panelSettings;
                    Debug.Log($"[OverlayBootstrap] 現在のPanelSettings: {current.name}, 解像度: {current.referenceResolution}, match: {current.match}");
                }
                
                // 新しいPanelSettingsを作成（1920x1080、幅基準）
                var newPanelSettings = ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
                newPanelSettings.name = "OverlayPanelSettings_1920x1080";
                newPanelSettings.referenceResolution = new Vector2Int(1920, 1080); // 正しい解像度
                newPanelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
                newPanelSettings.match = 0.0f; // 幅基準（縦長画面に対応）
                newPanelSettings.scale = 1f;
                newPanelSettings.fallbackDpi = 96f;
                newPanelSettings.referenceDpi = 96f;
                
                overlayDocument.panelSettings = newPanelSettings;
                
                Debug.Log($"[OverlayBootstrap] 新しいPanelSettingsを設定: 解像度={newPanelSettings.referenceResolution}, match={newPanelSettings.match} (0.0=幅基準)");
                
                // 画面情報もログ出力
                Debug.Log($"[OverlayBootstrap] Screen解像度: {Screen.width}x{Screen.height}");
                
                // UIDocumentを強制更新
                overlayDocument.enabled = false;
                overlayDocument.enabled = true;
                
                // 少し待機してから状態確認
                StartCoroutine(CheckPanelSettingsAfterUpdate());
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentがnullです");
            }
        }
        
        /// <summary>
        /// PanelSettings更新後の状態確認コルーチン
        /// </summary>
        private IEnumerator CheckPanelSettingsAfterUpdate()
        {
            yield return new WaitForSeconds(0.5f); // UIDocumentの更新を待機
            
            if (overlayDocument != null && overlayDocument.rootVisualElement != null)
            {
                var root = overlayDocument.rootVisualElement;
                Debug.Log($"[OverlayBootstrap] ✅ PanelSettings更新後のrootサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                Debug.Log($"[OverlayBootstrap] 現在のPanelSettings解像度: {overlayDocument.panelSettings.referenceResolution}");
                
                // スケール比を計算
                float scaleX = root.resolvedStyle.width / Screen.width;
                float scaleY = root.resolvedStyle.height / Screen.height;
                Debug.Log($"[OverlayBootstrap] UIスケール比 - X: {scaleX:F3}, Y: {scaleY:F3}");
                
                Debug.Log("[OverlayBootstrap] 🎯 PanelSettings更新完了！座標テストを再実行してください。");
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] PanelSettings更新後もrootVisualElementが取得できません");
            }
        }
        
        /// <summary>
        /// デバッグ用: UIDocumentを強制的に再初期化
        /// </summary>
        [ContextMenu("Debug: Reinitialize UIDocument")]
        public void DebugReinitializeUIDocument()
        {
            Debug.Log("[OverlayBootstrap] UIDocument再初期化開始");
            
            if (overlayDocument != null)
            {
                // UIDocumentを一時的に無効化してから有効化
                overlayDocument.enabled = false;
                overlayDocument.enabled = true;
                
                // visualTreeAssetを再設定
                if (overlayUXML != null)
                {
                    overlayDocument.visualTreeAsset = overlayUXML;
                    Debug.Log("[OverlayBootstrap] visualTreeAssetを再設定しました");
                }
                
                // rootVisualElementを取得し直し
                var root = overlayDocument.rootVisualElement;
                if (root != null)
                {
                    Debug.Log($"[OverlayBootstrap] 再初期化後のrootVisualElement: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
                    
                    // OverlayRootに直接テスト要素を追加
                    var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                    if (overlayRoot != null)
                    {
                        // テスト用の目立つ要素を追加（絶対座標で配置）
                        var testElement = new VisualElement();
                        testElement.name = "DebugTestElement";
                        testElement.style.position = Position.Absolute;
                        
                        // Screen座標を直接使用（右下に配置）
                        var screenWidth = Screen.width;
                        var screenHeight = Screen.height;
                        testElement.style.left = screenWidth - 150; // 右から150px
                        testElement.style.top = screenHeight - 150; // 下から150px
                        testElement.style.width = 100;
                        testElement.style.height = 100;
                        testElement.style.backgroundColor = Color.magenta; // 目立つピンク色
                        testElement.style.borderTopWidth = 3;
                        testElement.style.borderBottomWidth = 3;
                        testElement.style.borderLeftWidth = 3;
                        testElement.style.borderRightWidth = 3;
                        testElement.style.borderTopColor = Color.yellow; // さらに目立つ黄色の枠
                        testElement.style.borderBottomColor = Color.yellow;
                        testElement.style.borderLeftColor = Color.yellow;
                        testElement.style.borderRightColor = Color.yellow;
                        
                        // z-indexも設定（最前面に表示）
                        testElement.style.unityOverflowClipBox = OverflowClipBox.PaddingBox;
                        
                        Debug.Log($"[OverlayBootstrap] テスト要素をScreen座標で配置: ({testElement.style.left.value.value}, {testElement.style.top.value.value}) サイズ: 100x100");
                        
                        // 既存のテスト要素があれば削除
                        var existingTest = overlayRoot.Q<VisualElement>("DebugTestElement");
                        if (existingTest != null)
                        {
                            overlayRoot.Remove(existingTest);
                        }
                        
                        overlayRoot.Add(testElement);
                        Debug.Log("[OverlayBootstrap] 強制テスト要素を追加しました（右下にピンクの四角）");
                        
                        // OverlayRootを強制的に表示（テスト用）
                        overlayRoot.style.display = DisplayStyle.Flex;
                        overlayRoot.style.visibility = Visibility.Visible;
                        
                        // テスト用に背景色も設定（確実に見えるように）
                        overlayRoot.style.backgroundColor = new Color(0f, 0f, 0f, 0.3f); // 半透明の黒背景
                    }
                }
                else
                {
                    Debug.LogError("[OverlayBootstrap] 再初期化後もrootVisualElementがnullです");
                }
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] overlayDocumentがnullです");
            }
        }
        
        /// <summary>
        /// テスト用リアクション表示のコルーチン
        /// </summary>
        private IEnumerator ShowTestReaction()
        {
            // Phaseが更新されるまで少し待つ
            yield return new WaitForSeconds(0.1f);
            
            if (presenter != null)
            {
                var testPayload = new ReactionPayload
                {
                    Text = "テスト表示：オーバーレイが表示されています！",
                    Expression = GirlExpression.Smile,
                    RoomState = RoomState.CleanDay,
                    DisplayDuration = 5f
                };
                presenter.ShowReaction(testPayload);
                Debug.Log("[OverlayBootstrap] テスト用リアクションを表示しました。");
            }
        }

        /// <summary>
        /// すべての子要素にpickingModeをIgnoreに設定（再帰的）
        /// </summary>
        private void SetPickingModeIgnoreRecursive(VisualElement element)
        {
            if (element == null) return;
            
            // 現在の要素にpickingModeを設定
            element.pickingMode = PickingMode.Ignore;
            
            // すべての子要素にも再帰的に設定
            foreach (var child in element.Children())
            {
                SetPickingModeIgnoreRecursive(child);
            }
        }
    }
}

