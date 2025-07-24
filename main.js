// Import required runtime
import { dotnet } from './_framework/dotnet.js'

// Initialize the .NET runtime
const { getAssemblyExports, getConfig, runMain, Module } = await dotnet
    .withDiagnosticTracing(false)
    .create();

// Get exports from our assembly
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

globalThis.Module = Module;

document.getElementById('lotOfObjectsButton').addEventListener('click', () => {
    exports.Benchmarks.LotOfObjects(1000, false);
});

document.getElementById('lotOfObjectsGcButton').addEventListener('click', () => {
    exports.Benchmarks.LotOfObjects(1000, true);
});

document.getElementById('managedArraysButton').addEventListener('click', () => {
    exports.Benchmarks.ManagedArrays(1000, false);
});

document.getElementById('managedArraysGcButton').addEventListener('click', () => {
    exports.Benchmarks.ManagedArrays(1000, true);
});

document.getElementById('nativeArraysButton').addEventListener('click', () => {
    exports.Benchmarks.NativeArrays(1000);
});

document.getElementById('fileStreamButton').addEventListener('click', () => {
    exports.Benchmarks.FileStream(1000, false);
});

document.getElementById('fileStreamGcButton').addEventListener('click', () => {
    exports.Benchmarks.FileStream(1000, true);
});

document.getElementById('allocateStringsButton').addEventListener('click', () => {
    exports.Benchmarks.AllocateStrings(1000, false);
});

document.getElementById('allocateStringsGcButton').addEventListener('click', () => {
    exports.Benchmarks.AllocateStrings(1000, true);
});

document.getElementById('allocate1StringButton').addEventListener('click', () => {
    exports.Benchmarks.AllocateStrings(1, false);
});

document.getElementById('doGcButton').addEventListener('click', () => {
    exports.Benchmarks.DoGC();
});

document.getElementById('losFragmentationButton').addEventListener('click', () => {
    const count = document.getElementById('losFragmentationCount').value;
    const batchSize = document.getElementById('losFragmentationBatchSize').value;
    const size = document.getElementById('losFragmentationSize').value;
    exports.Benchmarks.LosFragmentation(+count, +batchSize, +size);
});

await runMain();