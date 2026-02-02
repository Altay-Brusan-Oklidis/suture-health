using OfficeOpenXml;
using SutureHealth.Hchb.Services.Testing.Model.Branch;
using SutureHealth.Hchb.Services.Testing.Model.Patient;
using SutureHealth.Hchb.Services.Testing.Model.Visit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Utility
{
    public static class Utilities
    {
        private static Random random = new Random();
        private static int sequenceNumber = 1;

        private static char GenerateRandomChar()
        {
            var random = new Random();
            string dictionaryString = "ACDEFGHJKMNPQRTUVWXY";
            return dictionaryString.ElementAt(random.Next(0, dictionaryString.Length));
        }
        private static char GenerateRandomNumber()
        {
            var random = new Random();
            string dictionaryString = "123456789";
            return dictionaryString.ElementAt(random.Next(0, dictionaryString.Length));
        }

        public static string GetRandomMBI()
        {                      //CAaN AaN AANN
                               //1EG4-TE5-MK73
            List<char> result = "0123-456-7891".ToList();
            foreach (var item in new List<int>() { 1, 5, 9, 10 })
            {
                result[item] = GenerateRandomChar();
            }

            foreach (var item in new List<int>() { 0, 3, 7, 11, 12 })
            {
                result[item] = GenerateRandomNumber();
            }
            Random rand = new Random();
            result[2] = (new List<char>() { 'A', 'N' })[rand.Next(0, 2)];
            result[6] = (new List<char>() { 'A', 'N' })[rand.Next(0, 2)];

            string output = new string(result.ToArray());
            return output;
        }


        public static string GetSequenceNumber()
        {
            if (sequenceNumber == 9999)
                sequenceNumber = 0;
            string sequence = sequenceNumber.ToString();
            sequenceNumber++;
            return sequence;
        }
        public static string GetGuid(int? length = null)
        {
            string guid = Guid.NewGuid().ToString();
            if (length == null)
                return guid;
            else
                return guid.Substring(0, length.Value);
        }
        public static string GetRandomDecimalString(int length, int startIndex = 0, int endIndex = 10)
        {
            string chars = "0123456789".Substring(startIndex, endIndex);

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GetRandomAlphabeticString(int length, int startIndex = 0, int endIndex = 25)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(0, 26);


            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GetRandomalphabeticString(int length, int startIndex = 0, int endIndex = 25)
        {
            string chars = "abcdefghigklmnopqrstuvwxyz".Substring(0, 26);

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static T? GetRandomEnumElement<T>() where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            Random gen = new Random();
            var item = values.GetValue(gen.Next(values.Length));
            return ((T?)item);
        }
        public static string? GetRandomProperyValue<T>() where T : class
        {
            Random rnd = new Random();
            var propertiesList = typeof(T).GetProperties();
            var value = (string?)propertiesList[rnd.Next(0, propertiesList.Count())].GetValue(null, null);
            return value;
        }

        public static DateTime GetRandomDateTime(DateTime? startDateTime = null)
        {
            DateTime start;
            if (startDateTime == null)
                start = new DateTime(1920, 1, 1);
            else
                start = startDateTime.Value;

            Random gen = new Random();
            int range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }

        public static string GetRandomFormattedPhoneNumber()
        {
            Random gen = new Random();
            string result = "(" + gen.Next(201, 990) + ")" + gen.Next(1, 999).ToString("D3") + "-" + gen.Next(0, 9999).ToString("D4");
            return result;
        }

        public static string UpToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMdd");
        }
        public static string UpToMinuteString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmm");
        }
        public static string UptoSecondsString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmss");
        }
        public static string GetRandomSSN()
        {
            Random gen = new Random();
            string result = gen.Next(100, 999).ToString() + "-" + gen.Next(1, 99).ToString("00") + "-" + gen.Next(1, 9999).ToString("0000");
            return result;
        }

        public static string GetRandomDeathIndicator()
        {
            Random gen = new Random();
            return gen.Next(0, 10) > 5 ? "Y" : "N";
        }

        public static string GetRandomNameOrFamilyName(string type)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string path;
            if (String.Equals(type, "FirstName", StringComparison.OrdinalIgnoreCase))
                path = Path.Combine(currentDirectory, "Data", "FirstNameDataSet.txt");
            else if (String.Equals(type, "LastName", StringComparison.OrdinalIgnoreCase))
                path = Path.Combine(currentDirectory, "Data", "LastNameDataSet.txt");
            else
                return string.Empty;

            string result;
            try
            {
                var lines = File.ReadAllLines(path);
                Random gen = new Random();
                result = lines[gen.Next(0, lines.Count() - 1)];
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        

        public static Model.Branch.Branch GetShortListRandomBranch()
        {
            List<Model.Branch.Branch>? branchList = new();
            branchList = new List<Model.Branch.Branch>()
                {
                    new Model.Branch.Branch()
                    {
                        Id = 1,
                        Name = "054",
                        Code = "SutureTest Home Health - St. Pete",
                        FacilityId = 12264,
                    },
                    new Model.Branch.Branch()
                    {
                        Id = 2,
                        Name = "0E4",
                        Code = "External Name - Agency Network Floater",
                        FacilityId = 13359,
                    },
                    new Model.Branch.Branch()
                    {
                        Id = 3,
                        Name = "8V1",
                        Code = "Harbor Hospice",
                        FacilityId = 13365
                    },
                    new Model.Branch.Branch()
                    {
                        Id = 4,
                        Name = "040",
                        Code = "Dentist Agency Senders",
                        FacilityId = 13504
                    }
                };

            Random rnd = new Random(DateTime.Now.Millisecond);
            return branchList[rnd.Next(0, branchList.Count)];
        }
    }
}
