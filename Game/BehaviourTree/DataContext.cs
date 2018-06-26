using ProtoBuf;
using System.Collections.Generic;
using System.Linq;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Simple data globalContext tool.
    /// </summary>
    [ProtoContract]
    public class DataContext {

        [ProtoMember(1)]
        private Dictionary<string, ValueField> internalData;

        public int Count {
            get {
                return internalData != null ? internalData.Count : 0;
            }
        }

        public DataContext() {
            this.internalData = new Dictionary<string, ValueField>();
        }

        public DataContext(Dictionary<string, ValueField> data) {
            this.internalData = data;
        }

        public IList<string> GetKeys() {
            return internalData.Keys.ToList();
        }

        public DataContext(IList<ValueField> data) {
            this.internalData = new Dictionary<string, ValueField>();
            if (data != null) {
                foreach (var value in data) {
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
                    internalData.Add(value.Name, value);
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

        public ValueField Get(string key) {
            ValueField val;
            return internalData.TryGetValue(key, out val) ? val : default(ValueField);
        }

        public object this[string key] {
            get {
                if (internalData.ContainsKey(key)) {
                    return internalData[key].Value;
                }
                return null;
            }
            set {
                ValueField val;
                if (internalData.TryGetValue(key, out val)) {
                    val.Value = value;
                    internalData[key] = val;
                    return;
                }
                internalData.Add(key, new ValueField(key, value));
                var thing = internalData[key];
                thing.Value = value;
                internalData[key] = thing;

            }
        }
    }
}
