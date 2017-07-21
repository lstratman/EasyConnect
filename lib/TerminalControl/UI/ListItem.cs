/*
 * Copyright 2012 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ListItem.cs,v 1.2 2012/03/11 14:28:53 kzmi Exp $
 */
using System;

namespace Poderosa.UI {

    /// <summary>
    /// List item object.
    /// </summary>
    public class ListItem<T> {

        private readonly T _value;
        private readonly string _text;

        /// <summary>
        /// Gets item value.
        /// </summary>
        public T Value {
            get {
                return _value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">A value</param>
        /// <param name="text">A text to be displayed in the list.</param>
        public ListItem(T value, string text) {
            _value = value;
            _text = text;
        }

        /// <summary>
        /// Overrides to display a custom text in the list.
        /// </summary>
        public override string ToString() {
            return _text;
        }

        /// <summary>
        /// Overrides so that an instance of T can be matched with this object.
        /// </summary>
        /// <remarks>
        /// By using this feature, selection of the List/ComboBox control can be set like this:
        /// <code>
        /// X value1 = ...;
        /// X value2 = ...;
        /// list.Items.Add(new ListItem&lt;X&gt;(value1, "Value 1"));
        /// list.Items.Add(new ListItem&lt;X&gt;(value2, "Value 2"));
        /// list.SelectedItem = value2;
        /// </code>
        /// </remarks>
        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            ListItem<T> listItem = obj as ListItem<T>;
            if (listItem != null) {
                return this.Value.Equals(listItem.Value);
            }
            if (obj.GetType() == typeof(T)) {
                return this.Value.Equals((T)obj);
            }
            return false;
        }

        /// <summary>
        /// Overrides so that an instance of T can be matched with this object.
        /// </summary>
        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }
    }
}
