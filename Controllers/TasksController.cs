using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Models;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    // Учебное хранилище в памяти: записи сбрасываются при перезапуске.
    private static readonly List<TaskItem> Tasks = new()
    {
        new TaskItem { Id = 1, Title = "Изучить JWT", Description = "Разобраться с созданием и проверкой токенов.", IsCompleted = false },
        new TaskItem { Id = 2, Title = "Создать API", Description = "Реализовать методы Task Management API.", IsCompleted = true },
        new TaskItem { Id = 3, Title = "Проверить безопасность", Description = "Проверить запросы с токеном и без него.", IsCompleted = false }
    };
    private static int _nextId = 4;
    private static readonly object Sync = new();

    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<TaskItem>> GetAll()
    {
        lock (Sync) return Ok(Tasks.Select(Clone).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ActionResult<TaskItem> GetById(int id)
    {
        lock (Sync)
        {
            var task = Tasks.FirstOrDefault(t => t.Id == id);
            return task is null ? NotFound(new { message = $"Задача с ID {id} не найдена." }) : Ok(Clone(task));
        }
    }

    [Authorize]
    [HttpPost]
    public ActionResult<object> Create([FromBody] TaskItem request)
    {
        var username = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            ?? HttpContext.User.Identity?.Name ?? "unknown";
        TaskItem created;
        lock (Sync)
        {
            created = new TaskItem { Id = _nextId++, Title = request.Title, Description = request.Description, IsCompleted = request.IsCompleted };
            Tasks.Add(created);
        }
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
        {
            task = created, createdBy = username,
            claims = HttpContext.User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        lock (Sync)
        {
            var task = Tasks.FirstOrDefault(t => t.Id == id);
            if (task is null) return NotFound(new { message = $"Задача с ID {id} не найдена." });
            Tasks.Remove(task);
        }
        return NoContent();
    }

    private static TaskItem Clone(TaskItem task) => new()
    {
        Id = task.Id, Title = task.Title, Description = task.Description, IsCompleted = task.IsCompleted
    };
}
