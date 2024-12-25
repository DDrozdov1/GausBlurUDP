using ImageProcessing.Core.Models;
using System.Threading.Tasks;
namespace ImageProcessing.Core.Interfaces
{
    public interface IImageProcessor
    {
        Task<ImageMessage> ProcessImage(ImageMessage message);
    }
}