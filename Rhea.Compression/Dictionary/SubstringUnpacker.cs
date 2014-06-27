// -----------------------------------------------------------------------
//  <copyright file="StringUnpacker.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Text;

namespace Rhea.Compression.Dictionary
{

    public class SubstringUnpacker
    {
        private readonly byte[] dictionary;
        private MemoryStream buffer = new MemoryStream();

        public SubstringUnpacker(byte[] dictionary)
        {
            this.dictionary = dictionary ?? new byte[0];
        }

        public void Reset()
        {
            buffer.SetLength(0);
        }

        public ArraySegment<byte> UncompressedData()
        {
            return new ArraySegment<byte>(buffer.GetBuffer(),0, (int)buffer.Length);
        } 

        public void EncodeLiteral(byte aByte)
        {
            buffer.WriteByte(aByte);
        }

        public void EncodeSubstring(int offset, int length)
        {
            var currentIndex = (int)buffer.Length;
            if (currentIndex + offset < 0)
            {
                int startDict = currentIndex + offset + dictionary.Length;
                int endDict = startDict + length;
                int end = 0;

                if (endDict > dictionary.Length)
                {
                    end = endDict - dictionary.Length;
                    endDict = dictionary.Length;
                }

                if (endDict - startDict > 0)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(dictionary, startDict, endDict - startDict));
                    buffer.Write(dictionary, startDict, endDict - startDict);
                }

                if (end > 0)
                {
                    var bytes = buffer.GetBuffer();
                    Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, end));
                    buffer.Write(bytes, 0, end);
                }
            }
            else
            {
                var bytes = buffer.GetBuffer();
                Console.WriteLine(Encoding.UTF8.GetString(bytes, currentIndex+offset, length));
                buffer.Write(bytes, currentIndex + offset, length);
            }
        }
    }

}