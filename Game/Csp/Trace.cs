using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Playblack.Csp {
    /// <summary>
    /// A thing that traces the event flow of CSP stuff.
    /// </summary>
    public class Trace {
        private LinkedList<TraceEntry> traceEntries;

        private SignalProcessor activator;

        public Trace(SignalProcessor activator) {
            traceEntries = new LinkedList<TraceEntry>();
            this.activator = activator;
        }

        public void Add(GameObject callerObj, OutputFunc output, GameObject receiverObj, InputFunc input) {
            traceEntries.AddLast(new TraceEntry(callerObj, output, receiverObj, input));
        }

        /// <summary>
        /// Returns a string builder where each line is one execution in the execution flow.
        /// Use however you see fit.
        /// </summary>
        /// <returns></returns>
        public StringBuilder GenerateExecutionFlow() {
            var node = traceEntries.First;
            StringBuilder traceRecorder = new StringBuilder();
            traceRecorder.AppendLine("Event chain activated by: " + activator.name);
            while (node != null) {
                traceRecorder.AppendLine(
                    string.Format(
                        "{0}: {1} => {3}: {2}",
                        node.Value.callerObj.name,
                        node.Value.caller.Name,
                        node.Value.receiver.Name,
                        node.Value.receiverObj.name
                    )
                );
                node = node.Next;
            }

            return traceRecorder;
        }
    }

    internal struct TraceEntry {
        public readonly OutputFunc caller;
        public readonly InputFunc receiver;

        public GameObject callerObj;
        public GameObject receiverObj;

        public TraceEntry(GameObject callerObj, OutputFunc caller, GameObject receiverObj, InputFunc receiver) {
            this.caller = caller;
            this.receiver = receiver;

            this.callerObj = callerObj;
            this.receiverObj = receiverObj;
        }


    }
}
