using MauiDay.Core.Models;
using MauiDay.Core.Services;

namespace MauiDay.App.Tests;

public sealed class RefreshNoticeBuilderTests
{
    [Fact]
    public void BothRefreshedReportsUpdatedJustNow()
    {
        var notice = RefreshNoticeBuilder.Build(
            ContentSource.Remote,
            configurationRefreshed: true,
            conferenceRefreshed: true);

        Assert.Equal("App information updated just now.", notice);
    }

    [Fact]
    public void ScheduleRefreshedButConfigReusedReportsScheduleUpdate()
    {
        var notice = RefreshNoticeBuilder.Build(
            ContentSource.Remote,
            configurationRefreshed: false,
            conferenceRefreshed: true);

        Assert.Equal("Schedule updated from Sessionize.", notice);
    }

    [Fact]
    public void FailedScheduleFetchWithPriorRemoteDoesNotClaimFreshness()
    {
        // Point C: the schedule fetch failed this cycle but a previously fetched remote schedule
        // is still shown. It must never be labelled "Updated just now."
        var notice = RefreshNoticeBuilder.Build(
            ContentSource.Remote,
            configurationRefreshed: true,
            conferenceRefreshed: false);

        Assert.Equal("Couldn't reach Sessionize. Showing the latest schedule you have.", notice);
        Assert.DoesNotContain("just now", notice, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FailedScheduleFetchWithCachedScheduleReportsSaved()
    {
        var notice = RefreshNoticeBuilder.Build(
            ContentSource.Cache,
            configurationRefreshed: true,
            conferenceRefreshed: false);

        Assert.Equal("Using saved schedule. Pull to retry.", notice);
    }

    [Fact]
    public void FailedScheduleFetchWithBundledScheduleReportsBundled()
    {
        var notice = RefreshNoticeBuilder.Build(
            ContentSource.Bundled,
            configurationRefreshed: false,
            conferenceRefreshed: false);

        Assert.Equal("Using bundled schedule. Pull to retry.", notice);
    }

    [Theory]
    [InlineData(ContentSource.Remote)]
    [InlineData(ContentSource.Cache)]
    [InlineData(ContentSource.Bundled)]
    public void UpdatedJustNowIsOnlyEverReportedWhenScheduleWasFetched(ContentSource conferenceSource)
    {
        var withoutFetch = RefreshNoticeBuilder.Build(
            conferenceSource,
            configurationRefreshed: true,
            conferenceRefreshed: false);

        Assert.NotEqual("App information updated just now.", withoutFetch);
    }
}
