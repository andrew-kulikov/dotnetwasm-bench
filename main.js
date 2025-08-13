// Import required runtime
import { dotnet } from './_framework/dotnet.js'

Error.stackTraceLimit = 50;

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

document.getElementById('largeJsonAsStringButton').addEventListener('click', () => {
    const count = document.getElementById('largeJsonAsStringCount').value;
    const sizeMb = document.getElementById('largeJsonAsStringSizeMb').value;
    const doGc = document.getElementById('largeJsonAsStringDoGc').checked;
    exports.Benchmarks.RequestLargeJsonAsString(+count, +sizeMb, doGc);
});

document.getElementById('losFragmentationButton').addEventListener('click', () => {
    const count = document.getElementById('losFragmentationCount').value;
    const batchSize = document.getElementById('losFragmentationBatchSize').value;
    const size = document.getElementById('losFragmentationSize').value;
    exports.Benchmarks.LosFragmentation(+count, +batchSize, +size);
});

document.getElementById('genericClassesButton').addEventListener('click', () => {
    const count = document.getElementById('genericClassesCount').value;
    const doGc = document.getElementById('genericClassesDoGc').checked;
    exports.Benchmarks.GenericClasses(+count, doGc);
});

document.getElementById('genericClassesLevel2Button').addEventListener('click', () => {
    const count = document.getElementById('genericClassesLevel2Count').value;
    const doGc = document.getElementById('genericClassesLevel2DoGc').checked;
    exports.Benchmarks.GenericClassesLevel2(+count, doGc);
});

document.getElementById('exceptionsButton').addEventListener('click', () => {
    const count = document.getElementById('exceptionsCount').value;
    const doGc = document.getElementById('exceptionsDoGc').checked;
    exports.Benchmarks.Exceptions(+count, doGc);
});

document.getElementById('asyncTaskExceptionsButton').addEventListener('click', () => {
    const count = document.getElementById('asyncTaskExceptionsCount').value;
    const doGc = document.getElementById('asyncTaskExceptionsDoGc').checked;
    exports.Benchmarks.AsyncTaskExceptions(+count, doGc);
});

document.getElementById('autofacGeneratedFactoriesButton').addEventListener('click', () => {
    const count = document.getElementById('autofacGeneratedFactoriesCount').value;
    const doGc = document.getElementById('autofacGeneratedFactoriesDoGc').checked;
    exports.Benchmarks.AutofacGeneratedFactories(+count, doGc);
});

document.getElementById('autofacGeneratedFactoriesRegisterEachTimeButton').addEventListener('click', () => {
    const count = document.getElementById('autofacGeneratedFactoriesRegisterEachTimeCount').value;
    const doGc = document.getElementById('autofacGeneratedFactoriesRegisterEachTimeDoGc').checked;
    exports.Benchmarks.AutofacGeneratedFactoriesRegisterEachTime(+count, doGc);
});

document.getElementById('compiledFuncExpressionsButton').addEventListener('click', () => {
    const count = document.getElementById('compiledFuncExpressionsCount').value;
    const doGc = document.getElementById('compiledFuncExpressionsDoGc').checked;
    exports.Benchmarks.CompiledFuncExpressions(+count, doGc);
});

document.getElementById('eventsButton').addEventListener('click', () => {
    const count = document.getElementById('eventsCount').value;
    const doGc = document.getElementById('eventsDoGc').checked;
    exports.Benchmarks.Events(+count, doGc);
});

document.getElementById('actionsButton').addEventListener('click', () => {
    const count = document.getElementById('actionsCount').value;
    const doGc = document.getElementById('actionsDoGc').checked;
    exports.Benchmarks.Actions(+count, doGc);
});

await runMain();