using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Iridium.Depend.Test.BasicResolveTests;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class MultiServiceTest
    {
        public interface IService
        {
            string Id { get; }
        }

        public class Obj1
        {

        }

        public class Obj2
        {

        }

        public class Service1 : IService
        {
            public string Id { get; }
            public Service1(string s) => Id = "string";
        }

        public class Service2 : IService
        {
            public string Id { get; }
            public Service2(int i) => Id = "int";
        }

        public class Service3 : IService
        {
            public string Id { get; }
            public Service3(Obj1 o) => Id = "obj1";
        }

        public class Service4 : IService
        {
            public string Id { get; }
            public Service4(Obj2 o) => Id = "obj2";
        }

        [Test]
        public void Test_Multiple_Services_With_Different_Parameters()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1>();
            repo.Register<Service2>();
            repo.Register<Service3>();
            repo.Register<Service4>();

            var provider = repo.CreateServiceProvider();

            var svc = provider.Get<IService>("");

            Assert.That(svc.Id, Is.EqualTo("string"));

            svc = provider.Get<IService>(1);

            Assert.That(svc.Id, Is.EqualTo("int"));

            svc = provider.Get<IService>(new Obj1());

            Assert.That(svc.Id, Is.EqualTo("obj1"));

            svc = provider.Get<IService>(new Obj2());

            Assert.That(svc.Id, Is.EqualTo("obj2"));


            svc = provider.Get<IService>(new {s =""});

            Assert.That(svc.Id, Is.EqualTo("string"));

            svc = provider.Get<IService>(new {i=1});

            Assert.That(svc.Id, Is.EqualTo("int"));

            svc = provider.Get<IService>(new {o = new Obj1()});

            Assert.That(svc.Id, Is.EqualTo("obj1"));

            svc = provider.Get<IService>(new { o = new Obj2() });

            Assert.That(svc.Id, Is.EqualTo("obj2"));

        }

    }
}