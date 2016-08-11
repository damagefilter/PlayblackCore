// FIXME: Find a library that can compress byte arrays that is not Gzip because
// it requires a native zip library (zlib) which unity doesn't ship.
namespace Playblack.Savegame.Model {

    public static class ZipTools {

        /// <summary>
        /// Takes in an array of bytes and applies gzip compression to it.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="input">Input.</param>
        public static byte[] CompressBytes(byte[] input) {
            return input;
            /*using (MemoryStream compressedDataStream = new MemoryStream()) {
                using (var zip = new GZipOutputStream(compressedDataStream)) {
                    zip.Write(input, 0, input.Length);
                    compressedDataStream.Position = 0;
                    var bytes = compressedDataStream.ToArray();
                    UnityEngine.Debug.Log("Writing " + bytes.Length + " bytes.");
                    return bytes;
                }
            }*/
        }

        /// <summary>
        /// Takes in an array of compressed byte data and decompresses it into its original state.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="input">Input.</param>
        public static byte[] DecompressBytes(byte[] input) {
            return input;
            /*UnityEngine.Debug.Log("Reading " + input.Length + " bytes.");
            // First read back the zipped stuff
            using (MemoryStream ms = new MemoryStream(input)) {
                using (var zippedData = new GZipInputStream(ms)) {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream targetStream = new MemoryStream()) {
                        int count = 0;
                        do {
                            count = zippedData.Read(buffer, 0, size);
                            if (count > 0) {
                                targetStream.Write(buffer, 0, count);
                            }
                        } while (count > 0);
                        return targetStream.ToArray();
                    }
                }
            }*/
        }
    }
}