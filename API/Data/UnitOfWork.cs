using API.Attributes;
using API.Interfaces;
using AutoMapper;

namespace API.Data;

[AutoRegister]
public class UnitOfWork(DataContext _context, IMapper _mapper) : IUnitOfWork
{
    public IUserRepository UserRepository => new UserRepository(_context, _mapper);
    public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);
    public ILikesRepository LikesRepository => new LikesRepository(_context);

    public async Task<bool> Complete() => await _context.SaveChangesAsync() > 0;
    public bool HasChanges() => _context.ChangeTracker.HasChanges();
}
