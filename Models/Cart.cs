using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaSystem.Models
{
    [PrimaryKey(nameof(ApplicationUserId), nameof(MovieId))]
    public class Cart
    {
        public string ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }
        public int MovieId { get; set; }
        [ForeignKey(nameof(MovieId))]
        public Movie Movie { get; set; }
        public int Quantity { get; set; }
    }
}
