using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NoteConnect.Models.Entities;
using NoteConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

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
        Console.WriteLine($"DEBUG: Claim ID = '{Id}'"); 
        var user = await _userManager.GetUserAsync(User);
       // var user = await _userRepo.ReadAsync(Id);
        //var user = await _userRepo.ReadAsync(User);
        if (user is null) 
        {
            return NotFound();
        } // End if

        return View(user);
    } // End method

   public async Task<IActionResult> Edit(string Id)
    {
        var user = await _userRepo.ReadAsync(Id);
        if(user is null)
        {
            return RedirectToAction("Index");
        } // End if

        return View(user);
    } // End method


    [HttpPost]
    public async Task<IActionResult> Edit(string Id, User input)
    {
        if(ModelState.IsValid == false)
        {
            // Added this bit for troubleshooting
            foreach (var state in ModelState.Values)
            foreach (var error in state.Errors)
                Console.WriteLine($"Error: {error.ErrorMessage}");
            return View(input);
        } // End if
        var user = await _userRepo.ReadAsync(Id);

        // Just including fields that we need right now
       // user.Id = Id;
        user.FirstName = input.FirstName;
        user.LastName = input.LastName;user.UserName = input.UserName;
        user.Email = input.Email;
        user.PhoneNumber = input.PhoneNumber;

        await _userRepo.UpdateAsync(user);
        return RedirectToAction("Index");
    } // End method


    public async Task<IActionResult> Delete(string Id)
    {
        var user = await _userRepo.ReadAsync(Id);
        if(user is null)
        {
            return RedirectToAction("Index");
        } // End if

        return View(user);
    } // End method


    [HttpPost]
    public async Task<IActionResult> Delete(string Id, User user)
    {
        await _userRepo.DeleteAsync(user);
        // Clear the session and log out basically
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction("Index", "Home");

    } // End method 
    
} // End class