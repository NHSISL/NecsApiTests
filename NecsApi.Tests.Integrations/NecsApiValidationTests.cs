// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using NecsApi.Tests.Integrations.Models;
using NecsApi.Tests.Integrations.Models.NECS.Requests;
using RESTFulSense.Clients;
using Tynamix.ObjectFiller;
using Xunit.Abstractions;

namespace NecsApi.Tests.Integrations
{
    public partial class NecsApiValidationTests
    {
        private readonly IConfiguration configuration;
        private readonly NecsReIdentificationConfigurations necsConfiguration;
        private readonly HttpClient httpClient;
        private readonly IRESTFulApiFactoryClient apiClient;
        private readonly ITestOutputHelper output;

        public NecsApiValidationTests(ITestOutputHelper output)
        {
            this.output = output;

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = configurationBuilder.Build();

            necsConfiguration = configuration
                .GetSection("necsReIdentificationConfigurations").Get<NecsReIdentificationConfigurations>();

            httpClient = new HttpClient()
            {
                BaseAddress = new Uri(uriString: necsConfiguration.ApiUrl),
            };

            httpClient.DefaultRequestHeaders.Add("X-API-KEY", necsConfiguration.ApiKey);
            apiClient = new RESTFulApiFactoryClient(httpClient);
        }

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static string GenerateRandom10DigitNumber()
        {
            Random random = new Random();
            var randomNumber = random.Next(1923366278, 1932457186).ToString();

            return randomNumber;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static NecsReIdentificationRequest CreateRandomNecsReIdentificationRequest(int count) =>
            CreateNecsReIdentificationRequestFiller(count).Create();

        private static Filler<NecsReIdentificationRequest> CreateNecsReIdentificationRequestFiller(int count)
        {
            var filler = new Filler<NecsReIdentificationRequest>();

            filler
                .Setup()
                .OnProperty(request => request.PseudonymisedNumbers).Use(CreateRandomNecsPseudonymisedItems(count));
            return filler;
        }

        private static List<NecsPseudonymisedItem> CreateRandomNecsPseudonymisedItems(int count)
        {
            return CreateNecsPseudonymisedItemFiller()
                .Create(count)
                    .ToList();
        }

        private static Filler<NecsPseudonymisedItem> CreateNecsPseudonymisedItemFiller()
        {
            var filler = new Filler<NecsPseudonymisedItem>();

            filler.Setup().OnProperty(item => item.Pseudo).Use(GenerateRandom10DigitNumber());

            return filler;
        }

        public static TheoryData<string> MissingItems()
        {
            return new TheoryData<string>
            {
                "RequestId",
                "PseudonymisedNumbers",
                "UserIdentifier",
                "Organisation",
                "Reason"
            };
        }
    }
}