using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Owin.Extensions
{
    public static class StreamExtensions
    {
        private const int CR = '\r';
        private const int LF = '\n';

        /// <summary>
        ///     Reads a complete stream
        /// </summary>
        /// <returns>The contents of the stream</returns>
        public static string ReadAll(this Stream stream, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            using (var sr = new StreamReader(stream, encoding)) {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        ///     Reads all bytes from a stream and returns a byte array
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        public static byte[] ReadAllBytes(this Stream stream) {
            // todo: use a list of byte[] to reduce the copies?

            const int defaultBufferSize = 4096;
            var buffer = new byte[defaultBufferSize];
            int read = 0;

            int numBytes;
            while ((numBytes = stream.Read(buffer, read, buffer.Length - read)) > 0) {
                read += numBytes;

                // If we've reached the end of our buffer, check to see if there's any more information
                if (read == buffer.Length) {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1) {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just read, and continue
                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            var result = new byte[read];
            Array.Copy(buffer, result, read);
            return result;
        }

        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count) {
            return Task<int>.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, offset, count, null);
        }

        public static string ReadHttpHeaderLine(this Stream stream) {
            var buffer = new List<byte>();
            while (true) {
                int next = stream.ReadByte();
                if (next == CR) {
                    if (stream.ReadByte() == LF) {
                        return Encoding.UTF8.GetString(buffer.ToArray());
                    }
                    return null;
                }
                if (next == -1) {
                    //end of stream, not a valid header line.
                    return null;
                }
                buffer.Add((byte)next);
            }
        }

        /// <summary>
        ///     Reads a stream line by line
        /// </summary>
        /// <returns>The read lines</returns>
        public static List<string> ReadLines(this Stream stream, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            var lines = new List<string>();
            using (var sr = new StreamReader(stream, encoding)) {
                while (sr.Peek() >= 0) {
                    lines.Add(sr.ReadLine());
                }
            }
            return lines;
        }

        public static void Reset(this MemoryStream stream) {
            if (stream == null) {
                return;
            }
            byte[] buffer = stream.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            stream.Position = 0;
            stream.SetLength(0);
        }

        /// <summary>
        ///     Sets the stream cursor to the beginning of the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The stream</returns>
        public static Stream SeekToBegin(this Stream stream) {
            if (stream.CanSeek == false) {
                throw new InvalidOperationException("Stream does not support seeking.");
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        ///     Sets the stream cursor to the end of the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The stream</returns>
        public static Stream SeekToEnd(this Stream stream) {
            if (stream.CanSeek == false) {
                throw new InvalidOperationException("Stream does not support seeking.");
            }
            stream.Seek(0, SeekOrigin.End);
            return stream;
        }

        public static void Write(this Stream stream, string data, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            byte[] buffer = encoding.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        ///     Writes all passed bytes to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The byte array / buffer.</param>
        public static void Write(this Stream stream, byte[] data) {
            stream.Write(data, 0, data.Length);
        }

        public static Task WriteAsync(this Stream stream, string data, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            byte[] buffer = encoding.GetBytes(data);
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static Task WriteAsync(this Stream stream, byte[] data) {
            return stream.WriteAsync(data, 0, data.Length);
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count) {
            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, offset, count, null)
                       .ContinueWith(ä => stream.Flush());
        }
    }
}