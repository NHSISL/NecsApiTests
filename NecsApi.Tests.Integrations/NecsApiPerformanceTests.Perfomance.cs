// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NecsApi.Tests.Integrations.Models;
using NecsApi.Tests.Integrations.Models.NECS.Requests;
using NecsApi.Tests.Integrations.Models.NECS.Responses;

namespace NecsApi.Tests.Integrations
{
    public partial class NecsApiPerformanceTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(300)]
        [InlineData(350)]
        [InlineData(400)]
        [InlineData(450)]
        [InlineData(500)]
        public async Task ShouldMeasureReIdentificationPerformanceForBatchAsync(int records)
        {
            // Given
            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: records);

            // When
            var result =
                await apiClient.PostContentAsync<NecsReIdentificationRequest, NecsReIdentificationResponse>
                    (necsConfiguration.ApiUrl, randomReIdentificationRequest);

            // Then
            result.Should().NotBeNull();
            result.ProcessedCount.Should().Be(records);
            output.WriteLine($"Items in the request: {records}");
            output.WriteLine($"ElapsedTime: {result.ElapsedTime}");
        }


        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(300)]
        [InlineData(350)]
        [InlineData(400)]
        [InlineData(450)]
        [InlineData(500)]

        public async Task ShouldMeasureReIdentificationRepeatPerformanceAsync(int records)
        {
            // Given
            int testIterations = 10; // Number of consecutive tests to run
            List<TimeSpan> elapsedTimes = new List<TimeSpan>();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: records);

            // When
            for (int i = 0; i < testIterations; i++)
            {
                randomReIdentificationRequest.RequestId = Guid.NewGuid();
                var stopwatch = Stopwatch.StartNew();

                var result =
                    await apiClient.PostContentAsync<NecsReIdentificationRequest, NecsReIdentificationResponse>
                        (necsConfiguration.ApiUrl, randomReIdentificationRequest);

                stopwatch.Stop();

                // Collect elapsed time
                elapsedTimes.Add(stopwatch.Elapsed);

                // Assert each iteration result
                result.Should().NotBeNull();
            }

            // Then
            TimeSpan minTime = elapsedTimes.Min();
            TimeSpan maxTime = elapsedTimes.Max();
            TimeSpan averageTime = TimeSpan.FromMilliseconds(elapsedTimes.Average(et => et.TotalMilliseconds));
            output.WriteLine($"Items in the request: {records}");
            output.WriteLine($"Performance over {testIterations} iterations:");
            output.WriteLine($"Minimum Time: {minTime}");
            output.WriteLine($"Maximum Time: {maxTime}");
            output.WriteLine($"Average Time: {averageTime}");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public async Task ShouldPerformConcurrentLoadTestAsync(int requests)
        {
            // Given
            int numberOfRequests = requests;
            int randomCount = 500;
            List<Task<(bool isSuccess, TimeSpan elapsedTime)>> tasks = new();

            // When
            for (int i = 0; i < numberOfRequests; i++)
            {
                var requestCopy = CreateRandomNecsReIdentificationRequest(count: randomCount);
                requestCopy.RequestId = Guid.NewGuid();
                tasks.Add(SendRequestAsync(requestCopy));
            }

            var results = await Task.WhenAll(tasks);

            // Then
            int successCount = results.Count(r => r.isSuccess);
            int failureCount = results.Count(r => !r.isSuccess);
            var elapsedTimes = results.Select(r => r.elapsedTime).ToList();
            TimeSpan minTime = elapsedTimes.Min();
            TimeSpan maxTime = elapsedTimes.Max();
            TimeSpan averageTime = TimeSpan.FromMilliseconds(elapsedTimes.Average(et => et.TotalMilliseconds));
            double requestsPerSecond = numberOfRequests / maxTime.TotalSeconds;
            output.WriteLine($"Load Test Results for {numberOfRequests} concurrent requests:");
            output.WriteLine($"Successful Requests: {successCount}");
            output.WriteLine($"Failed Requests: {failureCount}");
            output.WriteLine($"Minimum Time: {minTime}");
            output.WriteLine($"Maximum Time: {maxTime}");
            output.WriteLine($"Average Time: {averageTime}");
            output.WriteLine($"Requests Per Second: {requestsPerSecond:F2}");

            successCount.Should().BeGreaterThan(
                expected: (int)(numberOfRequests * 0.9),
                because: "at least 90% of requests should succeed");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public async Task ShouldPerformConcurrentLoadTestBasedOnRandomTestDataAsync(int requests)
        {
            // Given
            int numberOfRequests = requests;
            int itemCount = 500;
            List<LinkedItem> linkedItems = GetRandomLinkedItems(itemCount);

            List<Task<(bool isSuccess, TimeSpan elapsedTime)>> tasks = new();

            // When
            for (int i = 0; i < numberOfRequests; i++)
            {
                var requestCopy = CreateRandomNecsReIdentificationRequest(linkedItems);
                requestCopy.RequestId = Guid.NewGuid();
                tasks.Add(SendRequestAsync(requestCopy));
            }

            var results = await Task.WhenAll(tasks);

            // Then
            int successCount = results.Count(r => r.isSuccess);
            int failureCount = results.Count(r => !r.isSuccess);
            var elapsedTimes = results.Select(r => r.elapsedTime).ToList();
            TimeSpan minTime = elapsedTimes.Min();
            TimeSpan maxTime = elapsedTimes.Max();
            TimeSpan averageTime = TimeSpan.FromMilliseconds(elapsedTimes.Average(et => et.TotalMilliseconds));
            double requestsPerSecond = numberOfRequests / maxTime.TotalSeconds;
            output.WriteLine($"Load Test Results for {numberOfRequests} concurrent requests:");
            output.WriteLine($"Successful Requests: {successCount}");
            output.WriteLine($"Failed Requests: {failureCount}");
            output.WriteLine($"Minimum Time: {minTime}");
            output.WriteLine($"Maximum Time: {maxTime}");
            output.WriteLine($"Average Time: {averageTime}");
            output.WriteLine($"Requests Per Second: {requestsPerSecond:F2}");

            successCount.Should().BeGreaterThan(
                expected: (int)(numberOfRequests * 0.9),
                because: "at least 90% of requests should succeed");
        }
    }
}
