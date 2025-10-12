# study.dotnet

## 문법 정리
### 숫자 관련 문법
```C#
 // 1. 숫자 관련 문법
    static void NumberExamples()
    {
        // 기본 타입
        int intNum = 42;
        long longNum = 1234567890L;
        float floatNum = 3.14f;
        double doubleNum = 3.141592;
        decimal decimalNum = 99.99m;
        
        Console.WriteLine($"int: {intNum}");
        Console.WriteLine($"long: {longNum}");
        Console.WriteLine($"float: {floatNum}");
        Console.WriteLine($"double: {doubleNum}");
        Console.WriteLine($"decimal: {decimalNum}");
        
        // 산술 연산
        int a = 10, b = 3;
        Console.WriteLine($"\n산술 연산: {a} + {b} = {a + b}");
        Console.WriteLine($"{a} - {b} = {a - b}");
        Console.WriteLine($"{a} * {b} = {a * b}");
        Console.WriteLine($"{a} / {b} = {a / b}");
        Console.WriteLine($"{a} % {b} = {a % b}");
        
        // Math 클래스
        Console.WriteLine($"\nMath.Pow(2, 3) = {Math.Pow(2, 3)}");
        Console.WriteLine($"Math.Sqrt(16) = {Math.Sqrt(16)}");
        Console.WriteLine($"Math.Max(5, 10) = {Math.Max(5, 10)}");
        Console.WriteLine($"Math.Min(5, 10) = {Math.Min(5, 10)}");
        Console.WriteLine($"Math.Abs(-15) = {Math.Abs(-15)}");
        Console.WriteLine($"Math.Round(3.7) = {Math.Round(3.7)}");
        Console.WriteLine($"Math.Ceiling(3.1) = {Math.Ceiling(3.1)}");
        Console.WriteLine($"Math.Floor(3.9) = {Math.Floor(3.9)}");
        
        // 형변환
        string numStr = "123";
        int parsed = int.Parse(numStr);
        Console.WriteLine($"\nint.Parse(\"123\") = {parsed}");
        
        bool success = int.TryParse("456", out int result);
        Console.WriteLine($"TryParse \"456\" = {r
```

###  문자열 관련 문법
```C#
static void StringExamples()
    {
        string str = "Hello, World!";
        Console.WriteLine($"원본 문자열: {str}");
        
        // 문자열 속성
        Console.WriteLine($"Length: {str.Length}");
        Console.WriteLine($"첫 문자: {str[0]}");
        
        // 문자열 메서드
        Console.WriteLine($"\nToUpper: {str.ToUpper()}");
        Console.WriteLine($"ToLower: {str.ToLower()}");
        Console.WriteLine($"Contains 'World': {str.Contains("World")}");
        Console.WriteLine($"StartsWith 'Hello': {str.StartsWith("Hello")}");
        Console.WriteLine($"EndsWith '!': {str.EndsWith("!")}");
        Console.WriteLine($"IndexOf 'World': {str.IndexOf("World")}");
        Console.WriteLine($"Substring(7, 5): {str.Substring(7, 5)}");
        Console.WriteLine($"Replace 'World' -> 'C#': {str.Replace("World", "C#")}");
        
        // 문자열 분할과 결합
        string csv = "apple,banana,orange";
        string[] fruits = csv.Split(',');
        Console.WriteLine($"\nSplit: {string.Join(" | ", fruits)}");
        
        string joined = string.Join(", ", fruits);
        Console.WriteLine($"Join: {joined}");
        
        // 문자열 보간
        string name = "홍길동";
        int age = 30;
        Console.WriteLine($"\n문자열 보간: 이름은 {name}이고 나이는 {age}세입니다.");
        
        // Trim
        string padded = "  spaces  ";
        Console.WriteLine($"Trim: '{padded.Trim()}'");
        Console.WriteLine($"TrimStart: '{padded.TrimStart()}'");
        Console.WriteLine($"TrimEnd: '{padded.TrimEnd()}'");
        
        // 포맷팅
        double price = 1234.56;
        Console.WriteLine($"\n통화 포맷: {price:C}");
        Console.WriteLine($"숫자 포맷: {price:N2}");
        Console.WriteLine($"퍼센트: {0.85:P}");
    }
```

