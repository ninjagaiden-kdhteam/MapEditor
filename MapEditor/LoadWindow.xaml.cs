using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        OpenFileDialog openFileDialog;
        public LoadWindow()
        {
            InitializeComponent();
        }
        public int CellHeight { set; get; }
        public int CellWidth { set; get; }
        public string TileSetFilePath { set; get; }
        public string TileMapFilePath { set; get; }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            CellHeight = int.Parse(TextBoxCellHeight.Text);
            CellWidth = int.Parse(TextBoxCellWidth.Text);
            TileSetFilePath = TextBoxTileSetPath.Text;
            TileMapFilePath = TextBoxTileMapPath.Text;
            DialogResult = true;
        }

        private void BrowseTileSetFile_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "PNG TileSet file |*.png";
            openFileDialog.FileName = TextBoxTileSetPath.Text;
            if (openFileDialog.ShowDialog() == true)
            {
                TextBoxTileSetPath.Text = openFileDialog.FileName;
            }
        }

        private void BrowseTileMapFile_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Text file |*.txt";
            openFileDialog.FileName = TextBoxTileMapPath.Text;
            if (openFileDialog.ShowDialog() == true)
            {
                TextBoxTileMapPath.Text = openFileDialog.FileName;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
