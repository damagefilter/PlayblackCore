using System;
using System.IO;
using System.IO.Compression;

namespace Playblack.Savegame.Model {
    public static class ZipTools {
        /// <summary>
        /// Takes in an array of bytes and applies gzip compression to it.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="input">Input.</param>
        public static byte[] CompressBytes(byte[] input) {
            using (MemoryStream compressedDataStream = new MemoryStream()) {
                using (GZipStream zip = new GZipStream(compressedDataStream, CompressionMode.Compress)) {
                    zip.Write(input, 0, input.Length);
                    compressedDataStream.Position = 0;
                    return compressedDataStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Takes in an array of compressed byte data and decompresses it into its original state.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="input">Input.</param>
        public static byte[] DecompressBytes(byte[] input) {
            // First read back the zipped stuff
            using (MemoryStream ms = new MemoryStream(input)) {
                using (GZipStream zippedData = new GZipStream(ms, CompressionMode.Decompress)) {
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
            }
        }
    }
}

