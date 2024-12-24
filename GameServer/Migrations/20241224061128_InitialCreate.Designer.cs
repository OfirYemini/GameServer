﻿// <auto-generated />
using System;
using GameServer.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GameServer.Migrations
{
    [DbContext(typeof(GameDbContext))]
    [Migration("20241224061128_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("GameServer.DataAccess.Player", b =>
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

            modelBuilder.Entity("GameServer.DataAccess.PlayerBalance", b =>
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

                    b.ToTable("PlayersBalances");
                });
#pragma warning restore 612, 618
        }
    }
}
