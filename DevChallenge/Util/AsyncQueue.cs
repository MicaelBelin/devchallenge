using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevChallenge.Util
{
    /// <summary>
    /// Represents an asyncronous queue. 
    /// All available members are thread-safe.
    /// </summary>
    /// <typeparam name="T">Specifies the item type</typeparam>
    public class AsyncQueue<T>
    {
        /// <summary>
        /// Returns the items in the queue as an enumerable. Blocks until queue is closed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> AsEnumerable()
        {
            lock (queue)
            {
                while (isopen)
                {
                    foreach (var item in queue) yield return item;
                    queue.Clear();
                    Monitor.Wait(queue);
                }
                foreach (var item in queue) yield return item;
            }
        }

        /// <summary>
        /// Pops the frontmost item in the queue. If queue is active and no items are available, it blocks until an item is.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no items are available and queue is closed</exception>
        /// <returns>The frontmost item in the queue</returns>
        public T Pop()
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (!isopen) throw new InvalidOperationException("Queue is closed");
                    Monitor.Wait(queue);
                }
                var ret = queue[0];
                queue.RemoveAt(0);
                return ret;
            }
        }

        /// <summary>
        /// Pops the frontmost item in the queue. If queue is active and no items are available, it blocks until an item is available, or the given timespan times out.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no items are available and queue is closed</exception>
        /// <exception cref="TimeoutException">Thrown if no items are available and no item gets available until timeout expires</exception>
        /// <returns>The frontmost item in the queue</returns>
        public T Pop(TimeSpan timeout)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (!isopen) throw new InvalidOperationException("Queue is closed");
                    if (!Monitor.Wait(queue, timeout)) throw new TimeoutException();
                }
                var ret = queue[0];
                queue.RemoveAt(0);
                return ret;
            }
        }

        /// <summary>
        /// Adds the item to the queue.
        /// If the queue is closed, adding a new item into the queue reopens the queue.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            lock (queue)
            {
                isopen = true;
                queue.Add(item);
                Monitor.Pulse(queue);
            }
        }

        /// <summary>
        /// Closes the queue.
        /// If any items are still available in the queue, those items are returned before queue is closed.
        /// </summary>
        public void Close()
        {
            lock (queue)
            {
                isopen = false;
                Monitor.Pulse(queue);
            }
        }


        List<T> queue = new List<T>();
        bool isopen = true;


    }
}
