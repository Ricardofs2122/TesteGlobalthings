
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace APIMonitoramento.Models;

[Table("Sensor")]
public class Sensor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public int SetorEquipamentoId { get; set; }

    public SetorEquipamento SetorEquipamento { get; set; } = null!;
    
    [JsonIgnore]
    //Propiedade de navegação de 1 para muitos.
    public ICollection<Medicoes> Medicoes { get; set; }= new List<Medicoes>();

}
