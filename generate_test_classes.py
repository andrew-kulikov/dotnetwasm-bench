import sys

def generate_test_classes(n):
    """Generate n test classes in the format 'public class TestClass{i};'"""
    classes = []
    for i in range(1, n + 1):
        classes.append(f"public class TestClass{i};")
    return "\n".join(classes)

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
    
    # Write to GeneratedTestClases.cs
    with open("GeneratedTestClases.cs", "w") as f:
        f.write(generated_code)
    
    print(f"Generated {n} test classes in GeneratedTestClases.cs")

if __name__ == "__main__":
    main()
