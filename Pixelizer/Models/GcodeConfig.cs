using ReactiveUI;

namespace Pixelizer.Models
{
    public class GcodeConfig : ReactiveObject
    {
        private bool _autoHome;
        public bool AutoHome
        {
            get => _autoHome;
            set => this.RaiseAndSetIfChanged(ref _autoHome, value);
        }
        
        private int _zAxisUp = 1;
        public int ZAxisUp
        {
            get => _zAxisUp;
            set => this.RaiseAndSetIfChanged(ref _zAxisUp, value);
        }

        private int _zAxisDown;
        public int ZAxisDown
        {
            get => _zAxisDown;
            set => this.RaiseAndSetIfChanged(ref _zAxisDown, value);
        }

        private double _penWidth = 1;
        public double PenWidth
        {
            get => _penWidth;
            set => this.RaiseAndSetIfChanged(ref _penWidth, value);
        }

        private int _feedRate = 300;
        public int FeedRate
        {
            get => _feedRate;
            set => this.RaiseAndSetIfChanged(ref _feedRate, value);
        }

        private int _offsetX;
        public int OffsetX
        {
            get => _offsetX;
            set => this.RaiseAndSetIfChanged(ref _offsetX, value);
        }

        private int _offsetY;
        public int OffsetY
        {
            get => _offsetY;
            set => this.RaiseAndSetIfChanged(ref _offsetY, value);
        }
    }
}