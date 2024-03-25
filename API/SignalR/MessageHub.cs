using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static API.Helpers.Constants.SignalRMessages;

namespace API.SignalR;

[Authorize]
public class MessageHub(
    IUnitOfWork _uow,
    IMapper _mapper,
    IHubContext<PresenceHub> _presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var currentUser = Context.User!.GetUsername();

        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext!.Request.Query["user"];
        var groupName = GetGroupName(currentUser, otherUser!);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync(UpdateGroup, group);

        var messages = await _uow.MessageRepository.GetMessageThread(currentUser, otherUser!);

        if (_uow.HasChanges())
            await _uow.Complete();

        await Clients.Caller.SendAsync(ReceiveMessageThread, messages);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroup();

        await Clients.Group(group.Name).SendAsync(UpdateGroup);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User!.GetUsername();

        if (username == createMessageDto.RecipientUsername?.ToLower())
            throw new HubException("You cannot send messages to yourself");

        var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername!);

        if (recipient == null)
            throw new HubException("Not found user");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender!.UserName!,
            RecipientUsername = recipient.UserName!,
            Content = createMessageDto.Content!,
        };

        var groupName = GetGroupName(sender.UserName!, recipient.UserName!);
        var group = await _uow.MessageRepository.GetMessageGroup(groupName);

        if (group?.Connections.Any(c => c.Username == recipient.UserName) == true)
            message.DateRead = DateTime.UtcNow;
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName!);

            if (connections != null)
                await _presenceHub.Clients.Clients(connections).SendAsync(NewMessageReceived, new
                {
                    username = sender.UserName,
                    knownAs = sender.KnownAs,
                });
        }

        _uow.MessageRepository.AddMessage(message);

        if (await _uow.Complete())
            await Clients.Group(groupName).SendAsync(NewMessage, _mapper.Map<MessageDto>(message));
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _uow.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User!.GetUsername());

        if (group == null)
        {
            group = new Group(groupName);
            _uow.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await _uow.Complete())
            return group;

        throw new HubException("Failed to add to group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

        if (connection != null)
        {
            _uow.MessageRepository.RemoveConnection(connection);

            if (await _uow.Complete())
                return group!;
        }

        throw new HubException("Failed to remove from group");
    }
}
