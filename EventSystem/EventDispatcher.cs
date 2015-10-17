using System;
using System.Collections.Generic;

namespace Playblack.EventSystem {
    // Defines the callback for events
    public delegate void Callback<T>(T hook);

    public class EventDispatcher {
        private static EventDispatcher instance;
        public static EventDispatcher Instance {
            get {
                if (instance == null) {
                    instance = new EventDispatcher();
                }
                return instance;
            }
        }

        /// <summary>
        /// The registrants.
        /// Maps event type to event handlers.
        /// Max efficiency when calling hooks / events, whatever
        /// </summary>
        private Dictionary<Type, Delegate> registrants;

        private EventDispatcher() {
            registrants = new Dictionary<Type, Delegate>();
        }

        #region API

        /// <summary>
        /// Clears all registered listener callbacks
        /// </summary>
        public void ClearAll() {
            registrants.Clear();
        }
        /// <summary>
        /// Register the specified handler.
        /// The event type on the handler defines when it is called.
        /// If it has an UIShowEvent as argument, it will be called when an UIShowEvent 
        /// is passed to the Call() method.
        /// </summary>
        /// <param name="handler">Handler.</param>
        public void Register<T>(Callback<T> handler) where T : IEvent {
            var paramType = typeof(T);

            if (!registrants.ContainsKey(paramType)) {
                registrants.Add(paramType, null);
            }
            var handles = registrants[paramType];
            if (handles != null) {
                handles = Delegate.Combine(handles, handler);
            }
            else {
                handles = handler;
            }
            registrants[paramType] = handles;
        }

        /// <summary>
        /// Unregister the specified handler from the system
        /// </summary>
        /// <param name="handler">Handler.</param>
        public void Unregister<T>(Callback<T> handler) where T : IEvent {
            var paramType = typeof(T);

            if (!registrants.ContainsKey(paramType)) {
                return;
            }
            var handlers = registrants[paramType];
            handlers = Delegate.Remove(handlers, handler);
            registrants[paramType] = handlers;
        }


        /// <summary>
        /// Call the specified event.
        /// This will cause all registrants to be called that
        /// have typeof(e) events in their signature.
        /// If there is no registrant for the given event then nothing will happen.
        /// </summary>
        /// <param name="e">Event to raise.</param>
        public void Call<T>(IEvent e) where T : IEvent {
            Delegate d;
            if (registrants.TryGetValue(e.GetType(), out d)) {
                Callback<T> callback = d as Callback<T>;
                if (callback != null) {
                    callback((T)e);
                }
            }
        }
        #endregion
    }
}
