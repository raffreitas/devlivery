var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var devliveryDb = postgres.AddDatabase("devlivery-db", "devlivery");

builder.AddProject<Projects.Devlivery>("devlivery-webapi")
    .WaitFor(devliveryDb)
    .WithReference(devliveryDb, connectionName: "DefaultConnection");

await builder.Build().RunAsync();