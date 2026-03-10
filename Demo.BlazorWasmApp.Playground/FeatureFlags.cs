using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

public class NotProgram
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddFeatureManagement()
                        .AddFeatureFilter<PercentageFilter>()
                        .AddFeatureFilter<UserGroupFeatureFilter>();

        var app = builder.Build();

        app.MapGet("/", async (IFeatureManager featureManager) =>
        {
            var newDashboardEnabled = await featureManager.IsEnabledAsync("NewDashboard");
            var experimentalSearchEnabled = await featureManager.IsEnabledAsync("ExperimentalSearch");

            var dashboardData = new List<string>
            {
                "Data Point 1",
                "Data Point 2",
                "Data Point 3",
            };

            var response = new
            {
                NewDashboardEnabled = newDashboardEnabled,
                ExperimentalSearchEnabled = experimentalSearchEnabled,
                DashboardData = newDashboardEnabled ? dashboardData : null,
                Message = newDashboardEnabled 
                    ? "Welcome to the new dashboard!"
                    : "Welcome to the old dashboard!",
            };

            return response;
        });


        app.Run();
    }
}

public class UserGroupFeatureFilter : IFeatureFilter
{
    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var parameters = context.Parameters.Get<UserGroupFeatureFilterSettings>();
        var currentUser = "user1@example.com"; // This should be replaced with actual user context

        return Task.FromResult(parameters.AllowedUsers.Contains(currentUser));
    }
}

public class UserGroupFeatureFilterSettings
{
    public List<string> AllowedUsers { get; set; }
}
