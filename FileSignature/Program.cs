using CommonTypes;
using System;

namespace FileSignature
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid Syntax, use the following one 'signature <inputFile> <blockSizeInBytes>'");
                return 1;
            }
            if (!int.TryParse(args[1], out int blockSizeInBytes))
            {
                Console.WriteLine($"Unable to parse block size from: '{args[1]}'");
                return 1;
            }
            try
            {
                using(var blocks = AlgorithmBlocks.Init(args[0], blockSizeInBytes))
                {
                    var algorithm = new ParallelAlgorithm<byte[], string>(blocks.GetBlock, blocks.ProcessBlock, blocks.UseResult);
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
