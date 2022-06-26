using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CC_log_compare
{
    internal class CC_log_compare
    {
        public string filePath { get; private set; }
        public List<string> fileContents { get; private set; } = new List<string>();
        public CC_log_compare(string _filePath)
        {
            filePath = _filePath;
        }

        public List<string> readFile()
        { 
            foreach (string line in File.ReadLines(filePath))
            {
                fileContents.Add(line);
            }
            return fileContents;
        }

        public void writeFile(List<string> contents)
        {
            filePath += ".xxx";
            File.WriteAllLines(filePath, contents);
        }
        public List<string> parseContents(List<string> contents)
        {
            List<string> result = new List<string>();
            DateTime dateResult;
            DateTime? startTime = null;
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            var styles = DateTimeStyles.None;
            foreach (string line in contents)
            {
                string[] sep = { " : " };
                string[] parts = line.Split(sep, System.StringSplitOptions.None);
                if (parts.Length > 1 && DateTime.TryParse(parts[0], culture, styles, out dateResult))
                {
                    if (startTime == null)
                    {
                        startTime = dateResult;
                    }
                    TimeSpan newDate = (TimeSpan)(dateResult - startTime);
                    result.Add($"{newDate.Hours:D2}:{newDate.Minutes:D2}:{newDate.Seconds:D2} : {parts[1]}");
                }
                else
                {
                    result.Add(line);
                }
            }
            return result;
        }
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var obj = new CC_log_compare(args[0]);
                var contents = obj.readFile();
                var result = obj.parseContents(contents);
                obj.writeFile(result);
            }
        }
    }
}
