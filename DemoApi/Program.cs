// See https://aka.ms/new-console-template for more information

using DemoApi;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

await startup.ConfigureAsync(app, args);
