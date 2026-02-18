namespace CommerceHub.BackofficeApi.Features.Reporting.Dashboard;

public sealed record DashboardResponse(
    int ProductCount,
    DateTime GeneratedAt);
