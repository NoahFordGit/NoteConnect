using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NoteConnect.Models.Entities;
using NoteConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using NoteConnect.ViewModels;
using System.Security.Claims;

namespace NoteConnect.Controllers;

public class UserController : Controller
{

    private readonly IUserRepository _userRepo;
    private readonly UserManager<User> _userManager;

    public UserController(IUserRepository userRepo, UserManager<User> userManager)
    {
        _userRepo = userRepo;
        _userManager = userManager;
    } // End method

    public async Task<IActionResult> Index(string Id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null) 
        {
            return NotFound();
        } // End if

        var viewModel = new UserViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateGiven = user.DateCreated,
            HasPassword = await _userManager.HasPasswordAsync(user) // Gets password info (used to be in index)
        };

        return View(viewModel);
    } // End method

   public async Task<IActionResult> Edit(string Id)
    {
        var user = await _userRepo.ReadAsync(Id);
        if(user is null)
        {
            return RedirectToAction("Index");
        } // End if

        var viewModel = new UserViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateGiven = user.DateCreated
        };

        return View(viewModel);
    } // End method


    [HttpPost]
    [ValidateAntiForgeryToken]
    // The userManager is used here so that the base AspNetUser table gets udpated correctly
    public async Task<IActionResult> Edit(UserViewModel input)
    {
        if(ModelState.IsValid == false)
        {
            return View(input);
        } // End if
        var user = await _userManager.FindByIdAsync(input.UserId!);

        user.UserName = input.UserName;
        user.FirstName = input.FirstName;
        user.LastName = input.LastName;
        user.UserName = input.UserName;
        user.Email = input.Email;
        user.PhoneNumber = input.PhoneNumber;

        await _userManager.UpdateAsync(user);
        return RedirectToAction("Index");
    } // End method


    public async Task<IActionResult> Delete(string Id)
    {
        var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(Id != currentUser)
        {
            return Forbid();
        } // End if

        var user = await _userRepo.ReadAsync(Id);
        if (user is null)
        {
            return NotFound();
        } // End if

        return View(user);
    } // End method


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string Id, User user)
    {
        // Just for security purposes
        var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(Id != currentUser)
        {
            return Forbid();
        } // End if


        var userToDelete = await _userManager.FindByIdAsync(Id);
        if (user is null)
        {
            return NotFound();
        } // End if

        await _userManager.DeleteAsync(userToDelete);
        // Clear the session and log out basically
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction("Index", "Home");

    } // End method 
    
} // End class