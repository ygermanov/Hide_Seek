using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hide_Seek.Librarys;
using Microsoft.Win32;

namespace Hide_Seek
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private string text;
        private BitmapSource imageValue = null;
        private PictureManipulations.PixelColor[,] pixel;
        private PictureManipulations.PixelColor[,] pixelOut;

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        //revealing the keyword when checkbox is checked
        private void ShowPasswordCharsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            passwordBox.Visibility = System.Windows.Visibility.Collapsed;
            keywordReveal.Text = passwordBox.Password;
            keywordReveal.Visibility = System.Windows.Visibility.Visible;

            keywordReveal.Focus();
        }
        //hiding the keyword when checkbox is unchecked
        private void ShowPasswordCharsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            passwordBox.Visibility = System.Windows.Visibility.Visible;
            keywordReveal.Visibility = System.Windows.Visibility.Collapsed;

            passwordBox.Focus();
        }

        private void Encript_Click(object sender, RoutedEventArgs e)
        {
            string keyword = passwordBox.Password;
            TextManipulations test = new TextManipulations();
            text = textBox.Text;
            if (text == string.Empty)
            {
                MessageBox.Show(
                    "Missing text to encrypt.\nPlease add text!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (keyword == "")
            {
                MessageBox.Show(
                    "Keyword can`t be empty please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            string midResult = "";
            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                midResult += test.Cypher(keyword[index], text[i]);
                index++;
                if (index == keyword.Length) index = 0;
            }

            text = midResult;
            text = test.ConvertTo16Bit(text);
            textBox.Text = "";
            textResults.Text = text;
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            string keyword = passwordBox.Password;
            TextManipulations test = new TextManipulations();
            text = textResults.Text;
            if (text == string.Empty)
            {
                MessageBox.Show(
                    "Missing text to Decrypt.\nPlease add text!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (keyword == "")
            {
                MessageBox.Show(
                    "Keyword can`t be empty please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            string midResult = "";
            text = test.ConvertFrom16Bit(text);
            if(text == null)
            {
                MessageBox.Show(
                    "Invalid statement!!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                midResult += test.DeCypher(keyword[index], text[i]);
                index++;
                if (index == keyword.Length) index = 0;
            }
            text = midResult;
            textResults.Text = text;
        }
        private void AddToImage_Click(object sender, RoutedEventArgs e)
        {
            if (imageValue == null)
            {
                MessageBox.Show(
                    "No image loaded.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (textResults.Text == "")
            {
                MessageBox.Show(
                    "No text available !!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            pixel = new PictureManipulations.PixelColor[imageValue.PixelWidth, imageValue.PixelHeight];
            PictureManipulations.CopyPixels(
                imageValue, 
                pixel, 
                imageValue.PixelWidth * 4,
                0);
            pixel = PictureManipulations.ChangePixels(text, pixel);
            WriteableBitmap tempImage = new WriteableBitmap(
                imageValue.PixelWidth,
                imageValue.PixelHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            PictureManipulations.PutPixels(
                tempImage,
                PictureManipulations.RevertToSingleArray(pixel, 0),
                pixel.GetLength(0),
                pixel.GetLength(1));
            imageValue = BitmapFrame.Create(tempImage);
            imageBox.Source = imageValue;
            textResults.Text = "";
            MessageBox.Show(
                "Text added successfully.\nPlease save the Image !",
                "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
        }
        private void Extract_Click(object sender, RoutedEventArgs e)
        {
            if (imageValue == null)
            {
                MessageBox.Show(
                    "No image loaded.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            pixelOut = new PictureManipulations.PixelColor[imageValue.PixelWidth, imageValue.PixelHeight];
            PictureManipulations.CopyPixels(
                imageValue,
                pixelOut, 
                imageValue.PixelWidth * 4,
                0);
            text = PictureManipulations.ExtractText(pixelOut);
            textResults.Text = text;
        }

        private void Image_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpeg; *.png; *.bmp)|*.jpg; *.png; *.bmp"
            };

            if (open_dialog.ShowDialog() == true)
            {
                imageBox.Source = new BitmapImage(new Uri(open_dialog.FileName));
                imageValue = new FormatConvertedBitmap(
                    (BitmapImage)imageBox.Source,
                    PixelFormats.Bgra32,
                    null, 
                    0);
            }
        }

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog
            {
                Filter = "Text Files|*.txt"
            };

            if (open_dialog.ShowDialog() == true)
            {
                textBox.Text = File.ReadAllText(open_dialog.FileName);
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            try
            {
                encoder.Frames.Add(BitmapFrame.Create(imageValue));
            }
            catch(ArgumentNullException)
            {
                MessageBox.Show(
                    "No image!\nPlease load an Image!!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            

            SaveFileDialog save_dialog = new SaveFileDialog
            {
                Filter = "Png Image|*.png|Bitmap Image|*.bmp"
            };

            if (save_dialog.ShowDialog() == true)
            {
                using FileStream stream = new FileStream(save_dialog.FileName, FileMode.Create);
                encoder.Save(stream);
            }

            imageBox.Source = null;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Created by Y.Germanov\n" +
                "Using .NET 3.1\n" +
                "2021",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
