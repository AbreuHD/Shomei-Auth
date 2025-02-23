using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.DTOs.Geolocation
{
    internal class LocationInfoDto
    {
        [JsonProperty("location")]
        public required GeoLocationInfoDto Location { get; set; }
    }

    internal class GeoLocationInfoDto
    {
        [JsonProperty("country")]
        public required string Country { get; set; }
        [JsonProperty("city")]
        public required string City { get; set; }
    }
}
