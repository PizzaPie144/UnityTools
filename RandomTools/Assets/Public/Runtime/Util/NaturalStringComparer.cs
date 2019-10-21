using System;
using System.Collections;
using System.Collections.Generic;

namespace PizzaPie.Runtime.Util
{
    /// <summary>
    /// Natural string comparer.
    /// </summary>
    public class NaturalStringComparer : IComparer<String>
    {
        /// <summary>
        /// Compare sting x with string y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(string x, string y)
        {
            int index1 = 0;
            int index2 = 0;

            double num1 = 0;
            double num2 = 0;

            int maxLength = Math.Max(x.Length, y.Length);
            int safe = 0;

            while (index1 < maxLength && index2 < maxLength)
            {
                char c1 = GetCharAt(index1, x);
                char c2 = GetCharAt(index2, y);

                if (!Char.IsDigit(c1) && !Char.IsDigit(c2))
                {
                    int r = c1.CompareTo(c2);
                    if (r != 0)
                        return r;

                    index1++;
                    index2++;
                }
                else
                {
                    num1 = Char.GetNumericValue(c1);        
                    num2 = Char.GetNumericValue(c2);        
                    index1++;
                    index2++;

                    char c11 = GetCharAt(index1, x);
                    char c22 = GetCharAt(index2, y);

                    int count = 0;

                    while (Char.IsDigit(c11) || Char.IsDigit(c22))
                    {
                        if (Char.IsDigit(c11))
                        {
                            num1 = num1 * 10f + Char.GetNumericValue(c11);
                            index1++;
                        }
                        if (Char.IsDigit(c22))
                        {
                            num2 = num2 * 10f + Char.GetNumericValue(c22);
                            index2++;
                        }

                        c11 = GetCharAt(index1, x);
                        c22 = GetCharAt(index2, y);

                        count++;
                        if (count > 10000000)          //REMOVE
                            throw new Exception("Stuck in comparison");
                    }


                    if (num1 != num2)
                    {
                        return num1 < num2 ? -1 : 1;
                    }
                }
                safe++;
                if (safe > 10000000)
                    throw new Exception("Stuck in comparison");
            }
            return 0;
        }

        /// <summary>
        /// Get Char at or white space if index is out of range
        /// </summary>
        /// <param name="index"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private static char GetCharAt(int index, string str)
        {
            return index < str.Length ? str[index] : ' ';
        }
    }
}
