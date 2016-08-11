namespace Playblack.EventSystem.Events {

    public abstract class Event<TImplementor> : IEvent where TImplementor : IEvent {

        /// <summary>
        /// Call this this hook on the EventDispatcher.
        /// </summary>
        public Event<TImplementor> Call() {
            EventDispatcher.Instance.Call<TImplementor>((IEvent)this);
            return this;
        }
    }
}