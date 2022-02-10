using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class EnumerableTest
    {
        private interface IService1
        {
        }

        private interface IService2
        {
        }

        private interface IService3
        {
        }

        private class Service1 : IService1
        {
            public static int CreateCount;

            public Service1()
            {
                Interlocked.Increment(ref CreateCount);
            }
        }

        private class Service2 : IService1
        {
            public static int CreateCount;

            public Service2()
            {
                Interlocked.Increment(ref CreateCount);
            }
        }

        private class ServiceX
        {
            public IEnumerable<IService1> Services { get; }

            public ServiceX(IEnumerable<IService1> services)
            {
                Services = services;
            }
        }

        private class ServiceY
        {
            public IList<IService1> Services { get; }

            public ServiceY(IList<IService1> services)
            {
                Services = services;
            }
        }

        [Test]
        public void TestResolveEnumerable()
        {
            var svc1a = new Service1();
            var svc1b = new Service2();
            var svc1c = new Service1();

            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc1a);
            repo.Register(svc1b);
            repo.Register(svc1c);

            var provider = repo.CreateServiceProvider();

            var s1 = provider.Resolve<IEnumerable<IService1>>().ToArray();

            Assert.That(s1.Length, Is.EqualTo(3));

            AssertX.AllSame<Service1>(svc1a, s1[0]);
            AssertX.AllSame<Service2>(svc1b, s1[1]);
            AssertX.AllSame<Service1>(svc1c, s1[2]);
        }

        [Test]
        public void TestResolveList()
        {
            var svc1a = new Service1();
            var svc1b = new Service2();
            var svc1c = new Service1();
            

            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc1a);
            repo.Register(svc1b);
            repo.Register(svc1c);

            var provider = repo.CreateServiceProvider();

            var s1 = provider.Resolve<IList<IService1>>();

            Assert.That(s1.Count, Is.EqualTo(3));

            AssertX.AllSame<Service1>(svc1a, s1[0]);
            AssertX.AllSame<Service2>(svc1b, s1[1]);
            AssertX.AllSame<Service1>(svc1c, s1[2]);
        }

        [Test]
        public void TestResolveEnumerable_CheckDelayed()
        {
            Service1.CreateCount = 0;
            Service2.CreateCount = 0;

            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1>();
            repo.Register<Service2>();
            repo.Register<Service1>();

            var provider = repo.CreateServiceProvider();

            var s1 = provider.Resolve<IEnumerable<IService1>>();

            int n1 = 0;
            int n2 = 0;

            Assert.That(Service1.CreateCount, Is.EqualTo(0));

            foreach (var svc in s1)
            {
                if (svc is Service1)
                    n1++;
                else if (svc is Service2)
                    n2++;
                else
                    Assert.Fail();

                Assert.That(Service1.CreateCount, Is.EqualTo(n1));
                Assert.That(Service2.CreateCount, Is.EqualTo(n2));
            }

            Assert.That(Service1.CreateCount, Is.EqualTo(2));
            Assert.That(Service2.CreateCount, Is.EqualTo(1));
        }

        [Test]
        public void TestResolveList_CheckDelayed()
        {
            Service1.CreateCount = 0;
            Service2.CreateCount = 0;

            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1>();
            repo.Register<Service2>();
            repo.Register<Service1>();

            var provider = repo.CreateServiceProvider();

            var s1 = provider.Resolve<IList<IService1>>();

            Assert.That(Service1.CreateCount, Is.EqualTo(2));
            Assert.That(Service2.CreateCount, Is.EqualTo(1));

            for (var i = 0; i < s1.Count; i++)
            {
                if (i == 0 || i == 2)
                    Assert.That(s1[i], Is.InstanceOf<Service1>());
                if (i == 1)
                    Assert.That(s1[i], Is.InstanceOf<Service2>());
            }

            Assert.That(Service1.CreateCount, Is.EqualTo(2));
            Assert.That(Service2.CreateCount, Is.EqualTo(1));
        }

    }
}