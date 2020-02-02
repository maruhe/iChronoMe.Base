using System;
using System.Globalization;
using System.Text;

using iChronoMe.Core.Classes;

/*  
 Customized format patterns:

 Patterns   Format      Description                           Example
 =========  ==========  ===================================== ========
    
    "d"     "0"         day w/o leading zero                  1
    "dd"    "00"        day with leading zero                 01
    "ddd"               short weekday name (abbreviation)     Mon
    "dddd"              full weekday name                     Monday
    "dddd*"             full weekday name                     Monday


    "M"     "0"         month w/o leading zero                2
    "MM"    "00"        month with leading zero               02
    "MMM"               short month name (abbreviation)       Feb
    "MMMM"              full month name                       Febuary
    "MMMM*"             full month name                       Febuary

    "y"     "0"         two digit year (year % 100) w/o leading zero           0
    "yy"    "00"        two digit year (year % 100) with leading zero          00
    "yyy"   "D3"        year                                  2000
    "yyyy"  "D4"        year                                  2000
    "yyyyy" "D5"        year                                  2000
    ...

    ////////////May be implemented
    ////////////"z"     "+0;-0"     timezone offset w/o leading zero      -8
    ////////////"zz"    "+00;-00"   timezone offset with leading zero     -08
    ////////////"zzz"      "+00;-00" for hour offset, "00" for minute offset  full timezone offset   -07:30
    ////////////"zzz*"  "+00;-00" for hour offset, "00" for minute offset   full timezone offset   -08:00

    ////////////"K"    -Local       "zzz", e.g. -08:00
    ////////////       -Utc         "'Z'", representing UTC
    ////////////       -Unspecified ""               
    ////////////       -DateTimeOffset      "zzzzz" e.g -07:30:15

    ////////////"g*"                the current era name                  A.D.
    ////////////May be implemented

    ":"                 time separator                        : -- DEPRECATED - Insert separator directly into pattern (eg: "H.mm.ss")
    "/"                 date separator                        /-- DEPRECATED - Insert separator directly into pattern (eg: "M-dd-yyyy")
    "'"                 quoted string                         'ABC' will insert ABC into the formatted string.
    '"'                 quoted string                         "ABC" will insert ABC into the formatted string.
    "%"                 used to quote a single pattern characters      E.g.The format character "%y" is to print two digit year.
    "\"                 escaped character                     E.g. '\d' insert the character 'd' into the format string.
    other characters    insert the character into the format string. 

Pre-defined format characters: 
    (U) to indicate Universal time is used.
    (G) to indicate Gregorian calendar is used.

    Format              Description                             Real format                             Example
    =========           =================================       ======================                  =======================
    "d"                 short date                              culture-specific                        10/31/1999
    "D"                 long data                               culture-specific                        Sunday, October 31, 1999
    "f"                 full date (long date + short time)      culture-specific                        Sunday, October 31, 1999 2:00 AM
    "F"                 full date (long date + long time)       culture-specific                        Sunday, October 31, 1999 2:00:00 AM
    "g"                 general date (short date + short time)  culture-specific                        10/31/1999 2:00 AM
    "G"                 general date (short date + long time)   culture-specific                        10/31/1999 2:00:00 AM
    "m"/"M"             Month/Day date                          culture-specific                        October 31
(G) "o"/"O"             Round Trip XML                          "yyyy-MM-ddTHH:mm:ss.fffffffK"          1999-10-31 02:00:00.0000000Z
(G) "r"/"R"             RFC 1123 date,                          "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"   Sun, 31 Oct 1999 10:00:00 GMT
(G) "s"                 Sortable format, based on ISO 8601.     "yyyy-MM-dd'T'HH:mm:ss"                 1999-10-31T02:00:00
                                                                ('T' for local time)
    "t"                 short time                              culture-specific                        2:00 AM
    "T"                 long time                               culture-specific                        2:00:00 AM
(G) "u"                 Universal time with sortable format,    "yyyy'-'MM'-'dd HH':'mm':'ss'Z'"        1999-10-31 10:00:00Z
                        based on ISO 8601.
(U) "U"                 Universal time with full                culture-specific                        Sunday, October 31, 1999 10:00:00 AM
                        (long date + long time) format
    "y"/"Y"             Year/Month day                          culture-specific                        October, 1999

*/


namespace iChronoMe.Core.DynamicCalendar
{
    internal static class DynamicDateFormat
    {
        internal static int ParseRepeatPattern(String format, int pos, char patternChar)
        {
            int len = format.Length;
            int index = pos + 1;
            while ((index < len) && (format[index] == patternChar))
            {
                index++;
            }
            return (index - pos);
        }

