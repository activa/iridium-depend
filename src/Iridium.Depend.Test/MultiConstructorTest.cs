using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Frameworks;
using NUnit.Framework;

namespace Iridium.Depend.Test
{
    [TestFixture]
    public class MultiConstructorTest
    {
        public enum MyEnum
        {
            Hello
        }

        private interface IService1
        {
        }

        private interface IService2
        {
        }

        public class Service1 : IService1
        {
        }

        public class Service2 : IService2
        {
        }

        private class ServiceA
        {
            public string Text { get; }
            public int X { get; }
            public int Y { get; }
            public MyEnum Hello { get; }
            public double? Number { get; }
            public IService1 Svc1 { get; }
            public IService2 Svc2 { get; }
            
            public string ConstructorCalled { get; }

            public ServiceA()
            {
                ConstructorCalled = "()";
            }

            public ServiceA(string text)
            {
                ConstructorCalled = "(string)";
                Text = text;
            }

            public ServiceA(int x)
            {
                ConstructorCalled = "(int)";
                X = x;
            }

            public ServiceA(string text, int x)
            {
                ConstructorCalled = "(string,int)";
                Text = text;
                X = x;
            }

            public ServiceA(int x, int y)
            {
                ConstructorCalled = "(int,int)";
                X = x;
                Y = y;
            }

            public ServiceA(string text, int x, int y)
            {
                ConstructorCalled = "(string,int,int)";
                Text = text;
                X = x;
                Y = y;
            }

            public ServiceA(MyEnum hello)
            {
                ConstructorCalled = "(MyEnum)";
                Hello = hello;
            }

            public ServiceA(string text, MyEnum hello)
            {
                ConstructorCalled = "(string,MyEnum)";
                Text = text;
                Hello = hello;
            }

            public ServiceA(int x, MyEnum hello)
            {
                ConstructorCalled = "(int,MyEnum)";
                X = x;
                Hello = hello;
            }

            public ServiceA(string text, int x, MyEnum hello)
            {
                ConstructorCalled = "(string,int,MyEnum)";
                Text = text;
                X = x;
                Hello = hello;
            }

            public ServiceA(int x, int y, MyEnum hello)
            {
                ConstructorCalled = "(int,int,MyEnum)";
                X = x;
                Y = y;
                Hello = hello;
            }

            public ServiceA(string text, int x, int y, MyEnum hello)
            {
                ConstructorCalled = "(string,int,int,MyEnum)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
            }

            public ServiceA(double? number)
            {
                ConstructorCalled = "(double?)";
                Number = number;
            }

            public ServiceA(string text, double? number)
            {
                ConstructorCalled = "(string,double?)";
                Text = text;
                Number = number;
            }

            public ServiceA(int x, double? number)
            {
                ConstructorCalled = "(int,double?)";
                X = x;
                Number = number;
            }

            public ServiceA(string text, int x, double? number)
            {
                ConstructorCalled = "(string,int,double?)";
                Text = text;
                X = x;
                Number = number;
            }

            public ServiceA(int x, int y, double? number)
            {
                ConstructorCalled = "(int,int,double?)";
                X = x;
                Y = y;
                Number = number;
            }

            public ServiceA(string text, int x, int y, double? number)
            {
                ConstructorCalled = "(string,int,int,double?)";
                Text = text;
                X = x;
                Y = y;
                Number = number;
            }

            public ServiceA(MyEnum hello, double? number)
            {
                ConstructorCalled = "(MyEnum,double?)";
                Hello = hello;
                Number = number;
            }

            public ServiceA(string text, MyEnum hello, double? number)
            {
                ConstructorCalled = "(string,MyEnum,double?)";
                Text = text;
                Hello = hello;
                Number = number;
            }

            public ServiceA(int x, MyEnum hello, double? number)
            {
                ConstructorCalled = "(int,MyEnum,double?)";
                X = x;
                Hello = hello;
                Number = number;
            }

            public ServiceA(string text, int x, MyEnum hello, double? number)
            {
                ConstructorCalled = "(string,int,MyEnum,double?)";
                Text = text;
                X = x;
                Hello = hello;
                Number = number;
            }

            public ServiceA(int x, int y, MyEnum hello, double? number)
            {
                ConstructorCalled = "(int,int,MyEnum,double?)";
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
            }

            public ServiceA(string text, int x, int y, MyEnum hello, double? number)
            {
                ConstructorCalled = "(string,int,int,MyEnum,double?)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
            }

