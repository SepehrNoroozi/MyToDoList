using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyTodoist
{
    public partial class MainWindow : Window
    {
        private AppData appData = new AppData();
        private string dataFile = "data.json";
        private string archiveFile = "archive.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            UpdateProjectsList();
            SetPlaceholder(SearchBox, "Search tasks...");
            SetPlaceholder(FilterBox, "Filter by label...");
        }

        private void SetPlaceholder(TextBox box, string placeholder)
        {
            box.Text = placeholder;
            box.Foreground = System.Windows.Media.Brushes.Gray;
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search tasks...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.White;
            }
        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SetPlaceholder(SearchBox, "Search tasks...");
        }
        private void FilterBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FilterBox.Text == "Filter by label...")
            {
                FilterBox.Text = "";
                FilterBox.Foreground = System.Windows.Media.Brushes.White;
            }
        }
        private void FilterBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilterBox.Text))
                SetPlaceholder(FilterBox, "Filter by label...");
        }

        private void LoadData()
        {
            if (File.Exists(dataFile))
            {
                string json = File.ReadAllText(dataFile);
                appData = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            }
        }
        private void SaveData()
        {
            string json = JsonSerializer.Serialize(appData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dataFile, json);
        }
        private void SaveArchive(List<TaskItem> tasks)
        {
            string json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(archiveFile, json);
        }

        private void UpdateProjectsList()
        {
            ProjectsList.ItemsSource = null;
            ProjectsList.ItemsSource = appData.Projects;
        }
        private void UpdateTasksList(Project project)
        {
            TasksList.ItemsSource = null;
            TasksList.ItemsSource = project.Tasks;
        }

        private void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedItem is Project project)
                UpdateTasksList(project);
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Enter project name:", "Add Project");
            if (!string.IsNullOrWhiteSpace(name))
            {
                appData.Projects.Add(new Project { Name = name });
                SaveData();
                UpdateProjectsList();
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedItem is Project project)
            {
                string title = Microsoft.VisualBasic.Interaction.InputBox("Enter task title:", "Add Task");
                if (!string.IsNullOrWhiteSpace(title))
                {
                    project.Tasks.Add(new TaskItem
                    {
                        Title = title,
                        DueDate = DateTime.Today.AddDays(1),
                        Priority = 1,
                        Labels = new List<string>()
                    });
                    SaveData();
                    UpdateTasksList(project);
                    ShowToastNotification("Reminder set", $"Task '{title}' added with due date tomorrow.");
                }
            }
            else
            {
                MessageBox.Show("Select a project first!");
            }
        }

        private void TasksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem task)
            {
                if (MessageBox.Show($"Mark '{task.Title}' as done?", "Complete Task", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    task.IsCompleted = true;
                    SaveArchive(new List<TaskItem> { task });
                    if (ProjectsList.SelectedItem is Project proj)
                        proj.Tasks.Remove(task);
                    SaveData();
                    UpdateTasksList(ProjectsList.SelectedItem as Project);
                }
            }
        }

        private void ShowToastNotification(string title, string message)
        {
            // Replaced Toast with MessageBox for WPF compatibility without NuGet
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            File.Copy(dataFile, "export.json", true);
            MessageBox.Show("Data exported to export.json");
        }

        private void ImportData_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("export.json"))
            {
                MessageBox.Show("export.json not found!");
                return;
            }
            string json = File.ReadAllText("export.json");
            appData = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            SaveData();
            UpdateProjectsList();
        }

        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedItem is Project proj)
            {
                if (SortBox.SelectedIndex == 0)
                    proj.Tasks = proj.Tasks.OrderBy(t => t.DueDate).ToList();
                else if (SortBox.SelectedIndex == 1)
                    proj.Tasks = proj.Tasks.OrderByDescending(t => t.Priority).ToList();
                UpdateTasksList(proj);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProjectsList.SelectedItem is Project proj)
            {
                var query = SearchBox.Text;
                if (query != "Search tasks...")
                {
                    TasksList.ItemsSource = proj.Tasks.Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    UpdateTasksList(proj);
                }
            }
        }

        private void TasksList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TasksList.SelectedItem != null)
                DragDrop.DoDragDrop(TasksList, TasksList.SelectedItem, DragDropEffects.Move);
        }

        private void TasksList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(TaskItem)) is TaskItem task && ProjectsList.SelectedItem is Project proj)
            {
                foreach (var p in appData.Projects)
                    p.Tasks.Remove(task);
                proj.Tasks.Add(task);
                SaveData();
                UpdateTasksList(proj);
            }
        }
    }
}
