using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia
{
    public abstract class Node
    {
        public readonly Node? Parent;
        public readonly List<Node> Children = new List<Node>();

        public Node(Node? parent)
        {
            Parent = parent;
            Parent?.Children.Add(this);
        }

        public IEnumerable<Node> Descendants ()
        {
            foreach (Node child in Children)
            {
                yield return child;
                foreach (Node descendant in child.Descendants())
                    yield return descendant;
            }
        }
    }

    public abstract class ChildNode : Node
    {
        public new Node Parent => base.Parent!;

        public ChildNode(Node parent) : base(parent) { }
    }

    public class EntryInfo : Node
    {
        public EntryInfo() : base(null) { }
    }

    public class Heading : ChildNode
    {
        public int Level;
        public string Text;

        public Heading(Node parent) : base(parent) { }
    }
}
