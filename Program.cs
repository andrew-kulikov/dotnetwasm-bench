using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
unsafe
{
    Console.WriteLine($"Heap size: {Emscripten.emscripten_get_heap_size()}");
    Console.WriteLine($"SBRK ptr : {*(uint*)Emscripten.emscripten_get_sbrk_ptr()}");
    Console.WriteLine($"Page size: {Mono.mono_pagesize()}");
}

public static unsafe class Mono
{
    [DllImport("*")]
    public static unsafe extern IntPtr mono_pagesize();
}

public static unsafe class Emscripten
{
    [DllImport("*")]
    public static unsafe extern IntPtr emscripten_get_sbrk_ptr();

    [DllImport("*")]
    public static unsafe extern IntPtr emscripten_get_heap_size();
}

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
            PrintMemory($"FS #{i} - start");
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
            PrintMemory($"FS #{i} - end");
        }
    }

    [JSExport]
    public static async Task AllocateStrings(int count, bool doGc)
    {
        for (int i = 0; i < count; i++)
        {
            PrintMemory($"AS #{i} - start");
            var str = new string('a', random.Next(1000, 10_000));
            if (doGc)
            {
                DoGC();
            }
            await Task.Delay(16);
            PrintMemory($"AS #{i} - end");
        }
    }

    [JSExport]
    public static async Task LosFragmentation(int count, int batchSize, int size)
    {
        var losHolder = new List<byte[]>();
        for (int i = 0; i < count; i++)
        {
            PrintMemory($"LF #{i} - start");

            for (int j = 0; j < batchSize; j++)
            {
                losHolder.Add(new byte[size]);
            }
            await Task.Delay(16);
            // Collect to recalculate fragmentation stats
            GC.Collect();
            PrintMemory($"LF #{i} - end");
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
        var total = GC.GetTotalMemory(false);
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);
        var gcInfo = GC.GetGCMemoryInfo();

        var sb = new StringBuilder();
        sb.Append($"{prefix} | ");
        sb.Append($"Total Memory: {FormatBytes(total)} | ");
        sb.Append($"HeapSizeBytes: {FormatBytes(gcInfo.HeapSizeBytes)} | ");
        sb.Append($"FragmentedBytes: {FormatBytes(gcInfo.FragmentedBytes)} | ");
        sb.Append($"TotalCommittedBytes: {FormatBytes(gcInfo.TotalCommittedBytes)} | ");
        sb.Append($"G0: {gen0} | ");
        sb.Append($"G1: {gen1} | ");
        sb.Append($"G2: {gen2} | ");
   
        Console.WriteLine(sb.ToString());
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
            return $"{bytes / 1024.0:N2} KB";
        }
        if (bytes < 1024 * 1024 * 1024)
        {
            return $"{bytes / 1024.0 / 1024.0:N2} MB";
        }
        return $"{bytes / 1024.0 / 1024.0 / 1024.0:N2} GB";
    }
}

public class TestDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
}
