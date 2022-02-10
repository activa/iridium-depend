using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class ConcurrenyTests
    {
        private class Service1
        {
            public static int timesConstructed = 0;

            public int X { get; }

            public Service1(int x)
            {
                Interlocked.Increment(ref timesConstructed);

                X = x;
            }
        }

        private int _numTasks = 10000;
        private int _numScopes = 100;

        [Test]
        public void Test_Transient()
        {
            Service1.timesConstructed = 0;

            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1>();

            var serviceProvider = repo.CreateServiceProvider();

            Parallel.For(0, _numTasks, i =>
            {
                var svc1 = serviceProvider.Get<Service1>(i);
                ;
                Assert.That(svc1, Is.Not.Null);
                Assert.That(svc1.X, Is.EqualTo(i));
            });

            Assert.That(Service1.timesConstructed, Is.EqualTo(_numTasks));
        }

        [Test]
        public void Test_Singleton()
        {
            Service1.timesConstructed = 0;

            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1>().Singleton();

            var serviceProvider = repo.CreateServiceProvider();

            ConcurrentBag<Service1> constructedServices = new ConcurrentBag<Service1>();

            Parallel.For(0, _numTasks, i =>
            {
                var svc = serviceProvider.Get<Service1>(i);

                Assert.That(svc, Is.InstanceOf<Service1>());

                constructedServices.Add(svc);
            });

            Assert.That(Service1.timesConstructed, Is.EqualTo(1));
            AssertX.AllSame(constructedServices.Cast<object>().ToArray());
        }

        [Test]
        public void Test_Scoped()
        {
            Service1.timesConstructed = 0;

            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1>().Scoped();

            var serviceProvider = repo.CreateServiceProvider();

            var scopes = Enumerable.Range(0, _numScopes).Select(_ => new { scope = serviceProvider.CreateScope(), constructedServices = new ConcurrentBag<Service1>()}).ToArray();
            
            Parallel.For(0, _numTasks, i =>
            {
                var svc = scopes[i%_numScopes].scope.Get<Service1>(i);

                scopes[i%_numScopes].constructedServices.Add(svc);

                Assert.That(svc, Is.InstanceOf<Service1>());
            });

            Assert.That(Service1.timesConstructed, Is.EqualTo(_numScopes));

            foreach (var scope in scopes)
            {
                AssertX.AllSame(scope.constructedServices.Cast<object>().ToArray());
            }
        }

    }
}