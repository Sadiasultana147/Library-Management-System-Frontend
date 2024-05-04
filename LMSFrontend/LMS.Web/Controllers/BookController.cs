using LMS.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;
namespace LMS.Web.Controllers
{
    public class BookController : Controller
    {
        private readonly HttpClient _httpClient;

        public BookController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7163/api/");
        }

        public async Task<IActionResult> BookList()
        {
            var response = await _httpClient.GetAsync("Book");
            if (response.IsSuccessStatusCode)
            {
                var books = await response.Content.ReadAsAsync<List<BookModel>>();
                return View("_BookList", books);
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> AddBook()
        {
            
            HttpResponseMessage response = await _httpClient.GetAsync("Author");
            var authors = await response.Content.ReadAsAsync<List<AuthorModel>>();
           var authorList = authors.Select(d => new SelectListItem
            {
                Text = d.AuthorName,
                Value = d.AuthorID.ToString()
            }).ToList();            
            var model = new BookModel
            {
                AuthorList = authorList
            };
            return View(model);
          
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateBook(BookModel book)
        {
            try
            {
                var jsonBook = JsonSerializer.Serialize(book);
                var content = new StringContent(jsonBook, Encoding.UTF8, "application/json");
                if (book.BookID == 0)
                {
                    var response = await _httpClient.PostAsync("Book", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Book added successfully.";
                        return RedirectToAction(nameof(AddBook));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    var response = await _httpClient.PutAsync($"Book/{book.BookID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Book updated successfully.";
                        return RedirectToAction(nameof(AddBook));
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
        public async Task<IActionResult> EditBook(int id)
        {
            // Fetch the book details
            var bookResponse = await _httpClient.GetAsync($"Book/{id}");
            if (!bookResponse.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var book = await bookResponse.Content.ReadAsAsync<BookModel>();
            var authorResponse = await _httpClient.GetAsync("Author");
            if (!authorResponse.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var authors = await authorResponse.Content.ReadAsAsync<List<AuthorModel>>();
            var authorList = authors.Select(d => new SelectListItem
            {
                Text = d.AuthorName,
                Value = d.AuthorID.ToString()
            }).ToList();
            authorList.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            book.AuthorList = authorList;
            return View("AddBook", book);
        }

        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {

                var bookList = await _httpClient.GetAsync("BorrowdBook");
                if (!bookList.IsSuccessStatusCode)
                {
                    return View("Not Found");
                }

                var books = await bookList.Content.ReadAsAsync<List<BorrowdBooks>>();
                var borrowdBookExists = books.FirstOrDefault(book => book.BookID == id) != null;
                if (borrowdBookExists)
                {
                    TempData["ErrorMessage"] = "Book can't be deleted as it exists in borrowdBookList.";
                }
                else
                {
                    var deleteResponse = await _httpClient.DeleteAsync($"Book/{id}");
                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Book deleted successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to delete book.";
                    }
                }

                return RedirectToAction(nameof(BookList));

            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }
    }
}
