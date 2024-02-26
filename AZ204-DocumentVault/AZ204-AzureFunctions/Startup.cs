using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AZ204_AzureFunctions.Startup))]

namespace AZ204_AzureFunctions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        //throw new System.NotImplementedException();
    }
}