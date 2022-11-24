﻿using System;
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
            Root.Size = GetDirectorySize(root);
        }

        private long GetDirectorySize(Node node)
        {
            if (node.Children == null)
            {
                return node.Size;
            }

            foreach (var child in node.Children)
            {
                node.Size += GetDirectorySize(child);
            }
            return node.Size;
        }
    }
}
