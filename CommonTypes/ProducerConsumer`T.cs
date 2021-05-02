using System;
using System.Collections.Generic;
using System.Threading;

namespace CommonTypes
{
    //based on https://ru.stackoverflow.com/a/428328 with some minor code style updates
    public class ProducerConsumer<T> where T : class
    {
        private object mutex = new object();
        private Queue<T> queue = new Queue<T>();
        private bool isDead = false;
        private int blockIndex = 0;

        public void Enqueue(T task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            lock (mutex)
            {
                if (isDead)
                {
                    throw new InvalidOperationException("Queue already stopped");
                }
                queue.Enqueue(task);
                Monitor.Pulse(mutex);
            }
        }

        public Tuple<int, T> Dequeue()
        {
            lock (mutex)
            {
                while (queue.Count == 0 && !isDead)
                {
                    Monitor.Wait(mutex);
                }

                if (queue.Count == 0)
                {
                    return null;
                }

                var currentBlockIndex = blockIndex;
                blockIndex++;
                return new Tuple<int, T>(currentBlockIndex, queue.Dequeue());
            }
        }

        public int Size()
        {
            lock(mutex)
            {
                return queue.Count;
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
