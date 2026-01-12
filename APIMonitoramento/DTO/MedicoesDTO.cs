namespace APIMonitoramento.DTO
{
    public class MedicoesDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public DateTimeOffset DataHoraMedicao { get; set; }
        public decimal Medicao { get; set; }
    }
}
