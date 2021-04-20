using System;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class ServiceRepositoryTest
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

        private class Service1A : IService1
        {
        }

        private class Service1B : IService1
        {
        }

        private class Service2A : IService2
        {
            public IService1 Svc1;
            public string Prop;

            public Service2A(IService1 service1)
            {
                Svc1 = service1;
            }

            public Service2A(string prop, IService1 service1)
            {
                Svc1 = service1;
                Prop = prop;
            }

            public Service2A(int x)
            {

            }
        }

        private class Service3
        {
            public IService1 Svc1;

            public Service3()
            {

            }

            public Service3(IService1 svc)
            {
                Svc1 = svc;
            }
        }

        private class Service4
        {
            public IService2 Svc2;

            public Service4(IService2 svc)
            {
                Svc2 = svc;
            }
        }

        public class Service12A : IService1, IService2
        {
        }

        public class Service12B : IService1, IService2
        {
        }

        public interface IGenericServica1<T>
        {
        }

        public class GenericService1<T> : IGenericServica1<T>
        {
        }


        [Test]
        public void SimpleInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();
            repo.Register<Service1A>();

            var s1 = repo.Get<IService1>();
            var s2 = repo.Get<IService1>();
            var s3 = repo.Get<IService3>();

            Assert.That(s1, Is.InstanceOf<Service1A>());
            Assert.That(s2, Is.InstanceOf<Service1A>());
            Assert.That(s1, Is.Not.SameAs(s2));
            Assert.That(s3, Is.Null);
        }

        [Test]
        public void SimpleInterface_NonGeneric()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();
            repo.Register<Service1A>();

            var s1 = repo.Get(typeof(IService1));
            var s2 = repo.Get(typeof(IService1));
            var s3 = repo.Get(typeof(IService3));

            Assert.That(s1, Is.InstanceOf<Service1A>());
            Assert.That(s2, Is.InstanceOf<Service1A>());
            Assert.That(s1, Is.Not.SameAs(s2));
            Assert.That(s3, Is.Null);
        }

        [Test]
        public void SimpleInterface_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>().Singleton();

            var s1 = repo.Get<IService1>();
            var s2 = repo.Get<IService1>();

            Assert.That(s1, Is.InstanceOf<Service1A>());
            Assert.That(s2, Is.InstanceOf<Service1A>());
            Assert.That(s1, Is.SameAs(s2));
        }

        [Test]
        public void SimpleInterface_WithDependency()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();

            var s1 = repo.Get<IService2>();

            Assert.That(s1, Is.InstanceOf<Service2A>());
            Assert.That(((Service2A)s1).Svc1, Is.Null);

            Assert.That(repo.Register<Service1A>().RegisteredAsType, Is.EqualTo(typeof(Service1A)));

            s1 = repo.Get<IService2>();

            Assert.That(s1, Is.InstanceOf<Service2A>());
            Assert.That(((Service2A)s1).Svc1, Is.InstanceOf<Service1A>());
        }

        [Test]
        public void SimpleInstance()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc = new Service1B();

            repo.Register(svc);

            Assert.That(repo.Get<IService1>(), Is.SameAs(svc));
        }

        [Test]
        public void MultiInstance1()
        {
            Service12A svc12a = new Service12A();
            Service12A svc12b = new Service12A();

            ServiceRepository repo = new ServiceRepository();

            repo.Register<IService1>(svc12a);
            repo.Register<IService2>(svc12b);

            Assert.That(repo.Get<IService1>(), Is.SameAs(svc12a));
            Assert.That(repo.Get<IService2>(), Is.SameAs(svc12b));
        }

        [Test]
        public void MultiInstance2()
        {
            Service12A svc12a = new Service12A();
            Service12A svc12b = new Service12A();

            ServiceRepository repo = new ServiceRepository();

            Assert.That(repo.Register(svc12a).As<IService1>().RegisteredAsType, Is.EqualTo(typeof(IService1)));
            Assert.That(repo.Register(svc12b).As<IService2>().RegisteredAsType, Is.EqualTo(typeof(IService2)));

            Assert.That(repo.Get<IService1>(), Is.SameAs(svc12a));
            Assert.That(repo.Get<IService2>(), Is.SameAs(svc12b));
        }

        [Test]
        public void MultiInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService1>();
            repo.Register<Service12B>().As<IService2>();

            Assert.That(repo.Get<IService1>(), Is.InstanceOf<Service12A>());
            Assert.That(repo.Get<IService2>(), Is.InstanceOf<Service12B>());
        }


        [Test]
        public void Create1()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc1 = new Service1A();

            repo.Register(svc1);

            var svc3 = repo.Create<Service3>();

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3.Svc1, Is.SameAs(svc1));
        }

        [Test]
        public void Create1_NonGeneric()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc1 = new Service1A();

            repo.Register(svc1);

            var svc3 = repo.Create(typeof(Service3));

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3, Is.InstanceOf<Service3>());
            Assert.That(((Service3)svc3).Svc1, Is.SameAs(svc1));
        }

        [Test]
        public void Create2()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();
            repo.Register<Service2A>();

            var svc3 = repo.Create<Service3>();
            var svc4 = repo.Create<Service4>();

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc4, Is.Not.Null);
            Assert.That(svc3.Svc1, Is.InstanceOf<Service1A>());
            Assert.That(svc4.Svc2, Is.InstanceOf<Service2A>());
            Assert.That(((Service2A)svc4.Svc2).Svc1, Is.InstanceOf<Service1A>());
        }

        [Test]
        public void Create3()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();

            var svc3 = repo.Create<Service3>();

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3.Svc1, Is.Null);
        }

        [Test]
        public void Create4()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();

            var svc4 = repo.Create<Service4>();

            Assert.That(svc4, Is.Not.Null);
            Assert.That(svc4.Svc2, Is.Null);
        }

        [Test]
        public void Create_Fail()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();

            var svc3 = repo.Create<IService3>();

            Assert.That(svc3, Is.Null);
        }

        [Test]
        public void UnregisterMultiple()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();
            repo.Register<Service1B>();

            repo.UnRegister<Service1A>();

            Assert.That(repo.Get<IService1>(), Is.InstanceOf<Service1B>());
        }

        [Test]
        public void UnregisterSingle()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();

            repo.UnRegister<Service1A>();

            Assert.That(repo.Get<IService1>(), Is.Null);
        }

        [Test]
        public void Multiple1()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();
            repo.Register<Service1B>();

            Assert.That(repo.Get<IService1>(), Is.InstanceOf<Service1A>());

            repo.UnRegister<IService1>();

            Assert.That(repo.Get<IService1>(), Is.Null);
        }

        [Test]
        public void Multiple2()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1B>();
            repo.Register<Service1A>();

            Assert.That(repo.Get<IService1>(), Is.InstanceOf<Service1B>());
        }

        [Test]
        public void Multiple_Replace()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1B>();
            repo.Register<Service1A>().Replace<IService1>();

            Assert.That(repo.Get<IService1>(), Is.InstanceOf<Service1A>());

            repo = new ServiceRepository();

            repo.Register<Service1B>();
            repo.Register<Service1A>().Replace(typeof(IService1));
        }


        [Test]
        public void CreateObjectWithParam()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();
            repo.Register<Service1A>();

            var svc2a = repo.Get<IService2>();

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.Null);

            svc2a = repo.Create<Service2A>("Hello");

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.EqualTo("Hello"));
        }


        [Test]
        public void CreateObjectWithParam_NonGeneric()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();
            repo.Register<Service1A>();

            var svc2a = repo.Get<IService2>();

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.Null);

            svc2a = (Service2A) repo.Create(typeof(Service2A),"Hello");

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.EqualTo("Hello"));
        }

        [Test]
        public void RegisterNull()
        {
            Assert.Catch<ArgumentNullException>(() => new ServiceRepository().Register<IService1>(null));
        }

        [Test]
        public void RegisterFail()
        {
            Assert.Catch<ArgumentException>(() => new ServiceRepository().Register<Service1A>().As<IService2>());
        }

    }
}