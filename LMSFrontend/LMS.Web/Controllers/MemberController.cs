using Azure;
using LMS.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace LMS.Web.Controllers
{
    public class MemberController : Controller
    {
        private readonly HttpClient _httpClient;

        public MemberController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7163/api/");
        }
        public async Task<IActionResult> MemberList()
        {
            var userId = TempData.Peek("UserId") as int?;
            var userRole = TempData.Peek("Role") as string;
            var response = await _httpClient.GetAsync("Member");
            List<MemberModel> filteredMembers = new List<MemberModel>(); 

            if (response.IsSuccessStatusCode)
            {
                var members = await response.Content.ReadAsAsync<List<MemberModel>>();
                if (userRole == "Member")
                {
                   filteredMembers = members.Where(x => x.UserId == userId).ToList();
                }
                else if(userRole == "Admin")
                {
                   filteredMembers = members.ToList();
                }
                
                return View("_MemberList", filteredMembers);
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> AddMember()
        {
            MemberModel model = new MemberModel();
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateMember(MemberModel member)
        {
            try
            {
                var userId = TempData.Peek("UserId") as int?;
                member.UserId = Convert.ToInt32(userId);
                var jsonMember = JsonSerializer.Serialize(member);
                var content = new StringContent(jsonMember, Encoding.UTF8, "application/json");
                
                if (member.MemberID == 0)
                {
                    
                    var response = await _httpClient.PostAsync("Member", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Memebr added successfully.";
                        return RedirectToAction(nameof(AddMember));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    var response = await _httpClient.PutAsync($"Member/{member.MemberID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Member updated successfully.";
                        return RedirectToAction(nameof(AddMember));
                    }
                    else
                    {
                        return View("Error");
                    }
                }


            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }
        public async Task<IActionResult> EditMember(int id)
        {
            // Fetch the member details
            var memberResponse = await _httpClient.GetAsync($"Member/{id}");
            if (memberResponse.IsSuccessStatusCode)
            {
                var member = await memberResponse.Content.ReadAsAsync<MemberModel>();
                return View("AddMember", member);
            }

          else
            {
                return View("Error");
            }

        }

        public async Task<IActionResult> DeleteMember(int id)
        {
            try
            {

                var memberList = await _httpClient.GetAsync("BorrowdBook");
                if (!memberList.IsSuccessStatusCode)
                {
                    return View("Not Found");
                }

                var members = await memberList.Content.ReadAsAsync<List<BorrowdBooks>>();
                var borrowdBookExists = members.FirstOrDefault(member => member.MemberID == id) != null;
                if (borrowdBookExists)
                {
                    TempData["ErrorMessage"] = "Memebr can't be deleted as it exists in borrowdBookList.";
                }
                else
                {
                    var deleteResponse = await _httpClient.DeleteAsync($"Member/{id}");
                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Member deleted successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to delete member.";
                    }
                }

                return RedirectToAction(nameof(MemberList));

            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }
    }
}
