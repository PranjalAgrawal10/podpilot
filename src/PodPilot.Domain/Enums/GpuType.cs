namespace PodPilot.Domain.Enums;

/// <summary>
/// GPU hardware types supported across providers.
/// </summary>
public enum GpuType
{
    /// <summary>NVIDIA RTX 4090.</summary>
    RTX4090 = 0,

    /// <summary>NVIDIA RTX 5090.</summary>
    RTX5090 = 1,

    /// <summary>NVIDIA A100.</summary>
    A100 = 2,

    /// <summary>NVIDIA H100.</summary>
    H100 = 3,

    /// <summary>NVIDIA L40S.</summary>
    L40S = 4,

    /// <summary>NVIDIA A40.</summary>
    A40 = 5,

    /// <summary>NVIDIA V100.</summary>
    V100 = 6,

    /// <summary>Custom or unmapped GPU type.</summary>
    Custom = 7,

    /// <summary>NVIDIA H200.</summary>
    H200 = 8,

    /// <summary>NVIDIA B200 (future-ready).</summary>
    B200 = 9,
}
