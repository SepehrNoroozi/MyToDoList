using System.Collections.Generic;

namespace MyTodoist
{
    public class Project
    {
        public string Name { get; set; }
        public List<TaskItem> Tasks { get; set; } = new();
    }

    public class TaskItem
    {
        public string Title { get; set; }
    }

    public class AppData
    {
        public List<Project> Projects { get; set; } = new();
    }
}
