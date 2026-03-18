using RentalApp.Infrastructure;
using RentalApp.Infrastructure.Security.Secrets;
using RentalApp.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.DecryptMarkedValuesFromEnvironment();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
