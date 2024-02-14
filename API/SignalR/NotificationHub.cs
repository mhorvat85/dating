using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class NotificationHub : Hub
{
  private readonly IUnitOfWork _uow;
  private readonly IHubContext<PresenceHub> _presenceHub;

  public NotificationHub(IUnitOfWork uow, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
  {
    _uow = uow;
    _presenceHub = presenceHub;
  }

  public override async Task OnConnectedAsync()
  {
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception exception)
  {
    await base.OnDisconnectedAsync(exception);
  }

  public async Task NotificationSent(List<PhotoDto> photo)
  {
    var sender = await _uow.UserRepository.GetUserByUsernameAsync(Context.User.GetUsername());

    var recipients = await _uow.UserRepository.GetUsersIdInRoleAsync() ??
      throw new HubException("Not found user");

    await _presenceHub.Clients.Users(recipients).SendAsync("NotificationReceived",
      new { sender.KnownAs }, photo);
  }

  public async Task PhotoDeleted(int photoId)
  {
    var recipients = await _uow.UserRepository.GetUsersIdInRoleAsync() ??
      throw new HubException("Not found user");

    await _presenceHub.Clients.Users(recipients).SendAsync("PhotoRemoved", photoId);
  }

  public async Task onAppraise(int photoId)
  {
    var usersInRole = await _uow.UserRepository.GetUsersInRole();

    var connectionIds = new List<string>();
    foreach (var recipient in usersInRole)
    {
      var connection = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
      if (connection != null) connectionIds.AddRange(connection);
    }

    await _presenceHub.Clients.Clients(connectionIds).SendAsync("PhotoRemoved", photoId);
  }

  public async Task OnRoleChange(string userName)
  {
    var recipientId = (await _uow.UserRepository.GetUserByUsernameAsync(userName)).Id;

    await _presenceHub.Clients.User(recipientId.ToString()).SendAsync("RoleChanged");
  }
}
