﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static API.Helpers.Constants;

namespace API.Controllers;

[Authorize]
public class UsersController(
    IUnitOfWork _uow,
    IMapper _mapper,
    IPhotoService _photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        var username = User.GetUsername();

        var gender = await _uow.UserRepository.GetUserGender(username);
        userParams.CurrentUsername = username;

        if (string.IsNullOrEmpty(userParams.Gender))
            userParams.Gender = gender == Gender.Male ? Gender.Female : Gender.Male;

        var users = await _uow.UserRepository.GetMembersAsync(userParams);

        foreach (var user in users)
            user.Photos = user.Photos.Where(p => user.UserName == username || p.Approved).ToList();

        Response.AddPaginationHeader(new(
            users.CurrentPage,
            users.PageSize,
            users.TotalCount,
            users.TotalPages));

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        bool includePending = User.GetUsername() == username;

        var user = await _uow.UserRepository.GetMemberAsync(username);

        user!.Photos = user.Photos.Where(p => includePending || p.Approved).ToList();

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
            return NotFound();

        _mapper.Map(memberUpdateDto, user);

        if (await _uow.Complete())
            return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
            return NotFound();

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
        };

        user.Photos!.Add(photo);

        if (await _uow.Complete())
            return CreatedAtAction(nameof(GetUser), new
            {
                username = user.UserName,
            }, _mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem adding photo.");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
            return NotFound();

        var photo = user.Photos!.FirstOrDefault(x => x.Id == photoId);

        if (photo == null)
            return NotFound();

        if (!photo.Approved)
            return BadRequest("This photo is not approved yet");

        if (photo.IsMain)
            return BadRequest("This is already your main photo");

        var currentMain = user.Photos!.FirstOrDefault(x => x.IsMain);
        if (currentMain != null)
            currentMain.IsMain = false;

        photo.IsMain = true;

        if (await _uow.Complete())
            return NoContent();

        return BadRequest("Problem settin main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user!.Photos!.FirstOrDefault(x => x.Id == photoId);

        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error is { Message: var message })
                return BadRequest(message);
        }

        user.Photos!.Remove(photo);

        if (await _uow.Complete())
            return Ok();

        return BadRequest("Problem deleting photo");
    }
}
