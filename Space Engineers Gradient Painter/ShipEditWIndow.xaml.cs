using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using VRageMath;

namespace Space_Engineers_Gradient_Painter
{
    public partial class ShipEditWindow : Page
    {
        private MainWindow MainWindow;

        private string path;

        private List<List<List<Block>>> blocks = new List<List<List<Block>>>();

        private XmlDocument bp;

        private XmlNodeList cubeGridList;

        private XmlNode cubeBlocks;

        private MatrixI transformationMatrix = new MatrixI(Base6Directions.Direction.Up, Base6Directions.Direction.Forward);

        private Base6Directions.Direction dirA = Base6Directions.Direction.Up;

        private Base6Directions.Direction dirB;

        private double minX = double.MaxValue;

        private double minY = double.MaxValue;

        private double minZ = double.MaxValue;

        private double maxX = double.MinValue;

        private double maxY = double.MinValue;

        private double maxZ = double.MinValue;

        private double lengthX;

        private double lengthY;

        private double lengthZ;

        private double maxLength;

        private int spacing;

        private int currentGrid;

        private Vector3I lengths = new Vector3I(0, 0, 0);

        private double xZeroValue;

        private double yZeroValue;

        private double zZeroValue;

        private Vector3 originalSelectedColour;

        private DispatcherTimer timer;

        public static readonly float SATURATION_DELTA = 0.8f;

        public static readonly float VALUE_DELTA = 0.55f;

        public static readonly float VALUE_COLORIZE_DELTA = 0.1f;

        private Vector2I centerpoint;


        public ShipEditWindow(string editPath, MainWindow mainWindow)
        {
            InitializeComponent();
            InitializeTimer();
            base.Loaded += ShipEditWindow_Loaded;
            MainWindow = mainWindow;
            path = editPath;
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000.0);
            timer.Tick += Tick;
            timer.Start();
        }

