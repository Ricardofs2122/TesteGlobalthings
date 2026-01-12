
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace APIMonitoramento.Models;

[Table("Medicoes")]
public class Medicoes
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset? DataHoraMedicao { get; set; }

    [Required]
    public decimal? Medicao { get; set; }
}
