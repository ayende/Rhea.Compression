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
using System.Collections.Generic;

namespace Rhea.Compression.Dictionary
{
    public class SubstringArray
    {

        private List<int> indexes = new List<int>();
        private List<int> lengths = new List<int>();
        private List<int> scores = new List<int>();
        private int size;
        private int capacity;

        public SubstringArray(int capacity)
        {
            SetScore(capacity, 0, 0, 0);
        }

        public void Sort()
        {
            int[] histogram = new int[256];
            SubstringArray working = new SubstringArray(size);

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
                    working.SetScore(newOffset, indexes[i], lengths[i], this.scores[i]);
                }

                // swap (brain transplant) innards
                List<int> t = working.indexes;
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
                int growBy = (((i - capacity) / (8 * 1024)) + 1) * 8 * 1024;

                foreach (var list in new[] { indexes, lengths, scores})
                {
                    list.Capacity += growBy;
                    for (int j = list.Count; j < list.Capacity; j++)
                    {
                        list.Add(0);
                    }
                }

                capacity += growBy;
            }

            this.indexes[i] = index;
            this.lengths[i] = length;
            this.scores[i] = score;

            size = Math.Max(i + 1, size);

            return i;
        }

        public void Remove(int i)
        {
            indexes.Remove(i);
            lengths.Remove(i);
            scores.Remove(i);
            size--;
        }

        public int Size
        {
            get { return size; }
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
            return (100 * count * (length - 3)) / length;
        }
    }
}
