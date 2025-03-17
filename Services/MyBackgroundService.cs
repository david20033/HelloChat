using HelloChat.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HelloChat.Services
{
    public class MyBackgroundService: BackgroundService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public MyBackgroundService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                await _hubContext.Clients.All.SendAsync("Heartbeat", DateTime.Now);

            }
        }
    }
}
