using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Playblack.Savegame.Model {

    /// <summary>
    /// A block of data representing a full game object.
    /// This is a collection of ComponentDataBlock objects mapped by theor component name.
    /// </summary>
    [ProtoContract]
    public class GameObjectDataBlock : IDataBlock {

        #region data

        [ProtoMember(100)]
        private string uuid;

        public string UUID {
            get {
                return uuid;
            }
        }

        [ProtoMember(150)]
        private string sceneName;

        public string SceneName {
            get {
                return sceneName;
            }
        }

        /// <summary>
        /// The assetbundle name to request
        /// </summary>
        [ProtoMember(200)]
        private string assetBundle;

        public string AssetBundle {
            get {
                return assetBundle;
            }
        }

        /// <summary>
        /// In case of assetbundle assets, this is the path within the bundle.
        /// In case of prefabs, this is the path within a Resources folder.
        /// </summary>
        [ProtoMember(300)]
        private string assetPath;

        public string AssetPath {
            get {
                return assetPath;
            }
        }

        [ProtoMember(400)]
        private bool loadAsset;

        public bool MustLoadAsset {
            get {
                return loadAsset;
            }
        }

        [ProtoMember(500)]
        private List<ComponentDataBlock> componentList;

        public List<ComponentDataBlock> ComponentList {
            get {
                return componentList;
            }
        }

        #endregion data

        #region IDataBlock implementation

        public string DataId {
            get {
                return uuid;
            }
        }

        #endregion IDataBlock implementation

        public GameObjectDataBlock() {
            // protobuf ctor
        }

        public GameObjectDataBlock(string uuid, string sceneName, string assetBundle, string assetPath) {
            this.uuid = uuid;
            this.sceneName = sceneName;
            this.assetBundle = assetBundle;
            this.assetPath = assetPath;
            // We can have only assetPath, that will load from Resources.
            // Or we can have assetBundle and assetPath, that will load from an assetbundle
            this.loadAsset = !string.IsNullOrEmpty(assetPath) || (!string.IsNullOrEmpty(assetBundle) && !string.IsNullOrEmpty(assetPath));
            this.componentList = new List<ComponentDataBlock>();
        }

        public void AddComponentData(ComponentDataBlock block) {
            this.componentList.Add(block);
        }
    }
}