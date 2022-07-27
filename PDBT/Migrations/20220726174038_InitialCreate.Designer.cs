﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PDBT.Data;

#nullable disable

namespace PDBT.Migrations
{
    [DbContext(typeof(PdbtContext))]
    [Migration("20220726174038_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("PDBT.Models.Issue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("IssueName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime?>("TimeForCompletion")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Issues");

                    b.HasCheckConstraint("CK_Issues_Priority_Enum", "`Priority` IN (0, 1, 2, 3, 4)");

                    b.HasCheckConstraint("CK_Issues_Type_Enum", "`Type` IN (0, 1, 2, 3)");
                });

            modelBuilder.Entity("PDBT.Models.Label", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("IssueId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("IssueId");

                    b.ToTable("Labels");
                });

            modelBuilder.Entity("PDBT.Models.LinkedIssue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("IssueId")
                        .HasColumnType("int");

                    b.Property<int>("Reason")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IssueId");

                    b.ToTable("LinkedIssues");

                    b.HasCheckConstraint("CK_LinkedIssues_Reason_Enum", "`Reason` IN (0, 1, 2)");
                });

            modelBuilder.Entity("PDBT.Models.Label", b =>
                {
                    b.HasOne("PDBT.Models.Issue", null)
                        .WithMany("Labels")
                        .HasForeignKey("IssueId");
                });

            modelBuilder.Entity("PDBT.Models.LinkedIssue", b =>
                {
                    b.HasOne("PDBT.Models.Issue", "Issue")
                        .WithMany("LinkedIssues")
                        .HasForeignKey("IssueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Issue");
                });

            modelBuilder.Entity("PDBT.Models.Issue", b =>
                {
                    b.Navigation("Labels");

                    b.Navigation("LinkedIssues");
                });
#pragma warning restore 612, 618
        }
    }
}
