﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Model;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Api.Controllers;
using Microsoft.Health.Fhir.Core.Configs;
using Microsoft.Health.Fhir.Core.Exceptions;
using Microsoft.Health.Fhir.Core.Features.Context;
using Microsoft.Health.Fhir.Core.Features.Operations;
using Microsoft.Health.Fhir.Core.Features.Routing;
using NSubstitute;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Api.UnitTests.Controllers
{
    public class ExportControllerTests
    {
        private ExportController _exportEnabledController;
        private IMediator _mediator = Substitute.For<IMediator>();
        private IFhirRequestContextAccessor _fhirRequestContextAccessor = Substitute.For<IFhirRequestContextAccessor>();
        private IUrlResolver _urlResolver = Substitute.For<IUrlResolver>();

        public ExportControllerTests()
        {
            _exportEnabledController = GetController(new ExportJobConfiguration() { Enabled = true });
        }

        [Fact]
        public async Task GivenAnExportRequest_WhenDisabled_ThenRequestNotValidExceptionShouldBeThrown()
        {
            var exportController = GetController(new ExportJobConfiguration() { Enabled = false });

            await Assert.ThrowsAsync<RequestNotValidException>(() => exportController.Export(since: null));
        }

        [Fact]
        public void GivenAnExportByResourceTypeRequest_WhenResourceTypeIsNotPatient_ThenRequestNotValidExceptionShouldBeThrown()
        {
            Assert.Throws<RequestNotValidException>(() => _exportEnabledController.ExportResourceType(ResourceType.Observation.ToString()));
        }

        [Fact]
        public void GivenAnExportByResourceTypeRequest_WhenResourceTypeIsPatient_ThenOperationNotImplementedExceptionShouldBeThrown()
        {
            Assert.Throws<OperationNotImplementedException>(() => _exportEnabledController.ExportResourceType(ResourceType.Patient.ToString()));
        }

        [Fact]
        public void GivenAnExportResourceTypeIdRequest_WhenResourceTypeIsNotGroup_ThenRequestNotValidExceptionShouldBeThrown()
        {
            Assert.Throws<RequestNotValidException>(() => _exportEnabledController.ExportResourceTypeById(ResourceType.Patient.ToString(), "id"));
        }

        [Fact]
        public void GivenAnExportByResourceTypeIdRequest_WhenResourceTypeIsGroup_ThenOperationNotImplementedExceptionShouldBeThrown()
        {
            Assert.Throws<OperationNotImplementedException>(() => _exportEnabledController.ExportResourceTypeById(ResourceType.Group.ToString(), "id"));
        }

        private ExportController GetController(ExportJobConfiguration exportConfig)
        {
            var operationConfig = new OperationsConfiguration()
            {
                Export = exportConfig,
            };

            IOptions<OperationsConfiguration> optionsOperationConfiguration = Substitute.For<IOptions<OperationsConfiguration>>();
            optionsOperationConfiguration.Value.Returns(operationConfig);

            return new ExportController(
                _mediator,
                _fhirRequestContextAccessor,
                _urlResolver,
                optionsOperationConfiguration,
                NullLogger<ExportController>.Instance);
        }
    }
}
