using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Diagnostics.Debug;
using System.Diagnostics;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix, encrypted;
        String name;
        string OpenedFilePath;
        my_seed Seed;


        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                OpenedFilePath = openFileDialog1.FileName;
                var temp = openFileDialog1.FileName.ToString().Split('\\');
                var temp2 = temp[temp.Length - 1].Split('.');
                name = temp2[0];
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }

            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        static void SaveRGBPhotoAsImage(RGBPixel[,] buffer, int height, int width, string imagePath)
        {
            // Create a new bitmap with the specified dimensions
            Bitmap bitmap = new Bitmap(width, height);

            // Set pixel values based on RGB data from the buffer
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    RGBPixel pixel = buffer[y, x];
                    Color color = Color.FromArgb(pixel.red, pixel.green, pixel.blue);
                    bitmap.SetPixel(x, y, color);
                }
            }

            // Save the bitmap to an image file
            bitmap.Save(imagePath, ImageFormat.Bmp);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int height = int.Parse(ImageOperations.GetHeight(encrypted).ToString());
            int width = int.Parse(ImageOperations.GetWidth(encrypted).ToString());
            ;
            SaveRGBPhotoAsImage(encrypted, height, width,
                $"E:\\[1] Image Encryption and Compression\\Sample Test\\SampleCases_Encryption\\Saved\\{name}_Encrypted.bmp");
            MessageBox.Show("Saved successfully !!");
        }


        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            Decrypt_Click(sender, e);
        }

        private void Decrypt_Click(object sender, EventArgs e)
        {
            if (!binary.Checked && !characters.Checked)
            {
                MessageBox.Show("Please select a seed type.", "Seed Type Selection", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (ImageMatrix == null || ImageMatrix.Length == 0)
            {
                MessageBox.Show("Please select a Photo", "Select a photo", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (binary.Checked)
            {
                foreach (var ch in seedBox.Text)
                {
                    if (ch == '1' || ch == '0')
                        continue;
                    MessageBox.Show("Please enter  a valid binary seed", "valid seed", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            if (int.Parse(tabBox.Text.ToString()) >= seed.ToString().Length)
            {
                MessageBox.Show("Please enter  a valid Tab position", "valid Tab position", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Seed = new my_seed(tabBox, seedBox);

            int n = int.Parse(ImageOperations.GetHeight(ImageMatrix).ToString());
            int m = int.Parse(ImageOperations.GetWidth(ImageMatrix).ToString());
            encrypted = ImageOperations.toggle(n, m, ImageMatrix, Seed, binary.Checked);
            ImageOperations.DisplayImage(encrypted, pictureBox2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImageOperations.CompressImage(ImageMatrix, name);
            ImageOperations.DisplayImage(ImageOperations.DecompressImage($"E:\\Algorithms project\\{name}_Com.bin"), pictureBox2);
        }

        private void Decompress_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                OpenedFilePath = openFileDialog1.FileName;
                ImageOperations.DisplayImage(ImageOperations.DecompressImage(OpenedFilePath), pictureBox2);
            }

        }

        private void enc_comp_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            // Decrypt_Click(sender, e);
            Seed = new my_seed(tabBox, seedBox);
            int n = int.Parse(ImageOperations.GetHeight(ImageMatrix).ToString());
            int m = int.Parse(ImageOperations.GetWidth(ImageMatrix).ToString());
            encrypted = ImageOperations.toggle(n, m, ImageMatrix, Seed, binary.Checked);

            ImageOperations.CompressImage(encrypted, name);
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            long seconds = elapsedTime.Seconds;
            // Output the elapsed time
            MessageBox.Show($"Elapsed time: {seconds} , {seconds % 60}");
            //
            stopwatch = new Stopwatch();
            stopwatch.Start();
            encrypted = ImageOperations.DecompressImage($"E:\\Algorithms project\\{name}_Com.bin");
            n = int.Parse(ImageOperations.GetHeight(encrypted).ToString());
            m = int.Parse(ImageOperations.GetWidth(encrypted).ToString());
            encrypted = ImageOperations.toggle(n, m, encrypted, Seed, binary.Checked);
            elapsedTime = stopwatch.Elapsed;
            seconds = elapsedTime.Seconds;
            MessageBox.Show($"Elapsed time: {seconds} , {seconds % 60}");
            ImageOperations.DisplayImage(encrypted, pictureBox2);
        }
    }
}