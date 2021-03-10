using System;
using System.Collections.Generic;
using System.Linq;
using Iridium.Reflection;
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

        [Test]
        public void AssignableTests()
        {
            Assert.True(typeof(GenericService1Int).Inspector().IsAssignableTo(typeof(IGenericService1<>)));
            Assert.False(typeof(GenericService1Int).Inspector().IsAssignableTo(typeof(IGenericService2<>)));
            Assert.True(typeof(GenericService1<int>).Inspector().IsAssignableTo(typeof(IGenericService1<>)));
            Assert.True(typeof(GenericService2<int>).Inspector().IsAssignableTo(typeof(IGenericService2<>)));
            Assert.False(typeof(GenericService1<int>).Inspector().IsAssignableTo(typeof(IGenericService2<>)));
            Assert.False(typeof(GenericService2<int>).Inspector().IsAssignableTo(typeof(IGenericService1<>)));

            Assert.True(typeof(GenericService1<>).Inspector().IsAssignableTo(typeof(IGenericService1<>)));
        }



        [Test]
        public void SimpleInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService1<>));

            var s1 = repo.Get<IGenericService1<int>>();
            var s2 = repo.Get<IGenericService1<int>>();

            Assert.That(s1, Is.InstanceOf<GenericService1<int>>());
            Assert.That(s2, Is.InstanceOf<GenericService1<int>>());
            Assert.That(s1, Is.Not.SameAs(s2));

            repo.UnRegister(typeof(GenericService1<>));

            Assert.Null(repo.Get<IGenericService1<int>>());
        }

        [Test]
        public void SimpleInterface_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService1<>)).Singleton();

            var s1 = repo.Get<IGenericService1<int>>();
            var s2 = repo.Get<IGenericService1<int>>();

            Assert.That(s1, Is.InstanceOf<GenericService1<int>>());
            Assert.That(s2, Is.InstanceOf<GenericService1<int>>());
            Assert.That(s1, Is.SameAs(s2));

            repo.UnRegister(typeof(GenericService1<>));

            Assert.Null(repo.Get<IGenericService1<int>>());
        }

        [Test]
        public void SimpleInterface_WithDependency()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericServiceWithParam<>));

            Assert.IsNull(repo.Get<IGenericService2<int>>());
            Assert.IsNull(repo.Get<IGenericService2<IGenericService1<int>>>());

            repo.Register(typeof(GenericService1<>));

            var s1 = repo.Get<IGenericService2<IGenericService1<int>>>();

            Assert.That(s1, Is.InstanceOf<GenericServiceWithParam<IGenericService1<int>>>());
            Assert.That(((GenericServiceWithParam<IGenericService1<int>>)s1).Svc1, Is.InstanceOf<GenericService1<int>>());
        }

        [Test]
        public void SimpleInstance()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc = new GenericService1<int>();

            repo.Register(svc);

            Assert.That(repo.Get<GenericService1<int>>(), Is.SameAs(svc));
            Assert.That(repo.Get<GenericService1<bool>>(), Is.Null);

            repo.UnRegister<GenericService1<int>>();

            Assert.Null(repo.Get<GenericService1<int>>());
        }

        [Test]
        public void MultiInstance1()
        {
            GenericService12A<int> svc12a = new GenericService12A<int>();
            GenericService12B<int> svc12b = new GenericService12B<int>();

            ServiceRepository repo = new ServiceRepository();

            repo.Register<IGenericService1<int>>(svc12a);
            repo.Register<IGenericService2<int>>(svc12b);

            Assert.That(repo.Get<IGenericService1<int>>(), Is.SameAs(svc12a));
            Assert.That(repo.Get<IGenericService2<int>>(), Is.SameAs(svc12b));
        }


        [Test]
        public void MultiInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(typeof(GenericService12A<>)).As(typeof(IGenericService1<>));
            repo.Register(typeof(GenericService12B<>)).As(typeof(IGenericService2<>));

            Assert.That(repo.Get<IGenericService1<int>>(), Is.InstanceOf<GenericService12A<int>>());
            Assert.That(repo.Get<IGenericService2<bool>>(), Is.InstanceOf<GenericService12A<bool>>());
        }


        [Test]
        public void Create1()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc1 = new GenericService1<int>();

            repo.Register(svc1);

            var svc3 = repo.Create<GenericServiceWithParam<IGenericService1<int>>>();

            Assert.That(svc3, Is.Not.Null);
            Assert.That(svc3.Svc1, Is.SameAs(svc1));
        }

        [Test]
        public void Mixed()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<GenericService1Int>();
            repo.Register<GenericService1Bool>();

            Assert.That(repo.Get<IGenericService1<int>>(), Is.InstanceOf<GenericService1Int>());
            Assert.That(repo.Get<IGenericService1<bool>>(), Is.InstanceOf<GenericService1Bool>());
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

            Assert.That(repo.Get(genericType1), Is.SameAs(obj1));
            Assert.That(repo.Get(genericType2), Is.SameAs(obj2));

            repo.UnRegister(genericType1);

            Assert.That(repo.Get(genericType1), Is.Null);
            Assert.That(repo.Get(genericType2), Is.SameAs(obj2));

            repo.UnRegister(genericType2);

            Assert.That(repo.Get(genericType1), Is.Null);
            Assert.That(repo.Get(genericType2), Is.Null);
        }

    }
}