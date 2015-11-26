﻿using Infra.Authentications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Security.Claims;
using System.Threading;

namespace Infra.Authentications.Identity.Services
{
    public class Authenticator : IAuthenticator
    {
        public IPAddress ClientIP {
            get
            {
                IPAddress address;
                if (IPAddress.TryParse(HttpContext.Current.Request.UserHostAddress, out address))
                    return address;

                return IPAddress.None;
            }
        }

        public string ImpersonatorId
        {
            get
            {
                if (!IsAuthenticated)
                    return null;

                return Identity.GetImpersonatorId();
            }
        }

        public string UserId
        {
            get
            {
                if (!IsAuthenticated)
                    return null;

                return Identity.Name;
            }
        }

        ClaimsPrincipal Principal => Thread.CurrentPrincipal as ClaimsPrincipal;
        ClaimsIdentity Identity => Principal?.Identity as ClaimsIdentity;
        bool IsAuthenticated => Identity?.IsAuthenticated ?? false;

    }
}