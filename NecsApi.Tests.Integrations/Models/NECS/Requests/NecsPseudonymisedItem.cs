// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Newtonsoft.Json;

namespace NecsApi.Tests.Integrations.Models.NECS.Requests
{
    public class NecsPseudonymisedItem
    {
        [JsonProperty("rowId")]
        public string RowNumber { get; set; }

        [JsonProperty("pseudo")]
        public string Pseudo { get; set; }
    }
}
