using System;
using System.Collections.Generic;
using System.Text;

namespace Archiver
{
    interface AlgorithmBlocks: IDisposable
    {
        byte[] GetBlock();
        byte[] ProcessBlock(byte[] data);
        void UseResult(byte[] result);
    }
}
