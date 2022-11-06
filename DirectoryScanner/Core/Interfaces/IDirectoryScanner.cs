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
        FileTree StartScan(string path, ushort maxThreadCount);

        void StopScan();

        public bool IsScanning { get; }
    }
}
