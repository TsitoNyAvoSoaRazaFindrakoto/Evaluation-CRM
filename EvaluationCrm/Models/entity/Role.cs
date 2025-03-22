﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvaluationCrm.Models.entity;

public partial class Role
{
    public int Id { get; set; }

    public string? Name { get; set; }

    [NotMapped]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
