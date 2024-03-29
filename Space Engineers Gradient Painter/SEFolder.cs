using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace Space_Engineers_Gradient_Painter
{
    internal class SEFolder
    {
        private MainWindow mainWindow;

        public List<SEFolder> children = new List<SEFolder>();

        public string directoryPath { get; set; }

        public string directoryName { get; set; }

        public int level { get; set; }

        public SEFolder(string path, int level, MainWindow mainWindow)
        {
            directoryPath = path;
            directoryName = Path.GetFileName(path);
            this.level = level;
            this.mainWindow = mainWindow;
            string[] directories = Directory.GetDirectories(path);
            foreach (string subdirectory in directories)
            {
                if (subdirectory.EndsWith("_gradientEdit"))
                {
                    continue;
                }
                if (File.Exists(subdirectory + "\\bp.sbc"))
                {
                    SEBlueprint child2 = new SEBlueprint(subdirectory, level + 1, mainWindow);
                    children.Add(child2);
                    continue;
                }
                SEFolder child = new SEFolder(subdirectory, level + 1, mainWindow);
                if (child.children.Count != 0)
                {
                    children.Add(child);
                }
            }
        }

        public new virtual string ToString()
        {
            Console.WriteLine(level);
            string indent = "";
            for (int i = 0; i < level; i++)
            {
                indent += "  ";
            }
            string result = indent + directoryName + " = {\n";
            foreach (SEFolder child2 in children)
            {
                if (child2.GetType() == typeof(SEBlueprint))
                {
                    result += child2.ToString();
                }
            }
            foreach (SEFolder child in children)
            {
                if (child.GetType() == typeof(SEFolder))
                {
                    result += child.ToString();
                }
            }
            return result + indent + "}\n";
        }

        public virtual void CreateUI(StackPanel parent)
        {
            foreach (SEFolder child2 in children)
            {
                if (child2.GetType() == typeof(SEBlueprint))
                {
                    Button button = new Button();
                    button.Content = child2.directoryName;
                    double leftMargin2 = level * 20;
                    button.Margin = new Thickness(leftMargin2, 0.0, 0.0, 0.0);
                    button.HorizontalContentAlignment = HorizontalAlignment.Left;
                    button.HorizontalAlignment = HorizontalAlignment.Left;
                    button.MinWidth = 200.0;
                    button.FontSize = 15.0;
                    button.Click += delegate
                    {
                        Console.WriteLine("Editing blueprint: " + child2.directoryPath);
                        mainWindow.LoadBlueprint(child2.directoryPath);
                    };
                    parent.Children.Add(button);
                }
            }
            foreach (SEFolder child in children)
            {
                if (child.GetType() == typeof(SEFolder))
                {
                    Expander expander = new Expander();
                    expander.Header = child.directoryName;
                    expander.FontSize = 15.0;
                    expander.IsExpanded = false;
                    double leftMargin = level * 20;
                    expander.Margin = new Thickness(leftMargin, 0.0, 0.0, 0.0);
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Vertical;
                    expander.Content = stackPanel;
                    child.CreateUI(stackPanel);
                    parent.Children.Add(expander);
                }
            }
        }
    }
}
