namespace Simple.Data.GraphTest.Models
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Order> Orders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTimeOffset Date { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProductPrice> Prices { get; set; }
    }

    public class ProductPrice
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
        public decimal Price { get; set; }
    }

    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}