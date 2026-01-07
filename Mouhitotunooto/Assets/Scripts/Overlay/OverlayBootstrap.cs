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
                    
                    if (allPanelSettings.Count > 0)
                    {
                        // 最初に見つかったPanelSettingsを使用
                        panelSettings = allPanelSettings[0];
                        Debug.Log("[OverlayBootstrap] PanelSettingsアセットを検索して設定しました。");
                    }
                }
                
                if (panelSettings != null)
                {
                    overlayDocument.panelSettings = panelSettings;
                    Debug.Log("[OverlayBootstrap] PanelSettingsを自動的に設定しました。");
                }
                else
                {
                    Debug.LogWarning("[OverlayBootstrap] PanelSettingsが見つかりません。手動で設定してください。Unityエディタで「Create > UI Toolkit > Panel Settings Asset」から作成するか、既存のPanelSettingsをアサインしてください。");
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
            }

            // Presenterを初期化
            var root = overlayDocument.rootVisualElement;
            if (root != null)
            {
                // スクロールバーを非表示にする
                root.style.overflow = Overflow.Hidden;
                
                // USSスタイルシートを適用（pointer-events: noneを確実に設定するため）
                if (overlayStyles != null)
                {
                    root.styleSheets.Add(overlayStyles);
                }
                
                // pickingModeをIgnoreに設定して、オーバーレイがイベントを無視するようにする
                // これにより、オーバーレイが表示されていても、下のUIが操作可能になる
                root.pickingMode = PickingMode.Ignore;
                
                // OverlayRootにもpickingModeを設定
                var overlayRoot = root.Q<VisualElement>("OverlayRoot");
                if (overlayRoot != null)
                {
                    overlayRoot.pickingMode = PickingMode.Ignore;
                    // すべての子要素にも再帰的に設定
                    SetPickingModeIgnoreRecursive(overlayRoot);
                }
                
                presenter = new OverlayPresenter_UITK(root, this);
            }
            else
            {
                Debug.LogError("[OverlayBootstrap] rootVisualElementがnullです。UXMLが正しく読み込まれていない可能性があります。");
                return;
            }

            // イベント購読
            SubscribeToEvents();

            // 初期状態を設定
            if (presenter != null)
            {
                presenter.UpdatePhase(state.CurrentPhase);
            }
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
            
            state.CurrentMode = evt.Mode;
            reactionDirector.UpdatePhase();
            presenter.UpdatePhase(state.CurrentPhase);

            // リアクションを選択
            var payload = reactionDirector.SelectReaction(evt);
            if (payload != null)
            {
                presenter.ShowReaction(payload);
            }
        }

        private void OnDivisionEntered(DivisionEnteredEvt evt)
        {
            if (state == null || reactionDirector == null || presenter == null) return;
            
            state.CurrentDivision = evt.Division;
            reactionDirector.UpdatePhase();
            presenter.UpdatePhase(state.CurrentPhase);

            // リアクションを選択
            var payload = reactionDirector.SelectReaction(evt);
            if (payload != null)
            {
                presenter.ShowReaction(payload);
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
            }
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

