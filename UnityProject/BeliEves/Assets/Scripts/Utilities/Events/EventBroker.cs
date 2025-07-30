using System;
using System.Collections.Generic;
using Events.EventsLayout;
using JetBrains.Annotations;
using UnityEngine;
using Utilities.Events.EventsLayout;


namespace Events {
    public class ChannelException : Exception {
        public ChannelException(string message) : base(message) {}
    }
    public static class EventBroker{
        private static Dictionary<string, BasicEventChannel> _events = new Dictionary<string, BasicEventChannel>();
        private static CallbacksHandler _callbacksHandler;

        static EventBroker() {
            try {
                var eventSystemObject = UnityEngine.GameObject.Find("EventSystem");
                _callbacksHandler = eventSystemObject.GetComponent<CallbacksHandler>();
            }
            catch (Exception e) {
            }
        }

        public static void ResetEventSystem() {
            _events = new Dictionary<string, BasicEventChannel>();
            //_callbacksHandler = null;
            //_callbacksHandler = UnityEngine.GameObject.Find("EventSystem").GetComponent<CallbacksHandler>();
        }

        public static void SetCallBackHandler() {
            _callbacksHandler = UnityEngine.GameObject.Find("EventSystem").GetComponent<CallbacksHandler>();
            foreach (var e in _events.Values) {
                e.AddCallBackHandler(_callbacksHandler.HandleCallback);
            }
        }
        public static void AddEventChannel(string eventName, BasicEventChannel channel) {
            var _channel = channel;
            if (!_events.TryAdd(eventName, channel))
                throw new ChannelException("Channel already exists");
            if(_callbacksHandler!=null)_channel.AddCallBackHandler(_callbacksHandler.HandleCallback);
        }

        public static BasicEventChannel TryToAddEventChannel(string eventName, BasicEventChannel channel) {
            BasicEventChannel eventChannel = channel;
            try {
                AddEventChannel(eventName, channel);
            }
            catch (ChannelException) {
                eventChannel = GetEventChannel(eventName);
            }
            return eventChannel;
        }
        
        public static BasicEventChannel SubsToEventChannel(string eventName,[NotNull] Action callback) {
            if(!_events.TryGetValue(eventName, out BasicEventChannel channel))
                throw new System.Exception("Channel does not exist");
            channel.Subscribe(callback);
            return channel;
        }

        public static void UnsubsFromEventChannel(string eventName,[NotNull] Action callback) {
            if(!_events.TryGetValue(eventName, out BasicEventChannel channel))
                throw new System.Exception("Channel does not exist");
            channel.Unsubscribe(callback);
        }

        public static BasicEventChannel GetEventChannel(string eventName) {            
            if(!_events.TryGetValue(eventName, out BasicEventChannel channel))
                throw new System.Exception("Channel "+ eventName +" does not exist");
            return channel;
        }
    }
}
