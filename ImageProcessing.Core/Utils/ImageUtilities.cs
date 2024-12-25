using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageProcessing.Core.Utils
{
    public class ImageUtilities : IImageProcessor
    {
        public async Task<ImageMessage> ProcessImage(ImageMessage message)
        {
            Console.WriteLine($"Processing image {message.ImageId}...");
            using (var image = Image.Load<Rgba32>(message.ImageData))
            {
                image.Mutate(x => x.GaussianBlur());

                using (var memoryStream = new MemoryStream())
                {
                    image.SaveAsPng(memoryStream);
                    var processedImage = memoryStream.ToArray();
                    return new ImageMessage()
                    {
                        MessageType = "Result",
                        ImageId = message.ImageId,
                        Result = "Success" // или сообщение об ошибке
                    };
                }
            }
        }
    }
}