using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotNet_RwayTrie.DataStructures.RWayTrie;
using dotNet_RwayTrie.DataStructures.RWayTrieDt;
using dotNet_RwayTrie.DataStructures.RWayTrieStd;

namespace dotNet_RwayTrie
{
    class Program
    {
        static void Main(string[] args)
        {

            var list = new List<(byte[], String)>();

            var swStd = new Stopwatch();

            for (int i = 0; i < 3000; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var keyStr = $"Item {i}.{Guid.NewGuid()}";
                    var key = Encoding.UTF8.GetBytes(keyStr);

                    list.Add((key, keyStr));
                }
            }

            Console.WriteLine($"Inserting {list.Count} Guid Keys");
            Console.WriteLine();

            Test1(list);
            Console.WriteLine();

            Test2(list);
            Console.WriteLine();

            Test3(list);
            Console.WriteLine();

            Console.ReadLine();
        }

        static void Test1(List<(byte[], String)> list)
        {
            var currMem = GC.GetTotalMemory(true);

            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var rw = new RWayTrie<String>();

            foreach (var (key, val) in list)
            {
                sw1.Start();
                rw.Insert(key, val);
                sw1.Stop();
            }

            var totalMem = GC.GetTotalMemory(true);
            var diffMem = totalMem - currMem;

            Console.WriteLine($"RWayTrie (Optimized) - INSERT: {sw1.ElapsedMilliseconds,4:####} Ms - {diffMem,12:############} Bytes - {(diffMem / 1024D),12:########.000} Kb - {(diffMem / 1024D / 1024D),8:####.###} Mb");

            foreach (var (key, val) in list)
            {
                sw2.Start();
                var pairs = rw.Search(key).ToList();
                sw2.Stop();
            }

            Console.WriteLine($"RWayTrie (Optimized) - SEARCH: {sw2.ElapsedMilliseconds,4:####} Ms - {list.Count,12:############} prefix");
            Console.WriteLine($"GC Collect");

            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        static void Test2(List<(byte[], String)> list)
        {
            var currMem = GC.GetTotalMemory(true);

            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var rw = new RWayTrieDt<String>();

            foreach (var (key, val) in list)
            {
                sw1.Start();
                rw.Insert(key, val);
                sw1.Stop();
            }

            var totalMem = GC.GetTotalMemory(true);
            var diffMem = totalMem - currMem;

            Console.WriteLine($"RWayTrie (Dt)        - INSERT: {sw1.ElapsedMilliseconds,4:####} Ms - {diffMem,12:############} Bytes - {(diffMem / 1024D),12:########.000} Kb - {(diffMem / 1024D / 1024D),8:####.###} Mb");


            foreach (var (key, val) in list)
            {
                sw2.Start();
                var pairs = rw.Search(key).ToList();
                sw2.Stop();
            }

            Console.WriteLine($"RWayTrie (Dt)        - SEARCH: {sw2.ElapsedMilliseconds,4:####} Ms - {list.Count,12:############} prefix");
            Console.WriteLine($"GC Collect");

            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        static void Test3(List<(byte[], String)> list)
        {
            var currMem = GC.GetTotalMemory(true);

            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var rw = new RWayTrieStd<String>();

            foreach (var (key, val) in list)
            {
                sw1.Start();
                rw.Insert(key, val);
                sw1.Stop();
            }

            var totalMem = GC.GetTotalMemory(true);
            var diffMem = totalMem - currMem;

            Console.WriteLine($"RWayTrie (Std)       - INSERT: {sw1.ElapsedMilliseconds,4:####} Ms - {diffMem,12:############} Bytes - {(diffMem / 1024D),12:########.000} Kb - {(diffMem / 1024D / 1024D),8:####.###} Mb");


            foreach (var (key, val) in list)
            {
                sw2.Start();
                var pairs = rw.Search(key).ToList();
                sw2.Stop();
            }

            Console.WriteLine($"RWayTrie (Std)       - SEARCH: {sw2.ElapsedMilliseconds,4:####} Ms - {list.Count,12:############} prefix");
            Console.WriteLine($"GC Collect");

            GC.Collect();
            GC.WaitForFullGCComplete();
        }
    }
}
