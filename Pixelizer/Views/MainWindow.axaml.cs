using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Pixelizer.Models;
using Pixelizer.ViewModels;

namespace Pixelizer.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

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
        }

        private async void DropPanel_OnTapped(object? sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Extensions = new() { "jpg", "png", "gif" },
                        Name = "Images"
                    }
                },
                Title = "Select image",
                AllowMultiple = false,
            };

            var result = await openFileDialog.ShowAsync(this);
            if (result.Length == 0)
                return;
            var filePath = result[0];
            ViewModel.ImagePath = ImageInfo.FromPath(filePath);
        }
    }
}