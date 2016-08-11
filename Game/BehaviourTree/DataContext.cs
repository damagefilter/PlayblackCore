using ProtoBuf;
using System.Collections.Generic;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Simple data globalContext tool.
    /// </summary>
    [ProtoContract]
    public class DataContext {

        [ProtoMember(1)]
        private Dictionary<string, object> internalData;

        public DataContext() {
            this.internalData = new Dictionary<string, object>();
        }

        public DataContext(Dictionary<string, object> data) {
            this.internalData = data;
        }

        public DataContext(IList<ValueField> data) {
            this.internalData = new Dictionary<string, object>();
            if (data != null) {
                foreach (var value in data) {
                    if (value == null) {
                        UnityEngine.Debug.Log("context data value was null, skipping");
                        continue;
                    }
                    if (value.Name == null) {
                        // This might happen in composite tasks where there are x children prepared but only y are used from the sequencer
                        UnityEngine.Debug.LogWarning("context data value without name, skipping");
                        UnityEngine.Debug.LogWarning("Here's a value: " + value.UnityValue);
                        continue;
                    }
                    if (internalData.ContainsKey(value.Name)) {
                        UnityEngine.Debug.LogWarning("This DataContext already has a key names " + value.Name + ". The value is " + internalData[value.Name]);
                        continue;
                    }
                    internalData.Add(value.Name, value.Value);
                }
            }
        }

        public void Merge(DataContext other) {
            foreach (var kvp in other.internalData) {
                if (this.internalData.ContainsKey(kvp.Key)) {
                    continue;
                }
                this.internalData.Add(kvp.Key, kvp.Value);
            }
        }

        public object this[string key] {
            get {
                if (internalData.ContainsKey(key)) {
                    return internalData[key];
                }
                return null;
            }
            set {
                internalData[key] = value;
            }
        }
    }
}