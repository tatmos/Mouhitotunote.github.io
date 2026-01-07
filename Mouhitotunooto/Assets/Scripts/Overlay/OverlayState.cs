using System;
using System.Collections.Generic;

namespace NovelGame.Overlay
{
    /// <summary>
    /// Overlay内部状態管理
    /// </summary>
    public class OverlayState
    {
        public GameMode CurrentMode { get; set; } = GameMode.Normal;
        public Division CurrentDivision { get; set; } = Division.None;
        public OverlayPhase CurrentPhase { get; set; } = OverlayPhase.Hidden;
        public PlayerType CurrentPlayerType { get; set; } = PlayerType.Unknown;

        // 発話頻度制御
        private DateTime lastSpokeTime = DateTime.MinValue;
        private readonly List<DateTime> recentSpokeTimes = new List<DateTime>();
        private readonly HashSet<string> oncePerKeyEventFlags = new HashSet<string>();

        // 設定
        public float MinCooldownSeconds { get; set; } = 10f;
        public int MaxPerMinute { get; set; } = 2;
        public float CooldownSeconds { get; set; } = 10f;

        /// <summary>
        /// 発話可能かチェック
        /// </summary>
        public bool CanSpeak()
        {
            // クールダウンチェック
            if ((DateTime.Now - lastSpokeTime).TotalSeconds < CooldownSeconds)
            {
                return false;
            }

            // 1分あたりの最大回数チェック
            recentSpokeTimes.RemoveAll(t => (DateTime.Now - t).TotalSeconds > 60f);
            if (recentSpokeTimes.Count >= MaxPerMinute)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 発話を記録
        /// </summary>
        public void RecordSpoke()
        {
            lastSpokeTime = DateTime.Now;
            recentSpokeTimes.Add(DateTime.Now);
        }

        /// <summary>
        /// 初回のみ発話可能かチェック
        /// </summary>
        public bool CanSpeakOnce(string eventKey)
        {
            if (oncePerKeyEventFlags.Contains(eventKey))
            {
                return false;
            }
            return CanSpeak();
        }

        /// <summary>
        /// 初回のみ発話を記録
        /// </summary>
        public void RecordSpokeOnce(string eventKey)
        {
            oncePerKeyEventFlags.Add(eventKey);
            RecordSpoke();
        }

        /// <summary>
        /// 状態をリセット
        /// </summary>
        public void Reset()
        {
            lastSpokeTime = DateTime.MinValue;
            recentSpokeTimes.Clear();
            oncePerKeyEventFlags.Clear();
            CurrentMode = GameMode.Normal;
            CurrentDivision = Division.None;
            CurrentPhase = OverlayPhase.Hidden;
            CurrentPlayerType = PlayerType.Unknown;
        }
    }
}

