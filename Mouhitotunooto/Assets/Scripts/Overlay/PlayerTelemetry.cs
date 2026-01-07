using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame.Overlay
{
    /// <summary>
    /// プレイヤー行動計測
    /// </summary>
    public class PlayerTelemetry
    {
        private readonly List<float> clickIntervals = new List<float>();
        private int skipCount = 0;
        private int logViewCount = 0;
        private DateTime lastClickTime = DateTime.MinValue;

        // 設定
        public float FastClickThreshold { get; set; } = 0.5f; // 0.5秒以下でクリック = 速い
        public float CarefulReadThreshold { get; set; } = 3.0f; // 3秒以上でクリック = 慎重

        /// <summary>
        /// クリックを記録
        /// </summary>
        public void RecordClick()
        {
            DateTime now = DateTime.Now;
            if (lastClickTime != DateTime.MinValue)
            {
                float interval = (float)(now - lastClickTime).TotalSeconds;
                clickIntervals.Add(interval);
                
                // 最新100件のみ保持
                if (clickIntervals.Count > 100)
                {
                    clickIntervals.RemoveAt(0);
                }
            }
            lastClickTime = now;
        }

        /// <summary>
        /// スキップを記録
        /// </summary>
        public void RecordSkip()
        {
            skipCount++;
        }

        /// <summary>
        /// ログ閲覧を記録
        /// </summary>
        public void RecordLogView()
        {
            logViewCount++;
        }

        /// <summary>
        /// 平均クリック間隔を取得
        /// </summary>
        public float GetAverageClickInterval()
        {
            if (clickIntervals.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (var interval in clickIntervals)
            {
                sum += interval;
            }
            return sum / clickIntervals.Count;
        }

        /// <summary>
        /// スキップ率を取得
        /// </summary>
        public float GetSkipRate()
        {
            int totalActions = clickIntervals.Count + skipCount;
            if (totalActions == 0) return 0f;
            return (float)skipCount / totalActions;
        }

        /// <summary>
        /// ログ閲覧頻度を取得
        /// </summary>
        public int GetLogViewCount()
        {
            return logViewCount;
        }

        /// <summary>
        /// データをリセット
        /// </summary>
        public void Reset()
        {
            clickIntervals.Clear();
            skipCount = 0;
            logViewCount = 0;
            lastClickTime = DateTime.MinValue;
        }
    }
}

