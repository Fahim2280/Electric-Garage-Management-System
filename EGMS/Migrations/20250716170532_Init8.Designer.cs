﻿// <auto-generated />
using System;
using EGMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EGMS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250716170532_Init8")]
    partial class Init8
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.17")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EGMS.DTOs.ElectricBillDTO", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<decimal>("Clear_money")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Current_Unit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Customer_ID")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Electric_bill")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Present_dues")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Previous_duos")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Previous_unit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Rent_Bill")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Total_Unit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Total_bill")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ID");

                    b.ToTable("ElectricBillDTO");
                });

            modelBuilder.Entity("EGMS.Models.Customer", b =>
                {
                    b.Property<int>("C_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("C_ID"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("Advance_money")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Created_Date")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("F_name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("M_name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Mobile_number")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<string>("NID_Number")
                        .IsRequired()
                        .HasMaxLength(17)
                        .HasColumnType("nvarchar(17)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Previous_Unit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("C_ID");

                    b.HasIndex("UserId");

                    b.HasIndex("Mobile_number", "UserId")
                        .IsUnique();

                    b.HasIndex("NID_Number", "UserId")
                        .IsUnique();

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("EGMS.Models.ElectricBill", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<decimal>("Clear_money")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Current_Unit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Customer_ID")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Electric_bill")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Present_dues")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Previous_duos")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Previous_unit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Rent_Bill")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Total_Unit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Total_bill")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ID");

                    b.HasIndex("Customer_ID");

                    b.ToTable("ElectricBills");
                });

            modelBuilder.Entity("EGMS.Models.PasswordResetToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("bit");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PasswordResetTokens");
                });

            modelBuilder.Entity("EGMS.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("EGMS.Models.Customer", b =>
                {
                    b.HasOne("EGMS.Models.User", "User")
                        .WithMany("Customers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("EGMS.Models.ElectricBill", b =>
                {
                    b.HasOne("EGMS.Models.Customer", "Customer")
                        .WithMany("ElectricBills")
                        .HasForeignKey("Customer_ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("EGMS.Models.Customer", b =>
                {
                    b.Navigation("ElectricBills");
                });

            modelBuilder.Entity("EGMS.Models.User", b =>
                {
                    b.Navigation("Customers");
                });
#pragma warning restore 612, 618
        }
    }
}
