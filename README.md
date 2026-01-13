## TesteGlobalthings
Parte 01: 

a) Resposta: Criei uma API com um endpoint para recebimento de medições em lote (batch),  "POST /medicoes" e uma chave unica entre ID e DataHoraMedicao para evitar duplicidade no banco.  

Justificativa:
Envio em Lote: Como o firmware pode acumular dados quando o WiFi está intermitente, a API precisa aceitar várias medições no mesmo request

Chave Única: Caso os sensores dem algum problema ou reenviem dados duplicados é possivel garantir que os dados não foram duplicados.

b)Resposta: A API deve receber uma lista de Medições , pois o sensor pode enviar dados acumulados ou não. Para isso foram criados dois DTOs de Request. 

Um com todos os dados que irão persistir no banco e cum com o List que ira receber os dados do FromBody
```
public class MedicoesDTO
{
  public int Id { get; set; }
  public string Codigo { get; set; } = string.Empty;
  public DateTimeOffset DataHoraMedicao { get; set; }
  public decimal Medicao { get; set; }
}

public class MedicoesBatchDTO
{
  public List<MedicoesDTO> Medicoes { get; set; } = new();
}

```

E temos o request e Persistência em banco de dados tambem trazendo quandos dados não foram gravados e quanto deram falha.

```
[HttpPost]
public async Task<IActionResult> Medicoes([FromBody] MedicoesBatchDTO request)

{

    var response = new MedicaoBachResultado
    {
        TotalRecebidas = request.Medicoes?.Count ?? 0
    };

    if (request.Medicoes == null || !request.Medicoes.Any())
        return BadRequest(response);

    using var transaction = await _context.Database.BeginTransactionAsync();

    foreach (var dto in request.Medicoes)
    {
        var resultado = new MedicaoResultadoDTO
        {
            Id = dto.Id,
            DataHoraMedicao = dto.DataHoraMedicao
        };
        
        
        if (string.IsNullOrWhiteSpace(dto.Codigo))
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Código inválido.";
            response.Resultados.Add(resultado);
            continue;
        }

        if (dto.DataHoraMedicao == default)
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "DataHoraMedicao inválida.";
            response.Resultados.Add(resultado);
            continue;
        }

        var medicao = new Medicoes
        {
            Id = dto.Id,
            Codigo = dto.Codigo,
            DataHoraMedicao = dto.DataHoraMedicao,
            Medicao = dto.Medicao
        };

        try
        {
            await _context.Medicoes.AddAsync(medicao);
            await _context.SaveChangesAsync();

            resultado.Sucesso = true;
            resultado.Mensagem = "Medição salva com sucesso.";
            response.TotalProcessadas++;
        }
        catch (DbUpdateException)
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Medição duplicada, ignorada.";
        }

        response.Resultados.Add(resultado);
    }

    await transaction.CommitAsync();

    return Ok(response);

}
```

c) Resposta: Na minha opinião e também pela minha experiência em baco de dados usaria um banco de dados relacional como SQLServer, Postgre ou Mysql "Que é o que está no exemplo que criei", por ser mais estruturada e tipos bem definidos, consistência e integridade dos dados, sem contar o suporte ao DateTimeOffset.

Parte 02: 

a) Resposta: Criei um endpoint na APIMonitoramento para Vincular um Setor/Equipamento à um sensor,  "POST /VincularSensor"  onde  crio mais duas novas tabelas uma de Sensor e outra de SetorEquipamento que fazem esse controle. 

Models de tabelas

Sensor
```
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
```

SetorEquipamento

```
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

```
DTO de Request

```
namespace APIMonitoramento.DTO
{
    public class VincularSensorDTO
    {
        public int SensorId { get; set; }
        public int SetorEquipamentoId { get; set; }

    }
}
```

Endpoit para VincularSetor
```
    [HttpPost("/VincularSensor")]
    public async Task<IActionResult> VincularSensor([FromBody] VincularSensorDTO request)
    {
        var sensor = await _context.Sensor.FirstOrDefaultAsync(s => s.Id == request.SensorId);
        
        if (sensor == null) 
            return NotFound("Sensor não encontrado");

        var setor = await _context.SetorEquipamento.FirstOrDefaultAsync(s => s.Id.Equals(request.SetorEquipamentoId));

        if (setor == null)
            return NotFound("Setor/Equipamento não encontrado");

        sensor.SetorEquipamentoId = setor.Id;

        await _context.SaveChangesAsync();
        
        return Ok("Sensor Vinculado!");
    }
}
```
b) Resposta: Criei um endpoint na APIMonitoramento trazendo os ultimas medições de um dterminado Setor ,  "GET /{setorEquipamentoId}/ObterUltimasMedicoesSetor" , onde criei os DTOs de response  e o Endpoint como descritos abaixo
 
