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
    public class ClientServiceTests
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

        private IClientRepository GetClientRepository(IdentityServerConfigurationDbContext context)
        {
            IClientRepository clientRepository = new ClientRepository<IdentityServerConfigurationDbContext>(context);

            return clientRepository;
        }

        private IClientService GetClientService(IClientRepository repository, IClientServiceResources resources, IAuditEventLogger auditEventLogger)
        {
            IClientService clientService = new ClientService(repository, resources, auditEventLogger);

            return clientService;
        }

        private IClientService GetClientService(IdentityServerConfigurationDbContext context)
        {
            var clientRepository = GetClientRepository(context);

            var localizerMock = new Mock<IClientServiceResources>();
            var localizer = localizerMock.Object;

            var auditLoggerMock = new Mock<IAuditEventLogger>();
            var auditLogger = auditLoggerMock.Object;

            var clientService = GetClientService(clientRepository, localizer, auditLogger);

            return clientService;
        }

        [Fact]
        public async Task AddClientAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));
            }
        }

        [Fact]
        public async Task CloneClientAsync()
        {
            int clonedClientId;

            using (var context = GetDbContext())
            {
                //Generate random new client
                var clientDto = ClientDtoMock.GenerateRandomClient(0);

                var clientService = GetClientService(context);

                //Add new client
                await clientService.AddClientAsync(clientDto);

                var clientId = await context.Clients.Where(x => x.ClientId == clientDto.ClientId).Select(x => x.Id)
                    .SingleOrDefaultAsync();

                var clientDtoToClone = await clientService.GetClientAsync(clientId);

                var clientCloneDto = ClientDtoMock.GenerateClientCloneDto(clientDtoToClone.Id);

                //Try clone it
                clonedClientId = await clientService.CloneClientAsync(clientCloneDto);

                var cloneClientEntity = await context.Clients
                    .Include(x => x.AllowedGrantTypes)
                    .Include(x => x.RedirectUris)
                    .Include(x => x.PostLogoutRedirectUris)
                    .Include(x => x.AllowedScopes)
                    .Include(x => x.ClientSecrets)
                    .Include(x => x.Claims)
                    .Include(x => x.IdentityProviderRestrictions)
                    .Include(x => x.AllowedCorsOrigins)
                    .Include(x => x.Properties)
                    .Where(x => x.Id == clonedClientId).SingleOrDefaultAsync();

                var clientToCompare = await context.Clients
                    .Include(x => x.AllowedGrantTypes)
                    .Include(x => x.RedirectUris)
                    .Include(x => x.PostLogoutRedirectUris)
                    .Include(x => x.AllowedScopes)
                    .Include(x => x.ClientSecrets)
                    .Include(x => x.Claims)
                    .Include(x => x.IdentityProviderRestrictions)
                    .Include(x => x.AllowedCorsOrigins)
                    .Include(x => x.Properties)
                    .Where(x => x.Id == clientDtoToClone.Id).SingleOrDefaultAsync();

                //Assert cloned client
                clientToCompare.Should().BeEquivalentTo(cloneClientEntity,
                    options => options.Excluding(o => o.Id)
                        .Excluding(o => o.ClientSecrets)
                        .Excluding(o => o.ClientId)
                        .Excluding(o => o.ClientName)

                        //Skip the collections because is not possible ignore property in list :-(
                        //Note: I've found the solution above - try ignore property of the list using SelectedMemberPath                        
                        .Excluding(o => o.AllowedGrantTypes)
                        .Excluding(o => o.RedirectUris)
                        .Excluding(o => o.PostLogoutRedirectUris)
                        .Excluding(o => o.AllowedScopes)
                        .Excluding(o => o.ClientSecrets)
                        .Excluding(o => o.Claims)
                        .Excluding(o => o.IdentityProviderRestrictions)
                        .Excluding(o => o.AllowedCorsOrigins)
                        .Excluding(o => o.Properties)
                );


                //New client relations have new id's and client relations therefore is required ignore them
                clientToCompare.AllowedGrantTypes.Should().BeEquivalentTo(cloneClientEntity.AllowedGrantTypes,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.AllowedCorsOrigins.Should().BeEquivalentTo(cloneClientEntity.AllowedCorsOrigins,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.RedirectUris.Should().BeEquivalentTo(cloneClientEntity.RedirectUris,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.PostLogoutRedirectUris.Should().BeEquivalentTo(cloneClientEntity.PostLogoutRedirectUris,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.AllowedScopes.Should().BeEquivalentTo(cloneClientEntity.AllowedScopes,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.ClientSecrets.Should().BeEquivalentTo(cloneClientEntity.ClientSecrets,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.Claims.Should().BeEquivalentTo(cloneClientEntity.Claims,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.IdentityProviderRestrictions.Should().BeEquivalentTo(
                    clientToCompare.IdentityProviderRestrictions,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));

                clientToCompare.Properties.Should().BeEquivalentTo(cloneClientEntity.Properties,
                    option => option.Excluding(x => x.Path.EndsWith("Id"))
                        .Excluding(x => x.Path.EndsWith("Client")));
            }
        }

        [Fact]
        public async Task UpdateClientAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client without id
                var client = ClientDtoMock.GenerateRandomClient(0);

                //Add new client
                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Detached the added item
                context.Entry(clientEntity).State = EntityState.Detached;

                //Generete new client with added item id
                var updatedClient = ClientDtoMock.GenerateRandomClient(clientDto.Id);

                //Update client
                await clientService.UpdateClientAsync(updatedClient);

                //Get updated client
                var updatedClientEntity = await context.Clients.Where(x => x.Id == updatedClient.Id).SingleAsync();

                var updatedClientDto = await clientService.GetClientAsync(updatedClientEntity.Id);

                //Assert updated client
                updatedClientDto.Should().BeEquivalentTo(updatedClient, options => options.Excluding(o => o.Id));
            }
        }

        [Fact]
        public async Task RemoveClientAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client without id
                var client = ClientDtoMock.GenerateRandomClient(0);

                //Add new client
                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Detached the added item
                context.Entry(clientEntity).State = EntityState.Detached;

                //Remove client
                await clientService.RemoveClientAsync(clientDto);

                //Try Get Removed client
                var removeClientEntity = await context.Clients.Where(x => x.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Assert removed client - it might be null
                removeClientEntity.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetClientAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));
            }
        }

        [Fact]
        public async Task AddClientClaimAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client Claim
                var clientClaim = ClientDtoMock.GenerateRandomClientClaim(0, clientEntity.Id);

                //Add new client claim
                await clientService.AddClientClaimAsync(clientClaim);

                //Get inserted client claims
                var claim = await context.ClientClaims.Where(x => x.Value == clientClaim.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var claimsDto = claim.ToModel();

                //Get new client claim    
                var clientClaimsDto = await clientService.GetClientClaimAsync(claim.Id);

                //Assert
                claimsDto.Should().BeEquivalentTo(clientClaimsDto, options =>
                    options.Excluding(o => o.ClientClaimId)
                           .Excluding(o => o.ClientName));
            }
        }

        [Fact]
        public async Task DeleteClientClaimAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client Claim
                var clientClaim = ClientDtoMock.GenerateRandomClientClaim(0, clientEntity.Id);

                //Add new client claim
                await clientService.AddClientClaimAsync(clientClaim);

                //Get inserted client claims
                var claim = await context.ClientClaims.Where(x => x.Value == clientClaim.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var claimsDto = claim.ToModel();

                //Get new client claim    
                var clientClaimsDto = await clientService.GetClientClaimAsync(claim.Id);

                //Assert
                claimsDto.Should().BeEquivalentTo(clientClaimsDto, options => options.Excluding(o => o.ClientClaimId)
                                .Excluding(o => o.ClientName));

                //Delete client claim
                await clientService.DeleteClientClaimAsync(clientClaimsDto);

                //Get removed client claim
                var deletedClientClaim = await context.ClientClaims.Where(x => x.Id == claim.Id).SingleOrDefaultAsync();

                //Assert after delete it
                deletedClientClaim.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetClientClaimAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client Claim
                var clientClaim = ClientDtoMock.GenerateRandomClientClaim(0, clientEntity.Id);

                //Add new client claim
                await clientService.AddClientClaimAsync(clientClaim);

                //Get inserted client claims
                var claim = await context.ClientClaims.Where(x => x.Value == clientClaim.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var claimsDto = claim.ToModel();

                //Get new client claim    
                var clientClaimsDto = await clientService.GetClientClaimAsync(claim.Id);

                //Assert
                claimsDto.Should().BeEquivalentTo(clientClaimsDto, options => options.Excluding(o => o.ClientClaimId)
                    .Excluding(o => o.ClientName));
            }
        }

        [Fact]
        public async Task AddClientPropertyAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client property
                var clicentProperty = ClientDtoMock.GenerateRandomClientProperty(0, clientEntity.Id);

                //Add new client property
                await clientService.AddClientPropertyAsync(clicentProperty);

                //Get inserted client property
                var property = await context.ClientProperties.Where(x => x.Value == clicentProperty.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var propertyDto = property.ToModel();

                //Get new client property    
                var clientPropertiesDto = await clientService.GetClientPropertyAsync(property.Id);

                //Assert
                propertyDto.Should().BeEquivalentTo(clientPropertiesDto, options => 
                    options.Excluding(o => o.ClientPropertyId)
                           .Excluding(o => o.ClientName));
            }
        }

        [Fact]
        public async Task GetClientPropertyAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client property
                var clicentProperty = ClientDtoMock.GenerateRandomClientProperty(0, clientEntity.Id);

                //Add new client property
                await clientService.AddClientPropertyAsync(clicentProperty);

                //Get inserted client property
                var property = await context.ClientProperties.Where(x => x.Value == clicentProperty.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var propertyDto = property.ToModel();

                //Get new client property    
                var clientPropertiesDto = await clientService.GetClientPropertyAsync(property.Id);

                //Assert
                propertyDto.Should().BeEquivalentTo(clientPropertiesDto, options => options.Excluding(o => o.ClientPropertyId)
                    .Excluding(o => o.ClientName));
            }
        }

        [Fact]
        public async Task DeleteClientPropertyAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client Property
                var clientProperty = ClientDtoMock.GenerateRandomClientProperty(0, clientEntity.Id);

                //Add new client Property
                await clientService.AddClientPropertyAsync(clientProperty);

                //Get inserted client Property
                var property = await context.ClientProperties.Where(x => x.Value == clientProperty.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var propertiesDto = property.ToModel();

                //Get new client Property    
                var clientPropertiesDto = await clientService.GetClientPropertyAsync(property.Id);

                //Assert
                propertiesDto.Should().BeEquivalentTo(clientPropertiesDto, options => options.Excluding(o => o.ClientPropertyId)
                    .Excluding(o => o.ClientName));

                //Delete client Property
                await clientService.DeleteClientPropertyAsync(clientPropertiesDto);

                //Get removed client Property
                var deletedClientProperty = await context.ClientProperties.Where(x => x.Id == property.Id).SingleOrDefaultAsync();

                //Assert after delete it
                deletedClientProperty.Should().BeNull();
            }
        }

        [Fact]
        public async Task AddClientSecretAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client secret
                var clientSecret = ClientDtoMock.GenerateRandomClientSecret(0, clientEntity.Id);

                //Add new client secret
                await clientService.AddClientSecretAsync(clientSecret);

                //Get inserted client secret
                var secret = await context.ClientSecrets.Where(x => x.Value == clientSecret.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var clientSecretsDto = secret.ToModel();

                //Get new client secret    
                var secretsDto = await clientService.GetClientSecretAsync(secret.Id);

                clientSecretsDto.Value.Should().Be(clientSecret.Value);

                //Assert
                clientSecretsDto.Should().BeEquivalentTo(secretsDto, options => 
                    options.Excluding(o => o.ClientSecretId)
                           .Excluding(o => o.ClientName)
                           .Excluding(o => o.Value));
            }
        }

        [Fact]
        public async Task GetClientSecretAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);

                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client secret
                var clientSecret = ClientDtoMock.GenerateRandomClientSecret(0, clientEntity.Id);

                //Add new client secret
                await clientService.AddClientSecretAsync(clientSecret);

                //Get inserted client secret
                var secret = await context.ClientSecrets.Where(x => x.Value == clientSecret.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var clientSecretsDto = secret.ToModel();

                //Get new client secret    
                var secretsDto = await clientService.GetClientSecretAsync(secret.Id);

                clientSecretsDto.Value.Should().Be(clientSecret.Value);

                //Assert
                clientSecretsDto.Should().BeEquivalentTo(secretsDto, options => options.Excluding(o => o.ClientSecretId)
                    .Excluding(o => o.ClientName)
                    .Excluding(o => o.Value));
            }
        }

        [Fact]
        public async Task DeleteClientSecretAsync()
        {
            using (var context = GetDbContext())
            {
                var clientService = GetClientService(context);
                
                //Generate random new client
                var client = ClientDtoMock.GenerateRandomClient(0);

                await clientService.AddClientAsync(client);

                //Get new client
                var clientEntity =
                    await context.Clients.Where(x => x.ClientId == client.ClientId).SingleOrDefaultAsync();

                var clientDto = await clientService.GetClientAsync(clientEntity.Id);

                //Assert new client
                clientDto.Should().BeEquivalentTo(client, options => options.Excluding(o => o.Id));

                //Generate random new Client secret
                var clientSecret = ClientDtoMock.GenerateRandomClientSecret(0, clientEntity.Id);

                //Add new client secret
                await clientService.AddClientSecretAsync(clientSecret);

                //Get inserted client secret
                var secret = await context.ClientSecrets.Where(x => x.Value == clientSecret.Value && x.Client.Id == clientEntity.Id)
                    .SingleOrDefaultAsync();

                //Map entity to model
                var secretsDto = secret.ToModel();

                //Get new client secret    
                var clientSecretsDto = await clientService.GetClientSecretAsync(secret.Id);

                //Assert
                secretsDto.Should().BeEquivalentTo(clientSecretsDto, options => options.Excluding(o => o.ClientSecretId)
                    .Excluding(o => o.ClientName)
                    .Excluding(o => o.Value));

                clientSecret.Value.Should().Be(secret.Value);

                //Delete client secret
                await clientService.DeleteClientSecretAsync(clientSecretsDto);

                //Get removed client secret
                var deleteClientSecret = await context.ClientSecrets.Where(x => x.Id == secret.Id).SingleOrDefaultAsync();

                //Assert after delete it
                deleteClientSecret.Should().BeNull();
            }
        }
    }
}