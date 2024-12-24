﻿// <auto-generated />
using System;
using Game.Server.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GameServer.Migrations
{
    [DbContext(typeof(GameDbContext))]
    partial class GameDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("GameServer.Infrastructure.Player", b =>
                {
                    b.Property<int>("PlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("TEXT");

                    b.HasKey("PlayerId");

                    b.HasIndex("DeviceId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("GameServer.Infrastructure.PlayerBalance", b =>
                {
                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("ResourceType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ResourceBalance")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RowVersion")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("PlayerId", "ResourceType");

                    b.ToTable("PlayersBalances", t =>
                        {
                            t.HasCheckConstraint("CK_PlayerBalance_Positive", "ResourceBalance >= 0");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
