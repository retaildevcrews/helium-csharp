using System.ComponentModel.DataAnnotations;

namespace Helium.Model
{
    public abstract class QueryParameters
    {
        [Range(minimum:1, maximum:10000, ErrorMessage = "Invalid PageNumber parameter")]
        public int? PageNumber { get; set; }

        [Range(minimum:1, maximum:1000, ErrorMessage = "Invalid PageSize parameter")]
        public int? PageSize { get; set; }
    }

    public sealed class MovieQueryParameters : QueryParameters
    {

    }

    public sealed class ActorQueryParameters : QueryParameters
    {
        [StringLength(maximumLength: 20, MinimumLength = 2, ErrorMessage = "Invalid q (search) parameter")]
        public string Q { get; set; }
    }
}
