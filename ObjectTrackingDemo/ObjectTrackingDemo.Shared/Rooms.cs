using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectTrackingDemo
{
    class Rooms
    {
        public String id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }
        [JsonProperty(PropertyName = "player1")]
        public string player1 { get; set; }
        [JsonProperty(PropertyName = "player1color")]
        public string player1color { get; set; }
        [JsonProperty(PropertyName = "player2")]
        public string player2 { get; set; }
        [JsonProperty(PropertyName = "player2color")]
        public string player2color { get; set; }
        [JsonProperty(PropertyName = "player3")]
        public string player3 { get; set; }
        [JsonProperty(PropertyName = "player3color")]
        public string player3color { get; set; }
        [JsonProperty(PropertyName = "player4")]
        public string player4 { get; set; }
        [JsonProperty(PropertyName = "player4color")]
        public string player4color { get; set; }

    }
}
