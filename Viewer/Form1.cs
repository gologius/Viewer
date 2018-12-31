using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO; //add
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs; //add 
using SharpCompress.Archives; //add

namespace Viewer
{
    public partial class Form1 : Form
    {
        IArchive archive = null; //圧縮ファイルの実体
        List<IArchiveEntry> imgs = null; //画像ファイル群   
        int lookPage = 0; //現在閲覧しているページ

        public Form1()
        {
            InitializeComponent();
        }

        private void DirOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //「フォルダを開く」ダイアログの設定
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;  // フォルダーを開く設定にする
            dialog.EnsureReadOnly = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.DefaultDirectory = Application.StartupPath;

            //フォルダを開く
            var Result = dialog.ShowDialog();
            if (Result == CommonFileDialogResult.Ok)
            {
                string select_path = dialog.FileName;
                this.Text = select_path; //タイトル名変更
                updateFileList(select_path); //ファイルリスト更新
            }
        }

        //ファイルリストを更新する
        private void updateFileList(string path)
        {
            var fullpaths = System.IO.Directory.GetFiles(path, "*");
            foreach (string filename in fullpaths)
            {
                listView1.Items.Add(filename);
            }
        }

        //ファイルリストの項目をクリックした時
        private void listView1_Click(object sender, EventArgs e)
        {
            //項目が一つもない場合
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }

            //選択している最初の行の、最初の列の値を取得
            ListViewItem item = listView1.SelectedItems[0];
            string path = item.SubItems[0].Text;

            bool result = registerImage(path);
            if (result)
            {
                lookPage = 0;
                showImage(lookPage);
            }
        }

        //指定された圧縮ファイル内の画像を表示する
        private bool registerImage(string path)
        {
            Console.WriteLine(path);
            //初期化
            if (archive != null)
            {
                archive.Dispose();
                archive = null;
            }

            //圧縮ファイルから画像ファイルのみ取り出す
            try
            {
                archive = ArchiveFactory.Open(path);
                var entries = archive.Entries.Where(e =>
                    e.IsDirectory == false && (
                    Path.GetExtension(e.Key).Equals(".jpg") ||
                    Path.GetExtension(e.Key).Equals(".jpeg") ||
                    Path.GetExtension(e.Key).Equals(".png") ||
                    Path.GetExtension(e.Key).Equals(".bmp")));

                imgs = entries.ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show(path + " " + e.ToString(), "ファイル展開エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (archive != null)
                {
                    archive.Dispose();
                }
                archive = null;
                return false;
            }

            //ソート
            imgs.Sort((a, b) => { return a.Key.CompareTo(b.Key); });

            return true;
        }

        //指定ページの画像を表示する
        private bool showImage(int index)
        {
            if (imgs.Count() == 0)
            {
                return false;
            }

            //圧縮ファイル内のファイル指定
            var entry = imgs[index];
            try
            {
                //ファイルを読み込みビューワーにセット
                pictureBox1.Image = Image.FromStream(entry.OpenEntryStream());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "正常な画像ファイルではありません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
        
        //矢印キーでのページめくり
        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (archive == null)
            {
                return;
            }

            if (e.KeyCode == Keys.Left)
            {
                lookPage--;
                if (lookPage < 0)
                {
                    lookPage = imgs.Count() - 1;
                }
                showImage(lookPage);
            }
            else if (e.KeyCode == Keys.Right)
            {
                lookPage++;
                if (lookPage >= imgs.Count())
                {
                    lookPage = 0;
                }
                showImage(lookPage);
            }
        }
    }
}
