// -----------------------------------------------------------------------
// <copyright file="Product.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a product available in the marketplace.
/// </summary>
public class Product
{
    // Add this private parameterless constructor for Entity Framework Core
    private Product() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Product"/> class.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="name">The name of the product.</param>
    /// <param name="description">The description of the product.</param>
    /// <param name="price">The price of the product.</param>
    /// <param name="stock">The available stock quantity.</param>
    /// <param name="sellerId">The identifier of the seller.</param>
    public Product(int productId = 0, string name = "", string description = "", double price = 0, int stock = 0, int sellerId = 0)
    {
        this.ProductId = productId;
        this.Name = name;
        this.Description = description;
        this.Price = price;
        this.SellerId = sellerId;
        this.Stock = stock;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the seller.
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// Gets or sets the available stock quantity.
    /// </summary>
    public int Stock { get; set; }

    // from Art-Attack - added by Alex, not merged by who needed to do it
    public string ProductType { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
}
