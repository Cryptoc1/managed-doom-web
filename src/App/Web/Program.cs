using ManagedDoom.App.Web;
using ManagedDoom.App.Web.Infrastructure;

var builder = WebApplication.CreateBuilder( args )
    .WithManagedDoomWeb();

await using var app = builder.Build();
if( app.Environment.IsDevelopment() )
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCookiePolicy();

app.UseRequestDecompression();
app.UseResponseCaching();
if( !app.Environment.IsDevelopment() )
{
    app.UseResponseCompression();
}

app.UseCors();
app.UseRouting();
app.UseRequestTimeouts();
app.UseRequestCancellation();

app.MapStaticAssets();
app.MapFallbackToController( "Index", "App" );

await app.RunAsync();