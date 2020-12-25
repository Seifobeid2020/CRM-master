using CRM.Models;
using CRM.View_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Controllers
{
    // This Class Handles User Related Information
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        public UserController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        
        // Controller To Get a Specific User's Information (Logged in User).
        [HttpGet]
        [Route("getUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            //ClaimsPrincipal currentUser = this.User;
            var username = HttpContext.User.Identity?.Name;
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                IList<string> userRole = await _userManager.GetRolesAsync(user);

                UserInfo response = new UserInfo()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = userRole[0]
                };

                return Ok(response);
            }
            else
            {
                return NotFound(new Response { Status = "Failed", Message = "User Not Found!" });
            }

        }

        // Controller To Get All The User's Information (In The System).
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [Route("getAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            IList<User> users = await _userManager.Users.ToListAsync();

            if (users.Count > 0)
            {
                IList<Object> response = new List<Object>();

                foreach (User user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    response.Add(new
                    {
                        id = user.Id,
                        username = user.UserName,
                        email = user.Email,
                        role = roles[0]
                    });
                }
                return Ok(response);
            }
            else
            {
                return NotFound(new Response { Status = "Not Found", Message = "There is no users" });
            }
        }

        // Controller To Edit a Specific User Information.
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut]
        [Route("editUser/{id}")]
        public async Task<IActionResult> EditUser(string id, [FromBody] UserInfo user)
        {
            var existingUser = _userManager.Users.FirstOrDefault(u => u.Id == id);
            
            var userRole = await _userManager.GetRolesAsync(existingUser);
            
            if (existingUser != null)
            {
                existingUser.UserName = user.Username;
                existingUser.Email = user.Email;

                await _userManager.UpdateAsync(existingUser);

                if (userRole[0] != user.Role)
                {
                    await _userManager.RemoveFromRoleAsync(existingUser, userRole[0]);
                    await _userManager.AddToRoleAsync(existingUser, user.Role);

                    var role = await _userManager.GetRolesAsync(existingUser);

                    Console.WriteLine(role[0]);
                }

            }
            else
            {
                return NotFound();
            }
            return Ok();
        }
        
        // Controller To Delete a Specific User.
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete]
        [Route("deleteUser/{id}")]
        public async Task<IActionResult> EditUser(string id)
        {
            var existingUser = _userManager.Users.FirstOrDefault(u => u.Id == id);

            if (existingUser != null)
            {
                await _userManager.DeleteAsync(existingUser);
            }
            else
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
