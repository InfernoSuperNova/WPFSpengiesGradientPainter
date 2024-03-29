using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
namespace Space_Engineers_Gradient_Painter
{
    public partial class MainWindow : Window
    {
        private string blueprintsPath = "";

        private SEFolder folder;

        public StackPanel shipList;

        public Frame shipEditor;

        public MainWindow()
        {
            if (Directory.Exists("C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\SpaceEngineers\\Blueprints"))
            {
                blueprintsPath = "C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\SpaceEngineers\\Blueprints";
            }
            folder = new SEFolder(blueprintsPath, 0, this);
            base.Loaded += MainWindow_Loaded;
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(folder.ToString());
            shipList = (StackPanel)FindName("ShipList");
            shipEditor = (Frame)FindName("ShipEditor");
            folder.CreateUI(shipList);
        }

        public void LoadBlueprint(string path)
        {
            ShipEditWindow shipEditWindow = new ShipEditWindow(path, this);
            shipList.Visibility = Visibility.Collapsed;
            shipEditor.Visibility = Visibility.Visible;
            shipEditor.Content = shipEditWindow;
        }

        public void ReturnToMainWindow()
        {
            shipList.Visibility = Visibility.Visible;
            shipEditor.Visibility = Visibility.Collapsed;
            shipEditor.Content = null;
        }

        private void ShipEditor_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward)
            {
                e.Cancel = true;
            }
        }
    }
}