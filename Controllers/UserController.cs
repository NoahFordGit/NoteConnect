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

    public async Task<IActionResult> Index(string Id)
    {
        var user = await _userRepo.ReadAsync(Id);
        if (user is null) 
        {
            return NotFound();
        } // End if

        ViewBag.Id = Id;
        return View(user);
    } // End method

//    public async Task<IActionResult> Edit(int UserId)
//     {
//         var user = await _userRepo.ReadAsync(UserId);
//         if(user is null)
//         {
//             return RedirectToAction("Index");
//         } // End if

//         return View(user);
//     } // End method


//     [HttpPost]
//     public async Task<IActionResult> Edit(int UserId, User user)
//     {
//         if(ModelState.IsValid == false)
//         {
//             return View(user);
//         } // End if

//         await _userRepo.UpdateAsync(UserId, user);
//         return RedirectToAction("Index");
//     } // End method


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
        await _userRepo.DeleteAsync(Id);
        return RedirectToAction("Index");

    } // End method 
    
} // End class