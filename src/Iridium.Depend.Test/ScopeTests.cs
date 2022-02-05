using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class ScopeTests
    {
        private interface IService1
        {
            int ID { get; set; }
        }

        private interface IService2
        {
        }

        private class Service1A : IService1
        {
            public int ID { get; set; }

            public Service1A() { }
            public Service1A(int id) => ID = id;
        }

        private class Service1B : IService1
        {
            public int ID { get; set; }
        }

        private class Service2A : IService2
        {
            public IService1 Svc1;

            public Service2A(IService1 service1)
            {
                Svc1 = service1;
            }
        }

        [Test]
        public void TestSingleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>().Singleton();

            var provider = repo.CreateServiceProvider();

            IService1 svc1;

            svc1 = provider.Get<IService1>();
            
            Assert.That(provider.Get<IService1>(), Is.SameAs(svc1));
            Assert.That(provider.Get<IService1>(), Is.SameAs(svc1));

            var scope = provider.CreateScope();

            Assert.That(scope.Get<IService1>(), Is.SameAs(svc1));
        }

        [Test]
        public void TestScoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>().Scoped();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();

            Assert.That(svc1, Is.Not.Null);
            Assert.That(provider.Get<IService1>(), Is.SameAs(svc1));
            Assert.That(provider.Get<IService1>(), Is.SameAs(svc1));

            var scope = provider.CreateScope();

            var svc1a = scope.Get<IService1>();

            Assert.That(svc1a, Is.Not.Null);
            Assert.That(svc1a, Is.Not.SameAs(svc1));
            
            Assert.That(scope.Get<IService1>(), Is.SameAs(svc1a));
            Assert.That(scope.Get<IService1>(), Is.SameAs(svc1a));
        }

        [Test]
        public void TestRegisteredSingleton()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc = new Service1A();

            repo.Register(svc).As<IService1>();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();

            Assert.That(provider.Get<IService1>(), Is.SameAs(svc));
            Assert.That(provider.Get<IService1>(), Is.SameAs(svc1));

            var scope = provider.CreateScope();

            Assert.That(scope.Get<IService1>(), Is.SameAs(svc1));
        }


    }

    [TestFixture]
    public class DisposeScopeTests
    {
        [Test]
        public void Test1()
        {
        }
    }
}