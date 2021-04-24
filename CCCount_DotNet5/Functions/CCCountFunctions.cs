using System;

namespace CCCount.Functions
{
    public class CCCountFunctions
    {
        public static Tuple<int, int> GetFiscalCalendarMonthYear()
        {
            DateTime date = DateTime.Now;

            int month = date.Month;
            int year = date.Year;

            // If current month is 6, return 5
            // If month = 1, then return 12 and subtract year
            switch (month) {
                case 1:
                    month = 12;
                    year--;
                    break;
                default:
                    month--;
                    break;
            }

            return new Tuple<int, int>(month, year);
        }

        public static string GetShortGuid()
        {
            // Get a shorter version of System.Guid
            //  - Converts to base 64
            //  - Replaces web unsafe characters
            //  - Removes unneccessary '=' from end
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                                        .Replace('/', '-')
                                                        .Replace('+', '_')
                                                        .Trim('=');
        }
    }

    
}
