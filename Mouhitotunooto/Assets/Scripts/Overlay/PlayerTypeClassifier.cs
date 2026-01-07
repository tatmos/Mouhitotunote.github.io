namespace NovelGame.Overlay
{
    /// <summary>
    /// TelemetryからPlayerTypeを判定
    /// </summary>
    public class PlayerTypeClassifier
    {
        private readonly PlayerTelemetry telemetry;

        public PlayerTypeClassifier(PlayerTelemetry telemetry)
        {
            this.telemetry = telemetry;
        }

        /// <summary>
        /// 現在のプレイヤータイプを判定
        /// </summary>
        public PlayerType Classify()
        {
            if (telemetry == null) return PlayerType.Unknown;

            float avgInterval = telemetry.GetAverageClickInterval();
            float skipRate = telemetry.GetSkipRate();
            int logViews = telemetry.GetLogViewCount();

            // FastClicker: 平均クリック間隔が短い、またはスキップ率が高い
            if (avgInterval > 0 && avgInterval < telemetry.FastClickThreshold)
            {
                return PlayerType.FastClicker;
            }

            // CarefulReader: 平均クリック間隔が長い、ログ閲覧が多い
            if (avgInterval > telemetry.CarefulReadThreshold || logViews > 5)
            {
                return PlayerType.CarefulReader;
            }

            // Explorer: ログ閲覧が非常に多い、または中間的な行動パターン
            if (logViews > 10)
            {
                return PlayerType.Explorer;
            }

            // デフォルトはUnknown
            return PlayerType.Unknown;
        }
    }
}

