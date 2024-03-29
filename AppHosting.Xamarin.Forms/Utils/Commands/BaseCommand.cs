﻿using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using Xamarin.Forms;
using WeakEventManager = AsyncAwaitBestPractices.WeakEventManager;

namespace AppHosting.Xamarin.Forms.Utils.Commands
{
#nullable enable
    /// <summary>
    /// Abstract Base Class for AsyncCommand and AsyncValueCommand
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class BaseCommand<TCanExecute>
    {
        private readonly Func<TCanExecute, bool> _canExecute;
        private readonly WeakEventManager _weakEventManager = new();
        protected long _isExecuting;

        /// <summary>
        /// Initializes BaseCommand
        /// </summary>
        /// <param name="canExecute"></param>
        protected private BaseCommand(Func<TCanExecute, bool>? canExecute) =>
            _canExecute = canExecute ?? (_ => true);

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => _weakEventManager.AddEventHandler(value);
            remove => _weakEventManager.RemoveEventHandler(value);
        }

        /// <summary>
        /// Determines whether the command can execute in its current state
        /// </summary>
        /// <returns><c>true</c>, if this command can be executed; otherwise, <c>false</c>.</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(TCanExecute parameter)
        {
            if (Interlocked.Read(ref _isExecuting) != 0)
                return false;
            return _canExecute(parameter);
        }

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            Device.BeginInvokeOnMainThread(() =>
                _weakEventManager.RaiseEvent(this, EventArgs.Empty, nameof(CanExecuteChanged)));

        protected static bool IsNullable<T>()
        {
            var type = typeof(T);

            if (!type.GetTypeInfo().IsValueType)
                return true; // ref-type

            if (Nullable.GetUnderlyingType(type) != null)
                return true; // Nullable<T>

            return false; // value-type
        }
    }
#nullable disable
}