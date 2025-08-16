using NUnit.Framework;
using StorySpoiler.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
namespace StorySpoiler
{
    public class Tests
    {
        private RestClient client;
        private static string createdStoryId;
        private static string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net/";

        [OneTimeSetUp]
        public void Setup()
        {
           
            string token = GetJwtToken("BackendTest", "ElaboratePass2025");

            
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var story = new
            {
                Title = "New stroy",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString()?.Trim();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(string.IsNullOrEmpty(createdStoryId), Is.False, "StoryId was not returned!");
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));

        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnOk()
        {
            var edited = new
            {
                Title = "edited story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(edited);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully edited"));

        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(stories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);


            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Deleted successfully!"));

        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new
            {
                Name = "",
                Description = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string fakeId = "123";
            var story = new
            {
                Title = "edited story",
                Description = "Test story description",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("No spoilers..."));

        }

        [Test, Order(7)]
        public void DeleteNonExistingFood_ShouldReturnBadRequest()
        {
            string fakeId = "123";
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }

    }
}