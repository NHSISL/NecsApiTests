// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NecsApi.Tests.Integrations.Models.NECS.Requests
{
    public class NecsReIdentificationRequest
    {
        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("pseudonymisedNumbers")]
        public List<NecsPseudonymisedItem> PseudonymisedNumbers { get; set; } = new List<NecsPseudonymisedItem>();

        [JsonProperty("userIdentifier")]
        public string UserIdentifier { get; set; }

        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
