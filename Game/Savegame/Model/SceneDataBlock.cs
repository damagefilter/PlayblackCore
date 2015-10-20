using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Playblack.Savegame.Model {
    [ProtoContract]
    public class SceneDataBlock {
        [ProtoMember(100)]
        private string sceneName;
        [ProtoMember(200)]
        private List<GameObjectDataBlock> objects;
        public List<GameObjectDataBlock> SceneObjects {
            get {
                return objects;
            }
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
