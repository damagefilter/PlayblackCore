using System;

namespace Playblack.Csp {
    /// <summary>
    /// What is this?
    /// This attribute marks a method as input function.
    /// Each method that is decorated with this attribute must be public.
    /// It will be considered by the SignalProcessor.
    /// If a DisplayName is specified, this is what will be visible in the editor instead of the function name.
    /// Specify WithParameter = true if your method expects a parameter 
    /// The parameter must be string containing your information.
    /// It will be set by the level designer / mapper inside the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class InputFuncAttribute : System.Attribute {
        public string MethodName;
        public string DisplayName;

        public bool WithParameter;

        public InputFuncAttribute(string methodName) {
            this.MethodName = methodName;
            this.DisplayName = methodName;
        }
    }
}

