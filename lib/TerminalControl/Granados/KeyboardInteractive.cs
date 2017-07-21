// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using System;
using System.Linq;

namespace Granados.KeyboardInteractive {

    /// <summary>
    /// A handler of the keyboard-interactive authentication.
    /// </summary>
    public interface IKeyboardInteractiveAuthenticationHandler {

        /// <summary>
        /// Method for input text in the keyboard-interactive authentication.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <c>prompts</c> and <c>echoes</c> were set as below,
        /// </para>
        /// <code>
        /// prompts = { "User:", "Password:" }
        /// echoes = { true, false }
        /// </code>
        /// <para>
        /// This method should act like below:
        /// </para>
        /// <code>
        /// User:
        /// </code>
        /// <code>
        /// User:myname
        /// </code>
        /// <code>
        /// User:myname
        /// Password:
        /// </code>
        /// <code>
        /// User:myname
        /// Password:mypass ("mypass" is not displayed)
        /// </code>
        /// <para>
        /// Then returns:
        /// </para>
        /// <code>
        /// { "myname", "mypass" }
        /// </code>
        /// </remarks>
        /// <param name="prompts">prompt text of each line.</param>
        /// <param name="echoes">indicates whether the user input should be echoed in each line.</param>
        /// <returns>input texts by user</returns>
        string[] KeyboardInteractiveAuthenticationPrompt(string[] prompts, bool[] echoes);

        /// <summary>
        /// Notifies that the authentication has been started.
        /// </summary>
        void OnKeyboardInteractiveAuthenticationStarted();

        /// <summary>
        /// Notifies that the authentication has been completed.
        /// </summary>
        /// <param name="success">true if the authentication passed.</param>
        /// <param name="error">error information during the authentication. if "success" is true, this argument is null.</param>
        void OnKeyboardInteractiveAuthenticationCompleted(bool success, Exception error);
    }

    /// <summary>
    /// <see cref="IKeyboardInteractiveAuthenticationHandler"/> implementation that returns empty string.
    /// </summary>
    internal class NullKeyboardInteractiveAuthenticationHandler : IKeyboardInteractiveAuthenticationHandler {

        public string[] KeyboardInteractiveAuthenticationPrompt(string[] prompts, bool[] echoes) {
            return prompts.Select(s => string.Empty).ToArray();
        }

        public void OnKeyboardInteractiveAuthenticationStarted() {
        }

        public void OnKeyboardInteractiveAuthenticationCompleted(bool success, Exception error) {
        }
    }
}
