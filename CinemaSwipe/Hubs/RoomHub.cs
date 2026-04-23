using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CinemaSwipe.Hubs;

[Authorize]
public class RoomHub : Hub
{
    // Client calls this to subscribe to a room's events
    public async Task JoinRoomGroup(string roomId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room-{roomId}");

    public async Task LeaveRoomGroup(string roomId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room-{roomId}");
}

// Helper: call from services/pages to push events to a room
public static class RoomHubExtensions
{
    public static async Task NotifyMemberJoined(
        IHubContext<RoomHub> hub, int roomId, string displayName) =>
        await hub.Clients.Group($"room-{roomId}").SendAsync("MemberJoined", displayName);

    public static async Task NotifySwipingStarted(
        IHubContext<RoomHub> hub, int roomId) =>
        await hub.Clients.Group($"room-{roomId}").SendAsync("SwipingStarted");

    public static async Task NotifyVoteCast(
        IHubContext<RoomHub> hub, int roomId, string userId, int position, bool isLike) =>
        await hub.Clients.Group($"room-{roomId}").SendAsync("VoteCast", userId, position, isLike);

    public static async Task NotifyRoomDone(
        IHubContext<RoomHub> hub, int roomId, string winnerTitle, string winnerPoster) =>
        await hub.Clients.Group($"room-{roomId}").SendAsync("RoomDone", winnerTitle, winnerPoster);
}
