using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = new HostBuilder();
builder
    .UseContentRoot(Directory.GetCurrentDirectory())
    .ConfigureHostConfiguration(configurationBuilder => {
        configurationBuilder.AddCommandLine(args);
    }).ConfigureAppConfiguration((ctx,c) =>
{
    Console.WriteLine(c.ToJson());
    c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
}).ConfigureServices((h,s) =>
{
    
    s.AddHostedService<MyService>();
    s.Configure<MyService.MyOptions>(h.Configuration.GetSection("Hosted"));
}).ConfigureDefaults(args);
var built = builder.Build();

await built.StartAsync();
await built.WaitForShutdownAsync();

public static class JsonEx
{
    public static string ToJson<T>(this T obj) =>
        JsonSerializer.Serialize(obj);
}
internal class MyService : IHostedService
{
    private readonly ILogger<MyService> _logger;
    private readonly IOptionsMonitor<MyOptions> options;

    public MyService(ILogger<MyService> logger, IOptionsMonitor<MyOptions> options)
    {
        this._logger = logger;
        this.options = options;
    }
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started");
        _logger.LogInformation(options.CurrentValue.Name);
        options.OnChange(x=>_logger.LogWarning(x.Name));
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(5000);
            _logger.LogInformation("Still Alive");
        }
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping");
        await Task.Delay(5000);
        _logger.LogInformation("Stopped");
    }

    public class MyOptions
    {
        public string SectionName => "Hosted";
        public string Name { get; set; } = null!;
    }
}