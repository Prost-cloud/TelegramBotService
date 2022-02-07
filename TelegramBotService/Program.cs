using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TelegramBotService.DBContext;

namespace TelegramBotService
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                { 
                    services.AddHostedService<Worker>();
                    services.AddTransient<IDbProvider, DbProvider>();
                });
    }
}
