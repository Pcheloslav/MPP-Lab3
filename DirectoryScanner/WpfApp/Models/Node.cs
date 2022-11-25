using System.Collections.ObjectModel;

namespace WpfApp.Models
{
    public class Node
    {
        public string Name { get; }
        public long Size { get; }
        public double SizeInPercent { get; }
        public bool IsDirectory { get; }
        public ObservableCollection<Node>? Children { get; internal set; }
        public string IcoPath { get; }
        public Node(string name, long length, double sizeInPercent, bool isDirectory = false, ObservableCollection<Node>? children = null)
        {
            Name = name;
            Size = length;
            SizeInPercent = sizeInPercent;
            IsDirectory = isDirectory;
            Children = children;
            IcoPath = IsDirectory ? "Resources/folder.png" : "Resources/file.png";
        }
    }
}