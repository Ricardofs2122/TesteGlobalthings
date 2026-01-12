namespace APIMonitoramento.DTO
{
    public class MedicaoResultadoDTO
    {
        public int Id { get; set; }
        public DateTimeOffset DataHoraMedicao { get; set; }
        public bool Sucesso { get; set; }
        public string? Mensagem { get; set; }
    }
}
