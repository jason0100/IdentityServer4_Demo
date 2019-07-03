﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [Route("todo")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _context;
        public TodoController(TodoContext context) {
            _context = context;
            //if (_context.TodoItems.Count() == 0) {
            //    _context.TodoItems.Add(new TodoItem { Name = "Item1" });
            //    _context.SaveChanges();
            //}
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems() {
            return await _context.TodoItems.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id) {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null) {
                return NotFound();
            }
            return todoItem;
        }

        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem item) {
            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTodoItem), new { id = item.Id }, item);

        }
    }
}