using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CommonTypes
{
    public class OrderedProducerConsumerSink<T> where T: class
    {
        private Dictionary<int, T> results = new Dictionary<int, T>();
        private object mutex = new object();
        private bool isDead = false;
        private int consumerBlockIndex = 0;

        public void Enqueue(int index, T result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            lock (mutex)
            {
                if (isDead)
                {
                    throw new InvalidOperationException("Sink already stopped");
                }
                results.Add(index, result);
                Monitor.Pulse(mutex);
            }
        }

        public T Dequeue()
        {
            lock (mutex)
            {
                T result = null;
                while (!isDead && !results.TryGetValue(consumerBlockIndex, out result))
                {
                    Monitor.Wait(mutex);
                }
                if (result != null)
                {
                    results.Remove(consumerBlockIndex);
                    consumerBlockIndex++;
                    return result;
                }


                if (results.Count == 0)
                {
                    return null;
                }

                throw new InvalidOperationException("There are orphaned results in sink");
            }
        }

        public int Size()
        {
            lock(mutex)
            {
                return results.Count;
            }
        }

        public void Stop()
        {
            lock (mutex)
            {
                isDead = true;
                Monitor.PulseAll(mutex);
            }
        }
    }
}
