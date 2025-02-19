// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NecsApi.Tests.Integrations.Models.NECS.Requests;
using NecsApi.Tests.Integrations.Models.NECS.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NecsApi.Tests.Integrations
{
    public partial class NecsApiValidationTests
    {
        [Fact(DisplayName = "Validation - 2.01 - Body required")]
        public async Task ShouldThrowValidationErrorWhenNoBodyPresentAsync()
        {
            // Given
            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.16",
                Title = "Unsupported Media Type",
                Status = 415
            };

            // When
            var response =
                await httpClient.PostAsync(requestUri: necsConfiguration.ApiUrl, content: null);

            string actualContent = await response.Content.ReadAsStringAsync();
            dynamic actualResponse = JsonConvert.DeserializeObject(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            ((string)actualResponse.type).Should().Be(expectedResponse.Type);
            ((string)actualResponse.title).Should().Be(expectedResponse.Title);
            ((int)actualResponse.status).Should().Be(expectedResponse.Status);
        }

        [Fact(DisplayName = "Validation - 2.02 - Body empty")]
        public async Task ShouldThrowValidationErrorWhenBodyIsEmptyAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "Reason", new[] { "The Reason field is required." } },
                { "RequestId", new[] { "The RequestId field is required.", "The Guid value cannot be null." } },
                { "Organisation", new[] { "The Organisation field is required." } },
                { "UserIdentifier", new[] { "The UserIdentifier field is required." } },
                { "PseudonymisedNumbers", new[] { "The PseudonymisedNumbers field is required." } }
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(new { }),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.03 - RequestId missing")]
        public async Task ShouldThrowValidationErrorWhenRequestIdIsMissingAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "RequestId", new[] {
                    "The RequestId field is required.",
                    "The Guid value cannot be null." }
                },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.04 - RequestId must be unique")]
        public async Task ShouldThrowValidationErrorWhenRequestIdIsNotUniqueAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "RequestId", new[] { "RequestId must be unique." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.05 - RequestId not default Guid")]
        public async Task ShouldThrowValidationErrorWhenRequestIdIsEmptyGuidAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "RequestId", new[] { "The Guid value cannot be the default Guid." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                RequestId = Guid.Empty,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.06 - RequestId not valid Guid")]
        public async Task ShouldThrowValidationErrorWhenRequestIdIsInvalidGuidAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "RequestId", new[] { "The RequestId field is required.","The Guid value cannot be null." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                RequestId = GetRandomStringWithLengthOf(5),
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.07 - PseudonymisedNumbers field is required")]
        public async Task ShouldThrowValidationErrorWhenPseudonymisedNumbersMissingAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "PseudonymisedNumbers", new[] { "The PseudonymisedNumbers field is required." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.08 - PseudonymisedNumbers required")]
        public async Task ShouldThrowValidationErrorWhenPseudonymisedNumbersRequiredAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "PseudonymisedNumbers", new[] { "At least one PseudonymisedNumber is required." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            randomReIdentificationRequest.PseudonymisedNumbers.Clear();

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.09 - UserIdentifier required")]
        public async Task ShouldThrowValidationErrorWhenUserIdentifierRequiredAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "UserIdentifier", new[] { "The UserIdentifier field is required." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.10 - Organisation required")]
        public async Task ShouldThrowValidationErrorWhenOrganisationRequiredAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "Organisation", new[] { "The Organisation field is required." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.11 - Reason required")]
        public async Task ShouldThrowValidationErrorWhenReasonRequiredAsync()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "Reason", new[] { "The Reason field is required." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.12 - PseudonymisedNumbers exceed 500")]
        public async Task ShouldThrowValidationErrorWhenPseudonymisedNumbersExceed500Async()
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "PseudonymisedNumbers", new[] { "No more than 500 PseudonymisedNumbers are allowed." } },
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: 501);

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Theory(DisplayName = "Validation - 2.13 - Required on all string fields - No whitespace characters on its own.")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        [InlineData("\r\n")]
        [InlineData("\v")]
        [InlineData("\f")]
        [InlineData(" \t\n\r\v\f")]
        public async Task ShouldThrowValidationErrorWhenFieldValuesIsInvalidAsync(string invalidText)
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "Reason", new[] { "The Reason field is required." } },
                { "RequestId", new[] { "The RequestId field is required." , "The Guid value cannot be null." } },
                { "Organisation", new[] { "The Organisation field is required." } },
                { "UserIdentifier", new[] { "The UserIdentifier field is required." } },
                { "PseudonymisedNumbers", new[] { "The PseudonymisedNumbers field is required." } }
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: randomCount);

            var randomRequest = new
            {
                RequestId = invalidText,
                UserIdentifier = invalidText,
                Organisation = invalidText,
                Reason = invalidText
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Trait("Category", "Validation - 2.14 - Validations Missing")]
        [Theory(DisplayName = "Validation - 2.14 - Validations Missing")]
        [MemberData(nameof(MissingItems))]
        public async Task ShouldThrowValidationErrorWhenValidationItemsMissingAsync(string missingItem)
        {
            // Given
            var expectedErrors = new Dictionary<string, string[]>
            {
                { "Reason", new[] { "The Reason field is required." } },
                { "RequestId", new[] { "The RequestId field is required.", "The Guid value cannot be null." } },
                { "Organisation", new[] { "The Organisation field is required." } },
                { "UserIdentifier", new[] { "The UserIdentifier field is required." } },
                { "PseudonymisedNumbers", new[] { "The PseudonymisedNumbers field is required." } }
            };

            if (expectedErrors.ContainsKey(missingItem))
            {
                expectedErrors.Remove(missingItem);
            }

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: GetRandomNumber());

            var randomRequest = new
            {
                RequestId = missingItem != "RequestId" ? null : randomReIdentificationRequest.RequestId.ToString(),
                PseudonymisedNumbers = missingItem != "PseudonymisedNumbers" ? null : randomReIdentificationRequest.PseudonymisedNumbers,
                UserIdentifier = missingItem != "UserIdentifier" ? null : randomReIdentificationRequest.UserIdentifier,
                Organisation = missingItem != "Organisation" ? null : randomReIdentificationRequest.Organisation,
                Reason = missingItem != "Reason" ? null : randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
            actualResponse.Should().ContainKey("errors");
            var actualErrors = JObject.Parse(actualResponse["errors"].ToString());
            actualErrors.Should().NotBeNull();
            actualErrors.Properties().Select(p => p.Name).Should().BeEquivalentTo(expectedErrors.Keys);

            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().ContainKey(expectedError.Key);
                var actualErrorMessages = actualErrors[expectedError.Key].ToObject<string[]>();
                actualErrorMessages.Should().BeEquivalentTo(expectedError.Value);
            }
        }

        [Fact(DisplayName = "Validation - 2.15 - Auth / API Key Invalid")]
        public async Task ShouldThrowValidationErrorWhenApiKeyIsInvalidAsync()
        {
            // Given
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(uriString: necsConfiguration.ApiUrl),
            };

            var expectedResponse = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                Title = "Authentication Error",
                Status = 401,
                Detail = "Invalid credentials or access denied.",
            };

            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: 5);

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            actualResponse.Should().ContainKey("type").WhoseValue.Should().Be(expectedResponse.Type);
            actualResponse.Should().ContainKey("title").WhoseValue.Should().Be(expectedResponse.Title);
            actualResponse.Should().ContainKey("status").WhoseValue.Should().Be(expectedResponse.Status);
        }


        [Fact(DisplayName = "Validation - 2.16 - Success with items that failed RowId validation")]
        public async Task ShouldThrowValidationErrorWhenRowIdIsInvalidAsync()
        {
            // Given
            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: 1);

            randomReIdentificationRequest.PseudonymisedNumbers.FirstOrDefault().RowNumber = "";

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<NecsReIdentificationResponse>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            actualResponse.Results.FirstOrDefault().RowNumber.Should()
                .BeEquivalentTo(randomReIdentificationRequest.PseudonymisedNumbers.FirstOrDefault().RowNumber);

            actualResponse.Results.FirstOrDefault().NhsNumber.Should().Be("0000000000");
            actualResponse.Results.FirstOrDefault().Message.Should().Be("RowId is required.");
        }

        [Fact(DisplayName = "Validation - 2.17 - Success with items that failed pseudo validation")]
        public async Task ShouldThrowValidationErrorWhenPseudoIsInvalidAsync()
        {
            // Given
            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: 5);

            randomReIdentificationRequest.PseudonymisedNumbers[0].Pseudo = "";
            randomReIdentificationRequest.PseudonymisedNumbers[1].Pseudo = "A";
            randomReIdentificationRequest.PseudonymisedNumbers[2].Pseudo = "1";
            randomReIdentificationRequest.PseudonymisedNumbers[3].Pseudo = "01234567890";
            randomReIdentificationRequest.PseudonymisedNumbers[4].Pseudo = "01234567898754321";
            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<NecsReIdentificationResponse>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            actualResponse.Results.FirstOrDefault().RowNumber.Should()
                .BeEquivalentTo(randomReIdentificationRequest.PseudonymisedNumbers.FirstOrDefault().RowNumber);

            for (int i = 0; i < actualResponse.Results.Count; i++)
            {
                var input = randomReIdentificationRequest.PseudonymisedNumbers[i];
                var item = actualResponse.Results[i];
                item.RowNumber.Should().BeEquivalentTo(input.RowNumber);
                item.NhsNumber.Should().Be("0000000000");
                
                switch(i)
                {
                    case 0: item.Message.Should().Be("Pseudo number cannot be empty.");break;
                    case 1: item.Message.Should().Be("Pseudo must be numeric."); break;
                    case 2: item.Message.Should().Be("Pseudo could not be matched with a NHS number."); break;
                    case 3: item.Message.Should().Be("Pseudo could not be matched with a NHS number."); break;
                    case 4: item.Message.Should().Be("Pseudo must not exceed 15 digits."); break;

                }
            }
        }
        

        [Fact(DisplayName = "Validation - 2.18 - Unmatched pseudo validation")]
        public async Task ShouldThrowValidationErrorWhenPseudoNotMatchedAsync()
        {
            // Given
            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: 1);

            randomReIdentificationRequest.PseudonymisedNumbers.FirstOrDefault().Pseudo = "2202370224";

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<NecsReIdentificationResponse>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            actualResponse.Results.FirstOrDefault().RowNumber.Should()
                .BeEquivalentTo(randomReIdentificationRequest.PseudonymisedNumbers.FirstOrDefault().RowNumber);

            actualResponse.Results.FirstOrDefault().NhsNumber
                .Should().Be("0000000000");

            actualResponse.Results.FirstOrDefault().Message
                .Should().Be("Pseudo could not be matched with a NHS number.");
        }

        [Fact(DisplayName = "Validation - 2.19 - Success")]
        public async Task ShouldWithNoErrorsIfAllValidationIsCorrectAsync()
        {
            // Given
            int randomCount = GetRandomNumber();

            NecsReIdentificationRequest randomReIdentificationRequest =
                CreateRandomNecsReIdentificationRequest(count: GetRandomNumber());

            var randomRequest = new
            {
                randomReIdentificationRequest.RequestId,
                randomReIdentificationRequest.PseudonymisedNumbers,
                randomReIdentificationRequest.UserIdentifier,
                randomReIdentificationRequest.Organisation,
                randomReIdentificationRequest.Reason
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(randomRequest),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await httpClient.PostAsync(necsConfiguration.ApiUrl, jsonContent);
            string actualContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<NecsReIdentificationResponse>(actualContent);

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            actualResponse.Results.FirstOrDefault().RowNumber.Should()
                .BeEquivalentTo(randomReIdentificationRequest.PseudonymisedNumbers.FirstOrDefault().RowNumber);

            for (int i = 0; i < actualResponse.Results.Count; i++)
            {
                var input = randomReIdentificationRequest.PseudonymisedNumbers[i];
                var item = actualResponse.Results[i];
                item.RowNumber.Should().BeEquivalentTo(input.RowNumber);
                item.NhsNumber.Should().NotBeNullOrWhiteSpace();
                item.Message.Should().Be("OK");
            }
        }
    }
}