### 컬렉션 관련 문법
```C#
 static void CollectionExamples()
    {
        // List<T>
        Console.WriteLine("List<T>:");
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
        numbers.Add(6);
        numbers.AddRange(new[] { 7, 8, 9 });
        numbers.Insert(0, 0);
        Console.WriteLine($"  초기: {string.Join(", ", numbers)}");
        Console.WriteLine($"  Count: {numbers.Count}");
        Console.WriteLine($"  Contains(5): {numbers.Contains(5)}");
        Console.WriteLine($"  IndexOf(5): {numbers.IndexOf(5)}");
        numbers.Remove(5);
        numbers.RemoveAt(0);
        Console.WriteLine($"  Remove 후: {string.Join(", ", numbers)}");
        
        // Dictionary<TKey, TValue>
        Console.WriteLine("\nDictionary<TKey, TValue>:");
        Dictionary<string, int> ages = new Dictionary<string, int>
        {
            { "홍길동", 30 },
            { "김철수", 25 },
            { "이영희", 28 }
        };
        ages["박민수"] = 32;
        
        foreach (var kvp in ages)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}세");
        }
        Console.WriteLine($"  ContainsKey('홍길동'): {ages.ContainsKey("홍길동")}");
        Console.WriteLine($"  TryGetValue('김철수'): {ages.TryGetValue("김철수", out int age)} (값: {age})");
        
        // HashSet<T>
        Console.WriteLine("\nHashSet<T>:");
        HashSet<int> set1 = new HashSet<int> { 1, 2, 3, 4, 5 };
        HashSet<int> set2 = new HashSet<int> { 4, 5, 6, 7, 8 };
        
        set1.Add(10);
        Console.WriteLine($"  set1: {string.Join(", ", set1)}");
        Console.WriteLine($"  set2: {string.Join(", ", set2)}");
        
        HashSet<int> union = new HashSet<int>(set1);
        union.UnionWith(set2);
        Console.WriteLine($"  합집합: {string.Join(", ", union)}");
        
        HashSet<int> intersect = new HashSet<int>(set1);
        intersect.IntersectWith(set2);
        Console.WriteLine($"  교집합: {string.Join(", ", intersect)}");
        
        // Queue<T>
        Console.WriteLine("\nQueue<T> (FIFO):");
        Queue<string> queue = new Queue<string>();
        queue.Enqueue("첫번째");
        queue.Enqueue("두번째");
        queue.Enqueue("세번째");
        Console.WriteLine($"  Peek: {queue.Peek()}");
        Console.WriteLine($"  Dequeue: {queue.Dequeue()}");
        Console.WriteLine($"  남은 항목: {string.Join(", ", queue)}");
        
        // Stack<T>
        Console.WriteLine("\nStack<T> (LIFO):");
        Stack<string> stack = new Stack<string>();
        stack.Push("첫번째");
        stack.Push("두번째");
        stack.Push("세번째");
        Console.WriteLine($"  Peek: {stack.Peek()}");
        Console.WriteLine($"  Pop: {stack.Pop()}");
        Console.WriteLine($"  남은 항목: {string.Join(", ", stack)}");
    }
```

