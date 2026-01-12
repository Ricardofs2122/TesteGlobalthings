namespace APIMonitoramento.DTO
{
    public class ListaMedicoesDTO
    {
     

        public class SensorMedicoesResponseDto
        {
            public int SensorId { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public List<MedicoesDTO> Medicoes { get; set; } = new();
        }

        public class SetorEquipamentoMedicoesResponseDto
        {
            public int SetorEquipamentoId { get; set; }
            public string Nome { get; set; } = string.Empty;
            public List<SensorMedicoesResponseDto> Sensores { get; set; } = new();
        }

    }
}
