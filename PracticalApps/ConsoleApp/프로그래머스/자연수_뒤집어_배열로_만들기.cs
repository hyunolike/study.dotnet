using System.Collections.Generic;
using System.Linq;

public class Solution {
    public int[] solution(long n) {
        return sol2(n);
    }
    
    private int[] sol1(long n) {
        if (n == 0) return new[] { 0 };

        var list = new List<int>(20); // 최대 20자리 대비 여유
        while (n > 0)
        {
            list.Add((int)(n % 10)); // 뒤집힌 순서로 바로 누적
            n /= 10;
        }
        return list.ToArray();
    }
    
    private int[] sol2(long n) {
        return n.ToString()
            .Reverse()
            .Select(c => c - '0')
            .ToArray();
    }
}