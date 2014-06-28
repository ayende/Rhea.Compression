using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Rhea.Compression.Huffman
{
	public class HuffmanTable
	{
		private readonly Dictionary<int, HuffmanNode> _leaves;
		private readonly HuffmanNode _root;

		public HuffmanTable(Dictionary<int, HuffmanNode> leaves, HuffmanNode root)
		{
			_leaves = leaves;
			_root = root;
		}

		public static HuffmanTable Load(BinaryReader reader)
		{
			var leaves = new Dictionary<int, HuffmanNode>();
			var node = HuffmanNode.Load(reader, leaves);

			foreach (var huffmanNode in leaves.Values)
			{
				huffmanNode.SetupBitPattern();
			}

			return new HuffmanTable(leaves, node);
		}

		public void Save(BinaryWriter writer)
		{
			_root.Save(writer);
		}

		public void Write(int symbol, OutputBitStream output)
		{
			var huffmanNode = _leaves[symbol];
			Console.WriteLine("'{0}' {3} {1} {2}", (char)symbol, huffmanNode.BitPattern, huffmanNode.Bits.Count, symbol);
			foreach (var bit in huffmanNode.Bits)
			{
				output.Write(bit);
			}
		}

		public IEnumerable<int> Read(InputBitStream input)
		{
			var curr = _root;
			while (input.MoveNext())
			{
				curr = input.Current ? curr.Right : curr.Left;
				if (curr.IsBranch)
					continue;
				yield return curr.Symbol;
				curr = _root;
			}
		}

		public void DumpTable(TextWriter writer)
		{
			DumpNode(_root, "root", 0, writer);
		}

		private void DumpNode(HuffmanNode node,string tag, int i, TextWriter writer)
		{
			for (int j = 0; j < i; j++)
			{
				writer.Write("    ");
			}
			writer.Write('-');

			if (node.IsBranch)
				writer.Write(" {0} '{2}' x {1:4} = ", tag, node.Freq, (char)node.Symbol);
			else
				writer.Write(" {1} Freq {0} = ", node.Freq, tag);

			if (node.IsBranch == false)
			{
				foreach (var bit in node.Bits)
				{
					writer.Write(bit ? "1" : "0");
				}
			}

			writer.WriteLine();

			if (node.IsBranch == false)
				return;
			DumpNode(node.Left, "left", i+1, writer);
			DumpNode(node.Right, "right", i + 1, writer);
		}
	}
}