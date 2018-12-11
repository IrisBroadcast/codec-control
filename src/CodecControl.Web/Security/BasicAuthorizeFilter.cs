// -------------------------------------------------------------------------------------------------
// Copyright (c) Johan Boström. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// -------------------------------------------------------------------------------------------------
//
// Code from:
// https://www.johanbostrom.se/blog/adding-basic-auth-to-your-mvc-application-in-dotnet-core
//
namespace CodecControl.Web.Security
{
    using System;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;


    public class BasicAuthorizeFilter : IAuthorizationFilter
    {
        private readonly ApplicationSettings _appSettings;

        public BasicAuthorizeFilter(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string authHeader = context.HttpContext.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];
                if (IsAuthorized(username, password))
                {
                    return;
                }
            }

            // Return authentication type (causes browser to show login dialog)
            context.HttpContext.Response.Headers["WWW-Authenticate"] = "Basic";

            context.Result = new UnauthorizedResult();
        }

        public bool IsAuthorized(string username, string password)
        {
            return username.Equals(_appSettings.AuthenticatedUserName, StringComparison.InvariantCultureIgnoreCase)
                   && password.Equals(_appSettings.AuthenticatedPassword);
        }
    }
}