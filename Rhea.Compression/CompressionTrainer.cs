using System;
using System.Linq;
using System.Text;
using Rhea.Compression.Dictionary;
using Rhea.Compression.Huffman;

namespace Rhea.Compression
{
	public class CompressionTrainer
	{
		private readonly DictionaryOptimizer _dictionaryOptimizer = new DictionaryOptimizer();

		public void TrainOn(string doc)
		{
			_dictionaryOptimizer.Add(doc);
		}

		public void TrainOn(byte[] doc)
		{
			_dictionaryOptimizer.Add(doc);
		}

		public CompressionHandler CreateHandler(int desiredLength)
		{
			var dictionary = _dictionaryOptimizer.Optimize(desiredLength);
			var training = new SubstringPacker(dictionary);

			var huffmanTableTrainer = new HuffmanTableTrainer();
			foreach (var document in _dictionaryOptimizer.Documents)
			{
				training.Pack(document, huffmanTableTrainer, null);
			}

			var packer = huffmanTableTrainer.GeneratePacker();

			return new CompressionHandler(dictionary, packer);
		}

		public class HuffmanTableTrainer : IPackerOutput
		{
			readonly int[] symbols = new int[256 /* 0 - 255 byte literals */+ 256 /* lengths */+ 1 /* EOF marker */];

			private readonly int[][] offsets = new[]
			{
				new int[16],
				new int[16],
				new int[16],
				new int[16]
			};

			public void EncodeLiteral(byte aByte, object context)
			{
				symbols[aByte]++;
			}

			public void EncodeSubstring(int offset, int length, object context)
			{
				if (length < 1 || length > 255)
					throw new ArgumentException("invalid length (1 - 255)", "length");
				offset = -offset;
				if(offset > 64* 1024)
					throw new ArgumentException("invalid offset (0 - 64Kb)", "offset");

				symbols[256 + length]++;

				for (int i = 0; i < offsets.Length; i++)
				{
				    var offsetNibble = (offset >> (i*4)) & 0xf;
				    offsets[i][offsetNibble]++;
				}
			}

			public void EndEncoding(object context)
			{
				symbols[symbols.Length - 1]++; // eof marker
			}

			public HuffmanPacker GeneratePacker()
			{
				var sybolsTableGenerator = new HuffmanTableGenerator();
				for (int i = 0; i < symbols.Length; i++)
				{
					sybolsTableGenerator.Add(i, symbols[i]);
				}
				var offsetTableGenerators = new HuffmanTableGenerator[offsets.Length];
				for (int i = 0; i < offsets.Length; i++)
				{
					offsetTableGenerators[i] = new HuffmanTableGenerator();
					for (int j = 0; j < offsets[i].Length; j++)
					{
						offsetTableGenerators[i].Add(j, offsets[i][j]);
					}	
				}

				return new HuffmanPacker(sybolsTableGenerator.Build(), offsetTableGenerators.Select(x => x.Build()).ToArray());
			}
		}
	}
}