﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Options;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Skoruba.AuditLogging.Services;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Mappers;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Resources;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Services;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Services.Interfaces;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Repositories;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Repositories.Interfaces;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Shared.DbContexts;
using Skoruba.Duende.IdentityServer.Admin.UnitTests.Mocks;
using Xunit;

namespace Skoruba.Duende.IdentityServer.Admin.UnitTests.Services
{
	public class IdentityResourceServiceTests
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

		private IIdentityResourceRepository GetIdentityResourceRepository(IdentityServerConfigurationDbContext context)
		{
			IIdentityResourceRepository identityResourceRepository = new IdentityResourceRepository<IdentityServerConfigurationDbContext>(context);

			return identityResourceRepository;
		}

		private IIdentityResourceService GetIdentityResourceService(IIdentityResourceRepository repository, IIdentityResourceServiceResources identityResourceServiceResources, IAuditEventLogger auditEventLogger)
		{
			IIdentityResourceService identityResourceService = new IdentityResourceService(repository, identityResourceServiceResources, auditEventLogger);

			return identityResourceService;
		}

        private IIdentityResourceService GetIdentityResourceService(IdentityServerConfigurationDbContext context)
        {
            var identityResourceRepository = GetIdentityResourceRepository(context);

            var localizerIdentityResourceMock = new Mock<IIdentityResourceServiceResources>();
            var localizerIdentityResource = localizerIdentityResourceMock.Object;

            var auditLoggerMock = new Mock<IAuditEventLogger>();
            var auditLogger = auditLoggerMock.Object;

            var identityResourceService = GetIdentityResourceService(identityResourceRepository, localizerIdentityResource, auditLogger);

            return identityResourceService;
        }

        [Fact]
		public async Task AddIdentityResourceAsync()
		{
			using (var context = GetDbContext())
            {
                var identityResourceService = GetIdentityResourceService(context);

                //Generate random new identity resource
                var identityResourceDto = IdentityResourceDtoMock.GenerateRandomIdentityResource(0);

				await identityResourceService.AddIdentityResourceAsync(identityResourceDto);

				//Get new identity resource
				var identityResource = await context.IdentityResources.Where(x => x.Name == identityResourceDto.Name).SingleOrDefaultAsync();

				var newIdentityResourceDto = await identityResourceService.GetIdentityResourceAsync(identityResource.Id);

				//Assert new identity resource
                newIdentityResourceDto.Should().BeEquivalentTo(identityResourceDto, options => options.Excluding(o => o.Id));
			}
		}

		[Fact]
		public async Task GetIdentityResourceAsync()
		{
			using (var context = GetDbContext())
			{
                var identityResourceService = GetIdentityResourceService(context);
                
                //Generate random new identity resource
                var identityResourceDto = IdentityResourceDtoMock.GenerateRandomIdentityResource(0);

				await identityResourceService.AddIdentityResourceAsync(identityResourceDto);

				//Get new identity resource
				var identityResource = await context.IdentityResources.Where(x => x.Name == identityResourceDto.Name).SingleOrDefaultAsync();

				var newIdentityResourceDto = await identityResourceService.GetIdentityResourceAsync(identityResource.Id);

				//Assert new identity resource
                newIdentityResourceDto.Should().BeEquivalentTo(identityResourceDto, options => options.Excluding(o => o.Id));
			}
		}

		[Fact]
		public async Task RemoveIdentityResourceAsync()
		{
			using (var context = GetDbContext())
			{
                var identityResourceService = GetIdentityResourceService(context);
                
                //Generate random new identity resource
                var identityResourceDto = IdentityResourceDtoMock.GenerateRandomIdentityResource(0);

				await identityResourceService.AddIdentityResourceAsync(identityResourceDto);

				//Get new identity resource
				var identityResource = await context.IdentityResources.Where(x => x.Name == identityResourceDto.Name).SingleOrDefaultAsync();

				var newIdentityResourceDto = await identityResourceService.GetIdentityResourceAsync(identityResource.Id);

				//Assert new identity resource
                newIdentityResourceDto.Should().BeEquivalentTo(identityResourceDto, options => options.Excluding(o => o.Id));

				//Remove identity resource
				await identityResourceService.DeleteIdentityResourceAsync(newIdentityResourceDto);

				//Try Get Removed identity resource
				var removeIdentityResource = await context.IdentityResources.Where(x => x.Id == identityResource.Id)
					.SingleOrDefaultAsync();

				//Assert removed identity resource
				removeIdentityResource.Should().BeNull();
			}
		}

		[Fact]
		public async Task UpdateIdentityResourceAsync()
		{
			using (var context = GetDbContext())
			{
                var identityResourceService = GetIdentityResourceService(context);

                //Generate random new identity resource
                var identityResourceDto = IdentityResourceDtoMock.GenerateRandomIdentityResource(0);

				await identityResourceService.AddIdentityResourceAsync(identityResourceDto);

				//Get new identity resource
				var identityResource = await context.IdentityResources.Where(x => x.Name == identityResourceDto.Name).SingleOrDefaultAsync();

				var newIdentityResourceDto = await identityResourceService.GetIdentityResourceAsync(identityResource.Id);

				//Assert new identity resource
                newIdentityResourceDto.Should().BeEquivalentTo(identityResourceDto, options => options.Excluding(o => o.Id));

				//Detached the added item
				context.Entry(identityResource).State = EntityState.Detached;

				//Generete new identity resuorce with added item id
				var updatedIdentityResource = IdentityResourceDtoMock.GenerateRandomIdentityResource(identityResource.Id);

				//Update identity resuorce
				await identityResourceService.UpdateIdentityResourceAsync(updatedIdentityResource);

				var updatedIdentityResourceDto = await identityResourceService.GetIdentityResourceAsync(identityResource.Id);

				//Assert updated identity resuorce
                updatedIdentityResourceDto.Should().BeEquivalentTo(updatedIdentityResource, options => options.Excluding(o => o.Id));
			}
		}

