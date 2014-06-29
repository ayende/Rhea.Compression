using System.IO;
using System.Text;
using Rhea.Compression.Dictionary;

namespace Rhea.Compression.Debugging
{
	public class DebugUnpackerOutput
	{
		private readonly TextReader _data;
		private readonly SubstringUnpacker _unpacker;
		private readonly StringBuilder _builder = new StringBuilder();
		public DebugUnpackerOutput(TextReader data, SubstringUnpacker unpacker)
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
}