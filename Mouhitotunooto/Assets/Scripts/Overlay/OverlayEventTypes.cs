using System;

namespace NovelGame.Overlay
{
    /// <summary>
    /// Overlayイベントの共通インターフェイス
    /// </summary>
    public interface IOverlayEvent { }

    /// <summary>
    /// ゲームモード変更イベント
    /// </summary>
    public class ModeChangedEvt : IOverlayEvent
    {
        public GameMode Mode { get; }
        public ModeChangedEvt(GameMode mode) { Mode = mode; }
    }

    /// <summary>
    /// Division進入イベント
    /// </summary>
    public class DivisionEnteredEvt : IOverlayEvent
    {
        public Division Division { get; }
        public DivisionEnteredEvt(Division division) { Division = division; }
    }

    /// <summary>
    /// シナリオ開始イベント
    /// </summary>
    public class ScenarioStartedEvt : IOverlayEvent
    {
        public int ScenarioId { get; }
        public ScenarioStartedEvt(int scenarioId) { ScenarioId = scenarioId; }
    }

    /// <summary>
    /// シナリオクリアイベント
    /// </summary>
    public class ScenarioClearedEvt : IOverlayEvent
    {
        public int ScenarioId { get; }
        public bool GotMouhitotu { get; }
        public ScenarioClearedEvt(int scenarioId, bool gotMouhitotu)
        {
            ScenarioId = scenarioId;
            GotMouhitotu = gotMouhitotu;
        }
    }

    /// <summary>
    /// 「もうひとつ」結果イベント
    /// </summary>
    public class MouhitotuResultEvt : IOverlayEvent
    {
        public int ScenarioId { get; }
        public bool Success { get; }
        public string Reason { get; }
        public MouhitotuResultEvt(int scenarioId, bool success, string reason)
        {
            ScenarioId = scenarioId;
            Success = success;
            Reason = reason;
        }
    }

    /// <summary>
    /// 選択肢選択イベント
    /// </summary>
    public class ChoiceSelectedEvt : IOverlayEvent
    {
        public int ScenarioId { get; }
        public string ChoiceId { get; }
        public ChoiceSelectedEvt(int scenarioId, string choiceId)
        {
            ScenarioId = scenarioId;
            ChoiceId = choiceId;
        }
    }

    /// <summary>
    /// シナリオ選択画面に戻るイベント
    /// </summary>
    public class ReturnToScenarioSelectEvt : IOverlayEvent
    {
        public ReturnToScenarioSelectEvt() { }
    }

    /// <summary>
    /// ゲームモード
    /// </summary>
    public enum GameMode
    {
        Normal,
        Dark,
        Third
    }

    /// <summary>
    /// Division
    /// </summary>
    public enum Division
    {
        None,
        A,
        B,
        C,
        D,
        E
    }

    /// <summary>
    /// プレイヤータイプ
    /// </summary>
    public enum PlayerType
    {
        Unknown,
        FastClicker,
        CarefulReader,
        Explorer
    }

    /// <summary>
    /// 実況者表情
    /// </summary>
    public enum GirlExpression
    {
        Neutral,
        Smile,
        Laugh,
        Surprise,
        Thinking,
        Annoyed,
        Shock,
        Concern
    }

    /// <summary>
    /// 部屋状態
    /// </summary>
    public enum RoomState
    {
        CleanDay,
        NightGlow,
        Messy,
        Glitchy,
        CalmMorning
    }

    /// <summary>
    /// Overlayフェーズ
    /// </summary>
    public enum OverlayPhase
    {
        Hidden,
        Presence,
        Active,
        Quiet
    }
}

