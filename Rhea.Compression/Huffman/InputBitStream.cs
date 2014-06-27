using System;
using System.IO;

namespace Rhea.Compression.Huffman
{
	public class InputBitStream : IDisposable
	{
		private readonly Stream _stream;
		private readonly bool _leaveOpen;
		private int _position;
		private byte _buffer;


		public bool Current
		{
			get { return (_buffer & (byte) (1 << _position)) != 0; } 
		}

		public InputBitStream(Stream stream, bool leaveOpen = false)
		{
			_stream = stream;
			_leaveOpen = leaveOpen;
			_position = 8;
		}

		public bool MoveNext()
		{
			_position++;
			if (_position >=8)
			{
				var b = _stream.ReadByte();
				if (b == -1)
					return false;
				_buffer = (byte) b;
				_position = 0;
			}
			return true;
		}

		public void Dispose()
		{
			if (_leaveOpen == false)
				_stream.Dispose();
		}
	}
}