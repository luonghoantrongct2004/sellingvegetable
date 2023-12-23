using System;
using System.Collections.Generic;

namespace FruityFresh.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ShipDate { get; set; }

    public bool? StaticOrder { get; set; }

    public bool? Paid { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? PaymentId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ShipperId { get; set; }

    public string? Status { get; set; }

    public int? OrderdetailId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Orderdetail? Orderdetail { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();

    public virtual Shipper? Shipper { get; set; }
}
