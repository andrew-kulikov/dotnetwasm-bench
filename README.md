This repository contains SGen stress tests on browser wasm runtime based on different workloads:
1. **LotOfObjects**: Creates and deletes a lot of small objects to test memory usage and garbage collection.
2. **ManagedArrays**: Allocates and fills large managed (C#) arrays with random numbers to see how memory is used.
3. **NativeArrays**: Allocates large blocks of native (unmanaged) memory and fills them with random bytes, then frees the memory.
4. **FileStream**: Creates or opens files, writes and reads large amounts of JSON data, and deserializes it to objects.
5. **AllocateStrings**: Allocates large random strings to test how string memory is handled.
6. **LosFragmentation**: Allocates many large byte arrays to simulate and measure memory fragmentation in the Large Object Heap.
7. **DoGC**: Forces the .NET garbage collector to run and clean up unused memory.

Running sample:
```bash
dotnet publish ./memtest.csproj
cd ./bin/Release/AppBundle
python -m http.server 8080
```