        //
        // Get the next character at the index of 'pos' in the 'format' string.
        // Return value of -1 means 'pos' is already at the end of the 'format' string.
        // Otherwise, return value is the int value of the next character.
        //
        internal static int ParseNextChar(String format, int pos)
        {
            if (pos >= format.Length - 1)
            {
                return (-1);
            }
            return ((int)format[pos + 1]);
        }

        //
        // The pos should point to a quote character. This method will
        // get the string encloed by the quote character.
        //
        internal static int ParseQuoteString(String format, int pos, StringBuilder result)
        {
            //
            // NOTE : pos will be the index of the quote character in the 'format' string.
            //
            int formatLen = format.Length;
            int beginPos = pos;
            char quoteChar = format[pos++]; // Get the character used to quote the following string.

            bool foundQuote = false;
            while (pos < formatLen)
            {
                char ch = format[pos++];
                if (ch == quoteChar)
                {
                    foundQuote = true;
                    break;
                }
                else if (ch == '\\')
                {
                    // The following are used to support escaped character.
                    // Escaped character is also supported in the quoted string.
                    // Therefore, someone can use a format like "'minute:' mm\"" to display:
                    //  minute: 45"
                    // because the second double quote is escaped.
                    if (pos < formatLen)
                    {
                        result.Append(format[pos++]);
                    }
                    else
                    {
                        //
                        // This means that '\' is at the end of the formatting string.
                        //
                        throw new FormatException("Format_InvalidString");
                    }
                }
                else
                {
                    result.Append(ch);
                }
            }

            if (!foundQuote)
            {
                // Here we can't find the matching quote.
                throw new FormatException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            "Format_BadQuote", quoteChar));
            }

