﻿// <auto-generated />
using System;
using MZPO.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MZPO.Migrations
{
    [DbContext(typeof(MySQLContext))]
    partial class MySQLContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("MZPO.Data.AmoAccount", b =>
                {
                    b.Property<int>("id")
                        .HasColumnType("int");

                    b.Property<string>("authToken")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("client_id")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("client_secret")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("code")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("redirect_uri")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("refrToken")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("subdomain")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<DateTime>("validity")
                        .HasColumnType("datetime(6)");

                    b.HasKey("id");

                    b.ToTable("AmoAccounts");
                });

            modelBuilder.Entity("MZPO.Data.CF", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("AmoId")
                        .HasColumnType("int");

                    b.Property<string>("EntityName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id", "AmoId");

                    b.ToTable("CFs");
                });

            modelBuilder.Entity("MZPO.Data.City", b =>
                {
                    b.Property<string>("EngName")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("RusName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("EngName");

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("MZPO.Data.Tag", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("AmoId")
                        .HasColumnType("int");

                    b.Property<string>("EntityName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id", "AmoId");

                    b.ToTable("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}