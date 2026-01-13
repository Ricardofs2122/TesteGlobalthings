## TesteGlobalthings
Parte 01: 

a) Resposta: Criei uma API com um endpoint para recebimento de medições em lote (batch),  "POST /medicoes" e uma chave unica entre ID e DataHoraMedicao para evitar duplicidade no banco.  

Justificativa:
Envio em Lote: Como o firmware pode acumular dados quando o WiFi está intermitente, a API precisa aceitar várias medições no mesmo request

Chave Única: Caso os sensores dem algum problema ou reenviem dados duplicados é possivel garantir que os dados não foram duplicados.

b)Resposta: A API deve receber uma lista de Medições , pois o sensor pode enviar dados acumulados ou não. Para isso foram criados dois DTOs de Request. 

Um com todos os dados que irão persistir no banco e cum com o List que ira receber os dados do FromBody

```csharp
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

```csharp
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

```csharp
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

```csharp
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

```csharp
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

```csharp
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

```csharp
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

```csharp
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

```csharp
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

```csharp
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

Terceiro: Servico de Email (IEmailService e EmailService)

```csharp
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
Quarto: Crio um BrackgroudService que faz o envio de e-mail e Alertas

```csharp

using APIMonitoramento.Context;
using APIMonitoramento.Dominio.Services;
using APIMonitoramento.Infrastruture.Email;
using Microsoft.EntityFrameworkCore;

namespace APIMonitoramento.BackgroudServices
{
    public class AlertaBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailService _emailService;

        public AlertaBackgroundService(IServiceScopeFactory scopeFactory, IEmailService emailService)
        {
            _scopeFactory = scopeFactory;
            _emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var sensores = await context.Sensor
                    .Include(s => s.Medicoes)
                    .ToListAsync(stoppingToken);

                foreach (var sensor in sensores)
                {
                    var medicoesOrdenadas = sensor.Medicoes
                        .OrderBy(m => m.DataHoraMedicao)
                        .Select(m => m.Medicao)
                        .ToList();

                    if (RegraAlertaSensor.CincoForaLimite((IReadOnlyList<decimal>)medicoesOrdenadas))
                    {
                        await _emailService.EnviarAsync(
                            "ALERTA",
                            $"Sensor {sensor.Codigo} com 5 medições consecutivas fora do limite."
                        );
                    }
                    else if (RegraAlertaSensor.MediaUltima50Mergens((IReadOnlyList<decimal>)medicoesOrdenadas))
                    {
                        await _emailService.EnviarAsync(
                            "ALERTA",
                            $"Sensor {sensor.Codigo} próximo ao limite."
                        );
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}


```

Quinto e Ùltimo: Registrar o Servico no Program.cs

```csharp
using APIMonitoramento.Context;
using Microsoft.EntityFrameworkCore;
using APIMonitoramento.BackgroudServices;
using APIMonitoramento.Infrastruture.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//string de connection
var mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection,
    ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<AlertaBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

```

c) Resposta:

Testes criados:
1) Deve retornar true quando houver 5 medições consecutivas abaixo do minimo, no código = "CincoMedicoesAbaixoMinimo()"
2) Deve retornar true quando houver 5 Medicoes consecutivas acima do maximo, no código = "CincoMedicoesAcimaMaximo()"
3) Deve retornar false quando houver menos de 5 Consecutivas, no código = "CincoMedicoesMenosdeCinco()"
4) Deve resetar contador quando valor está dentro do limite, no código = "CincoMedicoesZeraContadorDentoLimite()"
5) Deve retornar false quando todas as medicoes estao no Limite, no código = "CincoMedicoesTodasMedicoesNoLimite"
6) Deve retornar false quando houver menos de 50 medicoes, no código = "MediaUltima50MergensFalsoMenos50Medicoes"
7) Deve retornar true quando media está perto do minimo, no código = "MediaUltima50MergensVerdadeiroMediaPertoMinimo()"
8) Deve retornar true quando media está perto do maximo, no código = "MediaUltima50MergensVerdadeiroMediaPertoMaximo()"
9) Deve retornar false quando media está longe dos limites, no código = "MediaUltima50MergensFalsoMediaLongeLimites()"
10) Deve retornar true quando media está exatamente no limite inferior com Margem, no código = "MediaUltima50MergensVerdadeiroMediaExatamenteLimiteInferior()"
11) Deve retornar true quando media está exatamente no limite superior com margem, no código = "RegraAlertaSensorVerdadeiroMediaExatamenteLimiteSuperior()"

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIMonitoramento.Dominio;
using APIMonitoramento.Dominio.Services;
using Xunit;

namespace APIMonitoramentoTests.Dominio.Services
{
    public class TesteAlertaRegraSensores
    {
        [Fact]
        public void CincoMedicoesAbaixoMinimo()
        {
            var medicoes = new decimal[]
            {
                10, 0.9m, 0.8m, 0.7m, 0.6m, 0.5m
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void CincoMedicoesAcimaMaximo()
        {
            var medicoes = new decimal[]
            {
                45, 51, 52, 53, 54, 55
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void CincoMedicoesMenosdeCinco()
        {
            var medicoes = new decimal[]
            {
                0.9m, 0.8m, 0.7m, 1.5m, 0.6m
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.False(resultado);
        }

        [Fact]
        public void CincoMedicoesZeraContadorDentoLimite()
        {
            var medicoes = new decimal[]
            {
                0.9m, 0.8m, 0.7m, 1.2m, 0.6m, 0.5m, 0.4m
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);


            Assert.False(resultado);
        }

        [Fact]
        public void CincoMedicoesTodasMedicoesNoLimite()
        {
            var medicoes = new decimal[]
            {
                10, 20, 30, 40, 50
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.False(resultado);
        }

        [Fact]
        public void MediaUltima50MergensFalsoMenos50Medicoes()
        {
            var medicoes = Enumerable.Repeat(10m, 49).ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);
                

            Assert.False(resultado);
        }

        [Fact]
        public void MediaUltima50MergensVerdadeiroMediaPertoMinimo()
        {
            var medicoes = Enumerable
                .Repeat(1.5m, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void MediaUltima50MergensVerdadeiroMediaPertoMaximo()
        {
            var medicoes = Enumerable
                .Repeat(49.5m, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void MediaUltima50MergensFalsoMediaLongeLimites()
        {
            var medicoes = Enumerable
                .Repeat(25m, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.False(resultado);
        }

        [Fact]
        public void MediaUltima50MergensVerdadeiroMediaExatamenteLimiteInferior()
        {
            var medicoes = Enumerable
                .Repeat(LimitesSensor.Minimo - LimitesSensor.MargemErro, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void RegraAlertaSensorVerdadeiroMediaExatamenteLimiteSuperior()
        {
            var medicoes = Enumerable
                .Repeat(LimitesSensor.Maximo + LimitesSensor.MargemErro, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

    }
}

```




 
