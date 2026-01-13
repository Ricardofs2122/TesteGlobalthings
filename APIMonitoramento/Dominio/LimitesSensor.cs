namespace APIMonitoramento.Dominio
{
    public static class LimitesSensor
    {
        public const decimal Minimo = 1m;
        public const decimal Maximo = 50m;
        public const decimal MargemErro= 2m;

        public const int LimiteConsecutivo = 5;
        public const int TotalMedia = 50;

    }
}
