﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Skoruba.AuditLogging.Events;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Dtos.Configuration;

namespace Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Events.ApiResource
{
    public class ApiResourcePropertyDeletedEvent : AuditEvent
    {
        public ApiResourcePropertyDeletedEvent(ApiResourcePropertiesDto apiResourceProperty)
        {
            ApiResourceProperty = apiResourceProperty;
        }

        public ApiResourcePropertiesDto ApiResourceProperty { get; set; }
    }
}