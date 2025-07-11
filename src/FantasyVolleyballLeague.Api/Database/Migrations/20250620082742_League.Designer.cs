﻿// <auto-generated />
using System;
using FantasyVolleyballLeague.Api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FantasyVolleyballLeague.Api.Database.Migrations
{
    [DbContext(typeof(FantasyVolleyballLeagueDbContext))]
    [Migration("20250620082742_League")]
    partial class League
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.League", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Country")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("League");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Match", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AwayTeamId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("HomeTeamId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AwayTeamId");

                    b.HasIndex("HomeTeamId");

                    b.ToTable("Match");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("Player");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.PlayerStatistics", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Aces")
                        .HasColumnType("int");

                    b.Property<int>("Blocks")
                        .HasColumnType("int");

                    b.Property<int>("Errors")
                        .HasColumnType("int");

                    b.Property<Guid>("MatchId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("PointsScored")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.HasIndex("PlayerId");

                    b.ToTable("PlayerStatistics");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Team", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("LeagueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("LeagueId");

                    b.ToTable("Team");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.UserTeam", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserTeam");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.UserTeamPlayer", b =>
                {
                    b.Property<Guid>("UserTeamId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserTeamId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("UserTeamPlayer");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Match", b =>
                {
                    b.HasOne("FantasyVolleyballLeague.Api.Entities.Team", "AwayTeam")
                        .WithMany()
                        .HasForeignKey("AwayTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FantasyVolleyballLeague.Api.Entities.Team", "HomeTeam")
                        .WithMany()
                        .HasForeignKey("HomeTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AwayTeam");

                    b.Navigation("HomeTeam");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Player", b =>
                {
                    b.HasOne("FantasyVolleyballLeague.Api.Entities.Team", "Team")
                        .WithMany("Players")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Team");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.PlayerStatistics", b =>
                {
                    b.HasOne("FantasyVolleyballLeague.Api.Entities.Match", "Match")
                        .WithMany("PlayerStatistics")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FantasyVolleyballLeague.Api.Entities.Player", "Player")
                        .WithMany("Statistics")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Match");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Team", b =>
                {
                    b.HasOne("FantasyVolleyballLeague.Api.Entities.League", "League")
                        .WithMany()
                        .HasForeignKey("LeagueId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("League");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.UserTeamPlayer", b =>
                {
                    b.HasOne("FantasyVolleyballLeague.Api.Entities.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FantasyVolleyballLeague.Api.Entities.UserTeam", "UserTeam")
                        .WithMany("Players")
                        .HasForeignKey("UserTeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");

                    b.Navigation("UserTeam");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Match", b =>
                {
                    b.Navigation("PlayerStatistics");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Player", b =>
                {
                    b.Navigation("Statistics");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.Team", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("FantasyVolleyballLeague.Api.Entities.UserTeam", b =>
                {
                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
