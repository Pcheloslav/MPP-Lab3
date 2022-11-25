using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Services;
using Core.Interfaces;
using WpfApp.Commands;
using WpfApp.Models;

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
                    DirectoryPath = folderBrowserDialog.SelectedPath;
                    Tree = null;
                }
            });

            ScanCommand = new RelayCommand(_ =>
            {
                Task.Run(() =>
                {
                    IsScanning = true;
                    Core.Models.FileTree result = _scanner.Scan(DirectoryPath, MaxThreadCount);
                    IsScanning = false;
                    Tree = new FileTree(result);
                });

            }, _ => _directoryPath != null && !IsScanning);

            StopCommand = new RelayCommand(_ =>
            {
                _scanner.Stop();
                IsScanning = false;
            }, _ => IsScanning);
        }

        private string? _directoryPath;
        public string? DirectoryPath
        {
            get { return _directoryPath; }
            set
            {
                _directoryPath = value;
                OnPropertyChanged("DirectoryPath");
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
            }
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