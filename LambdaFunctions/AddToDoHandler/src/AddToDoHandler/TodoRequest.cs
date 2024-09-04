
namespace AddToDoHandler
{
    public class TodoRequest
    {
        public ToDoFile File { get; set; }
        public string Notes { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public DateTime DueOn { get; set; }
    }
}
