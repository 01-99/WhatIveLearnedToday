﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Options;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Repositories;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Repositories.Interfaces;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Shared.DbContexts;
using Skoruba.Duende.IdentityServer.Admin.UnitTests.Mocks;
using Xunit;

namespace Skoruba.Duende.IdentityServer.Admin.UnitTests.Repositories
{
    public class ApiResourceRepositoryTests
    {
        private IdentityServerConfigurationDbContext GetDbContext()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(new ConfigurationStoreOptions());
            serviceCollection.AddSingleton(new OperationalStoreOptions());

            serviceCollection.AddDbContext<IdentityServerConfigurationDbContext>(builder => builder.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var context = serviceProvider.GetService<IdentityServerConfigurationDbContext>();

            return context;
        }

        private IApiResourceRepository GetApiResourceRepository(IdentityServerConfigurationDbContext context)
        {
            IApiResourceRepository apiResourceRepository = new ApiResourceRepository<IdentityServerConfigurationDbContext>(context);

            return apiResourceRepository;
        }


        [Fact]
        public async Task AddApiResourceAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Get new api resource
                var newApiResource = await context.ApiResources.Where(x => x.Id == apiResource.Id).SingleAsync();

                //Assert new api resource
                apiResource.Should().BeEquivalentTo(newApiResource, options => options.Excluding(o => o.Id));
            }
        }

        [Fact]
        public async Task GetApiResourceAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Get new api resource
                var newApiResource = await apiResourceRepository.GetApiResourceAsync(apiResource.Id);

                //Assert new api resource
                apiResource.Should().BeEquivalentTo(newApiResource, options => options.Excluding(o => o.Id).Excluding(o => o.Secrets)
                    .Excluding(o => o.Scopes)
                    .Excluding(o => o.UserClaims));

                apiResource.UserClaims.Should().BeEquivalentTo(newApiResource.UserClaims,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("ApiResource")));
            }
        }

        [Fact]
        public async Task DeleteApiResourceAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Get new api resource
                var newApiResource = await context.ApiResources.Where(x => x.Id == apiResource.Id).SingleAsync();

                //Assert new api resource
                apiResource.Should().BeEquivalentTo(newApiResource, options => options.Excluding(o => o.Id));

                //Delete api resource
                await apiResourceRepository.DeleteApiResourceAsync(newApiResource);

                //Get deleted api resource
                var deletedApiResource = await context.ApiResources.Where(x => x.Id == apiResource.Id).SingleOrDefaultAsync();

                //Assert if it not exist
                deletedApiResource.Should().BeNull();
            }
        }

        [Fact]
        public async Task UpdateApiResourceAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Get new api resource
                var newApiResource = await context.ApiResources.Where(x => x.Id == apiResource.Id).SingleOrDefaultAsync();

                //Assert new api resource
                apiResource.Should().BeEquivalentTo(newApiResource, options => options.Excluding(o => o.Id));

                //Detached the added item
                context.Entry(newApiResource).State = EntityState.Detached;

                //Generete new api resource with added item id
                var updatedApiResource = ApiResourceMock.GenerateRandomApiResource(newApiResource.Id);

                //Update api resource
                await apiResourceRepository.UpdateApiResourceAsync(updatedApiResource);

                //Get updated api resource
                var updatedApiResourceEntity = await context.ApiResources.Where(x => x.Id == updatedApiResource.Id).SingleAsync();

                //Assert updated api resource
                updatedApiResourceEntity.Should().BeEquivalentTo(updatedApiResource);
            }
        }

        [Fact]
        public async Task AddApiSecretAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Generate random new api secret
                var apiSecret = ApiResourceMock.GenerateRandomApiSecret(0);

                //Add new api secret
                await apiResourceRepository.AddApiSecretAsync(apiResource.Id, apiSecret);

                //Get new api secret
                var newApiSecret = await context.ApiSecrets.Where(x => x.Id == apiSecret.Id).SingleAsync();

                //Assert new api secret
                apiSecret.Should().BeEquivalentTo(newApiSecret, options => options.Excluding(o => o.Id));
            }
        }

        [Fact]
        public async Task DeleteApiSecretAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Generate random new api scope
                var apiSecret = ApiResourceMock.GenerateRandomApiSecret(0);

                //Add new api secret
                await apiResourceRepository.AddApiSecretAsync(apiResource.Id, apiSecret);

                //Get new api resource
                var newApiSecret = await context.ApiSecrets.Where(x => x.Id == apiSecret.Id).SingleOrDefaultAsync();

                //Assert new api resource
                apiSecret.Should().BeEquivalentTo(newApiSecret, options => options.Excluding(o => o.Id));

                //Try delete it
                await apiResourceRepository.DeleteApiSecretAsync(newApiSecret);

                //Get deleted api secret
                var deletedApiSecret = await context.ApiSecrets.Where(x => x.Id == newApiSecret.Id).SingleOrDefaultAsync();

                //Assert if it exist
                deletedApiSecret.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetApiSecretAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Generate random new api secret
                var apiSecret = ApiResourceMock.GenerateRandomApiSecret(0);

                //Add new api secret
                await apiResourceRepository.AddApiSecretAsync(apiResource.Id, apiSecret);

                //Get new api secret
                var newApiSecret = await apiResourceRepository.GetApiSecretAsync(apiSecret.Id);

                //Assert new api secret
                apiSecret.Should().BeEquivalentTo(newApiSecret, options => options.Excluding(o => o.Id)
                    .Excluding(o => o.ApiResource.Secrets)
                    .Excluding(o => o.ApiResource.UserClaims)
                    .Excluding(o => o.ApiResource.Scopes));
            }
        }

        [Fact]
        public async Task AddApiResourcePropertyAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource without id
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Get new api resource
                var resource = await apiResourceRepository.GetApiResourceAsync(apiResource.Id);

                //Assert new api resource
                apiResource.Should().BeEquivalentTo(resource, options => options.Excluding(o => o.Id)
                    .Excluding(o => o.Secrets)
                    .Excluding(o => o.Scopes)
                    .Excluding(o => o.UserClaims));

                apiResource.UserClaims.Should().BeEquivalentTo(resource.UserClaims,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("ApiResource")));

                //Generate random new api resource property
                var apiResourceProperty = ApiResourceMock.GenerateRandomApiResourceProperty(0);

                //Add new api resource property
                await apiResourceRepository.AddApiResourcePropertyAsync(resource.Id, apiResourceProperty);

                //Get new api resource property
                var resourceProperty = await context.ApiResourceProperties.Where(x => x.Id == apiResourceProperty.Id)
                    .SingleOrDefaultAsync();

                apiResourceProperty.Should().BeEquivalentTo(resourceProperty,
                    options => options.Excluding(o => o.Id).Excluding(x => x.ApiResource));
            }
        }

        [Fact]
        public async Task DeleteApiResourcePropertyAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource without id
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Get new api resource
                var resource = await apiResourceRepository.GetApiResourceAsync(apiResource.Id);

                //Assert new api resource
                apiResource.Should().BeEquivalentTo(resource, options => options.Excluding(o => o.Id)
                    .Excluding(o => o.Secrets)
                    .Excluding(o => o.Scopes)
                    .Excluding(o => o.UserClaims));

                apiResource.UserClaims.Should().BeEquivalentTo(resource.UserClaims,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("ApiResource")));

                //Generate random new api resource property
                var apiResourceProperty = ApiResourceMock.GenerateRandomApiResourceProperty(0);

                //Add new api resource property
                await apiResourceRepository.AddApiResourcePropertyAsync(resource.Id, apiResourceProperty);

                //Get new api resource property
                var property = await context.ApiResourceProperties.Where(x => x.Id == apiResourceProperty.Id)
                    .SingleOrDefaultAsync();

                //Assert
                apiResourceProperty.Should().BeEquivalentTo(property,
                    options => options.Excluding(o => o.Id).Excluding(x => x.ApiResource));

                //Try delete it
                await apiResourceRepository.DeleteApiResourcePropertyAsync(property);

                //Get new api resource property
                var resourceProperty = await context.ApiResourceProperties.Where(x => x.Id == apiResourceProperty.Id)
                    .SingleOrDefaultAsync();

                //Assert
                resourceProperty.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetApiResourcePropertyAsync()
        {
            using (var context = GetDbContext())
            {
                var apiResourceRepository = GetApiResourceRepository(context);

                //Generate random new api resource without id
                var apiResource = ApiResourceMock.GenerateRandomApiResource(0);

                //Add new api resource
                await apiResourceRepository.AddApiResourceAsync(apiResource);

                //Get new api resource
                var resource = await apiResourceRepository.GetApiResourceAsync(apiResource.Id);

                //Assert new api resource
                apiResource.Should().BeEquivalentTo(resource, options => options.Excluding(o => o.Id)
                    .Excluding(o => o.Secrets)
                    .Excluding(o => o.Scopes)
                    .Excluding(o => o.UserClaims));

                apiResource.UserClaims.Should().BeEquivalentTo(resource.UserClaims,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("ApiResource")));

                //Generate random new api resource property
                var apiResourceProperty = ApiResourceMock.GenerateRandomApiResourceProperty(0);

                //Add new api resource property
                await apiResourceRepository.AddApiResourcePropertyAsync(resource.Id, apiResourceProperty);

                //Get new api resource property
                var resourceProperty = await apiResourceRepository.GetApiResourcePropertyAsync(apiResourceProperty.Id);

                apiResourceProperty.Should().BeEquivalentTo(resourceProperty,
                    options => options.Excluding(o => o.Id).Excluding(x => x.ApiResource));
            }
        }
    }
}