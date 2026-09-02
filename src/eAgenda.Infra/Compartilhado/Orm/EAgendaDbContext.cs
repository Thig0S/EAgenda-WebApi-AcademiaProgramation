using eAgenda.Dominio.Compartilhado.Identity;
using eAgenda.Dominio.Modulos.ModuloCategoria;
using eAgenda.Dominio.Modulos.ModuloCompromisso;
using eAgenda.Dominio.Modulos.ModuloContato;
using eAgenda.Dominio.Modulos.ModuloDespesa;
using eAgenda.Dominio.Modulos.ModuloTarefa;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eAgenda.Infra.Compartilhado.Orm;

public sealed class EAgendaDbContext(DbContextOptions<EAgendaDbContext> options,
IProvedorDeUsuario? provedorDeUsuario = null)
    : IdentityDbContext<IdentityUser<Guid>,
    IdentityRole<Guid>,
    Guid>(options)
{
    public DbSet<Contato> Contatos => Set<Contato>();
    public DbSet<Compromisso> Compromissos => Set<Compromisso>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Despesa> Despesas => Set<Despesa>();
    public DbSet<ItemTarefa> ItensTarefa => Set<ItemTarefa>();
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EAgendaDbContext).Assembly);

        if (provedorDeUsuario is not null)
        {
            modelBuilder.Entity<Contato>().HasQueryFilter(c => c.UsuarioId == provedorDeUsuario.Id);
            modelBuilder.Entity<Compromisso>().HasQueryFilter(c => c.UsuarioId == provedorDeUsuario.Id);
            modelBuilder.Entity<Categoria>().HasQueryFilter(c => c.UsuarioId == provedorDeUsuario.Id);
            modelBuilder.Entity<Tarefa>().HasQueryFilter(t => t.UsuarioId == provedorDeUsuario.Id);
            modelBuilder.Entity<Despesa>().HasQueryFilter(d => d.UsuarioId == provedorDeUsuario.Id);
        }
    }
    public override int SaveChanges()
    {
        Guid? userId = provedorDeUsuario?.Id;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Não é possível salvar entidades do usuário sem estar autenticado."
            );
        }

        foreach (var entry in ChangeTracker.Entries<IEntidadeDoUsuario>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.UsuarioId == Guid.Empty)
                    {
                        entry.Property(nameof(IEntidadeDoUsuario.UsuarioId)).CurrentValue = userId.Value;
                    }
                    else if (entry.Entity.UsuarioId != userId.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "Tentativa de criar entidade para outro usuário."
                        );
                    }

                    break;

                case EntityState.Modified:
                    Guid idOriginalInstituicao = entry
                        .Property(nameof(IEntidadeDoUsuario.UsuarioId))
                        .OriginalValue is Guid idOriginal
                        ? idOriginal
                        : Guid.Empty;

                    Guid idAtualInstituicao = entry
                        .Property(nameof(IEntidadeDoUsuario.UsuarioId))
                        .OriginalValue is Guid idAtual
                        ? idAtual
                        : Guid.Empty;

                    if (idOriginalInstituicao != idAtualInstituicao)
                    {
                        throw new UnauthorizedAccessException(
                              "Não é permitido alterar o usuário de uma entidade."
                          );
                    }

                    if (idAtualInstituicao != userId.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "Tentativa de modificar entidade de outro usuário."
                        );
                    }

                    break;

                case EntityState.Deleted:
                    Guid instituicaoOriginal = entry
                        .Property(nameof(IEntidadeDoUsuario.UsuarioId))
                        .OriginalValue is Guid original
                        ? original
                        : Guid.Empty;

                    if (instituicaoOriginal != userId.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "Tentativa de excluir entidade de outro usuário."
                        );
                    }

                    break;

            }
        }

        return base.SaveChanges();
    }
}
