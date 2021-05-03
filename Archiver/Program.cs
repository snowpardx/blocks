using CommonTypes;
using System;

namespace Archiver
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid Syntax, use the following one 'archiver compress|decompress <inputFile> <outputFile>'");
                return 1;
            }
            bool compress = false;
            if (string.Compare(args[0], "compress", true) == 0)
            {
                compress = true;
            } else if (string.Compare(args[0], "decompress", true) == 0)
            {
                compress = false;
            } else
            {
                Console.WriteLine($"Expected 'compress' or 'decompress' command, but found '{args[0]}'");
                return 1;
            }
            try
            {
                using(var blocks = CompressAlgorithmBlocks.Init(args[1], args[2]))
                {
                    var algorithm = new ParallelAlgorithm<byte[], byte[]>(blocks.GetBlock, blocks.ProcessBlock, blocks.UseResult);
                    return algorithm.Run();
                }

            } catch (HandledException e)
            {
                return 1;
            } catch (Exception e)
            {
                Console.WriteLine($"Unhandled exception: {e.Message}");
                return 1;
            }

        }
    }
}
