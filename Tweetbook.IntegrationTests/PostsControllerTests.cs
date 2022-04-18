using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1;
using Tweetbook.Domain;
using Xunit;

namespace Tweetbook.IntegrationTests
{
    public class PostsControllerTests : IntegrationTest
    {
        [Fact]
        public async Task GetAll_WithoutAnyPost_ReturnsEmptyResponse()
        { 
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await TestClient.GetAsync(ApiRoutes.Posts.GetAll);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<Post>>()).Should().BeEmpty();
        }
    }
}
