// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NecsApi.Tests.Integrations.Models.NECS.Responses
{
    public class NecsReIdentificationResponse
    {
        [JsonProperty("uniqueRequestId")]
        public Guid UniqueRequestId { get; set; }

        [JsonProperty("results")]
        public List<NecsReidentifiedItem> Results { get; set; } = new List<NecsReidentifiedItem>();

        [JsonProperty("elapsedTime")]
        public int ElapsedTime { get; set; }

        [JsonProperty("processedCount")]
        public int ProcessedCount { get; set; }
    }
}
