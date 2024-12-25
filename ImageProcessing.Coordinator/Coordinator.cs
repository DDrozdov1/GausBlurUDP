using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using ImageProcessing.Core.Models;
using Microsoft.Extensions.Logging;
using ImageProcessing.Common;
using System.Threading.Tasks;

namespace ImageProcessing.Coordinator
{
    public class Coordinator
    {
        private readonly UdpHelper _udpHelper;
        private readonly ILogger<Coordinator> _logger;
        private readonly Queue<ImageMessage> _jobQueue = new Queue<ImageMessage>();
        private readonly IPEndPoint _workerEndpoint;
        private readonly CoordinatorSettings _settings;
        public Coordinator(CoordinatorSettings settings, UdpHelper udpHelper, ILogger<Coordinator> logger)
        {
            _udpHelper = udpHelper;
            _logger = logger;
            _settings = settings;
            _workerEndpoint = new IPEndPoint(IPAddress.Parse(_settings.WorkerIp), _settings.WorkerPort);
            _logger.LogInformation("Coordinator started on port " + _settings.Port);

        }
        public async Task StartListening()
        {
            while (true)
            {
                var result = await _udpHelper.ReceiveAsync();
                if (result == null || result.Value.Buffer == null) continue;
                var message = JsonConvert.DeserializeObject<ImageMessage>(Encoding.UTF8.GetString(result.Value.Buffer));
                if (message == null) continue;

                switch (message.MessageType)
                {
                    case "Image":
                        EnqueueJob(message);
                        _logger.LogInformation($"Received Image {message.ImageId}, current queue: {_jobQueue.Count}");
                        if (_jobQueue.Count > 0)
                            await DispatchJob();
                        break;
                    case "Result":
                        _logger.LogInformation($"Result from worker for ImageId: {message.ImageId}, Result: {message.Result}");
                        break;
                }
            }
        }
        public void EnqueueJob(ImageMessage message)
        {
            if (_jobQueue.Count >= 1000)
            {
                _logger.LogWarning($"Maximum queue limit of 1000 reached");
                return; // Лимит в 1000
            }
            _jobQueue.Enqueue(message);
            _logger.LogInformation($"Image {message.ImageId} added to queue. Queue count: {_jobQueue.Count}");
        }
        private async Task DispatchJob()
        {
            if (_jobQueue.Count <= 0)
                return;
            var job = _jobQueue.Dequeue();
            await _udpHelper.SendAsync(job, _workerEndpoint);
            _logger.LogInformation($"Image {job.ImageId} sent to worker");
        }
    }
}