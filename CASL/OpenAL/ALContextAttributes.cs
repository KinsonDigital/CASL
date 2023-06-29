// <copyright file="ALContextAttributes.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System;

/// <summary>
/// Convenience class for handling ALContext attributes.
/// </summary>
internal class ALContextAttributes
{
    private int[] additionalAttributes = Array.Empty<int>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ALContextAttributes"/> class.
    /// Leaving all attributes to the driver implementation default values.
    /// </summary>
    public ALContextAttributes()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ALContextAttributes"/> class.
    /// </summary>
    /// <param name="frequency">The mixing output buffer frequency in Hz.</param>
    /// <param name="monoSources">The number of mono sources available. Not guaranteed.</param>
    /// <param name="stereoSources">The number of stereo sources available. Not guaranteed.</param>
    /// <param name="refresh">The refresh interval in Hz.</param>
    /// <param name="sync">If the context is synchronous.</param>
    public ALContextAttributes(int? frequency, int? monoSources, int? stereoSources, int? refresh, bool? sync)
    {
        Frequency = frequency;
        MonoSources = monoSources;
        StereoSources = stereoSources;
        Refresh = refresh;
        Sync = sync;
    }

    /// <summary>
    /// Gets the output buffer frequency in Hz.
    /// This does not actually change any AL state. To apply these attributes see <see cref="ALC.CreateContext(ALDevice, int[])"/>.
    /// </summary>
    public int? Frequency { get; init; }

    /// <summary>
    /// Gets the number of mono sources.
    /// This does not actually change any AL state. To apply these attributes see <see cref="ALC.CreateContext(ALDevice, int[])"/>.
    /// Not guaranteed to get exact number of mono sources when creating a context.
    /// </summary>
    public int? MonoSources { get; init; }

    /// <summary>
    /// Gets the number of stereo sources.
    /// This does not actually change any AL state. To apply these attributes see <see cref="ALC.CreateContext(ALDevice, int[])"/>.
    /// Not guaranteed to get exact number of mono sources when creating a context.
    /// </summary>
    public int? StereoSources { get; init; }

    /// <summary>
    /// Gets the refresh interval in Hz.
    /// This does not actually change any AL state. To apply these attributes see <see cref="ALC.CreateContext(ALDevice, int[])"/>.
    /// </summary>
    public int? Refresh { get; init; }

    /// <summary>
    /// Gets if the context is synchronous.
    /// This does not actually change any AL state. To apply these attributes see <see cref="ALC.CreateContext(ALDevice, int[])"/>.
    /// </summary>
    public bool? Sync { get; init; }

    /// <summary>
    /// Gets or sets additional attributes.
    /// </summary>
    public int[]? AdditionalAttributes
    {
        get => this.additionalAttributes;
        set => this.additionalAttributes = value ?? Array.Empty<int>();
    }

    /// <summary>
    /// Converts these context attributes to a <see cref="ALC.CreateContext(ALDevice, int[])"/> compatible list.
    /// Alternatively, consider using the more convenient <see cref="ALC.CreateContext(ALDevice, int[])"/> overload.
    /// </summary>
    /// <returns>The attribute list in the form of a span.</returns>
    public int[] CreateAttributeArray()
    {
        // The number of members * 2 + additional attributes
        // The '+ 1' is the value of the trailing null byte required at the end
        var totalAttributes = (5 * 2) + this.additionalAttributes.Length + 1;

        var attributeList = new int[totalAttributes];
        var index = 0;

        void AddAttribute(int? value, AlcContextAttributes attribute)
        {
            if (value == null)
            {
                return;
            }

            attributeList[index++] = (int)attribute;
            attributeList[index++] = value.Value;
        }

        AddAttribute(Frequency, AlcContextAttributes.Frequency);
        AddAttribute(MonoSources, AlcContextAttributes.MonoSources);
        AddAttribute(StereoSources, AlcContextAttributes.StereoSources);
        AddAttribute(Refresh, AlcContextAttributes.Refresh);

        if (Sync != null)
        {
            AddAttribute(Sync.Value ? 1 : 0, AlcContextAttributes.Sync);
        }

        if (this.additionalAttributes.Length > 0)
        {
            Array.Copy(this.additionalAttributes, 0, attributeList, index, this.additionalAttributes.Length);
        }

        // Add the trailing null byte.
        attributeList[^1] = 0;

        return attributeList;
    }

    /// <summary>
    /// Converts the attributes to a string representation.
    /// </summary>
    /// <returns>The string representation of the attributes.</returns>
    public override string ToString() => $"{GetAttrNameAndValue(nameof(Frequency), Frequency)}, " +
                                         $"{GetAttrNameAndValue(nameof(MonoSources), MonoSources)}, " +
                                         $"{GetAttrNameAndValue(nameof(StereoSources), StereoSources)}, " +
                                         $"{GetAttrNameAndValue(nameof(Refresh), Refresh)}, " +
                                         $"{GetAttrNameAndValue(nameof(Sync), Sync)}" +
                                         $"{(this.additionalAttributes.Length <= 0 ? string.Empty : ", " + string.Join(", ", this.additionalAttributes))}";

    /// <summary>
    /// Returns the name and value of the given attribute <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to convert to a string.</typeparam>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="value">The value of the attribute.</param>
    /// <returns>A string representing the name and value.</returns>
    private static string GetAttrNameAndValue<T>(string name, T? value)
        where T : unmanaged
        => value is null
            ? $"{name}: N/A"
            : $"{name}: {value}";
}
