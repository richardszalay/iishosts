﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RichardSzalay.HostsFileExtension
{
    internal class FileInfoResource : IResource
    {
        private FileInfo file;

        public FileInfoResource(string filename)
            : this(new FileInfo(Environment.ExpandEnvironmentVariables(filename)))
        {
        }

        public FileInfoResource(FileInfo file)
        {
            this.file = file;
        }

        #region IResource Members

        public Stream OpenRead()
        {
            return this.file.OpenRead();
        }

        public Stream OpenWrite()
        {
            this.file.Delete();

            return this.file.OpenWrite();
        }

        #endregion
    }
}
