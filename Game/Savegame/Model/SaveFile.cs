using System;
using ProtoBuf;
using System.Collections.Generic;

namespace Playblack.Savegame.Model {
    [ProtoContract]
    public class SaveFile : IDataBlock {
        [ProtoMember(100)]
        private string saveName;

        [ProtoMember(200)]
        private Dictionary<string, IDataBlock> saveData;

        #region IDataBlock implementation
        public string DataId {
            get {
                return saveName;
            }
        }

        #endregion

        public SaveFile(string saveName) {
            this.saveName = saveName;
            this.saveData = new Dictionary<string, IDataBlock>();
        }

        public void Add(IDataBlock data) {
            if (saveData.ContainsKey(data.DataId)) {
                // Override if exists
                saveData[data.DataId] = data;
            }
            else {
                // new one otherwise
                saveData.Add(data.DataId, data);
            }
        }

        public IDataBlock Get(string name) {
            if (this.saveData.ContainsKey(name)) {
                return saveData[name];
            }
            return null;
        }
    }
}

