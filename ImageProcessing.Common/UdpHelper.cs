using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace ImageProcessing.Common
{
    public class UdpHelper
    {
        private readonly UdpClient _udpClient;
        private readonly ILogger<UdpHelper> _logger;
        public UdpHelper(ILogger<UdpHelper> logger, int port)
        {
            _logger = logger;
            _udpClient = new UdpClient(port);
            _logger.LogInformation($"UdpHelper listening on port {port}");
        }
        public async Task<UdpReceiveResult?> ReceiveAsync()
        {
            try
            {
                return await _udpClient.ReceiveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error receiving data: {ex.Message}");
                return null;
            }
        }
        public async Task SendAsync<T>(T data, IPEndPoint endPoint)
        {
            try
            {
                var jsonToSend = JsonConvert.SerializeObject(data);
                var dataToSend = Encoding.UTF8.GetBytes(jsonToSend);
                await _udpClient.SendAsync(dataToSend, dataToSend.Length, endPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending data: {ex.Message}");
            }

        }
        public async Task<T?> ReceiveAsync<T>()
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                var json = Encoding.UTF8.GetString(result.Buffer);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error receiving data: {ex.Message}");
                return default;
            }
        }
    }
}