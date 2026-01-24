using System;

namespace NovelGame.Overlay
{
    /// <summary>
    /// リアクション条件、優先度、クールダウン定義
    /// </summary>
    public class ReactionRule
    {
        public string Id { get; set; }
        public int Priority { get; set; }
        public float CooldownSeconds { get; set; }
        public OverlayPhase MinPhase { get; set; }
        public OverlayPhase MaxPhase { get; set; }
        public Func<ReactionContext, bool> Condition { get; set; }
        public Func<ReactionContext, ReactionPayload> PayloadFactory { get; set; }
        public bool OncePerKeyEvent { get; set; } = false;
        public string EventKey { get; set; } = null;
    }

    /// <summary>
    /// リアクションコンテキスト
    /// </summary>
    public class ReactionContext
    {
        public IOverlayEvent Event { get; set; }
        public OverlayState State { get; set; }
        public PlayerType PlayerType { get; set; }
    }

    /// <summary>
    /// リアクションペイロード（UIに適用するデータ）
    /// </summary>
    public class ReactionPayload
    {
        public string Text { get; set; }
        public GirlExpression Expression { get; set; }
        public RoomState RoomState { get; set; }
        public bool IsThought { get; set; } = false; // 心の声かどうか
        public float DisplayDuration { get; set; } = 3f; // 表示時間（秒）
    }

    /// <summary>
    /// リアクションルール定義
    /// </summary>
    public static class ReactionRules
    {
        /// <summary>
        /// 全ルールを取得
        /// </summary>
        public static ReactionRule[] GetAllRules()
        {
            return new ReactionRule[]
            {
                // Division A（Active） - 追加！
                new ReactionRule
                {
                    Id = "DivisionA_Active",
                    Priority = 15,
                    CooldownSeconds = 10f,
                    MinPhase = OverlayPhase.Active,
                    MaxPhase = OverlayPhase.Active,
                    OncePerKeyEvent = true,
                    EventKey = "DivisionA_First",
                    Condition = ctx => ctx.Event is DivisionEnteredEvt evt && evt.Division == Division.A,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("DivisionA_Active", ctx.PlayerType),
                        Expression = GirlExpression.Neutral,
                        RoomState = RoomState.CleanDay,
                        DisplayDuration = 3f
                    }
                },
                // Division B（Presence）
                new ReactionRule
                {
                    Id = "DivisionB_Presence",
                    Priority = 10,
                    CooldownSeconds = 15f,
                    MinPhase = OverlayPhase.Presence,
                    MaxPhase = OverlayPhase.Presence,
                    OncePerKeyEvent = true,
                    EventKey = "DivisionB_First",
                    Condition = ctx => ctx.Event is DivisionEnteredEvt evt && evt.Division == Division.B,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("DivisionB_Presence", ctx.PlayerType),
                        Expression = GirlExpression.Surprise,
                        RoomState = RoomState.NightGlow,
                        DisplayDuration = 2.5f
                    }
                },
                // Dark開始（Active）
                new ReactionRule
                {
                    Id = "DarkStart_Active",
                    Priority = 20,
                    CooldownSeconds = 15f,
                    MinPhase = OverlayPhase.Active,
                    MaxPhase = OverlayPhase.Active,
                    OncePerKeyEvent = true,
                    EventKey = "DarkStart_First",
                    Condition = ctx => ctx.Event is ModeChangedEvt evt && evt.Mode == GameMode.Dark,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("DarkStart_Active", ctx.PlayerType),
                        Expression = GirlExpression.Shock,
                        RoomState = RoomState.NightGlow,
                        DisplayDuration = 4f
                    }
                },
                // 「もうひとつ」成功（初回のみ）
                new ReactionRule
                {
                    Id = "MouhitotuSuccess_First",
                    Priority = 30,
                    CooldownSeconds = 12f,
                    MinPhase = OverlayPhase.Active,
                    MaxPhase = OverlayPhase.Active,
                    OncePerKeyEvent = true,
                    EventKey = "MouhitotuSuccess_First",
                    Condition = ctx => ctx.Event is MouhitotuResultEvt evt && evt.Success,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("MouhitotuSuccess_First", ctx.PlayerType),
                        Expression = GirlExpression.Smile,
                        RoomState = RoomState.CleanDay,
                        DisplayDuration = 4f
                    }
                },
                // Division C（強制再起動）
                new ReactionRule
                {
                    Id = "DivisionC_Active",
                    Priority = 25,
                    CooldownSeconds = 15f,
                    MinPhase = OverlayPhase.Active,
                    MaxPhase = OverlayPhase.Active,
                    OncePerKeyEvent = true,
                    EventKey = "DivisionC_First",
                    Condition = ctx => ctx.Event is DivisionEnteredEvt evt && evt.Division == Division.C,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("DivisionC_Active", ctx.PlayerType),
                        Expression = GirlExpression.Shock,
                        RoomState = RoomState.Glitchy,
                        DisplayDuration = 4f
                    }
                },
                // 3周目開始（Active）
                new ReactionRule
                {
                    Id = "ThirdStart_Active",
                    Priority = 20,
                    CooldownSeconds = 15f,
                    MinPhase = OverlayPhase.Active,
                    MaxPhase = OverlayPhase.Active,
                    OncePerKeyEvent = true,
                    EventKey = "ThirdStart_First",
                    Condition = ctx => ctx.Event is ModeChangedEvt evt && evt.Mode == GameMode.Third,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("ThirdStart_Active", ctx.PlayerType),
                        Expression = GirlExpression.Thinking,
                        RoomState = RoomState.CalmMorning,
                        DisplayDuration = 4f
                    }
                },
                // Division D（Quiet）
                new ReactionRule
                {
                    Id = "DivisionD_Quiet",
                    Priority = 5,
                    CooldownSeconds = 20f,
                    MinPhase = OverlayPhase.Quiet,
                    MaxPhase = OverlayPhase.Quiet,
                    OncePerKeyEvent = true,
                    EventKey = "DivisionD_First",
                    Condition = ctx => ctx.Event is DivisionEnteredEvt evt && evt.Division == Division.D,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("DivisionD_Quiet", ctx.PlayerType),
                        Expression = GirlExpression.Concern,
                        RoomState = RoomState.CalmMorning,
                        DisplayDuration = 3f,
                        IsThought = true
                    }
                },
                // Division E（Quiet）
                new ReactionRule
                {
                    Id = "DivisionE_Quiet",
                    Priority = 5,
                    CooldownSeconds = 20f,
                    MinPhase = OverlayPhase.Quiet,
                    MaxPhase = OverlayPhase.Quiet,
                    OncePerKeyEvent = true,
                    EventKey = "DivisionE_First",
                    Condition = ctx => ctx.Event is DivisionEnteredEvt evt && evt.Division == Division.E,
                    PayloadFactory = ctx => new ReactionPayload
                    {
                        Text = ReactionLines_JP.GetLine("DivisionE_Quiet", ctx.PlayerType),
                        Expression = GirlExpression.Concern,
                        RoomState = RoomState.CalmMorning,
                        DisplayDuration = 3f,
                        IsThought = true
                    }
                }
            };
        }
    }
}

