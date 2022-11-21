﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Skoruba.Duende.IdentityServer.Admin.UI.Helpers
{
    public static class DateTimeHelpers
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static double GetEpochTicks(this DateTimeOffset dateTime)
        {
            return dateTime.Subtract(Epoch).TotalMilliseconds;
        }
    }
}