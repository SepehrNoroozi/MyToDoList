using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualBasic;

namespace MyTodoist
{
    public partial class MainWindow : Window
    {
        private const string DataFile = "data.json";
        private AppData appData = new();

        private (int projectIndex, int taskIndex)? _dragData;
        private Point _dragStartPoint;

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

        // ---- Drag & Drop ----
        private void TasksList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            if (TasksList.SelectedIndex >= 0 && ProjectsList.SelectedIndex >= 0)
            {
                _dragData = (ProjectsList.SelectedIndex, TasksList.SelectedIndex);
                DragDrop.DoDragDrop(TasksList, _dragData, DragDropEffects.Move);
            }
        }

        private void TasksList_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ValueTuple<int, int>)))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void TasksList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ValueTuple<int, int>)))
            {
                var (sourceProjectIndex, sourceTaskIndex) = ((int, int))e.Data.GetData(typeof(ValueTuple<int, int>));
                var targetProjectIndex = ProjectsList.SelectedIndex;

                if (sourceProjectIndex >= 0 && sourceTaskIndex >= 0 &&
                    targetProjectIndex >= 0 && targetProjectIndex < appData.Projects.Count)
                {
                    var task = appData.Projects[sourceProjectIndex].Tasks[sourceTaskIndex];
                    appData.Projects[sourceProjectIndex].Tasks.RemoveAt(sourceTaskIndex);
                    appData.Projects[targetProjectIndex].Tasks.Add(task);

                    SaveData();
                    RefreshTasksList(appData.Projects[targetProjectIndex]);
                }
            }
        }
    }
}
