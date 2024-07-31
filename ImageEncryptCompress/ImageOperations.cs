using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Diagnostics.Debug;
using System.Diagnostics;

///Algorithms Project
///Intelligent Scissors
///

namespace ImageEncryptCompress
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }

    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    public struct my_seed
    {
        public long SEED;
        public char[] s;
        public int sz, TAB, pStart, pEnd, pTab;

        public my_seed(TextBox tabBox, TextBox seedBox)
        {
            TAB = int.Parse(tabBox.Text.ToString());
            string temp = seedBox.Text.ToString();
            s = new char[temp.Length];
            for (int i = 0; i < temp.Length; i++)
                s[i] = temp[i];
            long ret = 0;
            for (int i = 0; i < temp.Length; i++)
                ret = ret * 2 + (temp[i] - '0');
            SEED = ret;
            sz = s.Length;
            pStart = 0;
            pEnd = sz - 1;
            pTab = TAB;
        }

        public byte go_next(bool f)
        {
            if (f)
            {
                byte ret = 0;
                for (int i = 7; i >= 0; i--)
                {
                    long x = (SEED >> (sz - 1)) & 1L;
                    long y = SEED >> TAB & 1L;
                    SEED *= 2;
                    SEED += x ^ y;
                    ret += (byte)((x ^ y) << i);
                }

                return ret;
            }
            else
            {
                byte ret = (byte)(s[pTab] ^ s[pEnd]);
                pEnd = (pEnd - 1 + sz) % sz;
                pTab = (pTab - 1 + sz) % sz;
                pStart = (pStart - 1 + sz) % sz;
                s[pStart] = (char)ret;
                byte one = 1;
                int pos = (pStart + 1) % sz;
                ret = (byte)(ret | ((s[pos] & one) << 7));
                return ret;
            }
        }
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite,
                    original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb ||
                         original_bm.PixelFormat == PixelFormat.Format32bppRgb ||
                         original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }

                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }

                    p += nOffset;
                }

                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite,
                    ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }

                ImageBMP.UnlockBits(bmd);
            }

            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public class Node : IComparable<Node>
        {
            public int Left, Right, Leaf, Frq;

            public Node(int left, int right, int leaf, int frq)
            {
                this.Left = left;
                this.Right = right;
                this.Leaf = leaf;
                this.Frq = frq;
            }

            public int CompareTo(Node other)
            {
                return this.Frq.CompareTo(other.Frq);
            }
        }

        public static void CompressImage(RGBPixel[,] ImageMatrix, String fileName)
        {
            int n = GetHeight(ImageMatrix), m = GetWidth(ImageMatrix);
            int[][] frq = new int[3][];
            for (int i = 0; i < 3; i++)
                frq[i] = new int[1 << 8];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    frq[0][ImageMatrix[i, j].red]++;
                    frq[1][ImageMatrix[i, j].green]++;
                    frq[2][ImageMatrix[i, j].blue]++;
                }
            }

            Node[][] huffmanTree = new Node[3][];
            int[] sz = new int[3];
            int[][] newMask = new int[3][];
            int[][] szOfNewMask = new int[3][];
            for (int i = 0; i < 3; i++)
            {
                newMask[i] = new int[(1 << 8)];
                szOfNewMask[i] = new int[(1 << 8)];
            }

            for (int col = 0; col < 3; col++)
            {
                for (int i = 0; i < (1 << 8); i++)
                {
                    if (frq[col][i] > 0)
                        sz[col]++;
                }

                huffmanTree[col] = new Node[2 * sz[col] + 10];
                int idx = 0;
                for (int i = 0; i < (1 << 8); i++)
                {
                    if (frq[col][i] > 0)
                    {
                        huffmanTree[col][idx++] = new Node(-1, -1, i, frq[col][i]);
                    }
                }

                Array.Sort<Node>(huffmanTree[col], 0, idx);

                Queue<Tuple<int, int>>[] Qt = new Queue<Tuple<int, int>>[2];
                for (int i = 0; i < 2; i++)
                    Qt[i] = new Queue<Tuple<int, int>>();
                for (int i = 0; i < idx; i++)
                {
                    Qt[0].Enqueue(new Tuple<int, int>(i, huffmanTree[col][i].Frq));
                }

                while (Qt[0].Count + Qt[1].Count >= 2)
                {
                    Tuple<int, int>[] t = new Tuple<int, int>[2];
                    for (int i = 0; i < 2; i++)
                        t[i] = new Tuple<int, int>(-1, -1);
                    for (int _ = 0; _ < 2; _++)
                    {
                        int who = 1;
                        if (Qt[1].Count == 0 || (Qt[0].Count != 0 && Qt[0].First().Item2 < Qt[1].First().Item2))
                        {
                            who = 0;
                        }

                        for (int i = 0; i < 2; i++)
                            if (t[i].Item1 == -1)
                            {
                                t[i] = Qt[who].Dequeue();
                                break;
                            }
                    }

                    huffmanTree[col][idx] = new Node(t[0].Item1, t[1].Item1, -1, t[0].Item2 + t[1].Item2);
                    Qt[1].Enqueue(new Tuple<int, int>(idx, t[0].Item2 + t[1].Item2));
                    idx++;
                }

                Queue<Tuple<int, int, int>> Q = new Queue<Tuple<int, int, int>>();
                Q.Enqueue(new Tuple<int, int, int>(idx - 1, 0, 0));
                while (Q.Count != 0)
                {
                    Tuple<int, int, int> t = Q.Dequeue();
                    int u = t.Item1, curmsk = t.Item2, depth = t.Item3;
                    if (huffmanTree[col][u].Leaf != -1)
                    {
                        newMask[col][huffmanTree[col][u].Leaf] = curmsk;
                        szOfNewMask[col][huffmanTree[col][u].Leaf] = depth;
                        continue;
                    }

                    Q.Enqueue(new Tuple<int, int, int>(huffmanTree[col][u].Left, curmsk, depth + 1));
                    Q.Enqueue(new Tuple<int, int, int>(huffmanTree[col][u].Right, curmsk | (1 << depth), depth + 1));
                }
            }

            //MessageBox.Show(fileName);
            // orig msk, new msk,
            // 
            string filePath =
                $"E:\\Algorithms project\\{fileName}_Com.bin";
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    MessageBox.Show("File deleted successfully.", "Success", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting file: " + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            using (var stream = File.Open(filePath,
                       FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    writer.Write(n);
                    writer.Write(m);
                    for (int col = 0; col < 3; col++)
                    {
                        writer.Write(sz[col]);
                        for (int i = 0; i < sz[col]; i++)
                        {
                            writer.Write((byte)huffmanTree[col][i].Leaf);
                            writer.Write((byte)newMask[col][huffmanTree[col][i].Leaf]);
                            writer.Write((byte)szOfNewMask[col][huffmanTree[col][i].Leaf]);
                        }
                    }

                    byte b = 0;
                    int idx = 7;
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < m; j++)
                        {
                            for (int col = 0; col < 3; col++)
                            {
                                byte msk = 0;
                                if (col == 0)
                                    msk = ImageMatrix[i, j].red;
                                else if (col == 1)
                                    msk = ImageMatrix[i, j].green;
                                else msk = ImageMatrix[i, j].blue;
                                for (int k = 0; k < szOfNewMask[col][msk]; k++)
                                {
                                    if ((newMask[col][msk] & (1 << k)) > 0)
                                        b |= (byte)(1 << idx);
                                    idx--;
                                    if (idx == -1)
                                    {
                                        writer.Write(b);
                                        b = 0;
                                        idx = 7;
                                    }
                                }
                            }
                        }
                    }
                    if (idx != 7)
                        writer.Write(b);
                }
            }
        }

        public static int readBit(ref byte b, ref int idx, BinaryReader reader)
        {
            if (idx == -1)
            {
                b = reader.ReadByte();
                idx = 7;
            }

            return (b >> (idx--)) & 1;
        }


        public static RGBPixel[,] DecompressImage(String fileName)
        {
            List<Node>[] trie = new List<Node>[3];
            for (int i = 0; i < 3; i++)
            {
                trie[i] = new List<Node>();
                trie[i].Add(new Node(-1, -1, -1, 0));
            }

            RGBPixel[,] ImageMatrix;
            using (var stream = File.Open(
                       fileName,
                       FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    int n = reader.ReadInt32();
                    int m = reader.ReadInt32();
                    ImageMatrix = new RGBPixel[n, m];
                    for (int col = 0; col < 3; col++)
                    {
                        int sz = reader.ReadInt32();
                        for (int _ = 0; _ < sz; _++)
                        {
                            int oriMask = (int)reader.ReadByte();
                            int nwmask = (int)reader.ReadByte();
                            int szOfnwmsk = (int)reader.ReadByte();
                            int curNode = 0;
                            for (int c = 0; c < szOfnwmsk; c++)
                            {
                                int ch = (nwmask >> c) & 1;
                                if (ch == 0 && trie[col][curNode].Left == -1)
                                {
                                    trie[col][curNode].Left = trie[col].Count;
                                    trie[col].Add(new Node(-1, -1, -1, 0));
                                }

                                if (ch == 1 && trie[col][curNode].Right == -1)
                                {
                                    trie[col][curNode].Right = trie[col].Count;
                                    trie[col].Add(new Node(-1, -1, -1, 0));
                                }

                                if (ch == 0)
                                    curNode = trie[col][curNode].Left;
                                else curNode = trie[col][curNode].Right;
                            }

                            trie[col][curNode].Leaf = oriMask;
                        }
                    }
                    byte b = 0;
                    int idx = -1;
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < m; j++)
                        {
                            ImageMatrix[i, j] = new RGBPixel();
                            for (int col = 0; col < 3; col++)
                            {
                                int curNode = 0;
                                while (trie[col][curNode].Leaf == -1)
                                {
                                    int curBit = readBit(ref b, ref idx, reader);
                                    if (curBit == 0)
                                        curNode = trie[col][curNode].Left;
                                    else
                                        curNode = trie[col][curNode].Right;
                                }

                                if (col == 0)
                                    ImageMatrix[i, j].red = (byte)trie[col][curNode].Leaf;
                                else if (col == 1)
                                    ImageMatrix[i, j].green = (byte)trie[col][curNode].Leaf;
                                else
                                    ImageMatrix[i, j].blue = (byte)trie[col][curNode].Leaf;
                            }
                        }
                    }
                }
            }

            return ImageMatrix;
        }

        public static RGBPixel[,] toggle(int n, int m, RGBPixel[,] matrix, my_seed Seed, bool is_binary)
        {
            RGBPixel[,] nw = new RGBPixel[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    nw[i, j].red = (byte)(matrix[i, j].red ^ Seed.go_next(is_binary));
                    nw[i, j].green = (byte)(matrix[i, j].green ^ Seed.go_next(is_binary));
                    nw[i, j].blue = (byte)(matrix[i, j].blue ^ Seed.go_next(is_binary));
                }
            }

            return nw;
        }
    }
}