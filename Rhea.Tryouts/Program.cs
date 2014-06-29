using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Rhea.Compression;

namespace Rhea.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllLines(@"C:\Users\Ayende\Downloads\RadnomUsers\RadnomUsers.json");

            //var trainer = new CompressionTrainer();

            //for (int i = 0; i < json.Length/100; i++)
            //{
            //    trainer.TrainOn(json[i*100]);
            //}

            //var compressionHandler = trainer.CreateHandler();

            //using (var file = File.Create("compression.dic"))
            //{
            //    compressionHandler.Save(file);
            //    file.Flush();
            //}
            //return;

            CompressionHandler compressionHandler;
            using (var file = File.OpenRead("compression.dic"))
            {
                compressionHandler = CompressionHandler.Load(file);
            }

            var items = new List<byte[]>();
            int size = 0;
            int compressedSize = 0;
            var ms = new MemoryStream();
            int count = 0;
            foreach (var s in json)
            {
                if (count++%1000 == 0)
                    Console.WriteLine(count);
                size += s.Length;
                ms.SetLength(0);
                compressedSize += compressionHandler.Compress(s, ms);
                items.Add(ms.ToArray());
            }

            //Console.WriteLine(size);
            //Console.WriteLine(compressedSize);

            for (int i = 0; i < items.Count; i++)
            {
                if(count-- % 1000 == 0)
                    Console.WriteLine(count);
                var memoryStream = new MemoryStream(items[i]);
                var decompress = compressionHandler.Decompress(memoryStream);
                Debug.Assert(decompress.Length == json[i].Length);

                for (int j = 0; j < decompress.Length; j++)
                {
                    if (((char)decompress[j] != json[i][j]))
                    {
                        Console.WriteLine("BAD");

                        Console.WriteLine(json[i]);
                        Console.WriteLine(Encoding.UTF8.GetString(decompress));
                        break;
                    }
                }
            }

            Console.WriteLine("All good");
        }
    }
}
