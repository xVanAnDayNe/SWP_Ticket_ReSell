﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class Package
{
    public int ID_Package { get; set; }

    public string Name_Package { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}