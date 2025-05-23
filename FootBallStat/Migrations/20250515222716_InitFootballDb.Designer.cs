﻿// <auto-generated />
using System;
using FootBallStat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FootBallStat.Migrations
{
    [DbContext(typeof(DBFootballStatContext))]
    [Migration("20250515222716_InitFootballDb")]
    partial class InitFootballDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("FootBallStat.Championship", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CountryId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.ToTable("Championships");
                });

            modelBuilder.Entity("FootBallStat.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("FootBallStat.Match", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("ChampionshipId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime");

                    b.Property<int>("Team1Id")
                        .HasColumnType("int");

                    b.Property<int>("Team2Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ChampionshipId");

                    b.HasIndex("Team1Id");

                    b.HasIndex("Team2Id");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("FootBallStat.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Number")
                        .HasColumnType("int");

                    b.Property<int>("PositionId")
                        .HasColumnType("int");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PositionId");

                    b.HasIndex("TeamId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("FootBallStat.PlayersInMatch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("MatchId")
                        .HasColumnType("int");

                    b.Property<int>("PlayerGoals")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.HasIndex("PlayerId");

                    b.ToTable("PlayersInMatches");
                });

            modelBuilder.Entity("FootBallStat.Position", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("FootBallStat.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("FootBallStat.Championship", b =>
                {
                    b.HasOne("FootBallStat.Country", "Country")
                        .WithMany("Championships")
                        .HasForeignKey("CountryId")
                        .IsRequired()
                        .HasConstraintName("FK_Championship_Country");

                    b.Navigation("Country");
                });

            modelBuilder.Entity("FootBallStat.Match", b =>
                {
                    b.HasOne("FootBallStat.Championship", "Championship")
                        .WithMany("Matches")
                        .HasForeignKey("ChampionshipId")
                        .IsRequired()
                        .HasConstraintName("FK_Match_Championship");

                    b.HasOne("FootBallStat.Team", "Team1")
                        .WithMany("MatchTeam1s")
                        .HasForeignKey("Team1Id")
                        .IsRequired()
                        .HasConstraintName("FK_Match_Team");

                    b.HasOne("FootBallStat.Team", "Team2")
                        .WithMany("MatchTeam2s")
                        .HasForeignKey("Team2Id")
                        .IsRequired()
                        .HasConstraintName("FK_Match_Team1");

                    b.Navigation("Championship");

                    b.Navigation("Team1");

                    b.Navigation("Team2");
                });

            modelBuilder.Entity("FootBallStat.Player", b =>
                {
                    b.HasOne("FootBallStat.Position", "Position")
                        .WithMany("Players")
                        .HasForeignKey("PositionId")
                        .IsRequired()
                        .HasConstraintName("FK_Players_Positions");

                    b.HasOne("FootBallStat.Team", "Team")
                        .WithMany("Players")
                        .HasForeignKey("TeamId")
                        .IsRequired()
                        .HasConstraintName("FK_Player_Team");

                    b.Navigation("Position");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("FootBallStat.PlayersInMatch", b =>
                {
                    b.HasOne("FootBallStat.Match", "Match")
                        .WithMany("PlayersInMatches")
                        .HasForeignKey("MatchId")
                        .IsRequired()
                        .HasConstraintName("FK_PlayerInMatch_Match");

                    b.HasOne("FootBallStat.Player", "Player")
                        .WithMany("PlayersInMatches")
                        .HasForeignKey("PlayerId")
                        .IsRequired()
                        .HasConstraintName("FK_PlayersInMatches_Players");

                    b.Navigation("Match");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("FootBallStat.Championship", b =>
                {
                    b.Navigation("Matches");
                });

            modelBuilder.Entity("FootBallStat.Country", b =>
                {
                    b.Navigation("Championships");
                });

            modelBuilder.Entity("FootBallStat.Match", b =>
                {
                    b.Navigation("PlayersInMatches");
                });

            modelBuilder.Entity("FootBallStat.Player", b =>
                {
                    b.Navigation("PlayersInMatches");
                });

            modelBuilder.Entity("FootBallStat.Position", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("FootBallStat.Team", b =>
                {
                    b.Navigation("MatchTeam1s");

                    b.Navigation("MatchTeam2s");

                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
