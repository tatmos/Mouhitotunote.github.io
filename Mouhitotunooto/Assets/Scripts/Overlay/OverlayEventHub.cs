using System;
using System.Collections.Generic;

namespace NovelGame.Overlay
{
    /// <summary>
    /// Overlayイベントバス（Subscribe/Publish）
    /// </summary>
    public static class OverlayEventHub
    {
        private static readonly Dictionary<Type, List<Action<IOverlayEvent>>> subscribers = new Dictionary<Type, List<Action<IOverlayEvent>>>();

        /// <summary>
        /// イベントを購読
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IOverlayEvent
        {
            Type eventType = typeof(T);
            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new List<Action<IOverlayEvent>>();
            }

            subscribers[eventType].Add(evt => handler((T)evt));
        }

        /// <summary>
        /// イベントを配信
        /// </summary>
        public static void Publish<T>(T evt) where T : IOverlayEvent
        {
            Type eventType = typeof(T);
            if (subscribers.ContainsKey(eventType))
            {
                foreach (var handler in subscribers[eventType])
                {
                    try
                    {
                        handler(evt);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[OverlayEventHub] イベントハンドラでエラーが発生しました: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// 全購読をクリア（テスト用）
        /// </summary>
        public static void Clear()
        {
            subscribers.Clear();
        }
    }
}

