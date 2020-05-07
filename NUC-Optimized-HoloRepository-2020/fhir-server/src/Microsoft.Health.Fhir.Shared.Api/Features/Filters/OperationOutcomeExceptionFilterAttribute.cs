﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Transactions;
using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Health.Abstractions.Exceptions;
using Microsoft.Health.Fhir.Api.Features.ActionResults;
using Microsoft.Health.Fhir.Api.Features.Audit;
using Microsoft.Health.Fhir.Api.Features.Bundle;
using Microsoft.Health.Fhir.Api.Features.Exceptions;
using Microsoft.Health.Fhir.Core.Exceptions;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Features.Context;
using Microsoft.Health.Fhir.Core.Features.Operations;
using Microsoft.Health.Fhir.Core.Features.Persistence;
using Microsoft.Health.Fhir.Core.Features.Search;
using Microsoft.Health.Fhir.Core.Features.Validation;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Health.Fhir.Api.Features.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class OperationOutcomeExceptionFilterAttribute : ActionFilterAttribute
    {
        private const string RetryAfterHeaderName = "x-ms-retry-after-ms";
        private const string ValidateController = "Validate";

        private readonly IFhirRequestContextAccessor _fhirRequestContextAccessor;

        public OperationOutcomeExceptionFilterAttribute(IFhirRequestContextAccessor fhirRequestContextAccessor)
        {
            EnsureArg.IsNotNull(fhirRequestContextAccessor, nameof(fhirRequestContextAccessor));

            _fhirRequestContextAccessor = fhirRequestContextAccessor;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            if (context?.Exception == null)
            {
                return;
            }

            if (context.Exception is FhirException fhirException)
            {
                var operationOutcomeResult = new OperationOutcomeResult(
                    new OperationOutcome
                    {
                        Id = _fhirRequestContextAccessor.FhirRequestContext.CorrelationId,
                        Issue = fhirException.Issues.Select(x => x.ToPoco()).ToList(),
                    }, HttpStatusCode.BadRequest);

                switch (fhirException)
                {
                    case UnauthorizedFhirActionException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.Forbidden;
                        break;

                    case ResourceGoneException resourceGoneException:
                        operationOutcomeResult.StatusCode = HttpStatusCode.Gone;
                        if (!string.IsNullOrEmpty(resourceGoneException.DeletedResource?.VersionId))
                        {
                            operationOutcomeResult.Headers.Add(HeaderNames.ETag, WeakETag.FromVersionId(resourceGoneException.DeletedResource.VersionId).ToString());
                        }

                        break;
                    case ResourceNotFoundException _:
                    case JobNotFoundException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.NotFound;
                        break;
                    case MethodNotAllowedException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.MethodNotAllowed;
                        break;
                    case OpenIdConfigurationException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.ServiceUnavailable;
                        break;
                    case ResourceNotValidException _:
                        if (context.ActionDescriptor is ControllerActionDescriptor controllerDescriptor)
                        {
                            if (controllerDescriptor.ControllerName.Equals(ValidateController, StringComparison.OrdinalIgnoreCase))
                            {
                                operationOutcomeResult.StatusCode = HttpStatusCode.OK;
                                break;
                            }
                        }

                        operationOutcomeResult.StatusCode = HttpStatusCode.BadRequest;
                        break;
                    case BadRequestException _:
                    case RequestNotValidException _:
                    case BundleEntryLimitExceededException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.BadRequest;
                        break;
                    case ResourceConflictException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.Conflict;
                        break;
                    case PreconditionFailedException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.PreconditionFailed;
                        break;
                    case InvalidSearchOperationException _:
                    case SearchOperationNotSupportedException _:
                    case CustomerManagedKeyInaccessibleException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.Forbidden;
                        break;
                    case UnsupportedConfigurationException _:
                    case AuditException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.InternalServerError;
                        break;
                    case AuditHeaderException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.RequestHeaderFieldsTooLarge;
                        break;
                    case OperationFailedException ofe:
                        operationOutcomeResult.StatusCode = ofe.ResponseStatusCode;
                        break;
                    case OperationNotImplementedException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.MethodNotAllowed;
                        break;
                    case NotAcceptableException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.NotAcceptable;
                        break;
                    case RequestEntityTooLargeException _:
                        operationOutcomeResult.StatusCode = HttpStatusCode.RequestEntityTooLarge;
                        break;
                    case FhirTransactionFailedException fhirTransactionFailedException:
                        operationOutcomeResult.StatusCode = fhirTransactionFailedException.ResponseStatusCode;
                        break;
                }

                context.Result = operationOutcomeResult;
                context.ExceptionHandled = true;
            }
            else if (context.Exception is MicrosoftHealthException microsoftHealthException)
            {
                OperationOutcomeResult healthExceptionResult;

                switch (microsoftHealthException)
                {
                    case RequestRateExceededException ex:
                        healthExceptionResult = CreateOperationOutcomeResult(ex.Message, OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueType.Throttled, HttpStatusCode.TooManyRequests);

                        if (ex.RetryAfter != null)
                        {
                            healthExceptionResult.Headers.Add(
                                RetryAfterHeaderName,
                                ex.RetryAfter.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                        }

                        break;
                    case UnsupportedMediaTypeException unsupportedMediaTypeException:
                        healthExceptionResult = CreateOperationOutcomeResult(unsupportedMediaTypeException.Message, OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueType.NotSupported, HttpStatusCode.UnsupportedMediaType);
                        break;
                    case ServiceUnavailableException serviceUnavailableException:
                        healthExceptionResult = CreateOperationOutcomeResult(serviceUnavailableException.Message, OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueType.Processing, HttpStatusCode.ServiceUnavailable);
                        break;
                    case TransactionFailedException transactionFailedException:
                        healthExceptionResult = CreateOperationOutcomeResult(transactionFailedException.Message, OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueType.Processing, HttpStatusCode.InternalServerError);
                        break;
                    default:
                        healthExceptionResult = new OperationOutcomeResult(
                            new OperationOutcome
                            {
                                Id = _fhirRequestContextAccessor.FhirRequestContext.CorrelationId,
                            }, HttpStatusCode.InternalServerError);
                        break;
                }

                context.Result = healthExceptionResult;
                context.ExceptionHandled = true;
            }
            else
            {
                switch (context.Exception)
                {
                    case FormatException ex:
                        context.Result = CreateOperationOutcomeResult(ex.Message, OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueType.Structure, HttpStatusCode.BadRequest);
                        context.ExceptionHandled = true;

                        break;
                    case ArgumentException ex:
                        context.Result = CreateOperationOutcomeResult(ex.Message, OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueType.Invalid, HttpStatusCode.BadRequest);
                        context.ExceptionHandled = true;

                        break;
                }
            }
        }

        private OperationOutcomeResult CreateOperationOutcomeResult(string message, OperationOutcome.IssueSeverity issueSeverity, OperationOutcome.IssueType issueType, HttpStatusCode httpStatusCode)
        {
            return new OperationOutcomeResult(
                new OperationOutcome
                {
                    Id = _fhirRequestContextAccessor.FhirRequestContext.CorrelationId,
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = issueSeverity,
                            Code = issueType,
                            Diagnostics = message,
                        },
                    },
                }, httpStatusCode);
        }
    }
}
