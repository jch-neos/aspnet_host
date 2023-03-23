using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

// var builder = WebHost.CreateDefaultBuilder<MyStartup>(args);
var builder = WebApplication.CreateBuilder(args);
// var builder = new WebHost();
// builder
//     .UseContentRoot(Directory.GetCurrentDirectory())
//     .ConfigureAppConfiguration((ctx,c) =>
//     {
//         c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//         .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
//     }).ConfigureServices((h,s) =>
//     {
//         if (System.Diagnostics.Debugger.IsAttached)
//             System.Diagnostics.Debugger.Break();
//     }).UseKestrel()
//     .ConfigureLogging( l=> {
//         l.AddConsole();
//     })
//     .UseStartup<MyStartup>();

var app = builder.Build();

app.UseRouting();

app.Use(async (context, next) =>
{
    if (context.Request.Method != "GET")
    {
        context.Response.StatusCode = 400;
        context.Response.BodyWriter.Write(Encoding.UTF8.GetBytes("Invalid Request"));
        return;
    }

    if (context.GetEndpoint()?.Metadata.GetMetadata<RequiresAuditAttribute>() is not null)
    {
        Console.WriteLine($"ACCESS TO SENSITIVE DATA AT: {DateTime.UtcNow}");
    }

    await next(context);
});

app.MapGet("/", () => "Audit isn't required.");
app.MapGet("/sensitive", () => "Audit required for sensitive data.")
    .WithMetadata(new RequiresAuditAttribute());


await app.StartAsync();
await app.WaitForShutdownAsync();
internal class MyStartup : IStartup
{

    void IStartup.Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();
    }

    IServiceProvider IStartup.ConfigureServices(IServiceCollection services) =>
            services.BuildServiceProvider();
}

internal class RequiresAuditAttribute : Attribute
{
    public RequiresAuditAttribute()
    {
    }
}