### LINQ 관련 문법
```C#
static void LinqExamples()
    {
        List<int> nums = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // Where (필터링)
        var evens = nums.Where(n => n % 2 == 0);
        Console.WriteLine($"짝수: {string.Join(", ", evens)}");
        
        // Select (변환)
        var squares = nums.Select(n => n * n);
        Console.WriteLine($"제곱: {string.Join(", ", squares)}");
        
        // OrderBy / OrderByDescending
        var desc = nums.OrderByDescending(n => n);
        Console.WriteLine($"내림차순: {string.Join(", ", desc)}");
        
        // First, FirstOrDefault, Last
        Console.WriteLine($"First: {nums.First()}");
        Console.WriteLine($"Last: {nums.Last()}");
        Console.WriteLine($"First > 5: {nums.First(n => n > 5)}");
        
        // Any, All
        Console.WriteLine($"Any > 5: {nums.Any(n => n > 5)}");
        Console.WriteLine($"All > 0: {nums.All(n => n > 0)}");
        
        // Count, Sum, Average, Min, Max
        Console.WriteLine($"Count: {nums.Count()}");
        Console.WriteLine($"Sum: {nums.Sum()}");
        Console.WriteLine($"Average: {nums.Average()}");
        Console.WriteLine($"Min: {nums.Min()}");
        Console.WriteLine($"Max: {nums.Max()}");
        
        // Take, Skip
        var first3 = nums.Take(3);
        Console.WriteLine($"Take(3): {string.Join(", ", first3)}");
        
        var skip5 = nums.Skip(5);
        Console.WriteLine($"Skip(5): {string.Join(", ", skip5)}");
        
        // Distinct
        List<int> duplicates = new List<int> { 1, 2, 2, 3, 3, 3, 4 };
        var unique = duplicates.Distinct();
        Console.WriteLine($"Distinct: {string.Join(", ", unique)}");
        
        // GroupBy
        List<Student> students = new List<Student>
        {
            new Student { Name = "홍길동", Grade = "A" },
            new Student { Name = "김철수", Grade = "B" },
            new Student { Name = "이영희", Grade = "A" },
            new Student { Name = "박민수", Grade = "C" },
            new Student { Name = "정수진", Grade = "B" }
        };
        
        var grouped = students.GroupBy(s => s.Grade);
        Console.WriteLine("\nGroupBy 학점:");
        foreach (var group in grouped)
        {
            Console.WriteLine($"  {group.Key}: {string.Join(", ", group.Select(s => s.Name))}");
        }
        
        // Join
        List<Department> departments = new List<Department>
        {
            new Department { Id = 1, Name = "개발팀" },
            new Department { Id = 2, Name = "디자인팀" }
        };
        
        List<Employee> employees = new List<Employee>
        {
            new Employee { Name = "홍길동", DeptId = 1 },
            new Employee { Name = "김철수", DeptId = 2 },
            new Employee { Name = "이영희", DeptId = 1 }
        };
        
        var joined = employees.Join(
            departments,
            emp => emp.DeptId,
            dept => dept.Id,
            (emp, dept) => new { emp.Name, dept.Name }
        );
        
        Console.WriteLine("\nJoin 결과:");
        foreach (var item in joined)
        {
            Console.WriteLine($"  {item.Name} - {item.Name}");
        }
        
        // 쿼리 구문 (Query Syntax)
        var query = from n in nums
                    where n > 5
                    orderby n descending
                    select n * 2;
        Console.WriteLine($"\n쿼리 구문: {string.Join(", ", query)}");
        
        // Aggregate
        var product = nums.Take(5).Aggregate((acc, n) => acc * n);
        Console.WriteLine($"\nAggregate (곱셈): {product}");
    }
```

### 1. 배열 (Array)
```C#
// 1차원 배열
int[] numbers = new int[5];
int[] numbers2 = { 1, 2, 3, 4, 5 };
int[] numbers3 = new int[] { 1, 2, 3 };

// 2차원 배열
int[,] matrix = new int[3, 3];
int[,] matrix2 = { { 1, 2 }, { 3, 4 }, { 5, 6 } };

// 가변 배열 (Jagged Array)
int[][] jagged = new int[3][];
jagged[0] = new int[] { 1, 2 };
jagged[1] = new int[] { 3, 4, 5 };
jagged[2] = new int[] { 6 };

// 배열 메서드
Array.Sort(numbers2);
Array.Reverse(numbers2);
int index = Array.IndexOf(numbers2, 3);
```

