using APIMonitoramento.Models;
using Microsoft.EntityFrameworkCore;


namespace APIMonitoramento.Context
{
    public class AppDbContext: DbContext
    {
        //Construtor
        public AppDbContext(DbContextOptions<AppDbContext> option) : base(option) { }

        //Mapeamento de tabelas que serão criadas na base de dados
        public DbSet<Medicoes>? Medicoes { get; set; } = null!;
        
        public DbSet<Sensor>? Sensor { get; set; } = null!;

        public DbSet<SetorEquipamento>? SetorEquipamento { get; set; } = null!;

        //Criando Indice Unico para não permitir duplicidade.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medicoes>()
                .HasIndex(m => new { m.Id, m.DataHoraMedicao })
                .IsUnique(); 
        }
   
    }
}
