using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RxTest
{
    class Corecursion
    {
        public static IEnumerable<T> Unfold<T>(T seed, Func<T, T> accumulator)
        {
            var nextValue = seed;
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
                yield return nextValue;
                nextValue = accumulator(nextValue);
            }
        }
    }
}
