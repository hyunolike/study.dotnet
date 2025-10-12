using System;

public class Solution {
    public int[] solution(int[] array, int[,] commands) {
        int m = commands.GetLength(0);
        var answer = new int[m];
        
        for(int c = 0; c < m; c++) {
            int i = commands[c, 0];
            int j = commands[c, 1];
            int k = commands[c, 2];
            
            int len = j - i + 1;
            
            var tmp = new int[len];
            Array.Copy(array, i - 1, tmp, 0, len);
            Array.Sort(tmp);
            answer[c] = tmp[k-1];
        }
        return answer;
    }
}