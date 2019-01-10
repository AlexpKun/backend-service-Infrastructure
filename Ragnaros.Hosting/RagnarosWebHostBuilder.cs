using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace Ragnaros.Hosting
{
    public static class RagnarosWebHostBuilder
    {
        public static IWebHostBuilder CreateDefaultBuilder<TStartup>(string[] args) where TStartup : class
        {
            return
                WebHost
                .CreateDefaultBuilder(args)
                .UseStartup<TStartup>()
                .UseSerilog((ctx, cfg) =>
                {
                    cfg.ReadFrom.Configuration(ctx.Configuration)
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information);
                });
        }
    }
}
