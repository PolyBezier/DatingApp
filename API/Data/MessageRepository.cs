using API.Attributes;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

[AutoRegister]
public class MessageRepository(DataContext _context, IMapper _mapper) : IMessageRepository
{
    public void AddGroup(Group group) => _context.Groups.Add(group);

    public void AddMessage(Message message) => _context.Messages.Add(message);

    public void DeleteMessage(Message message) => _context.Messages.Remove(message);

    public async Task<Connection?> GetConnectionAsync(string connectionId) => await _context.Connections.FindAsync(connectionId);

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
            "Outbox" => query.Where(m => m.SenderUsername ==  messageParams.Username && !m.SenderDeleted),
            _ => query.Where(m => m.RecipientUsername == messageParams.Username && m.DateRead == null && !m.RecipientDeleted),
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender).ThenInclude(u => u!.Photos)
            .Include(m => m.Recipient).ThenInclude(u => u!.Photos)
            .Where(m =>
                (m.RecipientUsername == currentUserName &&
                m.SenderUsername == recipientUserName &&
                !m.RecipientDeleted) ||
                (m.RecipientUsername == recipientUserName &&
                m.SenderUsername == currentUserName &&
                !m.SenderDeleted))
            .OrderBy(m => m.MessageSent)
            .ToListAsync();

        var unreadMessages = messages
            .Where(m => m.DateRead == null && m.RecipientUsername == currentUserName)
            .ToList();

        if (unreadMessages.Any())
        {
            foreach (var message in unreadMessages)
                message.DateRead = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public void RemoveConnection(Connection connection) => _context.Connections.Remove(connection);

    public async Task<bool> SaveAllAsync() => await _context.SaveChangesAsync() > 0;
}
