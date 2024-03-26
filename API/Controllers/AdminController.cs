using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static API.Helpers.Constants;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> _userManager, IUnitOfWork _uow) : BaseApiController
{
    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles!.Select(ur => ur.Role!.Name).ToList(),
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPut("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
    {
        if (string.IsNullOrEmpty(roles))
            return BadRequest("You must select at least one role");

        var selectedRoles = roles.Split(',');

        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded)
            return BadRequest("Failed to add to roles");

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded)
            return BadRequest("Failed to remove from roles");

        return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = Policies.ModeratePhotoRole)]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<UserPhotoDto>>> GetPhotosForModeration()
    {
        var users = await _uow.UserRepository.GetUsersWithPhotosToApproveAsync();

        var result = new List<UserPhotoDto>();

        foreach (var user in users)
            foreach (var photo in user.Photos!)
                result.Add(new()
                {
                    Id = photo.Id,
                    IsMain = photo.IsMain,
                    Url = photo.Url,
                    Username = user.UserName!,
                });

        return Ok(result);
    }

    [Authorize(Policy = Policies.ModeratePhotoRole)]
    [HttpPut("approve-photo")]
    public async Task<ActionResult> ApprovePhoto(ApprovePhotoDto approveDto)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(approveDto.Username);

        if (user == null)
            return NotFound("User not found");

        var photo = user.Photos!.FirstOrDefault(p => p.Id == approveDto.Id);

        if (photo == null)
            return NotFound("Photo not found");

        if (approveDto.Approve)
        {
            photo.Approved = true;
            if (user.Photos!.None(p => p.IsMain))
                photo.IsMain = true;
        }
        else
            user.Photos!.Remove(photo);

        await _uow.Complete();

        return Ok();
    }
}
