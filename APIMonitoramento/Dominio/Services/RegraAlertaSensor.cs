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
