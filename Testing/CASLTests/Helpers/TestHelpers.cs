// <copyright file="TestHelpers.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable UnusedMember.Global
namespace CASLTests.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Shouldly;

/// <summary>
/// Provides testing helpers.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Asserts that the thrown <see cref="ArgumentException"/> has a message that matches the expected dotnet
    /// empty string parameter exception message.
    /// </summary>
    /// <param name="exception">The exception to assert against.</param>
    /// <param name="paramName">The name of the parameter that is being asserted.</param>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Ignored for possible future use.")]
    public static void WithEmptyStringParamMsg(this ArgumentException exception, string paramName) =>
        exception.Message.ShouldBe($"The value cannot be an empty string. (Parameter '{paramName}')");

    /// <summary>
    /// Asserts that the thrown <see cref="ArgumentNullException"/> has a message that matches the expected dotnet
    /// null parameter exception message.
    /// </summary>
    /// <param name="exception">The exception to assert against.</param>
    /// <param name="paramName">The name of the parameter that is being asserted.</param>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Ignored for possible future use.")]
    public static void WithNullParamMsg(this ArgumentNullException exception, string paramName) =>
        exception.Message.ShouldBe($"Value cannot be null. (Parameter '{paramName}')");

    /// <summary>
    /// Sets the value of a private field that matches the given <paramref name="fieldName"/> to the
    /// given <paramref name="value"/>.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The value to set the field to.</param>
    /// <typeparam name="TEnum">The type of enumeration of the field.</typeparam>
    public static void SetEnumFieldValue<TEnum>(this object fieldContainer, string fieldName, TEnum value)
        where TEnum : Enum
    {
        fieldContainer.ShouldNotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Length.ShouldBeGreaterThan(0, "no enum fields exist in the object.");

        var enumField = Array.Find(allEnumFields, f => f.FieldType.IsEnum && f.Name == fieldName);

        enumField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");

        enumField.SetValue(fieldContainer, value);
    }

    /// <summary>
    /// Sets an array field with a name that matches the given <paramref name="fieldName"/> to the value of null.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <typeparam name="TElements">The type of the field array's elements.</typeparam>
    public static void SetArrayFieldToNull<TElements>(this object fieldContainer, string fieldName)
    {
        fieldContainer.ShouldNotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Length.ShouldBeGreaterThan(0, "no enum fields exist in the object.");

        var foundField = Array.Find(allEnumFields, f => f.Name == fieldName);

        foundField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        foundField.FieldType.IsArray.ShouldBeTrue("the field is not an array.");
        foundField.FieldType.GetElementType().ShouldBe(typeof(TElements), $"the array's elements are not of type {typeof(TElements)}");

        foundField.SetValue(fieldContainer, null);
    }

    /// <summary>
    /// Sets a field with a name that matches the given <paramref name="fieldName"/> to the value of null.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    public static void SetFieldToNull(this object fieldContainer, string fieldName)
    {
        fieldContainer.ShouldNotBeNull("setting the field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("setting the field value requires a non-empty or null field name.");

        var fields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        fields.Length.ShouldBeGreaterThan(0, "no fields exist in the object.");

        var foundField = Array.Find(fields, f => f.Name == fieldName);

        foundField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        foundField.FieldType.IsValueType.ShouldBeFalse("the field is not a value type.");

        foundField.SetValue(fieldContainer, null);
    }

    /// <summary>
    /// Sets a field with a name that matches the given <paramref name="fieldName"/> to the value of null.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The value to set the field to.</param>
    /// <typeparam name="T">The type of parameter.</typeparam>
    public static void SetFieldValue<T>(this object fieldContainer, string fieldName, T value)
    {
        fieldContainer.ShouldNotBeNull("setting the field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("setting the field value requires a non-empty or null field name.");

        var fields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        fields.Length.ShouldBeGreaterThan(0, "no fields exist in the object.");

        var foundField = Array.Find(fields, f => f.Name == fieldName);

        foundField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        foundField.FieldType.ShouldBe(typeof(T), "the generic type should match the actual field type.");

        foundField.SetValue(fieldContainer, value);
    }

    /// <summary>
    /// Gets the boolean field value with a name that matches the given <paramref name="fieldName"/> inside of the object
    /// <paramref name="fieldContainer"/>.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>The boolean value.</returns>
    public static bool GetBoolFieldValue(this object fieldContainer, string fieldName)
    {
        fieldContainer.ShouldNotBeNull("getting the bool field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("getting the bool field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        allEnumFields.Length.ShouldBeGreaterThan(0, "no fields exist in the object.");

        var foundField = Array.Find(allEnumFields, f => f.Name == fieldName);

        foundField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        foundField.FieldType.ShouldBe(typeof(bool), "the field is not a boolean type.");

        return foundField.GetValue(fieldContainer) as bool? ?? false;
    }

    /// <summary>
    /// Sets an array field with a name that matches the given <paramref name="fieldName"/> to the value of null.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The new private field value.</param>
    public static void SetBoolField(this object fieldContainer, string fieldName, bool value)
    {
        fieldContainer.ShouldNotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Length.ShouldBeGreaterThan(0, "no enum fields exist in the object.");

        var foundField = Array.Find(allEnumFields, f => f.Name == fieldName);

        foundField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        foundField.FieldType.ShouldBe(typeof(bool), "the field is not a boolean type.");

        foundField.SetValue(fieldContainer, value);
    }

    /// <summary>
    /// Gets the boolean field value with a name that matches the given <paramref name="fieldName"/> inside of the object
    /// <paramref name="fieldContainer"/>.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>The struct field.</returns>
    /// <typeparam name="T">The type of field value.</typeparam>
    public static T GetStructFieldValue<T>(this object fieldContainer, string fieldName)
        where T : struct
    {
        fieldContainer.ShouldNotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Length.ShouldBeGreaterThan(0, "no enum fields exist in the object.");

        var foundField = Array.Find(allEnumFields, f => f.Name == fieldName);

        foundField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        foundField.FieldType.IsValueType.ShouldBeTrue("the field is not a value type.");

        var fieldValue = (T)(foundField.GetValue(fieldContainer) ?? default(T));

        return fieldValue;
    }

    /// <summary>
    /// Sets a field with a name that matches the given <paramref name="fieldName"/> to the given <paramref name="value"/>.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The value to set the struct field to.</param>
    /// <typeparam name="T">The struct value.</typeparam>
    public static void SetStructFieldValue<T>(this object fieldContainer, string fieldName, T value)
        where T : struct
    {
        fieldContainer.ShouldNotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.ShouldNotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Length.ShouldBeGreaterThan(0, "no enum fields exist in the object.");

        var foundField = Array.Find(allEnumFields, f => f.Name == fieldName);

        foundField.ShouldNotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        foundField.FieldType.IsValueType.ShouldBeTrue("the field is not a value type.");

        foundField.SetValue(fieldContainer, value);
    }
}
