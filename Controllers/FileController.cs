using Microsoft.AspNetCore.Mvc;
using ChatAppServer.Helpers;
using ChatAppServer.Models;
using ChatAppServer.Interfaces;
using System;

namespace ChatAppServer.Controllers;
[ApiController]
[Route("[controller]")]

public class FileController : ControllerBase
{
    private readonly IEntity<User> _userServices;
    private readonly IEntity<Group> _groupServices;
    public FileController(IEntity<User> userServices, IEntity<Group> groupServices)
    {
        _userServices = userServices;
        _groupServices = groupServices;
    }
    [HttpGet]
    [Route("DonloadFile")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        var uploader = new FileHandler();
        return await uploader.Download(fileName);
    }
    [HttpPost]
    [Route("SetProfilePicture")]
    public async Task<bool> SetProfilePicture(string userId, IFormFile file)
    {
        var uploader = new FileHandler();
        if (!uploader.IsFileAnImage(file.FileName))
        {
            return false;
        }
        string? result = await uploader.Upload(file);
        User? user = await _userServices.ReadFirst(x => x.Id.ToString() == userId);
        if (user == null || result == null)
        {
            return false;
        }
        user.AvatarUrl = result;
        if (!await _userServices.Update(user))
        {
            return false;
        }
        return true;
    }
    [HttpPost]
    [Route("SetGroupProfilePicture")]
    public async Task<bool> SetGroupProfilePicture(string groupId, IFormFile file)
    {
        var uploader = new FileHandler();
        if (!uploader.IsFileAnImage(file.FileName))
        {
            return false;
        }
        string? result = await uploader.Upload(file);
        Group? group = await _groupServices.ReadFirst(x => x.Id.ToString() == groupId);
        if (group == null || result == null)
        {
            return false;
        }
        group.AvatarUrl = result;
        if (!await _groupServices.Update(group))
        {
            return false;
        }
        return true;
    }

    [HttpPost]
    [Route("UploadFile")]
    public async Task<bool> UploadFile(IFormFile file)
    {
        var uploader = new FileHandler();
        string? result = await uploader.Upload(file);
        if (result == null)
        {
            return false;
        }
        return true;
    }
}