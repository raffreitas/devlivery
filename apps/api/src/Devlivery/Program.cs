using Devlivery;

var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureBuilder(builder);

var app = builder.Build();
Startup.ConfigureApp(app);

await app.RunAsync();

namespace Devlivery
{
    public partial class Program
    {
        protected Program()
        {
        }
    };
}