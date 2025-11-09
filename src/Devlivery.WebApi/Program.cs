using Devlivery.WebApi;

var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureBuilder(builder);
var app = builder.Build();
Startup.ConfigureApp(app);
await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
};