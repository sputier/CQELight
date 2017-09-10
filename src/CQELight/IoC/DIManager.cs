﻿using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.IoC
{
    /// <summary>
    /// Dependency injection manager.
    /// </summary>
    public static class DIManager
    {

        #region Static members

        /// <summary>
        /// Type resolver.
        /// </summary>
        private static IScopeFactory _scopeFactory;
        /// <summary>
        /// Initialization flag.
        /// </summary>
        private static bool _init;

        #endregion

        #region Public static properties

        /// <summary>
        /// Internal accessor to check if DIManager is init.
        /// </summary>
        internal static bool IsInit => _init;

        #endregion

        #region Public static methods

        /// <summary>
        /// Begins a new scope of DIManager.
        /// </summary>
        /// <returns>New instance of scope.</returns>
        public static IScope BeginScope()
            => _scopeFactory.CreateScope();

        /// <summary>
        /// Initialize the DIManager with a scope factory.
        /// </summary>
        /// <param name="scopeFactory">Scope factory to use for init.</param>
        public static void Init(IScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory), "DIManager.Init() : IScopeFactory should be provided.");
            _init = true;
        }

        #endregion

    }
}