using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySimpleDictionary.Interfaces;
using MySimpleDictionary;

class Program
{
    static void Main()
    {
        const int numElements = 100_000;

        Console.WriteLine("=== FUNKCIONALNI TEST ===\n");
        TestFunctionality();

        Console.WriteLine("\n=== BENCHMARK POREDJENJE ===");
        Console.WriteLine($"Broj elemenata: {numElements}\n");

        var myDict = new MySimpleDictionary<int, string>();
        var sysDict = new Dictionary<int, string>();

        var myResults = BenchmarkCustom(myDict, numElements);
        var sysResults = BenchmarkDotNetDictionary(sysDict, numElements);

        Console.WriteLine($"Ukupan broj testova (custom): {myResults.Count}");
        Console.WriteLine($"Ukupan broj testova (system): {sysResults.Count}");

        PrintComparisonTable(myResults, sysResults);

        Console.WriteLine("\nPritisnite Enter za kraj...");
        Console.ReadLine();
    }

    static void TestFunctionality()
    {
        IMySimpleDictionary<int, string> dict = new MySimpleDictionary<int, string>();

        Console.WriteLine("Dodavanje 5 elemenata...");
        for (int i = 1; i <= 5; i++)
        {
            dict.Add(i, $"Vrijednost {i}");
            Console.WriteLine($"Dodato: [{i}] = Vrijednost {i}");
        }

        Console.WriteLine("\nPrikaz svih ključeva:");
        foreach (var key in dict.Keys)
            Console.WriteLine($"Ključ: {key}");

        Console.WriteLine("\nPrikaz svih vrijednosti:");
        foreach (var val in dict.Values)
            Console.WriteLine($"Vrijednost: {val}");

        Console.WriteLine("\nProvjera ContainsKey i ContainsValue:");
        Console.WriteLine($"ContainsKey(3): {dict.ContainsKey(3)}");
        Console.WriteLine($"ContainsValue(\"Vrijednost 4\"): {dict.ContainsValue("Vrijednost 4")}");

        Console.WriteLine("\nPristup elementu preko indeksa:");
        Console.WriteLine($"dict[2] = {dict[2]}");

        Console.WriteLine("\nIteriranje kroz cijeli riječnik:");
        foreach (var kvp in dict)
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");

        Console.WriteLine("\nBrisanje ključa 3...");
        dict.Remove(3);
        Console.WriteLine($"ContainsKey(3): {dict.ContainsKey(3)}");

        // Test brisanja nepostojećeg ključa
        Console.WriteLine("\nTestiranje brisanja ključa koji ne postoji:");
        bool removed = dict.Remove(999); 
        Console.WriteLine($"Rezultat: {removed}");

        Console.WriteLine("\nDodavanje novog elementa na mjesto obrisanog ključa 3...");
        dict.Add(3, "Nova vrijednost za ključ 3");
        Console.WriteLine($"dict[3] = {dict[3]}");

        Console.WriteLine("\nProvjera ContainsKey(3) nakon ponovnog dodavanja:");
        Console.WriteLine(dict.ContainsKey(3));

        Console.WriteLine("\nBrisanje svih elemenata...");
        dict.Clear();
        Console.WriteLine($"Count nakon Clear: {dict.Count}");

    }

    class BenchmarkResult
    {
        public string Name = "";
        public double AvgMilliseconds = 0;
        public int Operations = 0;
        public double NanosecondsPerOp => (AvgMilliseconds * 1_000_000.0) / Math.Max(1, Operations);
    }

    static List<BenchmarkResult> BenchmarkCustom(IMySimpleDictionary<int, string> dict, int num)
    {
        var results = new List<BenchmarkResult>();

        results.Add(RunTest("Dodavanje", num, () =>
        {
            dict.Clear();
            for (int i = 0; i < num; i++) dict.Add(i, "V" + i);
        }));

        results.Add(RunTest("Pretraga (ContainsKey)", num, () =>
        {
            for (int i = 0; i < num; i++) _ = dict.ContainsKey(i);
        }));

        results.Add(RunTest("Dohvatanje (indeksa)", num, () =>
        {
            for (int i = 0; i < num; i++) _ = dict[i];
        }));

        int smallNum = Math.Min(1000, num);
        results.Add(RunTest("Pretraga (ContainsValue)", smallNum, () =>
        {
            for (int i = 0; i < smallNum; i++) dict.ContainsValue("V" + i);
        }));

        results.Add(RunTest($"Brisanje {num / 2}", num / 2, () =>
        {
            for (int i = 0; i < num / 2; i++) dict.Remove(i);
        }));

        results.Add(RunTest("Clear", 1, () =>
        {
            dict.Clear();
        }));

        return results;
    }

    static List<BenchmarkResult> BenchmarkDotNetDictionary(Dictionary<int, string> dict, int num)
    {
        var results = new List<BenchmarkResult>();

        results.Add(RunTest("Dodavanje", num, () =>
        {
            dict.Clear();
            for (int i = 0; i < num; i++) dict.Add(i, "V" + i);
        }));

        results.Add(RunTest("Pretraga (ContainsKey)", num, () =>
        {
            for (int i = 0; i < num; i++) _ = dict.ContainsKey(i);
        }));

        results.Add(RunTest("Dohvatanje (indeksa)", num, () =>
        {
            for (int i = 0; i < num; i++) _ = dict[i];
        }));

        int smallNum = Math.Min(1000, num);
        results.Add(RunTest("Pretraga (ContainsValue)", smallNum, () =>
        {
            for (int i = 0; i < smallNum; i++) dict.ContainsValue("V" + i);
        }));

        results.Add(RunTest($"Brisanje {num / 2}", num / 2, () =>
        {
            for (int i = 0; i < num / 2; i++) dict.Remove(i);
        }));

        results.Add(RunTest("Clear", 1, () =>
        {
            dict.Clear();
        }));

        return results;
    }

    static BenchmarkResult RunTest(string name, int ops, Action action)
    {
        // Warm-up
        action();

        const int runs = 3;
        double totalMs = 0;

        for (int i = 0; i < runs; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            totalMs += sw.Elapsed.TotalMilliseconds;
        }

        double avgMs = totalMs / runs;

        Console.WriteLine($"{name,-25}: {avgMs,8:F3} ms | ~{(avgMs * 1_000_000 / ops),8:F0} ns/op | ops={ops}");
        return new BenchmarkResult { Name = name, AvgMilliseconds = avgMs, Operations = ops };
    }

    static void PrintComparisonTable(List<BenchmarkResult> customResults, List<BenchmarkResult> systemResults)
    {
        Console.WriteLine("\n===== UPOTREBLJENOST VREMENA (ms i ns/op) =====");
        Console.WriteLine("{0,-25} | {1,15} | {2,15}", "Operacija", "MySimpleDictionary", "Dictionary<TKey,TValue>");
        Console.WriteLine(new string('-', 65));

        for (int i = 0; i < customResults.Count; i++)
        {
            var c = customResults[i];
            var s = systemResults[i];

            Console.WriteLine("{0,-25} | {1,8:F3}ms | {2,8:F0}ns/op | {3,8:F3}ms | {4,8:F0}ns/op",
                c.Name, c.AvgMilliseconds, c.NanosecondsPerOp, s.AvgMilliseconds, s.NanosecondsPerOp);
        }
    }
}
