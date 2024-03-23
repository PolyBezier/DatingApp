using API.Attributes;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static API.Helpers.Constants;

namespace API.Data;

[AutoRegister]
public class LikesRepository(DataContext _context) : ILikesRepository
{
    public async Task<UserLike?> GetUserLike(int sourceUserId, int targedUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, targedUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if (likesParams.Predicate == LikePredicates.Liked)
        {
            likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
            users = likes.Select(like => like.TargetUser!);
        }

        if (likesParams.Predicate == LikePredicates.LikedBy)
        {
            likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
            users = likes.Select(like => like.SourceUser!);
        }

        var likedUsers = users.Select(user => new LikeDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos!.FirstOrDefault(x => x.IsMain)!.Url,
            City = user.City,
        });

        return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }

    public Task<AppUser?> GetUserWithLikes(int userId)
    {
        return _context.Users
            .Include(x => x.LikedUsers)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
