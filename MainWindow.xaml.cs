using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic; // برای InputBox

namespace MyTodoist
{
    public partial class MainWindow : Window
    {
        private const string DataFile = "data.json";
        private AppData appData = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            RefreshProjectsList();
        }

        private void LoadData()
        {
            if (File.Exists(DataFile))
            {
                string json = File.ReadAllText(DataFile);
                appData = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            }
            else
            {
                appData = new AppData();
                SaveData();
            }
        }

        private void SaveData()
        {
            string json = JsonSerializer.Serialize(appData, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(DataFile, json);
        }

        private void RefreshProjectsList()
        {
            ProjectsList.Items.Clear();
            foreach (var project in appData.Projects)
            {
                ProjectsList.Items.Add(project.Name);
            }
        }

        private void RefreshTasksList(Project project)
        {
            TasksList.Items.Clear();
            foreach (var task in project.Tasks)
            {
                string due = task.DueDate.HasValue ? task.DueDate.Value.ToString("yyyy-MM-dd") : "No date";
                string priorityText = task.Priority switch
                {
                    1 => "High",
                    2 => "Medium",
                    3 => "Low",
                    _ => "?"
                };
                TasksList.Items.Add($"{task.Title} | {due} | {priorityText}");
            }
        }

        private void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedIndex >= 0)
            {
                var project = appData.Projects[ProjectsList.SelectedIndex];
                TasksHeader.Text = $"{project.Name} - Tasks";
                RefreshTasksList(project);
            }
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            string newProject = Interaction.InputBox("Enter project name:", "New Project", $"Project {appData.Projects.Count + 1}");
            if (string.IsNullOrWhiteSpace(newProject)) return;

            appData.Projects.Add(new Project { Name = newProject });
            SaveData();
            RefreshProjectsList();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex >= 0)
            {
                var project = appData.Projects[ProjectsList.SelectedIndex];

                string title = Interaction.InputBox("Enter task title:", "New Task", $"Task {project.Tasks.Count + 1}");
                if (string.IsNullOrWhiteSpace(title)) return;

                string dateInput = Interaction.InputBox("Enter due date (yyyy-mm-dd):", "Due Date", "");
                DateTime? dueDate = null;
                if (DateTime.TryParse(dateInput, out DateTime parsedDate))
                {
                    dueDate = parsedDate;
                }

                string priorityInput = Interaction.InputBox("Enter priority (1=High, 2=Medium, 3=Low):", "Priority", "2");
                int priority = 2;
                int.TryParse(priorityInput, out priority);

                project.Tasks.Add(new TaskItem
                {
                    Title = title,
                    DueDate = dueDate,
                    Priority = priority
                });

                SaveData();
                RefreshTasksList(project);
            }
            else
            {
                MessageBox.Show("Select a project first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
