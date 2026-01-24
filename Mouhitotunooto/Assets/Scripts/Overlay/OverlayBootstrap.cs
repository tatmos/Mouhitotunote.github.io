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

