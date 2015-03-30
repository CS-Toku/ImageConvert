using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageConvert
{

    public partial class Form1 : Form
    {
        public static string[] type = { ".jpeg", ".jpg", ".png", ".bmp", ".gif" };
        public Dictionary<string, System.Drawing.Imaging.ImageFormat> ImageType;
        public string SavePath;
        public System.Threading.Thread ConvThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //リストビュー初期化
            listView1.View = View.Details;
            listView1.Items.Clear();
            listView1.Columns.Clear();

            //項目1を追加
            ColumnHeader ColumnH1 = new ColumnHeader();
            ColumnH1.Text = "Title";
            ColumnH1.Width = 100;
            listView1.Columns.Add(ColumnH1);

            //項目2を追加
            ColumnHeader ColumnH2 = new ColumnHeader();
            ColumnH2.Text = "Width";
            ColumnH2.Width = 150;
            listView1.Columns.Add(ColumnH2);

            //項目3を追加
            ColumnHeader ColumnH3 = new ColumnHeader();
            ColumnH3.Text = "Height";
            ColumnH3.Width = 100;
            listView1.Columns.Add(ColumnH3);

            numericUpDown1.Minimum = 1;
            numericUpDown2.Minimum = 1;
            numericUpDown1.Maximum = 10000;
            numericUpDown2.Maximum = 10000;

            listView1.CheckBoxes = true;
            listView1.FullRowSelect = true;

            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "画像ファイル|*.bmp;*.gif;*.png;*.jpeg;*.jpg";

            ImageType = new Dictionary<string, System.Drawing.Imaging.ImageFormat>();
            comboBox1.Items.Add("JPG");
            ImageType["JPG"] = System.Drawing.Imaging.ImageFormat.Jpeg;
            comboBox1.Items.Add("PNG");
            ImageType["PNG"] = System.Drawing.Imaging.ImageFormat.Png;
            comboBox1.Items.Add("BMP");
            ImageType["BMP"] = System.Drawing.Imaging.ImageFormat.Bmp;
            comboBox1.Items.Add("GIF");
            ImageType["GIF"] = System.Drawing.Imaging.ImageFormat.Gif;

            comboBox1.SelectedIndex = 0;

            label4.Text = System.IO.Directory.GetCurrentDirectory() + @"\output";

        }


        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // 選択されたファイル名を格納
                foreach (string path in openFileDialog1.FileNames)
                {
                    if (Array.IndexOf(type, Path.GetExtension(path).ToLower()) >= 0)
                    {
                        listView1.Items.Add(new ImageRow(path));
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // 選択されたファイル名を格納
                string[] files = System.IO.Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                foreach (string file in files)
                {
                    if (Array.IndexOf(type, Path.GetExtension(file).ToLower()) >= 0)
                    {
                        listView1.Items.Add(new ImageRow(file));
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ConvThread == null)
            {
                ConvThread =
                    new System.Threading.Thread(
                    new System.Threading.ThreadStart(conv_Method));
                ConvThread.IsBackground = true;
                ConvThread.Start();
                button2.Text = "Abort";
            }
            else if (ConvThread.IsAlive == false)
            {
                ConvThread =
                    new System.Threading.Thread(
                    new System.Threading.ThreadStart(conv_Method));
                ConvThread.IsBackground = true;
                ConvThread.Start();
                button2.Text = "Abort";
            }
            else if (ConvThread.IsAlive == true)
            {
                ConvThread.Abort();
                ConvThread.Join();
                button2.Text = "Convert";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                listView1.Items.Remove(item);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                label4.Text = folderBrowserDialog1.SelectedPath;
            }
        }


        delegate string GetFileTypeDelegate();
        private string GetFileType()
        {
            return comboBox1.Text;
        }

        delegate void ProgressBarInitDelegate(int min, int max);
        private void ProgressBarInit(int min, int max)
        {
            progressBar1.Minimum = min;
            progressBar1.Maximum = max;
            progressBar1.Value = 0;
        }

        delegate ImageRow GetListItemDelegate(int index);
        private ImageRow GetListItem(int index)
        {
            return (ImageRow)listView1.Items[index];
        }

        delegate bool GetStatusDelegate(ImageRow ir);
        private bool GetStatus(ImageRow ir)
        {
            return ir.Checked;
        }

        delegate void SetStatusDelegate(ImageRow ir, bool b);
        private void SetStatus(ImageRow ir, bool b)
        {
            ir.Checked = b;
        }

        delegate void SetProgressBarValueDelegate(int num);
        private void SetProgressBarValue(int num)
        {
            progressBar1.Value = num;
        }

        delegate void SetButtonTextDelegate(string str);
        private void SetButtonText(string str)
        {
            button2.Text = str;
        }

        private void conv_Method()
        {
            string res = "Complete!!";
            try
            {
                int size = listView1.Items.Count;
                int width = (int)numericUpDown1.Value;
                int height = (int)numericUpDown2.Value;
                string imagetype_str = (string)Invoke(new GetFileTypeDelegate(GetFileType));
                System.Drawing.Imaging.ImageFormat imagetype = ImageType[imagetype_str];
                string ext = "." + imagetype_str.ToLower();
                Invoke(new ProgressBarInitDelegate(ProgressBarInit), new Object[] { 0, size });

                for (int i = 0; i < size; i++)
                {
                    ImageRow item = (ImageRow)Invoke(new GetListItemDelegate(GetListItem), new Object[] { i });
                    bool Checked = (bool)Invoke(new GetStatusDelegate(GetStatus), new Object[] { item });
                    if (Checked == true)
                    {
                        Bitmap bmp = new Bitmap(item.PathName);
                        Bitmap rbmp = new Bitmap(width, height);

                        Graphics g = Graphics.FromImage(rbmp);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bmp, 0, 0, width, height);
                        g.Dispose();

                        string filename = System.IO.Path.GetFileNameWithoutExtension(item.PathName);
                        rbmp.Save(label4.Text + @"\" + filename + ext, imagetype);
                        Invoke(new SetStatusDelegate(SetStatus), new Object[] { item, false });
                    }
                    Invoke(new SetProgressBarValueDelegate(SetProgressBarValue), new Object[] { i + 1 });
                }
            }
            catch (System.Threading.ThreadAbortException e)
            {
                res = "Abort.";
            }
            finally
            {
                Invoke(new SetButtonTextDelegate(SetButtonText), new Object[] { "Convert" });
                MessageBox.Show(res, "");
            }

        }

    }



    public class ImageRow : ListViewItem
    {
        public string PathName;

        public ImageRow(string path)
            : base(Path.GetFileName(path))
        {
            Checked = true;
            PathName = path;
            Image im = Image.FromFile(path);

            SubItems.Add(im.Width.ToString());
            SubItems.Add(im.Height.ToString());
        }
    }

}
