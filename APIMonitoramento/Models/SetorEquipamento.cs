
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace APIMonitoramento.Models;

[Table("SetorEquipamento")]
public class SetorEquipamento
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Nome { get; set; } = string.Empty;

    [JsonIgnore]
    //Propiedade de navegação de 1 para muitos.
    public ICollection<Sensor> Sensores { get; set; } = new List<Sensor>();
}
