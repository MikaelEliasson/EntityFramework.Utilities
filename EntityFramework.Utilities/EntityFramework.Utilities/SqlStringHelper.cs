using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFramework.Utilities
{
    static class SqlStringHelper
    {
        internal static string FixParantheses(string str)
        {
            var chars = str.ToCharArray();
            var stack = new Stack<Tuple<int, int>>();
            var toRemove = new HashSet<int>();
            foreach (var c in chars.Select((c, i) => new { c, i }))
            {
                if (c.c == '(')
                {
                    stack.Push(new Tuple<int, int>(c.i, -1));
                }
                else if (c.c == ')')
                {
                    if (stack.Any())
                    {
                        stack.Pop();
                    }
                    else
                    {
                        toRemove.Add(c.i);
                    }
                }
            }
            foreach (var item in stack)
            {
                toRemove.Add(item.Item1);
            }

            return string.Join("", chars.Where((c, i) => !toRemove.Contains(i)).Select(c => c));
        }
    }
}
