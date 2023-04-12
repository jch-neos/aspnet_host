using System.Buffers;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// var builder = WebHost.CreateDefaultBuilder<MyStartup>(args);
var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddSingleton(sp=>sp);
// builder.Services.AddScoped<Mediator>();
// builder.Services.AddScoped<GetBookCommandHandler>();
builder.Services.AddMediatR(c=>{
    c.RegisterServicesFromAssembly(typeof(GetBookCommand).Assembly);
});

var collection = builder.Services;

// collection.Scan(scan => scan
//     .FromAssemblyOf<ITransientService>()
//         .AddClasses(classes => classes.AssignableTo<ITransientService>())
//             .AsImplementedInterfaces()
//             .WithTransientLifetime()
//         .AddClasses(classes => classes.AssignableTo<IScopedService>())
//             .As<IScopedService>()
//             .WithScopedLifetime()
//         .AddClasses(classes => classes.AssignableTo(typeof(IOpenGeneric<>)))
//             .AsImplementedInterfaces()
//         .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
//             .AsImplementedInterfaces());

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

app.MapGet("/book/{id:int}", 
    ([FromServices] IMediator m, int id) => 
        m.Send(new GetBookCommand(id)));

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