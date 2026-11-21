
using Microsoft.EntityFrameworkCore;
using ClinicaMedicaAPI.Models;

namespace ClinicaMedicaAPI.Data;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(
        DbContextOptions<ClinicaDbContext> options
    ) : base(options)
    {
    }

    // Tabelas do banco de dados
    public DbSet<Paciente> Pacientes => Set<Paciente>();

    public DbSet<Especialidade> Especialidades
        => Set<Especialidade>();

    public DbSet<Medico> Medicos => Set<Medico>();

    public DbSet<Convenio> Convenios => Set<Convenio>();

    public DbSet<Consulta> Consultas => Set<Consulta>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PACIENTE
        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nome)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(p => p.Cpf)
                .HasMaxLength(11)
                .IsRequired();

            entity.HasIndex(p => p.Cpf)
                .IsUnique();

            entity.Property(p => p.Telefone)
                .HasMaxLength(20);

            // Paciente pode ter um convênio ou nenhum
            entity.HasOne(p => p.Convenio)
                .WithMany()
                .HasForeignKey(p => p.ConvenioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ESPECIALIDADE
        modelBuilder.Entity<Especialidade>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Nome)
                .IsUnique();

            entity.Property(e => e.ValorConsulta)
                .HasPrecision(10, 2);
        });

        // MEDICO
        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Nome)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(m => m.Crm)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(m => m.UfCrm)
                .HasMaxLength(2)
                .IsRequired();

            // CRM único dentro de cada estado
            entity.HasIndex(m => new { m.Crm, m.UfCrm })
                .IsUnique();

            // Relacionamento muitos-para-muitos
            entity.HasMany(m => m.Especialidades)
                .WithMany()
                .UsingEntity(j =>
                    j.ToTable("MedicoEspecialidade"));
        });

        // CONVENIO
        modelBuilder.Entity<Convenio>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Nome)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(c => c.RegistroAns)
                .HasMaxLength(20)
                .IsRequired();

            // Impede registros ANS duplicados
            entity.HasIndex(c => c.RegistroAns)
                .IsUnique();

            // Percentual com duas casas decimais
            entity.Property(c => c.PercentualCobertura)
                .HasPrecision(5, 2);

            entity.Property(c => c.FimVigencia)
                .IsRequired();
        });

        // CONSULTA
        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.HasKey(c => c.Id);

            // Valor da consulta com duas casas decimais
            entity.Property(c => c.Valor)
                .HasPrecision(10, 2);

            // Armazena o status como número inteiro
            entity.Property(c => c.Status)
                .HasConversion<int>();

            entity.Property(c => c.Observacoes)
                .HasMaxLength(2000);

            entity.Property(c => c.MotivoCancelamento)
                .HasMaxLength(500);

            // Relacionamento Paciente - Consulta
            entity.HasOne(c => c.Paciente)
                .WithMany()
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento Médico - Consulta
            entity.HasOne(c => c.Medico)
                .WithMany()
                .HasForeignKey(c => c.MedicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento Especialidade - Consulta
            entity.HasOne(c => c.Especialidade)
                .WithMany()
                .HasForeignKey(c => c.EspecialidadeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice para consultas por médico e horário
            entity.HasIndex(c => new
            {
                c.MedicoId,
                c.DataHora
            });
        });
    }
}
