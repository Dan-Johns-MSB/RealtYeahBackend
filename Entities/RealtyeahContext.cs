using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace RealtYeahBackend.Entities;

public partial class RealtyeahContext : DbContext
{
    public RealtyeahContext()
    {
    }

    public RealtyeahContext(DbContextOptions<RealtyeahContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientsStatus> ClientsStatuses { get; set; }

    public virtual DbSet<ClientsStatusesAssignment> ClientsStatusesAssignments { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeesStatus> EmployeesStatuses { get; set; }

    public virtual DbSet<EstateObject> EstateObjects { get; set; }

    public virtual DbSet<ObjectsStatus> ObjectsStatuses { get; set; }

    public virtual DbSet<Operation> Operations { get; set; }

    public virtual DbSet<OperationsStatus> OperationsStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("Server=localhost;Port=3306;Database=realtyeah;User=root;Password=Ovo4m14ASIDA_231Dllp");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PRIMARY");

            entity.ToTable("clients");

            entity.HasIndex(e => e.PassportNumber, "Passport_Number").IsUnique();

            entity.HasIndex(e => e.TaxpayerNumber, "Taxpayer_Number").IsUnique();

            entity.Property(e => e.ClientId).HasColumnName("Client_ID");
            entity.Property(e => e.Birthdate).HasColumnType("date");
            entity.Property(e => e.Birthplace).HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PassportNumber)
                .HasMaxLength(9)
                .HasColumnName("Passport_Number");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(13)
                .HasColumnName("Phone_Number");
            entity.Property(e => e.Photo).HasColumnType("text");
            entity.Property(e => e.TaxpayerNumber)
                .HasMaxLength(10)
                .HasColumnName("Taxpayer_Number");
        });

        modelBuilder.Entity<ClientsStatus>(entity =>
        {
            entity.HasKey(e => e.Status).HasName("PRIMARY");

            entity.ToTable("clients_statuses");

            entity.Property(e => e.Status).HasMaxLength(15);
        });

        modelBuilder.Entity<ClientsStatusesAssignment>(entity =>
        {
            entity.HasKey(e => new { e.Status, e.OperationId, e.ClientId }).HasName("PRIMARY");

            entity.ToTable("clients_statuses_assignments");

            entity.HasIndex(e => e.ClientId, "Clients_To_Statuses");

            entity.HasIndex(e => e.OperationId, "IX_Relationship1");

            entity.Property(e => e.Status).HasMaxLength(15);
            entity.Property(e => e.OperationId).HasColumnName("Operation_ID");
            entity.Property(e => e.ClientId).HasColumnName("Client_ID");
            entity.Property(e => e.Requirements).HasMaxLength(500);

            entity.HasOne(d => d.ClientNavigation).WithMany(p => p.ClientsStatusesAssignments)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("Clients_To_Statuses");

            entity.HasOne(d => d.Operation).WithMany(p => p.ClientsStatusesAssignments)
                .HasForeignKey(d => d.OperationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Operation_To");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.ClientsStatusesAssignments)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Statuses_To_Clients");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PRIMARY");

            entity.ToTable("documents");

            entity.HasIndex(e => e.OperationId, "IX_Relationship1");

            entity.Property(e => e.DocumentId).HasColumnName("Document_ID");
            entity.Property(e => e.File).HasColumnType("text");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.OperationId).HasColumnName("Operation_ID");

            entity.HasOne(d => d.Operation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.OperationId)
                .HasConstraintName("Operation_To_Documents");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PRIMARY");

            entity.ToTable("employees");

            entity.HasIndex(e => e.Status, "IX_Relationship3");

            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.Address).HasMaxLength(150);
            entity.Property(e => e.Birthdate).HasColumnType("date");
            entity.Property(e => e.Firedate).HasColumnType("date");
            entity.Property(e => e.Hiredate).HasColumnType("date");
            entity.Property(e => e.Job).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(13)
                .HasColumnName("Phone_Number");
            entity.Property(e => e.Photo).HasColumnType("text");
            entity.Property(e => e.Promotedate).HasColumnType("date");
            entity.Property(e => e.Status)
                .HasMaxLength(14)
                .HasDefaultValueSql("'Активний'");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Status_To_Employees");
        });

        modelBuilder.Entity<EmployeesStatus>(entity =>
        {
            entity.HasKey(e => e.Status).HasName("PRIMARY");

            entity.ToTable("employees_statuses");

            entity.Property(e => e.Status).HasMaxLength(14);
        });

        modelBuilder.Entity<EstateObject>(entity =>
        {
            entity.HasKey(e => e.EstateObjectId).HasName("PRIMARY");

            entity.ToTable("estate_objects");

            entity.HasIndex(e => e.Address, "Address").IsUnique();

            entity.HasIndex(e => e.Status, "IX_Relationship1");

            entity.Property(e => e.EstateObjectId).HasColumnName("Estate_Object_ID");
            entity.Property(e => e.Address).HasMaxLength(150);
            entity.Property(e => e.Photo).HasColumnType("text");
            entity.Property(e => e.Status).HasMaxLength(21);
            entity.Property(e => e.Type).HasMaxLength(100);

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.EstateObjects)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Status_To_Objects");
        });

        modelBuilder.Entity<ObjectsStatus>(entity =>
        {
            entity.HasKey(e => e.Status).HasName("PRIMARY");

            entity.ToTable("objects_statuses");

            entity.Property(e => e.Status).HasMaxLength(21);
        });

        modelBuilder.Entity<Operation>(entity =>
        {
            entity.HasKey(e => e.OperationId).HasName("PRIMARY");

            entity.ToTable("operations");

            entity.HasIndex(e => e.CounteragentLead, "Clients_To_Lead");

            entity.HasIndex(e => e.CounteragentSecondary, "Clients_To_Secondary");

            entity.HasIndex(e => e.Status, "IX_Relationship1");

            entity.HasIndex(e => e.EstateObjectId, "IX_Relationship11");

            entity.HasIndex(e => e.HostId, "IX_Relationship13");

            entity.HasIndex(e => e.FkOperationSecondary, "Operation_Lead_Chain_FK");

            entity.HasIndex(e => e.FkOperationLead, "Operation_Secondary_Chain_FK");

            entity.Property(e => e.OperationId).HasColumnName("Operation_ID");
            entity.Property(e => e.ActType)
                .HasMaxLength(55)
                .HasColumnName("Act_Type");
            entity.Property(e => e.CounteragentLead).HasColumnName("Counteragent_Lead");
            entity.Property(e => e.CounteragentSecondary).HasColumnName("Counteragent_Secondary");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EstateObjectId).HasColumnName("Estate_Object_ID");
            entity.Property(e => e.FkOperationLead).HasColumnName("FK_Operation_Lead");
            entity.Property(e => e.FkOperationSecondary).HasColumnName("FK_Operation_Secondary");
            entity.Property(e => e.HostId).HasColumnName("Host_ID");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(10);

            entity.HasOne(d => d.CounteragentLeadNavigation).WithMany(p => p.OperationCounteragentLeadNavigations)
                .HasForeignKey(d => d.CounteragentLead)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Clients_To_Lead");

            entity.HasOne(d => d.CounteragentSecondaryNavigation).WithMany(p => p.OperationCounteragentSecondaryNavigations)
                .HasForeignKey(d => d.CounteragentSecondary)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Clients_To_Secondary");

            entity.HasOne(d => d.EstateObjectNavigation).WithMany(p => p.Operations)
                .HasForeignKey(d => d.EstateObjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Object_To_Operations");

            entity.HasOne(d => d.FkOperationLeadNavigation).WithMany(p => p.InverseFkOperationLeadNavigation)
                .HasForeignKey(d => d.FkOperationLead)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Operation_Secondary_Chain_FK");

            entity.HasOne(d => d.FkOperationSecondaryNavigation).WithMany(p => p.InverseFkOperationSecondaryNavigation)
                .HasForeignKey(d => d.FkOperationSecondary)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Operation_Lead_Chain_FK");

            entity.HasOne(d => d.Host).WithMany(p => p.Operations)
                .HasForeignKey(d => d.HostId)
                .HasConstraintName("Employee_To_Operations");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Operations)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Status_To_Operations");
        });

        modelBuilder.Entity<OperationsStatus>(entity =>
        {
            entity.HasKey(e => e.Status).HasName("PRIMARY");

            entity.ToTable("operations_statuses");

            entity.Property(e => e.Status).HasMaxLength(10);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.EmployeeId, "IX_Relationship14");

            entity.HasIndex(e => e.Login, "Login").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.Login).HasMaxLength(320);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(15);

            entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("Employee_To_Account");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
