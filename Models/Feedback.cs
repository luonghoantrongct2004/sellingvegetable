using System;
using System.Collections.Generic;

namespace FruityFresh.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public string? FeedbackText { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Customer? User { get; set; }
}
