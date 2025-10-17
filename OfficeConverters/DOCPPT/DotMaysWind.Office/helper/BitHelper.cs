using System;

namespace DotMaysWind.Office.helper
{
    internal static class BitHelper
    {
        internal static bool GetBitFromInteger(int integer, int bitIndex)
        {
            int num = (int)Math.Pow(2, bitIndex);
            return (integer & num) == num;
        }
    }
}