            public ServiceA(IService1 service1)
            {
                ConstructorCalled = "(IService1)";
                Svc1 = service1;
            }

            public ServiceA(string text, IService1 service1)
            {
                ConstructorCalled = "(string,IService1)";
                Text = text;
                Svc1 = service1;
            }

            public ServiceA(int x, IService1 service1)
            {
                ConstructorCalled = "(int,IService1)";
                X = x;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, IService1 service1)
            {
                ConstructorCalled = "(string,int,IService1)";
                Text = text;
                X = x;
                Svc1 = service1;
            }

            public ServiceA(int x, int y, IService1 service1)
            {
                ConstructorCalled = "(int,int,IService1)";
                X = x;
                Y = y;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, int y, IService1 service1)
            {
                ConstructorCalled = "(string,int,int,IService1)";
                Text = text;
                X = x;
                Y = y;
                Svc1 = service1;
            }

            public ServiceA(MyEnum hello, IService1 service1)
            {
                ConstructorCalled = "(MyEnum,IService1)";
                Hello = hello;
                Svc1 = service1;
            }

            public ServiceA(string text, MyEnum hello, IService1 service1)
            {
                ConstructorCalled = "(string,MyEnum,IService1)";
                Text = text;
                Hello = hello;
                Svc1 = service1;
            }

            public ServiceA(int x, MyEnum hello, IService1 service1)
            {
                ConstructorCalled = "(int,MyEnum,IService1)";
                X = x;
                Hello = hello;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, MyEnum hello, IService1 service1)
            {
                ConstructorCalled = "(string,int,MyEnum,IService1)";
                Text = text;
                X = x;
                Hello = hello;
                Svc1 = service1;
            }

            public ServiceA(int x, int y, MyEnum hello, IService1 service1)
            {
                ConstructorCalled = "(int,int,MyEnum,IService1)";
                X = x;
                Y = y;
                Hello = hello;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, int y, MyEnum hello, IService1 service1)
            {
                ConstructorCalled = "(string,int,int,MyEnum,IService1)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
                Svc1 = service1;
            }

