using Devlivery.WebApi;

// TODO:
// [ ] - Refatorar slices
// [x] - ADD ASP.NET Identity
// [ ] - JWT Authentication
// [ ] - Configura CI/CD
// [ ] - Integrar com Front

var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureBuilder(builder);
var app = builder.Build();
Startup.ConfigureApp(app);
await app.RunAsync();