using System;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;

namespace Playblack.Savegame.Model {
    /// <summary>
    /// A block of data representing a full game object.
    /// This is a collection of ComponentDataBlock objects mapped by theor component name.
    /// </summary>
    [ProtoContract]
    public class GameObjectDataBlock {
        #region data
        [ProtoMember(100)]
        private string uuid;
        public string UUID {
            get {
                return uuid;
            }
        }

        [ProtoMember(200)]
        private string assetBundle;
        public string AssetBundle {
            get {
                return assetBundle;
            }
        }

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

        #endregion

        public GameObjectDataBlock(string uuid, string assetBundle, string assetPath) {
            this.uuid = uuid;
            this.assetBundle = assetBundle;
            this.assetPath = assetPath;
            this.loadAsset = !string.IsNullOrEmpty(assetBundle) && !string.IsNullOrEmpty(assetPath);
            this.componentList = new List<ComponentDataBlock>();
        }

        public void AddComponentData(ComponentDataBlock block) {
            this.componentList.Add(block);
        }
    }
}