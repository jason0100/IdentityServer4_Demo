﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models {
    public class TodoContext:DbContext {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
        }
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}
