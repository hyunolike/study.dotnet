using System;
using System.Text;

public class Solution {
    public string solution(string s) {
        if (string.IsNullOrEmpty(s)) return s;
        
        var sb = new StringBuilder(s.Length);
        int idx = 0; // 단어 내 위치
        
        foreach(char ch in s) {
            if (ch == ' ') {
                sb.Append(' ');
                idx = 0;
                continue;
            }
            
            sb.Append(idx % 2 == 0 ? char.ToUpper(ch) : char.ToLower(ch));
            idx++;
        }
        
        return sb.ToString();
    }
}