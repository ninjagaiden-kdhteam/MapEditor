using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    struct POSITION
    {
        public int x, y;
        public POSITION(int x,int y) { this.x = x; this.y = y; }
    }
    public partial class MainWindow : Window
    {
        OpenFileDialog openFileDialog;
        Image selectedImage;
        string openingFilePath = null;
        int selectedImageId;
        List<Image> tileSet;
        int[,] tileMap;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MakeGridMap(int m, int n, int width, int height)
        {
            #region Map
            //Bitmap
            //Grid
            ColumnDefinition[] columns = new ColumnDefinition[n];
            RowDefinition[] rows = new RowDefinition[m];
            GridTileMap.ShowGridLines = false;
            GridTileMap.ColumnDefinitions.Clear();
            GridTileMap.RowDefinitions.Clear();
            GridTileMap.Children.Clear();
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new ColumnDefinition();
                columns[i].Width = new GridLength(width);
                GridTileMap.ColumnDefinitions.Add(columns[i]);
            }
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new RowDefinition();
                rows[i].Height = new GridLength(height);
                GridTileMap.RowDefinitions.Add(rows[i]);
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    var image = new Image();
                    var value = tileMap[i, j];
                    if (value != -1)
                        image.Source = tileSet[value].Source;
                    else
                        image.Source = null;
                    var button = new Button();
                    button.Template = FindResource("TileButton0") as ControlTemplate;
                    button.MouseEnter += Button_MouseEnter;
                    button.MouseLeave += Button_MouseLeave;
                    button.Width = width;
                    button.Height = height;
                    button.Content = image;
                    button.BorderThickness = new Thickness(0);
                    button.Padding = new Thickness(0.1);
                    button.Name = "TileMapButton_" + i + "_" + j;
                    button.Click += Button_Click1;
                    button.Tag = new POSITION(i, j);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GridTileMap.Children.Add(button);
                }
            }

            #endregion
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextStatus.Text = null;
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int x = ((POSITION)(sender as Button).Tag).x;
            int y = ((POSITION)(sender as Button).Tag).y;
            TextStatus.Text = "(" + x + "," + y + ") = " + tileMap[x,y];       
        }

        public int[,] LoadTileMap(string pathToTileMapFile, out int m, out int n)
        {
            //Load tệp tilemap
            string[] lines = File.ReadAllLines(pathToTileMapFile);
            //Xác định kích thước map
            string[] tmp = lines[0].Split(' ');
            int tileSetCount = int.Parse(tmp[0]);
            m = int.Parse(tmp[1]);
            n = int.Parse(tmp[2]);
            int[,] result = new int[m, n];
            string[] tmp2 = new string[m];
            //Lưu phần tử vào mảng
            for (int i = 1; i < m+1; i++)
            {
                tmp2 = lines[i].Split(' ');
                for (int j = 0; j < n; j++)
                {
                    result[i - 1, j] = int.Parse(tmp2[j]);
                }
            }
            return result;
        }

        public void SaveTileMap(string pathToTileMapFile, int[,] tileMap, int count)
        {
            string[] lines = new string[tileMap.Length + 1];
            //Dòng đầu tiên
            int m = tileMap.GetLength(0);
            int n = tileMap.GetLength(1);
            lines[0] = count.ToString() + ' ' + m + ' ' + n;
            //Ma trận
            string tmp;
            for(int i = 1; i < m + 1;i++)
            {
                tmp = "";
                for(int j = 0; j < n; j++)
                {
                    tmp += tileMap[i - 1, j].ToString() + ' ';
                }
                lines[i] = tmp;
            }
            File.WriteAllLines(pathToTileMapFile,lines);
        }

        public List<Image> LoadTileSet(string tileSetPath, int width, int height)
        {
            WrapPanelTileSet.Children.Clear();
            selectedImage = null;
            ImagePreview.Source = null;
            List<Image> tileSet = new List<Image>();
            Bitmap inputBitmap = new Bitmap(tileSetPath);
            int bitmapWidth = inputBitmap.Width;
            int bitmapHeight = inputBitmap.Height;
            //Tiles list
            List<Bitmap> tiles = new List<Bitmap>();
            int m, n;
            if (width <= 0 && height <= 0)
                return null;
            n = bitmapWidth / width;
            m = bitmapHeight / height;
            int count = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Bitmap tmp = new Bitmap(width, height);
                    Rectangle r = new Rectangle(j * width, i * height, width, height);
                    tmp = inputBitmap.Clone(r, inputBitmap.PixelFormat);
                    var image = new System.Windows.Controls.Image();
                    image.Source = BitmapToImageSource(tmp);
                    tileSet.Add(image);
                    var button = new Button();
                    button.MouseEnter += TileSet_Button_MouseEnter;
                    button.MouseLeave += TileSet_Button_MouseLeave;
                    button.Width = width;
                    button.Height = height;
                    button.Content = image;
                    WrapPanelTileSet.Children.Add(button);
                    count++;
                    button.Name = "TileButton" + count;
                    button.Click += Button_Click;
                    button.Tag = count;
                }
            }

            return tileSet;
        }

        private void TileSet_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextStatus.Text = null;
        }

        private void TileSet_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int x = (int)(sender as Button).Tag - 1;
            TextStatus.Text = "Tile " + x;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Select " + ((Button)sender).Name);
            selectedImage = new Image();
            int id = (int)((Button)sender).Tag - 1;
            selectedImage.Source = tileSet[id].Source;
            selectedImageId = id;
            ImagePreview.Source = selectedImage.Source;
        }


        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Click " + ((Button)sender).Name);
            if(selectedImage != null)
            {
                //Đổi image trên editor
                ((Image)((Button)sender).Content).Source = selectedImage.Source;
                POSITION p = (POSITION)((Button)sender).Tag;
                //Đổi giá trị trong tilemap
                tileMap[p.x, p.y] = selectedImageId;
                Console.WriteLine("Change to " + selectedImageId);
            }


        }
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void MenuItem_LoadTileMap_Click(object sender, RoutedEventArgs e)
        {
            LoadWindow wd = new LoadWindow();
            wd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wd.ShowDialog() == true)
            {
                //Lưu lại đường dẫn file đang mở;
                openingFilePath = wd.TileMapFilePath;
                //Load tileset
                tileSet = LoadTileSet(wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);
                //Load tilemap
                tileMap = LoadTileMap(wd.TileMapFilePath, out int m, out int n);
                //Đưa lên màn hình
                MakeGridMap(m, n, wd.CellWidth, wd.CellHeight);
            }
        }
        //Fix bug mất một phần cửa sổ khi maximized
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this.WindowState == WindowState.Maximized)
            {
                // if maximised, set window border to window resize border + fixed frame border so that the window content is not chopped off
                this.BorderThickness = new Thickness(SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.FixedFrameVerticalBorderWidth,
                    SystemParameters.WindowResizeBorderThickness.Top + SystemParameters.FixedFrameHorizontalBorderHeight,
                    SystemParameters.WindowResizeBorderThickness.Right + SystemParameters.FixedFrameVerticalBorderWidth,
                    SystemParameters.WindowResizeBorderThickness.Bottom + SystemParameters.FixedFrameHorizontalBorderHeight);
            }
            else
            {
                this.BorderThickness = new Thickness();
            }
        }

        private void MenuItemZoomIn_Click(object sender, RoutedEventArgs e)
        {
            sourceGridScaleTransform.ScaleX *= 1.1;
            sourceGridScaleTransform.ScaleY *= 1.1;
        }

        private void MenuItemZoomOut_Click(object sender, RoutedEventArgs e)
        {
            sourceGridScaleTransform.ScaleX /= 1.1;
            sourceGridScaleTransform.ScaleY /= 1.1;
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (tileMap == null)
                return;
            if (openingFilePath == null || openingFilePath == "")//Nếu không biết lưu vào file nào
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text File|*.txt";
                saveFileDialog.AddExtension = true;
                if (saveFileDialog.ShowDialog() == true)
                {
                    openingFilePath = saveFileDialog.FileName;
                }
                else
                    return;
            }
            SaveTileMap(openingFilePath, tileMap, tileSet.Count);
            TextStatus.Text = "Saved";
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            CreateWindow wd = new CreateWindow();
            wd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if(wd.ShowDialog() == true)
            {
                openingFilePath = null;
                //Tạo ma trận -1
                tileMap = MakeBlankTileMap(wd.NumberOfRows, wd.NumberOfColumns);
                //Load tileset
                tileSet = LoadTileSet(wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);
                //Đưa lên màn hình
                MakeGridMap(wd.NumberOfRows, wd.NumberOfColumns, wd.CellWidth, wd.CellHeight);

            }
        }
        private int[,] MakeBlankTileMap(int m, int n)
        {
            int[,] result = new int[m, n];
            //Lưu phần tử vào mảng
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = -1;
                }
            }
            return result;
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemZoomReset_Click(object sender, RoutedEventArgs e)
        {
            sourceGridScaleTransform.ScaleX = 1;
            sourceGridScaleTransform.ScaleY = 1;
        }

        private void MenuItemErase_Click(object sender, RoutedEventArgs e)
        {
            if (selectedImage != null)
            {
                selectedImage.Source = null;
                selectedImageId = -1;
                ImagePreview.Source = selectedImage.Source;
            }

        }

        private void MenuItemMakeTileSet_Click(object sender, RoutedEventArgs e)
        {
            MakeTileSetWindow wd = new MakeTileSetWindow();
            wd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wd.ShowDialog() == true)
            {
                tileMap = MakeTileSet(wd.ImageFilePath, wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);
                tileSet = LoadTileSet(wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);
                //SaveTileMap(wd.TileSetFilePath, tileMap, tileSet.Count);
                MakeGridMap(tileMap.GetLength(0), tileMap.GetLength(1), wd.CellWidth, wd.CellHeight);
            }

        }

        private int[,] MakeTileSet(string inputImagePath, string outputImagePath, int width, int height)
        {
            //Read Bitmap
            Bitmap inputBitmap = new Bitmap(inputImagePath);
            int bitmapWidth = inputBitmap.Width;
            int bitmapHeight = inputBitmap.Height;
            //Tiles list
            List<Bitmap> tiles = new List<Bitmap>();
            //Crop bitmap
            int m, n;
            n = bitmapWidth / width;
            m = bitmapHeight / height;
            int[,] s = new int[m,n];
            bool available;
            int last = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Bitmap tmp = new Bitmap(width, height);
                    Rectangle r = new Rectangle(j * width, i * height, width, height);
                    tmp = inputBitmap.Clone(r, inputBitmap.PixelFormat);
                    available = false;
                    for (int k = 0; k < tiles.Count; k++)
                        if (CompareBitmapsFast(tiles[k], tmp))
                        {
                            s[i,j] = k;
                            available = true;
                            break;
                        }
                    if (!available)
                    {
                        s[i,j] = last++;
                        tiles.Add(tmp);
                    }
                }
            }
            Bitmap b = new Bitmap(width * tiles.Count, height);
            for (int i = 0; i < tiles.Count; i++)
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.DrawImage(tiles[i], width * i, 0);
                }
            }
            b.Save(outputImagePath);
            return s;
        }
        //Source: http://csharpexamples.com/c-fast-bitmap-compare/
        public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return false;
            if (object.Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (System.Drawing.Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }
    }
}
