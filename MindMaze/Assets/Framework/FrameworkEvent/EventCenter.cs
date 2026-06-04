using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.FrameworkEvent
{
    public static class EventCenter
    {
        private static Dictionary<string, Action<object>> _eventMap = new Dictionary<string, Action<object>>();
        
        #if UNITY_EDITOR
        private static Dictionary<string, List<ListenerInfo>> _listenerTracker = new Dictionary<string, List<ListenerInfo>>();
        #endif
        
        #if UNITY_EDITOR
        private struct ListenerInfo : IEquatable<ListenerInfo>
        {
            public readonly Action<object> Callback;
            public readonly string TypeName;
            public readonly string MethodName;

            public ListenerInfo(Action<object> callback)
            {
                Callback = callback;
                TypeName = callback.Target?.GetType().Name ?? "Unknown";
                MethodName = callback.Method.Name;
            }

            public override string ToString()
            {
                return $"{TypeName}.{MethodName}";
            }

            public bool Equals(ListenerInfo other)
            {
                return Equals(Callback, other.Callback) && TypeName == other.TypeName && MethodName == other.MethodName;
            }

            public override bool Equals(object obj)
            {
                return obj is ListenerInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Callback, TypeName, MethodName);
            }
        }
        #endif

        /// <summary>
        /// 添加事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">回调函数</param>
        public static void AddEventListener(string eventName, Action<object> callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
            {
                Debug.LogWarning($"EventCenter: Attempting to add listener with null event name or callback.");
                return;
            }

            _eventMap.TryAdd(eventName, null);

            // 添加到事件处理链
            _eventMap[eventName] += callback;

            #if UNITY_EDITOR
            // 编辑器模式下，同时添加到追踪器
            if (!_listenerTracker.ContainsKey(eventName))
            {
                _listenerTracker[eventName] = new List<ListenerInfo>();
            }

            var listenerInfo = new ListenerInfo(callback);
            if (!_listenerTracker[eventName].Contains(listenerInfo)) // 防止重复添加
            {
                _listenerTracker[eventName].Add(listenerInfo);
            }
            #endif
        }

        /// <summary>
        /// 移除事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">回调函数</param>
        public static void RemoveEventListener(string eventName, Action<object> callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
            {
                Debug.LogWarning($"EventCenter: Attempting to remove listener with null event name or callback.");
                return;
            }

            if (_eventMap.ContainsKey(eventName))
            {
                _eventMap[eventName] -= callback;

                if (_eventMap[eventName] == null)
                {
                    _eventMap.Remove(eventName);
                }
            }

            #if UNITY_EDITOR
            // 编辑器模式下，同时从追踪器移除
            if (_listenerTracker.ContainsKey(eventName))
            {
                var listeners = _listenerTracker[eventName];
                listeners.RemoveAll(l => l.Callback == callback);

                if (listeners.Count == 0)
                {
                    _listenerTracker.Remove(eventName);
                }
            }
            #endif
        }

        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventData">事件数据</param>
        public static void DispatchEvent(string eventName, object eventData = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning($"EventCenter: Attempting to dispatch event with null name.");
                return;
            }

            if (_eventMap.ContainsKey(eventName) && _eventMap[eventName] != null)
            {
                try
                {
                    _eventMap[eventName]?.Invoke(eventData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Exception in event handler for '{eventName}': {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                // 可选：如果事件没有监听者，可以打印信息（调试用）
                // Debug.Log($"Event '{eventName}' dispatched but has no listeners.");
            }
        }

        // --- 编辑器专用方法 ---
        #if UNITY_EDITOR
        public static Dictionary<string, List<string>> GetListenerTrackerForEditor()
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var kvp in _listenerTracker)
            {
                result[kvp.Key] = kvp.Value.Select(li => li.ToString()).ToList();
            }
            return result;
        }

        public static List<string> GetListenersForEventForEditor(string eventName)
        {
            if (_listenerTracker.ContainsKey(eventName))
            {
                return _listenerTracker[eventName].Select(li => li.ToString()).ToList();
            }
            return new List<string>();
        }

        public static List<string> GetRegisteredEventsForEditor()
        {
            return _listenerTracker.Keys.ToList();
        }
        #endif
    }
}