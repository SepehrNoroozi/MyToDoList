using System;
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
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; } // 1 = High, 2 = Medium, 3 = Low
        public List<string> Labels { get; set; } = new();
    }

    public class AppData
    {
        public List<Project> Projects { get; set; } = new();
    }
}
