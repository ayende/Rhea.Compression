using System;

namespace Rhea.Compression.Dictionary
{
    public interface IPackerOutput
    {
        void EncodeLiteral(byte aByte, Object context);
        void EncodeSubstring(int offset, int length, Object context);
        void EndEncoding(Object context);
    }
}