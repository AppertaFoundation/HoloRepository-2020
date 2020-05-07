﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Core.Features;

namespace Microsoft.Health.Fhir.Api.Features.Formatters
{
    public static class HttpContextExtensions
    {
        public static SummaryType GetSummaryType(this HttpContext context, ILogger logger)
        {
            var query = context.Request.Query[KnownQueryParameterNames.Summary].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(query) && context.Response.StatusCode == (int)HttpStatusCode.OK)
            {
                try
                {
                    var summary = Enum.Parse<SummaryType>(query, true);

                    logger.LogDebug("Changing response summary to '{0}'", summary);

                    return summary;
                }
                catch (Exception ex)
                {
                    // SearchOptionsFactory validates the _summary option before this method is called from the Formatters so this _shouldn't_ be called
                    logger.LogWarning(ex, ex.Message);
                    throw;
                }
            }

            return SummaryType.False;
        }

        public static bool GetIsPretty(this HttpContext context)
        {
            var query = context.Request.Query[KnownQueryParameterNames.Pretty].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(query))
            {
                if (!bool.TryParse(query, out bool isPretty))
                {
                    // ContentTypeService validates the _pretty parameter. This is reached if other errors are encountered when parsing the query string.
                    isPretty = default;
                }

                return isPretty;
            }

            return false;
        }

        public static void AllowSynchronousIO(this HttpContext context)
        {
            var bodyControlFeature = context.Features.Get<IHttpBodyControlFeature>();
            if (bodyControlFeature != null)
            {
                bodyControlFeature.AllowSynchronousIO = true;
            }
        }
    }
}
