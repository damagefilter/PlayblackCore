using System;
using System.Collections.Generic;

namespace Playblack.EventSystem {

    // Defines the callback for events
    public delegate void Callback<in T>(T hook);

    internal interface IEventContainer {
        void Call(IEvent e);
        void Add(Delegate d);
        void Remove(Delegate d);
    }

    internal class EventContainer<T> : IEventContainer where T : IEvent{
        private Callback<T> events;
        public void Call(IEvent e) {
            if (events != null) {
                events((T)e);
            }
        }

        public void Add(Delegate d) {
            events += d as Callback<T>;
        }

        public void Remove(Delegate d) {
            events -= d as Callback<T>;
        }
    }

    /// <summary>
    /// High-performance event pump.
    /// It's a event broadcasting service that will pump large amounts of events
    /// in a very fast fashion through your code. Events are called in no specific order.
    /// There are no targeted events.
    /// </summary>
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
        private Dictionary<Type, IEventContainer> registrants;

        private EventDispatcher() {
            registrants = new Dictionary<Type, IEventContainer>();
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
                registrants.Add(paramType, new EventContainer<T>());
            }
            var handles = registrants[paramType];
            handles.Add(handler);
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
            handlers.Remove(handler);
            registrants[paramType] = handlers;
        }

        /// <summary>
        /// Call the specified event.
        /// This will cause all registrants to be called that
        /// have typeof(T) events in their signature and the event data is passed along.
        /// If there is no registrant for the given event then nothing will happen.
        /// </summary>
        /// <param name="e">Event to raise.</param>
        public void Call<T>(IEvent e) where T : IEvent {
            IEventContainer d;
            if (registrants.TryGetValue(typeof(T), out d)) {
                d.Call(e);
            }
        }

        #endregion API
    }
}
