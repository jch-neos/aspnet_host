using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = new WebHostBuilder();
builder
    .UseContentRoot(Directory.GetCurrentDirectory())
    .ConfigureAppConfiguration((ctx,c) =>
    {
        c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
    }).ConfigureServices((h,s) =>
    {
        if (System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Break();
    }).UseKestrel()
    .ConfigureLogging( l=> {
        l.AddConsole();
    })
    .UseStartup<MyStartup>();

var app = builder.Build();

await app.StartAsync();
await app.WaitForShutdownAsync();
internal class MyStartup : IStartup
{ 

    void IStartup.Configure(IApplicationBuilder _)
    {
    }

    IServiceProvider IStartup.ConfigureServices(IServiceCollection services) =>
        services.BuildServiceProvider();
}