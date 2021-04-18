using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Pixelizer.Models;
using Pixelizer.Services;
using Pixelizer.Util;
using ReactiveUI;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Pixelizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _isBusy;
        public bool IsBusy => _isBusy.Value;
        
        private string _resultPath;
        public string ResultPath
        {
            get => _resultPath;
            set => this.RaiseAndSetIfChanged(ref _resultPath, value);
        }

        private int _width;
        public int Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        private int _height;
        public int Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        public List<ColorMode> ColorModes { get; } = new()
        {
            ColorMode.Black,
            ColorMode.Red,
            ColorMode.Green,
            ColorMode.Blue,
        };

        private ColorMode _selectedColorMode = ColorMode.Black;
        public ColorMode SelectedColorMode
        {
            get => _selectedColorMode;
            set => this.RaiseAndSetIfChanged(ref _selectedColorMode, value);
        }

        private int _threshold = 120;
        public int Threshold
        {
            get => _threshold;
            set => this.RaiseAndSetIfChanged(ref _threshold, value);
        }

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
        
        public GcodeConfig GcodeConfig { get; }

        private System.Drawing.Bitmap? _currentImage;

        public ReactiveCommand<Unit, Unit> ConvertToGcode { get; }
        private ReactiveCommand<Unit, Unit> ConvertImageCommand { get; }
        
        public MainWindowViewModel()
        {
            GcodeConfig = new GcodeConfig();
            ConvertToGcode = ReactiveCommand.CreateFromTask(Convert);
            ConvertImageCommand = ReactiveCommand.CreateFromTask(ConvertImage);

            _resultPath = 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "result.gcode");

            var imageChanged = this.WhenAnyValue(x => x.ImagePath)
                .Where(x => x != null);
            
            imageChanged
                .Do(LoadImage!)
                .Subscribe();

            var widthChanged = this.WhenAnyValue(x => x.Width)
                .Select(_ => Unit.Default);
            var thresholdChanged = this.WhenAnyValue(x => x.Threshold)
                .Select(_ => Unit.Default);
            var heightChanged = this.WhenAnyValue(x => x.Height)
                .Select(_ => Unit.Default);
            var penWidthChanged = this.WhenAnyValue(x => x.GcodeConfig.PenWidth)
                .Select(_ => Unit.Default);

            widthChanged
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Do(_ => CheckAspectHeight())
                .Subscribe();

            widthChanged
                .Merge(penWidthChanged)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Do(_ => CalculateWidth())
                .Subscribe();

            heightChanged
                .Merge(penWidthChanged)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Do(_ => CalculateHeight())
                .Subscribe();

            var calcWidthChanged = this.WhenAnyValue(x => x.CalculatedWidth)
                .Select(_ => Unit.Default);
            var calcHeightChanged = this.WhenAnyValue(x => x.CalculatedHeight)
                .Select(_ => Unit.Default);
            var colorModeChanged = this.WhenAnyValue(x => x.SelectedColorMode)
                .Select(_ => Unit.Default);
            
            
            calcHeightChanged
                .Merge(calcWidthChanged)
                .Merge(imageChanged.Select(_ => Unit.Default))
                .Merge(thresholdChanged)
                .Merge(colorModeChanged)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .InvokeCommand(this, x => x.ConvertImageCommand);

            _isBusy = ConvertImageCommand.IsExecuting
                .Merge(ConvertToGcode.IsExecuting)
                .ToProperty(this, x => x.IsBusy);
        }

        private async Task Convert()
        {
            if (_currentImage == null) 
                return;

            var result = await Task.Run(() =>
            {
                var encoder = new GcodeEncoder();
                return encoder.ConvertImageToGcode(_currentImage, GcodeConfig);
            });

            await File.WriteAllTextAsync(ResultPath, result);
        }

        private void CheckAspectHeight()
        {
            if (SourceImage == null)
                return;
            var sourceImageSize = SourceImage.Size;
            var aspect = Width / sourceImageSize.Width;
            Height = (int) (sourceImageSize.Height * aspect);
        }

        private void CalculateWidth()
        {
            if (GcodeConfig.PenWidth == 0)
            {
                CalculatedWidth = 0;
            }
            CalculatedWidth = (int)(Width / GcodeConfig.PenWidth);
        }
        
        private void CalculateHeight()
        {
            if (GcodeConfig.PenWidth == 0)
            {
                CalculatedHeight = 0;
            }
            CalculatedHeight = (int)(Height / GcodeConfig.PenWidth);
        }

        private void LoadImage(ImageInfo info)
        {
            if (File.Exists(info.Path))
            {
                var stream = File.OpenRead(info.Path);
                SourceImage = Bitmap.DecodeToWidth(stream, info.Width);
                
                Height = info.Height;
                Width = info.Width;
            }
        }

        private async Task ConvertImage()
        {
            TargetImage = await Task.Run(() 
                =>
            {
                if (SourceImage == null || GcodeConfig.PenWidth == 0)
                    return null;

                var width = (int)(Width / GcodeConfig.PenWidth);
                var height = (int)(Height / GcodeConfig.PenWidth);

                var scaled = SourceImage.CreateScaledBitmap(new PixelSize(width, height));
                if (scaled == null)
                    return null;
                var memoryStream = new MemoryStream();
                scaled.Save(memoryStream);
                var sysBitmap = new System.Drawing.Bitmap(memoryStream);
                sysBitmap.ToPixelImage(SelectedColorMode, Threshold);
                // Loading Avalonia Bitmap from MemoryStream did not work
                // Therefore, saving to temp file on disk
                var tempFile = Path.GetTempFileName();
                sysBitmap.Save(tempFile);

                _currentImage = sysBitmap;
                
                var loaded = new Bitmap(tempFile);
                File.Delete(tempFile);

                return loaded;
            });
        }
    }
    
    
}