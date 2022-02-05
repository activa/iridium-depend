using System.Collections.Generic;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class AfterActionTests
    {
        private interface IService1
        {
            List<string> Items { get; }
        }

        private interface IService2
        {
            List<string> Items { get; }
        }

        private class Service1A : IService1
        {
            public List<string> Items { get; } = new List<string>();
        }

        private class Service2A : IService2
        {
            public List<string> Items { get; } = new List<string>();
        }

        [Test]
        public void TestAfterCreateActions()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1A>()
                .As<IService1>()
                .Singleton()
                .OnCreate((o,svc) => o.Items.Add("X"))
                .OnCreate((o, svc) => o.Items.Add("Y"))
                .OnResolve((o, svc) => o.Items.Add("A"))
                .OnResolve((o, svc) => o.Items.Add("B"))
                .OnResolve((o, svc) => o.Items.Add("C"));

            var provider = repo.CreateServiceProvider();

            IService1 svc1;

            svc1 = provider.Get<IService1>();

            Assert.That(svc1.Items, Is.EquivalentTo(new[] {"X","Y","A","B","C"}));

            svc1 = provider.Get<IService1>();

            Assert.That(svc1.Items, Is.EquivalentTo(new[] { "X", "Y", "A", "B", "C", "A", "B", "C" }));
        }


    }
}