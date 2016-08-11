using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Playblack.Savegame.Model {

    /// <summary>
    /// Class of static methods helping with serializing and deserializing from and to byte arrays.
    /// </summary>
    public static class DataSerializer {

        #region Serialize

        /// <summary>
        /// Serializes any simple object into a byte array.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static byte[] SerializeSimpleObject(object o) {
            if (o == null) {
                return null;
            }
            using (MemoryStream stream = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, o);
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes a ProtoContract object into a byte array.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static byte[] SerializeProtoObject(object o) {
            using (MemoryStream ms = new MemoryStream()) {
                ProtoBuf.Serializer.NonGeneric.Serialize(ms, o);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serializes a ProtoContract into a byte array.
        /// This is type-safe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static byte[] SerializeProtoObject<T>(T o) {
            using (MemoryStream ms = new MemoryStream()) {
                ProtoBuf.Serializer.Serialize<T>(ms, o);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        #endregion Serialize

        #region Deserialize

        /// <summary>
        /// Deserializes a system byte array (non-protobuf bytes) back into an object.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static object DeserializeSimpleObject(byte[] b) {
            if (b == null || b.Length == 0) {
                return null;
            }
            using (MemoryStream stream = new MemoryStream(b)) {
                stream.Seek(0, SeekOrigin.Begin);
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Deserializes a system byte array (non-protobuf bytes) back into an object.
        /// This does automatic casting. It will not provide any performance gains but it's
        /// here for good measure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b"></param>
        /// <returns></returns>
        public static T DeserializeSimpleObject<T>(byte[] b) {
            if (b == null || b.Length == 0) {
                return default(T);
            }
            using (MemoryStream stream = new MemoryStream(b)) {
                stream.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Deserializes a byte array into protobuf object.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object DeserializeProtoObject(byte[] b, Type type) {
            using (MemoryStream ms = new MemoryStream(b)) {
                ms.Position = 0;
                var obj = ProtoBuf.Serializer.NonGeneric.Deserialize(type, ms);
                return obj;
            }
        }

        /// <summary>
        /// Deserializes a byte array into protobuf object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b"></param>
        /// <returns></returns>
        public static T DeserializeProtoObject<T>(byte[] b) {
            using (MemoryStream ms = new MemoryStream(b)) {
                ms.Position = 0;
                var obj = ProtoBuf.Serializer.Deserialize<T>(ms);
                return obj;
            }
        }

        #endregion Deserialize
    }
}