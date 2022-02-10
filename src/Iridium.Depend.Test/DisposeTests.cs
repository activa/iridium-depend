using System;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class DisposeTests
    {
        public interface IService1
        {
        }

        public interface IService2
        {
        }

        public interface IService3
        {
            
        }

        public class Service1 : IService1, IDisposable
        {
            public void Dispose()
            {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(Service1));

                Disposed = true;
            }

            public bool Disposed { get; private set; }
        }

        public class Service2 : IService2, IDisposable
        {
            public void Dispose()
            {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(Service2));

                Disposed = true;
            }

            public bool Disposed { get; private set; }
        }

        public class Service3 : IService3, IDisposable
        {
            public void Dispose()
            {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(Service3));

                Disposed = true;
            }

            public bool Disposed { get; private set; }
        }

        [Test]
        public void Test_Singleton()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1>().Singleton();
            repo.Register<Service2>().Singleton().SkipDispose();
            repo.Register<Service3>().Singleton();

            var provider = repo.CreateServiceProvider();

            var svc1 = provider.Resolve<Service1>();
            var svc2 = provider.Resolve<Service2>();
            var svc3 = provider.Resolve<Service3>();

            Assert.That(svc1.Disposed, Is.False);
            Assert.That(svc2.Disposed, Is.False);
            Assert.That(svc3.Disposed, Is.False);

            repo.Dispose();

            Assert.That(svc1.Disposed, Is.True);
            Assert.That(svc2.Disposed, Is.False);
            Assert.That(svc3.Disposed, Is.True);

            Assert.Throws<ObjectDisposedException>(() => provider.Resolve<Service1>());
        }

        [Test]
        public void Test_Scoped()
        {
            var repo = new ServiceRepository();

            repo.Register<Service1>().Scoped();
            repo.Register<Service2>().Scoped().SkipDispose();
            repo.Register<Service3>().Scoped();

            var provider = repo.CreateServiceProvider();

            var scope1 = provider.CreateScope();
            var scope2 = provider.CreateScope();

            var svc11 = scope1.Resolve<Service1>();
            var svc12 = scope1.Resolve<Service2>();
            var svc13 = scope1.Resolve<Service3>();
            var svc21 = scope2.Resolve<Service1>();
            var svc22 = scope2.Resolve<Service2>();
            var svc23 = scope2.Resolve<Service3>();

            Assert.That(svc11.Disposed, Is.False);
            Assert.That(svc12.Disposed, Is.False);
            Assert.That(svc13.Disposed, Is.False);
            Assert.That(svc21.Disposed, Is.False);
            Assert.That(svc22.Disposed, Is.False);
            Assert.That(svc23.Disposed, Is.False);

            scope1.Dispose();

            Assert.That(svc11.Disposed, Is.True);
            Assert.That(svc12.Disposed, Is.False);
            Assert.That(svc13.Disposed, Is.True);
            Assert.That(svc21.Disposed, Is.False);
            Assert.That(svc22.Disposed, Is.False);
            Assert.That(svc23.Disposed, Is.False);

            scope2.Dispose();

            Assert.That(svc11.Disposed, Is.True);
            Assert.That(svc12.Disposed, Is.False);
            Assert.That(svc13.Disposed, Is.True);
            Assert.That(svc21.Disposed, Is.True);
            Assert.That(svc22.Disposed, Is.False);
            Assert.That(svc23.Disposed, Is.True);

            Assert.Throws<ObjectDisposedException>(() => scope1.Resolve<Service1>());
            Assert.Throws<ObjectDisposedException>(() => scope2.Resolve<Service1>());
        }

    }
}