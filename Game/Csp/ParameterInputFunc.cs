using System;

namespace Playblack.Csp {
	public class ParameterInputFunc : InputFunc {
		private ParameterSignal callback;
		public ParameterInputFunc(string name, ParameterSignal callback) : base(name) {
			this.callback = callback;
		}

		#region InputFunc implementation
		public override void Invoke (string param) {
			this.callback(param);
		}
		public override bool HasParameter() {
			return true;
		}
		#endregion
	}
}

