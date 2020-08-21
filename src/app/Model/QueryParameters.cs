using System.ComponentModel.DataAnnotations;

namespace Helium.Model
{
    public class QueryParameters
    {
        [StringLength(maximumLength:20, ErrorMessage = "Invalid q (search) parameter", MinimumLength = 2)]
        public string QueryString { get; set; }

        [Range(minimum:1, maximum:10000, ErrorMessage = "Invalid PageNumber parameter")]
        public int PageNumber { get; set; }

        [Range(minimum:1, maximum:1000, ErrorMessage = "Invalid PageSize parameter")]
        public int PageSize { get; set; }
    }
}
