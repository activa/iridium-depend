using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
   
    public class MultiDerivedTests
    {
        public interface IService
        {
        }

        public class Model1
        {
        }

        public class Model2
        {
        }

        public class Model3
        {
        }

        public class Service1 : IService
        {

            public Service1()
            {
            }

            public Service1(Model1 model1)
            {
            }
        }

        public class Service2 : IService
        {
            public Service2(Model2 model2)
            {
            }
        }

        public class Service3 : IService
        {
            public Service3()
            {
            }

            public Service3(Model3 model)
            {
            }

            public Service3(Model3 model3, Service1 svc1)
            {
            }
        }

        public class Service4 : IService
        {
            public Model1 M1;
            public Model2 M2;

            public Service4(Model1 model1)
            {
                M1 = model1;
            }

            public Service4(Model2 model2)
            {
                M2 = model2;
            }

            public Service4(Model1 model1, Model2 model2)
            {
                M1 = model1;
                M2 = model2;
            }


        }

        [Test]
        public void TestMatchDerived()
        {
            ServiceRepository repo = new ServiceRepository();

            repo.Register<Service1>();
            repo.Register<Service2>();
            repo.Register<Service3>();

            var serviceProvider = repo.CreateServiceProvider();

            Assert.That(serviceProvider.Get<IService>(new Model1()), Is.InstanceOf<Service1>());
            Assert.That(serviceProvider.Get<IService>(new Model2()), Is.InstanceOf<Service2>());
            Assert.That(serviceProvider.Get<IService>(new Model3()), Is.InstanceOf<Service3>());

            Assert.That(serviceProvider.Get<IService>(new { model1 = new Model1() }), Is.InstanceOf<Service1>());
            Assert.That(serviceProvider.Get<IService>(new { model2 = new Model2() }), Is.InstanceOf<Service2>());
            Assert.That(serviceProvider.Get<IService>(new { model3 = new Model3() }), Is.InstanceOf<Service3>());

            Assert.That(serviceProvider.Get<IService>(new { model1 = (Model1)null }), Is.InstanceOf<Service1>());
            Assert.That(serviceProvider.Get<IService>(new { model2 = (Model2)null }), Is.InstanceOf<Service2>());
            Assert.That(serviceProvider.Get<IService>(new { model3 = (Model3)null }), Is.InstanceOf<Service3>());

            repo.Register<Service4>();

            Assert.That(serviceProvider.Get<IService>(new Model1(), new Model2()), Is.InstanceOf<Service4>());
            Assert.That(serviceProvider.Get<IService>(new Model2(), new Model1()), Is.InstanceOf<Service4>());
            Assert.That(serviceProvider.Get<IService>(new { model1 = new Model1(), model2 = new Model2() }), Is.InstanceOf<Service4>());
            Assert.That(serviceProvider.Get<IService>(new { model1 = (Model1)null, model2 = (Model2)null }), Is.InstanceOf<Service4>());

            Assert.That(((Service4)serviceProvider.Get<IService>(new Model1(), new Model2())).M2, Is.Not.Null);
            Assert.That(((Service4)serviceProvider.Get<IService>(new Model2(), new Model1())).M2, Is.Not.Null);
            Assert.That(((Service4)serviceProvider.Get<IService>(new { model1 = new Model1(), model2 = new Model2() })).M1, Is.Not.Null);
        }
    }
}