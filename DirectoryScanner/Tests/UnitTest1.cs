using Core.Interfaces;
using Core.Models;
using NUnit.Framework;
using Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Tests
{
    public class Tests
    {
        private IDirectoryScanner _scanner;

        [SetUp]
        public void Setup()
        {
            _scanner = new DirectoryScanner();
        }

        [Test]
        public void InvalidPathTest()
        {
            Assert.Catch<ArgumentException>(() =>
            {
                _scanner.Scan(".invalidPath", 5);
            });
        }

        [Test]
        public void InvalidThreadCount()
        {
            Assert.Catch<ArgumentException>(() =>
            {
                _scanner.Scan("C:/", 0);
            });
        }

        [Test]
        public void ScanResultTest()
        {
            string rootTestPath = "../../../TestDir";
            string rootDirName = "TestDir";
            string dirName = "dir";
            string fileName = "file";

            if (!Directory.Exists(rootTestPath))
            {
                Directory.CreateDirectory(rootTestPath);
            }

            for (int i = 0; i < 3; i++)
            {
                string subdirName = rootTestPath + "/" + dirName + i.ToString() + "/";

                if (!Directory.Exists(subdirName))
                {
                    Directory.CreateDirectory(subdirName);
                }

                string subFileName = subdirName + "/" + fileName + i.ToString();

                if (!File.Exists(subFileName))
                {
                    File.Create(subFileName);
                }
            }

            FileTree result = _scanner.Scan(rootTestPath, 5);

            Assert.Multiple(() =>
            {
                Assert.That(result.RootNode.Name, Is.EqualTo(rootDirName));

                Assert.NotNull(result.RootNode.Children);

                Node[] children = result.RootNode.Children!.ToArray();

                Assert.That(children.Length, Is.EqualTo(3));

                for (int i = 0; i < children.Length; i++)
                {
                    Assert.That(children[i].Name, Is.EqualTo(dirName + i.ToString()));
                    Assert.NotNull(children[i].Children);
                    Assert.That(children[i].Children![0].Name, Is.EqualTo(fileName + i.ToString()));
                }
            });
        }

        [Test]
        public void StopScanningTest()
        {
            string dirPath = "C:\\Windows";
            ushort threadCount = 50;



            FileTree? resultFull = null;
            FileTree? resultStopped = null;

            var fullTask = Task.Run(() =>
            {
                IDirectoryScanner scanner = new DirectoryScanner();
                resultFull = scanner.Scan(dirPath, threadCount);
            });

            var stoppedTask = Task.Run(() =>
            {
                resultStopped = _scanner.Scan(dirPath, threadCount);
            });

            Thread.Sleep(250);
            _scanner.Stop();

            Task.WaitAll(fullTask, stoppedTask);

            Assert.Multiple(() =>
            {
                Assert.That(resultStopped.RootNode.Size, Is.LessThanOrEqualTo(resultFull.RootNode.Size));
            });
        }
    }
}