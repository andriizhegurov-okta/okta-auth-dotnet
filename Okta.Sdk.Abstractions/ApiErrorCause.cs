﻿// <copyright file="ApiErrorCause.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

namespace Okta.Sdk.Abstractions
{
    /// <inheritdoc/>
    public sealed class ApiErrorCause : BaseResource, IApiErrorCause
    {
        /// <inheritdoc/>
        public string ErrorSummary => GetStringProperty("errorSummary");
    }
}