DTOs

```
namespace APIMonitoramento.DTO
{
    public class MedicaoResponseDto
    {
        public DateTimeOffset DataHoraMedicao { get; set; }
        public decimal Valor { get; set; }
    }
}

namespace APIMonitoramento.DTO
{
    public class SensorMedicoesResponseDto
    {
        public int SensorId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public List<MedicaoResponseDto> Medicoes { get; set; } = new();
    }
}

namespace APIMonitoramento.DTO
{
    public class SetorEquipamentoMedicoesResponseDto
    {
        public int SetorEquipamentoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<SensorMedicoesResponseDto> Sensores { get; set; } = new();
    }
}
```

EndPoint

```
[HttpGet("/{setorEquipamentoId}/ObterUltimasMedicoesSetor")]
public async Task<IActionResult> ObterUltimasMedicoesSetor(int setorEquipamentoId)
{
    var setor = await _context.Set<SetorEquipamento>()
        .Include(s => s.Sensores)
            .ThenInclude(sensor => sensor.Medicoes)
        .FirstOrDefaultAsync(s => s.Id == setorEquipamentoId);

    if (setor == null)
        return NotFound("Setor/Equipamento não encontrado.");

    var response = new SetorEquipamentoMedicoesResponseDto
    {
        SetorEquipamentoId = setor.Id,
        Nome = setor.Nome,
        Sensores = setor.Sensores.Select(sensor => new SensorMedicoesResponseDto
        {
            SensorId = sensor.Id,
            Codigo = sensor.Codigo,
            Medicoes = sensor.Medicoes
                .OrderByDescending(m => m.DataHoraMedicao)
                .Take(10)
                .Select(m => new MedicaoResponseDto
                {
                    DataHoraMedicao = (DateTimeOffset)m.DataHoraMedicao,
                    Valor = (Decimal)m.Medicao
                })
                .ToList()
        }).ToList()
    };

    return Ok(response);
}
```
Parte 03: 

a) Resposta: A solucão Ideal seia uma Arquitetura event-driven, com processamento assicrono. Memoria distribuida e envio de email desaclopado. Asp.Net
 Core, bancop de dados Relacional, Mensageria "RabbitMQ", Redis,.NetWorker Service e SMPT/SendGrid.

b) Resposta: 
Primeiro: Criei as constantes de Dominio
```
namespace APIMonitoramento.Dominio
{
    public static class LimitesSensor
    {
        public const decimal Minimo = 1m;
        public const decimal Maximo = 50m;
        public const decimal MargemErro= 2m;

        public const int LimiteConsecutivo = 5;
        public const int TotalMedia = 50;

    }
}
```

Segundo: Regra de Alerta do Sensor(CincoForaLimite e MediaUltima50Mergens)

```
namespace APIMonitoramento.Dominio.Services
{
    public static class RegraAlertaSensor
    {
        public static bool CincoForaLimite(IReadOnlyList<decimal> medicoesOrdenadas)
        {
            int cont = 0;

            foreach (var valor in medicoesOrdenadas)
            {
                if (valor < LimitesSensor.Minimo || valor > LimitesSensor.Maximo)
                {
                    cont++;
                    if (cont >= LimitesSensor.LimiteConsecutivo)
                        return true;
                }
                else 
                {
                    cont = 0;
                }
            }
            return false;
        }

        public static bool MediaUltima50Mergens (IReadOnlyList<decimal> medicoes)
        {
            if (medicoes.Count < LimitesSensor.TotalMedia)
                return false;

            var ultimas50 = medicoes.TakeLast(LimitesSensor.TotalMedia).ToList();

            var media = ultimas50.Average();

            bool pertoMim = media >= LimitesSensor.Minimo - LimitesSensor.MargemErro && media <= LimitesSensor.Minimo + LimitesSensor.MargemErro;
            bool pertoMax = media >= LimitesSensor.Maximo - LimitesSensor.MargemErro && media <= LimitesSensor.Maximo + LimitesSensor.MargemErro;

            return pertoMim || pertoMax;
        }

    }
}
```

Terceiro Servico de Email (IEmailService e EmailService)

```
using System.Threading.Tasks;

namespace APIMonitoramento.Infrastruture.Email
{
    public interface IEmailService
    {
        Task EnviarAsync(string assunto, string mensagem);
    }
}
```
```
using System;
using System.Threading.Tasks;

namespace APIMonitoramento.Infrastruture.Email
{
    public class EmailService : IEmailService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public Task EnviarAsync(string assunto, string mensagem)
        {
            Console.WriteLine("Email");
            Console.WriteLine($"Assunto: {assunto}");
            Console.WriteLine($"Mensagem: {mensagem}");
            
            return Task.CompletedTask;
        }
    }
}

```


 
