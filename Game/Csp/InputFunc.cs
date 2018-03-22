namespace Playblack.Csp {

    /// <summary>
    /// Represents an input function.
    /// </summary>
    public abstract class InputFunc {
        private readonly string name;

        public string Name {
            get {
                return name;
            }
        }

        public InputFunc(string name) {
            this.name = name;
        }

        public abstract void Invoke(string param);

        public abstract bool HasParameter();
    }
}
