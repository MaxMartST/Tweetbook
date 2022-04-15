using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Data;

namespace Tweetbook.IntegrationTests
{
    public class IntegrationTests
    {
        protected readonly HttpClient TestClient;

        public IntegrationTests()
        {
            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder => 
                {
                    builder.ConfigureServices(services => {
                        var descriptor = services.SingleOrDefault(d => 
                            d.ServiceType == typeof(DbContextOptions<DataContext>));

                        services.Remove(descriptor);
                        services.AddDbContext<DataContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestDb");
                        });
                    });
                });
            
            TestClient = appFactory.CreateClient();
        }

        protected async Task AuthenticateAsync()
        {
            TestClient.DefaultRequestHeaders.Clear();
            TestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await GetJwtAsync());
        }


        private async Task<string> GetJwtAsync()
        {
            var url = ApiRoutes.Identity.Register;
            var user = new UserRegistrationRequest
            {
                Email = "user-1@example.com",
                Password = "User-1"
            };

            var response = await TestClient
                .PostAsJsonAsync(url, user);

            var registrationResponse = await response.Content
                .ReadAsAsync<AuthSuccessResponse>();

            return registrationResponse.Token;
        }
    }
}
