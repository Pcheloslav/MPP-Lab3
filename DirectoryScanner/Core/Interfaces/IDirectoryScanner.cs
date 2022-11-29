using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IDirectoryScanner
    {
        FileTree Scan(string path, ushort maxThreadCount, Action<string> onScanStart);

        void Stop();

        public bool IsScanning { get; }
    }
}
