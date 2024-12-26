using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageProcessing.Core.Models;

namespace Components.Pages
{
    public partial class Coordinator : ComponentBase
    {
        private List<ImageMessage> jobQueue = new(); // Очередь заданий

        protected override async Task OnInitializedAsync()
        {
            // Инициализация данных
            await LoadJobQueue();
        }

        private Task LoadJobQueue()
        {
            // Пример: Добавление тестовых данных
            jobQueue.Add(new ImageMessage { ImageId = 1, MessageType = "Image" });
            jobQueue.Add(new ImageMessage { ImageId = 2, MessageType = "Image" });
            return Task.CompletedTask;
        }

        // Пример вызова StateHasChanged
        public void UpdateQueue(ImageMessage newJob)
        {
            jobQueue.Add(newJob);
            StateHasChanged(); // Обновляем интерфейс
        }
    }
}