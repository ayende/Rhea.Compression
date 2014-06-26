using System.IO;
using System.Text;

namespace Rhea.Compression
{
    public class DebugUnpackerOoutput
    {
        private readonly TextReader _data;
        private readonly SubstringUnpacker _unpacker;
        private readonly StringBuilder _builder = new StringBuilder();
        public DebugUnpackerOoutput(TextReader data, SubstringUnpacker unpacker)
        {
            _data = data;
            _unpacker = unpacker;
        }

        public void Unpack()
        {
            while (true)
            {
                var read = _data.Read();
                if (read == -1)
                    return;

                var ch = (char)read;
                if (ch == '<')
                {
                    var offset = ReadInteger();
                    var len = ReadInteger();

                    _unpacker.EncodeSubstring(offset, len);
                    continue;
                }
                _unpacker.EncodeLiteral((byte) ch);
            }
        }

        public int ReadInteger()
        {
            _builder.Length = 0;
            while (true)
            {
                var read = _data.Read();
                if (read == -1)
                    throw new InvalidDataException("Could not read integer properly");

                var ch = (char)read;
                if (ch == ',' || ch == '>')
                {
                    return int.Parse(_builder.ToString());
                }
                _builder.Append(ch);
            }
        }
    }
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
            textWriter.WriteLine();
        }
    }
}