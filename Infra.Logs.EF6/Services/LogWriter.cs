﻿using Infra.Authentications;
using Infra.Events;
using Infra.Logs.EF6.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Logs.EF6.Services
{
    public class LogWriter : 
        ILogWriter,
        IHandler<ILoggable>,
        IHandler<ICustomLoggable>,
        IHandler<Exception>,
        IHandler<If<ILoggable<Unhandled>, Unhandled>>,
        IHandler<If<ILoggable<Succeeded>, Succeeded>>,
        IHandler<If<ILoggable<Failed>, Failed>>
    {
        public LogWriter(IAuthenticator authenticator, IClock clock)
        {
            Authenticator = authenticator;
            Clock = clock;
        }

        IAuthenticator Authenticator { get; }
        IClock Clock { get; }

        public void Write<E, EX>(E e, EX ex) where EX : Exception
        {
            using (var ctx = new LogContext())
            {
                ctx.Messages.Add(new ELogMessage
                {
                    LoggedAt = Clock.GetTime(),
                    UserId = Authenticator.UserId,
                    ImpersonatorId = Authenticator.ImpersonatorId,
                    ClientIP = Authenticator.ClientIP.ToString(),

                    EventJson = e.ToJson(),
                    EventTypes = new ImplementedTypes(e)
                        .Select(t => ELogType(ctx, t))
                        .ToList(),

                    ExceptionJson = ex.ToJson(),
                    ExceptionTypes = new ImplementedTypes(ex)
                        .Select(t => ELogType(ctx, t))
                        .ToList()
                });

                ctx.SaveChanges();
            }
        }

        ELogType ELogType(LogContext contex, Type type)
        {
            var eLogType = contex.Types.FirstOrDefault(t => t.Name == type.FullName);
            if (eLogType == null)
                eLogType = contex.Types.Add(new ELogType { Name = type.FullName });

            return eLogType;
        }

        public async Task<bool> HandleAsync(If<ILoggable<Succeeded>, Succeeded> e)
        {
            Write<object, Exception>(e.Subject, null);
            return true;
        }

        public async Task<bool> HandleAsync(If<ILoggable<Unhandled>, Unhandled> e)
        {
            Write(e.Subject, new NotImplementedException($"Event {e.GetType()} is unhandled."));
            return true;
        }

        public async Task<bool> HandleAsync(If<ILoggable<Failed>, Failed> e)
        {
            Write<object, Exception>(e.Subject, e.Exception);
            return true;
        }

        public async Task<bool> HandleAsync(ILoggable e)
        {
            Write<object, Exception>(e, null);
            return true;
        }

        public async Task<bool> HandleAsync(ICustomLoggable e)
        {
            Write(e.Event, e.Exception);
            return true;
        }

        public async Task<bool> HandleAsync(Exception e)
        {
            Write<object, Exception>(null, e);
            return true;
        }
    }
}
