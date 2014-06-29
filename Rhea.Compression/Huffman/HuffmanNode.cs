using System;
using System.Collections.Generic;
using System.IO;

namespace Rhea.Compression.Huffman
{
	public class HuffmanNode : IComparable<HuffmanNode>
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

		protected bool Equals(HuffmanNode other)
		{
			if (Symbol == -1)
				return ReferenceEquals(this, other);
			return Symbol == other.Symbol;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((HuffmanNode) obj);
		}

		public override int GetHashCode()
		{
			if (Symbol == -1)
				// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
				return base.GetHashCode();
			return Symbol;
		}

		public int CompareTo(HuffmanNode other)
		{
			int i = Freq - other.Freq;
			if (i == 0)
				i = Symbol - other.Symbol;
			return i*-1;
		}

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
				Write7BitEncodedInt(writer, Symbol);
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
				var huffmanNode = new HuffmanNode(Read7BitEncodedInt(reader), -2/* we don't actually need this, so we don't save it*/);
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

		protected static void Write7BitEncodedInt(BinaryWriter writer, int value)
		{
			uint num = (uint)value;

			while (num >= 128U)
			{
				writer.Write((byte)(num | 128U));
				num >>= 7;
			}

			writer.Write((byte)num);
		}

		protected static int Read7BitEncodedInt(BinaryReader reader)
		{
			// some names have been changed to protect the readability  
			int returnValue = 0;
			int bitIndex = 0;

			while (bitIndex != 35)
			{
				byte currentByte = reader.ReadByte();
				returnValue |= ((int)currentByte & (int)sbyte.MaxValue) << bitIndex;
				bitIndex += 7;

				if (((int)currentByte & 128) == 0)
					return returnValue;
			}

			throw new FormatException("Invalid format for 7 bit encoded string");
		}  
	}
}