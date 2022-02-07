using System.Linq;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    public static class AssertX
    {
        public static void AllNull(params object[] objs)
        {
            Assert.That(objs, Is.All.Null);
        }

        public static void AllNotNull(params object[] objs)
        {
            Assert.That(objs, Is.All.Not.Null);
        }

        public static void AllDifferent(params object[] objs)
        {
            if (objs.Length < 2)
                return;
            
            var firstEqual = Enumerable.Range(0, objs.Length)
                    .SelectMany(i1 => Enumerable.Range(i1 + 1, objs.Length - i1 - 1).Select(i2 => new { i1, i2, obj1 = objs[i1], obj2 = objs[i2] }))
                    .FirstOrDefault(x => ReferenceEquals(x.obj1, x.obj2))
                ;

            if (firstEqual != null)
            {
                Assert.Fail($"Objects at indexes {firstEqual.i1} and {firstEqual.i2} are same. {firstEqual.obj1}=={firstEqual.obj2}");
            }
        }

        public static void AllDifferent<T>(params object[] objs)
        {
            AllInstanceOf<T>(objs);
            AllDifferent(objs);
        }

        public static void AllSame(params object[] objs)
        {
            if (objs.Length < 2)
                return;

            var firstDifference = Enumerable.Range(1, objs.Length - 1).FirstOrDefault(i => !ReferenceEquals(objs[i], objs[0]));

            if (firstDifference > 0)
            {
                Assert.Fail($"Object at index {firstDifference} is not same: {objs[0]}!={objs[firstDifference]}");
            }
        }

        public static void AllSame<T>(params object[] objs)
        {
            AllInstanceOf<T>(objs);
            AllSame(objs);
        }

        public static void AllInstanceOf<T>(params object[] objs)
        {
            Assert.That(objs, Is.All.InstanceOf<T>());
        }

        public static void AllEqual<T>(params T[] items)
        {
            if (items.Length < 2)
                return;

            var firstDifference = Enumerable.Range(1, items.Length - 1).FirstOrDefault(i => !items[i].Equals(items[0]));

            if (firstDifference > 0)
            {
                Assert.Fail($"Item at index {firstDifference} is not equal: {items[0]}!={items[firstDifference]}");
            }
        }


    }
}