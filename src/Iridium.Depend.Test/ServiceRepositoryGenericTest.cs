using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class ServiceRepositoryGenericTest
    {
        private interface IGenericService1<T>
        {
        }

        private interface IGenericService2<T>
        {
        }

        private class GenSvc<T1, T2>
        {
            public GenSvc()
            {
            }

            public GenSvc(IGenericService1<T1> svc1, IGenericService1<T2> svc2)
            {
            }
        }


        private class GenericService1<T> : IGenericService1<T>
        {
        }

        private class GenericService2<T> : IGenericService2<T>
        {
        }

        private class SimpleClass1
        {
        }

        private class SimpleClass2
        {
        }

        private class GenericServiceWithParam<T> : IGenericService2<T>
        {
            public T Svc1;

            public GenericServiceWithParam()
            {
            }

            public GenericServiceWithParam(T service1)
            {
                Svc1 = service1;
            }

            public GenericServiceWithParam(int x)
            {

            }
        }


        private class GenericService12A<T> : IGenericService1<T>, IGenericService2<T>
        {
        }

        private class GenericService12B<T> : IGenericService1<T>, IGenericService2<T>
        {
        }

        private class GenericService1Int : IGenericService1<int>
        {
        }

        private class GenericService1Bool : IGenericService1<bool>
        {
        }

        private class GenericService1A<T> : IGenericService1<T>, IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator()
            {
                return (new List<int>()).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class TestClass<T>
        {
            public TestClass(IGenericService1<T> p)
            {
            }
        }

        [Test]
        public void AssignableTests()
        {
            Assert.True(typeof(GenericService1Int).IsAssignableTo(typeof(IGenericService1<>)));
            Assert.False(typeof(GenericService1Int).IsAssignableTo(typeof(IGenericService2<>)));
            Assert.True(typeof(GenericService1<int>).IsAssignableTo(typeof(IGenericService1<>)));
            Assert.True(typeof(GenericService2<int>).IsAssignableTo(typeof(IGenericService2<>)));
            Assert.False(typeof(GenericService1<int>).IsAssignableTo(typeof(IGenericService2<>)));
            Assert.False(typeof(GenericService2<int>).IsAssignableTo(typeof(IGenericService1<>)));

            Assert.True(typeof(GenericService1<>).IsAssignableTo(typeof(IGenericService1<>)));
        }

        [Test]
        public void SimpleInterfaceTransient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService1<>));

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.CanResolve(typeof(GenericService1<>)));
            Assert.That(serviceProvider.CanResolve(typeof(IGenericService1<>)));
            Assert.That(serviceProvider.CanResolve(typeof(GenericService1<int>)));
            Assert.That(serviceProvider.CanResolve(typeof(IGenericService1<int>)));
            Assert.That(serviceProvider.CanResolve(typeof(GenericService1<string>)));
            Assert.That(serviceProvider.CanResolve(typeof(IGenericService1<string>)));

            var s1 = serviceProvider.Get<IGenericService1<int>>();
            var s2 = serviceProvider.Get<IGenericService1<int>>();

            AssertX.AllDifferent<GenericService1<int>>(s1,s2);

            repo.UnRegister(typeof(GenericService1<>));
            
            Assert.Null(serviceProvider.Get<IGenericService1<int>>());
        }

        [Test]
        public void GenericClassWithMultipleInterfaces()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService1A<>));

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.CanResolve(typeof(GenericService1A<>)));
            Assert.That(serviceProvider.CanResolve(typeof(IGenericService1<>)));
            Assert.That(serviceProvider.CanResolve(typeof(GenericService1A<int>)));
            Assert.That(serviceProvider.CanResolve(typeof(IGenericService1<int>)));
            Assert.That(serviceProvider.CanResolve(typeof(GenericService1A<string>)));
            Assert.That(serviceProvider.CanResolve(typeof(IGenericService1<string>)));

            var s1 = serviceProvider.Get<IGenericService1<int>>();
            var s2 = serviceProvider.Get<IGenericService1<int>>();

            AssertX.AllDifferent<GenericService1A<int>>(s1, s2);

            repo.UnRegister(typeof(GenericService1A<>));

            Assert.Null(serviceProvider.Get<IGenericService1<int>>());
        }

        [Test]
        public void SimpleInterface_Factory()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(IGenericService1<>), t => Activator.CreateInstance(typeof(GenericService1<>).MakeGenericType(t.GenericTypeArguments[0])));

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IGenericService1<int>>();
            var s2 = serviceProvider.Get<IGenericService1<int>>();

            AssertX.AllDifferent<GenericService1<int>>(s1,s2);

            repo.UnRegister(typeof(IGenericService1<>));
            
            Assert.Null(serviceProvider.Get<IGenericService1<int>>());
        }

        [Test]
        public void GenericClassWithMultipleInterfaces_Factory()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(IGenericService1<>), t => Activator.CreateInstance(typeof(GenericService1A<>).MakeGenericType(t.GenericTypeArguments[0])));

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IGenericService1<int>>();
            var s2 = serviceProvider.Get<IGenericService1<int>>();

            AssertX.AllDifferent<GenericService1A<int>>(s1, s2);

            repo.UnRegister(typeof(IGenericService1<>));

            Assert.Null(serviceProvider.Get<IGenericService1<int>>());
        }


        [Test]
        public void SimpleInterface_Factory2()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(IGenericService1<>), (svcs,type) => svcs.Create(typeof(GenericService1<>).MakeGenericType(type.GenericTypeArguments[0])));

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IGenericService1<int>>();
            var s2 = serviceProvider.Get<IGenericService1<int>>();

            AssertX.AllDifferent<GenericService1<int>>(s1,s2);

            repo.UnRegister(typeof(IGenericService1<>));
            
            Assert.Null(serviceProvider.Get<IGenericService1<int>>());
        }

        [Test]
        public void SimpleInterface_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService1<>)).Singleton();

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IGenericService1<int>>();
            var s2 = serviceProvider.Get<IGenericService1<int>>();

            Assert.That(s1, Is.InstanceOf<GenericService1<int>>());
            Assert.That(s2, Is.InstanceOf<GenericService1<int>>());
            Assert.That(s1, Is.SameAs(s2));

            repo.UnRegister(typeof(GenericService1<>));
            
            Assert.Null(serviceProvider.Get<IGenericService1<int>>());
        }

        [Test]
        
        public void SimpleInterface_Singleton_MultiType()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService1<>)).Singleton();

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IGenericService1<int>>();
            var s2 = serviceProvider.Get<IGenericService1<string>>();
            var s1a = serviceProvider.Get<IGenericService1<int>>();
            var s2a = serviceProvider.Get<IGenericService1<string>>();

            AssertX.AllSame<GenericService1<int>>(s1, s1a);
            AssertX.AllSame<GenericService1<string>>(s2, s2a);

            repo.UnRegister(typeof(GenericService1<>));
            
            Assert.Null(serviceProvider.Get<IGenericService1<int>>());
        }

        [Test]
        public void SimpleInterface_WithDependency()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericServiceWithParam<>));

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IGenericService2<IGenericService1<int>>>();

            Assert.That(s1, Is.InstanceOf<GenericServiceWithParam<IGenericService1<int>>>());
            Assert.That(((GenericServiceWithParam<IGenericService1<int>>)s1).Svc1, Is.Null);

            repo.Register(typeof(GenericService1<>));

            s1 = serviceProvider.Get<IGenericService2<IGenericService1<int>>>();

            Assert.That(s1, Is.InstanceOf<GenericServiceWithParam<IGenericService1<int>>>());
            Assert.That(((GenericServiceWithParam<IGenericService1<int>>)s1).Svc1, Is.InstanceOf<GenericService1<int>>());
        }

        [Test]
        public void SimpleInterface_Factory_WithDependency()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericServiceWithParam<>), (svcs,type) => svcs.Create(typeof(GenericServiceWithParam<>).MakeGenericType(type.GenericTypeArguments[0])));

            var serviceProvider = repo.CreateServiceProvider();

            var s1 = serviceProvider.Get<IGenericService2<IGenericService1<int>>>();

            Assert.That(s1, Is.InstanceOf<GenericServiceWithParam<IGenericService1<int>>>());
            Assert.That(((GenericServiceWithParam<IGenericService1<int>>)s1).Svc1, Is.Null);

            repo.Register(typeof(GenericService1<>));

            s1 = serviceProvider.Get<IGenericService2<IGenericService1<int>>>();

            Assert.That(s1, Is.InstanceOf<GenericServiceWithParam<IGenericService1<int>>>());
            Assert.That(((GenericServiceWithParam<IGenericService1<int>>)s1).Svc1, Is.InstanceOf<GenericService1<int>>());
        }

        [Test]
        public void SimpleInstance()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc = new GenericService1<int>();

            repo.Register(svc);

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<GenericService1<int>>(), Is.SameAs(svc));
            Assert.That(serviceProvider.Get<GenericService1<bool>>(), Is.Null);

            repo.UnRegister<GenericService1<int>>();
            
            Assert.Null(serviceProvider.Get<GenericService1<int>>());
        }

        [Test]
        public void MultiInstance1()
        {
            GenericService12A<int> svc12a = new GenericService12A<int>();
            GenericService12B<int> svc12b = new GenericService12B<int>();

            ServiceRepository repo = new ServiceRepository();

            repo.Register<IGenericService1<int>>(svc12a);
            repo.Register<IGenericService2<int>>(svc12b);

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IGenericService1<int>>(), Is.SameAs(svc12a));
            Assert.That(serviceProvider.Get<IGenericService2<int>>(), Is.SameAs(svc12b));
        }


        [Test]
        public void MultiInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService12A<>)).As(typeof(IGenericService1<>));
            repo.Register(typeof(GenericService12B<>)).As(typeof(IGenericService2<>));

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IGenericService1<int>>(), Is.InstanceOf<GenericService12A<int>>());
            Assert.That(serviceProvider.Get<IGenericService2<bool>>(), Is.InstanceOf<GenericService12B<bool>>());
        }

        [Test]
        public void Create1()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc1 = new GenericService1<int>();

            repo.Register(svc1);

            var serviceProvider = repo.CreateServiceProvider();

            var svc3 = serviceProvider.Create<GenericServiceWithParam<IGenericService1<int>>>();

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3.Svc1, Is.SameAs(svc1));
        }

        [Test]
        public void Mixed()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<GenericService1Int>();
            repo.Register<GenericService1Bool>();

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IGenericService1<int>>(), Is.InstanceOf<GenericService1Int>());
            Assert.That(serviceProvider.Get<IGenericService1<bool>>(), Is.InstanceOf<GenericService1Bool>());
        }

        [Test]
        public void SomethingThatFailsOnUWP()
        {
            var classType1 = typeof(SimpleClass1);
            var classType2 = typeof(SimpleClass2);

            ServiceRepository repo = new ServiceRepository();

            var genericType1 = typeof(GenericService1<>).MakeGenericType(classType1);
            var genericType2 = typeof(GenericService1<>).MakeGenericType(classType2);

            var obj1 = Activator.CreateInstance(genericType1);
            var obj2 = Activator.CreateInstance(genericType2);

            repo.Register(obj1).As(genericType1);
            repo.Register(obj2).As(genericType2);

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get(genericType1), Is.SameAs(obj1));
            Assert.That(serviceProvider.Get(genericType2), Is.SameAs(obj2));

            repo.UnRegister(obj1);
            
            Assert.That(serviceProvider.Get(genericType1), Is.Null);
            Assert.That(serviceProvider.Get(genericType2), Is.SameAs(obj2));
            
            repo.UnRegister(obj2);
            
            Assert.That(serviceProvider.Get(genericType1), Is.Null);
            Assert.That(serviceProvider.Get(genericType2), Is.Null);
        }

    }
}