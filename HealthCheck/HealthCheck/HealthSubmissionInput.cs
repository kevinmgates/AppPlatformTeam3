using System.Collections.Generic;

namespace HealthCheck
{
    public class HealthSubmissionInput
    {
        public bool IsFeelingWell { get; set; }

        public IList<string> Symptoms { get; set; }
    }
}

