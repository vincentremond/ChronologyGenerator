using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronologyGenerator
{
    public static class Helper
    {
        public static IEnumerable<IList<T>> Split<T>(this IList<T> list, int size)
        {
            var batchNumber = 0;
            while (true)
            {
                var result = list.Skip(batchNumber * size).Take(size).ToList();
                if (result.Any())
                {
                    yield return result;
                    batchNumber++;
                }
                else
                {
                    break;
                }
            }
        }

        public static T GetByIndexOrDefault<T>(this IList<T> list, int index)
        {
            return (index < list.Count) ? list[index] : default(T);
        }
    }
}
