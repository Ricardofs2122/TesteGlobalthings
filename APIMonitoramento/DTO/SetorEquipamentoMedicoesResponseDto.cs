namespace APIMonitoramento.DTO
{
    public class SetorEquipamentoMedicoesResponseDto
    {
        public int SetorEquipamentoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<SensorMedicoesResponseDto> Sensores { get; set; } = new();
    }
}
