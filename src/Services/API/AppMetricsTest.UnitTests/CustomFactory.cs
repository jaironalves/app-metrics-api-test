using AppMetricsTest.API;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppMetricsTest.UnitTests
{
    public class CustomFactory : WebApplicationFactory<Startup>
    {
    }
}
