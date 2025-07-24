This repository contains SGen stress tests on browser wasm runtime based on different workloads.

Running sample:
```bash
dotnet publish ./memtest.csproj`
cd ./bin/Release/AppBundle
python -m http.server 8080
```