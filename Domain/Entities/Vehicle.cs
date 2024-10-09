using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Domain.Entities;

public class Vehicle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Brand { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    public int Year { get; set; }

    [Required]
    [Range(0.01, 2500000.00,
            ErrorMessage = "Price must be between 0.01 and 2500000.00")]
        public decimal Price       { get; set; }

    [Required]
    [StringLength(255)]
    public string Color { get; set; }
}
