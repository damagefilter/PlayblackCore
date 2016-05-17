using Playblack.BehaviourTree;
using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Composite;
using Playblack.Savegame.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Playblack.Sequencer {

    [Serializable]
    public class SequenceContainer : ISerializationCallbackReceiver {

        /// <summary>
        /// The actual data.
        /// Since unity cannot serialize deep tree structures,
        /// we let protobuf handle that, see serializedModelTree.
        /// </summary>
        private UnityBtModel rootModel;

        [SerializeField]
        private byte[] serializedModelTree;

        [SerializeField]
        private ExecutionType executionType;

        public ExecutionType TypeOfExecution {
            get {
                return executionType;
            }
            set {
                executionType = value;
            }
        }

        public UnityBtModel RootModel {
            get {
                return rootModel;
            }
        }

        public SequenceContainer() {
            this.rootModel = UnityBtModel.NewInstance(null, new UnityBtModel(), typeof(ModelSequence).ToString());
        }

        public void OnAfterDeserialize() {
            using (var ms = new MemoryStream(serializedModelTree)) {
                ms.Position = 0;
                this.rootModel = DataSerializer.DeserializeProtoObject<UnityBtModel>(ms.ToArray());
            }
        }

        public void OnBeforeSerialize() {
            this.serializedModelTree = DataSerializer.SerializeProtoObject(this.rootModel);
        }

        public IBTExecutor GetExecutor(DataContext context, UnityEngine.Object actor) {
            context["actor"] = actor;

            var root = rootModel.Model;
            RecursiveLoadModelTree(rootModel, root);
            // TODO: Fetch an implementation from a factory
            return new CachingBtExecutor(root, context);
        }

        private void RecursiveLoadModelTree(UnityBtModel current, ModelTask root) {
            var childQueue = new Dictionary<ModelTask, UnityBtModel>();
            if (current.children != null && current.children.Count > 0) {
                foreach (var btModelChild in current.children) {
                    if (btModelChild == null || btModelChild.ModelClassName == null) {
                        continue;
                    }
                    var modelTask = btModelChild.Model;
                    root.Children.Add(modelTask);

                    if (btModelChild.children.Count > 0) {
                        childQueue.Add(modelTask, btModelChild);
                    }
                }
                foreach (var kvp in childQueue) {
                    RecursiveLoadModelTree(kvp.Value, kvp.Key);
                }
            }
        }
    }
}
