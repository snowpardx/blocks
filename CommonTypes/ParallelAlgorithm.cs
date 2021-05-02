using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CommonTypes
{
    /// <summary>
    /// This class represents any algoritm for scheme map -> processing -> result consolidation.
    /// There could be bunch of parameters and euristics (like number of threads used, full CPU utilization, etc),
    /// By the way to keep things simple I gonna just have fixed number of worker threads = number of cores, where
    /// ProducerConsumer queue and OrderedProducerConsumerSink are filled to max 2 * number of worker threads (roughly),
    /// So it should have limited memory usage.
    /// </summary>
    public class ParallelAlgorithm<Data, Result>
        where Data: class
        where Result: class
    {
        private Func<Data> getDataBlock;
        private Func<Data, Result> processData;
        private Action<Result> useResult;
        private ProducerConsumer<Data> tasks = new ProducerConsumer<Data>();
        private OrderedProducerConsumerSink<Result> results = new OrderedProducerConsumerSink<Result>();
        private int concurrency = Environment.ProcessorCount;
        private List<Thread> workingThreads;

        public ParallelAlgorithm(Func<Data> getDataBlock, Func<Data, Result> processData, Action<Result> useResult)
        {
            this.getDataBlock = getDataBlock;
            this.processData = processData;
            this.useResult = useResult;
            // assume that getting the data is easy operation where CPU usage is neglectable,
            // the same is true for using the result, so only data processing tasks are counted when using concurrency
            // we would use the thread executed Run for gathering the data and a separate thread for using the data, so that not get into deadlock
            workingThreads = new List<Thread>(concurrency + 1);
            workingThreads.Add(new Thread(this.UseDataTask));
            for (var i = 0; i < concurrency; i++)
            {
                workingThreads.Add(new Thread(this.ProcessDataTask));
            }
        }

        public void Run()
        {
            workingThreads.ForEach(thread => thread.Start());
            while(true)
            {
                if (tasks.Size() > 2 * concurrency || results.Size() > 2 * concurrency)
                {
                    Thread.Yield();
                    continue;
                }
                var data = getDataBlock();
                if (data == null)
                {
                    tasks.Stop();
                    break;
                }
                tasks.Enqueue(data);
            }
            workingThreads.ForEach(thread => thread.Join());
        }

        private void ProcessDataTask()
        {
            while(true)
            {
                var taskInfo = tasks.Dequeue();
                if(taskInfo == null)
                {
                    return;
                }
                var result = processData(taskInfo.Item2);
                results.Enqueue(taskInfo.Item1, result);
            }
        }

        private void UseDataTask()
        {
            while(true)
            {
                var result = results.Dequeue();
                if(result == null)
                {
                    return;
                }
                useResult(result);
            }
        }
    }
}
