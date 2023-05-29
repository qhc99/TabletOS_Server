using Microsoft.AspNetCore.Authorization;

// save time of the day instead of the date
public class MaintenanceTimeRequirement : IAuthorizationRequirement
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}

public class MaintenanceTimeAuthorizationHandler : AuthorizationHandler<MaintenanceTimeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MaintenanceTimeRequirement requirement)
    {
        var isAuthorized = true;
        if (!context.User.IsInRole("Administrator"))
        {
            var time = TimeOnly.FromDateTime(DateTime.Now);
            if (time >= requirement.StartTime && time <
            requirement.EndTime)
            {
                isAuthorized = false;
            }
        }
        if (isAuthorized)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}