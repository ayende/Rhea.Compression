using System.Collections.Generic;
using System.IO;

namespace Rhea.Compression.Huffman
{
	public class HuffmanNode
	{
		public readonly int Symbol;
		public readonly int Freq;

		public HuffmanNode Parent;
		public HuffmanNode Right;
		public HuffmanNode Left;
		public Stack<bool> Bits; 

		public HuffmanNode(int symbol, int freq)
		{
			Symbol = symbol;
			Freq = freq;
		}

		public bool IsBranch { get { return Symbol == -1; } }
		public int BitPattern { get; set; }

		public void SetupBitPattern()
		{
			BitPattern = 0;
			Bits = new Stack<bool>();
			
			var curr = this;
			while (curr.Parent != null)
			{
				var parent = curr.Parent;
				BitPattern <<= 1;
				var isRight = ReferenceEquals(parent.Right, curr);
				Bits.Push(isRight);
				BitPattern |= isRight ? 1 : 0;
				curr = parent;
			}
			
		}

		public override string ToString()
		{
			if(Symbol ==-1)
				return "Branch Freq: " + Freq;
			return "Leaf: " + (char) Symbol + " Freq: " + Freq;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(IsBranch);
			if (!IsBranch)
			{
				BinaryWriterExtensions.Write7BitEncodedInt(writer, Symbol);
				return;
			}
			Left.Save(writer);
			Right.Save(writer);
		}

		public static HuffmanNode Load(BinaryReader reader, Dictionary<int, HuffmanNode> leaves)
		{
			var branch = reader.ReadBoolean();
			if (branch == false)
			{
				var huffmanNode = new HuffmanNode(BinaryWriterExtensions.Read7BitEncodedInt(reader), -2/* we don't actually need this, so we don't save it*/);
				leaves.Add(huffmanNode.Symbol, huffmanNode);
				return huffmanNode;
			}
			var left = Load(reader, leaves);
			var right = Load(reader,leaves);

			var parent = new HuffmanNode(-1, -2)
			{
				Left = left,
				Right = right
			};
			left.Parent = parent;
			right.Parent = parent;
			return parent;
		}
	}
}