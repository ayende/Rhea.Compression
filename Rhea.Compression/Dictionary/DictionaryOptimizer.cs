// -----------------------------------------------------------------------
//  <copyright file="DictionaryOptimizer.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rhea.Compression.Dictionary
{
    public class DictionaryOptimizer
    {
        private MemoryStream stream = new MemoryStream();
        private List<int> starts = new List<int>();
        private int[] suffixArray;
        private int[] lcp;
        private SubstringArray substrings;
        private byte[] bytes;

        public void Add(byte[] doc)
        {
            starts.Add((int)stream.Position);
            stream.Write(doc, 0, doc.Length);
        }

        public void Add(string doc)
        {
            Add(Encoding.UTF8.GetBytes(doc));
        }

        public void DumpSuffixes(TextWriter output)
        {
            for (int i = 0; i < suffixArray.Length; i++)
            {
                output.Write(suffixArray[i] + "\t");
                output.Write(lcp[i] + "\t");
                output.Write(Encoding.UTF8.GetString(bytes, suffixArray[i], Math.Min(40, bytes.Length - suffixArray[i])));
                output.WriteLine();
            }
        }

        public void DumpSubstrings(TextWriter output)
        {
            if (substrings != null)
            {
                for (int j = substrings.Size - 1; j >= 0; j--)
                {
                    if (substrings.Length(j) == 0)
                        continue;
                    output.Write(substrings.Score(j) + "\t");
                    output.Write(Encoding.UTF8.GetString(bytes, suffixArray[substrings.Index(j)], Math.Min(40, substrings.Length(j))));
                    output.WriteLine();
                }
            }
        }

        public byte[] Optimize(int desiredLength)
        {
            bytes = stream.ToArray();
            suffixArray = SuffixArray.computeSuffixArray(bytes);
            lcp = SuffixArray.computeLCP(bytes, suffixArray);
            ComputeSubstrings();
            return Pack(desiredLength);
        }

        private byte[] Pack(int desiredLength)
        {
            var pruned = new SubstringArray(1024);
            int i, size = 0;

            for (i = substrings.Size - 1; i >= 0; i--)
            {
                bool alreadyCovered = false;
                for (int j = 0, c = pruned.Size; j < c; j++)
                {
                    if (pruned.IndexOf(j, substrings, i, bytes, suffixArray) != -1)
                    {

                        alreadyCovered = true;
                        break;
                    }
                }

                if (alreadyCovered)
                {
                    continue;
                }

                for (int j = pruned.Size - 1; j >= 0; j--)
                {
                    if (substrings.IndexOf(i, pruned, j, bytes, suffixArray) != -1)
                    {
                        size -= pruned.Length(j);
                        pruned.Remove(j);
                    }
                }
                pruned.SetScore(pruned.Size, substrings.Index(i), substrings.Length(i), substrings.Score(i));
                size += substrings.Length(i);
                // We calculate 2x because when we lay the strings out end to end we will merge common prefix/suffixes
                if (size >= 2 * desiredLength)
                {
                    break;
                }
            }

            byte[] packed = new byte[desiredLength];
            int pi = desiredLength;

            int count;
            for (i = 0, count = pruned.Size; i < count && pi > 0; i++)
            {
                int length = pruned.Length(i);
                if (pi - length < 0)
                {
                    length = pi;
                }
                pi -= prepend(bytes, suffixArray[pruned.Index(i)], packed, pi, length);
            }

            if (pi > 0)
            {
                packed = packed.Skip(pi).Take(packed.Length).ToArray();
            }

            return packed;
        }

        private int prepend(byte[] from, int fromIndex, byte[] to, int toIndex, int length)
        {
            int l;
            // See if we have a common suffix/prefix between the string being merged in, and the current strings in the front
            // of the destination.  For example if we pack " the " and then pack " and ", we should end up with " and the ", not " and  the ".
            for (l = Math.Min(length - 1, to.Length - toIndex); l > 0; l--)
            {
                if (byteRangeEquals(from, fromIndex + length - l, to, toIndex, l))
                {
                    break;
                }
            }

            Array.Copy(from, fromIndex, to, toIndex - length + l, length - l);
            return length - l;
        }

        private static bool byteRangeEquals(byte[] bytes1, int index1, byte[] bytes2, int index2, int length)
        {

            for (; length > 0; length--, index1++, index2++)
            {
                if (bytes1[index1] != bytes2[index2])
                {
                    return false;
                }
            }
            return true;
        }

        public string Suffix(int i)
        {
            var x = suffixArray[i];
            return Encoding.UTF8.GetString(bytes, x, Math.Min(15, bytes.Length - x));
        }
        // TODO Bring this up to parity with C++ version, which has optimized
        private void ComputeSubstrings()
        {
            var activeSubstrings = new SubstringArray(128);
            var uniqueDocIds = new HashSet<int>();

            substrings = new SubstringArray(1024);
            int n = lcp.Length;

            int lastLCP = lcp[0];
            for (int i = 1; i <= n; i++)
            {
                // Note we need to process currently existing runs, so we do that by acting like we hit an LCP of 0 at the end.
                // That is why the we loop i <= n vs i < n.  Otherwise runs that exist at the end of the suffixarray/lcp will
                // never be "cashed in" and counted in the substrings.  DictionaryOptimizerTest has a unit test for this.
                int currentLCP = i == n ? 0 : lcp[i];

                if (currentLCP > lastLCP)
                {
                    // The order here is important so we can optimize adding redundant strings below.
                    for (int j = lastLCP + 1; j <= currentLCP; j++)
                    {
                        activeSubstrings.Add(i, j, 0);
                    }
                }
                else if (currentLCP < lastLCP)
                {
                    int lastActiveIndex = -1, lastActiveLength = -1, lastActiveCount = -1;
                    for (int j = activeSubstrings.Size - 1; j >= 0; j--)
                    {
                        if (activeSubstrings.Length(j) > currentLCP)
                        {
                            int activeCount = i - activeSubstrings.Index(j) + 1;
                            int activeLength = activeSubstrings.Length(j);
                            int activeIndex = activeSubstrings.Index(j);

                            // Ok we have a string which occurs activeCount times.  The true measure of its
                            // value is how many unique documents it occurs in, because occurring 1000 times in the same
                            // document isn't valuable because once it occurs once, subsequent occurrences will reference
                            // a previous occurring instance in the document.  So for 2 documents: "garrick garrick garrick toubassi",
                            // "toubassi", the string toubassi is far more valuable in a shared dictionary.  So find out
                            // how many unique documents this string occurs in.  We do this by taking the start position of
                            // each occurrence, and then map that back to the document using the "starts" array, and uniquing.
                            // 
                            // TODO Bring this up to parity with C++ version, which has optimized
                            //

                            for (int k = activeSubstrings.Index(j) - 1; k < i; k++)
                            {
                                int byteIndex = suffixArray[k];

                                // Could make this a lookup table if we are willing to burn an int[bytes.length] but thats a lot
                                int docIndex = starts.BinarySearch(byteIndex);

                                if (docIndex < 0)
                                {
                                    docIndex = -docIndex -2;
                                }

                                // While we are at it lets make sure this is a string that actually exists in a single
                                // document, vs spanning two concatenanted documents.  The idea is that for documents
                                // "http://espn.com", "http://google.com", "http://yahoo.com", we don't want to consider
                                // ".comhttp://" to be a legal string.  So make sure the length of this string doesn't
                                // cross a document boundary for this particular occurrence.
                                int nextDocStart = docIndex < starts.Count - 1 ? starts[docIndex + 1] : bytes.Length;
                                if (activeLength <= nextDocStart - byteIndex)
                                {
                                    uniqueDocIds.Add(docIndex);
                                }
                            }

                            int scoreCount = uniqueDocIds.Count;

                            uniqueDocIds.Clear();

                            activeSubstrings.Remove(j);

                            if (scoreCount == 0)
                            {
                                continue;
                            }

                            // Don't add redundant strings.  If we just  added ABC, don't add AB if it has the same count.  This cuts down the size of substrings
                            // from growing very large.
                            if (!(lastActiveIndex != -1 && lastActiveIndex == activeIndex && lastActiveCount == activeCount && lastActiveLength > activeLength))
                            {

                                if (activeLength > 3)
                                {
                                    substrings.Add(activeIndex, activeLength, scoreCount);
                                }
                            }
                            lastActiveIndex = activeIndex;
                            lastActiveLength = activeLength;
                            lastActiveCount = activeCount;
                        }
                    }
                }
                lastLCP = currentLCP;
            }
            substrings.Sort();
        }

    }
}