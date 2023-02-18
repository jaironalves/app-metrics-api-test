using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AppMetricsTest.UnitTests
{
    public class TestFactory : IClassFixture<CustomFactory>
    {
        private readonly CustomFactory customFactory;

        public TestFactory(CustomFactory customFactory)
        {
            this.customFactory = customFactory;
        }

        [Fact]
        public void TestFactory01()
        {
            var client =
            customFactory
                .WithWebHostBuilder(builder =>
                {

                })
                .CreateClient();

            client.Should().NotBeNull();
        }

        [Fact]
        public void TestFactory02()
        {
            var client =
            customFactory
                .WithWebHostBuilder(builder =>
                {

                })
                .CreateClient();

            client.Should().NotBeNull();
        }
    }
}
