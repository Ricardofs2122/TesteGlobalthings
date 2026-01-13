using System;
using System.Threading.Tasks;

namespace APIMonitoramento.Infrastruture.Email
{
    public class EmailService : IEmailService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public Task EnviarAsync(string assunto, string mensagem)
        {
            
            
            Console.WriteLine("Email");
            Console.WriteLine($"Assunto: {assunto}");
            Console.WriteLine($"Mensagem: {mensagem}");
            
            return Task.CompletedTask;
        }
    }
}
