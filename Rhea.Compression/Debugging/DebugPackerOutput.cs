using System.IO;
using System.Text;
using Rhea.Compression.Dictionary;

namespace Rhea.Compression.Debugging
{
	public class DebugPackerOutput : IPackerOutput
    {
        readonly byte[] buffer = new byte[128];
        private int bufPos;
        public void EncodeLiteral(byte aByte, object context)
        {
            if (bufPos <= buffer.Length)
            {
                buffer[bufPos++] = aByte;
                return;
            }
            var textWriter = (TextWriter)context;
            FlushBuffer(textWriter);
        }

        public void EncodeSubstring(int offset, int length, object context)
        {
            var textWriter = (TextWriter)context;
            FlushBuffer(textWriter);
            textWriter.Write("<" + offset + "," + length + ">");
        }

        private void FlushBuffer(TextWriter textWriter)
        {
            if (bufPos <= 0)
                return;

            textWriter.Write(Encoding.UTF8.GetString(buffer, 0, bufPos));
            bufPos = 0;
        }

        public void EndEncoding(object context)
        {
            var textWriter = (TextWriter)context;
            FlushBuffer(textWriter);
        }
    }
}