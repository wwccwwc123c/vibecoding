using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vibecoding.Api.Data;
using Vibecoding.Api.Models;

namespace Vibecoding.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly AppDbContext _db;

    public TodosController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll()
    {
        var data = await _db.Todos.OrderByDescending(x => x.Id).ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetById(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo == null) return NotFound();
        return Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create([FromBody] CreateTodoRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            return BadRequest("Title is required.");

        var todo = new TodoItem
        {
            Title = input.Title.Trim(),
            IsDone = input.IsDone,
            CreatedAt = DateTime.UtcNow
        };
        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoItem>> Update(int id, [FromBody] UpdateTodoRequest input)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo == null) return NotFound();

        if (string.IsNullOrWhiteSpace(input.Title))
            return BadRequest("Title is required.");

        todo.Title = input.Title.Trim();
        todo.IsDone = input.IsDone;
        await _db.SaveChangesAsync();
        return Ok(todo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo == null) return NotFound();

        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    public class CreateTodoRequest
    {
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }

    public class UpdateTodoRequest
    {
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }
}