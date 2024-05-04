using LMS.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace LMS.Web.Controllers
{
    public class AuthorController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthorController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7163/api/");
        }

        public async Task<IActionResult> AuthorList()
        {
            var response = await _httpClient.GetAsync("Author");
            if (response.IsSuccessStatusCode)
            {
                var authors = await response.Content.ReadAsAsync<List<AuthorModel>>();
                return View("_AuthorList",authors);
            }
            else
            {
                
                return View("Error");
            }
        }
        public IActionResult AddAuthor()
        {
            AuthorModel model = new AuthorModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateAuthor(AuthorModel author)
        {
            try
            {
                var jsonAuthor = JsonSerializer.Serialize(author);
                var content = new StringContent(jsonAuthor, Encoding.UTF8, "application/json");
                if (author.AuthorID == 0)
                {
                    var response = await _httpClient.PostAsync("Author", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Author added successfully.";
                        return RedirectToAction(nameof(AddAuthor));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    var response = await _httpClient.PutAsync($"Author/{author.AuthorID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Author updated successfully.";
                        return RedirectToAction(nameof(AddAuthor));
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
        public async Task<IActionResult> EditAuthor(int id)
        {
            var response = await _httpClient.GetAsync($"Author/{id}");
            if (response.IsSuccessStatusCode)
            {
                var author = await response.Content.ReadAsAsync<AuthorModel>();
                return View("AddAuthor", author);
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> DeleteAuthor(int id)
        {
            try
            {
              
                var bookList = await _httpClient.GetAsync("Book");
                if (!bookList.IsSuccessStatusCode)
                {
                    return View("Not Found");
                }

                var books = await bookList.Content.ReadAsAsync<List<BookModel>>();
                var bookAuthorExists = books.FirstOrDefault(book => book.AuthorID == id) != null;
                if (bookAuthorExists)
                {
                    TempData["ErrorMessage"] = "Author can't be deleted as it exists in bookList.";
                }
                else
                {
                    var deleteResponse = await _httpClient.DeleteAsync($"Author/{id}");
                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Author deleted successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to delete author.";
                    }
                }

                return RedirectToAction(nameof(AuthorList));

            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }

    }
}
