using App.Metrics;
using App.Metrics.Filtering;
using App.Metrics.Meter;
using AppMetricsTest.API;
using AppMetricsTest.API.Metrics;
using AppMetricsTest.API.Metrics.Internal;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppMetricsTest.UnitTests
{
    public class UnitTest1
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
                .Build();

            //var metrics = metricsRoot as IMetrics;



            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.DecrementInProgressRequests(mockAccessor.Object);

            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.IncrementInProgressRequests(mockAccessor.Object);
            metrics.DecrementInProgressRequests(mockAccessor.Object);

            // Assert
            var metricsFilter = new MetricsFilter().WhereContext("HttpClient.Requests");
            var metricsDataValueSource = metrics.Snapshot.Get(metricsFilter);
            var metricsContextValueSource = metricsDataValueSource.Contexts.Single();

            var counterValueSource = metricsContextValueSource.Counters.Single();

            var metricTagsDictionary = counterValueSource.Tags.ToDictionary();

            var metricTagRequest = metricTagsDictionary.Where(kvp => kvp.Key.Equals("request")).Single();
            var metricTagSession = metricTagsDictionary.Where(kvp => kvp.Key.Equals("session")).Single();

            counterValueSource.Value.Count.Should().Be(2);
            metricTagRequest.Value.Should().Be(postRequest.Command);
            metricTagSession.Value.Should().Be(postRequest.Session);
        }
    }
}
