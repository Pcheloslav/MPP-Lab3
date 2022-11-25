using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Node
    {
        public string FullName { get; }
        public string Name { get; }
        public long Size { get; set; }
        public List<Node>? Children { get; set; }
        public bool IsDir { get; }

        public Node(string fullName, string name, bool isDir = false)
        {
            FullName = fullName;
            Name = name;
            IsDir = isDir;
        }

        public Node(string fullName, string name, long size) : this(fullName, name)
        {
            Size = size;
        }

        public Node(string fullName, string name, List<Node>? children) : this(fullName, name, true)
        {
            Children = children;
        }
    }
}
