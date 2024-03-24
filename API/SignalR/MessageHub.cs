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
    IMessageRepository _messageRepository,
    IUserRepository _userRepository,
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
        await AddToGroup(groupName);

        var messages = await _messageRepository.GetMessageThread(currentUser, otherUser!);

        await Clients.Group(groupName).SendAsync(ReceiveMessageThread, messages);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await RemoveFromMessageGroup();

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User!.GetUsername();

        if (username == createMessageDto.RecipientUsername?.ToLower())
            throw new HubException("You cannot send messages to yourself");

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername!);

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
        var group = await _messageRepository.GetMessageGroup(groupName);

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

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync())
            await Clients.Group(groupName).SendAsync(NewMessage, _mapper.Map<MessageDto>(message));
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<bool> AddToGroup(string groupName)
    {
        var group = await _messageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User!.GetUsername());

        if (group == null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        return await _messageRepository.SaveAllAsync();
    }

    private async Task RemoveFromMessageGroup()
    {
        var connection = await _messageRepository.GetConnectionAsync(Context.ConnectionId);

        if (connection != null)
            _messageRepository.RemoveConnection(connection);

        await _messageRepository.SaveAllAsync();
    }
}
