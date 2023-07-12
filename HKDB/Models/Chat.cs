using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HKDB.Models;
public partial class Chat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ChatId { get; set; }
    public DateTime ChatTime { get; set; } = DateTime.Now;
    public string? ChatName { get; set; }
    public string? ApplicationId { get; set; }
    public virtual int UserId { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<Qahistory> Qahistorie { get; set; }
}

