namespace APIMonitoramento.DTO
{
    public class MedicaoResponseDto
    {
        public DateTimeOffset DataHoraMedicao { get; set; }
        public decimal Valor { get; set; }
    }
}
