using System.Linq;

namespace NovelGame.Overlay
{
    /// <summary>
    /// イベント＋状態→最適Reactionを選ぶ（競合解決・頻度制御）
    /// </summary>
    public class ReactionDirector
    {
        private readonly OverlayState state;
        private readonly PlayerTypeClassifier playerTypeClassifier;
        private readonly ReactionRule[] rules;

        public ReactionDirector(OverlayState state, PlayerTypeClassifier playerTypeClassifier)
        {
            this.state = state;
            this.playerTypeClassifier = playerTypeClassifier;
            this.rules = ReactionRules.GetAllRules();
        }

        /// <summary>
        /// イベントから最適なリアクションを選択
        /// </summary>
        public ReactionPayload SelectReaction(IOverlayEvent evt)
        {
            // コンテキストを作成
            var ctx = new ReactionContext
            {
                Event = evt,
                State = state,
                PlayerType = playerTypeClassifier.Classify()
            };

            // 条件を満たすルールをフィルタ
            var candidateRules = rules
                .Where(rule => rule.Condition != null && rule.Condition(ctx))
                .Where(rule => state.CurrentPhase >= rule.MinPhase && state.CurrentPhase <= rule.MaxPhase)
                .Where(rule =>
                {
                    // 初回のみチェック
                    if (rule.OncePerKeyEvent && !string.IsNullOrEmpty(rule.EventKey))
                    {
                        return state.CanSpeakOnce(rule.EventKey);
                    }
                    return state.CanSpeak();
                })
                .OrderByDescending(rule => rule.Priority)
                .ToList();

            if (candidateRules.Count == 0)
            {
                return null;
            }

            // 最優先ルールを選択
            var selectedRule = candidateRules[0];

            // ペイロードを生成
            var payload = selectedRule.PayloadFactory(ctx);

            // 発話を記録
            if (selectedRule.OncePerKeyEvent && !string.IsNullOrEmpty(selectedRule.EventKey))
            {
                state.RecordSpokeOnce(selectedRule.EventKey);
            }
            else
            {
                state.RecordSpoke();
            }

            return payload;
        }

        /// <summary>
        /// フェーズを更新（Division/Modeから判定）
        /// </summary>
        public void UpdatePhase()
        {
            OverlayPhase oldPhase = state.CurrentPhase;
            
            // Division/Modeからフェーズを判定
            if (state.CurrentDivision == Division.B && state.CurrentMode == GameMode.Normal)
            {
                state.CurrentPhase = OverlayPhase.Presence;
            }
            else if (state.CurrentMode == GameMode.Dark || state.CurrentMode == GameMode.Third)
            {
                if (state.CurrentDivision == Division.D || state.CurrentDivision == Division.E)
                {
                    state.CurrentPhase = OverlayPhase.Quiet;
                }
                else
                {
                    state.CurrentPhase = OverlayPhase.Active;
                }
            }
            else if (state.CurrentDivision == Division.A && state.CurrentMode == GameMode.Normal)
            {
                // Division=A, Mode=Normal でもオーバーレイストリーマーを表示
                state.CurrentPhase = OverlayPhase.Active;
            }
            else if (state.CurrentDivision != Division.None && state.CurrentMode == GameMode.Normal)
            {
                // その他のDivision（C, PreA等）でも基本的にはオーバーレイ表示
                state.CurrentPhase = OverlayPhase.Active;
            }
            else
            {
                state.CurrentPhase = OverlayPhase.Hidden;
            }
            
            // デバッグログ
            if (oldPhase != state.CurrentPhase)
            {
                UnityEngine.Debug.Log($"[ReactionDirector] Phase更新: {oldPhase} → {state.CurrentPhase} (Division: {state.CurrentDivision}, Mode: {state.CurrentMode})");
            }
        }
    }
}

