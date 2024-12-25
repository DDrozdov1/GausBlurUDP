using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ImageProcessing.Core.Models;
using Microsoft.Extensions.Logging;
using ImageProcessing.Common;

namespace ImageProcessing.Client
{
    public class ImageSender
    {
        private readonly UdpHelper _udpHelper;
        private readonly ILogger<ImageSender> _logger;
        private readonly string _coordinatorIp;
        private readonly int _coordinatorPort;

        public ImageSender(UdpHelper udpHelper, ILogger<ImageSender> logger, string coordinatorIp, int coordinatorPort)
        {
            _udpHelper = udpHelper;
            _logger = logger;
            _coordinatorIp = coordinatorIp;
            _coordinatorPort = coordinatorPort;
        }
        public async Task SendImage(string imagePath, int imageId)
        {

            try
            {
                var imageData = await File.ReadAllBytesAsync(imagePath);
                var message = new ImageMessage
                {
                    MessageType = "Image",
                    ImageId = imageId,
                    ImageData = imageData
                };

                var endpoint = new IPEndPoint(IPAddress.Parse(_coordinatorIp), _coordinatorPort);
                await _udpHelper.SendAsync(message, endpoint);
                _logger.LogInformation($"Image {imageId} sent to coordinator");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending Image {imageId}: {ex.Message}");
            }

        }
    }
}