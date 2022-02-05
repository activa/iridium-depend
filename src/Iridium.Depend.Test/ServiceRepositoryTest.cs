using System;
using System.Net.Cache;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class ServiceRepositoryTest
    {
        private interface IService1
        {
            bool Disposed { get; } 
        }

        private interface IService2
        {
            bool Disposed { get; }
        }

        private interface IService3
        {
            bool Disposed { get; }
        }

        private class Service1A : IService1, IDisposable
        {
            public int ID;

            public Service1A(){}
            public Service1A(int id) => ID = id;

            public void Dispose() { Disposed = true; }

            public bool Disposed { get; private set; }
        }

        private class Service1B : IService1
        {
            public bool Disposed { get; private set; }
        }

        private class Service2A : IService2
        {
            public bool Disposed { get; private set; }

            public IService1 Svc1;
            public string Prop;
            public int X;

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
                X = x;
            }
        }

        private class ServiceX
        {
            public IService1 Svc1;

            public ServiceX()
            {
            }

            public ServiceX(IService1 svc)
            {
                Svc1 = svc;
            }
        }

        private class ServiceY
        {
            public IService2 Svc2;

            public ServiceY(IService2 svc)
            {
                Svc2 = svc;
            }
        }

        public class Service12A : IService1, IService2
        {
            public bool Disposed { get; private set; }
        }

        public class Service12B : IService1, IService2
        {
            public bool Disposed { get; private set; }
        }

        public interface IGenericService1<T>
        {
        }

        public class GenericService1<T> : IGenericService1<T>
        {
        }


        [Test]
        public void SimpleInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();
            repo.Register<Service1A>();

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IService1>();
            var s2 = serviceProvider.Get<IService1>();
            var s3 = serviceProvider.Get<IService3>();

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

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get(typeof(IService1));
            var s2 = serviceProvider.Get(typeof(IService1));
            var s3 = serviceProvider.Get(typeof(IService3));

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

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IService1>();
            var s2 = serviceProvider.Get<IService1>();

            Assert.That(s1, Is.InstanceOf<Service1A>());
            Assert.That(s2, Is.InstanceOf<Service1A>());
            Assert.That(s1, Is.SameAs(s2));
        }


        [Test]
        public void SimpleInterface_WithDependency()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IService2>();

            Assert.That(s1, Is.InstanceOf<Service2A>());
            Assert.That(((Service2A)s1).Svc1, Is.Null);

            repo.Register<Service1A>();

            s1 = serviceProvider.Get<IService2>();

            Assert.That(s1, Is.InstanceOf<Service2A>());
            Assert.That(((Service2A)s1).Svc1, Is.InstanceOf<Service1A>());
        }

        [Test]
        public void SimpleInstance()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc = new Service1B();

            repo.Register(svc);

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IService1>(), Is.SameAs(svc));
        }

        [Test]
        public void MultiInstance1()
        {
            Service12A svc12a = new Service12A();
            Service12A svc12b = new Service12A();

            ServiceRepository repo = new ServiceRepository();

            repo.Register<IService1>(svc12a);
            repo.Register<IService2>(svc12b);

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IService1>(), Is.SameAs(svc12a));
            Assert.That(serviceProvider.Get<IService2>(), Is.SameAs(svc12b));
        }

        [Test]
        public void MultiInstance2()
        {
            Service12A svc12a = new Service12A();
            Service12A svc12b = new Service12A();

            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc12a).As<IService1>();
            repo.Register(svc12b).As<IService2>();

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IService1>(), Is.SameAs(svc12a));
            Assert.That(serviceProvider.Get<IService2>(), Is.SameAs(svc12b));
        }

        [Test]
        public void MultiInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService1>();
            repo.Register<Service12B>().As<IService2>();

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IService1>(), Is.InstanceOf<Service12A>());
            Assert.That(serviceProvider.Get<IService2>(), Is.InstanceOf<Service12B>());
        }

        [Test]
        public void TypeWithoutAs()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>();

            var provider = repo.CreateServiceProvider();

            Assert.That(provider.Get<IService1>(), Is.InstanceOf<Service12A>());
            Assert.That(provider.Get<IService2>(), Is.InstanceOf<Service12A>());
            Assert.That(provider.Get<Service12A>(), Is.InstanceOf<Service12A>());
        }

        [Test]
        public void TypeAsInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService1>();

            var provider = repo.CreateServiceProvider();

            Assert.That(provider.Get<IService1>(), Is.InstanceOf<Service12A>());
            Assert.That(provider.Get<IService2>(), Is.Null);
            Assert.That(provider.Get<Service1A>(), Is.Null);
        }

        [Test]
        public void TypeAsMultipleInterfaces()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>()
                .As<IService1>()
                .As<IService2>();

            var provider = repo.CreateServiceProvider();

            Assert.That(provider.Get<IService1>(), Is.InstanceOf<Service12A>());
            Assert.That(provider.Get<IService2>(), Is.InstanceOf<Service12A>());
            Assert.That(provider.Get<Service12A>(), Is.Null);
        }

        [Test]
        public void TypeAsInterfaceAndSelf()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>()
                .AsSelf()
                .As<IService2>();

            var provider = repo.CreateServiceProvider();

            Assert.That(provider.Get<IService1>(), Is.Null);
            Assert.That(provider.Get<IService2>(), Is.InstanceOf<Service12A>());
            Assert.That(provider.Get<Service12A>(), Is.InstanceOf<Service12A>());
        }


        [Test]
        public void Create1()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc1 = new Service1A();

            repo.Register(svc1);

            var serviceProvider = repo.CreateServiceProvider();

            var svc3 = serviceProvider.Create<ServiceX>();

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3.Svc1, Is.SameAs(svc1));
        }

        [Test]
        public void Create1_NonGeneric()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc1 = new Service1A();

            repo.Register(svc1);

            var serviceProvider = repo.CreateServiceProvider();

            var svc3 = serviceProvider.Create(typeof(ServiceX));

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3, Is.InstanceOf<ServiceX>());
            Assert.That(((ServiceX)svc3).Svc1, Is.SameAs(svc1));
        }

        [Test]
        public void Create2()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();
            repo.Register<Service2A>();

            var serviceProvider = repo.CreateServiceProvider();

            var svc3 = serviceProvider.Create<ServiceX>();
            var svc4 = serviceProvider.Create<ServiceY>();

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

            var serviceProvider = repo.CreateServiceProvider();

            var svc3 = serviceProvider.Create<ServiceX>();

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3.Svc1, Is.Null);
        }

        [Test]
        public void Create4()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();

            var serviceProvider = repo.CreateServiceProvider();

            var svc4 = serviceProvider.Create<ServiceY>();

            Assert.That(svc4, Is.Not.Null);
            Assert.That(svc4.Svc2, Is.Null);
        }

        [Test]
        public void Create_Fail()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();

            var serviceProvider = repo.CreateServiceProvider();

            var svc3 = serviceProvider.Create<IService3>();

            Assert.That(svc3, Is.Null);
        }

        [Test]
        public void UnregisterMultiple()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();
            repo.Register<Service1B>();

            var serviceProvider = repo.CreateServiceProvider();

            // repo.UnRegister<Service1A>();
            //
            // Assert.That(serviceProvider.Get<IService1>(), Is.InstanceOf<Service1B>());
        }

        [Test]
        public void UnregisterSingle()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();

            // repo.UnRegister<Service1A>();
            //
            // var serviceProvider = repo.CreateServiceProvider();
            //
            // Assert.That(serviceProvider.Get<IService1>(), Is.Null);
        }

        [Test]
        public void Multiple1()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();
            repo.Register<Service1B>();

            var serviceProvider = repo.CreateServiceProvider();

            // Assert.That(serviceProvider.Get<IService1>(), Is.InstanceOf<Service1A>());
            //
            // repo.UnRegister<IService1>();
            //
            // Assert.That(serviceProvider.Get<IService1>(), Is.Null);
        }

        [Test]
        public void Multiple2()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>();
            repo.Register<Service1B>();

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IService1>(), Is.InstanceOf<Service1B>());
        }

        [Test]
        public void Multiple_Replace()
        {
            // ServiceRepository repo = new ServiceRepository();
            //
            // repo.Register<Service1B>();
            // repo.Register<Service1A>().Replace<IService1>();
            //
            // var serviceProvider = repo.CreateServiceProvider();
            //
            // Assert.That(serviceProvider.Get<IService1>(), Is.InstanceOf<Service1A>());
            //
            // repo = new ServiceRepository();
            //
            // serviceProvider = repo.CreateServiceProvider();
            //
            // repo.Register<Service1B>();
            // repo.Register<Service1A>().Replace(typeof(IService1));
        }

        [Test]
        public void RegisterFactory1()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<IService1>(() => new Service1A(123));

            var serviceProvider = repo.CreateServiceProvider();

            var service1 = serviceProvider.Get<IService1>();

            Assert.That(service1, Is.InstanceOf<Service1A>());
            Assert.That(((Service1A)service1).ID, Is.EqualTo(123));
        }

        [Test]
        public void RegisterFactory2()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<IService1>(svc => svc.Create<Service1A>(123));

            var serviceProvider = repo.CreateServiceProvider();

            var service1 = serviceProvider.Get<IService1>();

            Assert.That(service1, Is.InstanceOf<Service1A>());
            Assert.That(((Service1A)service1).ID, Is.EqualTo(123));
        }

        [Test]
        public void CreateObjectWithParam()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();
            repo.Register<Service1A>();

            var serviceProvider = repo.CreateServiceProvider();

            var svc2a = serviceProvider.Get<IService2>();

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.Null);

            svc2a = serviceProvider.Create<Service2A>("Hello");

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.EqualTo("Hello"));
        }


        [Test]
        public void CreateObjectWithParam_NonGeneric()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service2A>();
            repo.Register<Service1A>();

            var serviceProvider = repo.CreateServiceProvider();

            var svc2a = serviceProvider.Get<IService2>();

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.Null);

            svc2a = (Service2A) serviceProvider.Create(typeof(Service2A),"Hello");

            Assert.That(svc2a, Is.InstanceOf<Service2A>());

            Assert.That(((Service2A)svc2a).Prop, Is.EqualTo("Hello"));
        }

        [Test]
        public void TestDoubleRegistration_Transient()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>();
            repo.Register<Service1B>().As<IService1>();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();
            var svc2 = provider.Get<IService1>();

            Assert.That(svc1, Is.InstanceOf<Service1B>());
            Assert.That(svc1, Is.Not.SameAs(svc2));
        }

        [Test]
        public void TestDoubleRegistration_Scoped()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>().Scoped();
            repo.Register<Service1B>().As<IService1>().Scoped();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();
            var svc2 = provider.Get<IService1>();

            var scope = provider.CreateScope();

            var svc1a = scope.Get<IService1>();
            var svc2a = scope.Get<IService1>();

            Assert.That(svc1, Is.InstanceOf<Service1B>());
            Assert.That(svc1a, Is.InstanceOf<Service1B>());

            Assert.That(svc1, Is.SameAs(svc2));
            Assert.That(svc1a, Is.SameAs(svc2a));
            Assert.That(svc1, Is.Not.SameAs(svc1a));
        }

        [Test]
        public void TestDoubleRegistration_Singleton()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>().Singleton();
            repo.Register<Service1B>().As<IService1>().Singleton();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();
            var svc2 = provider.Get<IService1>();

            var scope = provider.CreateScope();

            var svc1a = scope.Get<IService1>();
            var svc2a = scope.Get<IService1>();

            Assert.That(svc1, Is.InstanceOf<Service1B>());
            Assert.That(svc1a, Is.InstanceOf<Service1B>());

            Assert.That(svc1, Is.SameAs(svc2));
            Assert.That(svc1a, Is.SameAs(svc2a));
            Assert.That(svc1, Is.SameAs(svc1a));
        }

        [Test]
        public void TestDoubleRegistration_IfNotExists_Transient()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>();
            repo.Register<Service1B>().As<IService1>().IfNotRegistered<IService1>();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();
            var svc2 = provider.Get<IService1>();

            Assert.That(svc1, Is.InstanceOf<Service1A>());
            Assert.That(svc1, Is.Not.SameAs(svc2));
        }

        [Test]
        public void TestDoubleRegistration_IfNotExists_Scoped()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>().Scoped();
            repo.Register<Service1B>().As<IService1>().Scoped().IfNotRegistered<IService1>();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();
            var svc2 = provider.Get<IService1>();

            var scope = provider.CreateScope();

            var svc1a = scope.Get<IService1>();
            var svc2a = scope.Get<IService1>();

            Assert.That(svc1, Is.InstanceOf<Service1A>());
            Assert.That(svc1a, Is.InstanceOf<Service1A>());

            Assert.That(svc1, Is.SameAs(svc2));
            Assert.That(svc1a, Is.SameAs(svc2a));
            Assert.That(svc1, Is.Not.SameAs(svc1a));
        }

        [Test]
        public void TestDoubleRegistration_IfNotExists_Singleton()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>().Singleton();
            repo.Register<Service1B>().As<IService1>().Singleton().IfNotRegistered<IService1>();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();
            var svc2 = provider.Get<IService1>();

            var scope = provider.CreateScope();

            var svc1a = scope.Get<IService1>();
            var svc2a = scope.Get<IService1>();

            Assert.That(svc1, Is.InstanceOf<Service1A>());
            Assert.That(svc1a, Is.InstanceOf<Service1A>());

            Assert.That(svc1, Is.SameAs(svc2));
            Assert.That(svc1a, Is.SameAs(svc2a));
            Assert.That(svc1, Is.SameAs(svc1a));
        }

        [Test]
        public void RegisterNull()
        {
            Assert.Catch<ArgumentNullException>(() => new ServiceRepository().Register<IService1>((IService1)null));
        }

        [Test]
        public void TestScopeDispose1()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>().As<IService1>().Scoped();
            repo.Register<Service2A>().As<IService2>().Scoped();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Get<IService1>();
            var svc2 = provider.Get<IService2>();

            Assert.That(svc1, Is.Not.Null);
            Assert.That(provider.Get<IService1>(), Is.SameAs(svc1));
            Assert.That(provider.Get<IService1>(), Is.SameAs(svc1));

            var scope = provider.CreateScope();

            var svc1a = scope.Get<IService1>();
            var svc1b = scope.Get<IService1>();
            var svc2a = scope.Get<IService2>();
            var svc2b = scope.Get<IService2>();

            Assert.That(svc1a, Is.Not.Null);
            Assert.That(svc1b, Is.SameAs(svc1a));
            Assert.That(svc1a, Is.Not.SameAs(svc1));

            Assert.That(svc2a, Is.Not.Null);
            Assert.That(svc2b, Is.SameAs(svc2a));
            Assert.That(svc2a, Is.Not.SameAs(svc2));

            scope.Dispose();

            Assert.That(svc1.Disposed, Is.False);
            Assert.That(svc2.Disposed, Is.False);
            Assert.That(svc1a.Disposed, Is.True);
            Assert.That(svc2a.Disposed, Is.False);
        }
    }
}