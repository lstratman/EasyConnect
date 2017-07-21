/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: EnumDescription.cs,v 1.9 2012/03/18 10:57:33 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Poderosa.Util {

    /// <summary>
    /// Attribute to define the description of an enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumValueAttribute : Attribute {
        private string _description;

        /// <summary>
        /// Key string to get a descripton text from a string resource.
        /// </summary>
        public string Description {
            get {
                return _description;
            }
            set {
                _description = value;
            }
        }
    }

    /// <summary>
    /// Represents a value of an enum type.
    /// </summary>
    /// <typeparam name="T">Type of enum</typeparam>
    public class EnumListItem<T> {
        private readonly T _value;
        private readonly string _text;

        /// <summary>
        /// Gets an enum value.
        /// </summary>
        public T Value {
            get {
                return _value;
            }
        }

        /// <summary>
        /// Gets a string representing an enum value.
        /// </summary>
        public string Text {
            get {
                return _text;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">an enum value.</param>
        /// <param name="text">a localized name of the enum value.</param>
        public EnumListItem(T value, string text) {
            _value = value;
            _text = text;
        }

        /// <summary>
        /// Overrides to display localized name of the enum value.
        /// </summary>
        /// <returns>localized name of the enum value.</returns>
        public override string ToString() {
            return _text;
        }

        /// <summary>
        /// Overrides to check equality based on the enum value.
        /// </summary>
        /// <remarks>
        /// By using this feature, selection of the List/ComboBox control can be set like this:
        /// <code>
        /// X value1 = ...;
        /// X value2 = ...;
        /// list.Items.Add(new EnumListItem&lt;X&gt;(value1, "Value 1"));
        /// list.Items.Add(new EnumListItem&lt;X&gt;(value2, "Value 2"));
        /// list.SelectedItem = value2;
        /// </code>
        /// </remarks>
        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            EnumListItem<T> listItem = obj as EnumListItem<T>;
            if (listItem != null) {
                return this.Value.Equals(listItem.Value);
            }
            if (obj.GetType() == typeof(T)) {
                return this.Value.Equals((T)obj);
            }
            return false;
        }

        /// <summary>
        /// Overrides to check equality based on the enum value.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Create single instance from an enum value.
        /// </summary>
        /// <param name="value">an enum value</param>
        /// <returns>new instance</returns>
        public static EnumListItem<T> CreateListItem(T value) {
            return CreateListItemInternal(false, value);
        }

        /// <summary>
        /// Create single instance from an enum value.
        /// Text of an instance is a key of the string resource.
        /// </summary>
        /// <param name="value">an enum value</param>
        /// <returns>new instance</returns>
        public static EnumListItem<T> CreateListItemWithTextID(T value) {
            return CreateListItemInternal(true, value);
        }

        /// <summary>
        /// Create single instance from an enum value.
        /// </summary>
        /// <param name="keyText">Text of an instance is a key of the string resource.</param>
        /// <param name="value">an enum value</param>
        /// <returns>new instance</returns>
        private static EnumListItem<T> CreateListItemInternal(bool keyText, T value) {
            Type enumType = typeof(T);
            FieldInfo field = enumType.GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                throw new ArgumentException("Not a member of enum " + enumType.Name, "value");

            string desc = null;

            string descId = GetDescriptionID(field);
            if (descId != null) {
                if (keyText) {
                    desc = descId;
                }
                else {
                    string textFound;
                    StringResource res = FindStringResource(enumType, descId, out textFound);
                    if (res != null) {
                        desc = textFound;
                    }
                }
            }

            if (desc == null) {
                // fallback
                desc = field.Name;
            }

            return new EnumListItem<T>(value, desc);
        }

        /// <summary>
        /// Creates an array of list item according to the enum values.
        /// </summary>
        /// <returns>an array of list item.</returns>
        public static EnumListItem<T>[] GetListItems() {
            return GetListItemsInternal(false);
        }

        /// <summary>
        /// Creates an array of list item according to the enum values.
        /// Text of an instance is a key of the string resource.
        /// </summary>
        /// <returns>an array of list item.</returns>
        public static EnumListItem<T>[] GetListItemsWithTextID() {
            return GetListItemsInternal(true);
        }

        /// <summary>
        /// Creates an array of list item according to the enum values.
        /// </summary>
        /// <param name="excludeValues">Enum values to exclude.</param>
        /// <returns>an array of list item.</returns>
        public static EnumListItem<T>[] GetListItemsExcept(params T[] excludeValues) {
            return GetListItemsInternal(false, excludeValues);
        }

        /// <summary>
        /// Creates an array of list item according to the enum values.
        /// </summary>
        /// <param name="keyText">Text of an instance is a key of the string resource.</param>
        /// <param name="excludeValues">Enum values to exclude.</param>
        /// <returns>an array of list item.</returns>
        private static EnumListItem<T>[] GetListItemsInternal(bool keyText, params T[] excludeValues) {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("Not an enum type: " + enumType.Name, "T");

            List<EnumListItem<T>> enumValues = new List<EnumListItem<T>>();

            StringResource stringResource = null;

            foreach (FieldInfo field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static)) {
                T val = (T)field.GetValue(null);
                if (Array.IndexOf<T>(excludeValues, val) >= 0)
                    continue;

                string desc = null;

                string descId = GetDescriptionID(field);
                if (descId != null) {
                    if (keyText) {
                        desc = descId;
                    }
                    else {
                        // find localized text
                        if (stringResource != null) {
                            desc = stringResource.GetString(descId);
                        }
                        else {
                            string textFound;
                            StringResource res = FindStringResource(enumType, descId, out textFound);
                            if (res != null) {
                                stringResource = res;
                                desc = textFound;
                            }
                        }
                    }
                }

                if (desc == null) {
                    // fallback
                    desc = field.Name;
                }

                enumValues.Add(new EnumListItem<T>(val, desc));
            }

            return enumValues.ToArray();
        }

        private static string GetDescriptionID(FieldInfo field) {
            object[] attrs = field.GetCustomAttributes(typeof(EnumValueAttribute), false);
            if (attrs.Length > 0) {
                EnumValueAttribute enumValueAttr = (EnumValueAttribute)attrs[0];
                return enumValueAttr.Description;
            }
            return null;
        }

        private static StringResource FindStringResource(Type enumType, string resourceId, out string textFound) {
            Assembly assembly = enumType.Assembly;
            foreach (StringResource res in StringResource.GetStringResourceEnumerable(assembly)) {
                string text = res.GetString(resourceId);
                if (text != null) {
                    textFound = text;
                    return res;
                }
            }
            textFound = null;
            return null;
        }
    }

}