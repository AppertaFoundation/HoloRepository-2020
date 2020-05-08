﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Model.Primitives;

namespace Microsoft.Health.Fhir.Core.Features.Security
{
    public static class Constants
    {
        private static readonly Coding RestfulSecurityServiceOAuthCodeableConcept = new Coding("http://hl7.org/fhir/ValueSet/restful-security-service", "OAuth");
        public const string SmartOAuthUriExtension = "http://fhir-registry.smarthealthit.org/StructureDefinition/oauth-uris";
        public const string SmartOAuthUriExtensionToken = "token";
        public const string SmartOAuthUriExtensionAuthorize = "authorize";

        public static ref readonly Coding RestfulSecurityServiceOAuth => ref RestfulSecurityServiceOAuthCodeableConcept;
    }
}
