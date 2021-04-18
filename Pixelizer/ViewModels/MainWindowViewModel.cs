using System;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Pixelizer.Models;
using Pixelizer.Services;
using Pixelizer.Util;
using ReactiveUI;

namespace Pixelizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string Path = @"C:\Users\chris\Downloads\result.jpg";
        
        private string _width = "";
        public string Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        private string _stringThreshold = "120";
        public string StringThreshold
        {
            get => _stringThreshold;
            set => this.RaiseAndSetIfChanged(ref _stringThreshold, value);
        }

        private string _zAxisDown = "0";
        public string ZAxisDown
        {
            get => _zAxisDown;
            set => this.RaiseAndSetIfChanged(ref _zAxisDown, value);
        }

        private string _zAxisUp = "1";
        public string ZAxisUp
        {
            get => _zAxisUp;
            set => this.RaiseAndSetIfChanged(ref _zAxisUp, value);
        }
        
        public int Threshold => StringThreshold.ParseOrDefault(150);
        public int NumericWidth => Width.ParseOrDefault(0);
        public int NumericHeight => Height.ParseOrDefault(0);
        public double NumericPenWidth => PenWidth.ParseOrDefault(0.0);
        
        private int _calculatedWidth;
        public int CalculatedWidth
        {
            get => _calculatedWidth;
            set => this.RaiseAndSetIfChanged(ref _calculatedWidth, value);
        }

        private int _calculatedHeight;
        public int CalculatedHeight
        {
            get => _calculatedHeight;
            set => this.RaiseAndSetIfChanged(ref _calculatedHeight, value);
        }
        
        private string _height = "";
        public string Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        private string _penWidth = "1";
        public string PenWidth
        {
            get => _penWidth;
            set => this.RaiseAndSetIfChanged(ref _penWidth, value);
        }

        private ImageInfo? _imagePath;
        public ImageInfo? ImagePath
        {
            get => _imagePath;
            set => this.RaiseAndSetIfChanged(ref _imagePath, value);
        }

        private Bitmap? _sourceImage;
        public Bitmap? SourceImage
        {
            get => _sourceImage;
            set => this.RaiseAndSetIfChanged(ref _sourceImage, value);
        }

        private Bitmap? _targetImage;
        public Bitmap? TargetImage
        {
            get => _targetImage;
            set => this.RaiseAndSetIfChanged(ref _targetImage, value);
        }

        private System.Drawing.Bitmap? _currentImage;

        public ReactiveCommand<Unit, Unit> ConvertToGcode { get; }
        
        public MainWindowViewModel()
        {
            ConvertToGcode = ReactiveCommand.Create(Convert);
            
            var imageChanged = this.WhenAnyValue(x => x.ImagePath)
                .Where(x => x != null);
            
            imageChanged
                .Do(LoadImage!)
                .Subscribe();

            var widthChanged = this.WhenAnyValue(x => x.Width);
            var thresholdChanged = this.WhenAnyValue(x => x.StringThreshold);
            var heightChanged = this.WhenAnyValue(x => x.Height);
            var penWidthChanged = this.WhenAnyValue(x => x.PenWidth);

            widthChanged
                .Do(_ => CheckAspectHeight())
                .Subscribe();
            
            heightChanged
                .Do(_ => CheckAspectWidth())
                .Subscribe();
            
            widthChanged
                .Merge(penWidthChanged)
                .Do(_ => CalculateWidth())
                .Subscribe();

            heightChanged
                .Merge(penWidthChanged)
                .Do(_ => CalculateHeight())
                .Subscribe();

            var calcWidthChanged = this.WhenAnyValue(x => x.CalculatedWidth);
            var calcHeightChanged = this.WhenAnyValue(x => x.CalculatedHeight);

            calcHeightChanged
                .Merge(calcWidthChanged)
                .Merge(imageChanged.Select(_ => 0))
                .Merge(thresholdChanged.Select(_ => 0))
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Do(async _ => await ConvertImage())
                .Subscribe();
        }

        private void Convert()
        {
            if (_currentImage == null) 
                return;

            if (!int.TryParse(ZAxisDown, out var zDown))
            {
                throw new Exception("ZAxis down value is not numeric");
            }

            if (!int.TryParse(ZAxisUp, out var zUp))
            {
                throw new Exception("ZAxis Up value is not numeric");
            }

            if (NumericPenWidth <= 0)
            {
                throw new Exception("Pen width must be number greater than 0");
            }
            
            var encoder = new GcodeEncoder();
            var result = encoder.ToGcode(_currentImage, new GcodeConfig
            {
                PenWidth = NumericPenWidth,
                ZAxisDown = zDown,
                ZAxisUp = zUp,
                FeedRate = 500
            });
        }

        private void CheckAspectWidth()
        {
            if (SourceImage == null)
                return;
            var height = Height.ParseOrDefault(0);
            var aspect = height / SourceImage.Size.Height;
            Width = ((int) (SourceImage.Size.Width * aspect)).ToString();
        }

        private void CheckAspectHeight()
        {
            if (SourceImage == null)
                return;
            var width = Width.ParseOrDefault(0);
            var sourceImageSize = SourceImage.Size;
            var aspect = width / sourceImageSize.Width;
            Height = ((int) (sourceImageSize.Height * aspect)).ToString();
        }

        private void CalculateWidth()
        {
            if (NumericPenWidth == 0)
            {
                CalculatedWidth = 0;
            }
            CalculatedWidth = (int)(NumericWidth / NumericPenWidth);
        }
        
        private void CalculateHeight()
        {
            if (NumericPenWidth== 0)
            {
                CalculatedHeight = 0;
            }
            CalculatedHeight = (int)(NumericHeight / NumericPenWidth);
        }

        private void LoadImage(ImageInfo info)
        {
            if (File.Exists(info.Path))
            {
                var stream = File.OpenRead(info.Path);
                SourceImage = Bitmap.DecodeToWidth(stream, info.Width);
                if (string.IsNullOrWhiteSpace(Height)
                    && string.IsNullOrWhiteSpace(Width))
                {
                    Height = info.Height.ToString();
                    Width = info.Width.ToString();
                }
            }
        }

        private async Task ConvertImage()
        {
            TargetImage = await Task.Run(() 
                =>
            {
                
                if (CalculatedHeight == 0 || CalculatedWidth == 0
                                          || SourceImage == null)
                    return null;

                var scaled = SourceImage.CreateScaledBitmap(new PixelSize(CalculatedWidth, CalculatedHeight));
                var memoryStream = new MemoryStream();
                scaled.Save(memoryStream);
                var sysBitmap = new System.Drawing.Bitmap(memoryStream);
                sysBitmap.ToBlackAndWhite(Threshold);
                sysBitmap.Save(Path);

                _currentImage = sysBitmap;
                
                return new Bitmap(Path);
            });
        }
    }
    
    
}