using System.ComponentModel.DataAnnotations;

namespace Blackdot.Models
{
    public class SearchViewModel
    {
        [Required(ErrorMessage = "Please enter a search term")]
        public string SearchTerm { get; set; }

        [Required(ErrorMessage = "Please select the number of results to return")]
        public int NumOfResults { get; set; }
    }
}
