using API.Attributes;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.DTOs;

[AutoRegister]
public class LikesRepository(DataContext _context) : ILikesRepository
{
    public async Task<UserLike?> GetUserLike(int sourceUserId, int targedUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, targedUserId);
    }

    public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
    {
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if (predicate == "liked")
        {
            likes = likes.Where(like => like.SourceUserId == userId);
            users = likes.Select(like => like.TargetUser!);
        }

        if (predicate == "likedBy")
        {
            likes = likes.Where(like => like.TargetUserId == userId);
            users = likes.Select(like => like.SourceUser!);
        }

        return await users.Select(user => new LikeDto
        {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos!.FirstOrDefault(x => x.IsMain)!.Url,
            City = user.City,
        }).ToListAsync();
    }

    public Task<AppUser?> GetUserWithLikes(int userId)
    {
        return _context.Users
            .Include(x => x.LikedUsers)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
