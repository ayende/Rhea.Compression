using System;
using System.IO;
using System.Linq;
using Rhea.Compression.Dictionary;
using Rhea.Compression.Huffman;

namespace Rhea.Compression
{
	public class HuffmanPacker : IPackerOutput
	{
		private readonly HuffmanTable symbols;
		private readonly HuffmanTable[] offsets;

		private const int EofMarker = 512;

		public HuffmanPacker(HuffmanTable symbols, HuffmanTable[] offsets)
		{
			this.symbols = symbols;
			this.offsets = offsets;
		}

		public void EncodeLiteral(byte aByte, object context)
		{
			symbols.Write(aByte, (OutputBitStream)context);
		}

		public void EncodeSubstring(int offset, int length, object context)
		{
			var outputBitStream = (OutputBitStream)context;
			symbols.Write(length + 256, outputBitStream);

			offset = -offset;
			for (int i = 0; i < offsets.Length; i++)
			{
				var offsetNibble = ((offset >> (i*4)) & 0xf);
				offsets[i].Write(offsetNibble, outputBitStream);
			}
		}

		public void EndEncoding(object context)
		{
			var outputBitStream = (OutputBitStream)context;
			symbols.Write(EofMarker, outputBitStream);
			outputBitStream.Flush();
		}

		public void Unpack(InputBitStream intputBitStream, SubstringUnpacker unpacker)
		{
			var offsetReaders = offsets.Select(x=>x.Read(intputBitStream).GetEnumerator()).ToArray();
			bool hasEof = false;
			foreach (var symbol in symbols.Read(intputBitStream))
			{
				if (symbol == EofMarker)
				{
					hasEof = true;
					break;
				}
				if (symbol < 256)
				{
					unpacker.EncodeLiteral((byte)symbol);
					continue;
				}
				int len = symbol - 256;
				int offset = 0;
				for (int i = 0; i < offsetReaders.Length; i++)
				{
					var moveNext = offsetReaders[i].MoveNext();
					if (moveNext == false)
						throw new InvalidDataException("Expected offset, but got end of stream");
					offset |= offsetReaders[i].Current << (i*4);
				}
				offset = -offset;
				unpacker.EncodeSubstring(offset, len);
			}
			if(hasEof==false)
				throw new InvalidDataException("End of stream before EOF marker");
		}
	}
}