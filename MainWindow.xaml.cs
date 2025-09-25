using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

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
                TasksList.Items.Add(task.Title);
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
            string newProject = $"Project {appData.Projects.Count + 1}";
            appData.Projects.Add(new Project { Name = newProject });
            SaveData();
            RefreshProjectsList();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedIndex >= 0)
            {
                var project = appData.Projects[ProjectsList.SelectedIndex];
                string newTask = $"Task {project.Tasks.Count + 1}";
                project.Tasks.Add(new TaskItem { Title = newTask });
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
