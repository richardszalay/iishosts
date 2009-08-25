using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace RichardSzalay.HostsFileExtension
{
    public class HostsFile
    {
        private string[] lines;
        public HostEntry[] entries;

        private const string AddressExpression = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";
        private const string EntryExpression = @"^(?'Enabled'\#)?\s*" +
            @"(?'Address'" + AddressExpression + @")" +
            @"(?'Spacer'\s+)" +
            @"(?'Hostname'\w+)\s*" +
            @"\#?\s*(?'Comment'.+)?$";

        private static readonly Regex lineRegex = new Regex(EntryExpression);

        public HostsFile(string filename)
            : this(File.Open(filename, FileMode.Open))
        {
            
        }

        public HostsFile(Stream stream)
        {
            
        }

        private void Load(Stream stream)
        {
            string[] lines = ReadAllLines(stream);

            this.entries = lines.Select(c => ParseHostEntry(c))
                .Where(c => c != null)
                .ToArray();
        }

        public void Save(string filename)
        {
        }

        internal void Save(Stream stream)
        {
            this.ApplyChanges();

            StreamWriter writer = new StreamWriter(stream);

            foreach (string line in this.lines)
            {
                writer.WriteLine(line);
            }
        }

        private void ApplyChanges()
        {
            List<string> newLines = new List<string>(lines);

            foreach (HostEntry entry in entries)
            {
                if (entry.IsDirty)
                {
                    if (entry.IsNew)
                    {
                        newLines.Add(entry.ToString());
                    }
                    else
                    {
                        newLines[entry.Line] = entry.ToString();
                    }
                }
            }

            lines = newLines.ToArray();
        }

        public IEnumerable<HostEntry> Entries
        {
            get { return entries; }
        }

        public bool IsDirty
        {
            get { return entries.Any(c => c.IsDirty); }
        }

        private HostEntry ParseHostEntry(int lineIndex, string line)
        {
            if (line.Length == 0)
            {
                return null;
            }

            Match match = lineRegex.Match(line);

            if (match == null || !match.Success)
            {
                return null;
            }

            bool enabled = match.Groups["Enabled"].Success;
            string address = match.Groups["Address"].Value;
            string spacer = match.Groups["Spacer"].Value;
            string hostname = match.Groups["Hostname"].Value;
            
            Group commentGroup = match.Groups["Comment"];
            
            string comment = (commentGroup != null && commentGroup.Success)
                ? commentGroup.Value
                : null;

            return new HostEntry(lineIndex, line, spacer, enabled, 
                hostname, address, comment);
        }



        private string[] ReadAllLines(Stream stream)
        {
            List<string> lines = new List<string>();

            StreamReader reader = new StreamReader(stream)
            
            string line = reader.ReadLine();

            while (line != null)
            {
                lines.Add(line);

                line = reader.ReadLine();
            }

            return lines.ToArray();
        }
    }
}
