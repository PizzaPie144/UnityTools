using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;

/// <summary>
/// Contains utilities usable in Unity Editor.
/// </summary>
namespace PizzaPie.Editor.Util
{
    /// <summary>
    /// Utility class for handling SerializedProperties. 
    /// Array refers to serializable (by Unity's Serializer) collections (List, Array).
    /// </summary>
    public static class SerializedProperties 
    {
        /// <summary>
        /// Get Unity Object at index.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        /// <param name="serializedProperty">SerializedProperty array.</param>
        /// <returns>Element at given index.</returns>
        public static UnityEngine.Object GetArrayElementAt(int index, SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                throw new ArgumentException("Serialized Property must represent an Array.");

            if (index > serializedProperty.arraySize)
                throw new IndexOutOfRangeException();

            var elementSerializedProperty = serializedProperty.GetArrayElementAtIndex(index);
            return elementSerializedProperty.objectReferenceValue;
        }

        /// <summary>
        /// Add Unity Object to Serialized Property array.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="serializedProperty">Serialized Property array.</param>
        public static void AddArrayItem(UnityEngine.Object item, SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                throw new ArgumentException("Serialized Property must represent an Array.");

            Type elementType = GetElementType(serializedProperty);

            if (item != null && elementType != item.GetType() && item.GetType().IsSubclassOf(elementType))
                throw new ArgumentException("Trying to add invalid item type, make sure the item's type match array's elements type or inherits from it.");

            for (int i = 0; i < serializedProperty.arraySize; i++)
            {
                if (GetArrayElementAt(i, serializedProperty) == item)
                    return;
            }

            serializedProperty.arraySize++;
            var elementSerializedProperty = serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
            elementSerializedProperty.objectReferenceValue = item;
        }

        /// <summary>
        /// Remove item from Serialized Property array.
        /// </summary>
        /// <param name="item">Element to remove.</param>
        /// <param name="serializedProperty">Arrat SerializedProperty.</param>
        public static void RemoveArrayItem(UnityEngine.Object item, SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                throw new ArgumentException("Serialized Property must represent an Array.");

            Type elementType = GetElementType(serializedProperty);

            if (item != null && elementType != item.GetType() && item.GetType().IsSubclassOf(elementType))
                throw new ArgumentException("Trying to remove invalid item type, make sure the item's type match array's elements type or inherits from it.");

            for (int i = 0; i < serializedProperty.arraySize; i++)
            {
                if (item == GetArrayElementAt(i, serializedProperty))
                {
                    var elementSerializedProperty = serializedProperty.GetArrayElementAtIndex(i);
                    elementSerializedProperty.objectReferenceValue = null;

                    for (int j = i; j < serializedProperty.arraySize - 1; j++)
                    {
                        var elementProperty1 = serializedProperty.GetArrayElementAtIndex(j);
                        var elementProperty2 = serializedProperty.GetArrayElementAtIndex(j + 1);
                        elementProperty1.objectReferenceValue = elementProperty2.objectReferenceValue;
                        elementProperty2 = null;
                    }
                    break;
                }
            }
            serializedProperty.arraySize--;
        }

        /// <summary>
        /// Get the Type of array's elements.
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <returns>Type of elements.</returns>
        public static Type GetElementType(SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                throw new ArgumentException("Serialized Property must represent an Array.");

            Type type = serializedProperty.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = type.GetField(serializedProperty.propertyPath,BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo.FieldType.HasElementType)
                type = fieldInfo.FieldType.GetElementType();
            else
                type = fieldInfo.FieldType.GetGenericArguments()[0];

            return type;
        }

        /// <summary>
        /// Doesn't work... have to account for assembly name and namespace
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <returns></returns>
        public static Type GetElementTypeFromString(SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                throw new ArgumentException("Serialized Property must represent an Array.");

            string typeString = serializedProperty.arrayElementType;
            Regex regex = new Regex(@"(?<=\$).+[^\>]");
            Match match = regex.Match(typeString);

            if (!match.Success)
                throw new InvalidFilterCriteriaException("Regex could not match Type string in SerializedProperty.arrayElementType");

            typeString += ", " + Assembly.GetExecutingAssembly().FullName;
            return Type.GetType(typeString);
        }
    }
}
