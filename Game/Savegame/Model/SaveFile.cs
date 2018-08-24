using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Playblack.Savegame.Model {
    public class SaveFile : IDisposable {
        private string saveName;

        public string SaveName {
            get {
                return saveName;
            }
        }

        private FileStream readStream;

        /// <summary>
        /// Offset to apply in stream reads when reading data blocks after index header
        /// </summary>
        private int offsetLength;

        private List<SaveFileIndexPointer> dataIdIndex;

        private Dictionary<string, SaveFileEntry> memorySaveData;

        public SaveFile(string saveName) {
            this.saveName = saveName;
            this.memorySaveData = new Dictionary<string, SaveFileEntry>();
            dataIdIndex = new List<SaveFileIndexPointer>();
        }

        public void Add(IDataBlock data) {
            memorySaveData[data.DataId] = new SaveFileEntry(data);
        }

        public IDataBlock Get(string name) {
            var idx = dataIdIndex.FirstOrDefault(x => x.dataId == name);
            if (idx == null) {
                return null;
            }

            return Get(idx);
        }

        private IDataBlock Get(SaveFileIndexPointer pointer) {
            var saveFileEntry = FindAndLoad(pointer);
            return saveFileEntry != null ? saveFileEntry.DataBlock : null;
        }

        private SaveFileEntry FindAndLoad(SaveFileIndexPointer idx) {
            readStream.Position = idx.startOffset + offsetLength;
            byte[] buffer = new byte[idx.chunkLength];
            readStream.Read(buffer, 0, idx.chunkLength);
            Debug.Log($"READ: data id: {idx.dataId} from offset {idx.startOffset + offsetLength} with length: {idx.chunkLength}");
            return buffer.Length == 0 ? null : DataSerializer.DeserializeProtoObject<SaveFileEntry>(buffer);
        }

        public void Save(string saveFile) {
            dataIdIndex.Clear();
            byte[] mainRawData;
            using (MemoryStream mainDataStream = new MemoryStream()) {
                foreach (var sfe in memorySaveData.Values) {
                    var buffer = DataSerializer.SerializeProtoObject(sfe);
                    long startOffset = mainDataStream.Length;
                    mainDataStream.Write(buffer, 0, buffer.Length);
                    dataIdIndex.Add(new SaveFileIndexPointer(sfe.dataId, startOffset, buffer.Length));
                    Debug.Log($"WRITE {sfe.dataId} startoffset: {startOffset} - length: {buffer.Length}");
                }

                mainDataStream.Position = 0;
                mainRawData = mainDataStream.ToArray();
            }

            byte[] indexRawData = DataSerializer.SerializeProtoObject(dataIdIndex);
            int indexBufferLength = indexRawData.Length;
            var intBuffer = DataSerializer.SerializeInteger(indexBufferLength);
            var offsetLength = DataSerializer.SerializeInteger(indexBufferLength + 8); // buffer + 8 bytes index length & this offset info as 2 ints

            using (MemoryStream finalStream = new MemoryStream()) {
                finalStream.Write(intBuffer, 0, intBuffer.Length);
                finalStream.Write(offsetLength, 0, offsetLength.Length);
                finalStream.Write(indexRawData, 0, indexRawData.Length);
                finalStream.Write(mainRawData, 0, mainRawData.Length);
                finalStream.Position = 0;
                if (readStream == null) {
                    // means while reading indices the file didn't exist.
                    readStream = new FileStream(saveFile, FileMode.OpenOrCreate);
                }
                readStream.Position = 0;
                var buffer = finalStream.ToArray();
                readStream.Write(buffer, 0, buffer.Length);
                readStream.SetLength(buffer.Length);
            }
        }

        /// <summary>
        /// Read index header.
        /// Contains positions in save game chunk of specific data blocks.
        /// For streaming stuff.
        /// </summary>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        /// <exception cref="SaveGameException"></exception>
        public bool ReadIndices(string saveFile) {
            if (!File.Exists(saveFile)) {
                return false;
            }

            if (readStream != null) {
                readStream.Dispose();
            }

            try {
                readStream = new FileStream(saveFile, FileMode.Open);
            }
            catch (Exception e) {
                throw new SaveGameException("Failed opening file for streaming save game data", e);
            }
            byte[] intBuffer = new byte[sizeof(int)];
            readStream.Read(intBuffer, 0, intBuffer.Length);
            int indexLength = DataSerializer.DeserializeInteger(intBuffer);

            byte[] offsetBuffer = new byte[sizeof(int)];
            readStream.Read(offsetBuffer, 0, offsetBuffer.Length);
            offsetLength = DataSerializer.DeserializeInteger(offsetBuffer);

            Debug.Log($"Found index length is {indexLength}");
            Debug.Log($"Found offset length is {offsetLength}");
            Debug.Log($"stream position is now {readStream.Position}");
            byte[] indexData = new byte[indexLength];
            int bytesRead = readStream.Read(indexData, 0, indexData.Length);
            Debug.Log($"Read {bytesRead} bytes to represent index");
            var tmpIndex = DataSerializer.DeserializeProtoObject<List<SaveFileIndexPointer>>(indexData);
            if (tmpIndex != null) {
                dataIdIndex = tmpIndex;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the whole save game file into memory.
        /// Use this to prepare the file for saving. Otherwise there will be data loss.
        /// </summary>
        /// <returns></returns>
        public bool PrepareForSave(string saveFile) {
            if (readStream == null) {
                if (!this.ReadIndices(saveFile)) {
                    return false;
                }
            }

            foreach (var indexPointer in dataIdIndex) {
                var data = this.Get(indexPointer.dataId);
                if (data != null) {
                    this.Add(data);
                }
            }

            return true;
        }

        public void Dispose() {
            if (readStream != null) {
                readStream.Dispose();
            }
        }
    }
}
