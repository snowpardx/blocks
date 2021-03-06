using CommonTypes;
using System;
using System.IO;
using System.IO.Compression;

namespace Archiver
{
    class DecompressAlgorithmBlocks: IDisposable, AlgorithmBlocks
    {
        private FileStream inputFileStream;
        private BufferedStream inputBufferedStream;
        private BinaryReader binaryReader;
        private FileStream outputFileStream;
        private BufferedStream outputBufferedStream;
        private BinaryWriter binaryWriter;
        private bool readingFinished = false;
        private const double resizeFactor = 1.5; // used to set initial memory stream size in case zipped data would take more memory

        private DecompressAlgorithmBlocks() { }

        public static DecompressAlgorithmBlocks Init(string inputFilePath, string outputFilePath)
        {
            var blocks = new DecompressAlgorithmBlocks();
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
            int blockLength;
            try
            {
                blockLength = binaryReader.ReadInt32(); // before each block we wrote the integer denoting the block length
            }
            catch (EndOfStreamException e)
            {
                readingFinished = true;
                return null;
            }
            var block = binaryReader.ReadBytes(blockLength);
            if(block.Length < blockLength)
            {
                Console.WriteLine("Part of file is missed");
                throw new HandledException(new Exception("Part of file is missed"));
            }
            return block;
        }

        public byte[] ProcessBlock(byte[] data)
        {
            using(MemoryStream inputStream = new MemoryStream(data))
            {
                using (MemoryStream resultStream = new MemoryStream((int)(data.Length * resizeFactor)))
                {
                    using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }

        public void UseResult(byte[] result)
        {
            binaryWriter.Write(result);
        }
    }
}
