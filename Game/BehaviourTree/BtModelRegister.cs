using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Playblack.BehaviourTree.Model.Core;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Playblack.BehaviourTree {
    public class BtModelRegister : ScriptableObject {

        #region Static

        private static BtModelRegister _instance;

        public static BtModelRegister Instance {
            get {
                if (_instance == null) {
                    #if UNITY_EDITOR
                    if (!File.Exists("Assets/Resources/Playblack/BtModelData.asset")) {
                        if (!Directory.Exists("Assets/Resources/Playblack")) { // Create first or unity goes full retard
                            Directory.CreateDirectory("Assets/Resources/Playblack");
                        }
                        var reg = ScriptableObject.CreateInstance<BtModelRegister>();
                        AssetDatabase.CreateAsset(reg, "Assets/Resources/Playblack/BtModelData.asset");
                        AssetDatabase.SaveAssets();
                    }
                    #endif
                    _instance = Resources.Load<BtModelRegister>("Playblack/BtModelData");
                }
                return _instance;
            }
        }

        #endregion

        [SerializeField]
        private List<ModelSet> modelSets = new List<ModelSet>();

        public void RegisterModelSet(string namespaceRoot, string category) {
            var ms = new ModelSet(namespaceRoot, category);
            if (!modelSets.Contains(ms)) {
                modelSets.Add(ms);
            }
        }

        public List<ModelSet> GetModelSets() {
            return this.modelSets;
        }
    }

    [Serializable]
    public struct ModelSet {

        /// <summary>
        /// All models in this namespace will be taken into consideration,
        /// given that they are not abstract or interfaces and implement,
        /// in one way or the other, ModelTask.
        /// </summary>
        public readonly string namespaceRoot;

        /// <summary>
        /// The category of these models.
        /// In the sequencer selection window this is used to group models
        /// into logical units for easier navigation.
        /// </summary>
        public readonly string category;

        public ModelSet(string nsRoot, string cat) {
            this.namespaceRoot = nsRoot;
            this.category = cat;
        }

        /// <summary>
        /// Returns all valid ModelTask types in the namespace.
        /// This will not include interfaces, abstracts and classes not implementing ModelTask in a way.
        /// </summary>
        /// <returns>The model types.</returns>
        public Type[] GetModelTypes() {
            var tmp = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.FullName.StartsWith(namespaceRoot) && t.IsSubclassOf(typeof(ModelTask)));
            return tmp.ToArray();
        }

        /// <summary>
        /// Work around to get hold of types in CSharp-Assembly outside the editor dll
        /// </summary>
        /// <returns>The type.</returns>
        /// <param name="typeName">Type name.</param>
        protected static Type GetModelType(string typeName) {
            var type = Type.GetType(typeName);
            if (type != null) {
                return type;
            }
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
                type = a.GetType(typeName);
                if (type != null) {
                    return type;
                }
            }
            return null;
        }

        public override bool Equals(object obj) {
            if (typeof(ModelSet) != obj.GetType()) {
                return false;
            }
            var other = (ModelSet)obj;
            return other.namespaceRoot == this.namespaceRoot && other.category == this.category;
        }

        public override int GetHashCode() {
            return namespaceRoot.GetHashCode() + category.GetHashCode();
        }
    }
}

