```

BenchmarkDotNet v0.15.2, macOS Sequoia 15.6 (24G84) [Darwin 24.6.0]
Apple M3, 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), Arm64 RyuJIT AdvSIMD
  .NET 9.0 : .NET 9.0.0 (9.0.24.52809), Arm64 RyuJIT AdvSIMD

Job=.NET 9.0  Runtime=.NET 9.0  

```
| Method       | EntityCount | Mean        | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------------- |------------ |------------:|----------:|----------:|-------:|-------:|----------:|
| Add_Test     | 1000        | 12,249.4 ns | 149.76 ns | 140.09 ns | 9.7046 | 1.6174 |   81272 B |
| Get_Test     | 1000        |  5,131.5 ns |  24.67 ns |  23.08 ns |      - |      - |         - |
| Has_Test     | 1000        |  2,845.7 ns |  13.92 ns |  12.34 ns |      - |      - |         - |
| Iterate_Test | 1000        |    284.2 ns |   1.41 ns |   1.10 ns |      - |      - |         - |
