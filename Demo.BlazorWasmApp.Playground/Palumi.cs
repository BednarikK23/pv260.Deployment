// Pulumi – Infrastructure as Code example for Azure
// Docs: https://www.pulumi.com/docs/clouds/azure/
//
// This stack provisions:
//   1. Resource Group   – logical container for all Azure resources
//   2. Storage Account  – blob storage (e.g. for static files or backups)
//   3. App Service Plan – defines the region and pricing tier
//   4. Web App   – hosts the Blazor WASM application
//
// Run with:  pulumi up

using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

await Pulumi.Deployment.RunAsync<AppStack>();

class AppStack : Stack
{
    public AppStack()
    {
        // ── 1. Resource Group ────────────────────────────────────────────
        // Every Azure resource must belong to a resource group.
        var resourceGroup = new ResourceGroup("rg-blazor-demo", new ResourceGroupArgs
        {
            Location = "westeurope",
        });

        // ── 2. Storage Account ───────────────────────────────────────────
        // Used for storing static assets or application backups.
        var storageAccount = new StorageAccount("stblazdemo", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Kind = Kind.StorageV2,
            Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
            {
                Name = SkuName.Standard_LRS,  // Locally redundant – cheapest option
            },
        });

        // ── 3. App Service Plan ──────────────────────────────────────────
        // Defines what hardware/pricing tier the Web App runs on.
        var appServicePlan = new AppServicePlan("plan-blazor-demo", new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = new SkuDescriptionArgs
            {
                Name = "B1",    // Basic tier – suitable for demos and dev
                Tier = "Basic",
            },
        });

        // ── 4. Web App ───────────────────────────────────────────────────
        // The actual application host. References the plan created above.
        var webApp = new WebApp("app-blazor-demo", new WebAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            ServerFarmId = appServicePlan.Id,   // link to the plan
            HttpsOnly = true,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs
                        {
                            Name  = "WEBSITE_RUN_FROM_PACKAGE",
                            Value = "1",   // app is deployed as a zip package
                        },
                        new NameValuePairArgs
                        {
                            Name  = "StorageAccountName",
                            Value = storageAccount.Name,   // Output<T> – resolved at deploy time
                    },
                },
            },
        });

        // ── Outputs ──────────────────────────────────────────────────────
        // These values are printed after `pulumi up` and saved in the stack state.
        AppUrl = webApp.DefaultHostName.Apply(h => $"https://{h}");
        StorageAccountName = storageAccount.Name;
    }

    /// <summary>Public HTTPS URL of the deployed Blazor application.</summary>
    [Output] public Output<string> AppUrl { get; set; }

    /// <summary>Name of the provisioned Storage Account.</summary>
    [Output] public Output<string> StorageAccountName { get; set; }
}
