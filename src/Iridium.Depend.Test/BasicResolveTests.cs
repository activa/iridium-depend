using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class BasicResolveTests
    {
        private interface IService1
        {
        }

        private interface IService2
        { 
        }

        public class Service12 : IService1, IService2
        {
        }

        [Test]
        public void RegType_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllDifferent<Service12>(i1a, i1b, i2a, i2b, ca, cb);
        }

        [Test]
        public void RegTypeByInterface_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().As<IService2>();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllNull(i1a,i1b,ca,cb);
            AssertX.AllDifferent<Service12>(i2a,i2b);
        }

        [Test]
        public void RegTypeBySelf_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().AsSelf();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();
            
            AssertX.AllNull(i1a,i1b,i2a,i2b);
            AssertX.AllDifferent<Service12>(ca, cb);
        }


        [Test]
        public void RegTypeByInterfaceAndSelf_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().As<IService1>().AsSelf();

            var provider = repo.CreateServiceProvider();

            var s1a = provider.Get<IService1>();
            var s1b = provider.Get<IService1>();
            var s2a = provider.Get<IService2>();
            var s2b = provider.Get<IService2>();
            var sa = provider.Get<Service12>();
            var sb = provider.Get<Service12>();

            AssertX.AllNull(s2a,s2b);
            AssertX.AllDifferent<Service12>(s1a, s1b, sa, sb);
        }

        [Test]
        public void RegType_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().Singleton();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllSame<Service12>(i1a, i1b, i2a, i2b, ca, cb);
        }

        [Test]
        public void RegTypeByInterface_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().As<IService2>().Singleton();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllSame<Service12>(i2a,i2b);
            AssertX.AllNull(i1a,i1b,ca,cb);
        }

        [Test]
        public void RegTypeBySelf_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().AsSelf().Singleton();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllNull(i1a,i1b,i2a,i2b);
            AssertX.AllSame<Service12>(ca,cb);
        }


        [Test]
        public void RegTypeByInterfaceAndSelf_Singleton()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().As<IService1>().AsSelf().Singleton();

            var provider = repo.CreateServiceProvider();

            var s1a = provider.Get<IService1>();
            var s1b = provider.Get<IService1>();
            var s2a = provider.Get<IService2>();
            var s2b = provider.Get<IService2>();
            var sa = provider.Get<Service12>();
            var sb = provider.Get<Service12>();

            AssertX.AllNull(s2a,s2b);
            AssertX.AllSame<Service12>(s1a,s1b,sa,sb);
        }

        [Test]
        public void RegType_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().Scoped();

            var provider = repo.CreateServiceProvider();

            var scope1 = provider.CreateScope();
            var scope2 = provider.CreateScope();

            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12>();
            var cb = scope1.Get<Service12>();
            var cc = scope2.Get<Service12>();
            var cd = scope2.Get<Service12>();

            AssertX.AllSame<Service12>(i1a, i1b, i2a, i2b, ca, cb);
            AssertX.AllSame<Service12>(i1c, i1d, i2c, i2d, cc, cd);

            Assert.That(i1a, Is.Not.SameAs(i1c));
        }

        [Test]
        public void RegTypeByInterface_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().As<IService2>().Scoped();

            var provider = repo.CreateServiceProvider();

            var scope1 = provider.CreateScope();
            var scope2 = provider.CreateScope();

            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12>();
            var cb = scope1.Get<Service12>();
            var cc = scope2.Get<Service12>();
            var cd = scope2.Get<Service12>();

            AssertX.AllSame<Service12>(i2a, i2b);
            AssertX.AllSame<Service12>(i2c, i2d);
            AssertX.AllNull(i1a,i1b,i1c,i1d,ca,cb,cc,cd);

            Assert.That(i2a, Is.Not.SameAs(i2c));
        }

        [Test]
        public void RegTypeBySelf_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().AsSelf().Scoped();

            var provider = repo.CreateServiceProvider();

            var scope1 = provider.CreateScope();
            var scope2 = provider.CreateScope();
            
            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12>();
            var cb = scope1.Get<Service12>();
            var cc = scope2.Get<Service12>();
            var cd = scope2.Get<Service12>();

            AssertX.AllSame<Service12>(ca, cb);
            AssertX.AllSame<Service12>(cc, cd);
            AssertX.AllNull(i1a, i1b, i1c, i1d, i2a, i2b, i2c, i2d);

            Assert.That(ca, Is.Not.SameAs(cc));
        }


        [Test]
        public void RegTypeByInterfaceAndSelf_Scoped()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service12>().As<IService1>().AsSelf().Scoped();

            var provider = repo.CreateServiceProvider();

            var scope1 = provider.CreateScope();
            var scope2 = provider.CreateScope();

            var i1a = scope1.Get<IService1>();
            var i1b = scope1.Get<IService1>();
            var i1c = scope2.Get<IService1>();
            var i1d = scope2.Get<IService1>();
            var i2a = scope1.Get<IService2>();
            var i2b = scope1.Get<IService2>();
            var i2c = scope2.Get<IService2>();
            var i2d = scope2.Get<IService2>();
            var ca = scope1.Get<Service12>();
            var cb = scope1.Get<Service12>();
            var cc = scope2.Get<Service12>();
            var cd = scope2.Get<Service12>();

            AssertX.AllSame<Service12>(i1a, i1b, ca, cb);
            AssertX.AllSame<Service12>(i1c, i1d, cc, cd);
            AssertX.AllNull(i2a,i2b,i2c,i2d);

            Assert.That(i1a, Is.Not.SameAs(i1c));
        }

        [Test]
        public void RegInstance()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12());

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllSame<Service12>(i1a, i1b, i2a, i2b, ca, cb);
        }

        [Test]
        public void RegInstanceByInterface()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12()).As<IService2>();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllSame<Service12>(i2a, i2b);
            AssertX.AllNull(i1a, i1b, ca, cb);
        }

        [Test]
        public void RegInstanceBySelf()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12()).AsSelf();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllNull(i1a, i1b, i2a, i2b);
            AssertX.AllSame<Service12>(ca, cb);
        }


        [Test]
        public void RegInstanceByInterfaceAndSelf()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(new Service12()).As<IService1>().AsSelf();

            var provider = repo.CreateServiceProvider();

            var s1a = provider.Get<IService1>();
            var s1b = provider.Get<IService1>();
            var s2a = provider.Get<IService2>();
            var s2b = provider.Get<IService2>();
            var sa = provider.Get<Service12>();
            var sb = provider.Get<Service12>();

            AssertX.AllNull(s2a, s2b);
            AssertX.AllSame<Service12>(s1a, s1b, sa, sb);
        }

        [Test]
        public void RegFactory_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc => new Service12());

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllDifferent<Service12>(i1a, i1b, i2a, i2b, ca, cb);
        }

        [Test]
        public void RegFactoryByInterface_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc => new Service12()).As<IService2>();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllNull(i1a, i1b, ca, cb);
            AssertX.AllDifferent<Service12>(i2a, i2b);
        }

        [Test]
        public void RegFactoryBySelf_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc => new Service12()).AsSelf();

            var provider = repo.CreateServiceProvider();

            var i1a = provider.Get<IService1>();
            var i1b = provider.Get<IService1>();
            var i2a = provider.Get<IService2>();
            var i2b = provider.Get<IService2>();
            var ca = provider.Get<Service12>();
            var cb = provider.Get<Service12>();

            AssertX.AllNull(i1a, i1b, i2a, i2b);
            AssertX.AllDifferent<Service12>(ca, cb);
        }


        [Test]
        public void RegFactoryByInterfaceAndSelf_Transient()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register(svc => new Service12()).As<IService1>().AsSelf();

            var provider = repo.CreateServiceProvider();

            var s1a = provider.Get<IService1>();
            var s1b = provider.Get<IService1>();
            var s2a = provider.Get<IService2>();
            var s2b = provider.Get<IService2>();
            var sa = provider.Get<Service12>();
            var sb = provider.Get<Service12>();

            AssertX.AllNull(s2a, s2b);
            AssertX.AllDifferent<Service12>(s1a, s1b, sa, sb);
        }

    }
}