using System;
using System.Collections.Generic;

namespace FruityFresh.Models;

public partial class Shipper
{
    public int ShipperId { get; set; }

    public string? ShipperName { get; set; }

    public string? Phone { get; set; }

    public string? Company { get; set; }

    public DateTime? ShipDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
