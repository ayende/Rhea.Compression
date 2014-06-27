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
		readonly ThreadLocal<Stack<bool>> _path = new ThreadLocal<Stack<bool>>(() => new Stack<bool>());

		public HuffmanTable(Dictionary<int, HuffmanNode> leaves, HuffmanNode root)
		{
			_leaves = leaves;
			_root = root;
		}

		public static HuffmanTable Load(BinaryReader reader)
		{
			var leaves = new Dictionary<int, HuffmanNode>();
			var node = HuffmanNode.Load(reader, leaves);

			return new HuffmanTable(leaves, node);
		}

		public void Save(BinaryWriter writer)
		{
			_root.Save(writer);
		}

		public void Write(int symbol, OutputBitStream output)
		{
			_leaves[symbol].TraverseUp(_path.Value);
			var path = "";
			while (_path.Value.Count > 0)
			{
				var pop = _path.Value.Pop();
				path += pop ? "1" : "0";
				output.Write(pop);
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
			node.TraverseUp(_path.Value);

			if (node.IsBranch)
				writer.Write(" {0} '{2}' x {1:4} = ", tag, node.Freq, (char)node.Symbol);
			else
				writer.Write(" {1} Freq {0} = ", node.Freq, tag);

			while (_path.Value.Count > 0)
			{
				writer.Write(_path.Value.Pop() ? "1" : "0");
			}

			writer.WriteLine();

			if (node.IsBranch == false)
				return;
			DumpNode(node.Left, "left", i+1, writer);
			DumpNode(node.Right, "right", i + 1, writer);
		}
	}
}