### 2. List<T> (리스트)
```C#
using System.Collections.Generic;

// 생성
List<int> numbers = new List<int>();
List<string> names = new List<string> { "Alice", "Bob", "Charlie" };

// 추가
numbers.Add(10);
numbers.AddRange(new int[] { 20, 30, 40 });
numbers.Insert(1, 15); // 인덱스 1에 삽입

// 삭제
numbers.Remove(20);
numbers.RemoveAt(0);
numbers.RemoveAll(x => x > 30);
numbers.Clear();

// 검색
bool exists = numbers.Contains(30);
int index = numbers.IndexOf(30);
int found = numbers.Find(x => x > 20);
List<int> filtered = numbers.FindAll(x => x > 20);

// 정렬
numbers.Sort();
numbers.Reverse();

// 기타
int count = numbers.Count;
int capacity = numbers.Capacity; // 내부 배열 크기
```

### 4. Dictionary<TKey, TValue> (딕셔너리)
```C#
// 생성
Dictionary<string, int> ages = new Dictionary<string, int>();
Dictionary<string, int> ages2 = new Dictionary<string, int>
{
    { "Alice", 25 },
    { "Bob", 30 },
    { "Charlie", 35 }
};

// 추가
ages.Add("David", 40);
ages["Eve"] = 28; // 추가 또는 업데이트

// 삭제
ages.Remove("Bob");
ages.Clear();

// 검색
bool exists = ages.ContainsKey("Alice");
bool valueExists = ages.ContainsValue(25);

// 안전한 값 가져오기
if (ages.TryGetValue("Alice", out int age))
{
    Console.WriteLine($"Alice is {age} years old");
}

// 순회
foreach (KeyValuePair<string, int> kvp in ages)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

foreach (string key in ages.Keys)
{
    Console.WriteLine(key);
}

foreach (int value in ages.Values)
{
    Console.WriteLine(value);
}
```

### 5. SortedDictionary<TKey, TValue> (정렬된 딕셔너리)
```C#
// 키를 기준으로 자동 정렬
SortedDictionary<string, int> sorted = new SortedDictionary<string, int>
{
    { "Charlie", 35 },
    { "Alice", 25 },
    { "Bob", 30 }
};

// 순회하면 키가 정렬된 순서로 출력됨
foreach (var kvp in sorted)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
// 출력: Alice: 25, Bob: 30, Charlie: 35
```

### 6. HashSet<T> (해시셋)
```C#
// 중복을 허용하지 않는 컬렉션
HashSet<int> set = new HashSet<int> { 1, 2, 3, 4, 5 };

// 추가 (중복은 무시됨)
bool added = set.Add(6); // true
bool notAdded = set.Add(3); // false (이미 존재)

// 삭제
set.Remove(2);

// 검색
bool exists = set.Contains(3);

// 집합 연산
HashSet<int> set2 = new HashSet<int> { 4, 5, 6, 7, 8 };

// 합집합
set.UnionWith(set2);

// 교집합
set.IntersectWith(set2);

// 차집합
set.ExceptWith(set2);

// 대칭 차집합
set.SymmetricExceptWith(set2);

// 부분집합 확인
bool isSubset = set.IsSubsetOf(set2);
bool isSuperset = set.IsSupersetOf(set2);
```

### 7. SortedSet<T> (정렬된 셋)
```C#
// 자동으로 정렬되는 중복 없는 컬렉션
SortedSet<int> sortedSet = new SortedSet<int> { 5, 2, 8, 1, 9 };

// 추가하면 자동 정렬됨
sortedSet.Add(3);
sortedSet.Add(7);

// Min, Max 접근
int min = sortedSet.Min;
int max = sortedSet.Max;

// 범위 검색
var subset = sortedSet.GetViewBetween(2, 7);

// 순회 (정렬된 순서)
foreach (int item in sortedSet)
{
    Console.WriteLine(item); // 1, 2, 3, 5, 7, 8, 9
}
```

