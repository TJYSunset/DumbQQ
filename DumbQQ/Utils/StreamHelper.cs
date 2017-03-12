using System.IO;

namespace DumbQQ.Utils
{
    internal static class StreamHelper
    {
        public static byte[] ToBytes(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                stream.Close();
                return ms.ToArray();
            }
        }
    }
}