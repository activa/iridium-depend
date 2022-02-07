using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class BasicTests
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
        }

        private class Service2B : IService2
        {
        }

        private class ServiceX
        {
            public IService1 Svc1;

            public ServiceX()
            {
            }

            public ServiceX(IService1 svc1)
            {
                Assert.NotNull(svc1);

                Svc1 = svc1;
            }
        }

        private class ServiceY
        {
            public IService2 Svc2;

            public ServiceY(IService2 svc2)
            {
                Assert.NotNull(svc2);

                Svc2 = svc2;
            }
        }

        private class ServiceXY
        {
            public IService1 Svc1;
            public IService2 Svc2;

            public ServiceXY(IService1 svc1)
            {
                Assert.NotNull(svc1);

                Svc1 = svc1;
            }

            public ServiceXY(IService2 svc2)
            {
                Assert.NotNull(svc2);

                Svc2 = svc2;
            }

            public ServiceXY(IService1 svc1, IService2 svc2)
            {
                Assert.NotNull(svc1);
                Assert.NotNull(svc2);

                Svc1 = svc1;
                Svc2 = svc2;
            }
        }

        public class Service12A : IService1, IService2
        {
        }

        public class Service12B : IService1, IService2
        {
        }

        [Test]
        public void RegType_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllDifferent<Service12A>(i1a, i1b, i2a, i2b, ca, cb);
        }

        [Test]
        public void RegTypeByInterface_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService2>();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllNull(i1a,i1b,ca,cb);
            AssertX.AllDifferent<Service12A>(i2a,i2b);
        }

        [Test]
        public void RegTypeBySelf_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().AsSelf();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();
            
            AssertX.AllNull(i1a,i1b,i2a,i2b);
            AssertX.AllDifferent<Service12A>(ca, cb);
        }


        [Test]
        public void RegTypeByInterfaceAndSelf_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService1>().AsSelf();

            var provider = repo.CreateServiceProvider();

            var s1a = provider.Get<IService1>();
            var s1b = provider.Get<IService1>();
            var s2a = provider.Get<IService2>();
            var s2b = provider.Get<IService2>();
            var sa = provider.Get<Service12A>();
            var sb = provider.Get<Service12A>();

            AssertX.AllNull(s2a,s2b);
            AssertX.AllDifferent<Service12A>(s1a, s1b, sa, sb);
        }

        [Test]
        public void RegType_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().Singleton();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllSame<Service12A>(i1a, i1b, i2a, i2b, ca, cb);
        }

        [Test]
        public void RegTypeByInterface_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService2>().Singleton();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllSame<Service12A>(i2a,i2b);
            AssertX.AllNull(i1a,i1b,ca,cb);
        }

        [Test]
        public void RegTypeBySelf_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().AsSelf().Singleton();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllNull(i1a,i1b,i2a,i2b);
            AssertX.AllSame<Service12A>(ca,cb);
        }


        [Test]
        public void RegTypeByInterfaceAndSelf_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService1>().AsSelf().Singleton();

            var provider = repo.CreateServiceProvider();

            var s1a = provider.Get<IService1>();
            var s1b = provider.Get<IService1>();
            var s2a = provider.Get<IService2>();
            var s2b = provider.Get<IService2>();
            var sa = provider.Get<Service12A>();
            var sb = provider.Get<Service12A>();

            AssertX.AllNull(s2a,s2b);
            AssertX.AllSame<Service12A>(s1a,s1b,sa,sb);
        }

        [Test]
        public void RegType_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().Scoped();

            var scope1 = repo.CreateServiceProvider();
            var scope2 = scope1.CreateScope();

            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12A>();
            var cb = scope1.Get<Service12A>();
            var cc = scope2.Get<Service12A>();
            var cd = scope2.Get<Service12A>();

            AssertX.AllSame<Service12A>(i1a, i1b, i2a, i2b, ca, cb);
            AssertX.AllSame<Service12A>(i1c, i1d, i2c, i2d, cc, cd);

            Assert.That(i1a, Is.Not.SameAs(i1c));
        }

        [Test]
        public void RegTypeByInterface_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService2>().Scoped();

            var scope1 = repo.CreateServiceProvider();
            var scope2 = scope1.CreateScope();

            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12A>();
            var cb = scope1.Get<Service12A>();
            var cc = scope2.Get<Service12A>();
            var cd = scope2.Get<Service12A>();

            AssertX.AllSame<Service12A>(i2a, i2b);
            AssertX.AllSame<Service12A>(i2c, i2d);
            AssertX.AllNull(i1a,i1b,i1c,i1d,ca,cb,cc,cd);

            Assert.That(i2a, Is.Not.SameAs(i2c));
        }

        [Test]
        public void RegTypeBySelf_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().AsSelf().Scoped();

            var scope1 = repo.CreateServiceProvider();
            var scope2 = scope1.CreateScope();

            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12A>();
            var cb = scope1.Get<Service12A>();
            var cc = scope2.Get<Service12A>();
            var cd = scope2.Get<Service12A>();

            AssertX.AllSame<Service12A>(ca, cb);
            AssertX.AllSame<Service12A>(cc, cd);
            AssertX.AllNull(i1a, i1b, i1c, i1d, i2a, i2b, i2c, i2d);

            Assert.That(ca, Is.Not.SameAs(cc));
        }


        [Test]
        public void RegTypeByInterfaceAndSelf_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12A>().As<IService1>().AsSelf().Scoped();

            var scope1 = repo.CreateServiceProvider();
            var scope2 = scope1.CreateScope();

            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12A>();
            var cb = scope1.Get<Service12A>();
            var cc = scope2.Get<Service12A>();
            var cd = scope2.Get<Service12A>();

            AssertX.AllSame<Service12A>(i1a, i1b, ca, cb);
            AssertX.AllSame<Service12A>(i1c, i1d, cc, cd);
            AssertX.AllNull(i2a,i2b,i2c,i2d);

            Assert.That(i1a, Is.Not.SameAs(i1c));
        }

        [Test]
        public void RegInstance()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12A());

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllSame<Service12A>(i1a, i1b, i2a, i2b, ca, cb);
        }

        [Test]
        public void RegInstanceByInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12A()).As<IService2>();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllSame<Service12A>(i2a, i2b);
            AssertX.AllNull(i1a, i1b, ca, cb);
        }

        [Test]
        public void RegInstanceBySelf()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12A()).AsSelf();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12A>();
            var cb = provider.Get<Service12A>();

            AssertX.AllNull(i1a, i1b, i2a, i2b);
            AssertX.AllSame<Service12A>(ca, cb);
        }


        [Test]
        public void RegInstanceByInterfaceAndSelf()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12A()).As<IService1>().AsSelf();

            var provider = repo.CreateServiceProvider();

            var s1a = provider.Get<IService1>();
            var s1b = provider.Get<IService1>();
            var s2a = provider.Get<IService2>();
            var s2b = provider.Get<IService2>();
            var sa = provider.Get<Service12A>();
            var sb = provider.Get<Service12A>();

            AssertX.AllNull(s2a, s2b);
            AssertX.AllSame<Service12A>(s1a, s1b, sa, sb);
        }
    }
}