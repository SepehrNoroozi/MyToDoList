using System.Windows;
using System.Windows.Controls;

namespace MyTodoist
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // نمونه داده
            ListLeft.Items.Add("Item 1");
            ListLeft.Items.Add("Item 2");
            ListLeft.Items.Add("Item 3");
        }

        private void MoveToRight_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListLeft.SelectedItems;
            foreach (var item in selectedItems)
            {
                ListRight.Items.Add(item);
            }

            while (ListLeft.SelectedItems.Count > 0)
            {
                ListLeft.Items.Remove(ListLeft.SelectedItems[0]);
            }
        }

        private void MoveToLeft_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListRight.SelectedItems;
            foreach (var item in selectedItems)
            {
                ListLeft.Items.Add(item);
            }

            while (ListRight.SelectedItems.Count > 0)
            {
                ListRight.Items.Remove(ListRight.SelectedItems[0]);
            }
        }
    }
}
