
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LMS.Model
{
    public class BookModel
    {
        [Key]
        public int BookID { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int AuthorID { get; set; }
        public string AuthorName { get; set; }

        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
        public List<SelectListItem> AuthorList { get; set; }
    }
}
