﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrderService.API.Infrastructure.DBContext;

#nullable disable

namespace OrderService.API.Migrations
{
    [DbContext(typeof(OrderDbContext))]
    [Migration("20241015144040_UpdateToSnakeCase")]
    partial class UpdateToSnakeCase
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OrderService.API.Infrastructure.Entities.Order", b =>
                {
                    b.Property<Guid>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("order_id");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid")
                        .HasColumnName("customer_id");

                    b.Property<decimal>("DiscountPercentage")
                        .HasColumnType("numeric")
                        .HasColumnName("discount_percentage");

                    b.Property<decimal>("DiscountedTotal")
                        .HasColumnType("numeric")
                        .HasColumnName("discounted_total");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("order_date");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("numeric")
                        .HasColumnName("total_amount");

                    b.HasKey("OrderId");

                    b.ToTable("orders", (string)null);
                });

            modelBuilder.Entity("OrderService.API.Infrastructure.Entities.OrderItem", b =>
                {
                    b.Property<Guid>("OrderItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("order_item_id");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid")
                        .HasColumnName("order_id");

                    b.Property<int>("ProductId")
                        .HasColumnType("integer")
                        .HasColumnName("product_id");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("numeric")
                        .HasColumnName("unit_price");

                    b.HasKey("OrderItemId");

                    b.HasIndex("OrderId");

                    b.ToTable("order_items", (string)null);
                });

            modelBuilder.Entity("OrderService.API.Infrastructure.Entities.OrderItem", b =>
                {
                    b.HasOne("OrderService.API.Infrastructure.Entities.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("OrderService.API.Infrastructure.Entities.Order", b =>
                {
                    b.Navigation("OrderItems");
                });
#pragma warning restore 612, 618
        }
    }
}
