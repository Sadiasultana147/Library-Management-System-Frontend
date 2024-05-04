using LMS.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace LMS.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7163/api/");
        }
        public IActionResult RegisterUser()
        {
            UserModel model = new UserModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserModel user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("User/Register", data);

                if (response.IsSuccessStatusCode)
                {

                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "User registration failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while registering the user");
            }
        }

        public IActionResult Login()
        {
            LoginModel model = new LoginModel();
            return View(model);
        }

        
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginModel);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("User/Login", data);

                if (response.IsSuccessStatusCode)
                {
                    
                    var content = await response.Content.ReadAsStringAsync();

                    var user = JsonSerializer.Deserialize<UserModel>(content);
                    var userId = HttpContext.Session.GetInt32("UserId");
                    var email = HttpContext.Session.GetString("Email");
                    var password = HttpContext.Session.GetString("Password");
                    var firstName = HttpContext.Session.GetString("FirstName");
                    var lastName = HttpContext.Session.GetString("LastName");
                    var role = HttpContext.Session.GetString("Role");

                    // Store user information in TempData
                    TempData["UserId"] = user.UserId;
                    TempData["Email"] = user.Email;
                    TempData["FirstName"] = user.FirstName;
                    TempData["LastName"] = user.LastName;
                    TempData["Role"] = user.Role;
                    return RedirectToAction("Index", "Home");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return Unauthorized("Invalid email or password");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Login failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing the login request");
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            TempData.Clear();

            return RedirectToAction("Index", "Home");
        }

    }
}

