using System.Collections.Generic;

public static class ParenthesesStack
{
    public static bool IsValid(string s)
    {
        var st = new Stack<char>();
        foreach (var ch in s)
        {
            if (ch == '(') st.Push(ch);
            else
            {
                if (st.Count == 0) return false;
                st.Pop();
            }
        }
        return st.Count == 0;
    }
}