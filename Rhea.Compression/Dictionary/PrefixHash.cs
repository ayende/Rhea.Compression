// -----------------------------------------------------------------------
//  <copyright file="PrefixHash.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

namespace Rhea.Compression.Dictionary
{
    public class PrefixHash
    {
        public static int PrefixLength = 4;

        private readonly byte[] buffer;
        private readonly int[] hash;
        private readonly int[] heap;

        public PrefixHash(byte[] buf, bool addToHash)
        {
            buffer = buf;
            hash = new int[(int) (1.75*buf.Length)];
            for (int i = 0; i < hash.Length; i++)
            {
                hash[i] = -1;
            }
            heap = new int[buf.Length];
            for (int i = 0; i < heap.Length; i++)
            {
                heap[i] = -1;
            }
            if (addToHash)
            {
                for (int i = 0, count = buf.Length - PrefixLength; i < count; i++)
                {
                    Put(i);
                }
            }
        }

        public void DumpState(TextWriter output)
        {
            output.WriteLine("Hash:");
            for (int i = 0; i < hash.Length; i++)
            {
                if(hash[i] == -1)
                    continue;
                output.WriteLine("hash[{0,3}] = {1,3};",i,hash[i]);
            }

            output.WriteLine("Heap:");
            for (int i = 0; i < heap.Length; i++)
            {
                if (heap[i] == -1)
                    continue;
                output.WriteLine("heap[{0,3}] = {1,3};", i, heap[i]);
            }
        }

        private int HashIndex(byte[] buf, int i)
        {
            int code = (buf[i] & 0xff) | ((buf[i + 1] & 0xff) << 8) | ((buf[i + 2] & 0xff) << 16) |
                       ((buf[i + 3] & 0xff) << 24);
            return (code & 0x7fffff)%hash.Length;
        }


        public void Put(int index)
        {
            int hi = HashIndex(buffer, index);
            heap[index] = hash[hi];
            hash[hi] = index;
        }

        public Match GetBestMatch(int index, byte[] targetBuf)
        {
            int bestMatchIndex = 0;
            int bestMatchLength = 0;

            int bufLen = buffer.Length;

            if (bufLen == 0)
            {
                return new Match(0, 0);
            }

            int targetBufLen = targetBuf.Length;

            int maxLimit = Math.Min(255, targetBufLen - index);

            int targetHashIndex = HashIndex(targetBuf, index);
            int candidateIndex = hash[targetHashIndex];
            while (candidateIndex >= 0)
            {
                int distance;
                if (targetBuf != buffer)
                {
                    distance = index + bufLen - candidateIndex;
                }
                else
                {
                    distance = index - candidateIndex;
                }
                if (distance > (2 << 15) - 1)
                {
                    // Since we are iterating over nearest offsets first, once we pass 64k
                    // we know the rest are over 64k too.
                    break;
                }

                int maxMatchJ = index + Math.Min(maxLimit, bufLen - candidateIndex);
                int j, k;
                for (j = index, k = candidateIndex; j < maxMatchJ; j++, k++)
                {
                    if (buffer[k] != targetBuf[j])
                    {
                        break;
                    }
                }

                int matchLength = j - index;
                if (matchLength > bestMatchLength)
                {
                    bestMatchIndex = candidateIndex;
                    bestMatchLength = matchLength;
                }
                candidateIndex = heap[candidateIndex];
            }

            return new Match(bestMatchIndex, bestMatchLength);
        }

        public class Match
        {
            public readonly int BestMatchIndex;
            public readonly int BestMatchLength;

            public Match(int bestMatchIndex, int bestMatchLength)
            {
                BestMatchIndex = bestMatchIndex;
                BestMatchLength = bestMatchLength;
            }
        }
    }
}