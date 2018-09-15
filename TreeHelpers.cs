using System;
using System.Collections.Generic;

namespace IgorPopov.SampleCode
{
    public interface ITreeItem<T> where T: class, ITreeItem<T> 
    {
        T Parent { get; }
        IList<T> Children { get; }
    }

    public static class TreeHelpers
    {
        // Provides a deep list of the tree, with elements on the same level occurring next to each other
        public static IEnumerable<T> DeepListHorizontal<T>(this T root, bool includeRoot) where T : class, ITreeItem<T>
        {
            Stack<IEnumerable<T>> stack = new Stack<IEnumerable<T>>();
            if (includeRoot)
                stack.Push(new List<T> { root });
            else
                stack.Push(root.Children);

            while (stack.Count > 0)
            {
                var items = stack.Pop();
                foreach (var item in items)
                {
                    yield return item;
                    if (item.Children.Count > 0) stack.Push(item.Children);
                }
            }
        }

        // Provides a deep list of of the tree, going vertically first and then horizontally
        public static IEnumerable<T> DeepListVertical<T>(this T root, bool includeRoot) where T : class, ITreeItem<T>
        {
            Stack<IEnumerator<T>> stack = new Stack<IEnumerator<T>>();
            if (includeRoot)
                stack.Push((new List<T>() { root }).GetEnumerator());
            else
                stack.Push(root.Children.GetEnumerator());

            while (stack.Count > 0)
            {
                var en = stack.Pop();
                while (en.MoveNext())
                {
                    T current = en.Current;
                    yield return current;
                    if (current.Children.Count > 0)
                    {
                        stack.Push(en);
                        en = current.Children.GetEnumerator();
                    }
                }
                en.Dispose();
            }
        }

        // Returns all elements satisfying findWhat on various horizontal levels (but not below one another), and ignoring deeper levels when ignoreChildren returns true
        // Inlcudes the Root element in the search
        public static IEnumerable<T> HorizontalSearch<T>(this T root, Func<T, bool> findWhat, Func<T, bool> ignoreChildren) where T : class, ITreeItem<T>
        {
            Stack<IEnumerable<T>> stack = new Stack<IEnumerable<T>>();
            stack.Push(new List<T> { root });
            while (stack.Count > 0)
            {
                var items = stack.Pop();
                foreach (var item in items)
                {
                    if (findWhat(item))
                        yield return item;
                    else if (item.Children.Count > 0 && !ignoreChildren(item))
                        stack.Push(item.Children);
                }
            }
        }

        // Provides a deep list of of the tree, going vertically first and then horizontally
        public static IEnumerable<T> VerticalSearch<T>(this T root, Func<T, bool> findWhat, Func<T, bool> ignoreChildren) where T : class, ITreeItem<T>
        {
            Stack<IEnumerator<T>> stack = new Stack<IEnumerator<T>>();
            stack.Push((new List<T>() { root }).GetEnumerator());

            while (stack.Count > 0)
            {
                var en = stack.Pop();
                while (en.MoveNext())
                {
                    T item = en.Current;
                    if (findWhat(item))
                        yield return item;
                    else if (item.Children.Count > 0 && !ignoreChildren(item))
                    {
                        stack.Push(en);
                        en = item.Children.GetEnumerator();
                    }
                }
                en.Dispose();
            }
        }

        // Traverses the tree up to the root looking for an element, stops at an element marked by the stop predicate
        public static T SearchUp<T>(this T root, Func<T, bool> findWhat, Func<T, bool> stop) where T : class, ITreeItem<T>
        {
            T item = root;
            while (true)
            {
                if (findWhat(item)) return item;
                if (item.Parent == null || stop(item)) break;
                item = item.Parent;
            }
            return null;
        }
    }
}
