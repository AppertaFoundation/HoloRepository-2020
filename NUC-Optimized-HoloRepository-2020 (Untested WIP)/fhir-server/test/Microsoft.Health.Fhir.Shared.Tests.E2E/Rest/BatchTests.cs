﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Tests.Common;
using Microsoft.Health.Fhir.Tests.Common.FixtureParameters;
using Microsoft.Health.Fhir.Tests.E2E.Common;
using Xunit;
using static Hl7.Fhir.Model.OperationOutcome;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Tests.E2E.Rest
{
    [Trait(Traits.Category, Categories.Batch)]
    [HttpIntegrationFixtureArgumentSets(DataStore.All, Format.All)]
    public class BatchTests : IClassFixture<HttpIntegrationTestFixture>
    {
        private readonly Dictionary<HttpStatusCode, string> statusCodeMap = new Dictionary<HttpStatusCode, string>()
        {
            { HttpStatusCode.NoContent, "204" },
            { HttpStatusCode.NotFound, "404" },
            { HttpStatusCode.OK, "200" },
            { HttpStatusCode.PreconditionFailed, "412" },
            { HttpStatusCode.Created, "201" },
            { HttpStatusCode.Forbidden, "403" },
        };

        public BatchTests(HttpIntegrationTestFixture fixture)
        {
            Client = fixture.FhirClient;
        }

        protected FhirClient Client { get; set; }

        [Fact]
        [Trait(Traits.Priority, Priority.One)]
        public async Task GivenAValidBundle_WhenSubmittingABatch_ThenSuccessIsReturnedForBatchAndExpectedStatusCodesPerRequests()
        {
            var requestBundle = Samples.GetDefaultBatch().ToPoco<Bundle>();

            await Client.UpdateAsync(requestBundle.Entry[2].Resource as Patient);

            FhirResponse<Bundle> fhirResponse = await Client.PostBundleAsync(requestBundle);
            Assert.NotNull(fhirResponse);
            Assert.Equal(HttpStatusCode.OK, fhirResponse.StatusCode);

            Bundle resource = fhirResponse.Resource;

            Assert.Equal("201", resource.Entry[0].Response.Status);
            Assert.Equal("201", resource.Entry[1].Response.Status);
            Assert.Equal("200", resource.Entry[2].Response.Status);
            Assert.Equal("201", resource.Entry[3].Response.Status);
            Assert.Equal(((int)Constants.IfMatchFailureStatus).ToString(), resource.Entry[4].Response.Status);
            Assert.Equal("204", resource.Entry[5].Response.Status);
            ValidateOperationOutcome(resource.Entry[6].Response.Status, resource.Entry[6].Response.Outcome as OperationOutcome, statusCodeMap[HttpStatusCode.NotFound], $"The route for \"/{requestBundle.Entry[6].Request.Url}\" was not found.", IssueType.NotFound);
            ValidateOperationOutcome(resource.Entry[7].Response.Status, resource.Entry[7].Response.Outcome as OperationOutcome, statusCodeMap[HttpStatusCode.NotFound], "The route for \"/ValueSet/$lookup\" was not found.", IssueType.NotFound);
            Assert.Equal("200", resource.Entry[8].Response.Status);
            ValidateOperationOutcome(resource.Entry[9].Response.Status, resource.Entry[9].Response.Outcome as OperationOutcome, statusCodeMap[HttpStatusCode.NotFound], "Resource type 'Patient' with id '12334' couldn't be found.", IssueType.NotFound);
        }

        [Fact]
        [Trait(Traits.Priority, Priority.One)]
        [Trait(Traits.Category, Categories.Authorization)]
        public async Task GivenAValidBundleWithReadonlyUser_WhenSubmittingABatch_ThenForbiddenAndOutcomeIsReturned()
        {
            FhirClient tempClient = Client.CreateClientForUser(TestUsers.ReadOnlyUser, TestApplications.NativeClient);
            Bundle requestBundle = Samples.GetDefaultBatch().ToPoco<Bundle>();

            FhirResponse<Bundle> fhirResponse = await tempClient.PostBundleAsync(requestBundle);

            Assert.NotNull(fhirResponse);
            Assert.Equal(HttpStatusCode.OK, fhirResponse.StatusCode);

            for (int i = 0; i < 6; i++)
            {
                var entry = fhirResponse.Resource.Entry[i];
                var operationOutcome = entry.Response.Outcome as OperationOutcome;
                ValidateOperationOutcome(entry.Response.Status, operationOutcome, statusCodeMap[HttpStatusCode.Forbidden], "Authorization failed.", IssueType.Forbidden);
            }

            var resourceEntry = fhirResponse.Resource.Entry[6];
            ValidateOperationOutcome(resourceEntry.Response.Status, resourceEntry.Response.Outcome as OperationOutcome, statusCodeMap[HttpStatusCode.NotFound], $"The route for \"/{requestBundle.Entry[6].Request.Url}\" was not found.", IssueType.NotFound);
            resourceEntry = fhirResponse.Resource.Entry[7];
            ValidateOperationOutcome(resourceEntry.Response.Status, resourceEntry.Response.Outcome as OperationOutcome, statusCodeMap[HttpStatusCode.NotFound], "The route for \"/ValueSet/$lookup\" was not found.", IssueType.NotFound);
            resourceEntry = fhirResponse.Resource.Entry[8];
            Assert.Equal("200", resourceEntry.Response.Status);
            Assert.Null(resourceEntry.Response.Outcome);
            resourceEntry = fhirResponse.Resource.Entry[9];
            ValidateOperationOutcome(resourceEntry.Response.Status, resourceEntry.Response.Outcome as OperationOutcome, statusCodeMap[HttpStatusCode.NotFound], "Resource type 'Patient' with id '12334' couldn't be found.", IssueType.NotFound);
        }

        [Fact]
        [Trait(Traits.Priority, Priority.One)]
        public async Task GivenANonBundleResource_WhenSubmittingABatch_ThenBadRequestIsReturned()
        {
            FhirException ex = await Assert.ThrowsAsync<FhirException>(() => Client.PostBundleAsync(Samples.GetDefaultObservation().ToPoco<Observation>()));

            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        }

        [Fact]
        [Trait(Traits.Priority, Priority.One)]
        public async Task GivenBundleTypeIsMissing_WhenSubmittingABundle_ThenMethodNotAllowedExceptionIsReturned()
        {
            FhirException ex = await Assert.ThrowsAsync<FhirException>(() => Client.PostBundleAsync(Samples.GetJsonSample("Bundle-TypeMissing").ToPoco<Bundle>()));
            ValidateOperationOutcome(ex.StatusCode.ToString(), ex.OperationOutcome, "MethodNotAllowed", "Bundle type is not present. Possible values are: transaction or batch", IssueType.Forbidden);
        }

        private void ValidateOperationOutcome(string actualStatusCode, OperationOutcome operationOutcome, string expectedStatusCode, string expectedDiagnostics, IssueType expectedIssueType)
        {
            Assert.Equal(expectedStatusCode, actualStatusCode);
            Assert.NotNull(operationOutcome);
            Assert.Single(operationOutcome.Issue);

            var issue = operationOutcome.Issue.First();

            Assert.Equal(IssueSeverity.Error, issue.Severity.Value);
            Assert.Equal(expectedIssueType, issue.Code);
            Assert.Equal(expectedDiagnostics, issue.Diagnostics);
        }
    }
}
