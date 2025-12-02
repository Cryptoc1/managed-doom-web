using ManagedDoom.App.UI;
using ManagedDoom.App.UI.Abstractions;
using ManagedDoom.App.UI.Infrastructure;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault( args );
builder.Logging.SetMinimumLevel( builder.HostEnvironment.IsDevelopment()
    ? LogLevel.Information
    : LogLevel.Error );

builder.Services.AddManagedDoomUI();

builder.Services.AddTransient<IBootloader, ConfigBootloader>();
builder.Services.AddHttpClient<IBootloader, WadBootloader>(
        http => http.BaseAddress = new( builder.HostEnvironment.BaseAddress ) )
        .AddStandardResilienceHandler();

await using var app = builder.Build();
await app.RunAsync();