using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class LazyTest
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
        }

        private class Service2 : IService2
        {
        }

        private class TargetService
        {
            private readonly IService1 _svc1;
            private readonly Lazy<IService2> _svc2;

            public IService1 Svc1 => _svc1;
            public IService2 Svc2 => _svc2.Value;

            public TargetService(IService1 svc1, Lazy<IService2> svc2)
            {
                _svc1 = svc1;
                _svc2 = svc2;
            }
        }

        private class TargetService2
        {
            [Inject] public IService1 _svc1 { get; set; }
            [Inject] public Lazy<IService2> _svc2 { get; set; }

            public IService1 Svc1 => _svc1;
            public IService2 Svc2 => _svc2.Value;
        }

        [Test]
        public void TestNothing()
        {
            ServiceRepository repo = new ServiceRepository();

            var obj = repo.Create<TargetService>();

            Assert.That(obj.Svc1, Is.Null);
            Assert.That(obj.Svc2, Is.Null);
        }

        [Test]
        public void TestLazyNotResolving()
        {
            var svc1 = new Service1();

            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc1);

            var obj = repo.Create<TargetService>();

            Assert.That(obj.Svc1, Is.SameAs(svc1));
            Assert.That(obj.Svc2, Is.Null);
        }

        [Test]
        public void TestLazyResolving()
        {
            var svc1 = new Service1();
            var svc2 = new Service2();

            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc1);
            repo.Register(svc2);

            var obj = repo.Create<TargetService>();

            Assert.That(obj.Svc1, Is.SameAs(svc1));
            Assert.That(obj.Svc2, Is.SameAs(svc2));
        }

        [Test]
        public void TestNothingWithProperties()
        {
            ServiceRepository repo = new ServiceRepository();

            var obj = repo.Create<TargetService2>();

            Assert.That(obj.Svc1, Is.Null);
            Assert.That(obj.Svc2, Is.Null);
        }

        [Test]
        public void TestLazyNotResolvingWithProperties()
        {
            var svc1 = new Service1();

            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc1);

            var obj = repo.Create<TargetService2>();

            Assert.That(obj.Svc1, Is.SameAs(svc1));
            Assert.That(obj.Svc2, Is.Null);
        }

        [Test]
        public void TestLazyResolvingWithProperties()
        {
            var svc1 = new Service1();
            var svc2 = new Service2();

            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc1);
            repo.Register(svc2);

            var obj = repo.Create<TargetService2>();

            Assert.That(obj.Svc1, Is.SameAs(svc1));
            Assert.That(obj.Svc2, Is.SameAs(svc2));
        }
    }
}