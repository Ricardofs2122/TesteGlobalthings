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
