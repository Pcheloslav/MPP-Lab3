using System.Collections.ObjectModel;

namespace WpfApp.Models
{
    public class FileTree
    {
        public Node Root { get; }

        public FileTree(Core.Models.FileTree tree)
        {
            var rootNode = tree.RootNode;
            Root = new Node(rootNode.Name, rootNode.Size, 0, rootNode.IsDir);
            if (tree.RootNode.Children != null)
            {
                SetChilds(rootNode, Root);
            }
        }

        private void SetChilds(Core.Models.Node node, Node dtoNode)
        {
            if (node.Children != null)
            {
                dtoNode.Children = new ObservableCollection<Node>();
                foreach (var child in node.Children)
                {
                    double sizeInPercent = node.Size == 0 ? 0 : (double)child.Size / (double)node.Size * 100;

                    Node newNode = new(child.Name, child.Size, sizeInPercent, child.IsDir);
                    if (child.Children != null)
                    {
                        SetChilds(child, newNode);
                    }
                    dtoNode.Children.Add(newNode);
                }
            }
        }
    }
}