using System.Linq;
using System.Collections.Generic;

// # 1.배열
int[] numbers = new int[5]; 
int[] numbers2 = {1, 9, 3, 4, 5};
int[] numbers3 = new int[] {1, 2, 3};
// 2차원 배열
int[,] matrix = new int[3,3];
int[,] matrix2 = {{1,2}, {3,4}};

// 배열 메서드
Console.WriteLine($"numbers: {string.Join(", ", numbers2)}");
Array.Sort(numbers2);
Array.Reverse(numbers3);
Console.WriteLine($"numbers: {string.Join(", ", numbers2)}");

// # 2.List<T> 리스트
List<int> numbersList = new List<int>();
List<string> names = new List<string> {"Alice", "Bob", "Charlie"};

// 추가
numbersList.Add(10);
numbersList.AddRange(new int[] {1, 2, 3});

Console.WriteLine($"numbersList: {string.Join(", ", numbersList)}");

numbersList.Insert(1, 15); // 인덱스 1에 삽입

Console.WriteLine($"numbersList: {string.Join(", ", numbersList)}");

// 삭제
numbersList.Remove(1);
Console.WriteLine($"numbersList: {string.Join(", ", numbersList)}");
numbersList.RemoveAt(1);
numbersList.RemoveAll(x => x > 10);
Console.WriteLine($"numbersList: {string.Join(", ", numbersList)}");

// numbersList.Clear();
Console.WriteLine($"numbersList: {string.Join(", ", numbersList)}");

// 검색
bool exists = numbersList.Contains(10);
Console.WriteLine($"exists: {exists}");

int index = numbersList.IndexOf(2);
Console.WriteLine($"index: {index}");

int found = numbersList.Find(x => x > 10);
List<int> filtered = numbersList.FindAll(x => x > 1);

// 정렬
numbersList.Sort();
numbersList.Reverse();

// 기타
int count = numbersList.Count;
int capacity = numbersList.Capacity;
Console.WriteLine($"count: {count}, capacity: {capacity}");

// # 3.Dictionary<TKey, TValue> 딕셔너리




