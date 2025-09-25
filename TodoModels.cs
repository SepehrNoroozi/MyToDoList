using System;
using System.Collections.Generic;

namespace MyTodoist
{
    // Task model
    public class TaskItem
    {
        // Task title
        public string Title { get; set; } = "";

        // Due date
        public DateTime DueDate { get; set; }

        // Task priority (1 = Low, 3 = High)
        public int Priority { get; set; }

        // Labels
        public List<string> Labels { get; set; } = new List<string>();

        // Task Completion Status
        public bool IsCompleted { get; set; } = false;
    }

    // Project Model
    public class Project
    {
        // Project Name
        public string Name { get; set; } = "";

        // Project Task List
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    // Complete App Data
    public class AppData
    {
        public List<Project> Projects { get; set; } = new List<Project>();
    }
}