            //
            // Return the character count including the begin/end quote characters and enclosed string.
            //
            return (pos - beginPos);
        }

        internal static void FormatDigits(StringBuilder outputBuffer, int value, int len)
        {
            String fmtPattern = "D" + len;
            outputBuffer.Append(value.ToString(fmtPattern, CultureInfo.InvariantCulture));
        }

        // Expand a pre-defined format string (like "D" for long date) to the real format that
        // we are going to use in the date time parsing.
        // This method also convert the dateTime if necessary (e.g. when the format is in Universal time),
        // and change ddfi if necessary (e.g. when the format should use invariant culture).
        //
        private static String ExpandPredefinedFormat(String format, ref DynamicDateFormatInfo ddfi)
        {
            switch (format[0])
            {
                case 'o':
                case 'O':       // Round trip format
                    ddfi = DynamicDateFormatInfo.InvariantInfo;
                    break;
                case 'r':
                case 'R':       // RFC 1123 Standard                    
                    ddfi = DynamicDateFormatInfo.InvariantInfo;
                    break;
                case 's':       // Sortable without Time Zone Info                
                    ddfi = DynamicDateFormatInfo.InvariantInfo;
                    break;
                case 'u':       // Universal time in sortable format.
                    ddfi = DynamicDateFormatInfo.InvariantInfo;
                    break;
                case 'U':       // Universal time in culture dependent format.
                    // Universal time is always in Greogrian calendar.
                    //
                    // Change the Calendar to be Gregorian Calendar.
                    //
                    ddfi = (DynamicDateFormatInfo)ddfi.Clone();
                    //if (ddfi.Calendar.GetType() != typeof(GregorianCalendar))
                    {
                        //ddfi.Calendar = GregorianCalendar.GetDefaultInstance();
                    }
                    break;
            }
            format = GetRealFormat(format, ddfi);
            return (format);
        }

        internal const String RoundtripFormat = "yyyy'-'MM'-'dd'T'";//HH':'mm':'ss.fffffffK";
        internal const String RoundtripDateTimeUnfixed = "yyyy'-'MM'-'ddT";// HH':'mm':'ss zzz";

        internal static String GetRealFormat(String format, DynamicDateFormatInfo ddfi)
        {
            String realFormat = null;

            switch (format[0])
            {
                case 'd':       // Short Date
                    realFormat = ddfi.MiddleDatePattern;
                    break;
                case 'D':       // Long Date
                    realFormat = ddfi.LongDatePattern;
                    break;
                case 'f':       // Full (long date + short time)
                    realFormat = ddfi.LongDatePattern;// + " " + ddfi.;
                    break;
                case 'F':       // Full (long date + long time)
                    realFormat = ddfi.LongDatePattern; //FullDateTimePattern;
                    break;
                case 'g':       // General (short date + short time)
                    realFormat = ddfi.MiddleDatePattern;
                    break;
                case 'G':       // General (short date + long time)
                    realFormat = ddfi.MiddleDatePattern;
                    break;
                case 'm':
                case 'M':       // Month/Day Date
                    realFormat = ddfi.LongMonthDayPattern;
                    break;
                case 'o':
                case 'O':
                    realFormat = RoundtripFormat;
                    break;
                case 'r':
                case 'R':       // RFC 1123 Standard                    
                    realFormat = ddfi.RFC1123Pattern;
                    break;
                case 's':       // Sortable without Time Zone Info
                    //realFormat = ddfi.SortableDateTimePattern;
                    break;
                case 't':       // Short Time
                    //realFormat = ddfi.ShortTimePattern;
                    break;
                case 'T':       // Long Time
                    //realFormat = ddfi.LongTimePattern;
                    break;
                case 'u':       // Universal with Sortable format
                    //realFormat = ddfi.UniversalSortableDateTimePattern;
                    break;
                case 'U':       // Universal with Full (long date + long time) format
                    //realFormat = ddfi.FullDateTimePattern;
                    break;
                case 'y':
                case 'Y':       // Year/Month Date
                    realFormat = ddfi.LongYearMonthPattern;
                    break;
                default:
                    throw new FormatException("Format_InvalidString");
            }
            return (realFormat);
        }

        internal static String Format(DynamicDate dDate, string format, IFormatProvider provider)
        {
            DynamicDateFormatInfo ddfi = DynamicDateFormatInfo.GetInstance(dDate.ModelID, provider);
            return Format(dDate, format, ddfi);
        }

        internal static String Format(DynamicDate dDate, string format, DynamicDateFormatInfo ddfi)
        {
            if (dDate.IsOutOfTime)
                return "OutATime";

            if (format.Length == 1)
            {
                format = ExpandPredefinedFormat(format, ref ddfi);
            }

            if (format.StartsWith("_") && format.Length < 5)
            {
                format = DynamicDateFormatInfo.ExpandDynamicPredefinedFormat(format, ddfi);
            }

            return FormatCustomized(dDate, format, ddfi);
        }

        private static String FormatCustomized(DynamicDate dDate, string format, DynamicDateFormatInfo ddfi)
        {
            StringBuilder result = new StringBuilder();

            int i = 0;
            int tokenLen;

            while (i < format.Length)
            {
                char ch = format[i];
                int nextChar;
                switch (ch)
                {
                    /*case 'g':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        result.Append(ddfi.GetEraName(cal.GetEra(dateTime)));
                        break;*/
                    case 'd':
                        //
                        // tokenLen == 1 : Day of month as digits with no leading zero.
                        // tokenLen == 2 : Day of month as digits with leading zero for single-digit months.
                        // tokenLen == 3 : Day of week as a three-leter abbreviation.
                        // tokenLen >= 4 : Day of week as its full name.
                        //
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        if (tokenLen <= 2)
                        {
                            FormatDigits(result, dDate.DayNumber, tokenLen);
                        }
                        else
                        {
                            if (tokenLen == 3)
                                result.Append(dDate.WeekDayNameShort);
                            else
                                result.Append(dDate.WeekDayNameFull);
                        }
                        break;
                    case 'D':
                        //
                        // tokenLen == 1 : Day of month as digits with no leading zero.
                        // tokenLen == 2 : Day of month as digits with leading zero for single-digit months.
                        // tokenLen == 3 : Day of month as its short name.
                        // tokenLen >= 4 : Day of week as its full name.
                        //
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        if (tokenLen <= 2)
                        {
                            FormatDigits(result, dDate.DayNumber, tokenLen);
                        }
                        else
                        {
                            if (tokenLen == 3)
                                result.Append(dDate.MonthDayNameShort);
                            else
                                result.Append(dDate.MonthDayNameFull);
                        }
                        break;
                    case 'M':
                        // 
                        // tokenLen == 1 : Month as digits with no leading zero.
                        // tokenLen == 2 : Month as digits with leading zero for single-digit months.
                        // tokenLen == 3 : Month as a three-letter abbreviation.
                        // tokenLen >= 4 : Month as its full name.
                        //
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        if (tokenLen <= 2)
                        {
                            FormatDigits(result, dDate.MonthNumber, tokenLen);
                        }
                        else
                        {
                            if (tokenLen == 3)
                                result.Append(dDate.MonthNameShort);
                            else
                                result.Append(dDate.MonthNameFull);
                        }
                        break;
                    case 'y':
                        //Info aus der DateTyme-Klasse und die letzte Zeile klingt falsch..
                        // Notes about OS behavior:
                        // y: Always print (year % 100). No leading zero.
                        // yy: Always print (year % 100) with leading zero.
                        // yyy: Always print (year % 1000) with leading zero.
                        // yyyy/yyyyy/... : Print year value. No leading zero.

                        int year = dDate.YearNumber;
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        int iX = ddfi.OverrideMaxYearDigits > 0 ? Math.Min(tokenLen, ddfi.OverrideMaxYearDigits) : tokenLen;
                        if (iX <= 2)
                        {
                            FormatDigits(result, year % 100, iX);
                        }
                        else if (iX == 3)
                        {
                            FormatDigits(result, year % 1000, iX);
                        }
                        else
                        {
                            String fmtPattern = "D" + iX;
                            result.Append(year.ToString(fmtPattern, CultureInfo.InvariantCulture));
                        }
                        break;
                    case ':':
                        //result.Append(ddfi.TimeSeparator);
                        tokenLen = 1;
                        break;
                    case '/':
                        result.Append(ddfi.DateSeparator);
                        tokenLen = 1;
                        break;
                    /*case '|':
                        tokenLen = 1;
                        nextChar = ParseNextChar(format, i);
                        if (nextChar == ch)
                        {
                            tokenLen = 2;
                            result.Append(DynamicDate.FormatLBMode ? "\n" : ", ");
                        } else
                            result.Append(DynamicDate.FormatLBMode ? '\n' : ' ');
                        break;*/
                    case '\'':
                    case '\"':
                        StringBuilder enquotedString = new StringBuilder();
                        tokenLen = ParseQuoteString(format, i, enquotedString);
                        result.Append(enquotedString);
                        break;
                    case '&':
                        enquotedString = new StringBuilder();
                        tokenLen = ParseQuoteString(format, i, enquotedString);
                        string c = "???";
                        switch (enquotedString.ToString())
                        {
                            case "MayaDate":
                                c = MayaCalc.Gregorian2Maya(dDate.UtcDate, dDate.Model.MayaCorrelation);
                                break;
                            case "MayaHaabKin":
                                c = MayaCalc.Gregorian2MayaHaabKin(dDate.UtcDate, dDate.Model.MayaCorrelation).ToString();
                                break;
                            case "MayaDateHaab":
                                c = MayaCalc.Gregorian2MayaHaab(dDate.UtcDate, dDate.Model.MayaCorrelation);
                                break;
                            case "MayaDateTzolkin":
                                c = MayaCalc.Gregorian2MayaTzolkin(dDate.UtcDate, dDate.Model.MayaCorrelation);
                                break;
                        }
                        result.Append(c);
                        break;
                    case '%':
                        // Optional format character.
                        // For example, format string "%d" will print day of month 
                        // without leading zero.  Most of the cases, "%" can be ignored.
                        nextChar = ParseNextChar(format, i);
                        // nextChar will be -1 if we already reach the end of the format string.
                        // Besides, we will not allow "%%" appear in the pattern.
                        if (nextChar >= 0 && nextChar != (int)'%')
                        {
                            result.Append(FormatCustomized(dDate, ((char)nextChar).ToString(), ddfi));
                            tokenLen = 2;
                        }
                        else
                        {
                            //
                            // This means that '%' is at the end of the format string or
                            // "%%" appears in the format string.
                            //
                            throw new FormatException("Format_InvalidString");
                        }
                        break;
                    case '\\':
                        // Escaped character.  Can be used to insert character into the format string.
                        // For exmple, "\d" will insert the character 'd' into the string.
                        //
                        // NOTENOTE : we can remove this format character if we enforce the enforced quote 
                        // character rule.
                        // That is, we ask everyone to use single quote or double quote to insert characters,
                        // then we can remove this character.
                        //
                        nextChar = ParseNextChar(format, i);
                        if (nextChar >= 0)
                        {
                            result.Append(((char)nextChar));
                            tokenLen = 2;
                        }
                        else
                        {
                            //
                            // This means that '\' is at the end of the formatting string.
                            //
                            throw new FormatException("Format_InvalidString");
                        }
                        break;
                    default:
                        // NOTENOTE : we can remove this rule if we enforce the enforced quote
                        // character rule.
                        // That is, if we ask everyone to use single quote or double quote to insert characters,
                        // then we can remove this default block.
                        result.Append(ch);
                        tokenLen = 1;
                        break;
                }
                i += tokenLen;
            }
            return result.ToString();
        }
    }
}