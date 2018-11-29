﻿// <copyright file="ForgotPasswordOptions.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Okta.Auth.Sdk.Models;

namespace Okta.Auth.Sdk
{
    public class ForgotPasswordOptions
    {
        public string UserName { get; set; }

        public string RelayState { get; set; }

        public FactorType FactorType { get; set; }
    }
}
