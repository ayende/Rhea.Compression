using System;
using System.IO;

namespace Rhea.Compression.Huffman
{
	public class OutputBitStream : IDisposable
	{
		private readonly Stream _stream;
		private readonly bool _leaveOpen;
		private int _count;
		private byte _buffer;
		private int _length;

		public int Length
		{
			get { return _length; }
		}

		public OutputBitStream(Stream stream, bool leaveOpen = false)
		{
			_stream = stream;
			_leaveOpen = leaveOpen;
		}

		public void Write(bool bit)
		{
			_length++;
			if (_count == 8)
				Flush();

			_count++;
			if (bit == false)
				return;
			_buffer |= (byte) (1 << _count - 1);
		}

		public void Dispose()
		{
			Flush();
			if(_leaveOpen)
				_stream.Dispose();
		}

		public void Flush()
		{
			if (_count == 0)
				return;

			_stream.WriteByte(_buffer);
			_count = 0;
			_buffer = 0;
		}
	}
}