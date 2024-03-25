using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext _context, IMapper _mapper) : IUserRepository
{
    public Task<MemberDto?> GetMemberAsync(string username)
    {
        return _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        var query = _context.Users.AsQueryable()
            .Where(u => u.UserName != userParams.CurrentUsername)
            .Where(u => u.Gender == userParams.Gender)
            .Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive),
        };

        return PagedList<MemberDto>.CreateAsync(
            query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
            userParams.PageNumber,
            userParams.PageSize);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return _context.Users
            .Include(e => e.Photos)
            .FirstOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<string?> GetUserGender(string username) => await _context.Users
        .Where(x => x.UserName == username)
        .Select(x => x.Gender)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(e => e.Photos)
            .ToListAsync();
    }

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }
}
