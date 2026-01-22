using Hangfire;
using Microsoft.AspNetCore.Builder;
using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Analytics
{
    public static class AnalyticsJobRegistrationExtensions
    {
        public static IApplicationBuilder RegisterAnalyticsRecurringJobs(this IApplicationBuilder app)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

                RecurringJob.AddOrUpdate<FrequencyAggregationRunner>(
                    "frequency-aggregation-daily",
                    runner => runner.RunDailyAsync(),
                    Cron.Daily(2),
                    new RecurringJobOptions { TimeZone = vnTimeZone });

                RecurringJob.AddOrUpdate<SeverityAggregationRunner>(
                    "severity-aggregation-daily",
                    runner => runner.RunDailyAsync(),
                    Cron.Daily(2, 15),
                    new RecurringJobOptions { TimeZone = vnTimeZone });

                RecurringJob.AddOrUpdate<HotspotAggregationRunner>(
                    "hotspot-aggregation-weekly",
                    runner => runner.RunWeeklyAsync(),
                    Cron.Weekly(DayOfWeek.Monday, 3),
                    new RecurringJobOptions { TimeZone = vnTimeZone });

                RecurringJob.AddOrUpdate<FrequencyAggregationRunner>(
                    "frequency-aggregation-monthly",
                    runner => runner.RunMonthlyAsync(),
                    Cron.Monthly(1, 4),
                    new RecurringJobOptions { TimeZone = vnTimeZone });
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine("Timezone 'Asia/Ho_Chi_Minh' not found. Ensure tzdata is installed in the host/container.");
            }
            catch (InvalidTimeZoneException)
            {
                Console.WriteLine("Timezone 'Asia/Ho_Chi_Minh' is invalid on this platform. Ensure tzdata is installed in the host/container.");
            }

            return app;
        }
    }
}


