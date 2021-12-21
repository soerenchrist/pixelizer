using ReactiveUI;

namespace Pixelizer.Models;

public class GcodeConfig : ReactiveObject
{
    private bool _autoHome;
    private bool _drawFrame;

    private int _feedRate = 300;

    private int _offsetX;

    private int _offsetY;

    private double _penWidth = 1;

    private int _zAxisDown;

    private int _zAxisUp = 1;

    public bool DrawFrame
    {
        get => _drawFrame;
        set => this.RaiseAndSetIfChanged(ref _drawFrame, value);
    }

    public bool AutoHome
    {
        get => _autoHome;
        set => this.RaiseAndSetIfChanged(ref _autoHome, value);
    }

    public int ZAxisUp
    {
        get => _zAxisUp;
        set => this.RaiseAndSetIfChanged(ref _zAxisUp, value);
    }

    public int ZAxisDown
    {
        get => _zAxisDown;
        set => this.RaiseAndSetIfChanged(ref _zAxisDown, value);
    }

    public double PenWidth
    {
        get => _penWidth;
        set => this.RaiseAndSetIfChanged(ref _penWidth, value);
    }

    public int FeedRate
    {
        get => _feedRate;
        set => this.RaiseAndSetIfChanged(ref _feedRate, value);
    }

    public int OffsetX
    {
        get => _offsetX;
        set => this.RaiseAndSetIfChanged(ref _offsetX, value);
    }

    public int OffsetY
    {
        get => _offsetY;
        set => this.RaiseAndSetIfChanged(ref _offsetY, value);
    }
}