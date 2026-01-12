namespace APIMonitoramento.DTO
{
    public class SensorMedicoesResponseDto
    {
        public int SensorId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public List<MedicaoResponseDto> Medicoes { get; set; } = new();
    }
}
