using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ImageProcessing.Core.Models;
using Microsoft.Extensions.Logging;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Utils;
using ImageProcessing.Common;

namespace ImageProcessing.Worker
{
    public class Worker
    {
        private readonly UdpHelper _udpHelper;
        private readonly ILogger<Worker> _logger;
        private readonly IPEndPoint _coordinatorEndpoint;
        private readonly IImageProcessor _imageProcessor;
        private readonly WorkerSettings _settings;
        public Worker(WorkerSettings settings, UdpHelper udpHelper, ILogger<Worker> logger, IImageProcessor imageProcessor)
        {
            _settings = settings;
            _udpHelper = udpHelper;
            _logger = logger;
            _coordinatorEndpoint = new IPEndPoint(IPAddress.Parse(_settings.CoordinatorIp), _settings.CoordinatorPort);
            _imageProcessor = imageProcessor;
            _logger.LogInformation("Worker started on port " + _settings.Port);
        }

        public async Task StartListening()
        {
            while (true)
            {
                var result = await _udpHelper.ReceiveAsync();
                if (result == null || result.Value.Buffer == null) continue;
                var message = JsonConvert.DeserializeObject<ImageMessage>(Encoding.UTF8.GetString(result.Value.Buffer));
                if (message == null) continue;
                if (message.MessageType == "Image")
                {
                    var resultMessage = await _imageProcessor.ProcessImage(message);
                    await SendResultToCoordinator(resultMessage);
                }
            }
        }
        private async Task SendResultToCoordinator(ImageMessage result)
        {
            await _udpHelper.SendAsync(result, _coordinatorEndpoint);
            _logger.LogInformation($"Result for Image {result.ImageId} sent");
        }

    }
}