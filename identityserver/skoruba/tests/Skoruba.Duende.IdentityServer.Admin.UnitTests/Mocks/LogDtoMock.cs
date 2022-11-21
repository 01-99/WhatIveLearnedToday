﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Bogus;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Dtos.Log;

namespace Skoruba.Duende.IdentityServer.Admin.UnitTests.Mocks
{
    public class LogDtoMock
    {
        public static Faker<LogDto> GetLogFaker(int id)
        {
            var fakerLogDto = new Faker<LogDto>()
                .RuleFor(o => o.Id, id)
                .RuleFor(o => o.Exception, f => f.Random.Words(f.Random.Number(1, 30)))
                .RuleFor(o => o.LogEvent, f => f.Random.Words(f.Random.Number(1, 30)))
                .RuleFor(o => o.Level, f => f.Random.Words(1))
                .RuleFor(o => o.TimeStamp, f => f.Date.Future())
                .RuleFor(o => o.Message, f => f.Random.Words(f.Random.Number(1, 30)))
                .RuleFor(o => o.MessageTemplate, f => f.Random.Words(f.Random.Number(1, 30)))
                .RuleFor(o => o.Properties, f => f.Random.Words(f.Random.Number(1, 30)));

            return fakerLogDto;
        }

        public static LogDto GenerateRandomLog(int id)
        {
            var log = GetLogFaker(id).Generate();

            return log;
        }
    }
}