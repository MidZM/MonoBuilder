using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoBuilder.Utils
{

    /// <summary>
    /// Provides a registry for creating, querying, and managing Windows Forms controls by name and scope.
    /// </summary>
    public static class Elements
    {
        private static readonly Dictionary<string, Control> _registry = new();
        private static readonly Dictionary<Form, List<string>> _formKeys = new();

        public static T Create<T>(string name, Action<T>? initialize = null) where T : Control, new()
        {
            T control = new T { Name = name };
            initialize?.Invoke(control);

            if (_registry.TryGetValue(name, out var existing))
                existing.Dispose();

            _registry[name] = control;

            return control;
        }

        /// <summary>
        /// Retrieves a control of the specified type from the registry by name.
        /// </summary>
        /// <typeparam name="T">The type of control to retrieve.</typeparam>
        /// <param name="name">The name of the control to query.</param>
        /// <returns>The control of type T if found; otherwise, null.</returns>
        public static T? Query<T>(string name) where T : Control
        {
            if (_registry.TryGetValue(name, out var control))
                return control as T;

            return null;
        }

        /// <summary>
        /// Retrieves all registered controls of the specified type that have a tag matching the provided value.
        /// </summary>
        /// <typeparam name="T">The type of control to query. Must inherit from Control.</typeparam>
        /// <param name="tag">The tag value to match.</param>
        /// <returns>A collection of controls of type T with the specified tag.</returns>
        public static IEnumerable<T> QueryAllByTag<T>(string tag) where T : Control
        {
            return _registry.Values
                .OfType<T>()
                .Where(c => c.Tag?.ToString() == tag)
                .ToList();
        }

        /// <summary>
        /// Removes and disposes the control associated with the specified name from the registry.
        /// </summary>
        /// <param name="name">The name of the control to remove.</param>
        public static void Remove(string name)
        {
            if (_registry.TryGetValue(name, out var control))
            {
                control.Dispose();
                _registry.Remove(name);
            }
        }


        /// <summary>
        /// Registers a form to track its associated scope and handles cleanup when the form is closed.
        /// </summary>
        /// <param name="form">The form to register for scope tracking.</param>
        public static void RegisterScope(Form form)
        {
            if (_formKeys.ContainsKey(form)) return;

            _formKeys[form] = new List<string>();
            form.FormClosed += OnFormClosed;
        }

        /// <summary>
        /// Creates a new control of the specified type, associates it with the given form, and optionally initializes
        /// it with an invoker.
        /// </summary>
        /// <typeparam name="T">The type of control to create.</typeparam>
        /// <param name="scope">The form to associate with the control.</param>
        /// <param name="name">The unique name for the control.</param>
        /// <param name="initialize">An optional action to initialize the control.</param>
        /// <returns>A new instance of the specified control type.</returns>
        public static T Create<T>(Form scope, string name, Action<T>? initialize = null) where T : Control, new()
        {
            var control = Create<T>(name, initialize);

            if (!_formKeys.TryGetValue(scope, out var keys))
            {
                keys = new List<string>();
                _formKeys[scope] = keys;
                scope.FormClosed += OnFormClosed;
            }

            keys.Add(name);
            return control;
        }

        /// <summary>
        /// Handles cleanup of associated controls and event handlers when a form is closed.
        /// </summary>
        /// <param name="sender">The form that triggered the event.</param>
        /// <param name="e">The event data for the form closed event.</param>
        private static void OnFormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is not Form form) return;

            if (_formKeys.TryGetValue(form, out var keys))
            {
                foreach (var key in keys)
                {
                    if (_registry.TryGetValue(key, out var control))
                    {
                        control.Dispose();
                        _registry.Remove(key);
                    }
                }
                _formKeys.Remove(form);
            }

            form.FormClosed -= OnFormClosed;
        }

        /// <summary>Disposes and removes all registered controls. Used as a full reset.</summary>
        public static void Clear()
        {
            foreach (Control control in _registry.Values)
                control.Dispose();

            _registry.Clear();
            _formKeys.Clear();
        }
    }
}