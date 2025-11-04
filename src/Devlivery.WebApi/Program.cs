using Devlivery.WebApi;

// TODO:
// [x] - JWT Authentication
// [ ] - Refatorar Slices
// [ ] - Configura CI/CD
// [ ] - Integrar com Front

var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureBuilder(builder);
var app = builder.Build();
Startup.ConfigureApp(app);
await app.RunAsync();