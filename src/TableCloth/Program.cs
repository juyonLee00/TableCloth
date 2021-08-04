using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using TableCloth.Contracts;
using TableCloth.Implementations;
using TableCloth.Implementations.WinForms;

namespace TableCloth
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            using var serviceProvider = services.BuildServiceProvider();
            var startup = serviceProvider.GetService<IAppStartup>();
            var userInterface = serviceProvider.GetService<IAppUserInterface>();
            var messageBox = serviceProvider.GetService<IAppMessageBox>();

            startup.Arguments = args;

            if (!startup.HasRequirementsMet(out Exception failedReason, out bool isCritical))
            {
                messageBox.DisplayError(failedReason, isCritical);

                if (isCritical)
                {
                    Environment.Exit(1);
                    return;
                }
            }

            if (!startup.Initialize(out failedReason, out isCritical))
            {
                messageBox.DisplayError(failedReason, isCritical);

                if (isCritical)
                {
                    Environment.Exit(2);
                    return;
                }
            }

            userInterface.StartApplication(args);
        }

        public static void ConfigureServices(ServiceCollection services)
        {
            // Add Logging
            services.AddLogging(c =>
            {
                c.AddSerilog(dispose: true);
            });

            // Add Services
            services.AddTransient<IX509CertPairScanner, X509CertPairScanner>();
            services.AddTransient<ICatalogDeserializer, CatalogDeserializer>();
            services.AddTransient<ISandboxSpecSerializer, SandboxSpecSerializer>();
            services.AddTransient<ISandboxBuilder, SandboxBuilder>();
            services.AddTransient<IAppStartup, AppStartup>();

            // Windows Forms UI
            services.AddTransient<IAppMessageBox, WinFormMessageBox>();
            services.AddSingleton<IAppUserInterface, WinFormUserInterface>();
        }
    }
}
