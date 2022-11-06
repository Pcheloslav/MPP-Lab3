using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class FileTree
    {
        public Node Root { get; }
        public FileTree(Node root)
        {
            Root = root;
            Root.Length = GetLength(root);
        }

        private long GetLength(Node node)
        {
            if (node.Children == null)
            {
                return node.Length;
            }

            foreach (var child in node.Children)
            {
                node.Length += GetLength(child);
            }
            return node.Length;
        }
    }
}

