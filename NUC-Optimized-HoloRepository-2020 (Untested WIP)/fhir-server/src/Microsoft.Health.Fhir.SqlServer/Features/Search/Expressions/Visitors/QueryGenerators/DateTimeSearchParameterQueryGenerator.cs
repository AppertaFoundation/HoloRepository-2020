﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Core.Features.Search.Expressions;
using Microsoft.Health.Fhir.SqlServer.Features.Schema.Model;
using Microsoft.Health.SqlServer.Features.Schema.Model;

namespace Microsoft.Health.Fhir.SqlServer.Features.Search.Expressions.Visitors.QueryGenerators
{
    internal class DateTimeSearchParameterQueryGenerator : NormalizedSearchParameterQueryGenerator
    {
        public static readonly DateTimeSearchParameterQueryGenerator Instance = new DateTimeSearchParameterQueryGenerator();

        public override Table Table => VLatest.DateTimeSearchParam;

        public override SearchParameterQueryGeneratorContext VisitBinary(BinaryExpression expression, SearchParameterQueryGeneratorContext context)
        {
            DateTime2Column column;
            switch (expression.FieldName)
            {
                case FieldName.DateTimeStart:
                    column = VLatest.DateTimeSearchParam.StartDateTime;
                    break;
                case FieldName.DateTimeEnd:
                    column = VLatest.DateTimeSearchParam.EndDateTime;
                    break;
                case SqlFieldName.DateTimeIsLongerThanADay:
                    // we don't want to use a parameter here because we want the query plan to use the filtered index based on this field
                    AppendColumnName(context, VLatest.DateTimeSearchParam.IsLongerThanADay, expression).Append(" = ").Append((bool)expression.Value ? '1' : '0');
                    return context;
                default:
                    throw new ArgumentOutOfRangeException(expression.FieldName.ToString());
            }

            return VisitSimpleBinary(expression.BinaryOperator, context, column, expression.ComponentIndex, ((DateTimeOffset)expression.Value).UtcDateTime);
        }
    }
}
