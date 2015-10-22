using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Playblack.Savegame.Model {
    [ProtoContract]
    public class SceneDataBlock : IDataBlock {
        #region Data Block
        [ProtoMember(100)]
        private string sceneName;
        public string SceneName {
            get {
                return sceneName;
            }
        }

        [ProtoMember(200)]
        private List<GameObjectDataBlock> objects;
        public List<GameObjectDataBlock> SceneObjects {
            get {
                return objects;
            }
        }
        #endregion

        #region IDataBlock implementation

        public string DataId {
            get {
                return sceneName;
            }
        }

        #endregion

        public SceneDataBlock(string sceneName) {
            this.sceneName = sceneName;
            this.objects = new List<GameObjectDataBlock>();
        }

        public void AddGameObjectData(GameObjectDataBlock data) {
            this.objects.Add(data);
        }
    }
}
