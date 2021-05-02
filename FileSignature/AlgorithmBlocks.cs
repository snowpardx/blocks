using CommonTypes;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace FileSignature
{
    class AlgorithmBlocks: IDisposable
    {
        private FileStream fileStream;
        private BufferedStream bufferedStream;
        private BinaryReader binaryReader;
        private int blockLength;
        private bool readingFinished = false;
        private int blockNumber = 0;
        private ThreadLocal<SHA256> hashAlgorithm = new ThreadLocal<SHA256>(trackAllValues: true);

        private AlgorithmBlocks() { }

        public static AlgorithmBlocks Init(string filePath, int blockLength)
        {
            var blocks = new AlgorithmBlocks();
            try
            {
                blocks.blockLength = blockLength;
                blocks.fileStream = File.OpenRead(filePath);
                blocks.bufferedStream = new BufferedStream(blocks.fileStream);
                blocks.binaryReader = new BinaryReader(blocks.bufferedStream);
                return blocks;
            } catch (FileNotFoundException e)
            {
                Console.WriteLine($"Unable to find on the path: {filePath}");
                throw new HandledException(e);
            }
        }

        public void Dispose()
        {
            foreach(var instance in hashAlgorithm.Values)
            {
                instance.Dispose();
            }
            hashAlgorithm.Dispose();
            binaryReader?.Dispose();
            bufferedStream?.Dispose();
            fileStream?.Dispose();
        }

        public byte[] GetBlock()
        {
            if (readingFinished)
            {
                return null;
            }
            var block = binaryReader.ReadBytes(blockLength);
            if(block.Length < blockLength)
            {
                readingFinished = true;
                if(block.Length == 0)
                {
                    return null;
                }
            }
            return block;
        }

        public string ProcessBlock(byte[] data)
        {
            if(!hashAlgorithm.IsValueCreated)
            {
                hashAlgorithm.Value = SHA256.Create();
            }
            var hash = hashAlgorithm.Value.ComputeHash(data);
            // hash to string is from https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-5.0
            var sBuilder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sBuilder.Append(hash[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public void UseResult(string result)
        {
            Console.WriteLine($"{blockNumber}: {result}");
            blockNumber++;
        }
    }
}