            public ServiceA(double? number, IService1 service1)
            {
                ConstructorCalled = "(double?,IService1)";
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(string text, double? number, IService1 service1)
            {
                ConstructorCalled = "(string,double?,IService1)";
                Text = text;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(int x, double? number, IService1 service1)
            {
                ConstructorCalled = "(int,double?,IService1)";
                X = x;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, double? number, IService1 service1)
            {
                ConstructorCalled = "(string,int,double?,IService1)";
                Text = text;
                X = x;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(int x, int y, double? number, IService1 service1)
            {
                ConstructorCalled = "(int,int,double?,IService1)";
                X = x;
                Y = y;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, int y, double? number, IService1 service1)
            {
                ConstructorCalled = "(string,int,int,double?,IService1)";
                Text = text;
                X = x;
                Y = y;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(MyEnum hello, double? number, IService1 service1)
            {
                ConstructorCalled = "(MyEnum,double?,IService1)";
                Hello = hello;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(string text, MyEnum hello, double? number, IService1 service1)
            {
                ConstructorCalled = "(string,MyEnum,double?,IService1)";
                Text = text;
                Hello = hello;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(int x, MyEnum hello, double? number, IService1 service1)
            {
                ConstructorCalled = "(int,MyEnum,double?,IService1)";
                X = x;
                Hello = hello;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, MyEnum hello, double? number, IService1 service1)
            {
                ConstructorCalled = "(string,int,MyEnum,double?,IService1)";
                Text = text;
                X = x;
                Hello = hello;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(int x, int y, MyEnum hello, double? number, IService1 service1)
            {
                ConstructorCalled = "(int,int,MyEnum,double?,IService1)";
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(string text, int x, int y, MyEnum hello, double? number, IService1 service1)
            {
                ConstructorCalled = "(string,int,int,MyEnum,double?,IService1)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
                Svc1 = service1;
            }

            public ServiceA(IService2 service2)
            {
                ConstructorCalled = "(IService2)";
                Svc2 = service2;
            }

            public ServiceA(string text, IService2 service2)
            {
                ConstructorCalled = "(string,IService2)";
                Text = text;
                Svc2 = service2;
            }

            public ServiceA(int x, IService2 service2)
            {
                ConstructorCalled = "(int,IService2)";
                X = x;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, IService2 service2)
            {
                ConstructorCalled = "(string,int,IService2)";
                Text = text;
                X = x;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, IService2 service2)
            {
                ConstructorCalled = "(int,int,IService2)";
                X = x;
                Y = y;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,IService2)";
                Text = text;
                X = x;
                Y = y;
                Svc2 = service2;
            }

            public ServiceA(MyEnum hello, IService2 service2)
            {
                ConstructorCalled = "(MyEnum,IService2)";
                Hello = hello;
                Svc2 = service2;
            }

            public ServiceA(string text, MyEnum hello, IService2 service2)
            {
                ConstructorCalled = "(string,MyEnum,IService2)";
                Text = text;
                Hello = hello;
                Svc2 = service2;
            }

            public ServiceA(int x, MyEnum hello, IService2 service2)
            {
                ConstructorCalled = "(int,MyEnum,IService2)";
                X = x;
                Hello = hello;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, MyEnum hello, IService2 service2)
            {
                ConstructorCalled = "(string,int,MyEnum,IService2)";
                Text = text;
                X = x;
                Hello = hello;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, MyEnum hello, IService2 service2)
            {
                ConstructorCalled = "(int,int,MyEnum,IService2)";
                X = x;
                Y = y;
                Hello = hello;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, MyEnum hello, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,MyEnum,IService2)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
                Svc2 = service2;
            }

            public ServiceA(double? number, IService2 service2)
            {
                ConstructorCalled = "(double?,IService2)";
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(string text, double? number, IService2 service2)
            {
                ConstructorCalled = "(string,double?,IService2)";
                Text = text;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(int x, double? number, IService2 service2)
            {
                ConstructorCalled = "(int,double?,IService2)";
                X = x;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, double? number, IService2 service2)
            {
                ConstructorCalled = "(string,int,double?,IService2)";
                Text = text;
                X = x;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, double? number, IService2 service2)
            {
                ConstructorCalled = "(int,int,double?,IService2)";
                X = x;
                Y = y;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, double? number, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,double?,IService2)";
                Text = text;
                X = x;
                Y = y;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(MyEnum hello, double? number, IService2 service2)
            {
                ConstructorCalled = "(MyEnum,double?,IService2)";
                Hello = hello;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(string text, MyEnum hello, double? number, IService2 service2)
            {
                ConstructorCalled = "(string,MyEnum,double?,IService2)";
                Text = text;
                Hello = hello;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(int x, MyEnum hello, double? number, IService2 service2)
            {
                ConstructorCalled = "(int,MyEnum,double?,IService2)";
                X = x;
                Hello = hello;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, MyEnum hello, double? number, IService2 service2)
            {
                ConstructorCalled = "(string,int,MyEnum,double?,IService2)";
                Text = text;
                X = x;
                Hello = hello;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, MyEnum hello, double? number, IService2 service2)
            {
                ConstructorCalled = "(int,int,MyEnum,double?,IService2)";
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, MyEnum hello, double? number, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,MyEnum,double?,IService2)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
                Svc2 = service2;
            }

            public ServiceA(IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(IService1,IService2)";
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,IService1,IService2)";
                Text = text;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,IService1,IService2)";
                X = x;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,IService1,IService2)";
                Text = text;
                X = x;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,int,IService1,IService2)";
                X = x;
                Y = y;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,IService1,IService2)";
                Text = text;
                X = x;
                Y = y;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(MyEnum hello, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(MyEnum,IService1,IService2)";
                Hello = hello;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, MyEnum hello, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,MyEnum,IService1,IService2)";
                Text = text;
                Hello = hello;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, MyEnum hello, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,MyEnum,IService1,IService2)";
                X = x;
                Hello = hello;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, MyEnum hello, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,MyEnum,IService1,IService2)";
                Text = text;
                X = x;
                Hello = hello;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, MyEnum hello, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,int,MyEnum,IService1,IService2)";
                X = x;
                Y = y;
                Hello = hello;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, MyEnum hello, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,MyEnum,IService1,IService2)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(double?,IService1,IService2)";
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,double?,IService1,IService2)";
                Text = text;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,double?,IService1,IService2)";
                X = x;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,double?,IService1,IService2)";
                Text = text;
                X = x;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,int,double?,IService1,IService2)";
                X = x;
                Y = y;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,double?,IService1,IService2)";
                Text = text;
                X = x;
                Y = y;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(MyEnum hello, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(MyEnum,double?,IService1,IService2)";
                Hello = hello;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, MyEnum hello, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,MyEnum,double?,IService1,IService2)";
                Text = text;
                Hello = hello;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, MyEnum hello, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,MyEnum,double?,IService1,IService2)";
                X = x;
                Hello = hello;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, MyEnum hello, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,MyEnum,double?,IService1,IService2)";
                Text = text;
                X = x;
                Hello = hello;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(int x, int y, MyEnum hello, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(int,int,MyEnum,double?,IService1,IService2)";
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }

            public ServiceA(string text, int x, int y, MyEnum hello, double? number, IService1 service1, IService2 service2)
            {
                ConstructorCalled = "(string,int,int,MyEnum,double?,IService1,IService2)";
                Text = text;
                X = x;
                Y = y;
                Hello = hello;
                Number = number;
                Svc1 = service1;
                Svc2 = service2;
            }
        }

            [Test]
        
        public void Test1()
        {
            var repo = new ServiceRepository();

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("()"));
            Assert.That(svc.Svc1, Is.Null);
            Assert.That(svc.Svc2, Is.Null);
            Assert.That(svc.Text, Is.Null);
        }

        [Test]
        
        public void Test2()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService1)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Null);
            Assert.That(svc.Text, Is.Null);
        }

        [Test]
        
        public void Test3()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service2());

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService2)"));
            Assert.That(svc.Svc1, Is.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Text, Is.Null);
        }

        [Test]
        
        public void Test4()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());
            repo.Register(new Service2());

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>();

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(IService1,IService2)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Text, Is.Null);
        }

        [Test]
        
        public void Test5()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>("X");

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(string,IService1)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Null);
            Assert.That(svc.Text, Is.EqualTo("X"));
        }

        [Test]
        public void Test6()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service2());

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>("X");

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(string,IService2)"));
            Assert.That(svc.Svc1, Is.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Text, Is.EqualTo("X"));
        }

        [Test]
        
        public void Test7()
        {
            var repo = new ServiceRepository();

            repo.Register(new Service1());
            repo.Register(new Service2());

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>("X");

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(string,IService1,IService2)"));
            Assert.That(svc.Svc1, Is.Not.Null);
            Assert.That(svc.Svc2, Is.Not.Null);
            Assert.That(svc.Text, Is.EqualTo("X"));
        }

        [Test]
        public void TestNamedParameters1()
        {
            var repo = new ServiceRepository();

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>(new {x = 5});

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(int)"));
            Assert.That(svc.X, Is.EqualTo(5));

            repo.Register(new Service1());
            repo.Register(new Service2());

            svc = serviceProvider.Create<ServiceA>(new { x = 5 });

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(int,IService1,IService2)"));
            Assert.That(svc.X, Is.EqualTo(5));

            svc = serviceProvider.Create<ServiceA>(new { x = 5, y = 6 });

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(int,int,IService1,IService2)"));
            Assert.That(svc.X, Is.EqualTo(5));
            Assert.That(svc.Y, Is.EqualTo(6));

            // Check if passing a named parameter that also resolves as a service will be used instead of the service

            repo = new ServiceRepository();
            repo.Register(new Service1());
            repo.Register(new Service2());

            var svc2 = new Service2();

            svc = serviceProvider.Create<ServiceA>(new { service2 = svc2, x = 5 });

            Assert.That(svc, Is.Not.Null);
            Assert.That(svc.ConstructorCalled, Is.EqualTo("(int,IService1,IService2)"));
            Assert.That(svc.Svc2, Is.SameAs(svc2));
        }

        [Test]
        
        [TestCase(new object[] { 1 }, "int")]
        [TestCase(new object[] { 1,"x" }, "string,int")]
        [TestCase(new object[] { 1, "x", 5.0 }, "string,int,double?")]
        [TestCase(new object[] { 1, "x", null }, "string,int,double?")]
        [TestCase(new object[] { 1, "x", MyEnum.Hello }, "string,int,MyEnum")]
        [TestCase(new object[] { 1, "x", MyEnum.Hello, null }, "string,int,MyEnum,double?")]
        public void TestParameterized(object[] parameters, string expectedConstructor)
        {
            var repo = new ServiceRepository();

            repo.Register<Service1>();
            repo.Register<Service2>();

            var serviceProvider = repo.CreateServiceProvider();

            var svc = serviceProvider.Create<ServiceA>(parameters);

            Assert.That(svc.ConstructorCalled, Is.EqualTo($"({expectedConstructor},IService1,IService2)"));

        }

    }
}