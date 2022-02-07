using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class BasicDependencyTests
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

    }
}