using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class PropertyInjectionTest
    {
        private class Service1
        {
        }

        private class Service2
        {
        }

        private class Service3
        {
            public Service1 Svc1 { get; set; }
            [Inject]
            public Service2 Svc2 { get; set; }
        }

        [Test]
        
        public void Test1()
        {
            ServiceRepository repo = new ServiceRepository();

            var svc1 = new Service1();
            var svc2 = new Service2();

            repo.Register(svc1);
            repo.Register(svc2);

            var serviceProvider = repo.CreateServiceProvider();

            var svc3 = serviceProvider.Create<Service3>();

            Assert.That(svc3.Svc1, Is.Null);
            Assert.That(svc3.Svc2, Is.SameAs(svc2));
            
            svc3 = serviceProvider.Create<Service3>();

            Assert.That(svc3.Svc1, Is.Null);
            Assert.That(svc3.Svc2, Is.SameAs(svc2));
        }

    }
}