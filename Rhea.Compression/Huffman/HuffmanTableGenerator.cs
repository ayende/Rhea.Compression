using System;
using System.Collections.Generic;

namespace Rhea.Compression.Huffman
{
	public class HuffmanTableGenerator
	{
		private readonly Heap<HuffmanNode> _priorityQueue = new Heap<HuffmanNode>(768);
		private readonly Dictionary<int, HuffmanNode> _leaves = new Dictionary<int, HuffmanNode>();

		public void Add(int symbol, int freq)
		{
			var node = new HuffmanNode(symbol, freq);
			_leaves.Add(symbol, node);
			_priorityQueue.Enqueue(node);
		}

		public HuffmanTable Build()
		{
			if(_priorityQueue.Count ==0)
				throw new InvalidOperationException("You must add some symbols before we can generate valid table");
			while (_priorityQueue.Count > 1)
			{
				var x = _priorityQueue.Dequeue();
				var y = _priorityQueue.Dequeue();

				var parent = new HuffmanNode(-1, (ushort)(x.Freq + y.Freq));
				parent.Left = x;
				parent.Right = y;
				x.Parent = parent;
				y.Parent = parent;

				_priorityQueue.Enqueue(parent);
			}
			return new HuffmanTable(_leaves, _priorityQueue.Dequeue());
		}
	}
}