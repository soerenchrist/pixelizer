using System.Text;
using Pixelizer.Util;

namespace Pixelizer.Services;

public class GcodeBuilder
{
    private readonly StringBuilder _builder;

    public GcodeBuilder()
    {
        _builder = new StringBuilder();
        SetAbsolutePositioning();
    }

    private void SetAbsolutePositioning()
    {
        _builder.AppendLine("G90");
    }

    public void Home()
    {
        _builder.AppendLine("G28");
    }

    public void MoveTo(double x, double y, int feedRate)
    {
        _builder.AppendLine($"G1 X{x.ToFormatted()} Y{y.ToFormatted()} F{feedRate}");
    }

    public void SetZPosition(double position)
    {
        _builder.AppendLine($"G1 Z{position}");
    }

    public string Build()
    {
        return _builder.ToString();
    }
}