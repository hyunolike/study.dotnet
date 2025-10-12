using System;
// 동적프로그래밍
public class Solution {
    const int MOD = 1234567;
    
    public int solution(int n) {
        if (n == 0) return 0;
        if (n == 1) return 1;
        
        int a = 0;
        int b = 1;
        
        for(int i = 2; i <= n; i++) {
            int c = (a + b) % MOD;
            a = b;
            b = c;
        }
        return b;
    }
}