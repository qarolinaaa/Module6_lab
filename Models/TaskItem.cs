using System.ComponentModel.DataAnnotations;
namespace TaskManagementApi.Models;
public class TaskItem
{
    public int Id { get; set; }
    [Required, StringLength(120, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
