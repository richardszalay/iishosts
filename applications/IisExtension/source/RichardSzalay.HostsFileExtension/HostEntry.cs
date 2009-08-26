﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension
{
    public class HostEntry : IEquatable<HostEntry>
    {
        private const string CommentPrefix = "# ";

        private const string DefaultSpacer = "\t";

        private bool isDirty = false;

        private int line;
        private string originalLine;
        private string spacer;

        private bool enabled;
        private string hostname;
        private string address;
        private string comment;

        public HostEntry(string hostname, string address, string comment)
            : this(-1, null, DefaultSpacer, true, hostname, address, comment)
        {
        }

        internal HostEntry(int line, string originalLine, string spacer, bool enabled, string hostname, string address, string comment)
        {
            this.line = line;
            this.originalLine = originalLine;
            this.spacer = spacer;

            this.enabled = enabled;
            this.hostname = hostname;
            this.address = address;
            this.comment = comment;
        }

        public int Line
        {
            get { return line; }
            private set
            {
                if (line != value)
                {
                    line = value;
                    isDirty = true;
                }
            }
        }

        public string Hostname
        {
            get { return hostname; }
            set
            {
                if (hostname != value)
                {
                    hostname = value;
                    isDirty = true;
                }
            }
        }

        public string Address
        {
            get { return address; }
            set
            {
                if (address != value)
                {
                    address = value;
                    isDirty = true;
                }
            }
        }

        public string Comment
        {
            get { return comment; }
            set
            {
                if (comment != value)
                {
                    comment = value;
                    isDirty = true;
                }
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    isDirty = true;
                }
            }
        }

        public void SwapLine(HostEntry other)
        {
            int otherLine = other.Line;

            other.Line = this.Line;
            this.Line = otherLine;
        }

        public bool IsDirty
        {
            get { return isDirty || IsNew || String.IsNullOrEmpty(originalLine); }
        }

        public bool IsNew
        {
            get { return Line == -1; }
        }

        public override string ToString()
        {
            if (this.IsDirty)
            {
                StringBuilder sb = new StringBuilder(); // TODO: estimate size?

                if (!enabled)
                {
                    sb.Append(CommentPrefix);
                }

                sb.Append(address);
                sb.Append(spacer);
                sb.Append(hostname);

                if (!String.IsNullOrEmpty(comment))
                {
                    sb.Append(" ");
                    sb.Append(CommentPrefix);
                    sb.Append(comment);
                }

                return sb.ToString();
            }
            else
            {
                return originalLine;
            }
        }

        #region IEquatable<HostEntry> Members

        public bool Equals(HostEntry other)
        {
            return other.Line == this.Line &&
                other.originalLine == this.originalLine &&
                other.enabled == this.enabled &&
                other.isDirty == this.isDirty &&
                other.hostname == this.hostname &&
                other.address == this.address &&
                other.comment == this.comment;
        }

        #endregion

        public static bool IsIgnoredHostname(string hostname)
        {
            return ((IList<string>)IgnoredHostnames)
                .Contains(hostname.ToLowerInvariant());
        }

        public static readonly string[] IgnoredHostnames = new string[]
        {
            "rhino.acme.com", "x.acme.com", "localhost"
        };
    }
}
