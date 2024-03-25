using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext _context, IMapper _mapper) : IMessageRepository
{
    public void AddGroup(Group group) => _context.Groups.Add(group);

    public void AddMessage(Message message) => _context.Messages.Add(message);

    public void DeleteMessage(Message message) => _context.Messages.Remove(message);

    public async Task<Connection?> GetConnectionAsync(string connectionId) => await _context.Connections.FindAsync(connectionId);

    public Task<Group?> GetGroupForConnection(string connectionId) => _context.Groups
        .Include(x => x.Connections)
        .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
        .FirstOrDefaultAsync();

    public async Task<Message?> GetMessage(int id) => await _context.Messages.FindAsync(id);

    public Task<Group?> GetMessageGroup(string groupName) => _context.Groups
        .Include(x => x.Connections)
        .FirstOrDefaultAsync(x => x.Name == groupName);

    public Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = _context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.RecipientUsername == messageParams.Username && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.SenderUsername == messageParams.Username && !m.SenderDeleted),
            _ => query.Where(m => m.RecipientUsername == messageParams.Username && m.DateRead == null && !m.RecipientDeleted),
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
    {
        var query = _context.Messages
            .Where(m =>
                (m.RecipientUsername == currentUserName &&
                m.SenderUsername == recipientUserName &&
                !m.RecipientDeleted) ||
                (m.RecipientUsername == recipientUserName &&
                m.SenderUsername == currentUserName &&
                !m.SenderDeleted))
            .OrderBy(m => m.MessageSent)
            .AsQueryable();

        var unreadMessages = query
            .Where(m => m.DateRead == null && m.RecipientUsername == currentUserName)
            .ToList();

        if (unreadMessages.Any())
            foreach (var message in unreadMessages)
                message.DateRead = DateTime.UtcNow;

        return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveConnection(Connection connection) => _context.Connections.Remove(connection);
}
