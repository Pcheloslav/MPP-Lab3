using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Services;
using Core.Interfaces;
using WpfApp.Commands;
using WpfApp.Models;
using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace WpfApp.ViewModel
{
    public class AppViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly IDirectoryScanner _scanner = new DirectoryScanner();

        public RelayCommand ChooseDirCommand { get; }
        public RelayCommand ScanCommand { get; }
        public RelayCommand StopCommand { get; }

        public AppViewModel()
        {
            ChooseDirCommand = new RelayCommand(_ =>
            {
                using var folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    DirPath = folderBrowserDialog.SelectedPath;
                    Tree = null;
                }
            });

            ScanCommand = new RelayCommand(_ =>
            {
                Task.Run(() =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                        new Action<string>((_) => Tree = null), null);
                    var onScanStartAction = new Action<string>(currentDir => {
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                            new Action<string>((_) => CurrentDir = currentDir), null);
                    });

                    IsScanning = true;
                    var result = _scanner.Scan(DirPath, MaxThreadCount, onScanStartAction);
                    Tree = new FileTree(result);
                    IsScanning = false;
                });

            }, _ => _DirPath != null && !IsScanning);

            StopCommand = new RelayCommand(_ =>
            {
                _scanner.Stop();
                IsScanning = false;
            }, _ => IsScanning);
        }

        private string? _DirPath;
        public string? DirPath
        {
            get { return _DirPath; }
            set
            {
                _DirPath = value;
                OnPropertyChanged("DirPath");
            }
        }

        private string? _currentDir = "";
        public string? CurrentDir
        {
            get { return _currentDir; }
            set
            {
                _currentDir = value;
                OnPropertyChanged("CurrentDir");
            }
        }

        private ushort _maxThreadCount = 100;
        public ushort MaxThreadCount
        {
            get { return _maxThreadCount; }
            set
            {
                _maxThreadCount = value;
                OnPropertyChanged("MaxThreadCount");
            }
        }

        private FileTree? _tree;
        public FileTree? Tree
        {
            get { return _tree; }
            private set
            {
                _tree = value;
                OnPropertyChanged("Tree");
            }

        }

        private volatile bool _isScanning = false;
        public bool IsScanning
        {
            get { return _isScanning; }
            private set
            {
                _isScanning = value;
                OnPropertyChanged("IsScanning");
                OnPropertyChanged("IsNotScanning");
            }
        }

        public bool IsNotScanning
        {
            get { return !_isScanning; }
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
