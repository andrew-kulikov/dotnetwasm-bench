import sys

def generate_test_classes(n):
    """Generate n test classes in the format 'public class TestClass{i};'"""
    lines = []
    lines.append("using System.Threading;")
    lines.append("using System.Threading.Tasks;")
    for i in range(1, n + 1):
        lines.append(f"public class TestClass{i}")
        lines.append(f"{{")
        lines.append(f"    public int Num {{ get; set; }}")
        lines.append(f"    public async Task<TestClass{i}> Foo()")
        lines.append(f"    {{")
        lines.append(f"        var randomNumber = new Random().Next(1, 100);")
        lines.append(f"        if (randomNumber % 9 == 0) await Task.Yield();")
        lines.append(f"        Num = randomNumber;")
        lines.append(f"        return this;")
        lines.append(f"    }}")
        lines.append(f"}}")
    return "\n".join(lines)


def generate_test_classes_caller(n):
    n_methods = n // 100
    lines = []
    lines.append("using System;")
    lines.append("using System.Threading;")
    lines.append("using System.Threading.Tasks;")
    lines.append(f"public class TestClassCaller")
    lines.append(f"{{")
    lines.append(f"    public async Task<long> SumAll(Action<string> printMemory)")
    lines.append(f"    {{")
    lines.append(f"        long sum = 0;")
    for i in range(n_methods):
        lines.append(f"        sum += await SumAll{i}(printMemory);")
    lines.append(f"        return sum;")
    lines.append(f"    }}")
    for i in range(n_methods):
        lines.append(f"    private async Task<long> SumAll{i}(Action<string> printMemory)")
        lines.append(f"    {{")
        lines.append(f"        long sum = 0;")
        for j in range(1, 101):
            lines.append(f"        sum += (await new TestClass{i * 100 + j}().Foo()).Num;")
            if j % 10 == 0:
                lines.append(f"        printMemory($\"#{i * 100 + j}\");")
        lines.append(f"        return sum;")
        lines.append(f"    }}")
    lines.append(f"}}")
    return "\n".join(lines)

def main():
    if len(sys.argv) != 2:
        print("Usage: python generate_test_classes.py <number_of_classes>")
        sys.exit(1)
    
    try:
        n = int(sys.argv[1])
        if n <= 0:
            print("Number of classes must be positive")
            sys.exit(1)
    except ValueError:
        print("Please provide a valid integer")
        sys.exit(1)
    
    # Generate the classes
    generated_code = generate_test_classes(n)
    with open("GeneratedTestClases.cs", "w") as f:
        f.write(generated_code)
    print(f"Generated {n} test classes in GeneratedTestClases.cs")
    
    # Generate the caller
    generated_code = generate_test_classes_caller(n)
    with open("GeneratedTestClasesCaller.cs", "w") as f:
        f.write(generated_code)
    print(f"Generated caller in GeneratedTestClasesCaller.cs")

if __name__ == "__main__":
    main()
