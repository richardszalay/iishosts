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
            : this(new FileInfo(filename))
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
            return this.file.OpenWrite();
        }

        #endregion
    }
}
