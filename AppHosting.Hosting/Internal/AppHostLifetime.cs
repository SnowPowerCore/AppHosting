﻿using AppHosting.Abstractions.Interfaces;
using AppHosting.Hosting.Extensions;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using System;

namespace AppHosting.Hosting.Internal
{
    /// <summary>
    /// Allows consumers to perform cleanup during a graceful shutdown.
    /// </summary>
    internal class AppHostLifetime : ApplicationLifetime, IAppHostLifetime
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of <see cref="XamarinHostApplicationLifetime"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> used to log messages.</param>
        public AppHostLifetime(ILogger<ApplicationLifetime> logger) : base(logger)
        {
            _logger = logger;
            ApplicationSleeping = new LifecycleRegister();
            ApplicationResuming = new LifecycleRegister();
        }

        /// <summary>
        /// Triggered when the application host has gone to sleep.
        /// </summary>
        public ILifecycleRegister ApplicationSleeping { get; }

        /// <summary>
        /// Triggered when the application host is resuming.
        /// </summary>

        public ILifecycleRegister ApplicationResuming { get; }

        /// <summary>
        /// Signals the ApplicationSleeping event and blocks until it completes.
        /// </summary>
        public void NotifySleeping()
        {
            try
            {
                (ApplicationSleeping as LifecycleRegister).Notify();
            }
            catch (Exception ex)
            {

                _logger.ApplicationError(
                    LoggerEventIds.ApplicationSleepingException,
                    "An error occurred while starting the application",
                    ex);
            }
        }

        /// <summary>
        /// Signals the ApplicationResuming event and blocks until it completes.
        /// </summary>
        public void NotifyResuming()
        {
            try
            {
                (ApplicationResuming as LifecycleRegister).Notify();
            }
            catch (Exception ex)
            {
                _logger.ApplicationError(
                    LoggerEventIds.ApplicationResumingException,
                     "An error occurred resuming the application",
                     ex);
            }
        }
    }
}