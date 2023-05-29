using DevExpress.Blazor;
using WebImageLibPoc;
using WebImageLibPoc.Infra;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Service Registrations
builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddSingleton<IToDoService, ToDoService>();

// Register Dev Express Blazor
builder.Services.AddDevExpressBlazor(configure => configure.BootstrapVersion = BootstrapVersion.v5);

await builder.Build().RunAsync();
