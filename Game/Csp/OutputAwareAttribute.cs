using System;

namespace Playblack {
    /// <summary>
    /// What is this?
    /// This is an attribute to inform the signal processor that a component is capable of firing outputs.
    /// You can specify a list of named outputs like OnKill, OnFire, OnHit and such.
    /// This is meta information and makes the signal processor aware of possible outputs.
    /// 
    /// As a side note: You can fire outputs by calling SendMessage() in your MonoBehaviour.
    /// If a signal processor is attached it will receive the message and process it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class OutputAwareAttribute : System.Attribute {
        public string[] OutputGetter;

        public OutputAwareAttribute(params string[] outputGetter) {
            this.OutputGetter = outputGetter;
        }
    }
}

