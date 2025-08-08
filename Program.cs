using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using Autofac;
using System.Linq.Expressions;

Console.WriteLine("Hello from dotnet");

public static partial class Benchmarks
{
    private static Random random = new Random();

    [JSExport]
    public static async Task LotOfObjects(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            var objs = random.Next(100_000, 1_000_000);
            PrintMemory($"LO #{i} - start | {objs}");
            var objects = new List<object>();
            for (int j = 0; j < objs; j++)
            {
                objects.Add(new object());
            }
            objects = null;
            if (doGc)
            {
                DoGC();
            }
            await Task.Delay(16);
            PrintMemory($"LO #{i} - end");
        }
    }

    [JSExport]
    public static async Task ManagedArrays(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            var size = random.Next(4_000_000, 60_000_000);
            PrintMemory($"MA #{i} - start | {FormatBytes(size)}");
            var array = new int[size];
            for (int j = 0; j < array.Length; j++)
            {
                array[j] = random.Next();
            }
            array = null;
            if (doGc)
            {
                DoGC();
            }
            await Task.Delay(16);
            PrintMemory($"MA #{i} - end");
        }
    }

    [JSExport]
    public static async Task NativeArrays(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var size = random.Next(4_000_000, 60_000_000);
            PrintMemory($"NA #{i} - start | {FormatBytes(size)}");
            var array = Marshal.AllocHGlobal(size);
            try
            {
                for (int j = 0; j < size; j++)
                {
                    Marshal.WriteByte(array, j, (byte)random.Next(0, 255));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(array);
            }
            await Task.Delay(16);
            PrintMemory($"NA #{i} - end");
        }
    }

    [JSExport]
    public static async Task FileStream(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            var fileName = $"file_json_{i}.txt";
            if (!File.Exists(fileName))
            {
                using var createFs = File.Create(fileName);

                var size = random.Next(100_000, 500_000);
                var initialDtos = Enumerable.Range(0, size).Select(x => new TestDto { Id = x, Name = $"Test {x}", Value = x }).ToArray();
                var initialJson = JsonConvert.SerializeObject(initialDtos);
                createFs.Write(Encoding.UTF8.GetBytes(initialJson));
            }

            var fs = File.Open(fileName, FileMode.Open);
            var json = new StreamReader(fs).ReadToEnd();
            var dtos = JsonConvert.DeserializeObject<TestDto[]>(json);
            dtos = null;
            if (doGc)
            {
                DoGC();
            }
            await Task.Delay(16);
            PrintMemory($"FS #{i}");
        }
    }

    [JSExport]
    public static async Task AllocateStrings(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            var str = new string('a', random.Next(1000, 10_000));
            if (doGc)
            {
                DoGC();
            }
            await Task.Delay(16);
            PrintMemory($"AS #{i}");
        }
    }

    [JSExport]
    public static async Task LosFragmentation(int count, int batchSize, int size)
    {
        var losHolder = new List<byte[]>();
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < batchSize; j++)
            {
                losHolder.Add(new byte[size]);
            }
            await Task.Delay(16);
            // Collect to recalculate fragmentation stats
            GC.Collect();
            PrintMemory($"LF #{i}");
        }
    }

    [JSExport]
    public static async Task RequestLargeJsonAsString(int count, int sizeMb, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            // Start server in jsonserver folder to run this test
            var url = $"http://localhost:8090/{sizeMb}.json";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<TestDto>>(json);
            data = null;
            if (doGc)
            {
                DoGC();
            }
            await Task.Delay(16);
            PrintMemory($"#{i}");
        }
    }

    [JSExport]
    public static async Task GenericClasses(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                var obj = await GetGenericClass((i * 10 + j) % 10000 + 1);
                if (doGc)
                {
                    obj = null;
                    DoGC();
                }
            }
           
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }

        async Task<object> GetGenericClass(int number, CancellationToken cancellationToken = default)
        {
            //await Task.Delay(1, cancellationToken);
            var type = typeof(GenericClass<>);
            var testClassType = Assembly.GetExecutingAssembly().GetType($"TestClass{number}");
            var genericType = type.MakeGenericType(testClassType);
            var instance = Activator.CreateInstance(genericType);
            return instance;
        }
    }

    [JSExport]
    public static async Task GenericClassesLevel2(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                var obj = await GetGenericClass((i * 10 + j) % 10000 + 1);
                if (doGc)
                {
                    obj = null;
                    DoGC();
                }
            }
           
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }

        async Task<object> GetGenericClass(int number, CancellationToken cancellationToken = default)
        {
            var type = typeof(GenericClass1<>);
            var testClassType = Assembly.GetExecutingAssembly().GetType($"TestClass{number}");
            var genericType = type.MakeGenericType(testClassType);
            var instance = Activator.CreateInstance(genericType);
            var method = genericType.GetMethod("Compare");
            var anotherType = Assembly.GetExecutingAssembly().GetType($"TestClass{(number + 1) % 10000 + 1}");
            var result = method.MakeGenericMethod(anotherType).Invoke(instance, null);
            return result;
        }
    }

    [JSExport]
    public static async Task Exceptions(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                try
                {
                    throw new Exception($"Test exception {i * j}");
                }
                catch (Exception ex)
                {}
            }
            if (doGc)
            {
                DoGC();
            }
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }
    }

    [JSExport]
    public static async Task AsyncTaskExceptions(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                await DoSomething().ContinueWith(t => t.Exception, TaskContinuationOptions.OnlyOnFaulted);
            }
            if (doGc)
            {
                DoGC();
            }
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }

        async Task DoSomething()
        {
            await Task.Yield();
            throw new Exception("Test exception");
        }
    }

    [JSExport]
    public static async Task AutofacGeneratedFactories(int count, bool doGc)
    {
        var cb = new ContainerBuilder();
        cb.RegisterType<AutofacTestClass>();
        var container = cb.Build();
        for (int i = 0; i < count; i++)
        {
            var objs = new List<AutofacTestClass>();
            for (int j = 0; j < 100; j++)
            {
                using var scope = container.BeginLifetimeScope();
                var obj = scope.Resolve<Func<object, AutofacTestClass>>();
                var testObj = obj(new object());
                objs.Add(testObj);
            }
            objs = null;
            if (doGc)
            {
                DoGC();
            }
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }
    }

    
    [JSExport]
    public static async Task AutofacGeneratedFactoriesRegisterEachTime(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            var objs = new List<AutofacTestClass>();
            for (int j = 0; j < 100; j++)
            {
                var cb = new ContainerBuilder();
                cb.RegisterType<AutofacTestClass>();
                using var container = cb.Build();
                var obj = container.Resolve<Func<object, AutofacTestClass>>();
                var testObj = obj(new object());
                objs.Add(testObj);
            }
            objs = null;
            if (doGc)
            {
                DoGC();
            }
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }
    }

    [JSExport]
    public static async Task CompiledFuncExpressions(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            var objects = new List<object>();
            for (int j = 0; j < 100; j++)
            {
                var func = Expression.Lambda<Func<object, object>>(Expression.Constant(new object()), Expression.Parameter(typeof(object))).Compile();
                var obj = func(new object());
                objects.Add(obj);
            }
            objects = null;
            if (doGc)
            {
                DoGC();
            }
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }
    }

    [JSExport]
    public static async Task Events(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                var action1 = new Action(() => {});
                var action2 = new Action(() => {});
                var obj = new EventTestClass();
                obj.Event += action1;
                obj.Event += action2;
                obj.Event -= action1;
                obj.Event -= action2;
            }
            if (doGc)
            {
                DoGC();
            }
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }
    }

     [JSExport]
    public static async Task Actions(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            var actions = new List<Action>();
            for (int j = 0; j < 100; j++)
            {
                var action = new Action(() => {});
                actions.Add(action);
                action();
            }
            actions = null;
            if (doGc)
            {
                DoGC();
            }
            PrintMemory($"#{i}");
            await Task.Delay(16);
        }
    }

    [JSExport]
    public static void DoGC()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private static void PrintMemory(string prefix)
    {
        var stats = GetCurrentStatistics();
        Console.WriteLine(
            $"{prefix} "
            + $"| WHS: {FormatBytes(stats.WasmHeapSize)} "
            + $"| SBRK: {FormatBytes(stats.WasmSbrkPtr)} "
            + $"| TM: {FormatBytes(stats.TotalMemory)} "
            + $"| TAB: {FormatBytes(stats.TotalAllocatedBytes)} "
            + $"| G0: {stats.Generation0Collections} "
            + $"| G1: {stats.Generation1Collections} "
            + $"| G2: {stats.Generation2Collections} "
            + $"| HSB: {FormatBytes(stats.HeapSizeBytes)} "
            + $"| FB: {FormatBytes(stats.FragmentedBytes)} "
            + $"| TCB: {FormatBytes(stats.TotalCommittedBytes)}");
    }

    public static GcStatistics GetCurrentStatistics()
    {
        var stats = new GcStatistics
        {
            Generation0Collections = GC.CollectionCount(0),
            Generation1Collections = GC.CollectionCount(1),
            Generation2Collections = GC.CollectionCount(2),
            TotalMemory = GC.GetTotalMemory(false),
            TotalAllocatedBytes = GC.GetTotalAllocatedBytes(),
        };

        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            stats.HeapSizeBytes = gcInfo.HeapSizeBytes;
            stats.FragmentedBytes = gcInfo.FragmentedBytes;
            stats.TotalCommittedBytes = gcInfo.TotalCommittedBytes;
            stats.MemoryLoadBytes = gcInfo.MemoryLoadBytes;
            stats.TotalAvailableMemoryBytes = gcInfo.TotalAvailableMemoryBytes;
            stats.HasDetailedInfo = true;
        }
        catch
        {
            stats.HeapSizeBytes = stats.TotalMemory;
            stats.HasDetailedInfo = false;
        }

        stats.WasmHeapSize = Emscripten.emscripten_get_heap_size();
        unsafe
        {
            stats.WasmSbrkPtr = new IntPtr(*(uint*)Emscripten.emscripten_get_sbrk_ptr());
        }

        return stats;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 0)
        {
            return string.Empty;
        }
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }
        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:N3} KB";
        }
        if (bytes < 1024 * 1024 * 1024)
        {
            return $"{bytes / 1024.0 / 1024.0:N3} MB";
        }
        return $"{bytes / 1024.0 / 1024.0 / 1024.0:N3} GB";
    }
}

