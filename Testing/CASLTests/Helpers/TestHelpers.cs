// <copyright file="TestHelpers.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable UnusedMember.Global
namespace CASLTests.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Primitives;
using FluentAssertions.Specialized;

/// <summary>
/// Provides testing helpers.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Asserts that the current <see cref="Delegate" /> throws an exception of type <see name="ArgumentException"/>.
    /// </summary>
    /// <param name="actionAssertions">
    /// Contains a number of methods to assert that an <see cref="Action"/> yields the expected result.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <returns>The exception assertion object.</returns>
    public static ExceptionAssertions<ArgumentException> ThrowArgException(
        this ActionAssertions actionAssertions,
        string because = "",
        params object[] becauseArgs) =>
        actionAssertions.Throw<ArgumentException>(because, becauseArgs);

    /// <summary>
    /// Asserts that the current <see cref="Delegate" /> throws an exception of type <see name="ArgumentException"/>.
    /// </summary>
    /// <param name="functionAssertions">
    /// Contains a number of methods to assert that a synchronous function yields the expected result.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <typeparam name="T">The type returned by the <see cref="Func{T}"/>.</typeparam>
    /// <returns>The exception assertion object.</returns>
    public static ExceptionAssertions<ArgumentException> ThrowArgException<T>(
        this FunctionAssertions<T> functionAssertions,
        string because = "",
        params object[] becauseArgs) =>
        functionAssertions.Throw<ArgumentException>(because, becauseArgs);

    /// <summary>
    /// Asserts that the current <see cref="Delegate" /> throws an exception of type <see name="ArgumentNullException"/>.
    /// </summary>
    /// <param name="actionAssertions">
    /// Contains a number of methods to assert that an <see cref="Action"/> yields the expected result.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <returns>The exception assertion object.</returns>
    public static ExceptionAssertions<ArgumentNullException> ThrowArgNullException(
        this ActionAssertions actionAssertions,
        string because = "",
        params object[] becauseArgs) =>
        actionAssertions.Throw<ArgumentNullException>(because, becauseArgs);

    /// <summary>
    /// Asserts that the current <see cref="Delegate" /> throws an exception of type <see name="ArgumentNullException"/>.
    /// </summary>
    /// <param name="functionAssertions">
    /// Contains a number of methods to assert that a synchronous function yields the expected result.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <typeparam name="T">The type returned by the <see cref="Func{T}"/>.</typeparam>
    /// <returns>The exception assertion object.</returns>
    public static ExceptionAssertions<ArgumentNullException> ThrowArgNullException<T>(
        this FunctionAssertions<T> functionAssertions,
        string because = "",
        params object[] becauseArgs) =>
        functionAssertions.Throw<ArgumentNullException>(because, becauseArgs);

    /// <summary>
    /// Asserts that the thrown exception has a message that matches the expected dotnet empty string parameter exception message.
    /// </summary>
    /// <param name="exAssertion">
    /// Contains a number of methods to assert that an <see cref="Exception" /> is in the correct state.
    /// </param>
    /// <param name="paramName">The name of the parameter that is being asserted.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
    /// </param>
    /// <returns>The exception assertion object.</returns>
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Ignored for possible future use.")]
    public static ExceptionAssertions<ArgumentException> WithEmptyStringParamMsg(
        this ExceptionAssertions<ArgumentException> exAssertion,
        string paramName,
        string because = "",
        params object[] becauseArgs) =>
        exAssertion.WithMessage($"The value cannot be an empty string. (Parameter '{paramName}')", because, becauseArgs);

    /// <summary>
    /// Asserts that the thrown exception has a message that matches the expected dotnet null parameter exception message.
    /// </summary>
    /// <param name="exAssertion">
    /// Contains a number of methods to assert that an <see cref="Exception" /> is in the correct state.
    /// </param>
    /// <param name="paramName">The name of the parameter that is being asserted.</param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
    /// </param>
    /// <returns>The exception assertion object.</returns>
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Ignored for possible future use.")]
    public static ExceptionAssertions<ArgumentNullException> WithNullParamMsg(
        this ExceptionAssertions<ArgumentNullException> exAssertion,
        string paramName,
        string because = "",
        params object[] becauseArgs) =>
        exAssertion.WithMessage($"Value cannot be null. (Parameter '{paramName}')", because, becauseArgs);

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
        fieldContainer.Should().NotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.Should().NotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Should().HaveCountGreaterThan(0, $"no enum fields exist in the object.");

        var enumField = Array.Find(allEnumFields, f => f.FieldType.IsEnum && f.Name == fieldName);

        enumField.Should().NotBeNull($"a field with the name '{fieldName}' does not exist in the object.");

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
        fieldContainer.Should().NotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.Should().NotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Should().HaveCountGreaterThan(0, $"no enum fields exist in the object.");

        var arrayField = Array.Find(allEnumFields, f => f.Name == fieldName);

        arrayField.Should().NotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        arrayField.FieldType.IsArray.Should().BeTrue("the field is not an array.");
        arrayField.FieldType.GetElementType().Should().Be(typeof(TElements), $"the array's elements are not of type {typeof(TElements)}");

        arrayField.SetValue(fieldContainer, null);
    }

    /// <summary>
    /// Sets an array field with a name that matches the given <paramref name="fieldName"/> to the value of null.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    public static void SetFieldToNull(this object fieldContainer, string fieldName)
    {
        fieldContainer.Should().NotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.Should().NotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Should().HaveCountGreaterThan(0, $"no enum fields exist in the object.");

        var arrayField = Array.Find(allEnumFields, f => f.Name == fieldName);

        arrayField.Should().NotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        arrayField.FieldType.IsValueType.Should().BeFalse("the field is not a value type.");

        arrayField.SetValue(fieldContainer, null);
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
        fieldContainer.Should().NotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.Should().NotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Should().HaveCountGreaterThan(0, $"no enum fields exist in the object.");

        var arrayField = Array.Find(allEnumFields, f => f.Name == fieldName);

        arrayField.Should().NotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        arrayField.FieldType.Should().Be(typeof(bool), "the field is not a boolean type.");

        return arrayField.GetValue(fieldContainer) as bool? ?? false;
    }

    /// <summary>
    /// Sets an array field with a name that matches the given <paramref name="fieldName"/> to the value of null.
    /// </summary>
    /// <param name="fieldContainer">The object that contains the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The new private field value.</param>
    public static void SetBoolField(this object fieldContainer, string fieldName, bool value)
    {
        fieldContainer.Should().NotBeNull("setting the enum field value of a null object is not possible.");
        fieldName.Should().NotBeNullOrEmpty("setting an enum field value requires a non-empty or null field name.");

        var allEnumFields = fieldContainer.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        allEnumFields.Should().HaveCountGreaterThan(0, $"no enum fields exist in the object.");

        var arrayField = Array.Find(allEnumFields, f => f.Name == fieldName);

        arrayField.Should().NotBeNull($"a field with the name '{fieldName}' does not exist in the object.");
        arrayField.FieldType.Should().Be(typeof(bool), "the field is not a boolean type.");

        arrayField.SetValue(fieldContainer, value);
    }
}