        private void ShipEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string tempPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\temp";
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            File.Copy(path + "\\bp.sbc", tempPath + "\\bp.sbc", overwrite: true);
            bp = new XmlDocument();
            if (!IsXmlFileValid(tempPath + "\\bp.sbc"))
            {
                MessageBox.Show("Invalid XML file (may be an issue on older blueprints. As a workaround, you can resave the blueprint.)");
                ReturnBtn_Click(sender, e);
                return;
            }
            bp.Load(tempPath + "\\bp.sbc");
            cubeGridList = bp.GetElementsByTagName("CubeGrid");
            cubeBlocks = cubeGridList[0].SelectSingleNode("CubeBlocks");
            GridIdContainer.Text = currentGrid + "/" + (cubeGridList.Count - 1);
            FindMinMaxCoords(cubeBlocks);
            CalculateShipDimensions();
            Console.WriteLine("lengthx: " + lengthX);
            Console.WriteLine("lengthy: " + lengthY);
            Console.WriteLine("lengthz: " + lengthZ);
            Console.WriteLine("minx: " + minX);
            Console.WriteLine("miny: " + minY);
            Console.WriteLine("minz: " + minZ);
            Console.WriteLine("maxx: " + maxX);
            Console.WriteLine("maxy: " + maxY);
            Console.WriteLine("maxz: " + maxZ);
            double screenWidth = EditorWindow.ActualWidth;
            spacing = (int)(screenWidth / maxLength);
            transformationMatrix = new MatrixI(dirB, dirA);
            lengths = new Vector3I((int)lengthX, (int)lengthY, (int)lengthZ);
            lengths = Vector3I.TransformNormal(lengths, ref transformationMatrix);
            UpdateShipDisplay();
        }

        public bool IsXmlFileValid(string filePath)
        {
            try
            {
                new XmlDocument().Load(filePath);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        private void Tick(object sender, EventArgs e)
        {
        }

        private void UpdateShipDisplay()
        {
            CreateDisplayGrid();
            CreateBlockGrid(spacing);
        }

        private void CreateDisplayGrid()
        {
            InitializeBlockMatrix();
            UpdateBlockMatrixFromXml();
            Console.WriteLine("Block matrix created");
        }

        private void InitializeBlockMatrix()
        {
            blocks = new List<List<List<Block>>>();
            for (int x = 0; x <= Math.Abs(lengths.X); x++)
            {
                List<List<Block>> xList = new List<List<Block>>();
                for (int y = 0; y <= Math.Abs(lengths.Y); y++)
                {
                    List<Block> yList = new List<Block>();
                    for (int z = 0; z <= Math.Abs(lengths.Z); z++)
                    {
                        bool isReal = false;
                        Vector3I vector3I = new Vector3I(x, y, z);
                        Block block = new Block(colour: new Vector3(0f, 0f, 0f), position: vector3I, doesExist: isReal);
                        yList.Add(block);
                    }
                    xList.Add(yList);
                }
                blocks.Add(xList);
            }
        }

        private void UpdateBlockMatrixFromXml()
        {
            foreach (XmlNode cubeBlock in cubeBlocks)
            {
                XmlNode positionNode = cubeBlock.SelectSingleNode("Min");
                if (positionNode == null || positionNode.Attributes == null)
                {
                    continue;
                }
                int xPosBlock = (int)((double)int.Parse(positionNode.Attributes["x"].Value) + xZeroValue);
                int yPosBlock = (int)((double)int.Parse(positionNode.Attributes["y"].Value) + yZeroValue);
                int zPosBlock = (int)((double)int.Parse(positionNode.Attributes["z"].Value) + zZeroValue);
                Vector3I pos = new Vector3I(xPosBlock, yPosBlock, zPosBlock);
                Vector3I.TransformNormal(ref pos, ref transformationMatrix, out pos);
                if (pos.X < 0)
                {
                    pos.X += Math.Abs(lengths.X);
                }
                if (pos.Y < 0)
                {
                    pos.Y += Math.Abs(lengths.Y);
                }
                if (pos.Z < 0)
                {
                    pos.Z += Math.Abs(lengths.Z);
                }
                XmlNode colourNode = cubeBlock.SelectSingleNode("ColorMaskHSV");
                if (colourNode != null)
                {
                    float h = float.Parse(colourNode.Attributes["x"].Value);
                    float s = float.Parse(colourNode.Attributes["y"].Value);
                    float v = float.Parse(colourNode.Attributes["z"].Value);
                    Vector3 colour = new Vector3(h, s, v);
                    if (IsValidBlockPosition(pos))
                    {
                        blocks[pos.X][pos.Y][pos.Z].doesExist = true;
                        blocks[pos.X][pos.Y][pos.Z].colour = colour;
                        blocks[pos.X][pos.Y][pos.Z].rawColour = HSVToRGB(HSVOffsetToHSV(colour));
                        blocks[pos.X][pos.Y][pos.Z].block = cubeBlock;
                    }
                }
            }
        }

        private bool IsValidBlockPosition(Vector3I pos)
        {
            if (pos.X >= 0 && pos.X < blocks.Count && pos.Y >= 0 && pos.Y < blocks[pos.X].Count && pos.Z >= 0)
            {
                return pos.Z < blocks[pos.X][pos.Y].Count;
            }
            return false;
        }

        private void CreateBlockGrid(int size)
        {
            gridContainer.Children.Clear();
            gridContainer.RowDefinitions.Clear();
            gridContainer.ColumnDefinitions.Clear();
            int rowCount = Math.Abs(lengths.X);
            int columnCount = Math.Abs(lengths.Y);
            for (int c = 0; c < columnCount; c++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = GridLength.Auto;
                gridContainer.ColumnDefinitions.Add(column);
            }
            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                gridContainer.RowDefinitions.Add(row);
                for (int j = 0; j < columnCount; j++)
                {
                    Button button = new Button();
                    button.Width = size;
                    button.Height = size;
                    button.Background = GetTopmostBlockColour(i, j);
                    button.BorderThickness = new Thickness(0.0);
                    button.Click += delegate
                    {
                        GenericBlockBtnHandler(Grid.GetColumn(button), Grid.GetRow(button), button.Background);
                    };
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    gridContainer.Children.Add(button);
                }
            }
        }

        private SolidColorBrush GetTopmostBlockColour(int thisX, int thisY)
        {
            Vector3 vectorColour = new Vector3(0f, 0f, 0f);
            for (int z = blocks[thisX][thisY].Count - 1; z >= 0; z--)
            {
                if (blocks[thisX][thisY][z].doesExist)
                {
                    vectorColour = blocks[thisX][thisY][z].colour;
                    vectorColour = HSVOffsetToHSV(vectorColour);
                    return new SolidColorBrush(HSVToRGB(vectorColour));
                }
            }
            return new SolidColorBrush(HSVToRGB(vectorColour));
        }

        private System.Windows.Media.Color HSVToRGB(Vector3 hsv)
        {
            float h = hsv.X * 360f;
            float s = hsv.Y;
            float v = hsv.Z;
            int hi = (int)Math.Floor(h / 60f) % 6;
            float f = h / 60f - (float)Math.Floor(h / 60f);
            float p = v * (1f - s);
            float q = v * (1f - f * s);
            float t = v * (1f - (1f - f) * s);
            float r;
            float g;
            float b;
            switch (hi)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }
            return System.Windows.Media.Color.FromArgb(byte.MaxValue, (byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f));
        }

        private Vector3 RGBToHSV(System.Windows.Media.Color rgb)
        {
            float r = (float)(int)rgb.R / 255f;
            float g = (float)(int)rgb.G / 255f;
            float b = (float)(int)rgb.B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            float h = 0f;
            float s = 0f;
            float v = max;
            if (max > 0f)
            {
                s = delta / max;
            }
            if (max == min)
            {
                h = 0f;
            }
            else
            {
                if (max == r)
                {
                    h = (g - b) / delta + ((g < b) ? 6f : 0f);
                }
                else if (max == g)
                {
                    h = (b - r) / delta + 2f;
                }
                else if (max == b)
                {
                    h = (r - g) / delta + 4f;
                }
                h /= 6f;
            }
            return new Vector3(h, s, v);
        }

        private void FindMinMaxCoords(XmlNode cubeBlocks)
        {
            foreach (XmlNode cubeBlock in cubeBlocks)
            {
                XmlNode positionNode = cubeBlock.SelectSingleNode("Min");
                if (positionNode != null && positionNode.Attributes != null)
                {
                    XmlAttribute xAttr = positionNode.Attributes["x"];
                    XmlAttribute yAttr = positionNode.Attributes["y"];
                    XmlAttribute zAttr = positionNode.Attributes["z"];
                    if (xAttr != null && yAttr != null && zAttr != null && double.TryParse(xAttr.Value, out var x) && double.TryParse(yAttr.Value, out var y) && double.TryParse(zAttr.Value, out var z))
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        minZ = Math.Min(minZ, z);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                        maxZ = Math.Max(maxZ, z);
                    }
                }
            }
            Console.WriteLine($"Min coordinates: ({minX}, {minY}, {minZ})");
            Console.WriteLine($"Max coordinates: ({maxX}, {maxY}, {maxZ})");
        }

        private void CalculateShipDimensions()
        {
            lengthX = maxX - minX;
            lengthY = maxY - minY;
            lengthZ = maxZ - minZ;
            xZeroValue = Math.Abs(minX);
            yZeroValue = Math.Abs(minY);
            zZeroValue = Math.Abs(minZ);
            maxLength = Math.Max(lengthX, Math.Max(lengthY, lengthZ));
        }

        private void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Returning to main menu");
            MainWindow.ReturnToMainWindow();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string savePath = path + "_gradientEdit";
            if (Directory.Exists(savePath))
            {
                Directory.Delete(savePath, recursive: true);
            }
            Directory.CreateDirectory(savePath);
            bp.Save(savePath + "\\bp.sbc");
            MessageBox.Show("Blueprint saved to " + savePath);
        }

        public static Vector3 HSVToHSVOffset(Vector3 hsv)
        {
            float y = hsv.Y - SATURATION_DELTA;
            float z = hsv.Z - VALUE_DELTA + VALUE_COLORIZE_DELTA;
            return new Vector3(hsv.X, y, z);
        }

        public static Vector3 HSVOffsetToHSV(Vector3 hsvOffset)
        {
            return new Vector3(hsvOffset.X, Math.Clamp(hsvOffset.Y + SATURATION_DELTA, 0f, 1f), Math.Clamp(hsvOffset.Z + VALUE_DELTA - VALUE_COLORIZE_DELTA, 0f, 1f));
        }

        private void RotateBtn_Click(object sender, RoutedEventArgs e)
        {
            double screenWidth = EditorWindow.ActualWidth;
            spacing = (int)(screenWidth / maxLength);
            dirA++;
            if ((int)dirA > 5)
            {
                dirB++;
                dirA = Base6Directions.Direction.Forward;
                if ((int)dirB > 5)
                {
                    dirB = Base6Directions.Direction.Forward;
                }
            }
            transformationMatrix = new MatrixI(dirB, dirA);
            lengths = new Vector3I((int)lengthX, (int)lengthY, (int)lengthZ);
            lengths = Vector3I.TransformNormal(lengths, ref transformationMatrix);
            while (lengths.X == 0 || lengths.Y == 0 || lengths.Z == 0)
            {
                dirA++;
                if ((int)dirA > 5)
                {
                    dirB++;
                    dirA = Base6Directions.Direction.Forward;
                    if ((int)dirB > 5)
                    {
                        dirB = Base6Directions.Direction.Forward;
                    }
                }
                transformationMatrix = new MatrixI(dirB, dirA);
                lengths = new Vector3I((int)lengthX, (int)lengthY, (int)lengthZ);
                lengths = Vector3I.TransformNormal(lengths, ref transformationMatrix);
            }
            UpdateShipDisplay();
        }

        private void CycleGridBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Cycling grid");
            currentGrid = (currentGrid + 1) % cubeGridList.Count;
            GridIdContainer.Text = currentGrid + "/" + (cubeGridList.Count - 1);
            cubeBlocks = cubeGridList[currentGrid].SelectSingleNode("CubeBlocks");
            Console.WriteLine("currentGrid: " + currentGrid);
            transformationMatrix = new MatrixI(dirB, dirA);
            lengths = new Vector3I((int)lengthX, (int)lengthY, (int)lengthZ);
            lengths = Vector3I.TransformNormal(lengths, ref transformationMatrix);
            FindMinMaxCoords(cubeBlocks);
            CalculateShipDimensions();
            double screenWidth = EditorWindow.ActualWidth;
            spacing = (int)(screenWidth / maxLength);
            transformationMatrix = new MatrixI(dirB, dirA);
            lengths = new Vector3I((int)lengthX, (int)lengthY, (int)lengthZ);
            lengths = Vector3I.TransformNormal(lengths, ref transformationMatrix);
            UpdateShipDisplay();
        }

        private void GenericBlockBtnHandler(int x, int y, Brush background)
        {
            Console.WriteLine($"Block clicked at ({x}, {y})");
            ColourBox.Background = background;
            System.Windows.Media.Color color = ((SolidColorBrush)background).Color;
            centerpoint = new Vector2I(x, y);
            ReplaceColourWithRainbow(color);
        }

        private void ReplaceColourWithRainbow(System.Windows.Media.Color background)
        {
            for (int i = 0; i < gridContainer.RowDefinitions.Count; i++)
            {
                int j;
                for (j = 0; j < gridContainer.ColumnDefinitions.Count; j++)
                {
                    Button button = gridContainer.Children.OfType<Button>().FirstOrDefault((Button e) => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);
                    if (button == null)
                    {
                        continue;
                    }
                    System.Windows.Media.Color color = ((SolidColorBrush)button.Background).Color;
                    int distanceToCenterColumn = Math.Abs(i - centerpoint.Y);
                    Vector3 hsvColour = new Vector3(z: 1f - (float)distanceToCenterColumn / (float)(gridContainer.RowDefinitions.Count / 2), x: (float)j / (float)gridContainer.ColumnDefinitions.Count, y: 1f);
                    if (color == background)
                    {
                        button.Background = new SolidColorBrush(HSVToRGB(hsvColour));
                    }
                    Vector3 encodedColour = HSVToHSVOffset(hsvColour);
                    foreach (Block block in blocks[i][j])
                    {
                        if (block.doesExist)
                        {
                            XmlNode colourNode = block.block.SelectSingleNode("ColorMaskHSV");
                            if (colourNode != null && block.rawColour == background)
                            {
                                colourNode.Attributes["x"].Value = encodedColour.X.ToString();
                                colourNode.Attributes["y"].Value = encodedColour.Y.ToString();
                                colourNode.Attributes["z"].Value = encodedColour.Z.ToString();
                            }
                        }
                    }
                }
            }
            Console.WriteLine("paint created");
            bp.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\temp\\bp.sbc");
        }
    }
}
