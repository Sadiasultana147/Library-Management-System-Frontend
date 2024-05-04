using Azure;
using LMS.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Reflection.Metadata.BlobBuilder;

namespace LMS.Web.Controllers
{
    public class BorrowdBooksController : Controller
    {
        private readonly HttpClient _httpClient;

        public BorrowdBooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7163/api/");
        }
        public async Task<IActionResult> BorrowdBookList()
        {
            var userId = TempData.Peek("UserId") as int?;
            var userRole = TempData.Peek("Role") as string;
            List<BorrowdBooks> filteredBooks = new List<BorrowdBooks>();

            var response = await _httpClient.GetAsync("BorrowdBook");
            if (response.IsSuccessStatusCode)
            {
                var books = await response.Content.ReadAsAsync<List<BorrowdBooks>>();

                // Read users and members separately
                response = await _httpClient.GetAsync("User");
                var users = await response.Content.ReadAsAsync<List<UserModel>>();

                response = await _httpClient.GetAsync("Member");
                var members = await response.Content.ReadAsAsync<List<MemberModel>>();

                // Filter user and member based on userId
                var currentUser = users.FirstOrDefault(x => x.UserId == userId);
                var currentMember = members.FirstOrDefault(x => x.UserId == userId);

                if (currentUser != null && currentMember != null)
                {
                    if (userRole == "Member")
                    {
                        filteredBooks = books.Where(x => x.MemberID == currentMember.MemberID).ToList();
                    }
                   
                }
                else if (userRole == "Admin")
                {
                    filteredBooks = books.ToList();
                }
                return View("_BorrowdBookList", filteredBooks);
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> BorrowdBook()
        {

            HttpResponseMessage authorresponse = await _httpClient.GetAsync("Member");
            HttpResponseMessage bookresponse = await _httpClient.GetAsync("Book");           
            var members = await authorresponse.Content.ReadAsAsync<List<MemberModel>>();
            var MemberList = members.Select(d => new SelectListItem
            {
                Text = d.FirstName + ' ' + d.LastName,
                Value = d.MemberID.ToString()
            }).ToList();
           
            var Books = await bookresponse.Content.ReadAsAsync<List<BookModel>>();
            var bookList = Books.Select(d => new SelectListItem
            {
                Text = d.Title,
                Value = d.BookID.ToString()
            }).ToList();
            
            var statusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Borrowed", Text = "Borrowed" },
                new SelectListItem { Value = "Returned", Text = "Returned" },
                new SelectListItem { Value = "Overdue", Text = "Overdue" }
            };
            var model = new BorrowdBooks
            {
                MemberList = MemberList,
                BookList = bookList,
                statusList= statusList
            };
            return View(model);

        }
       
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateBorrowdBook(BorrowdBooks book)
        {
            try
            {
                var jsonBook = JsonSerializer.Serialize(book);
                var content = new StringContent(jsonBook, Encoding.UTF8, "application/json");
                var result = await _httpClient.GetAsync("BorrowdBook");
                var borrowdBooks = await result.Content.ReadAsAsync<List<BorrowdBooks>>();
                var memberBorrowedBooks = borrowdBooks.Where(b => b.MemberID == book.MemberID && b.status != "Returned").ToList();
                if (book.BorrowID == 0)
                {
                    if (memberBorrowedBooks.Any())
                    {
                        TempData["ErrorMessage"] = "Previous book not returned/Overdue";
                        return RedirectToAction(nameof(BorrowdBook));
                    }
                    var response = await _httpClient.PostAsync("BorrowdBook", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Book borrowed successfully.";
                        return RedirectToAction(nameof(BorrowdBook));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
               
                else
                {
                    var response = await _httpClient.PutAsync($"BorrowdBook/{book.BorrowID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Borrow updated successfully.";
                        return RedirectToAction(nameof(BorrowdBook));
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

        public async Task<IActionResult> EditBorrowdBook(int id)
        {
            var bookResponse = await _httpClient.GetAsync($"BorrowdBook/{id}");
            if (!bookResponse.IsSuccessStatusCode)
            {
                return View("Error");
            }
            var borrowdbook = await bookResponse.Content.ReadAsAsync<BorrowdBooks>();
            HttpResponseMessage authorresponse = await _httpClient.GetAsync("Member");
            HttpResponseMessage bookresponse = await _httpClient.GetAsync("Book");
            var members = await authorresponse.Content.ReadAsAsync<List<MemberModel>>();
            var MemberList = members.Select(d => new SelectListItem
            {
                Text = d.FirstName + ' ' + d.LastName,
                Value = d.MemberID.ToString()
            }).ToList();
            MemberList.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var Books = await bookresponse.Content.ReadAsAsync<List<BookModel>>();
            var bookList = Books.Select(d => new SelectListItem
            {
                Text = d.Title,
                Value = d.BookID.ToString()
            }).ToList();
            bookList.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var model = new BorrowdBooks
            {
                MemberList = MemberList,
                BookList = bookList
            };
            var statusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Borrowed", Text = "Borrowed" },
                new SelectListItem { Value = "Returned", Text = "Returned" },
                new SelectListItem { Value = "Overdue", Text = "Overdue" }
            };
            borrowdbook.MemberList = MemberList;
            borrowdbook.BookList = bookList;
            borrowdbook.statusList = statusList;
            return View("BorrowdBook",borrowdbook);
        }
        public async Task<IActionResult> DeleteBorrowdBook(int id)
        {
            try
            {

                var deleteResponse = await _httpClient.DeleteAsync($"BorrowdBook/{id}");
                if (deleteResponse.IsSuccessStatusCode)
                {
                   TempData["SuccessMessage"] = "Borrowd Book deleted successfully.";
                }
                else
                {
                  TempData["ErrorMessage"] = "Failed to delete borrowd book.";
                }
                

                return RedirectToAction(nameof(BorrowdBook));

            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }
    }
    
}
