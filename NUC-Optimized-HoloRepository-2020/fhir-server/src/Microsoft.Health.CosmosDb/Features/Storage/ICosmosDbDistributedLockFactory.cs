﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.Documents;

namespace Microsoft.Health.CosmosDb.Features.Storage
{
    public interface ICosmosDbDistributedLockFactory
    {
        ICosmosDbDistributedLock Create(Uri collectionUri, string lockId);

        ICosmosDbDistributedLock Create(IDocumentClient client, Uri collectionUri, string lockId);
    }
}
