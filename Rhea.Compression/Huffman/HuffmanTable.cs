using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Rhea.Compression.Huffman
{
	public class HuffmanTable
	{
		private readonly Dictionary<int, HuffmanNode> _leaves;
		private readonly HuffmanNode _root;
		readonly Stack<bool> _path = new Stack<bool>();

		public HuffmanTable(Dictionary<int, HuffmanNode> leaves, HuffmanNode root)
		{
			_leaves = leaves;
			_root = root;
		}

		public void Write(int symbol, OutputBitStream output)
		{
			_leaves[symbol].TraverseUp(_path);
			var path = "";
			while (_path.Count > 0)
			{
				var pop = _path.Pop();
				path += pop ? "1" : "0";
				output.Write(pop);
			}

			Console.WriteLine("[" + (char)symbol +"] = " + path);
		}

		public IEnumerable<int> Read(InputBitStream input)
		{
			var curr = _root;
			while (input.MoveNext())
			{
				curr = input.Current ? curr.Right : curr.Left;
				if (curr.Right != null)
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
			node.TraverseUp(_path);

			if (node.Symbol != -1)
				writer.Write(" {0} '{2}' x {1:4} = ", tag, node.Freq, (char)node.Symbol);
			else
				writer.Write(" {1} Freq {0} = ", node.Freq, tag);

			while (_path.Count > 0)
			{
				writer.Write(_path.Pop() ? "1" : "0");
			}

			writer.WriteLine();

			if (node.Right == null)
				return;
			DumpNode(node.Left, "left", i+1, writer);
			DumpNode(node.Right, "right", i + 1, writer);
		}
	}
}