using System.Windows;

namespace MyTodoist
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            ProjectsList.Items.Add("Personal");
            ProjectsList.Items.Add("Work");
            ProjectsList.Items.Add("Shopping");
        }

        private void ProjectsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedItem != null)
            {
                TasksHeader.Text = $"{ProjectsList.SelectedItem} - Tasks";
                TasksList.Items.Clear();
            }

            
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            string newProjectName = $"Project {ProjectsList.Items.Count + 1}";
            ProjectsList.Items.Add(newProjectName);
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedItem != null)
            {
                string newTaskName = $"Task {TasksList.Items.Count + 1}";
                TasksList.Items.Add(newTask);
            }
            else
            {
                MessageBox.Show("Select a project first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