		[Fact]
		public async Task AddIdentityResourcePropertyAsync()
		{
			using (var context = GetDbContext())
			{
                var identityResourceService = GetIdentityResourceService(context);

                //Generate random new identity resource
                var identityResource = IdentityResourceDtoMock.GenerateRandomIdentityResource(0);

				await identityResourceService.AddIdentityResourceAsync(identityResource);

				//Get new identity resource
				var resource = await context.IdentityResources.Where(x => x.Name == identityResource.Name).SingleOrDefaultAsync();

				var identityResourceDto = await identityResourceService.GetIdentityResourceAsync(resource.Id);

				//Assert new identity resource
                identityResourceDto.Should().BeEquivalentTo(identityResource, options => options.Excluding(o => o.Id));

				//Generate random new identity resource property
				var resourceProperty = IdentityResourceDtoMock.GenerateRandomIdentityResourceProperty(0, resource.Id);

				//Add new identity resource property
				await identityResourceService.AddIdentityResourcePropertyAsync(resourceProperty);

				//Get inserted identity resource property
				var property = await context.IdentityResourceProperties.Where(x => x.Value == resourceProperty.Value && x.IdentityResource.Id == resource.Id)
					.SingleOrDefaultAsync();

				//Map entity to model
				var propertyDto = property.ToModel();

				//Get new identity resource property    
				var resourcePropertiesDto = await identityResourceService.GetIdentityResourcePropertyAsync(property.Id);

				//Assert
                propertyDto.Should().BeEquivalentTo(resourcePropertiesDto, options =>
					options.Excluding(o => o.IdentityResourcePropertyId)
						   .Excluding(o => o.IdentityResourceName));
			}
		}

		[Fact]
		public async Task GetIdentityResourcePropertyAsync()
		{
			using (var context = GetDbContext())
			{
                var identityResourceService = GetIdentityResourceService(context);

                //Generate random new identity resource
                var identityResource = IdentityResourceDtoMock.GenerateRandomIdentityResource(0);

				await identityResourceService.AddIdentityResourceAsync(identityResource);

				//Get new identity resource
				var resource = await context.IdentityResources.Where(x => x.Name == identityResource.Name).SingleOrDefaultAsync();

				var identityResourceDto = await identityResourceService.GetIdentityResourceAsync(resource.Id);

				//Assert new identity resource
                identityResourceDto.Should().BeEquivalentTo(identityResource, options => options.Excluding(o => o.Id));

				//Generate random new identity resource property
				var identityResourceProperty = IdentityResourceDtoMock.GenerateRandomIdentityResourceProperty(0, resource.Id);

				//Add new identity resource property
				await identityResourceService.AddIdentityResourcePropertyAsync(identityResourceProperty);

				//Get inserted identity resource property
				var property = await context.IdentityResourceProperties.Where(x => x.Value == identityResourceProperty.Value && x.IdentityResource.Id == resource.Id)
					.SingleOrDefaultAsync();

				//Map entity to model
				var propertyDto = property.ToModel();

				//Get new identity resource property    
				var resourcePropertiesDto = await identityResourceService.GetIdentityResourcePropertyAsync(property.Id);

				//Assert
                propertyDto.Should().BeEquivalentTo(resourcePropertiesDto, options =>
					options.Excluding(o => o.IdentityResourcePropertyId)
					.Excluding(o => o.IdentityResourceName));
			}
		}

		[Fact]
		public async Task DeleteIdentityResourcePropertyAsync()
		{
			using (var context = GetDbContext())
			{
                var identityResourceService = GetIdentityResourceService(context);

                //Generate random new identity resource
                var resourceDto = IdentityResourceDtoMock.GenerateRandomIdentityResource(0);

				await identityResourceService.AddIdentityResourceAsync(resourceDto);

				//Get new identity resource
				var resource = await context.IdentityResources.Where(x => x.Name == resourceDto.Name).SingleOrDefaultAsync();

				var identityResourceDto = await identityResourceService.GetIdentityResourceAsync(resource.Id);

				//Assert new identity resource
                identityResourceDto.Should().BeEquivalentTo(resourceDto, options => options.Excluding(o => o.Id));

				//Generate random new identity resource Property
				var identityResourcePropertiesDto = IdentityResourceDtoMock.GenerateRandomIdentityResourceProperty(0, resource.Id);

				//Add new identity resource Property
				await identityResourceService.AddIdentityResourcePropertyAsync(identityResourcePropertiesDto);

				//Get inserted identity resource Property
				var property = await context.IdentityResourceProperties.Where(x => x.Value == identityResourcePropertiesDto.Value && x.IdentityResource.Id == resource.Id)
					.SingleOrDefaultAsync();

				//Map entity to model
				var propertiesDto = property.ToModel();

				//Get new identity resource Property    
				var resourcePropertiesDto = await identityResourceService.GetIdentityResourcePropertyAsync(property.Id);

				//Assert
                propertiesDto.Should().BeEquivalentTo(resourcePropertiesDto, options =>
					options.Excluding(o => o.IdentityResourcePropertyId)
					.Excluding(o => o.IdentityResourceName));

				//Delete identity resource Property
				await identityResourceService.DeleteIdentityResourcePropertyAsync(resourcePropertiesDto);

				//Get removed identity resource Property
				var identityResourceProperty = await context.IdentityResourceProperties.Where(x => x.Id == property.Id).SingleOrDefaultAsync();

				//Assert after delete it
				identityResourceProperty.Should().BeNull();
			}
		}
	}
}