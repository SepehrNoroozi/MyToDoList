using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyTodoist
{
    public partial class MainWindow : Window
    {
        private AppData data = new();
        private string filePath = "data.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            RefreshProjectsList();
            FilterBox.Text = "Enter label to filter";
        }

        private void LoadData()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                data = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            }
        }

        private void SaveData()
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        private void RefreshProjectsList()
        {
            ProjectsList.ItemsSource = null;
            ProjectsList.ItemsSource = data.Projects.Select(p => p.Name);
        }

        private void RefreshTasksList(Project project)
        {
            TasksList.ItemsSource = null;
            TasksList.ItemsSource = project.Tasks.Select(t =>
                $"{t.Title} (Due: {t.DueDate?.ToShortDateString() ?? "N/A"}, Priority: {t.Priority}, Labels: [{string.Join(", ", t.Labels)}])");
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Enter project name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                data.Projects.Add(new Project { Name = name });
                SaveData();
                RefreshProjectsList();
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;

            var title = Microsoft.VisualBasic.Interaction.InputBox("Task title:");
            if (string.IsNullOrWhiteSpace(title)) return;

            var dueDateStr = Microsoft.VisualBasic.Interaction.InputBox("Due date (yyyy-mm-dd) or leave empty:");
            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(dueDateStr) && DateTime.TryParse(dueDateStr, out var date))
                dueDate = date;

            var priorityStr = Microsoft.VisualBasic.Interaction.InputBox("Priority (1=High, 2=Medium, 3=Low):");
            if (!int.TryParse(priorityStr, out var priority) || priority < 1 || priority > 3)
                priority = 3;

            var labelsStr = Microsoft.VisualBasic.Interaction.InputBox("Labels (comma separated):");
            var labels = labelsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(l => l.Trim())
                                  .ToList();

            var project = data.Projects[ProjectsList.SelectedIndex];
            project.Tasks.Add(new TaskItem
            {
                Title = title,
                DueDate = dueDate,
                Priority = priority,
                Labels = labels
            });

            SaveData();
            RefreshTasksList(project);
        }

        private void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            RefreshTasksList(data.Projects[ProjectsList.SelectedIndex]);
        }

        // Drag & Drop logic
        private void TasksList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TasksList.SelectedItem != null)
                DragDrop.DoDragDrop(TasksList, TasksList.SelectedItem, DragDropEffects.Move);
        }

        private void TasksList_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void TasksList_Drop(object sender, DragEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            var sourceTaskStr = e.Data.GetData(typeof(string)) as string;
            if (sourceTaskStr == null) return;

            foreach (var project in data.Projects)
            {
                var task = project.Tasks.FirstOrDefault(t =>
                    sourceTaskStr.StartsWith(t.Title) &&
                    sourceTaskStr.Contains($"Labels: [{string.Join(", ", t.Labels)}]"));
                if (task != null)
                {
                    project.Tasks.Remove(task);
                    data.Projects[ProjectsList.SelectedIndex].Tasks.Add(task);
                    SaveData();
                    RefreshTasksList(data.Projects[ProjectsList.SelectedIndex]);
                    break;
                }
            }
        }

        // Filter logic
        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            var filter = FilterBox.Text.Trim().ToLower();
            var project = data.Projects[ProjectsList.SelectedIndex];

            var filtered = project.Tasks
                                  .Where(t => t.Labels.Any(l => l.ToLower() == filter))
                                  .Select(t =>
                                      $"{t.Title} (Due: {t.DueDate?.ToShortDateString() ?? "N/A"}, Priority: {t.Priority}, Labels: [{string.Join(", ", t.Labels)}])");

            TasksList.ItemsSource = filtered;
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterBox.Text = "";
            if (ProjectsList.SelectedIndex >= 0)
                RefreshTasksList(data.Projects[ProjectsList.SelectedIndex]);
        }

        private void FilterBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FilterBox.Text == "Enter label to filter")
                FilterBox.Text = "";
        }

        private void FilterBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilterBox.Text))
                FilterBox.Text = "Enter label to filter";
        }
    }
}