### 8. Queue<T> (큐 - FIFO)
```C#
Queue<string> queue = new Queue<string>();

// 추가 (Enqueue)
queue.Enqueue("First");
queue.Enqueue("Second");
queue.Enqueue("Third");

// 제거 및 반환 (Dequeue)
string first = queue.Dequeue(); // "First"

// 확인만 (제거하지 않음)
string peek = queue.Peek(); // "Second"

// 기타
int count = queue.Count;
bool contains = queue.Contains("Third");
queue.Clear();

// 배열로 변환
string[] array = queue.ToArray();
```

### 9. Stack<T> (스택 - LIFO)
```C#
Stack<int> stack = new Stack<int>();

// 추가 (Push)
stack.Push(1);
stack.Push(2);
stack.Push(3);

// 제거 및 반환 (Pop)
int top = stack.Pop(); // 3

// 확인만 (제거하지 않음)
int peek = stack.Peek(); // 2

// 기타
int count = stack.Count;
bool contains = stack.Contains(1);
stack.Clear();

// 배열로 변환
int[] array = stack.ToArray();
```

### 10. SortedList<TKey, TValue>
```C#
// 키로 정렬되며 인덱스로도 접근 가능
SortedList<string, int> sortedList = new SortedList<string, int>
{
    { "Charlie", 35 },
    { "Alice", 25 },
    { "Bob", 30 }
};

// 추가
sortedList.Add("David", 40);

// 인덱스로 접근
string keyAt1 = sortedList.Keys[1];
int valueAt1 = sortedList.Values[1];

// 인덱스로 제거
sortedList.RemoveAt(0);

// 키로 검색
int age = sortedList["Bob"];
```

---

## C# LINQ 완벽 가이드
### 1. LINQ 기본 개념
```C#
using System;
using System.Linq;
using System.Collections.Generic;

// LINQ는 두 가지 구문이 있습니다
// 1. 쿼리 구문 (Query Syntax)
// 2. 메서드 구문 (Method Syntax)
```

### 2. Where (필터링)
```C#
List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// 메서드 구문
var evenNumbers = numbers.Where(n => n % 2 == 0);

// 쿼리 구문
var evenNumbers2 = from n in numbers
                   where n % 2 == 0
                   select n;

// 여러 조건
var result = numbers.Where(n => n > 3 && n < 8);

// 인덱스 사용
var indexed = numbers.Where((n, index) => index % 2 == 0);

foreach (var num in evenNumbers)
{
    Console.WriteLine(num); // 2, 4, 6, 8, 10
}
```

### 3. Select (투영/변환)
```C#
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

// 단순 변환
var squared = numbers.Select(n => n * n);

// 익명 타입으로 변환
var detailed = numbers.Select(n => new 
{ 
    Original = n, 
    Squared = n * n,
    IsEven = n % 2 == 0
});

// 쿼리 구문
var squared2 = from n in numbers
               select n * n;

// 인덱스 포함
var withIndex = numbers.Select((n, index) => new 
{ 
    Index = index, 
    Value = n 
});

// 문자열 변환
List<string> names = new List<string> { "alice", "bob", "charlie" };
var upperNames = names.Select(n => n.ToUpper());

foreach (var item in detailed)
{
    Console.WriteLine($"원본: {item.Original}, 제곱: {item.Squared}, 짝수: {item.IsEven}");
}
```

### 4. SelectMany (평탄화)
```C#
// 중첩된 컬렉션 평탄화
List<List<int>> nestedList = new List<List<int>>
{
    new List<int> { 1, 2, 3 },
    new List<int> { 4, 5 },
    new List<int> { 6, 7, 8, 9 }
};

var flattened = nestedList.SelectMany(list => list);
// 결과: 1, 2, 3, 4, 5, 6, 7, 8, 9

// 문자열 배열에서 모든 문자 추출
string[] words = { "hello", "world" };
var allChars = words.SelectMany(w => w);
// 결과: h, e, l, l, o, w, o, r, l, d

// 복잡한 예제
class Student
{
    public string Name { get; set; }
    public List<string> Subjects { get; set; }
}

List<Student> students = new List<Student>
{
    new Student { Name = "Alice", Subjects = new List<string> { "Math", "Science" } },
    new Student { Name = "Bob", Subjects = new List<string> { "English", "History" } }
};

var allSubjects = students.SelectMany(s => s.Subjects);
// 결과: Math, Science, English, History

// 학생 정보와 함께
var studentSubjects = students.SelectMany(
    s => s.Subjects,
    (student, subject) => new { student.Name, Subject = subject }
);
```

