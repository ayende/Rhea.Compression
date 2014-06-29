/**
 *   Copyright 2011 Garrick Toubassi
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */

using System;

namespace Rhea.Compression.Dictionary
{
    public class SubstringArray
    {
        private int capacity;
        private int[] indexes;
        private int[] lengths;
        private int[] scores;
        private int size;

        public SubstringArray(int capacity)
        {
            this.capacity = capacity;
            indexes = new int[capacity];
            lengths = new int[capacity];
            scores = new int[capacity];
        }

        public int Size
        {
            get { return size; }
        }

        public void Sort()
        {
            var histogram = new int[256];
            var working = new SubstringArray(size);

            for (int bitOffset = 0; bitOffset <= 24; bitOffset += 8)
            {
                if (bitOffset > 0)
                {
                    for (int j = 0; j < histogram.Length; j++)
                    {
                        histogram[j] = 0;
                    }
                }
                int i, count, rollingSum;
                for (i = 0, count = size; i < count; i++)
                {
                    int sortValue = scores[i];
                    int sortByte = (sortValue >> bitOffset) & 0xff;
                    histogram[sortByte]++;
                }

                for (i = 0, count = histogram.Length, rollingSum = 0; i < count; i++)
                {
                    int tmp = histogram[i];
                    histogram[i] = rollingSum;
                    rollingSum += tmp;
                }

                for (i = 0, count = size; i < count; i++)
                {
                    int sortValue = scores[i];
                    int sortByte = (sortValue >> bitOffset) & 0xff;
                    int newOffset = histogram[sortByte]++;
                    working.SetScore(newOffset, indexes[i], lengths[i], scores[i]);
                }

                // swap (brain transplant) innards
                int[] t = working.indexes;
                working.indexes = indexes;
                indexes = t;

                t = working.lengths;
                working.lengths = lengths;
                lengths = t;

                t = working.scores;
                working.scores = scores;
                scores = t;

                size = working.size;
                working.size = 0;

                i = working.capacity;
                working.capacity = capacity;
                capacity = i;
            }
        }


        public int Add(int index, int length, int count)
        {
            return SetScore(size, index, length, ComputeScore(length, count));
        }

        public int SetScore(int i, int index, int length, int score)
        {
            if (i >= capacity)
            {
                int growBy = (((i - capacity)/(8*1024)) + 1)*8*1024;
                // Since this array is going to be VERY big, don't double.        

                var newindex = new int[indexes.Length + growBy];
                Array.Copy(indexes, 0, newindex, 0, indexes.Length);
                indexes = newindex;

                var newlength = new int[lengths.Length + growBy];
                Array.Copy(lengths, 0, newlength, 0, lengths.Length);
                lengths = newlength;

                var newscores = new int[scores.Length + growBy];
                Array.Copy(scores, 0, newscores, 0, scores.Length);
                scores = newscores;

                capacity = indexes.Length;
            }

            indexes[i] = index;
            lengths[i] = length;
            scores[i] = score;

            size = Math.Max(i + 1, size);

            return i;
        }

        public void Remove(int i)
        {
            Array.Copy(indexes, i + 1, indexes, i, size - i - 1);
            Array.Copy(lengths, i + 1, lengths, i, size - i - 1);
            Array.Copy(scores, i + 1, scores, i, size - i - 1);
            size--;
        }

        public int Index(int i)
        {
            return indexes[i];
        }

        public int Length(int i)
        {
            return lengths[i];
        }

        public int Score(int i)
        {
            return scores[i];
        }

        public int IndexOf(int s1, SubstringArray sa, int s2, byte[] s, int[] prefixes)
        {
            int index1 = indexes[s1];
            int length1 = lengths[s1];
            int index2 = sa.indexes[s2];
            int length2 = sa.lengths[s2];

            for (int i = prefixes[index1], n = prefixes[index1] + length1 - length2 + 1; i < n; i++)
            {
                bool found = true;
                for (int j = prefixes[index2], nj = prefixes[index2] + length2, i1 = i; j < nj; j++, i1++)
                {
                    if (s[i1] != s[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        /*
     * Substring of length n occurring m times.  We will reduce output by n*m characters, and add 3*m offsets/lengths.  So net benefit is (n - 3)*m.
     * Costs n characters to include in the compression dictionary, so compute a "per character consumed in the compression dictionary" benefit.
     * score = m*(n-3)/n
     */

        private int ComputeScore(int length, int count)
        {
            if (length <= 3)
            {
                return 0;
            }
            return (100*count*(length - 3))/length;
        }
    }
}