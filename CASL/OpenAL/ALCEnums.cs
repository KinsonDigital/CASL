// <copyright file="ALCEnums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable UnusedMember.Global
namespace CASL.OpenAL;

/// <summary>
/// Defines available context attributes.
/// </summary>
internal enum AlcContextAttributes
{
    /// <summary>
    /// Followed by System.Int32 Hz
    /// </summary>
    Frequency = 0x1007,

    /// <summary>
    /// Followed by System.Int32 Hz
    /// </summary>
    Refresh = 0x1008,

    /// <summary>
    /// Followed by AlBoolean.True, or AlBoolean.False
    /// </summary>
    Sync = 0x1009,

    /// <summary>
    /// Followed by System.Int32 Num of requested Mono (3D) Sources
    /// </summary>
    MonoSources = 0x1010,

    /// <summary>
    /// Followed by System.Int32 Num of requested Stereo Sources
    /// </summary>
    StereoSources = 0x1011,

    /// <summary>
    /// (EFX Extension) This Context property can be passed to OpenAL during Context creation (alcCreateContext) to
    /// request a maximum number of Auxiliary Sends desired on each Source. It is not guaranteed that the desired number
    /// of sends will be available, so an application should query this property after creating the context using 'alcGetIntergerv'.
    /// Default: 2
    /// </summary>
    EfxMaxAuxiliarySends = 0x20003,
}

/// <summary>
/// Defines available parameters for <see cref="ALC.GetString(ALDevice, AlcGetString)"/>.
/// </summary>
internal enum AlcGetString
{
    /// <summary>The specifier string for the default device.
    /// </summary>
    DefaultDeviceSpecifier = 0x1004,

    /// <summary>
    /// A list of available context extensions separated by spaces.
    /// </summary>
    Extensions = 0x1006,

    /// <summary>
    /// The name of the default capture device
    /// </summary>
    /// <remarks>ALC_EXT_CAPTURE extension</remarks>
    CaptureDefaultDeviceSpecifier = 0x311,

    /// <summary>
    /// a list of the default devices.
    /// </summary>
    DefaultAllDevicesSpecifier = 0x1012,

    /// <summary>
    /// Will only return the first Device, not a list.
    /// </summary>
    /// <summary>
    /// Use AlcGetStringList.CaptureDeviceSpecifier. ALC_EXT_CAPTURE_EXT
    /// </summary>
    CaptureDeviceSpecifier = 0x310,

    /// <summary>
    /// Will only return the first Device, not a list. Use AlcGetStringList.DeviceSpecifier
    /// </summary>
    DeviceSpecifier = 0x1005,

    /// <summary>
    /// Will only return the first Device, not a list. Use AlcGetStringList.AllDevicesSpecifier
    /// </summary>
    AllDevicesSpecifier = 0x1013,
}

/// <summary>
/// Defines available parameters for <see cref="ALC.GetString(ALDevice, AlcGetString)"/>.
/// </summary>
internal enum AlcGetStringList
{
    /// <summary>
    /// The name of the specified capture device, or a list of all available capture devices if no capture device is specified.
    /// </summary>
    /// <remarks>ALC_EXT_CAPTURE_EXT</remarks>
    CaptureDeviceSpecifier = 0x310,

    /// <summary>
    /// The specifier strings for all available devices.
    /// </summary>
    /// <remarks>ALC_ENUMERATION_EXT</remarks>
    DeviceSpecifier = 0x1005,

    /// <summary>
    /// The specifier strings for all available devices.
    /// </summary>
    /// <summary>ALC_ENUMERATE_ALL_EXT</summary>
    AllDevicesSpecifier = 0x1013,
}

/// <summary>
/// A list of valid string AL.Get() parameters.
/// </summary>
internal enum ALGetString
{
    /// <summary>
    /// Gets the Vendor name.
    /// </summary>
    Vendor = 0xB001,

    /// <summary>
    /// Gets the driver version.
    /// </summary>
    Version = 0xB002,

    /// <summary>
    /// Gets the renderer mode.
    /// </summary>
    Renderer = 0xB003,

    /// <summary>
    /// Gets a list of all available Extensions, separated with spaces.
    /// </summary>
    Extensions = 0xB004,
}

/// <summary>
/// Defines OpenAL context errors.
/// </summary>
internal enum AlcError
{
    /// <summary>
    /// There is no current error.
    /// </summary>
    NoError = 0,

    /// <summary>
    /// No Device. The device handle or specifier names an inaccessible driver/server.
    /// </summary>
    InvalidDevice = 0xA001,

    /// <summary>
    /// Invalid context ID. The Context argument does not name a valid context.
    /// </summary>
    InvalidContext = 0xA002,

    /// <summary>
    /// Bad enum. A token used is not valid, or not applicable.
    /// </summary>
    InvalidEnum = 0xA003,

    /// <summary>
    /// Bad value. A value (e.g. Attribute) is not valid, or not applicable.
    /// </summary>
    InvalidValue = 0xA004,

     /// <summary>
    /// Out of memory. Unable to allocate memory.
    /// </summary>
    OutOfMemory = 0xA005,
}
