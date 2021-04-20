using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class MultiConstructorTest
    {
        private interface IService1
        {
        }

        private interface IService2
        {
        }

        public class Service1 : IService1
        {
        }

        public class Service2 : IService2
        {
        }

        private class ServiceA
        {
            public string Prop { get; }
            public string ConstructorCalled { get; }
            public IService1 Svc1 { get; }
            public IService2 Svc2 { get; }

            public ServiceA()
            {
                ConstructorCalled = "()";
            }

            public ServiceA(int x)
            {
                ConstructorCalled = "(int)";
            }

            public ServiceA(string prop)
            {
                ConstructorCalled = "(string)";
                Prop = prop;
            }

            public ServiceA(IService1 svc1)
            {
                ConstructorCalled = "(IService1)";
                Svc1 = svc1;
            }

            public ServiceA(IService2 svc2)
            {
                ConstructorCalled = "(IService2)";
                Svc2 = svc2;
            }

            public ServiceA(IService1 svc1, string prop)
            {
                ConstructorCalled = "(IService1,string)";
                Svc1 = svc1;
                Prop = prop;
            }

            public ServiceA(IService2 svc2, string prop)
            {
                ConstructorCalled = "(IService2,string)";
                Svc2 = svc2;
                Prop = prop;
            }

            public ServiceA(IService1 svc1, IService2 svc2)
            {
                ConstructorCalled = "(IService1,IService2)";
                Svc1 = svc1;
                Svc2 = svc2;
            }

            public ServiceA(IService1 svc1, IService2 svc2, int x)
            {
                ConstructorCalled = "(IService1,IService2,int)";
                Svc1 = svc1;
                Svc2 = svc2;
            }

            public ServiceA(IService1 svc1, IService2 svc2, string prop)
            {
                ConstructorCalled = "(IService1,IService2,string)";
                Svc1 = svc1;
                Svc2 = svc2;
                Prop = prop;
            }

            public ServiceA(IService1 svc1, IService2 svc2, string prop, int x)
            {
                ConstructorCalled = "(IService1,IService2,string,int)";
                Svc1 = svc1;
                Svc2 = svc2;
                Prop = prop;
            }

        }

        [Test]
        [Repeat(1000)]
        public void Test1()
        {
            var repo = new ServiceRepository();

            var svc = repo.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("()"));
            Assert.That(svc.Svc1, Is.Null);
            Assert.That(svc.Svc2, Is.Null);
            Assert.That(svc.Prop, Is.Null);
        }

        [Test]
        [Repeat(1000)]
        public void Test2()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());

            var svc = repo.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService1)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Null);
            Assert.That(svc.Prop, Is.Null);
        }

        [Test]
        [Repeat(1000)]
        public void Test3()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service2());

            var svc = repo.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService2)"));
            Assert.That(svc.Svc1, Is.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Prop, Is.Null);
        }

        [Test]
        [Repeat(1000)]
        public void Test4()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());
            repo.Register(new Service2());

            var svc = repo.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService1,IService2)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Prop, Is.Null);
        }

        [Test]
        [Repeat(1000)]
        public void Test5()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());

            var svc = repo.Create<ServiceA>("X");

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService1,string)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Null);
            Assert.That(svc.Prop, Is.EqualTo("X"));
        }

        [Test]
        [Repeat(1000)]
        public void Test6()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service2());

            var svc = repo.Create<ServiceA>("X");

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService2,string)"));
            Assert.That(svc.Svc1, Is.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Prop, Is.EqualTo("X"));
        }

        [Test]
        [Repeat(1000)]
        public void Test7()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());
            repo.Register(new Service2());

            var svc = repo.Create<ServiceA>("X");

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService1,IService2,string)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Prop, Is.EqualTo("X"));
        }

    }
}