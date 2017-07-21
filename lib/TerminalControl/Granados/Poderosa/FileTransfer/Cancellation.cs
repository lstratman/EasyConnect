/*
 * Copyright 2012 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Cancellation.cs,v 1.1 2012/05/05 12:42:45 kzmi Exp $
 */

namespace Granados.Poderosa.FileTransfer {

    /// <summary>
    /// A class to request a cancellation.
    /// </summary>
    public class Cancellation {

        #region Private fields

        private bool _requested = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether a cancellation has been requested.
        /// </summary>
        public bool IsRequested {
            get {
                return _requested;
            }
        }
        
        #endregion Properties

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Cancellation() {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Request cancellation.
        /// </summary>
        public void Cancel() {
            _requested = true;
        }

        #endregion
    }
}
