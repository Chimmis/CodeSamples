using System;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a DI container
            var container = new ServiceCollection();

            // Register the services with the container 
            container.AddSingleton<App>()
                .AddSingleton<DoSomethingService>()
                .AddSingleton<IDoSomethingElseService, DoSomethingElseService>(); // I can also register a service as an interface, this is something a lot of programers over-use. Having an interface here gives me no benefit

            // I get the initialized service from the container and run it
            var service = container.BuildServiceProvider().GetRequiredService<App>();

            service.Run();
            Console.ReadKey();
            // Now what happens here is I NEVER initialize DoSomethingService, the DI container takes care of that for me. 
            // You can make thousands of services like this, and you will never have to create them by yourself.
        }
    }

    // Main service
    public class App
    {
        
        private readonly DoSomethingService doSomethingService;
        private readonly IDoSomethingElseService doSomethingElseService;

        // This is called 'Constructor Injection' and it's the most common way of injecting services, you will see and use this A LOT'
        public App(DoSomethingService doSomethingService, IDoSomethingElseService doSomethingElseService)
        {
            this.doSomethingService = doSomethingService;
            this.doSomethingElseService = doSomethingElseService;
        }

        public void Run()
        {
            doSomethingService.DoSomething();
            doSomethingElseService.DoSomethingElse();

            // Uncomment this and see what happens
            // doSomethingElseService.InvokeTheOtherService();
        }
    }

    // Just some service

    public class DoSomethingService
    {
        public void DoSomething()
        {
            Console.WriteLine("I am a crazy service and I am doing something");
        }
    }

    public interface IDoSomethingElseService
    {
        void DoSomethingElse();
        void InvokeTheOtherService();
    }

    public class DoSomethingElseService : IDoSomethingElseService
    {
        private readonly DoSomethingService doSomethingService;

        // I can inject any service that is registered with dependency injection
        public DoSomethingElseService(DoSomethingService doSomethingService)
        {
            this.doSomethingService = doSomethingService;
        }

        public void DoSomethingElse()
        {
            Console.WriteLine("My programming dick is large and I am quite a vile service");
        }

        public void InvokeTheOtherService()
        {
            // I'm never calling this method in any other service, it's just a demo that you can do this and compose services however the fuck you want'
            this.doSomethingService.DoSomething();
        }
    }
}
