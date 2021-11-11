using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace HealthCheck
{

    public static class Function1
    {
        [FunctionName(nameof(CreateHealthCheck))]
        [OpenApiOperation(operationId: nameof(CreateHealthCheck), tags: new[] { "HealthCheck Write" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(HealthSubmissionInput), Required = true, Description = "The Health Submission Input")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> CreateHealthCheck(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "healthcheck")] HealthSubmissionInput healthSubmissionInput,
            [CosmosDB(databaseName: "HealthCheckDB", containerName: "HealthChecks", Connection = "CosmosDBConnection")] IAsyncCollector<HealthSubmission> asyncCollector,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var healthSubmission = new HealthSubmission
            {
                PatientId = "0",
                SubmissionDate = DateTime.UtcNow,
                SubmissionId = Guid.NewGuid().ToString(),
                IsFeelingWell = healthSubmissionInput.IsFeelingWell,
                Symptoms = healthSubmissionInput.Symptoms
            };

            await asyncCollector.AddAsync(healthSubmission);

            return new OkResult();
        }

        [FunctionName(nameof(GetHealthCheckByPatient))]
        [OpenApiOperation(operationId: nameof(GetHealthCheckByPatient), tags: new[] { "HealthCheck Query" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> GetHealthCheckByPatient(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "healthcheck")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName(nameof(GetHealthCheckBySubmission))]
        [OpenApiOperation(operationId: nameof(GetHealthCheckBySubmission), tags: new[] { "HealthCheck Query" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "submissionId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The Submission Id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> GetHealthCheckBySubmission(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "healthcheck/{submissionId}")] HttpRequest req,
            ILogger log,
            [CosmosDB(databaseName: "HealthCheckDB", containerName: "HealthChecks", Connection = "CosmosDBConnection", Id = "{submissionId}", PartitionKey = "0")] HealthSubmission healthSubmission,
            string submissionId)
        {
            return new OkObjectResult(healthSubmission);
        }
    }
}

