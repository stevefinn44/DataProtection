﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("settings.json");
            var config = builder.Build();

#if NET461
            var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates.Find(X509FindType.FindByThumbprint, config["CertificateThumbprint"], false);
#endif

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("."))
#if NET461
                .ProtectKeysWithAzureKeyVault(config["KeyId"], config["ClientId"], cert.OfType<X509Certificate2>().Single());
#else
                .ProtectKeysWithAzureKeyVault(config["KeyId"], config["ClientId"], config["ClientSecret"]);
#endif

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddConsole();

            var protector = serviceProvider.GetDataProtector("Test");

            Console.WriteLine(protector.Protect("Hello world"));
        }
    }
}