﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Extensions.Common;

namespace Skoruba.Duende.IdentityServer.Admin.EntityFramework.Repositories.Interfaces
{
    public interface IIdentityResourceRepository
    {
        Task<PagedList<IdentityResource>> GetIdentityResourcesAsync(string search, int page = 1, int pageSize = 10);

        Task<IdentityResource> GetIdentityResourceAsync(int identityResourceId);

        Task<bool> CanInsertIdentityResourceAsync(IdentityResource identityResource);

        Task<int> AddIdentityResourceAsync(IdentityResource identityResource);

        Task<int> UpdateIdentityResourceAsync(IdentityResource identityResource);

        Task<int> DeleteIdentityResourceAsync(IdentityResource identityResource);

        Task<bool> CanInsertIdentityResourcePropertyAsync(IdentityResourceProperty identityResourceProperty);

        Task<PagedList<IdentityResourceProperty>> GetIdentityResourcePropertiesAsync(int identityResourceId,
            int page = 1, int pageSize = 10);

        Task<IdentityResourceProperty> GetIdentityResourcePropertyAsync(int identityResourcePropertyId);

        Task<int> AddIdentityResourcePropertyAsync(int identityResourceId,
            IdentityResourceProperty identityResourceProperty);

        Task<int> DeleteIdentityResourcePropertyAsync(IdentityResourceProperty identityResourceProperty);

        Task<int> SaveAllChangesAsync();

        bool AutoSaveChanges { get; set; }
    }
}