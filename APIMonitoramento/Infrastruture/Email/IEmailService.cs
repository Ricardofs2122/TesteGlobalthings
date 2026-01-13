using System.Threading.Tasks;

namespace APIMonitoramento.Infrastruture.Email
{
    public interface IEmailService
    {
        Task EnviarAsync(string assunto, string mensagem);
    }
}
