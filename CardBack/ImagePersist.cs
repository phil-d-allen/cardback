using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CardBack;

internal static class ImagePersist
{
    private const int DefaultDpi = 96; // other values create padded or uncentered results.

    public static void PersistImage(Drawing image, int width, int Height, string name)
    {
        DrawingVisual UIElementFromDrawing(Drawing image)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawDrawing(image);
            }
            return visual;
        }

        static BitmapSource CaptureControl(DrawingVisual control, int width, int height)
        {
            // The clipping (rounded corners) are transparent in the result.
            var rtb = new RenderTargetBitmap(
                (int)width,
                (int)height,
                dpiX:DefaultDpi,
                dpiY:DefaultDpi,
                PixelFormats.Pbgra32);

            rtb.Render(control);
            return rtb;
        }

        string GetPathAndFormatFromSystemFileSave(string name)
        {
                // Create and configure SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Save the cardback",
                    FileName = $"{name}.png",
                    Filter = "Portable Network Graphics Files (*.png)|*.png|All Files (*.*)|*.*",
                    DefaultExt = "png",
                    AddExtension = true,
                    OverwritePrompt = true // Ask before overwriting
                };

                // Show dialog and check if user clicked Save
                if (saveFileDialog.ShowDialog() == true)
                {
                    return saveFileDialog.FileName;
                }
            else
            {
                return string.Empty;
            }
        }


        var drawingVisual = UIElementFromDrawing(image);
        var bitmap = CaptureControl(drawingVisual, width, Height);
        var filePath = GetPathAndFormatFromSystemFileSave(name);

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        // Save to PNG
        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show("Permissions error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (IOException ioEx)
        {
            MessageBox.Show($"General I/O Error: {ioEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}