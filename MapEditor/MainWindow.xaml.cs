using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    //Lưu lại tọa độ của Button trong ma trận bằng thuộc tính Button.Tag
    struct POSITION
    {
        public int x, y;
        public POSITION(int x,int y) { this.x = x; this.y = y; }
    }
    public partial class MainWindow : Window
    {
        Image selectedImage;//Tile được chọn để lát
        string openingFilePath = null;//Chứa đường dẫn tệp đang mở
        int selectedImageId;//Index của selectedImage trong tileSet
        List<Image> tileSet;//Danh sách các tiles
        int[,] tileMap;//Ma trận chứa index các tiles
        public MainWindow()
        {
            InitializeComponent();
        }
        //Vẽ grid map
        private void MakeGridMap(int m, int n, int width, int height)
        {
            ColumnDefinition[] columns = new ColumnDefinition[n];
            RowDefinition[] rows = new RowDefinition[m];
            GridTileMap.ShowGridLines = false;//Không hiện đường kẻ
            //Xóa sạch gridtilemap để tránh chồng lấp trong mỗi lần tạo
            GridTileMap.ColumnDefinitions.Clear();
            GridTileMap.RowDefinitions.Clear();
            GridTileMap.Children.Clear();
            //Định nghĩa các cột
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new ColumnDefinition();
                columns[i].Width = new GridLength(width);
                GridTileMap.ColumnDefinitions.Add(columns[i]);
            }
            //Định nghĩa các dòng
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new RowDefinition();
                rows[i].Height = new GridLength(height);
                GridTileMap.RowDefinitions.Add(rows[i]);
            }
            //Thêm các button để hiển thị tile lên grid
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    var image = new Image();
                    var value = tileMap[i, j];//lấy index từ tilemap
                    if (value != -1) 
                        image.Source = tileSet[value].Source; //Gán ảnh = ảnh có index tương ứng trong tileSet
                    else
                        image.Source = null; //Nếu bằng -1 thì không được gán tile
                    var button = new Button();
                    button.Template = FindResource("TileButton0") as ControlTemplate;
                    //Thông báo tọa đọ của button khi được rê chuột
                    button.MouseEnter += Button_MouseEnter;
                    button.MouseLeave += Button_MouseLeave;
                    button.Click += Button_Click1;
                    //Gán kích thước
                    button.Width = width;
                    button.Height = height;
                    button.Content = image;//Gán ảnh vào button
                    button.BorderThickness = new Thickness(0);//Không có viền
                    button.Padding = new Thickness(0.1);
                    button.Name = "TileMapButton_" + i + "_" + j;//Đặt tên cho button theo tọa độ tương ứng trong matrix
                    button.Tag = new POSITION(i, j);
                    //Add btn vào grid
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GridTileMap.Children.Add(button);
                }
            }
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
            //Lấy kích thước ma trận
            int m = tileMap.GetLength(0);
            int n = tileMap.GetLength(1);
            //Dòng đầu tiên
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
            //Ghi vào file
            File.WriteAllLines(pathToTileMapFile,lines);
        }

        public List<Image> LoadTileSet(string tileSetPath, int width, int height)
        {
            //Xóa sạch nội dung trong wrappanel
            WrapPanelTileSet.Children.Clear();
            
            ImagePreview.Source = null;
            List<Image> m_tileSet = new List<Image>();
            //Load bitmap chứa tileset từ file
            Bitmap inputBitmap = new Bitmap(tileSetPath);
            int bitmapWidth = inputBitmap.Width;
            int bitmapHeight = inputBitmap.Height;
            //Danh sách các tile
            List<Bitmap> tiles = new List<Bitmap>();
            int m, n;
            if (width <= 0 && height <= 0)
                return null;
            n = bitmapWidth / width;
            m = bitmapHeight / height;
            int count = 0;//biến đếm để tạo số thứ tự
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    //Chi tiết xem comment trong MakeGridMap
                    Bitmap tmp = new Bitmap(width, height);
                    Rectangle r = new Rectangle(j * width, i * height, width, height);
                    tmp = inputBitmap.Clone(r, inputBitmap.PixelFormat);
                    var image = new Image();
                    image.Source = BitmapToImageSource(tmp);
                    m_tileSet.Add(image);
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
            selectedImage = new Image();
            selectedImage.Source = m_tileSet[0].Source;
            selectedImageId = 0;
            return m_tileSet;
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
            //Lưu lại index và imagesource của tile được chọn
            selectedImage = new Image();
            int id = (int)((Button)sender).Tag - 1;//id bắt đầu từ 0
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
        //Chuyển kiểu Bitmap sang kiểu ImageSource
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
                //Load tileset
                tileSet = LoadTileSet(wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);
                try
                {
                    //Load tilemap
                    tileMap = LoadTileMap(wd.TileMapFilePath, out int m, out int n);
                    //Đưa lên màn hình
                    MakeGridMap(m, n, wd.CellWidth, wd.CellHeight);
                    //Lưu lại đường dẫn file đang mở;
                    openingFilePath = wd.TileMapFilePath;
                }
                catch(Exception)
                {
                    MessageBox.Show("Invalid file format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


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
            if (tileMap == null)//Nếu chưa có tilemap thì bỏ qua
                return;
            if (openingFilePath == null || openingFilePath == "")//Nếu không biết lưu vào file nào thì yêu cầu người dùng tạo mới
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
            //Tiến hành lưu
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
        //Tạo một ma trận có tất cả phần tử = -1
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
                //Tạo một tileset
                tileMap = MakeTileSet(wd.ImageFilePath, wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);
                //Load tileset lên cửa sổ
                tileSet = LoadTileSet(wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);
                //Load map tilemap lên cửa sổ
                MakeGridMap(tileMap.GetLength(0), tileMap.GetLength(1), wd.CellWidth, wd.CellHeight);
            }

        }

        private int[,] MakeTileSet(string inputImagePath, string outputImagePath, int width, int height)
        {
            //Đọc Bitmap lớn từ file
            Bitmap inputBitmap = new Bitmap(inputImagePath);
            int bitmapWidth = inputBitmap.Width;
            int bitmapHeight = inputBitmap.Height;

            List<Bitmap> tiles = new List<Bitmap>();
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
                    if (i == 3) System.Console.Out.Write(true);
                    Bitmap tmp = new Bitmap(width, height);
                    Rectangle r = new Rectangle(j * width, i * height, width, height);
                    tmp = inputBitmap.Clone(r, PixelFormat.Format32bppArgb); //Cắt bitmap
                    available = false;
                    for (int k = 0; k < tiles.Count; k++)
                        if (CompareBitmapsFast(tiles[k], tmp))//So sách để kiểm tra bitmap có sẵn trong list không
                        {
                            s[i,j] = k;
                            available = true;
                            break;
                        }
                    if (!available)//Nếu bitmap này không có sẵn trong list thì thêm vào
                    {
                        s[i,j] = last++;
                        tiles.Add(tmp);
                    }
                }
            }
            //Vẽ ra bitmap tileset
            Bitmap b = new Bitmap(width * tiles.Count, height);
            for (int i = 0; i < tiles.Count; i++)
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.DrawImage(tiles[i], width * i, 0);
                }
            }
            //Xuất ra file
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
