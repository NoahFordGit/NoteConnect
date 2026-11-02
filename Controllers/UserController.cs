using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NoteConnect.Models;
using NoteConnect.Models.Entities;
using NoteConnect.Services;

namespace NoteConnect.Controllers;

public class UserController : Controller
{

    private readonly IUserRepository _userRepo;

    public UserController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    } // End method

    public async Task<IActionResult> Index(int UserId)
    {
        var user = await _userRepo.ReadAsync(UserId);
        if (user is null) 
        {
            return NotFound();
        } // End if

        ViewBag.UserId = UserId;
        return View(user);
    } // End method


} // End class