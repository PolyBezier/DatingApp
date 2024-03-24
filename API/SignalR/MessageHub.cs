using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using static API.Helpers.Constants.SignalRMessages;

namespace API.SignalR;

public class MessageHub(
    IMessageRepository _messageRepository,
    IUserRepository _userRepository,
    IMapper _mapper) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var currentUser = Context.User!.GetUsername();

        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext!.Request.Query["user"];
        var groupName = GetGroupName(currentUser, otherUser!);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var messages = await _messageRepository.GetMessageThread(currentUser, otherUser!);

        await Clients.Group(groupName).SendAsync(ReceiveMessageThread, messages);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
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

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync())
        {
            var group = GetGroupName(sender.UserName!, recipient.UserName!);
            await Clients.Group(group).SendAsync(NewMessage, _mapper.Map<MessageDto>(message));
        }
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
