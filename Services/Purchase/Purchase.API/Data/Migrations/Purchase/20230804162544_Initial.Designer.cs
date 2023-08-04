﻿// <auto-generated />
using System;
using Me.Services.Purchase.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Purchase.API.Migrations.Purchase
{
    [DbContext(typeof(PurchaseContext))]
    [Migration("20230804162544_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.HasSequence("buyerseq", "purchase")
                .IncrementsBy(10);

            modelBuilder.HasSequence("orderitemseq")
                .IncrementsBy(10);

            modelBuilder.HasSequence("orderseq", "purchase")
                .IncrementsBy(10);

            modelBuilder.HasSequence("paymentseq", "purchase")
                .IncrementsBy(10);

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.Buyer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"), "buyerseq", "purchase");

                    b.Property<string>("IdentityGuid")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("IdentityGuid")
                        .IsUnique();

                    b.ToTable("buyers", "purchase");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.CardType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.ToTable("cardtypes", "purchase");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.PaymentMethod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"), "paymentseq", "purchase");

                    b.Property<int>("BuyerId")
                        .HasColumnType("int");

                    b.Property<string>("_alias")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("Alias");

                    b.Property<string>("_cardHolderName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("CardHolderName");

                    b.Property<string>("_cardNumber")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)")
                        .HasColumnName("CardNumber");

                    b.Property<int>("_cardTypeId")
                        .HasColumnType("int")
                        .HasColumnName("CardTypeId");

                    b.Property<DateTime>("_expiration")
                        .HasMaxLength(25)
                        .HasColumnType("datetime2")
                        .HasColumnName("Expiration");

                    b.HasKey("Id");

                    b.HasIndex("BuyerId");

                    b.HasIndex("_cardTypeId");

                    b.ToTable("paymentmethods", "purchase");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"), "orderseq", "purchase");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("_buyerId")
                        .HasColumnType("int")
                        .HasColumnName("BuyerId");

                    b.Property<DateTime>("_orderDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("OrderDate");

                    b.Property<int>("_orderStatusId")
                        .HasColumnType("int")
                        .HasColumnName("OrderStatusId");

                    b.Property<int?>("_paymentMethodId")
                        .HasColumnType("int")
                        .HasColumnName("PaymentMethodId");

                    b.HasKey("Id");

                    b.HasIndex("_buyerId");

                    b.HasIndex("_orderStatusId");

                    b.HasIndex("_paymentMethodId");

                    b.ToTable("orders", "purchase");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.OrderItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"), "orderitemseq");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<decimal>("_discount")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("Discount");

                    b.Property<string>("_pictureUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("PictureUrl");

                    b.Property<string>("_productName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("ProductName");

                    b.Property<decimal>("_unitPrice")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("UnitPrice");

                    b.Property<int>("_units")
                        .HasColumnType("int")
                        .HasColumnName("Units");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("orderItems", "purchase");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.OrderStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.ToTable("orderstatus", "purchase");
                });

            modelBuilder.Entity("Me.Services.Purchase.Infrastructure.Idempotency.ClientRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("requests", "purchase");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.PaymentMethod", b =>
                {
                    b.HasOne("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.Buyer", null)
                        .WithMany("PaymentMethods")
                        .HasForeignKey("BuyerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.CardType", "CardType")
                        .WithMany()
                        .HasForeignKey("_cardTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CardType");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.Order", b =>
                {
                    b.HasOne("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.Buyer", null)
                        .WithMany()
                        .HasForeignKey("_buyerId");

                    b.HasOne("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.OrderStatus", "OrderStatus")
                        .WithMany()
                        .HasForeignKey("_orderStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.PaymentMethod", null)
                        .WithMany()
                        .HasForeignKey("_paymentMethodId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.OwnsOne("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.Address", "Address", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseHiLo(b1.Property<int>("OrderId"), "orderseq", "purchase");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Country")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("State")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("ZipCode")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("OrderId");

                            b1.ToTable("orders", "purchase");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");
                        });

                    b.Navigation("Address")
                        .IsRequired();

                    b.Navigation("OrderStatus");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.OrderItem", b =>
                {
                    b.HasOne("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.Order", null)
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate.Buyer", b =>
                {
                    b.Navigation("PaymentMethods");
                });

            modelBuilder.Entity("Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate.Order", b =>
                {
                    b.Navigation("OrderItems");
                });
#pragma warning restore 612, 618
        }
    }
}
