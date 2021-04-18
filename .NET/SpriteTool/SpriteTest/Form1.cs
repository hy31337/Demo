using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpriteTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //sprite json文件
            string text = ReadFile(textBox3.Text);
            JObject obj = JObject.Parse(text);
            JToken item = null;
            //将json转为对象
            List<Param> paramlist = new List<Param>();
            for (int i = 0; i < obj.Count; i++)
            {
                if (item == null)
                {
                    item = obj.First;
                }
                else
                {
                    item = item.Next;
                }
                Param p = new Param();
                p.name = item.Path.Replace("/", "-").Replace(":", "&").Replace("'", "").Replace("\\", "");//.Substring(2, item.Path.Length - 4)
                p.x = (int)item.First["x"];
                p.y = (int)item.First["y"];
                p.width = (int)item.First["width"];
                p.height = (int)item.First["height"];
                paramlist.Add(p);
            }

            using (Bitmap map = (Bitmap)Image.FromFile(textBox3.Text.Contains(".png") ? textBox3.Text : textBox3.Text + @"\sprite.png"))
            {

                using (Bitmap editMap = new Bitmap(map, map.Width, map.Height))
                {
                    foreach (var itemp in paramlist)
                    {
                        //保存图片的画布
                        Bitmap itemMap = new Bitmap(itemp.width, itemp.height);
                        for (int i = 0; i < itemp.width; i++)
                        {
                            for (int j = 0; j < itemp.height; j++)
                            {
                                //获取像素
                                Color color = editMap.GetPixel(itemp.x + i, itemp.y + j);
                                itemMap.SetPixel(i, j, color);
                            }
                        }
                        //保存
                        string savepath = System.Environment.CurrentDirectory + @"\spriteicon\" + itemp.name + ".png";
                        if (!Directory.Exists(savepath)) Directory.CreateDirectory(savepath);
                        itemMap.Save(savepath);
                    }
                }
            }
        }
        private static string ReadFile(string path)
        {
            StreamReader sr = new StreamReader(path + @"\sprite.json", Encoding.UTF8);
            string line;
            string jsonobj = "";
            while ((line = sr.ReadLine()) != null)
            {
                jsonobj = jsonobj + line.ToString();
            }
            return jsonobj;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DirectoryInfo folder = new DirectoryInfo(System.Environment.CurrentDirectory);
            List<string> filenames = new List<string>();
            int addnum = Convert.ToInt32(textBox2.Text);
            foreach (var NextFolder in folder.GetFiles("*.png"))
            {
                if (NextFolder.Name.Contains(txt_IcoName.Text))
                {
                    filenames.Add(NextFolder.Name);
                }
            }
            foreach (var item in filenames)
            {
                using (Bitmap map = (Bitmap)Image.FromFile(System.Environment.CurrentDirectory + "/" + item))
                {
                    using (Bitmap editMap = new Bitmap(map.Width, map.Height + addnum))
                    {
                        int centernum = map.Height / 2;
                        for (int i = 0; i < map.Height; i++)
                        {
                            for (int j = 0; j < map.Width; j++)
                            {
                                //获取像素
                                Color color = map.GetPixel(j, i);
                                if (i == centernum)
                                {
                                    editMap.SetPixel(j, i, color);
                                    if (addnum > 0)
                                    {
                                        for (int m = 0; m < addnum; m++)
                                        {
                                            editMap.SetPixel(j, i + m + 1, color);
                                        }
                                    }
                                }
                                else if (i < centernum)
                                {
                                    editMap.SetPixel(j, i, color);
                                }
                                else
                                {
                                    editMap.SetPixel(j, i + addnum, color);
                                }
                            }
                        }
                        //保存
                        string savepath = System.Environment.CurrentDirectory + @"\result\" + item;
                        editMap.Save(savepath);
                    }
                }
            }
        }
        /// <summary>
        /// 合成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            string strPath = textBox3.Text;//System.Environment.CurrentDirectory
            DirectoryInfo folder = new DirectoryInfo(strPath);
            List<Param> paramlist = new List<Param>();
            foreach (var NextFolder in folder.GetFiles("*.png"))
            {
                using (Bitmap map = (Bitmap)Image.FromFile(strPath + "/" + NextFolder.Name))
                {
                    Param p = new Param();
                    p.name = NextFolder.Name.Replace(".png", "");
                    p.width = map.Width;
                    p.height = map.Height;
                    paramlist.Add(p);
                }
            }
            //图片默认宽度为255，
            int widthnum = int.Parse(txt_WD.Text);
            paramlist = paramlist.OrderBy(m => m.name).OrderBy(m => m.height).ToList();
            //一行一行的图片集合
            List<List<Param>> rowparams = new List<List<Param>>();
            List<Param> paramnowlist = new List<Param>();
            int countnum = 0;
            for (int i = 0; i < paramlist.Count; i++)
            {
                countnum += paramlist[i].width;
                if (countnum > widthnum)
                {
                    i = i - 1;
                    countnum = 0;
                    rowparams.Add(paramnowlist);
                    paramnowlist = new List<Param>();
                }
                else
                {
                    paramnowlist.Add(paramlist[i]);
                }
                if (i == paramlist.Count - 1)
                {
                    rowparams.Add(paramnowlist);
                    break;
                }
            }
            //计算应有的高度
            int allheight = 0;
            foreach (var item in rowparams)
            {
                allheight += item.Select(m => m.height).Max();
            }

            StringBuilder strsCss = new StringBuilder();
            string spritejson = "{";
            //开始画大图
            using (Bitmap editMap = new Bitmap(widthnum, allheight))
            {
                //保存起始高度
                int heighttemp = 0;
                for (int i = 0; i < rowparams.Count; i++)
                {
                    int tempwidthnum = 0;
                    for (int j = 0; j < rowparams[i].Count; j++)
                    {
                        using (Bitmap map = (Bitmap)Image.FromFile(strPath + "/" + rowparams[i][j].name + ".png"))
                        {
                            //循环小图片
                            for (int x = 0; x < map.Width; x++)
                            {
                                for (int y = 0; y < map.Height; y++)
                                {
                                    //获取像素
                                    Color color = map.GetPixel(x, y);
                                    editMap.SetPixel(x + tempwidthnum, y + heighttemp, color);
                                }
                            }
                        }

                        strsCss.AppendFormat(".{0}", rowparams[i][j].name);
                        strsCss.Append("{");
                        strsCss.AppendFormat(" background-position: -{0}px -{1}px; width: {2}px; height: {3}px;", tempwidthnum, heighttemp, rowparams[i][j].width, rowparams[i][j].height);
                        strsCss.Append("}\r\n");

                        spritejson += "\"" + rowparams[i][j].name + "\":{\"x\":";//.Replace("-", "/").Replace("&", ":") 
                        spritejson += tempwidthnum + ",\"y\":" + heighttemp + ",\"width\":" + rowparams[i][j].width;
                        spritejson += ",\"height\":" + rowparams[i][j].height + ",\"pixelRatio\":1,\"sdf\":false},";
                        //增加宽度
                        tempwidthnum += rowparams[i][j].width;
                    }
                    heighttemp += rowparams[i].Select(m => m.height).Max();
                }

                //保存到上一级路径
                strPath = strPath.Substring(0, strPath.LastIndexOf(@"\"));

                //保存大图
                string savepath = strPath + @"\" + txt_spriteName.Text + ".png";
                if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);
                editMap.Save(savepath);
            }
            spritejson = spritejson.TrimEnd(',');
            spritejson += "}";
            //写入文件
            using (StreamWriter fw = new StreamWriter(strPath + @"\" + txt_spriteName.Text + ".json"))
            {
                fw.WriteLine(spritejson);
            }

            //写入文件
            using (StreamWriter fw = new StreamWriter(strPath + @"\" + txt_spriteName.Text + ".css"))
            {
                fw.WriteLine(strsCss.ToString());
            }
        }
        /// <summary>
        /// lable增加宽度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            DirectoryInfo folder = new DirectoryInfo(System.Environment.CurrentDirectory);
            List<string> filenames = new List<string>();
            int addnum = Convert.ToInt32(textBox2.Text);
            foreach (var NextFolder in folder.GetFiles("*.png"))
            {
                if (NextFolder.Name.Contains(txt_IcoName.Text))
                {
                    filenames.Add(NextFolder.Name);
                }
            }
            foreach (var item in filenames)
            {
                using (Bitmap map = (Bitmap)Image.FromFile(System.Environment.CurrentDirectory + "/" + item))
                {
                    using (Bitmap editMap = new Bitmap(map.Width + addnum, map.Height))
                    {
                        int centernum = map.Width / 2;
                        for (int i = 0; i < map.Width; i++)
                        {
                            for (int j = 0; j < map.Height; j++)
                            {
                                //获取像素
                                Color color = map.GetPixel(i, j);
                                if (i == centernum)
                                {
                                    editMap.SetPixel(i, j, color);
                                    if (addnum > 0)
                                    {
                                        for (int m = 0; m < addnum; m++)
                                        {
                                            editMap.SetPixel(i + m + 1, j, color);
                                        }
                                    }
                                }
                                else if (i < centernum)
                                {
                                    editMap.SetPixel(i, j, color);
                                }
                                else
                                {
                                    editMap.SetPixel(i + addnum, j, color);
                                }
                            }
                        }
                        //保存
                        string savepath = System.Environment.CurrentDirectory + @"\result\" + item;
                        editMap.Save(savepath);
                    }
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = System.Environment.CurrentDirectory + "/result";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
    class Param
    {
        public string name { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
