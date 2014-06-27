using System;
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

			symbols.DumpTable(Console.Out);
			this.offsets = offsets;
		}

		public void EncodeLiteral(byte aByte, object context)
		{
			symbols.Write(aByte, (OutputBitStream)context);
		}

		public void EncodeSubstring(int offset, int length, object context)
		{
			var outputBitStream = (OutputBitStream)context;
			symbols.Write(length, outputBitStream);

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
	}
}