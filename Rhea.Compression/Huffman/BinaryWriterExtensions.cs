using System;
using System.IO;

namespace Rhea.Compression.Huffman
{
    public static class BinaryWriterExtensions
    {
        public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
        {
            uint num = (uint)value;

            while (num >= 128U)
            {
                writer.Write((byte)(num | 128U));
                num >>= 7;
            }

            writer.Write((byte)num);
        }

        public static int Read7BitEncodedInt(this BinaryReader reader)
        {
            // some names have been changed to protect the readability  
            int returnValue = 0;
            int bitIndex = 0;

            while (bitIndex != 35)
            {
                byte currentByte = reader.ReadByte();
                returnValue |= ((int)currentByte & (int)SByte.MaxValue) << bitIndex;
                bitIndex += 7;

                if (((int)currentByte & 128) == 0)
                    return returnValue;
            }

            throw new FormatException("Invalid format for 7 bit encoded string");
        }
    }
}