namespace APIMonitoramento.DTO
{
    public class MedicaoBachResultado
    {
        public int TotalRecebidas { get; set; }
        public int TotalProcessadas { get; set; }
        public List<MedicaoResultadoDTO> Resultados { get; set; } = new();
    }
}
