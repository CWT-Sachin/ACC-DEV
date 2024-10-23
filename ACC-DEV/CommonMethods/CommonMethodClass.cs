using System;


namespace ACC_DEV.CommonMethods
{
    

    public class CommonMethodClass
    {
        private static string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
        private static string[] tens = { "", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        private static string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        private static string[] largeNumbers = { "", "Thousand", "Million", "Billion", "Trillion" };
        public static string ConvertToWords(decimal amount)
        {
            if (amount == 0)
                return "Zero";

            if (amount < 0)
                return "Minus " + ConvertToWords(Math.Abs(amount));

            string words = "";

            // Handle the integer part
            long integerPart = (long)Math.Floor(amount);
            int largeNumberIndex = 0;

            while (integerPart > 0)
            {
                if (integerPart % 1000 != 0)
                {
                    words = ConvertToWordsUnderThousand((int)(integerPart % 1000)) + " " + largeNumbers[largeNumberIndex] + " " + words;
                }

                integerPart /= 1000;
                largeNumberIndex++;
            }

            // Handle the decimal part
            int decimalPart = (int)((amount - Math.Truncate(amount)) * 100);

            if (decimalPart > 0)
            {
                if (!string.IsNullOrEmpty(words))
                    words += " and ";

                words += ConvertToWordsUnderThousand(decimalPart) + " Cents";
            }

            return words;
        }

        private static string ConvertToWordsUnderThousand(int number)
        {
            string words = "";

            int hundredsDigit = number / 100;
            int tensDigit = (number % 100) / 10;
            int onesDigit = number % 10;

            if (hundredsDigit > 0)
            {
                words += units[hundredsDigit] + " Hundred";
                if (tensDigit > 0 || onesDigit > 0)
                    words += " and ";
            }

            if (tensDigit > 1)
            {
                words += tens[tensDigit];
                if (onesDigit > 0)
                    words += " " + units[onesDigit];
            }
            else if (tensDigit == 1)
            {
                words += teens[number % 100 - 10];
            }
            else if (onesDigit > 0)
            {
                words += units[onesDigit];
            }

            return words.Trim();
        }



    }

}
