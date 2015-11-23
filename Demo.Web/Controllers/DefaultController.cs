﻿using Demo.Web.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Events;
using Demo.Web.Test.Services;
using Logs;
using System.Diagnostics.Contracts;
using System.Web;

namespace Demo.Web.Controllers
{    
    public class DefaultController : ApiController
    {
        public DefaultController(ILog log)
        {
            Contract.Requires<ArgumentNullException>(log != null);
            Contract.Ensures(Log != null);
            Log = log;
        }

        ILog Log { get; }

        class MyEvent : ILoggable
        {
            public int MyProperty { get; set; }
        }

        public async Task<IEnumerable<ILogMessage<object, Exception>>> Get()
        {            
            return Log.Read(new LogQuery());
        }
    }
}