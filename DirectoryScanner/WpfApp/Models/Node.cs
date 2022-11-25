using System.Collections.ObjectModel;

namespace WpfApp.Models
{
    public class Node
    {
        public string Name { get; }
        public long Size { get; }
        public double SizeInPercent { get; }
        public ObservableCollection<Node>? Children { get; internal set; }
        public string IconPath { get; }
        public bool IsDir { get; }
        public Node(string name, long length, double sizeInPercent, bool isDir = false, ObservableCollection<Node>? children = null)
        {
            Name = name;
            Size = length;
            IsDir = isDir;
            Children = children;
            SizeInPercent = sizeInPercent;
            IconPath = IsDir ? "Resources/folder.png" : "Resources/file.png";
        }
    }
}