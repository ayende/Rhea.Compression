using System.IO;
using System.Text;
using Rhea.Compression.Dictionary;
using Rhea.Compression.Huffman;

namespace Rhea.Compression
{
	public class CompressionHandler
	{
		private readonly byte[] _dictionary;
		private readonly HuffmanPacker _packer;
		private readonly SubstringPacker _substringPacker;

		public CompressionHandler(byte[] dictionary, HuffmanPacker packer)
		{
			_dictionary = dictionary;
			_packer = packer;
			_substringPacker = new SubstringPacker(_dictionary);
		}

		public int Compress(string input, Stream output)
		{
			return Compress(Encoding.UTF8.GetBytes(input), output);
		}

		public int Compress(byte[] input, Stream output)
		{
			using (var outputBitStream = new OutputBitStream(output, leaveOpen: true))
			{
				_substringPacker.Pack(input, _packer, outputBitStream);
				return outputBitStream.Length / 8;
			}
		}

		public byte[] Decompress(Stream compressed)
		{
			using (var intputBitStream = new InputBitStream(compressed, leaveOpen: true))
			{
				var unpacker = new SubstringUnpacker(_dictionary);
				_packer.Unpack(intputBitStream, unpacker);
				return unpacker.UncompressedData();
			}
		}
	}
}