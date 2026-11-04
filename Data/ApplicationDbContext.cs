using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NoteConnect.Models.Entities;

namespace NoteConnect.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Notebook> Notebooks => Set<Notebook>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Note>()
            .HasOne(note => note.Notebook)
            .WithMany(notebook => notebook.Notes)
            .HasForeignKey(note => note.NotebookId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