### 5. OrderBy / OrderByDescending / ThenBy
```C#
List<int> numbers = new List<int> { 5, 2, 8, 1, 9, 3 };

// 오름차순 정렬
var ascending = numbers.OrderBy(n => n);

// 내림차순 정렬
var descending = numbers.OrderByDescending(n => n);

// 쿼리 구문
var sorted = from n in numbers
             orderby n
             select n;

var sortedDesc = from n in numbers
                 orderby n descending
                 select n;

// 복합 정렬
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string City { get; set; }
}

List<Person> people = new List<Person>
{
    new Person { Name = "Alice", Age = 25, City = "Seoul" },
    new Person { Name = "Bob", Age = 30, City = "Busan" },
    new Person { Name = "Charlie", Age = 25, City = "Seoul" },
    new Person { Name = "David", Age = 30, City = "Seoul" }
};

// 나이로 정렬 후, 같은 나이면 이름으로 정렬
var sorted1 = people.OrderBy(p => p.Age).ThenBy(p => p.Name);

// 도시로 내림차순, 그 다음 나이로 오름차순
var sorted2 = people.OrderByDescending(p => p.City)
                    .ThenBy(p => p.Age)
                    .ThenBy(p => p.Name);

// 쿼리 구문
var sorted3 = from p in people
              orderby p.Age, p.Name
              select p;

var sorted4 = from p in people
              orderby p.City descending, p.Age
              select p;
```

### 6. GroupBy (그룹화)
```C#
List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// 짝수/홀수로 그룹화
var grouped = numbers.GroupBy(n => n % 2 == 0 ? "Even" : "Odd");

foreach (var group in grouped)
{
    Console.WriteLine($"Key: {group.Key}");
    foreach (var num in group)
    {
        Console.Write($"{num} ");
    }
    Console.WriteLine();
}

// 복잡한 예제
class Student
{
    public string Name { get; set; }
    public string Department { get; set; }
    public int Grade { get; set; }
}

List<Student> students = new List<Student>
{
    new Student { Name = "Alice", Department = "CS", Grade = 90 },
    new Student { Name = "Bob", Department = "Math", Grade = 85 },
    new Student { Name = "Charlie", Department = "CS", Grade = 88 },
    new Student { Name = "David", Department = "Math", Grade = 92 }
};

// 학과별 그룹화
var byDept = students.GroupBy(s => s.Department);

foreach (var group in byDept)
{
    Console.WriteLine($"학과: {group.Key}");
    foreach (var student in group)
    {
        Console.WriteLine($"  {student.Name}: {student.Grade}");
    }
}

// 그룹화 후 집계
var deptAverage = students.GroupBy(s => s.Department)
                          .Select(g => new
                          {
                              Department = g.Key,
                              Count = g.Count(),
                              Average = g.Average(s => s.Grade),
                              Max = g.Max(s => s.Grade)
                          });

// 쿼리 구문
var grouped2 = from s in students
               group s by s.Department into g
               select new
               {
                   Department = g.Key,
                   Students = g,
                   Average = g.Average(s => s.Grade)
               };

// 여러 키로 그룹화
var multiGroup = students.GroupBy(s => new { s.Department, GradeRange = s.Grade / 10 * 10 });
```

