using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Playblack.Csp {
    /// <summary>
    /// A thing that traces the event flow of CSP stuff.
    /// </summary>
    public class Trace {

        private static Type formatterType;
        
        
        private LinkedList<TraceEntry> traceEntries;

        private SignalProcessor activator;

        /// <summary>
        /// Flag to determine if this Trace has expired.
        /// When this returns true, you need to create a new Trace!
        /// </summary>
        public bool HasReturnedToCaller {
            get;
            set;
        }

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
        public StringBuilder GetTraceAsStringBuilder() {
            var node = traceEntries.First;
            ITraceFormatter formatter;
            // use custom trace formatter if it was set
            if (formatterType != null) {
                formatter = (ITraceFormatter)Activator.CreateInstance(formatterType);
            }
            else {
                formatter = new DefaultTraceFormatter();
            }
            
            formatter.ProvideActivator(activator);
            while (node != null) {
                formatter.Append(node.Value);
                node = node.Next;
            }

            return formatter.Get();
        }

        /// <summary>
        /// You can specify a custom trace formatter type here.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void BindTraceFormatterType<T>() where T : ITraceFormatter {
            formatterType = typeof(T);
        }
    }

    public interface ITraceFormatter {

        void ProvideActivator(SignalProcessor activator);
        /// <summary>
        /// Add the given TraceEntry to the formatters data set.
        /// </summary>
        /// <param name="data"></param>
        void Append(TraceEntry data);

        /// <summary>
        /// Return a StringBuilder containing the formatted trace information.
        /// </summary>
        /// <returns></returns>
        StringBuilder Get();
    }

    internal class DefaultTraceFormatter : ITraceFormatter {

        private StringBuilder traceRecorder;
        public DefaultTraceFormatter() {
            traceRecorder = new StringBuilder();
        }
        
        public void ProvideActivator(SignalProcessor activator) {
            traceRecorder.AppendLine("Event Chain activated by " + activator.gameObject.name);
        }

        public void Append(TraceEntry data) {
            traceRecorder.AppendLine(
                string.Format(
                    "Out: {0}.{1} => In: {3}.{2}",
                    data.callerObj.name,
                    data.caller.Name,
                    data.receiver.Name,
                    data.receiverObj.name
                )
            );
        }

        public StringBuilder Get() {
            return traceRecorder;
        }
    }

    public struct TraceEntry {
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
