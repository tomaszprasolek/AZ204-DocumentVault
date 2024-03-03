using AZ204_DocumentVault.Pages;
using AZ204_DocumentVault.Services;
using AZ204_DocumentVault.Services.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add services to the container.
builder.Services
    .AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddScoped<IKeyVaultService, KeyVaultService>();
builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<IStorageAccountService, StorageAccountService>();

builder.Services.Configure<AzureConfig>(
    builder.Configuration.GetSection(nameof(AzureConfig)));

builder.Services.AddHttpClient("AzureFunctionsClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("AzureConfig:FunctionApp:BaseUrl")!);
    client.DefaultRequestHeaders.Clear();
});

// ----------------
// BUILD APP
// ----------------
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();