### 7. Join (조인)
```C#
class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
}

class Employee
{
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public decimal Salary { get; set; }
}

List<Department> departments = new List<Department>
{
    new Department { Id = 1, Name = "IT" },
    new Department { Id = 2, Name = "HR" },
    new Department { Id = 3, Name = "Sales" }
};

List<Employee> employees = new List<Employee>
{
    new Employee { Name = "Alice", DepartmentId = 1, Salary = 5000 },
    new Employee { Name = "Bob", DepartmentId = 2, Salary = 4500 },
    new Employee { Name = "Charlie", DepartmentId = 1, Salary = 5500 },
    new Employee { Name = "David", DepartmentId = 3, Salary = 4000 }
};

// Inner Join (메서드 구문)
var joined = employees.Join(
    departments,
    emp => emp.DepartmentId,
    dept => dept.Id,
    (emp, dept) => new
    {
        EmployeeName = emp.Name,
        DepartmentName = dept.Name,
        Salary = emp.Salary
    }
);

// Inner Join (쿼리 구문)
var joined2 = from emp in employees
              join dept in departments
              on emp.DepartmentId equals dept.Id
              select new
              {
                  EmployeeName = emp.Name,
                  DepartmentName = dept.Name,
                  Salary = emp.Salary
              };

// Group Join
var groupJoined = departments.GroupJoin(
    employees,
    dept => dept.Id,
    emp => emp.DepartmentId,
    (dept, emps) => new
    {
        Department = dept.Name,
        Employees = emps,
        Count = emps.Count()
    }
);

// Group Join (쿼리 구문)
var groupJoined2 = from dept in departments
                   join emp in employees
                   on dept.Id equals emp.DepartmentId into empGroup
                   select new
                   {
                       Department = dept.Name,
                       Employees = empGroup,
                       Count = empGroup.Count()
                   };
```

### 8. 집계 함수
```C#
List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Count
int count = numbers.Count();
int evenCount = numbers.Count(n => n % 2 == 0);

// Sum
int sum = numbers.Sum();
int evenSum = numbers.Sum(n => n % 2 == 0 ? n : 0);

// Average
double avg = numbers.Average();

// Min / Max
int min = numbers.Min();
int max = numbers.Max();

// Aggregate (사용자 정의 집계)
int product = numbers.Aggregate((a, b) => a * b); // 모든 수의 곱
int factorial = Enumerable.Range(1, 5).Aggregate((a, b) => a * b); // 5!

// 시드 값과 함께
string concatenated = numbers.Aggregate("Numbers: ", (acc, n) => acc + n + " ");

// 복잡한 객체
class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

List<Product> products = new List<Product>
{
    new Product { Name = "A", Price = 100, Stock = 10 },
    new Product { Name = "B", Price = 200, Stock = 5 },
    new Product { Name = "C", Price = 150, Stock = 8 }
};

decimal totalValue = products.Sum(p => p.Price * p.Stock);
decimal avgPrice = products.Average(p => p.Price);
Product mostExpensive = products.OrderByDescending(p => p.Price).First();

// MinBy / MaxBy (C# 10+)
Product cheapest = products.MinBy(p => p.Price);
Product expensive = products.MaxBy(p => p.Price);
```

### 9. Any / All / Contains
```C#
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

// Any - 조건을 만족하는 요소가 하나라도 있는지
bool hasEven = numbers.Any(n => n % 2 == 0); // true
bool hasNegative = numbers.Any(n => n < 0); // false
bool hasAny = numbers.Any(); // 비어있지 않은지

// All - 모든 요소가 조건을 만족하는지
bool allPositive = numbers.All(n => n > 0); // true
bool allEven = numbers.All(n => n % 2 == 0); // false

// Contains - 특정 요소를 포함하는지
bool contains3 = numbers.Contains(3); // true
bool contains10 = numbers.Contains(10); // false

// 복잡한 예제
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

List<Person> people = new List<Person>
{
    new Person { Name = "Alice", Age = 25 },
    new Person { Name = "Bob", Age = 30 },
    new Person { Name = "Charlie", Age = 35 }
};

bool hasAdult = people.Any(p => p.Age >= 18);
bool allAdults = people.All(p => p.Age >= 18);
bool hasAlice = people.Any(p => p.Name == "Alice");
```

... 
