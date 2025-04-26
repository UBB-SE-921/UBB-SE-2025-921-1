// <copyright file="SellerApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing seller data.
    /// </summary>
    [Route("api/sellers")]
    [ApiController]
    public class SellerApiController : ControllerBase
    {
        private readonly ISellerRepository sellerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerApiController"/> class.
        /// </summary>
        /// <param name="sellerRepository">The seller repository dependency.</param>
        public SellerApiController(ISellerRepository sellerRepository)
        {
            this.sellerRepository = sellerRepository;
        }
    }
}
