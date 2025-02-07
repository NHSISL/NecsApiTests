// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Newtonsoft.Json;

namespace NecsApi.Tests.Integrations.Models.NECS.Responses
{
    public class NecsReidentifiedItem
    {
        [JsonProperty("rowId")]
        public string RowNumber { get; set; }

        [JsonProperty("nhsNumber")]
        public string NhsNumber { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
