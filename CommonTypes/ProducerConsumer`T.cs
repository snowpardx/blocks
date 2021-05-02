using System;
using System.Collections.Generic;
using System.Threading;

namespace CommonTypes
{
    //https://ru.stackoverflow.com/a/428328
    public class ProducerConsumer<T> where T : class
    {
        object mutex = new object();
        Queue<T> queue = new Queue<T>();
        bool isDead = false;

        public void Enqueue(T task)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            lock (mutex)
            {
                if (isDead)
                    throw new InvalidOperationException("Queue already stopped");
                queue.Enqueue(task);
                Monitor.Pulse(mutex);
            }
        }

        public T Dequeue()
        {
            lock (mutex)
            {
                while (queue.Count == 0 && !isDead)
                    Monitor.Wait(mutex);

                if (queue.Count == 0)
                    return null;

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
