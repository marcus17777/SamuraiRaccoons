using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace testtest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Tasks
        {
            private String taskName;

            public String TaskName
            {
                get
                {
                    return taskName;
                }
                set
                {
                    taskName = value;
                }
            }

            public Tasks(String taskName)
            {
                this.taskName = taskName;
            }
        }

        public MainWindow() {
            InitializeComponent();
            taskList.Add(new Tasks("tere"));
            
            
        }

        ObservableCollection<Tasks> taskList = new ObservableCollection<Tasks>();
        
                

        public void on_Click(object sender, RoutedEventArgs e)
        {
            
            //button.Visibility = System.Windows.Visibility.Hidden;
            taskList.Add(new Tasks("tere"));
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
