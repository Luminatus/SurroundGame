using System.Collections.Generic;

namespace SurroundGameWPF.Extensions
{

    public static class LinkedListNodeExtensions
    {

        public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> node)
        {
            if (node == node.List.Last)
            {
                return node.List.First;
            }
            else
            {
                return node.Next;
            }
        }
        public static LinkedListNode<T> PreviousOrLast<T>(this LinkedListNode<T> node)
        {
            if (node == node.List.First)
            {
                return node.List.Last;
            }
            else
            {
                return node.Previous;
            }
        }
    }
}
