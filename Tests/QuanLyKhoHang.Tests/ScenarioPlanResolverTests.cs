using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class ScenarioPlanResolverTests
{
    [Fact]
    public void Every_workbook_row_has_a_group_action_input_and_expectation()
    {
        var plans = ScenarioPlanResolver.ResolveAll(ExcelTestCaseLoader.LoadAll());

        Assert.Equal(204, plans.Count);
        Assert.All(plans, plan =>
        {
            Assert.False(string.IsNullOrWhiteSpace(plan.Action));
            Assert.False(string.IsNullOrWhiteSpace(plan.Target));
            Assert.Contains("raw", plan.Input.Keys);
            Assert.False(string.IsNullOrWhiteSpace(plan.ExpectedResponse));
            Assert.False(string.IsNullOrWhiteSpace(plan.ExpectedDatabase));
        });
    }

    [Fact]
    public void Provider_specific_and_benchmark_cases_are_manual_review_only()
    {
        var plans = ScenarioPlanResolver.ResolveAll(ExcelTestCaseLoader.LoadAll());
        var review = plans.Where(plan => plan.Status == AutomationStatus.ManualReview).ToArray();

        Assert.Equal(4, review.Length);
        Assert.All(review, plan =>
        {
            Assert.Equal(ScenarioGroup.ManualReview, plan.Group);
            Assert.False(string.IsNullOrWhiteSpace(plan.ReviewReason));
        });
    }

    [Fact]
    public void Cases_without_a_verified_shared_handler_are_not_reported_as_executable()
    {
        var plans = ScenarioPlanResolver.ResolveAll(ExcelTestCaseLoader.LoadAll());
        var unresolved = plans.Where(plan => plan.Status == AutomationStatus.NeedsHandler).ToArray();

        Assert.NotEmpty(unresolved);
        Assert.All(unresolved, plan => Assert.False(string.IsNullOrWhiteSpace(plan.ReviewReason)));
    }
}
