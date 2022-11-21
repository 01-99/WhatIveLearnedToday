﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Mvc;
using Skoruba.Duende.IdentityServer.Admin.UI.Configuration;

namespace Skoruba.Duende.IdentityServer.Admin.UI.ViewComponents
{
	public class IdentityServerLinkViewComponent : ViewComponent
    {
        private readonly AdminConfiguration _configuration;

        public IdentityServerLinkViewComponent(AdminConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IViewComponentResult Invoke()
        {
            var identityServerUrl = _configuration.IdentityServerBaseUrl;
            
            return View(model: identityServerUrl);
        }
    }
}
