using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhea.Compression.Huffman
{
	public class HuffmanTableGenerator
	{
		private readonly Dictionary<int, HuffmanNode> _leaves = new Dictionary<int, HuffmanNode>();

		public void Add(int symbol, int freq)
		{
			var node = new HuffmanNode(symbol, freq);
			_leaves.Add(symbol, node);
		}

		public HuffmanTable Build()
		{
			var leaves = new Queue<HuffmanNode>(_leaves.Values.OrderBy(node => node.Freq == 0 ? 1 : node.Freq));
			var branches = new Queue<HuffmanNode>();

			while (leaves.Count + branches.Count > 1)
			{
				Queue<HuffmanNode> q1 = null, q2 = null;
				int candidateWeight = int.MaxValue;

				if (leaves.Count > 0 && branches.Count > 0)
				{
					var weight = leaves.Peek().Freq + branches.Peek().Freq;
					if (weight < candidateWeight)
					{
						candidateWeight = weight;
						q1 = leaves;
						q2 = branches;
					}
				}
				if (leaves.Count > 1)
				{
					var weight = leaves.Peek().Freq + leaves.Peek().Freq;
					if (weight < candidateWeight)
					{
						candidateWeight = weight;
						q1 = q2 = leaves;
					}
				}
				if (branches.Count > 1)
				{
					var weight = branches.Peek().Freq + branches.Peek().Freq;
					if (weight< candidateWeight)
					{
						candidateWeight = weight;
						q1 = q2 = branches;
					}
				}

				System.Diagnostics.Debug.Assert(q1 != null && q2 != null);

				var left = q1.Dequeue();
				var right = q2.Dequeue();
				var parent = new HuffmanNode(-1, candidateWeight)
				{
					Left = left,
					Right = right
				};
				left.Parent = parent;
				right.Parent = parent;
				branches.Enqueue(parent);
			}

			foreach (var huffmanNode in _leaves.Values)
			{
				huffmanNode.SetupBitPattern();
			}
			return new HuffmanTable(_leaves, branches.Dequeue());
		}
	}
}