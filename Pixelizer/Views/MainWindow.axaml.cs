using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Pixelizer.Models;
using Pixelizer.ViewModels;

namespace Pixelizer.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private Panel _dropPanel;
        private Image _sourceImage;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _dropPanel = this.FindControl<Panel>("DropPanel");
            _sourceImage = this.FindControl<Image>("SourceImage");
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            var filenames = e.Data.GetFileNames()?.ToArray();
            var result = CheckFilenames(filenames);

            if (result == null)
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            e.DragEffects = DragDropEffects.Move;
        }

        private string? CheckFilenames(string[]? filenames)
        {
            if (filenames == null)
            {
                return null;
            }

            if (filenames.Length != 1)
            {
                return null;
            }

            var file = filenames[0];
            var extension = Path.GetExtension(file).ToLower();
            if (extension != ".png" && extension != ".jpg")
            {
                return null;
            }

            return file;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        void Drop(object? sender, DragEventArgs e)
        {
            var filename = e.Data.GetFileNames()?.ToArray();
            var file = CheckFilenames(filename);
            if (file == null)
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }
            
            ViewModel.ImagePath = ImageInfo.FromPath(file);

            //ToCcitt(file);
            //
            // var bitmap = new Bitmap(File.OpenRead(file));
            // using var graphics = Graphics.FromImage(bitmap);
            // var grayMatrix = new float[][] { 
            //     new[] { 0.299f, 0.299f, 0.299f, 0, 0 }, 
            //     new[] { 0.587f, 0.587f, 0.587f, 0, 0 }, 
            //     new[] { 0.114f, 0.114f, 0.114f, 0, 0 }, 
            //     new[] { 0f,      0f,      0f,      1f, 0f }, 
            //     new[] { 0f,      0f,      0f,      0f, 1f } 
            // };
            //
            // var ia = new ImageAttributes();
            // ia.SetColorMatrix(new ColorMatrix(grayMatrix));
            // ia.SetThreshold(0.1f); // Change this threshold as needed
            // var rc = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            // graphics.DrawImage(bitmap, rc, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
            //
            // var newBitmap = new Bitmap(bitmap.Width, bitmap.Height, graphics);

            // newBitmap.Save(@"C:\Users\chris\Desktop\Test.jpg");
        }
    }
}