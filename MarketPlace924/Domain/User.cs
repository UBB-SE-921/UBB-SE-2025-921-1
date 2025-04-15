// -----------------------------------------------------------------------
// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MarketPlace924.Domain;

using System;

/// <summary>
/// Represents a user in the marketplace system.
/// </summary>
public class User
{
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="userID">The user identifier.</param>
    /// <param name="username">The username.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="phoneNumber">The user's phone number.</param>
    /// <param name="password">The user's password (should be hashed).</param>
    /// <param name="role">The user's role.</param>
    /// <param name="failedLogIns">The count of failed login attempts.</param>
    /// <param name="bannedUntil">The date until which the user is banned, if any.</param>
    /// <param name="isBanned">A value indicating whether the user is currently banned.</param>
    public User(int userID = 0, string username = "", string email = "", string phoneNumber = "", string password = "", UserRole role = UserRole.Unassigned, int failedLogIns = 0, DateTime? bannedUntil = null, bool isBanned = false)
    {
        this.UserId = userID;
        this.Username = username;
        this.Email = email;
        this.Password = password;
        this.Role = role;
        this.PhoneNumber = phoneNumber;
        this.BannedUntil = bannedUntil;
        this.IsBanned = isBanned;
        this.FailedLogins = failedLogIns;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password (should be stored securely, e.g., hashed).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time until which the user is banned. Null if not banned.
    /// </summary>
    public DateTime? BannedUntil { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is currently banned.
    /// </summary>
    public bool IsBanned { get; set; }

    /// <summary>
    /// Gets or sets the count of consecutive failed login attempts.
    /// </summary>
    public int FailedLogins { get; set; }
}
