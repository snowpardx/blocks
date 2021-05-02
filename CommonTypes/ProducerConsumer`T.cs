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

        public T Dequeue()
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

                return queue.Dequeue();
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
