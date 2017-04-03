using System;
using System.Text.RegularExpressions;
using System.Collections;

namespace Reusable
{
    public class Node<T>
    {
        public Node(T value, int depth)
        {
            Value = value;
            Depth = depth;
        }

        public T Value { get; }

        public int Depth { get; }

        public static implicit operator T(Node<T> node) => node.Value;
    }    
}