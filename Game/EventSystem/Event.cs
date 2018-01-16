namespace Playblack.EventSystem.Events {

    public abstract class Event<TImplementor> : IEvent where TImplementor : IEvent {

        /// <summary>
        /// Call this this hook on the EventDispatcher.
        /// </summary>
        public void Call() {
            EventDispatcher.Instance.Call<TImplementor>(this);
        }

        public static void Register(Callback<TImplementor> handler) {
            EventDispatcher.Instance.Register(handler);
        }

        public static void Unregister(Callback<TImplementor> handler) {
            EventDispatcher.Instance.Unregister(handler);
        }
    }
}
