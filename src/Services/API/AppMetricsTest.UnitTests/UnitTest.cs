using App.Metrics;
using App.Metrics.Counter;
using AppMetricsTest.API;
using AppMetricsTest.API.Metrics;
using AppMetricsTest.API.Metrics.Formatters;
using AppMetricsTest.API.Metrics.Internal;
using AppMetricsTest.API.Metrics.Options;
using AppMetricsTest.API.Provider;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AppMetricsTest.UnitTests
{
    public class UnitTest
    {
        [Fact]
        public void TestFeature()
        {
            var mockEndpointFeature = new Mock<IEndpointFeature>();

            static Task requestDelegateCompletedTask(HttpContext context) => Task.CompletedTask;

            var routePattern = RoutePatternFactory.Parse("api/tests/{id}");

            var routeEndpoint = new RouteEndpoint(requestDelegateCompletedTask, routePattern, 1, EndpointMetadataCollection.Empty, nameof(RouteEndpoint));
            var endpointToTest = new Endpoint(requestDelegateCompletedTask, EndpointMetadataCollection.Empty, nameof(Endpoint));

            mockEndpointFeature.Setup(s => s.Endpoint)
                .Returns(endpointToTest);


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Features.Set(mockEndpointFeature.Object);


            var endpointFeature = httpContext.Features[typeof(IEndpointFeature)] as IEndpointFeature;


            var endpoint = endpointFeature?.Endpoint;

            RoutePattern routePatternTest = null;

            if (endpoint != null)
            {
                routePatternTest = (endpoint as RouteEndpoint)?.RoutePattern;
            }

            var test = routePatternTest.RawText;
        }

        [Fact]
        public void TestListener()
        {
            static void OnMeasurementRecorded<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object>> tags, object state)
            {
                Console.WriteLine($"{instrument.Name} recorded measurement {measurement}");
            }

            Meter s_meter = new Meter("HatCo.HatStore", "1.0.0");
            Counter<int> s_hatsSold = s_meter.CreateCounter<int>(name: "hats-sold",
                                                               unit: "Hats",
                                                               description: "The number of hats sold in our store");

            using MeterListener meterListener = new MeterListener();
            meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "HatCo.HatStore")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            meterListener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
            meterListener.Start();

            var tags = new KeyValuePair<string, object?>[]
            {
                new KeyValuePair<string, object>("tag01", "123"),
                new KeyValuePair<string, object>("tag02", 3)
            };

            s_hatsSold.Add(10, tags);
            s_hatsSold.Add(3, tags);
        }

        [Fact]
        public void TestMetrics()
        {
            PostRequest postRequest = new PostRequest()
            {
                Alternate = false,
                Command = "CMD001",
                Description = "Ola",
                Session = "S001"
            };

            var mockAccessor = new Mock<IRequestAccessor>();
            mockAccessor.Setup(s => s.PostRequest)
                .Returns(postRequest);

            IMetrics metrics = AppMetrics.CreateDefaultBuilder()
                //.Report.Using()
                .Build();

            //var metrics = metricsRoot as IMetrics;



            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.DecrementInProgressRequests(mockAccessor.Object);

            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.DecrementInProgressRequests(mockAccessor.Object);

            // Assert


            var metricsContextValueSource = metrics.Snapshot.GetForContext("HttpClient.Requests");

            metrics.Manage.Reset();

            //metrics.Snapshot.GetMeterValue()

            //metrics.

            //

            var metricsContextValueSource02 = metrics.Snapshot.GetForContext("HttpClient.Requests");

            //var metricsContextValueSource = metricsDataValueSource.Contexts.Single();

            var counterValueSource = metricsContextValueSource.Counters.Single();

            var metricTagsDictionary = counterValueSource.Tags.ToDictionary();

            var metricTagRequest = metricTagsDictionary.Where(kvp => kvp.Key.Equals("request")).Single();
            var metricTagSession = metricTagsDictionary.Where(kvp => kvp.Key.Equals("session")).Single();

            counterValueSource.Value.Count.Should().Be(2);
            metricTagRequest.Value.Should().Be(postRequest.Command);
            metricTagSession.Value.Should().Be(postRequest.Session);
        }

        private MetricsDataValueSource FilledMetrics()
        {
            var metrics = AppMetrics.CreateDefaultBuilder().Build();

            var counterOptions = new CounterOptions()
            {
                Context = "Context",
            };

            metrics.Provider.Counter.Instance(counterOptions)
                .Increment(10);

            return metrics.Snapshot.Get();
        }

        [Fact]
        public async Task ResetTest_ResetContext()
        {
            var mockMetrics = new Mock<IMetricsRoot>();
            var mockMetricsManage = new Mock<IManageMetrics>();
            var mockOptions = new Mock<IOptions<MetricCustomOptions>>();
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();

            var options = new MetricCustomOptions()
            {
                ContextName = "Context",
                ContextNameReset = "ContextMeta"
            };

            var date = new DateTime(2023, 01, 01, 15, 00, 00);
            var dateReset = new DateTime(2023, 01, 01, 15, 02, 00);
            var dateOnReset = new DateTime(2023, 01, 01, 15, 03, 00);

            mockMetrics.Setup(s => s.Manage)
                .Returns(mockMetricsManage.Object);

            mockOptions.Setup(s => s.Value)
                .Returns(options);

            mockDateTimeProvider.SetupSequence(s => s.Now)
                .Returns(date)
                .Returns(dateOnReset)
                .Returns(dateOnReset)
                .Throws(new InvalidOperationException("Not expected call"));

            var metricsData = FilledMetrics();

            var reset = new MetricsPrometheusAutomaticReset(mockMetrics.Object, mockOptions.Object, mockDateTimeProvider.Object);

            using var stream = new MemoryStream();


            // Action
            var streamLengthBefore = stream.Length;

            await reset.WriteAsync(stream, metricsData);
            var dateTimeResetFirstCall = reset.DateTimeReset;

            await reset.WriteAsync(stream, metricsData);
            var dateTimeResetSecondCall = reset.DateTimeReset;

            var streamLengthAfter = stream.Length;

            // Assert
            dateTimeResetFirstCall.Value.Should().Be(dateReset);
            dateTimeResetSecondCall.Should().BeNull();

            streamLengthBefore.Should().Be(0);
            streamLengthAfter.Should().BePositive();

            mockMetricsManage.Verify(v => v.ShutdownContext(It.Is<string>(m => m.Equals(options.ContextNameReset))), Times.Once());
        }

        [Fact]
        public async Task ResetTest_ResetIfNowEqualNextReset()
        {
            var mockMetrics = new Mock<IMetricsRoot>();
            var mockMetricsManage = new Mock<IManageMetrics>();
            var mockOptions = new Mock<IOptions<MetricCustomOptions>>();
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();

            var options = new MetricCustomOptions()
            {
                ContextName = "Context",
                ContextNameReset = "ContextMeta"
            };

            var date = new DateTime(2023, 01, 01, 15, 00, 00);
            var dateReset = new DateTime(2023, 01, 01, 15, 02, 00);
            var dateNextReset = new DateTime(2023, 01, 01, 15, 04, 00);

            mockMetrics.Setup(s => s.Manage)
                .Returns(mockMetricsManage.Object);

            mockOptions.Setup(s => s.Value)
                .Returns(options);

            mockDateTimeProvider.SetupSequence(s => s.Now)
                .Returns(date)
                .Returns(dateNextReset)
                .Returns(dateNextReset)
                .Throws(new InvalidOperationException("Not expected call"));

            var reset = new MetricsPrometheusAutomaticReset(mockMetrics.Object, mockOptions.Object, mockDateTimeProvider.Object);

            using var stream = new MemoryStream();

            await reset.WriteAsync(stream, MetricsDataValueSource.Empty);
            var dateTimeResetFirstCall = reset.DateTimeReset;

            await reset.WriteAsync(stream, MetricsDataValueSource.Empty);
            var dateTimeResetSecondCall = reset.DateTimeReset;

            dateTimeResetFirstCall.Value.Should().Be(dateReset);
            dateTimeResetSecondCall.Should().BeNull();

            mockMetricsManage.Verify(v => v.ShutdownContext(It.Is<string>(m => m.Equals(options.ContextNameReset))), Times.Once());
        }

        [Fact]
        public async Task ResetTest_OnlyNowAfterDateReset()
        {
            var mockMetrics = new Mock<IMetricsRoot>();
            var mockMetricsManage = new Mock<IManageMetrics>();
            var mockOptions = new Mock<IOptions<MetricCustomOptions>>();
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();

            var options = new MetricCustomOptions()
            {
                ContextName = "Context",
                ContextNameReset = "ContextMeta"
            };

            var date = new DateTime(2023, 01, 01, 15, 00, 00);
            var dateReset = new DateTime(2023, 01, 01, 15, 02, 00);            

            mockMetrics.Setup(s => s.Manage)
                .Returns(mockMetricsManage.Object);

            mockOptions.Setup(s => s.Value)
                .Returns(options);

            mockDateTimeProvider.SetupSequence(s => s.Now)
                .Returns(date)
                .Returns(dateReset)                
                .Throws(new InvalidOperationException("Not expected call"));

            var reset = new MetricsPrometheusAutomaticReset(mockMetrics.Object, mockOptions.Object, mockDateTimeProvider.Object);

            using var stream = new MemoryStream();

            await reset.WriteAsync(stream, MetricsDataValueSource.Empty);
            var dateTimeResetFirstCall = reset.DateTimeReset;

            await reset.WriteAsync(stream, MetricsDataValueSource.Empty);
            var dateTimeResetSecondCall = reset.DateTimeReset;

            dateTimeResetFirstCall.Value.Should().Be(dateReset);
            dateTimeResetSecondCall.Value.Should().Be(dateReset);

            mockMetricsManage.Verify(v => v.ShutdownContext(It.Is<string>(m => m.Equals(options.ContextNameReset))), Times.Never());
        }

        [Fact]
        public async Task ResetTest_RestartDateResetIfDateTimeNextGreaterThanNow()
        {
            var mockMetrics = new Mock<IMetricsRoot>();
            var mockMetricsManage = new Mock<IManageMetrics>();
            var mockOptions = new Mock<IOptions<MetricCustomOptions>>();
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();

            var options = new MetricCustomOptions()
            {
                ContextName = "Context",
                ContextNameReset = "ContextMeta"
            };

            var date = new DateTime(2023, 01, 01, 15, 00, 00);
            var dateReset = new DateTime(2023, 01, 01, 15, 02, 00);            
            var dateAfterNextReset = new DateTime(2023, 01, 01, 15, 30, 00);

            mockMetrics.Setup(s => s.Manage)
                .Returns(mockMetricsManage.Object);

            mockOptions.Setup(s => s.Value)
                .Returns(options);

            mockDateTimeProvider.SetupSequence(s => s.Now)
                .Returns(date)
                .Returns(dateAfterNextReset)
                .Returns(dateAfterNextReset)
                .Throws(new InvalidOperationException("Not expected call"));

            var reset = new MetricsPrometheusAutomaticReset(mockMetrics.Object, mockOptions.Object, mockDateTimeProvider.Object);

            using var stream = new MemoryStream();

            await reset.WriteAsync(stream, MetricsDataValueSource.Empty);
            var dateTimeResetFirstCall = reset.DateTimeReset;

            await reset.WriteAsync(stream, MetricsDataValueSource.Empty);
            var dateTimeResetSecondCall = reset.DateTimeReset;

            dateTimeResetFirstCall.Value.Should().Be(dateReset);
            dateTimeResetSecondCall.Should().BeNull();

            mockMetricsManage.Verify(v => v.ShutdownContext(It.Is<string>(m => m.Equals(options.ContextNameReset))), Times.Never());
        }

        [Fact]
        public async Task TestAsyncLocal()
        {
            AsyncLocal<string> asyncLocal = new AsyncLocal<string>
            {
                Value = "A01"
            };

            var t01 = Task.Run(async () =>
            {
                asyncLocal.Value.Should().Be("A01");
                //asyncLocal.Value = "A01T";
                await Task.Delay(400);
                asyncLocal.Value.Should().Be("A01");
            });

            asyncLocal.Value = "A02";

            var t02 = Task.Run(async () =>
            {
                asyncLocal.Value.Should().Be("A02");
                //asyncLocal.Value = "A02T";
                await Task.Delay(100);
                asyncLocal.Value.Should().Be("A02");
            });

            await Task.WhenAll(t01, t02);

            await t01;
            await t02;
            //await t01;
        }

    }
}
