using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Emgu.CV;
using Websocket.Client;

namespace CameraClient
{
    public class Camera
    {
        private VideoCapture? _capture;
        private readonly WebsocketClient _client;

        // private VideoQuality quality;
        private readonly ImageFormat _imageFormat;

        public Camera(WebsocketClient client)
        {
            _client = client;

            // Set video encoding.
            _imageFormat = ImageFormat.Jpeg;
        }

        private byte[] Compress(Mat? image)
        {
            var ms = new MemoryStream();
            image.ToBitmap().Save(ms, _imageFormat);
            return ms.ToArray();
        }

        private void ProcessFrame(object? sender, EventArgs arg)
        {
            var imageFrame = _capture?.QueryFrame();
            byte[] bytes = Compress(imageFrame);
            _client.Send(bytes);
        }

        public void TurnOn()
        {
            // Initialise `capture` if it is null.
            _capture ??= new VideoCapture(0, VideoCapture.API.DShow);
            _capture.ImageGrabbed += ProcessFrame;
            _capture.Start();
        }

        public void TurnOff()
        {
            // Stop and dispose `capture` only if it is not null.
            _capture?.Stop();
            _capture?.Dispose();

            // Set it to `null` to turn off the camera.
            _capture = null;
        }

        public void Blink(int repeat, int delay)
        {
            for (var i = 0; i < repeat; i++)
            {
                TurnOn();
                Thread.Sleep(delay);
                
                TurnOff();
                Thread.Sleep(delay);
            }
        }
    }
}
