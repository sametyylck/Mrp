using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace DAL.Models
{
    public partial class kolaymrpContext : DbContext
    {
        

        public virtual DbSet<Bom> Boms { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<Contact> Contacts { get; set; } = null!;
        public virtual DbSet<Currency> Currencies { get; set; } = null!;
        public virtual DbSet<GeneralDefaultSetting> GeneralDefaultSettings { get; set; } = null!;
        public virtual DbSet<Item> Items { get; set; } = null!;
        public virtual DbSet<Location> Locations { get; set; } = null!;
        public virtual DbSet<LocationStock> LocationStocks { get; set; } = null!;
        public virtual DbSet<Log> Logs { get; set; } = null!;
        public virtual DbSet<ManufacturingOrder> ManufacturingOrders { get; set; } = null!;
        public virtual DbSet<ManufacturingOrderItem> ManufacturingOrderItems { get; set; } = null!;
        public virtual DbSet<MeasureClas> Measures { get; set; } = null!;
        public virtual DbSet<Operation> Operations { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrdersItem> OrdersItems { get; set; } = null!;
        public virtual DbSet<ProductOperationsBom> ProductOperationsBoms { get; set; } = null!;
        public virtual DbSet<Resource> Resources { get; set; } = null!;
        public virtual DbSet<Rezerve> Rezerves { get; set; } = null!;
        public virtual DbSet<StockAdjusment> StockAdjusments { get; set; } = null!;
        public virtual DbSet<StockAdjusmentItem> StockAdjusmentItems { get; set; } = null!;
        public virtual DbSet<StockTransfer> StockTransfers { get; set; } = null!;
        public virtual DbSet<StockTransferItem> StockTransferItems { get; set; } = null!;
        public virtual DbSet<Tax> Taxes { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bom>(entity =>
            {
                entity.ToTable("Bom");

                entity.Property(e => e.id).HasColumnName("id");

                entity.Property(e => e.Note).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Boms)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Bom_Company");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Boms)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Bom_Items");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeletedUser).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Categories_Company");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DisplayName).HasMaxLength(50);

                entity.Property(e => e.LegalName).HasMaxLength(50);
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Comment).HasMaxLength(100);

                entity.Property(e => e.CompanyName).HasMaxLength(50);

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeletedUser).HasMaxLength(50);

                entity.Property(e => e.DisplayName).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Mail).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(18);

                entity.Property(e => e.Tip).HasMaxLength(50);

                entity.HasOne(d => d.BillingLocation)
                    .WithMany(p => p.Contacts)
                    .HasForeignKey(d => d.BillingLocationId)
                    .HasConstraintName("FK_Contacts_Locations");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Contacts)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Contacts_Company");
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("Currency");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name).HasMaxLength(5);
            });

            modelBuilder.Entity<GeneralDefaultSetting>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.GeneralDefaultSettings)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_GeneralDefaultSettings_Company");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.GeneralDefaultSettings)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_GeneralDefaultSettings_Currency");

                entity.HasOne(d => d.DefaultManufacturingLocation)
                    .WithMany(p => p.GeneralDefaultSettingDefaultManufacturingLocations)
                    .HasForeignKey(d => d.DefaultManufacturingLocationId)
                    .HasConstraintName("FK_GeneralDefaultSettings_Locations2");

                entity.HasOne(d => d.DefaultPurchaseLocation)
                    .WithMany(p => p.GeneralDefaultSettingDefaultPurchaseLocations)
                    .HasForeignKey(d => d.DefaultPurchaseLocationId)
                    .HasConstraintName("FK_GeneralDefaultSettings_Locations1");

                entity.HasOne(d => d.DefaultSalesLocation)
                    .WithMany(p => p.GeneralDefaultSettingDefaultSalesLocations)
                    .HasForeignKey(d => d.DefaultSalesLocationId)
                    .HasConstraintName("FK_GeneralDefaultSettings_Locations");

                entity.HasOne(d => d.DefaultTaxPurchaseOrder)
                    .WithMany(p => p.GeneralDefaultSettingDefaultTaxPurchaseOrders)
                    .HasForeignKey(d => d.DefaultTaxPurchaseOrderId)
                    .HasConstraintName("FK_GeneralDefaultSettings_Tax1");

                entity.HasOne(d => d.DefaultTaxSalesOrder)
                    .WithMany(p => p.GeneralDefaultSettingDefaultTaxSalesOrders)
                    .HasForeignKey(d => d.DefaultTaxSalesOrderId)
                    .HasConstraintName("FK_GeneralDefaultSettings_Tax");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DefaultPrice).HasDefaultValueSql("((0))");

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeletedUser).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Tip).HasMaxLength(50);

                entity.Property(e => e.VariantCode).HasMaxLength(50);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Items_Categories");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Items_Company");

                entity.HasOne(d => d.Contact)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.ContactId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Items_Contacts");

                entity.HasOne(d => d.Measure)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.MeasureId)
                    .HasConstraintName("FK_Items_Measure");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AddressLine1).HasMaxLength(100);

                entity.Property(e => e.AddressLine2).HasMaxLength(100);

                entity.Property(e => e.CityTown).HasMaxLength(50);

                entity.Property(e => e.CompanyName).HasMaxLength(50);

                entity.Property(e => e.Country).HasMaxLength(50);

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeletedUser).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.LegalName).HasMaxLength(50);

                entity.Property(e => e.LocationName).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(18);

                entity.Property(e => e.StateRegion).HasMaxLength(50);

                entity.Property(e => e.Tip).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Locations_Company");
            });

            modelBuilder.Entity<LocationStock>(entity =>
            {
                entity.ToTable("LocationStock");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Tip).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.LocationStocks)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_LocationStock_Company");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.LocationStocks)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_LocationStock_Items");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.LocationStocks)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_LocationStock_Locations");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("Log");

                entity.Property(e => e.Level).HasMaxLength(50);

                entity.Property(e => e.Logged).HasColumnType("datetime");

                entity.Property(e => e.Logger).HasMaxLength(250);

                entity.Property(e => e.MachineName).HasMaxLength(50);
            });

            modelBuilder.Entity<ManufacturingOrder>(entity =>
            {
                entity.ToTable("ManufacturingOrder");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeleteUser).HasMaxLength(50);

                entity.Property(e => e.DoneDate).HasColumnType("datetime");

                entity.Property(e => e.ExpectedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.ManufacturingOrders)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_ManufacturingOrder_Company1");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ManufacturingOrders)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_ManufacturingOrder_Items1");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.ManufacturingOrders)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_ManufacturingOrder_Locations");
            });

            modelBuilder.Entity<ManufacturingOrderItem>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Notes).HasMaxLength(50);

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Tip).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.ManufacturingOrderItems)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_ManufacturingOrderItems_Company");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.ManufacturingOrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_ManufacturingOrderItems_ManufacturingOrder1");
            });

            modelBuilder.Entity<MeasureClas>(entity =>
            {
                entity.ToTable("Measure");

                entity.Property(e => e.id).HasColumnName("id");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Measures)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Measure_Company");
            });

            modelBuilder.Entity<Operation>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Operations)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Operations_Company");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeletedUser).HasMaxLength(50);

                entity.Property(e => e.DeliveryDeadline).HasColumnType("datetime");

                entity.Property(e => e.ExpectedDate).HasColumnType("datetime");

                entity.Property(e => e.Info).HasMaxLength(150);

                entity.Property(e => e.OrderName).HasMaxLength(50);

                entity.Property(e => e.Tip).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Ordes_Company");

                entity.HasOne(d => d.Contact)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ContactId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Ordes_Contacts");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_Orders_Locations");
            });

            modelBuilder.Entity<OrdersItem>(entity =>
            {
                entity.ToTable("OrdersItem");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Tip).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.OrdersItems)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_PurchaseOrderItems_Company");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.OrdersItems)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_PurchaseOrderItems_Items");

                entity.HasOne(d => d.Orders)
                    .WithMany(p => p.OrdersItems)
                    .HasForeignKey(d => d.OrdersId)
                    .HasConstraintName("FK_PurchaseOrderItems_Orders");
            });

            modelBuilder.Entity<ProductOperationsBom>(entity =>
            {
                entity.ToTable("ProductOperationsBom");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.ProductOperationsBoms)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_ProductOperationsBom_Company");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ProductOperationsBoms)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_ProductOperationsBom_Items");

                entity.HasOne(d => d.Operation)
                    .WithMany(p => p.ProductOperationsBoms)
                    .HasForeignKey(d => d.OperationId)
                    .HasConstraintName("FK_ProductOperationsBom_Operations");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ProductOperationsBoms)
                    .HasForeignKey(d => d.ResourceId)
                    .HasConstraintName("FK_ProductOperationsBom_Resources");
            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Resources)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Resources_Company");
            });

            modelBuilder.Entity<Rezerve>(entity =>
            {
                entity.ToTable("Rezerve");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Tip).HasMaxLength(50);
            });

            modelBuilder.Entity<StockAdjusment>(entity =>
            {
                entity.ToTable("StockAdjusment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeletedUser).HasMaxLength(50);

                entity.Property(e => e.Info).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Reason).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.StockAdjusments)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_StockAdjusment_Company");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.StockAdjusments)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_StockAdjusment_Locations");
            });

            modelBuilder.Entity<StockAdjusmentItem>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.StockAdjusmentItems)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_StockAdjusmentItems_Company");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.StockAdjusmentItems)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_StockAdjusmentItems_Items");

                entity.HasOne(d => d.StockAdjusment)
                    .WithMany(p => p.StockAdjusmentItems)
                    .HasForeignKey(d => d.StockAdjusmentId)
                    .HasConstraintName("FK_StockAdjusmentItems_StockAdjusment");
            });

            modelBuilder.Entity<StockTransfer>(entity =>
            {
                entity.ToTable("StockTransfer");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DeleteDate).HasColumnType("datetime");

                entity.Property(e => e.DeletedUser).HasMaxLength(50);

                entity.Property(e => e.Info).HasMaxLength(50);

                entity.Property(e => e.StockTransferName).HasMaxLength(50);

                entity.Property(e => e.TransferDate).HasColumnType("datetime");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.StockTransfers)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_StockTransfer_Company");

                entity.HasOne(d => d.Destination)
                    .WithMany(p => p.StockTransferDestinations)
                    .HasForeignKey(d => d.DestinationId)
                    .HasConstraintName("FK_StockTransfer_Locations1");

                entity.HasOne(d => d.Origin)
                    .WithMany(p => p.StockTransferOrigins)
                    .HasForeignKey(d => d.OriginId)
                    .HasConstraintName("FK_StockTransfer_Locations");
            });

            modelBuilder.Entity<StockTransferItem>(entity =>
            {
                entity.Property(e => e.id).HasColumnName("id");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.StockTransferItems)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_StockTranferItems_Company");

                entity.HasOne(d => d.StockTransfer)
                    .WithMany(p => p.StockTransferItems)
                    .HasForeignKey(d => d.StockTransferId)
                    .HasConstraintName("FK_StockTranferItems_StockTransfer");
            });

            modelBuilder.Entity<Tax>(entity =>
            {
                entity.ToTable("Tax");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.TaxName).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Taxes)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Tax_Company");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Mail).HasMaxLength(50);

                entity.Property(e => e.PasswordSalt).HasMaxLength(16);

                entity.Property(e => e.PasswordHash).HasMaxLength(16);

                entity.Property(e => e.PhoneNumber).HasMaxLength(18);

                entity.Property(e => e.Role).HasMaxLength(5);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Users_Company");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
