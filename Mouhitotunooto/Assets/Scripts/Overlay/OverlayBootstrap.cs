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

            // UXMLを読み込み
            if (overlayUXML != null)
            {
                overlayDocument.visualTreeAsset = overlayUXML;
            }
            else
            {
                Debug.LogWarning("[OverlayBootstrap] overlayUXMLが設定されていません。Overlay.uxmlを設定してください。");
            }

            // Presenterを初期化
            var root = overlayDocument.rootVisualElement;
            if (root != null)
            {
                // スクロールバーを非表示にする
                root.style.overflow = Overflow.Hidden;
                
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
            // 特にリアクションなし
        }

        private void OnDestroy()
        {
            // 購読をクリア（必要に応じて）
            // OverlayEventHub.Clear();
        }
    }
}

