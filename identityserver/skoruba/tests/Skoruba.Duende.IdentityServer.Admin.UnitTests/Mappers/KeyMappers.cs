﻿using System;
using FluentAssertions;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Mappers;
using Skoruba.Duende.IdentityServer.Admin.UnitTests.Mocks;
using Xunit;

namespace Skoruba.Duende.IdentityServer.Admin.UnitTests.Mappers
{
    public class KeyMappers
    {
        [Fact]
        public void CanMapKeyToKeyDto()
        {
            var key = KeyMock.GenerateRandomKey(Guid.NewGuid().ToString());

            var keyDto = key.ToModel();

            keyDto.Should().BeEquivalentTo(key, options => 
                options.Excluding(x=> x.DataProtected)
                    .Excluding(x=> x.Data));
        }
    }
}