namespace Vargshala.Contracts.Common;

/// <summary>
/// Helper to convert monetary amounts into standard Indian currency words (Rupees / Paise).
/// Format: Crores, Lakhs, Thousands, Hundreds, Units.
/// Shared across Contracts, Application, Infrastructure, API, and Web.
/// </summary>
public static class NumberToWordsHelper
{
    private static readonly string[] UnitsMap =
    {
        "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
    };

    private static readonly string[] TensMap =
    {
        "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    };

    public static string ToIndianCurrencyWords(decimal amount)
    {
        if (amount <= 0) return "Zero Rupees Only";

        long wholePart = (long)Math.Floor(amount);
        int decimalPart = (int)Math.Round((amount - wholePart) * 100);

        string words = ConvertWholeNumberToWords(wholePart);
        string result = words + " Rupees";

        if (decimalPart > 0)
        {
            result += " and " + ConvertWholeNumberToWords(decimalPart) + " Paise";
        }

        return result + " Only";
    }

    private static string ConvertWholeNumberToWords(long number)
    {
        if (number == 0) return "Zero";
        if (number < 0) return "Minus " + ConvertWholeNumberToWords(Math.Abs(number));

        string words = "";

        if (number / 10000000 > 0) // Crores
        {
            words += ConvertWholeNumberToWords(number / 10000000) + " Crore ";
            number %= 10000000;
        }

        if (number / 100000 > 0) // Lakhs
        {
            words += ConvertWholeNumberToWords(number / 100000) + " Lakh ";
            number %= 100000;
        }

        if (number / 1000 > 0) // Thousands
        {
            words += ConvertWholeNumberToWords(number / 1000) + " Thousand ";
            number %= 1000;
        }

        if (number / 100 > 0) // Hundreds
        {
            words += ConvertWholeNumberToWords(number / 100) + " Hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            if (number < 20)
            {
                words += UnitsMap[number];
            }
            else
            {
                words += TensMap[number / 10];
                if (number % 10 > 0)
                {
                    words += " " + UnitsMap[number % 10];
                }
            }
        }

        return words.Trim();
    }
}
