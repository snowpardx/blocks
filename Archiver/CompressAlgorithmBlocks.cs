using CommonTypes;
using System;
using System.IO;
using System.IO.Compression;

namespace Archiver
{
    class CompressAlgorithmBlocks: IDisposable, AlgorithmBlocks
    {
        private FileStream inputFileStream;
        private BufferedStream inputBufferedStream;
        private BinaryReader binaryReader;
        private FileStream outputFileStream;
        private BufferedStream outputBufferedStream;
        private BinaryWriter binaryWriter;
        private const int blockLength = 1024 * 1024; // 1MiB
        private bool readingFinished = false;
        private const double resizeFactor = 1.25; // used to set initial memory stream size in case zipped data would take more memory

        private CompressAlgorithmBlocks() { }

        public static CompressAlgorithmBlocks Init(string inputFilePath, string outputFilePath)
        {
            var blocks = new CompressAlgorithmBlocks();
            try
            {
                blocks.inputFileStream = File.OpenRead(inputFilePath);
                blocks.inputBufferedStream = new BufferedStream(blocks.inputFileStream);
                blocks.binaryReader = new BinaryReader(blocks.inputBufferedStream);
            } catch (FileNotFoundException e)
            {
                Console.WriteLine($"Unable to find on the path: {inputFilePath}");
                throw new HandledException(e);
            }
            try
            {
                blocks.outputFileStream = File.Open(outputFilePath, FileMode.Create);
                blocks.outputBufferedStream = new BufferedStream(blocks.outputFileStream);
                blocks.binaryWriter = new BinaryWriter(blocks.outputBufferedStream);
            } catch (FileNotFoundException e)
            {
                Console.WriteLine($"Unable to create file on the path: {outputFilePath}");
                throw new HandledException(e);
            }
            return blocks;
        }

        public void Dispose()
        {
            binaryReader?.Dispose();
            inputBufferedStream?.Dispose();
            inputFileStream?.Dispose();

            binaryWriter?.Dispose();
            outputBufferedStream?.Dispose();
            outputFileStream?.Dispose();
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

        public byte[] ProcessBlock(byte[] data)
        {
            using (MemoryStream resultStream = new MemoryStream((int)(data.Length * resizeFactor)))
            {
                using (GZipStream zipStream = new GZipStream(resultStream, CompressionMode.Compress))
                {
                    zipStream.Write(data);
                    zipStream.Flush();
                    return resultStream.ToArray();
                }
            }
        }
        public void UseResult(byte[] result)
        {
            binaryWriter.Write(result.Length); // we would use this when decompress, so that read the data corresponding to compressed block
            binaryWriter.Write(result);
        }

    }
}
