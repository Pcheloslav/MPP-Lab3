using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class FileTree
    {
        public Node RootNode { get; }
        public FileTree(Node root)
        {
            RootNode = root;
            RootNode.Size = GetDirSize(root);
        }

        private long GetDirSize(Node node)
        {
            if (node.Children == null)
            {
                return node.Size;
            }

            foreach (var child in node.Children)
            {
                node.Size += GetDirSize(child);
            }
            return node.Size;
        }
    }
}

