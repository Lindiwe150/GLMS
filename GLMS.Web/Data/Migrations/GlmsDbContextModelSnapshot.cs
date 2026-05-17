using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using GLMS.Web.Data;

#nullable disable

namespace GLMS.Web.Data.Migrations;

[DbContext(typeof(GlmsDbContext))]
partial class GlmsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        modelBuilder.Entity("GLMS.Web.Models.Client", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<string>("ContactDetails").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("Name").IsRequired().HasMaxLength(150).HasColumnType("nvarchar(150)");
            b.Property<string>("Region").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.HasKey("Id");
            b.ToTable("Clients");
        });

        modelBuilder.Entity("GLMS.Web.Models.Contract", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<int>("ClientId").HasColumnType("int");
            b.Property<DateTime>("EndDate").HasColumnType("datetime2");
            b.Property<string>("ServiceLevel").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("SignedAgreementPath").HasColumnType("nvarchar(max)");
            b.Property<DateTime>("StartDate").HasColumnType("datetime2");
            b.Property<string>("Status").IsRequired().HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("ClientId");
            b.ToTable("Contracts");
        });

        modelBuilder.Entity("GLMS.Web.Models.ServiceRequest", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<int>("ContractId").HasColumnType("int");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
            b.Property<decimal>("CostUsd").HasColumnType("decimal(18,2)");
            b.Property<decimal>("CostZar").HasColumnType("decimal(18,2)");
            b.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("Status").IsRequired().HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("ContractId");
            b.ToTable("ServiceRequests");
        });
#pragma warning restore 612, 618
    }
}
