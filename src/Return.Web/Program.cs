// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Program.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    public static class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args: args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args: args).
                ConfigureWebHostDefaults(configure: webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
