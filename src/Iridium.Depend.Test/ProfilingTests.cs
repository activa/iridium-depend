using System;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class ProfilingTests
    {
        public interface ISvc1
        {
            void DoSomething();
        }

        public interface ISvc2
        {
            void DoSomething();
        }

        public interface ISvcA { }
        public interface ISvcB { }

        public class Svc1 : ISvc1
        {
            private readonly ISvcB _svc2;
            private readonly ISvcA _svc1;

            public Svc1(ISvcA svc1, ISvcB svc2)
            {
                _svc1 = svc1;
                _svc2 = svc2;
            }

            public void DoSomething()
            {
                if (_svc1 == null)
                    throw new Exception("svc1 not set");
                if (_svc2 == null)
                    throw new Exception("svc2 not set");
            }
        }

        public class SvcA : ISvcA
        {
        }

        public class SvcB : ISvcB
        {
        }

        public class Svc2 : ISvc2
        {
            [Inject] public ISvcA Svc_A { get; set; }
            [Inject] public ISvcB Svc_B { get; set; }

            public void DoSomething()
            {
                if (Svc_B == null || Svc_A == null)
                    throw new Exception();
            }
        }

        public class SvcDummyA { }
        public class SvcDummyB { }
        public class SvcDummyC { }
        public class SvcDummyD { }
        public class SvcDummyE { }
        public class SvcDummyF { }
        public class SvcDummyG { }
        public class SvcDummyH { }
        public class SvcDummyI { }
        public class SvcDummyJ { }
        public class SvcDummyK { }
        public class SvcDummyL { }
        public class SvcDummyM { }
        public class SvcDummyN { }
        public class SvcDummyO { }
        public class SvcDummyP { }
        public class SvcDummyQ { }
        public class SvcDummyR { }
        public class SvcDummyS { }
        public class SvcDummyT { }
        public class SvcDummyU { }
        public class SvcDummyV { }
        public class SvcDummyW { }
        public class SvcDummyX { }
        public class SvcDummyY { }
        public class SvcDummyZ { }

        private ServiceRepository _repo1;
        private ServiceRepository _repo2;
        private ServiceRepository _repo3;
        private ServiceRepository _repo1s;
        private ServiceRepository _repo2s;
        private ServiceRepository _repo3s;

        private IServiceProvider _provider1;
        private IServiceProvider _provider2;
        private IServiceProvider _provider3;
        private IServiceProvider _provider1s;
        private IServiceProvider _provider2s;
        private IServiceProvider _provider3s;

        private void RegisterDummies1(ServiceRepository container)
        {
            container.Register<SvcDummyA>();
            container.Register<SvcDummyB>();
            container.Register<SvcDummyC>();
            container.Register<SvcDummyD>();
            container.Register<SvcDummyE>();
            container.Register<SvcDummyF>();
            container.Register<SvcDummyG>();
            container.Register<SvcDummyH>();
            container.Register<SvcDummyI>();
            container.Register<SvcDummyJ>();
            container.Register<SvcDummyK>();
            container.Register<SvcDummyL>();
            container.Register<SvcDummyM>();
        }

        private void RegisterDummies2(ServiceRepository container)
        {
            container.Register<SvcDummyN>();
            container.Register<SvcDummyO>();
            container.Register<SvcDummyP>();
            container.Register<SvcDummyQ>();
            container.Register<SvcDummyR>();
            container.Register<SvcDummyS>();
            container.Register<SvcDummyT>();
            container.Register<SvcDummyU>();
            container.Register<SvcDummyV>();
            container.Register<SvcDummyW>();
            container.Register<SvcDummyX>();
            container.Register<SvcDummyY>();
            container.Register<SvcDummyZ>();
        }


        public ProfilingTests()
        {
            _repo1 = new ServiceRepository();
            _repo2 = new ServiceRepository();
            _repo3 = new ServiceRepository();

            _repo1s = new ServiceRepository();
            _repo2s = new ServiceRepository();
            _repo3s = new ServiceRepository();

            RegisterDummies1(_repo1);
            _repo1.Register<SvcA>().As<ISvcA>();
            _repo1.Register<SvcB>().As<ISvcB>();
            RegisterDummies2(_repo1);

            RegisterDummies1(_repo2);
            _repo2.Register<SvcA>().As<ISvcA>();
            _repo2.Register<SvcB>().As<ISvcB>();
            _repo2.Register<Svc1>().As<ISvc1>();
            RegisterDummies2(_repo2);

            RegisterDummies1(_repo3);
            _repo3.Register<SvcA>().As<ISvcA>();
            _repo3.Register<SvcB>().As<ISvcB>();
            _repo3.Register<Svc2>().As<ISvc2>();
            RegisterDummies2(_repo3);

            RegisterDummies1(_repo1s);
            _repo1s.Register(new SvcA()).As<ISvcA>();
            _repo1s.Register(new SvcB()).As<ISvcB>();
            RegisterDummies2(_repo1s);

            RegisterDummies1(_repo1s);
            _repo2s.Register(new SvcA()).As<ISvcA>();
            _repo2s.Register(new SvcB()).As<ISvcB>();
            _repo2s.Register<Svc1>().As<ISvc1>();
            RegisterDummies2(_repo1s);

            RegisterDummies1(_repo1s);
            _repo3s.Register(new SvcA()).As<ISvcA>();
            _repo3s.Register(new SvcB()).As<ISvcB>();
            _repo3s.Register<Svc2>().As<ISvc2>();
            RegisterDummies2(_repo1s);

            _provider1 = _repo1.CreateServiceProvider();
            _provider2 = _repo2.CreateServiceProvider();
            _provider3 = _repo3.CreateServiceProvider();
            _provider1s = _repo1s.CreateServiceProvider();
            _provider2s = _repo2s.CreateServiceProvider();
            _provider3s = _repo3s.CreateServiceProvider();
        }

        private const int _repeat = 100;

        [Test][Repeat(_repeat)]
        public void Create_Repo_Constr()
        {
            _provider1.Create<Svc1>().DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Create_Repo_Inject()
        {
            _provider1.Create<Svc2>().DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Create_Repo_Constr_Singleton()
        {
            _provider1s.Create<Svc1>().DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Create_Repo_Inject_Singleton()
        {
            _provider1s.Create<Svc2>().DoSomething();
        }

        // [Test][Repeat(_repeat)]
        // public void Create_Hybrid()
        // {
        //     new Svc2()
        //     {
        //         Svc_A = _provider1.Get<ISvcA>(),
        //         Svc_B = _provider1.Get<ISvcB>()
        //     }.DoSomething();
        // }
        //
        // [Test][Repeat(_repeat)]
        // public void Create_Hybrid_Singleton()
        // {
        //     new Svc2()
        //     {
        //         Svc_A = _provider1s.Get<ISvcA>(),
        //         Svc_B = _provider1s.Get<ISvcB>()
        //     }.DoSomething();
        // }

        [Test][Repeat(_repeat)]
        public void Create_Inject()
        {
            var svc2 = new Svc2();
            _provider1.UpdateDependencies(svc2);
            svc2.DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Create_Inject_Singleton()
        {
            var svc3 = new Svc2();
            _provider1s.UpdateDependencies(svc3);
            svc3.DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Resolve_Repo_Constr()
        {
            _provider2.Get<ISvc1>().DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Resolve_Repo_Inject()
        {
            _provider3.Get<ISvc2>().DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Resolve_Repo_Constr_Singleton()
        {
            _provider2s.Get<ISvc1>().DoSomething();
        }

        [Test][Repeat(_repeat)]
        public void Resolve_Repo_Inject_Singleton()
        {
            _provider3s.Get<ISvc2>().DoSomething();
        }
    }
}