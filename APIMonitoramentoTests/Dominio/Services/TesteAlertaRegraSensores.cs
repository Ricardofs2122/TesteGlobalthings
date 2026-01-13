using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIMonitoramento.Dominio;
using APIMonitoramento.Dominio.Services;
using Xunit;

namespace APIMonitoramentoTests.Dominio.Services
{
    public class TesteAlertaRegraSensores
    {
        [Fact]
        public void CincoMedicoesAbaixoMinimo()
        {
            var medicoes = new decimal[]
            {
                10, 0.9m, 0.8m, 0.7m, 0.6m, 0.5m
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void CincoMedicoesAcimaMaximo()
        {
            var medicoes = new decimal[]
            {
                45, 51, 52, 53, 54, 55
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void CincoMedicoesMenosdeCinco()
        {
            var medicoes = new decimal[]
            {
                0.9m, 0.8m, 0.7m, 1.5m, 0.6m
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.False(resultado);
        }

        [Fact]
        public void CincoMedicoesZeraContadorDentoLimite()
        {
            var medicoes = new decimal[]
            {
                0.9m, 0.8m, 0.7m, 1.2m, 0.6m, 0.5m, 0.4m
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);


            Assert.False(resultado);
        }

        [Fact]
        public void CincoMedicoesTodasMedicoesNoLimite()
        {
            var medicoes = new decimal[]
            {
                10, 20, 30, 40, 50
            };

            var resultado = RegraAlertaSensor.CincoForaLimite(medicoes);

            Assert.False(resultado);
        }

        [Fact]
        public void MediaUltima50MergensFalsoMenos50Medicoes()
        {
            var medicoes = Enumerable.Repeat(10m, 49).ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);
                

            Assert.False(resultado);
        }

        [Fact]
        public void MediaUltima50MergensVerdadeiroMediaPertoMinimo()
        {
            var medicoes = Enumerable
                .Repeat(1.5m, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void MediaUltima50MergensVerdadeiroMediaPertoMaximo()
        {
            var medicoes = Enumerable
                .Repeat(49.5m, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void MediaUltima50MergensFalsoMediaLongeLimites()
        {
            var medicoes = Enumerable
                .Repeat(25m, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.False(resultado);
        }

        [Fact]
        public void MediaUltima50MergensVerdadeiroMediaExatamenteLimiteInferior()
        {
            var medicoes = Enumerable
                .Repeat(LimitesSensor.Minimo - LimitesSensor.MargemErro, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

        [Fact]
        public void RegraAlertaSensorVerdadeiroMediaExatamenteLimiteSuperior()
        {
            var medicoes = Enumerable
                .Repeat(LimitesSensor.Maximo + LimitesSensor.MargemErro, 50)
                .ToList();

            var resultado = RegraAlertaSensor.MediaUltima50Mergens(medicoes);

            Assert.True(resultado);
        }

    }
}
