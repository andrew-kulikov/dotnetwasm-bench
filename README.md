This repository contains SGen stress tests on browser wasm runtime based on different workloads:
1. **LotOfObjects**: Creates and deletes a lot of small objects to test memory usage and garbage collection.
2. **ManagedArrays**: Allocates and fills large managed (C#) arrays with random numbers to see how memory is used.
3. **NativeArrays**: Allocates large blocks of native (unmanaged) memory and fills them with random bytes, then frees the memory.
4. **FileStream**: Creates or opens files, writes and reads large amounts of JSON data, and deserializes it to objects.
5. **AllocateStrings**: Allocates large random strings to test how string memory is handled.
6. **LosFragmentation**: Allocates many large byte arrays to simulate and measure memory fragmentation in the Large Object Heap.
7. **DoGC**: Forces the .NET garbage collector to run and clean up unused memory.
8. **RequestLargeJsonAsString**: Downloads a large JSON payload from a local server as a string and deserializes it to objects.
9. **GenericClasses**: Instantiates closed generic types via reflection repeatedly to stress metadata and allocation.
10. **GenericClassesLevel2**: Instantiates nested generics and invokes a generic method via reflection to stress type system operations.
11. **Exceptions**: Throws and catches many exceptions to stress exception allocation and handling paths.
12. **AsyncTaskExceptions**: Triggers exceptions in async tasks and observes faulted continuations to stress async exception flows.
13. **AutofacGeneratedFactories**: Uses Autofac to resolve factory delegates and create many instances across scopes.
14. **CompiledFuncExpressions**: Builds and compiles expression trees into delegates and invokes them repeatedly.
15. **Events**: Subscribes and unsubscribes delegates to events many times to stress event infrastructure.
16. **Actions**: Allocates and invokes many Action delegates to measure delegate allocation/invocation overhead.

### Running sample

1. Build app

Without aot:
```bash
dotnet publish ./memtest.csproj
```
With aot:
```bash
dotnet publish ./memtest.csproj -p aot=true
```

2. Launch server
```bash
cd ./bin/Release/AppBundle
python -m http.server 8080
```
