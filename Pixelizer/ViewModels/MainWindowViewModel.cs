using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DynamicData.Binding;
using Pixelizer.Models;
using Pixelizer.Services;
using Pixelizer.Services.Image;
using Pixelizer.Services.Image.Abstractions;
using Pixelizer.Services.Strategies;
using Pixelizer.Util;
using ReactiveUI;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Pixelizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<bool> _isBusy;
        public bool IsBusy => _isBusy.Value;

        private readonly ObservableAsPropertyHelper<bool> _isAdaptiveThresholding;
        public bool IsAdaptiveThresholding => _isAdaptiveThresholding.Value;

        private readonly ObservableAsPropertyHelper<bool> _hasImage;
        public bool HasImage => _hasImage.Value;

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

        private double _totalDistance;
        public double TotalDistance
        {
            get => _totalDistance;
            set => this.RaiseAndSetIfChanged(ref _totalDistance, value);
        }

        private int _timeInMinutes;
        public int TimeInMinutes
        {
            get => _timeInMinutes;
            set => this.RaiseAndSetIfChanged(ref _timeInMinutes, value);
        }

        public List<ColorMode> ColorModes { get; } = new()
        {
            ColorMode.Black,
            ColorMode.Red,
            ColorMode.Green,
            ColorMode.Blue,
        };

        public List<string> PixelStrategies => new()
        {
            "Thresholding",
            "Adaptive Thresholding"
        };

        private int _selectedPixelStrategy;

        public int SelectedPixelStrategy
        {
            get => _selectedPixelStrategy;
            set => this.RaiseAndSetIfChanged(ref _selectedPixelStrategy, value);
        }

        public int PixelCount => _pixelList.Count;

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

        private int _adaptiveBrushSize = 9;
        public int AdaptiveBrushSize
        {
            get => _adaptiveBrushSize;
            set => this.RaiseAndSetIfChanged(ref _adaptiveBrushSize, value);
        }

        private int _adaptiveOffset = 7;
        public int AdaptiveOffset
        {
            get => _adaptiveOffset;
            set => this.RaiseAndSetIfChanged(ref _adaptiveOffset, value);
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

        private List<(double, double)> _pixelList = new();

        public GcodeConfig GcodeConfig { get; } = new();

        private System.Drawing.Bitmap? _currentImage;

        public ReactiveCommand<Unit, Unit> ConvertToGcode { get; }
        private ReactiveCommand<Unit, Unit> ConvertImageCommand { get; }
        private ReactiveCommand<Unit, Unit> ExportImageCommand { get; }
        private ReactiveCommand<Unit, Unit> CancelCommand { get; }
        private ReactiveCommand<Unit, Unit> CalculateTimeCommand { get; }

        private readonly IPixelOrderStrategy _pixelOrderer = new KdTreeOrderStrategy();

        public MainWindowViewModel()
        {
            ConvertToGcode = ReactiveCommand.CreateFromTask(Convert);
            ConvertImageCommand = ReactiveCommand.CreateFromObservable(
                () => Observable.StartAsync(ConvertImage)
                    .TakeUntil(CancelCommand!));
            ExportImageCommand = ReactiveCommand.CreateFromTask(ExportImage);
            CancelCommand = ReactiveCommand.Create(() => { }, ConvertImageCommand.IsExecuting);
            CalculateTimeCommand = ReactiveCommand.Create(CalcuateTime);

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
            var brushSizeChanged = this.WhenAnyValue(x => x.AdaptiveBrushSize)
                .Select(_ => Unit.Default);
            var offsetChanged = this.WhenAnyValue(x => x.AdaptiveOffset)
                .Select(_ => Unit.Default);
            var strategyChanged = this.WhenAnyValue(x => x.SelectedPixelStrategy)
                .Select(_ => Unit.Default);

            var inputsChanged = calcHeightChanged
                .Merge(calcWidthChanged)
                .Merge(imageChanged.Select(_ => Unit.Default))
                .Merge(thresholdChanged)
                .Merge(colorModeChanged)
                .Merge(brushSizeChanged)
                .Merge(strategyChanged)
                .Merge(offsetChanged);

            inputsChanged.InvokeCommand(this, x => x.CancelCommand);
            
            inputsChanged
                .Throttle(TimeSpan.FromMilliseconds(500))
                .InvokeCommand(this, x => x.ConvertImageCommand);

            _isBusy = ConvertImageCommand.IsExecuting
                .Merge(ConvertToGcode.IsExecuting)
                .ToProperty(this, x => x.IsBusy);

            _isAdaptiveThresholding = this.WhenAnyValue(x => x.SelectedPixelStrategy)
                .Select(x => x == 1)
                .ToProperty(this, x => x.IsAdaptiveThresholding);

            _hasImage = this.WhenAnyValue(x => x.ImagePath)
                .Select(x => x != null)
                .ToProperty(this, x => x.HasImage);

            var gcodeConfigChanged = GcodeConfig.WhenAnyValue(x => x.FeedRate, x => x.ZAxisDown, x => x.ZAxisUp)
                .Select(_ => Unit.Default);
            
            var pixelsChanged = this.WhenAnyValue(x => x.PixelCount)
                .Select(_ => Unit.Default);
                
            gcodeConfigChanged
                .Merge(pixelsChanged)
                .InvokeCommand(this, x => x.CalculateTimeCommand);
        }

        private void CalcuateTime()
        {
            Task.Run(() =>
            {
                var ordered = _pixelOrderer.GetPixelOrder(_pixelList);
                var distanceInMm = ordered.CalculateDistance();
                var upDownMovementInMm = ordered.Count * 2 * (GcodeConfig.ZAxisUp - GcodeConfig.ZAxisDown);
                TotalDistance = Math.Round((distanceInMm + upDownMovementInMm) / 1000, 2);

                var movingTimeInMinutes = distanceInMm / GcodeConfig.FeedRate;
                var upDownMovementTime = upDownMovementInMm / GcodeConfig.FeedRate;
                
                TimeInMinutes = (int)(movingTimeInMinutes + upDownMovementTime);
            });
        }

        private async Task ExportImage()
        {
            if (_currentImage == null)
            {
                return;
            }

            var path = await GetSaveFileName("jpg");
            if (path != null)
            {
                _currentImage.Save(path);
            }
        }

        private async Task<string?> GetSaveFileName(string extension)
        {
            SaveFileDialog dialog = new();

            dialog.Filters.Add(new FileDialogFilter { Name = extension, Extensions = { extension } });
            dialog.DefaultExtension = extension;
            dialog.InitialFileName = "result";

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                var path = await dialog.ShowAsync(window);
                return path;
            }

            return null;
        }

        private async Task Convert()
        {
            if (_currentImage == null)
                return;

            var path = await GetSaveFileName("gcode");
            if (path == null)
            {
                return;
            }

            var result = await Task.Run(() =>
            {
                var encoder = new GcodeEncoder(_pixelOrderer);
                return encoder.ConvertImageToGcode(_pixelList, _currentImage.Width, _currentImage.Height, GcodeConfig);
            });

            await File.WriteAllTextAsync(path, result);
        }

        private void CheckAspectHeight()
        {
            if (SourceImage == null)
                return;
            var sourceImageSize = SourceImage.Size;
            var aspect = Width / sourceImageSize.Width;
            Height = (int)(sourceImageSize.Height * aspect);
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

        private async Task ConvertImage(CancellationToken token)
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
                var sysBitmap = scaled.ToSystemBitmap();

                IPixelizerService strategy = SelectedPixelStrategy switch
                {
                    0 => new ThresholdingPixelizerService(Threshold),
                    _ => new AdaptiveThresholdingPixelizerService(AdaptiveBrushSize, AdaptiveOffset)
                };

                var result = strategy.ConvertToPixelImage(sysBitmap, SelectedColorMode, token);
                if (result == null)
                    return null;
                
                _currentImage = result;
                _pixelList = _currentImage.GetPixels(GcodeConfig);
                this.RaisePropertyChanged(nameof(PixelCount));
                
                return result.ToAvaloniaBitmap();
            }, token);
        }
    }
}