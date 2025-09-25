using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

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
            CheckDueTasks();
        }

        private void LoadData()
        {
            if (File.Exists(filePath))
                data = JsonSerializer.Deserialize<AppData>(File.ReadAllText(filePath)) ?? new AppData();
        }

        private void SaveData()
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void RefreshProjectsList()
        {
            ProjectsList.ItemsSource = null;
            ProjectsList.ItemsSource = data.Projects.Select(p => p.Name);
        }

        private void RefreshTasksList(Project project)
        {
            TasksList.ItemsSource = null;
            TasksList.ItemsSource = project.Tasks.Select(TaskDisplay);
        }

        private string TaskDisplay(TaskItem t) =>
            $"{t.Title} (Due: {t.DueDate?.ToShortDateString() ?? "N/A"}, Priority: {t.Priority}, Labels: [{string.Join(", ", t.Labels)}])";

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

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            var name = Microsoft.VisualBasic.Interaction.InputBox("Edit project name:", data.Projects[ProjectsList.SelectedIndex].Name);
            if (!string.IsNullOrWhiteSpace(name))
            {
                data.Projects[ProjectsList.SelectedIndex].Name = name;
                SaveData();
                RefreshProjectsList();
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            data.Projects.RemoveAt(ProjectsList.SelectedIndex);
            SaveData();
            RefreshProjectsList();
            TasksList.ItemsSource = null;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;

            var title = Microsoft.VisualBasic.Interaction.InputBox("Task title:");
            if (string.IsNullOrWhiteSpace(title)) return;

            var dueDateStr = Microsoft.VisualBasic.Interaction.InputBox("Due date (yyyy-mm-dd):");
            DateTime? dueDate = null;
            if (DateTime.TryParse(dueDateStr, out var date))
                dueDate = date;

            var priorityStr = Microsoft.VisualBasic.Interaction.InputBox("Priority (1=High, 2=Medium, 3=Low):");
            if (!int.TryParse(priorityStr, out var priority)) priority = 3;

            var labelsStr = Microsoft.VisualBasic.Interaction.InputBox("Labels (comma separated):");
            var labels = labelsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();

            data.Projects[ProjectsList.SelectedIndex].Tasks.Add(new TaskItem
            {
                Title = title,
                DueDate = dueDate,
                Priority = priority,
                Labels = labels
            });

            SaveData();
            RefreshTasksList(data.Projects[ProjectsList.SelectedIndex]);
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0 || TasksList.SelectedIndex < 0) return;
            var project = data.Projects[ProjectsList.SelectedIndex];
            var task = project.Tasks[TasksList.SelectedIndex];

            var title = Microsoft.VisualBasic.Interaction.InputBox("Edit task title:", task.Title);
            var dueStr = Microsoft.VisualBasic.Interaction.InputBox("Edit due date:", task.DueDate?.ToShortDateString() ?? "");
            DateTime? due = null;
            if (DateTime.TryParse(dueStr, out var date)) due = date;

            var prioStr = Microsoft.VisualBasic.Interaction.InputBox("Edit priority:", task.Priority.ToString());
            if (!int.TryParse(prioStr, out var prio)) prio = task.Priority;

            var labelsStr = Microsoft.VisualBasic.Interaction.InputBox("Edit labels:", string.Join(", ", task.Labels));
            var labels = labelsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();

            task.Title = title;
            task.DueDate = due;
            task.Priority = prio;
            task.Labels = labels;

            SaveData();
            RefreshTasksList(project);
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0 || TasksList.SelectedIndex < 0) return;
            var project = data.Projects[ProjectsList.SelectedIndex];
            project.Tasks.RemoveAt(TasksList.SelectedIndex);
            SaveData();
            RefreshTasksList(project);
        }

        private void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            RefreshTasksList(data.Projects[ProjectsList.SelectedIndex]);
        }

        // Drag & Drop
        private void TasksList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TasksList.SelectedItem != null)
                DragDrop.DoDragDrop(TasksList, TasksList.SelectedItem, DragDropEffects.Move);
        }

        private void TasksList_DragOver(object sender, DragEventArgs e) => e.Effects = DragDropEffects.Move;

        private void TasksList_Drop(object sender, DragEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            var sourceTaskStr = e.Data.GetData(typeof(string)) as string;
            if (sourceTaskStr == null) return;

            foreach (var project in data.Projects)
            {
                var task = project.Tasks.FirstOrDefault(t => TaskDisplay(t) == sourceTaskStr);
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

        // Filter
        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            var filter = FilterBox.Text.Trim().ToLower();
            var project = data.Projects[ProjectsList.SelectedIndex];
            TasksList.ItemsSource = project.Tasks
                .Where(t => t.Labels.Any(l => l.ToLower() == filter))
                .Select(TaskDisplay);
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

        // Sort
        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedIndex < 0) return;
            var project = data.Projects[ProjectsList.SelectedIndex];
            var selected = (SortCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

            var sorted = selected switch
            {
                "Sort by Due Date ↑" => project.Tasks.OrderBy(t => t.DueDate ?? DateTime.MaxValue),
                "Sort by Due Date ↓" => project.Tasks.OrderByDescending(t => t.DueDate ?? DateTime.MinValue),
                "Sort by Priority ↑" => project.Tasks.OrderBy(t => t.Priority),
                "Sort by Priority ↓" => project.Tasks.OrderByDescending(t => t.Priority),
                _ => project.Tasks
            };

            TasksList.ItemsSource = sorted.Select(TaskDisplay);
        }

        // Backup
        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON Files (*.json)|*.json" };
            if (dlg.ShowDialog() == true)
                File.Copy(filePath, dlg.FileName, true);
        }

        private void ImportData_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON Files (*.json)|*.json" };
            if (dlg.ShowDialog() == true)
            {
                File.Copy(dlg.FileName, filePath, true);
                LoadData();
                RefreshProjectsList();
                TasksList.ItemsSource = null;
            }
        }

        // Notification
        private void CheckDueTasks()
        {
            var soonTasks = data.Projects
                .SelectMany(p => p.Tasks)
                .Where(t => t.DueDate.HasValue && (t.DueDate.Value - DateTime.Now).TotalDays <= 1)
                .ToList();

            if (soonTasks.Any())
            {
                MessageBox.Show(
                    "Upcoming tasks:\n" + string.Join("\n", soonTasks.Select(t => $"{t.Title} - {t.DueDate?.ToShortDateString()}")),
                    "Reminder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