public class EventTestClass
{
    public event Action Event;
}

public class AutofacTestClass
{
    private object _value;
    public AutofacTestClass(object value)
    {
        _value = value;
    }
}

public sealed class DecoratorFunc<TInterface>
{
    public readonly Func<IContainer, TInterface> Func;

    public DecoratorFunc(Func<IContainer, TInterface> func)
    {
        Func = func;
    }
}

public class TestDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
}

public class GcStatistics
{
    public int Generation0Collections { get; set; }
    public int Generation1Collections { get; set; }
    public int Generation2Collections { get; set; }
    public long TotalMemory { get; set; }
    public long TotalAllocatedBytes { get; set; }

    // Detailed heap information (available when HasDetailedInfo is true)
    public long HeapSizeBytes { get; set; }
    public long FragmentedBytes { get; set; }
    public long TotalCommittedBytes { get; set; }
    public long MemoryLoadBytes { get; set; }
    public long TotalAvailableMemoryBytes { get; set; }
    public bool HasDetailedInfo { get; set; }

    // Wasm heap
    public IntPtr WasmHeapSize { get; set; }
    public IntPtr WasmSbrkPtr { get; set; }
}

public static unsafe class Emscripten
{
    [DllImport("*")]
    public static unsafe extern IntPtr emscripten_get_sbrk_ptr();

    [DllImport("*")]
    public static unsafe extern IntPtr emscripten_get_heap_size();
}
