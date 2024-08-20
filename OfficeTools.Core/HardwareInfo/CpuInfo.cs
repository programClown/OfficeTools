namespace OfficeTools.Core.HardwareInfo;

public readonly record struct CpuInfo
{
    public string ProcessorCaption { get; init; }
    public string ProcessorName { get; init; }
}