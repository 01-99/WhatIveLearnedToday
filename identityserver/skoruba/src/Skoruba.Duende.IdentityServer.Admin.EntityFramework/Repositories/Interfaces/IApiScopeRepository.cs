﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Extensions.Common;

namespace Skoruba.Duende.IdentityServer.Admin.EntityFramework.Repositories.Interfaces
{
    public interface IApiScopeRepository
    {
        Task<PagedList<ApiScope>> GetApiScopesAsync(string search, int page = 1, int pageSize = 10);

        Task<ApiScope> GetApiScopeAsync(int apiScopeId);

        Task<int> AddApiScopeAsync(ApiScope apiScope);

        Task<int> UpdateApiScopeAsync(ApiScope apiScope);

        Task<int> DeleteApiScopeAsync(ApiScope apiScope);

        Task<bool> CanInsertApiScopeAsync(ApiScope apiScope);

        Task<ICollection<string>> GetApiScopesNameAsync(string scope, int limit = 0);

        Task<PagedList<ApiScopeProperty>> GetApiScopePropertiesAsync(int apiScopeId, int page = 1, int pageSize = 10);

        Task<ApiScopeProperty> GetApiScopePropertyAsync(int apiScopePropertyId);

        Task<int> AddApiScopePropertyAsync(int apiScopeId, ApiScopeProperty apiScopeProperty);

        Task<bool> CanInsertApiScopePropertyAsync(ApiScopeProperty apiScopeProperty);

        Task<int> DeleteApiScopePropertyAsync(ApiScopeProperty apiScopeProperty);

        Task<string> GetApiScopeNameAsync(int apiScopeId);

        Task<int> SaveAllChangesAsync();

        bool AutoSaveChanges { get; set; }
    }
}