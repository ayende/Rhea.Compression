using System;
using System.Collections.Generic;

namespace Rhea.Compression.Huffman
{
	public class HuffmanNode : IComparable<HuffmanNode>
	{
		public readonly int Symbol;
		public readonly int Freq;

		public HuffmanNode Parent;
		public HuffmanNode Right;
		public HuffmanNode Left;

		public HuffmanNode(int symbol, int freq)
		{
			Symbol = symbol;
			Freq = freq;
		}

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

		public void TraverseUp(Stack<bool> path)
		{
			var curr = this;
			while (curr.Parent != null)
			{
				var parent = curr.Parent;
				path.Push(ReferenceEquals(parent.Right, curr));
				curr = parent;
			}
		}

		public override string ToString()
		{
			if(Symbol ==-1)
				return "Branch Freq: " + Freq;
			return "Leaf: " + (char) Symbol + " Freq: " + Freq;
		}
	}
}