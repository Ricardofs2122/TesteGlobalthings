using APIMonitoramento.Context;
using APIMonitoramento.DTO;
using APIMonitoramento.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace APIMonitoramento.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
    public class MedicoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicoesController(AppDbContext context)
        {

            _context = context;
        }

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


    }
}

