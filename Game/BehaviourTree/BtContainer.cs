using System;
using System.Collections.Generic;
using System.IO;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Composite;
using UnityEngine;
using Playblack.BehaviourTree.Execution.Core;

namespace Playblack.BehaviourTree {
    /// <summary>
    /// Contains a prepared behaviour tree and can generate instances from it
    /// for any given operator
    /// </summary>
    [Serializable]
    public class BtContainer : ISerializationCallbackReceiver {
        private UnityBtModel rootModel;

        [SerializeField]
        private byte[] serializedData;

        public UnityBtModel Root {
            get {
                return this.rootModel;
            }
        }

        public BtContainer() {
            this.rootModel = UnityBtModel.NewInstance(null, new UnityBtModel());
            this.rootModel.ModelClassName = typeof(ModelSequence).ToString();
        }

        public void OnAfterDeserialize() {
            using (var ms = new MemoryStream(serializedData)) {
                ms.Position = 0;
                this.rootModel = ProtoBuf.Serializer.Deserialize<UnityBtModel>(ms);
            }
        }

        public void OnBeforeSerialize() {
            using (var ms = new MemoryStream()) {
                ProtoBuf.Serializer.Serialize<UnityBtModel>(ms, rootModel);
                ms.Position = 0;
                serializedData = ms.ToArray();
            }
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

