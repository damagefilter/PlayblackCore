namespace Playblack.Csp {

    public class SimpleInputFunc : InputFunc {
        private readonly SimpleSignal callback;

        public SimpleInputFunc(string name, SimpleSignal callback) : base(name) {
            this.callback = callback;
        }

        #region InputFunc implementation

        public override void Invoke(string param) {
            // param is unused here
            this.callback();
        }

        public override bool HasParameter() {
            return false;
        }

        #endregion InputFunc implementation
    }
}
