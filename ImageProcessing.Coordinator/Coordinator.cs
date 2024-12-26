using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using ImageProcessing.Core.Models;
using Microsoft.Extensions.Logging;
using ImageProcessing.Common;
using System.Threading;
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
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _queueLock = new object(); // Для синхронизации доступа к очереди

        public Coordinator(CoordinatorSettings settings, UdpHelper udpHelper, ILogger<Coordinator> logger)
        {
            _udpHelper = udpHelper;
            _logger = logger;
            _settings = settings;
            _workerEndpoint = new IPEndPoint(IPAddress.Parse(_settings.WorkerIp), _settings.WorkerPort);
            _logger.LogInformation("Coordinator started on port " + _settings.Port);
        }

        /// <summary>
        /// Запуск обработки входящих UDP-сообщений и отправки заданий
        /// </summary>
        public async Task StartAsync()
        {
            // Запускаем задачи для обработки входящих сообщений и отправки заданий
            var listeningTask = StartListeningAsync();
            var dispatchingTask = StartDispatchingAsync();

            await Task.WhenAll(listeningTask, dispatchingTask); // Ждём завершения обеих задач
        }

        /// <summary>
        /// Обработка входящих UDP-сообщений
        /// </summary>
        private async Task StartListeningAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var message = await _udpHelper.ReceiveAsync<ImageMessage>();
                    if (message == null) continue;

                    _logger.LogInformation($"Received message of type {message.MessageType} for ImageId: {message.ImageId}");

                    switch (message.MessageType)
                    {
                        case "Image":
                            EnqueueJob(message);
                            break;
                        case "Result":
                            _logger.LogInformation($"Result received from worker for ImageId: {message.ImageId}, Result: {message.Result}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while listening for UDP messages: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавление задания в очередь
        /// </summary>
        private void EnqueueJob(ImageMessage message)
        {
            lock (_queueLock) // Блокируем очередь для потокобезопасности
            {
                if (_jobQueue.Count >= 1000)
                {
                    _logger.LogWarning($"Maximum queue limit of 1000 reached. Dropping ImageId: {message.ImageId}");
                    return;
                }

                _jobQueue.Enqueue(message);
                _logger.LogInformation($"Image {message.ImageId} added to queue. Current queue size: {_jobQueue.Count}");
            }
        }

        /// <summary>
        /// Фоновая отправка заданий из очереди рабочим узлам
        /// </summary>
        private async Task StartDispatchingAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    ImageMessage? job = null;

                    lock (_queueLock) // Блокируем очередь для потокобезопасного извлечения
                    {
                        if (_jobQueue.Count > 0)
                        {
                            job = _jobQueue.Dequeue();
                        }
                    }

                    if (job != null)
                    {
                        await _udpHelper.SendAsync(job, _workerEndpoint);
                        _logger.LogInformation($"Dispatched Image {job.ImageId} to worker at {_workerEndpoint}");
                    }

                    await Task.Delay(100); // Задержка, чтобы не перегружать процессор
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while dispatching jobs: {ex.Message}");
            }
        }

        /// <summary>
        /// Остановка всех фоновых задач
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}