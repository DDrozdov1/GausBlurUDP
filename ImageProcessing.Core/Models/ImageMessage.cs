using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Models
{
    public class ImageMessage
    {
        public string MessageType { get; set; } // "Image", "Result"
        public int ImageId { get; set; }
        public byte[]? ImageData { get; set; }
        public string? Result { get; set; }
        public int PacketNumber { get; set; }
        public int TotalPackets { get; set; }
    }
}