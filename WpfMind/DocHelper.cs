using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using WpfMind.Properties;

namespace Mind
{
    class DocHelper
    {
        static string open()
        {
            string fileName;
            using (OpenFileDialog OpenFD = new OpenFileDialog())     //实例化一个 OpenFileDialog 的对象
            {
                //定义打开的默认文件夹位置
                //OpenFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                OpenFD.ShowDialog();                  //显示打开本地文件的窗体
                fileName = OpenFD.FileName;       //把文件路径及名称赋给 fileName
            }
            return fileName;
        }

        public static string readFromTemplate()
        {
            string tempFolderPath = Path.GetTempPath();
            string fileName = tempFolderPath + "document.zip";
            FileStream f = new FileStream(fileName, FileMode.Create);
            f.Write(Resources.中心主题, 0, Resources.中心主题.Length);
            f.Close();
            return read(fileName);
        }

        public static string read(string fileName = "")
        {
            if (fileName.Length == 0) fileName = open();
            if (fileName.Length == 0) return readFromTemplate();
            using (FileStream zipToOpen = new FileStream(fileName, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry readmeEntry = archive.GetEntry("content.json");
                    using (StreamReader reader = new StreamReader(readmeEntry.Open()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static void update(string fileName, string content)
        {
            using (FileStream zipToOpen = new FileStream(fileName, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.GetEntry("content.json").Delete();
                    ZipArchiveEntry readmeEntry = archive.CreateEntry("content.json");
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.Write(content);
                    }
                }
            }
        }

        public static void create(string content)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "xmind files (*.xmind)|*.xmind|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    myStream.Write(Resources.中心主题, 0, Resources.中心主题.Length);

                    //Assembly assm = Assembly.GetExecutingAssembly();

                    //Stream istr = assm.GetFile("中心主题.xmind");
                    //Stream istr = assm.GetManifestResourceStream("中心主题");
                    //istr.CopyTo(myStream);
                    myStream.Close();
                }
            }

            using (FileStream zipToOpen = new FileStream(saveFileDialog1.FileName, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.GetEntry("content.json").Delete();
                    ZipArchiveEntry readmeEntry = archive.CreateEntry("content.json");
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.Write(content);
                    }
                }
            }
        }

    }
}
