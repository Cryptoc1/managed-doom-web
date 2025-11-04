using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault( args );
builder.Logging.SetMinimumLevel( builder.HostEnvironment.IsDevelopment()
    ? LogLevel.Information
    : LogLevel.Error );

await using var app = builder.Build();
await app.RunAsync();