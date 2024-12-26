using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ImageProcessing.Core.Models;
using Microsoft.Extensions.Logging;
using ImageProcessing.Core.Interfaces;
using Microsoft.Extensions.Options;
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

        public Worker(IOptions<WorkerSettings> settings, UdpHelper udpHelper, ILogger<Worker> logger, IImageProcessor imageProcessor)
        {
            _settings = settings.Value;
            _udpHelper = udpHelper;
            _logger = logger;
            _coordinatorEndpoint = new IPEndPoint(IPAddress.Parse(_settings.CoordinatorIp), _settings.CoordinatorPort);
            _imageProcessor = imageProcessor;

            _logger.LogInformation($"Worker {_settings.WorkerId} started on port {_settings.Port}");
        }

        public async Task StartListening()
        {
            while (true)
            {
                try
                {
                    var message = await _udpHelper.ReceiveAsync<ImageMessage>();
                    if (message == null) continue;

                    if (message.MessageType == "Image")
                    {
                        _logger.LogInformation($"Worker {_settings.WorkerId}: Received Image {message.ImageId} for processing.");

                        var startTime = DateTime.Now;
                        var resultMessage = await _imageProcessor.ProcessImage(message);
                        var endTime = DateTime.Now;

                        _logger.LogInformation($"Worker {_settings.WorkerId}: Processed Image {message.ImageId} in {(endTime - startTime).TotalMilliseconds} ms.");
                        await SendResultToCoordinator(resultMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Worker {_settings.WorkerId}: Error while processing message: {ex.Message}");
                }
            }
        }

        private async Task SendResultToCoordinator(ImageMessage result)
        {
            try
            {
                await _udpHelper.SendAsync(result, _coordinatorEndpoint);
                _logger.LogInformation($"Worker {_settings.WorkerId}: Result for Image {result.ImageId} sent to coordinator.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Worker {_settings.WorkerId}: Error sending result for Image {result.ImageId} to coordinator: {ex.Message}");
            }
        }
    }
}