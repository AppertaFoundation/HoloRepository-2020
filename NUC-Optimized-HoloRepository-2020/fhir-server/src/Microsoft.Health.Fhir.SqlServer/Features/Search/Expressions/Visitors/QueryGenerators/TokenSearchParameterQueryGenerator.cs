﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using Microsoft.Health.Fhir.Core.Features.Search.Expressions;
using Microsoft.Health.Fhir.SqlServer.Features.Schema.Model;
using Microsoft.Health.SqlServer.Features.Schema.Model;

namespace Microsoft.Health.Fhir.SqlServer.Features.Search.Expressions.Visitors.QueryGenerators
{
    internal class TokenSearchParameterQueryGenerator : NormalizedSearchParameterQueryGenerator
    {
        public static readonly TokenSearchParameterQueryGenerator Instance = new TokenSearchParameterQueryGenerator();

        public override Table Table => VLatest.TokenSearchParam;

        public override SearchParameterQueryGeneratorContext VisitMissingField(MissingFieldExpression expression, SearchParameterQueryGeneratorContext context)
        {
            return VisitMissingFieldImpl(expression, context, FieldName.TokenSystem, VLatest.TokenSearchParam.SystemId);
        }

        public override SearchParameterQueryGeneratorContext VisitString(StringExpression expression, SearchParameterQueryGeneratorContext context)
        {
            Debug.Assert(expression.StringOperator == StringOperator.Equals, "Only equals is supported");

            switch (expression.FieldName)
            {
                case FieldName.TokenSystem:
                    if (context.Model.TryGetSystemId(expression.Value, out var systemId))
                    {
                        return VisitSimpleBinary(BinaryOperator.Equal, context, VLatest.TokenSearchParam.SystemId, expression.ComponentIndex, systemId);
                    }

                    AppendColumnName(context, VLatest.TokenSearchParam.SystemId, expression)
                        .Append(" IN (SELECT ")
                        .Append(VLatest.System.SystemId, null)
                        .Append(" FROM ").Append(VLatest.System)
                        .Append(" WHERE ")
                        .Append(VLatest.System.Value, null)
                        .Append(" = ")
                        .Append(context.Parameters.AddParameter(VLatest.System.Value, expression.Value))
                        .Append(")");

                    return context;

                case FieldName.TokenCode:
                    VisitSimpleString(expression, context, VLatest.TokenSearchParam.Code, expression.Value);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return context;
        }
    }
}
