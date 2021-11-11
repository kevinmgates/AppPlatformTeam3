using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HealthCheck
{
    public class HealthSubmission
    {
        [JsonProperty("id")]
        public string SubmissionId { get; set; }

        [JsonProperty("patientId")]
        public string PatientId { get; set; }

        public DateTime SubmissionDate { get; set; }

        public bool IsFeelingWell { get; set; }

        public IList<string> Symptoms { get; set; }
    }
}

