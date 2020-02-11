using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Awv.Lexica.Parsing
{
    public struct TimeConverter
    {
        /// <summary>
        /// [364d] [23h] [59m] [59s] [999ms]
        /// </summary>
        private static readonly Regex Time = new Regex(@"((\d{1,3})d)?\s*((\d{1,2})h)?\s*((\d{1,2})m)?\s*((\d{1,2})s)?\s*((\d{1,3})ms)?");
        /// <summary>
        /// Base millisecond function.
        /// </summary>
        private static readonly Func<TimeSpan, string> Milliseconds = ts => ((long)ts.TotalMilliseconds).ToString();
        /// <summary>
        /// Replaces timespans within the <paramref name="input"/> which match the <see cref="Time"/> <see cref="Regex"/> with a value provided by <paramref name="predicate"/>.
        /// </summary>
        /// <param name="input">Input string to check for timespans</param>
        /// <param name="predicate">What to replace the timespans with</param>
        /// <returns>The updated string</returns>
        public string ReplaceTimespans(string input, Func<TimeSpan, string> predicate = null)
            => ReplaceTimespans(input, 0, input.Length, predicate);

        /// <summary>
        /// Replaces timespans within the <paramref name="input"/> which match the <see cref="Time"/> <see cref="Regex"/> with a value provided by <paramref name="predicate"/>.
        /// </summary>
        /// <param name="input">Input string to check for timespans</param>
        /// <param name="startIndex">The start of the string to search through</param>
        /// <param name="length">The length of the string to search through</param>
        /// <param name="predicate">What to replace the timespans with</param>
        /// <returns>The updated string</returns>
        public string ReplaceTimespans(string input, int startIndex, int length, Func<TimeSpan, string> process = null)
        {
            if (process == null)
                process = Milliseconds;
            var substr = input.Substring(startIndex, length);
            var sub = new StringBuilder();

            var matches = Time.Matches(substr);
            if (matches.Count > 0)
            {
                var lastEnd = 0;

                foreach (Match match in matches)
                {
                    sub.Append(substr.Substring(lastEnd, match.Index - lastEnd));
                    lastEnd = match.Index + match.Length;
                    if (!string.IsNullOrWhiteSpace(match.Value))
                    {
                        var span = ParseTimeSpan(match.Value);
                        sub.Append(process(span));
                    } else
                    {
                        sub.Append(match.Value);
                    }
                }
            } else
            {
                sub.Append(substr);
            }


            return $"{input.Substring(0, startIndex)}{sub}{input.Substring(startIndex + length)}";
        }

        /// <summary>
        /// Parses a timespan string and converts it to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="source">Timespan string</param>
        /// <returns>TimeSpan representation of the string</returns>
        public TimeSpan ParseTimeSpan(string source)
        {
            var match = Time.Match(source);
            var ts = new TimeSpan();

            int val;

            if (int.TryParse(match.Groups[2].Value, out val)) ts += TimeSpan.FromDays(val);
            if (int.TryParse(match.Groups[4].Value, out val)) ts += TimeSpan.FromHours(val);
            if (int.TryParse(match.Groups[6].Value, out val)) ts += TimeSpan.FromMinutes(val);
            if (int.TryParse(match.Groups[8].Value, out val)) ts += TimeSpan.FromSeconds(val);
            if (int.TryParse(match.Groups[10].Value, out val)) ts += TimeSpan.FromMilliseconds(val);

            return ts;
        }
    }
}
