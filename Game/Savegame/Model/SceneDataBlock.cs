using ProtoBuf;
using System.Collections.Generic;

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

        #endregion Data Block

        #region IDataBlock implementation

        public string DataId {
            get {
                return sceneName;
            }
        }

        #endregion IDataBlock implementation

        public SceneDataBlock() {
            // Protobuf ctor
        }

        public SceneDataBlock(string sceneName) {
            this.sceneName = sceneName;
            this.objects = new List<GameObjectDataBlock>();
        }

        public void AddGameObjectData(GameObjectDataBlock data) {
            this.objects.Add(data);
        }
    }
}