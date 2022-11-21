﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Skoruba.Duende.IdentityServer.Admin.UI.Configuration
{
    public class AuditLoggingConfiguration
    {
        public string Source { get; set; }

        public string SubjectIdentifierClaim { get; set; }

        public string SubjectNameClaim { get; set; }

        public bool IncludeFormVariables { get; set; }
    }
}