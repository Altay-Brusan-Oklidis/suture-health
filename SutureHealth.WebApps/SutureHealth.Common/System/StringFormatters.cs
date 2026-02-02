using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringFormatters
    {
        public static string ToPascalCase(this string source)
        {
            try
            {
                var words = source.Split(' ');
                var pascalWords = words.Where(x => string.IsNullOrEmpty(x) == false).Select(x => x[0] + x.Substring(1).ToLower()).ToArray();

                for (var j = 0; j < pascalWords.Length; j++)
                {
                    if (j > 0)
                    {
                        if (new[] { "the", "of", "a", "at", "in", "and", "to" }.Contains(pascalWords[j], StringComparer.OrdinalIgnoreCase))
                        {
                            pascalWords[j] = pascalWords[j].ToLower();
                        }
                    }
                    if (pascalWords[j].Contains("-") && pascalWords[j].Length > 1)
                    {
                        pascalWords[j] = string.Join("-", pascalWords[j].Split('-').Select(x => x[0].ToString().ToUpper() + x.Substring(1)));
                    }

                    var prefix = new[] { "Mc", "O'" }.FirstOrDefault(x => pascalWords[j].StartsWith(x));
                    if (prefix != null)
                    {
                        var restOfWord = pascalWords[j].Replace(prefix, string.Empty);
                        pascalWords[j] = $"{prefix}{restOfWord[0].ToString().ToUpper()}{restOfWord.Substring(1)}";
                    }

                    var acronyms = new[] { "LLC", "PA", "LLP", "PC", "P.C.", "PSC" }.ToList();
                    var directions = new[] { "N", "S", "E", "W" };
                    directions = directions.SelectMany(d => directions.Select(d2 => d + d2)).ToArray();
                    acronyms.AddRange(directions);
                    acronyms.AddRange(States.Select(x => x.Key));
                    if (
                        acronyms.Contains(pascalWords[j], StringComparer.OrdinalIgnoreCase)
                        || (pascalWords[j].Contains(".") && pascalWords[j].Split('.').All(x => x.Length <= 1))
                        || (pascalWords[j] == "a" && j == pascalWords.Length - 1)
                        || (pascalWords[j].ToUpper().ToCharArray().All(c => new[] { 'X', 'I', 'V' }.Contains(c)))
                        )
                    {
                        pascalWords[j] = pascalWords[j].ToUpper();
                    }
                }

                return string.Join(" ", pascalWords);
            }
            catch
            {
                return source;
            }
        }


        public static readonly IDictionary<string, string> States = new Dictionary<string, string>
{
        { "AL", "Alabama" },
        { "AK", "Alaska" },
        { "AZ", "Arizona" },
        { "AR", "Arkansas" },
        { "CA", "California" },
        { "CO", "Colorado" },
        { "CT", "Connecticut" },
        { "DE", "Delaware" },
        { "DC", "District of Columbia" },
        { "FL", "Florida" },
        { "GA", "Georgia" },
        { "HI", "Hawaii" },
        { "ID", "Idaho" },
        { "IL", "Illinois" },
        { "IN", "Indiana" },
        { "IA", "Iowa" },
        { "KS", "Kansas" },
        { "KY", "Kentucky" },
        { "LA", "Louisiana" },
        { "ME", "Maine" },
        { "MD", "Maryland" },
        { "MA", "Massachusetts" },
        { "MI", "Michigan" },
        { "MN", "Minnesota" },
        { "MS", "Mississippi" },
        { "MO", "Missouri" },
        { "MT", "Montana" },
        { "NE", "Nebraska" },
        { "NV", "Nevada" },
        { "NH", "New Hampshire" },
        { "NJ", "New Jersey" },
        { "NM", "New Mexico" },
        { "NY", "New York" },
        { "NC", "North Carolina" },
        { "ND", "North Dakota" },
        { "OH", "Ohio" },
        { "OK", "Oklahoma" },
        { "OR", "Oregon" },
        { "PA", "Pennsylvania" },
        { "RI", "Rhode Island" },
        { "SC", "South Carolina" },
        { "SD", "South Dakota" },
        { "TN", "Tennessee" },
        { "TX", "Texas" },
        { "UT", "Utah" },
        { "VT", "Vermont" },
        { "VA", "Virginia" },
        { "WA", "Washington" },
        { "WV", "West Virginia" },
        { "WI", "Wisconsin" },
        { "WY", "Wyoming" }
    };

        static readonly System.Text.RegularExpressions.Regex phone_formatter = new(@"^\s*((\+?((?<Country>1|))\s*(\(?\s*((?<Area>\d{3}))\s*\)?)) | (\(?\s*((?<Area>\d{3}))\s*\)?))?\s*((?<Exchange>\d{3}))\s*-?\s*((?<Line>\d{4}))([^0-9]+((?<Extension>\d{1,10})))?\s*$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
        static readonly System.Text.RegularExpressions.Regex numbers_only_formatter = new(@"[^0-9]+", RegexOptions.Compiled);

        public static string RemoveEverythingButNumbers(this string @string)
            => numbers_only_formatter.Replace(@string, "");

        public static string ToFormattedPhoneNumber(this string phoneNumber) => (string.IsNullOrWhiteSpace(phoneNumber)) ? phoneNumber : phone_formatter.Replace(phoneNumber, m =>
        {
            try
            {
                var buffer = new System.Text.StringBuilder();

                if (m.Groups["Country"].Success && !string.IsNullOrWhiteSpace(m.Groups["Country"].Value)) buffer.Append($"+{m.Groups["Country"].Value} ");
                if (m.Groups["Area"].Success && !string.IsNullOrWhiteSpace(m.Groups["Area"].Value)) buffer.Append($"({m.Groups["Area"].Value}) ");
                buffer.Append($"{m.Groups["Exchange"].Value}-{m.Groups["Line"].Value}");
                if (m.Groups["Extension"].Success && !string.IsNullOrWhiteSpace(m.Groups["Extension"].Value)) buffer.Append($" x {m.Groups["Extension"]}");

                return buffer.ToString();

            }
            catch (Exception)
            {

                return phoneNumber;
            }
        });

        public static string SanitatePhoneNumber(this string phoneNumber) => (string.IsNullOrWhiteSpace(phoneNumber)) ? phoneNumber : phone_formatter.Replace(phoneNumber, m =>
        {
            try
            {
                return phoneNumber?.Replace("(", "").Replace(" ", "").Replace("-", "").Replace(")", "").Trim();
            }
            catch (Exception)
            {

                return phoneNumber;
            }
        });

        public static string ToFormattedZip(this string source)
        {
            if (source != null)
            {
                var newSource = source.Replace(" ", "");
                if (newSource.Length == 9)
                {
                    try
                    {
                        int newZip = int.Parse(newSource);

                        var formattedZip = string.Format("{0:00000-0000}", newZip);
                        return formattedZip;
                    }
                    catch (Exception ex)
                    {
#if !NETSTANDARD1_1
                        Console.WriteLine(ex.Message);
#endif
                        return newSource;
                    }
                }
                else
                {
                    return source;
                }
            }
            return source;
        }

        public static string ToFormattedSSN(this string source)
        {
            if (source != null)
            {
                var newSource = source.Replace(" ", "");
                if (newSource.Length == 9)
                {
                    try
                    {
                        int newSSN = int.Parse(newSource);

                        var formattedSSN = string.Format("{0:000-00-0000}", newSSN);
                        return formattedSSN;
                    }
                    catch (Exception ex)
                    {
#if !NETSTANDARD1_1
                        Console.WriteLine(ex.Message);
#endif
                        return source;
                    }
                }
                else if (newSource.Length == 4)
                {
                    return $"###-##-{newSource}";
                }
                else
                {
                    return source;
                }
            }
            return source;
        }
        public static string ToFormattedGenericPhoneNumber(this string source)
        {
            if (source != null)
            {
                source = source.Trim().Insert(3, "-").Insert(7, "-");

            }
            return source;
        }

        public static string ToFormmatedSSNForGrid(this string source)
        {
            if (source != null)
            {
                var newSource = source.Replace(" ", "");
                if (newSource.Length == 9)
                {
                    try
                    {
                        int newSSN = int.Parse(newSource);

                        var formattedSSN = string.Format("{0:000-00-0000}", newSSN);
                        return formattedSSN;
                    }
                    catch (Exception ex)
                    {
#if !NETSTANDARD1_1
                        Console.WriteLine(ex.Message);
#endif
                        return source;
                    }
                }
                else if (newSource.Length == 4)
                {
                    return $"***-**-{newSource}";
                }
                else
                {
                    return source;
                }
            }
            return source;
        }

        public static string ToFormattedMBI(this string source)
        {
            var pattern = @"^([A-Za-z0-9]{4})([A-Za-z0-9]{3})([A-Za-z0-9]{4})$";
            if (source != null)
            {
                if (Regex.IsMatch(source, pattern))
                {
                    return Regex.Replace(source, pattern, "$1-$2-$3").ToUpper();
                }
            }

            return source;
        }
    }
}
