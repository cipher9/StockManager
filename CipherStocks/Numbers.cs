using System;
using System.Collections.Generic;
using System.Text;

namespace CipherStocks
{
    public static class Numbers
    {
        public const int Million = 1000000;
        public const long Billion = 1000000000;
        public const long Trillion = 1000000000000;

        public static string FormatNumber(decimal number, bool isCurrency)
        {
            if(isCurrency == true)
            {
                if(number >= Trillion)
                    return (number/Trillion).ToString("C2") + "T";
                if(number >= Billion)
                    return (number/Billion).ToString("C2") + "B";
                if(number >= Million)
                    return (number/Million).ToString("C2") + "M";
                return number.ToString("C2");
            }
            else
            {
                if(number >= Trillion)
                    return (number/Trillion).ToString("N0") + "T";
                if(number >= Billion)
                    return (number/Billion).ToString("N0") + "B";
                if(number >= Million)
                    return (number/Million).ToString("N0") + "M";
                return number.ToString("N0");
            }
        }
    }
}
