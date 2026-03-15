using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WaqfSystem.Web.Hubs
{
    [Authorize]
    public class MissionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);
            var governorateId = Context.User?.FindFirstValue("GovernorateId");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, $"inspector_{userId}");
            }

            if ((role == "REGIONAL_MGR" || role == "SYS_ADMIN" || role == "AUTH_DIRECTOR") && !string.IsNullOrWhiteSpace(governorateId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"manager_{governorateId}");
            }

            await base.OnConnectedAsync();
        }

        public async Task WatchMission(int missionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"mission_{missionId}");
        }

        public async Task UnwatchMission(int missionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"mission_{missionId}");
        }

        public static Task BroadcastStageChange(IHubContext<MissionHub> hub, int missionId, string newStage, string changedByName, string? notes)
        {
            return hub.Clients.Group($"mission_{missionId}").SendAsync("StageChanged", new
            {
                missionId,
                newStage,
                changedByName,
                notes
            });
        }

        public static Task BroadcastProgressUpdate(IHubContext<MissionHub> hub, int missionId, decimal progressPercent, int enteredCount)
        {
            return hub.Clients.Group($"mission_{missionId}").SendAsync("ProgressUpdated", new
            {
                missionId,
                progressPercent,
                enteredCount
            });
        }

        public static Task BroadcastNewNotification(IHubContext<MissionHub> hub, int userId, string title, string body)
        {
            return hub.Clients.Group($"user_{userId}").SendAsync("NewNotification", new
            {
                userId,
                title,
                body
            });
        }

        public static Task BroadcastCheckin(IHubContext<MissionHub> hub, int missionId, decimal lat, decimal lng, string inspectorName)
        {
            return hub.Clients.Group($"mission_{missionId}").SendAsync("CheckinReceived", new
            {
                missionId,
                lat,
                lng,
                inspectorName
            });
        }
    }
}
