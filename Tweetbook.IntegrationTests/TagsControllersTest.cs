using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1;

namespace Tweetbook.IntegrationTests
{
    [TestFixture]
    internal class TagsControllersTest
    {
        [Test]
        public async Task CheckStatusSendRequestShouldReturnOk()
        {
            // Arrange
            WebApplicationFactory<Startup> webHost = new WebApplicationFactory<Startup>().WithWebHostBuilder(_ => { });
            HttpClient httpClient = webHost.CreateClient();

            // Act
            HttpResponseMessage response = await httpClient.GetAsync(ApiRoutes.Tags.GetAll);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
