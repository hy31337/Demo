namespace jsZip
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Windows.Forms;
    using Yahoo.Yui.Compressor;

    public class Form1 : Form
    {
        private Button btnCompressor;
        private Button btnLoad;
        private Button btnSelect;
        private IContainer components;
        private ComboBox cbEncoder;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private int cssCount;
        private IList<FileInfo> fileList = new List<FileInfo>();
        private FolderBrowserDialog folderBrowserDialog1;
        private int jsCount;
        private Label label2;
        private ListView ListFile;
        private string strFilePath = "";
        private TextBox txtInfo;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem 菜单ToolStripMenuItem;
        private ToolStripMenuItem 退出ToolStripMenuItem;
        private ToolStripMenuItem 其他ToolStripMenuItem;
        private ToolStripMenuItem 帮助ToolStripMenuItem;
        private ToolStripMenuItem 关于ToolStripMenuItem;
        private GroupBox groupBox1;
        private CheckBox semi;
        private CheckBox isObfus;
        private TextBox txt_HHWZ;
        private Label label1;
        private CheckBox opt;
        private GroupBox groupBox2;
        private TextBox txt_HCD;
        private Label label3;
        private CheckBox cb_RemoveRemark;
        private ProgressBar progressBar1;
        private ComboBox cbb_YSLX;
        private Label label5;
        private CheckBox cb_ZXYSCss;
        private CheckBox cb_YSCss;
        private CheckBox cb_ZXYSJS;
        private CheckBox cb_YSJS;
        private Label label6;
        private TextBox txt_CS;
        private Label label7;
        private GroupBox groupBox3;
        private RadioButton rb_bf2;
        private RadioButton rb_bf1;
        private TextBox txtPath;

        public Form1()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCompressor_Click(object sender, EventArgs e)
        {
            if (this.ListFile.Items.Count != 0)
            {
                progressBar1.Maximum = this.ListFile.Items.Count;
                progressBar1.Value = 0;
                DirectoryInfo info = new DirectoryInfo(this.strFilePath);

                Encoding encoding = this.GetEncoding();
                int num = 0;
                long num2 = 0L;
                long num3 = 0L;
                int jswitdh = -1;
                int.TryParse(txt_HHWZ.Text, out jswitdh);
                int csswidth = -1;
                int.TryParse(txt_HCD.Text, out csswidth);
                bool blRemoveRemark = cb_RemoveRemark.Checked;
                bool blBackup = rb_bf1.Checked;
                int iCS = 25;
                int.TryParse(txt_CS.Text, out csswidth);
                iCS *= 1000;

                CssCompressionType ct = CssCompressionType.StockYuiCompressor;
                switch (cbb_YSLX.SelectedIndex)
                {
                    case 0: ct = CssCompressionType.StockYuiCompressor; break;
                    case 1: ct = CssCompressionType.Hybrid; break;
                    case 2: ct = CssCompressionType.MichaelAshRegexEnhancements; break;
                }

                DirectoryInfo info2 = new DirectoryInfo(info.FullName.Substring(0, info.FullName.Length - 1) + "_bak");
                if (blBackup)
                {
                    if (!info2.Exists)
                    {
                        info2.Create();
                    }
                }

                for (int i = 0; i < this.fileList.Count; i++)
                {
                    try
                    {
                        FileInfo infoold = this.fileList[i];
                        if (infoold.Name.Contains(".min."))
                        {
                            this.ListFile.Items[i].SubItems[3].Text = "跳过";
                        }
                        else
                        {
                            string strContent = File.ReadAllText(infoold.FullName, encoding);

                            if (blBackup)
                            {//备份
                                FileInfo infobak = new FileInfo(infoold.FullName.Replace(info.FullName, info2.FullName));
                                if (!infobak.Directory.Exists)
                                {
                                    infobak.Directory.Create();
                                }

                                //写入
                                File.WriteAllText(infobak.FullName, strContent);
                            }

                            string strMsg = "";
                            if (infoold.Extension.ToLower() == ".js")
                            {
                                if (cb_YSJS.Checked)
                                {
                                    try
                                    {
                                        strMsg = new JavaScriptCompressor(strContent, false, encoding, CultureInfo.CurrentCulture).Compress(isObfus.Checked, semi.Checked, opt.Checked, jswitdh);
                                        this.ListFile.Items[i].SubItems[3].Text = "完成";
                                    }
                                    catch (Exception ex)
                                    {
                                        if (cb_ZXYSJS.Checked)
                                        {
                                            //假如压缩失败（error），可进行在线压缩 
                                            string strMsg2 = RequestService("https://tool.oschina.net/action/jscompress/js_compress?munge=" + (isObfus.Checked ? "1" : "0") + "&linebreakpos=", RequestMethod.POST, strContent);
                                            if (strMsg2.StartsWith("{\"result\":"))
                                            {
                                                var rspObj = JsonConvert.DeserializeAnonymousType(strMsg2, new { result = "" });
                                                strMsg = rspObj.result;
                                                //strMsg = strMsg2.Remove(strMsg2.Length - 2, 2).Remove(0, 10); //strMsg2.TrimStart('{').TrimEnd('}').Remove(0, 9).Trim('\"');
                                                this.ListFile.Items[i].SubItems[3].Text = "完成-在线";
                                            }
                                            else
                                            {
                                                //if (strMsg2.StartsWith("{\"msg\":"))
                                                //{
                                                this.ListFile.Items[i].ForeColor = Color.Red;
                                                this.ListFile.Items[i].SubItems[2].Text = "0";
                                                this.ListFile.Items[i].SubItems[3].Text = "错误（本地、在线）:" + strMsg2;
                                                num++;
                                                //}
                                            }
                                        }
                                        else
                                        {
                                            throw ex;
                                        }
                                    }
                                }
                            }
                            else if (infoold.Extension.ToLower() == ".css")
                            {
                                if (cb_YSCss.Checked)
                                {
                                    try
                                    {
                                        bool ret = false;
                                        new System.Threading.Tasks.TaskFactory().StartNew(() =>
                                        {//异步等待
                                            strMsg = CssCompressor.Compress(strContent, csswidth, ct, blRemoveRemark);
                                            ret = true;
                                        }).Wait(iCS);

                                        if (!ret)
                                        {
                                            throw new Exception("超时");
                                        }

                                        this.ListFile.Items[i].SubItems[3].Text = "完成";

                                    }
                                    catch (Exception ex)
                                    {
                                        if (cb_ZXYSCss.Checked)
                                        {
                                            //假如压缩失败（error），可进行在线压缩 
                                            string strMsg2 = RequestService("https://tool.oschina.net/action/css_compress/js_compress?linebreakpos=", RequestMethod.POST, strContent);
                                            if (strMsg2.StartsWith("{\"result\":"))
                                            {
                                                var rspObj = JsonConvert.DeserializeAnonymousType(strMsg2, new { result = "" });
                                                strMsg = rspObj.result;
                                                //strMsg = strMsg2.Remove(strMsg2.Length - 2, 2).Remove(0, 10); //strMsg2.TrimStart('{').TrimEnd('}').Remove(0, 9).Trim('\"');
                                                this.ListFile.Items[i].SubItems[3].Text = "完成-在线:";
                                            }
                                            else
                                            {
                                                //if (strMsg2.StartsWith("{\"msg\":"))
                                                //{
                                                this.ListFile.Items[i].ForeColor = Color.Red;
                                                this.ListFile.Items[i].SubItems[2].Text = "0";
                                                this.ListFile.Items[i].SubItems[3].Text = "错误（本地、在线）:" + strMsg2;
                                                num++;
                                                //}
                                            }
                                        }
                                        else
                                        {
                                            throw ex;
                                        }
                                    }
                                }
                                else
                                {
                                    this.ListFile.Items[i].SubItems[3].Text = "跳过";
                                }
                            }

                            if (this.ListFile.Items[i].SubItems[3].Text.Contains("完成"))
                            {
                                //覆盖
                                File.WriteAllText(infoold.FullName, strMsg);
                                this.ListFile.Items[i].ForeColor = Color.Blue;
                                this.ListFile.Items[i].SubItems[2].Text = this.FormatSize((long)strMsg.Length);
                                //this.ListFile.Items[i].SubItems[3].Text = "完成";
                                num2 += strContent.Length;
                                num3 += strMsg.Length;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        this.ListFile.Items[i].ForeColor = Color.Red;
                        this.ListFile.Items[i].SubItems[2].Text = "0";
                        this.ListFile.Items[i].SubItems[3].Text = "错误:" + exception.Message;
                        num++;
                    }

                    progressBar1.PerformStep();
                    int num5 = i + 1;
                    this.txtInfo.Text = "共" + this.fileList.Count.ToString() + "个文件！正处理第" + num5.ToString() + "个文件！";
                    Application.DoEvents();
                }
                string text = "压缩完成！\r\n";
                if (num > 0)
                {
                    text = text + num.ToString() + "个文件压缩发生错误！\r\n";
                }
                text = text + "总体压缩：";
                text = text + Math.Round((((1.0 * num3) / ((double)num2)) * 100.0), 2) + "%";
                this.txtInfo.Text = text.Replace("\r\n", " ");
                if (num > 0)
                {
                    MessageBox.Show(text, "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    MessageBox.Show(text, "成功", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(this.txtPath.Text))
            {
                MessageBox.Show("路径不存在，请重新选择！");
                this.btnSelect.PerformClick();
            }
            else
            {
                this.strFilePath = this.txtPath.Text;
                this.jsCount = 0;
                this.cssCount = 0;
                this.fileList.Clear();
                this.ListFile.Items.Clear();
                this.GetDirList(new DirectoryInfo(this.txtPath.Text));
                if (this.fileList.Count == 0)
                {
                    this.btnCompressor.Enabled = false;
                    MessageBox.Show("未找到js或css文件！");
                }
                else
                {
                    this.txtInfo.Text = "共加载了" + this.jsCount.ToString() + "个JS文件、" + this.cssCount.ToString() + "个CSS文件！";
                    this.ListFile.BeginUpdate();
                    foreach (FileInfo info in this.fileList)
                    {
                        ListViewItem item = new ListViewItem(info.FullName.Replace(this.txtPath.Text, ""), 0);
                        item.SubItems.Add(this.FormatSize(info.Length));
                        item.SubItems.Add("");
                        item.SubItems.Add("就绪");
                        this.ListFile.Items.Add(item);
                    }
                    this.ListFile.EndUpdate();
                    this.btnCompressor.Enabled = true;
                }
            }
        }

        // <summary>
        /// 访问REST服务进行数据交互
        /// </summary>
        /// <param name="sUrl">服务地址</param>
        /// <param name="method">请求服务方式</param>
        /// <param name="sJsonData">请求内容</param>
        /// <returns>结果</returns>
        public static string RequestService(string sUrl, RequestMethod method, string sJsonData)
        {
            if (sJsonData == null) sJsonData = string.Empty;
            Uri url = new Uri(sUrl);

            //访问地址+密码模式验证
            //string encodedUri = EncodeText(url.AbsolutePath);

            //账户+密码，加密认证
            //string encodedUri = _WebAPIPWD;

            HttpWebRequest request = WebRequest.Create(sUrl) as HttpWebRequest;
            //request.Headers[HttpRequestHeader.Authorization] = encodedUri;
            //request.Timeout = 1000 * 120;

            //提交数据获取返回结果
            string strResult = string.Empty;

            if (method == RequestMethod.POST)
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                byte[] data = UTF8Encoding.UTF8.GetBytes(sJsonData);
                request.ContentLength = data.LongLength;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                    requestStream.Dispose();
                }
            }
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response != null)
                {
                    using (Stream bookmarksStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(bookmarksStream, UTF8Encoding.UTF8);
                        strResult = reader.ReadToEnd();
                        reader.Close();
                        bookmarksStream.Close();
                        bookmarksStream.Dispose();
                    }
                }
            }
            catch
            { }
            return strResult;
        }

        /// <summary>
        /// 请求REST方式
        /// </summary>
        public enum RequestMethod
        {
            /// <summary>
            /// POST方式
            /// </summary>
            POST,

            /// <summary>
            /// GET方式
            /// </summary>
            GET
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.txtPath.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.cbEncoder.Items.Add("UTF8");
            this.cbEncoder.Items.Add("ASCII");
            this.cbEncoder.Items.Add("Default");
            this.cbEncoder.Items.Add("Unicode");
            this.cbEncoder.SelectedIndex = 0;
            cbb_YSLX.SelectedIndex = 0;
            this.ListFile.ListViewItemSorter = new Common.ListViewColumnSorter();
            this.ListFile.ColumnClick += new ColumnClickEventHandler(Common.ListViewHelper.ListView_ColumnClick);
        }

        private string FormatSize(long size)
        {
            string str = "";
            double num = (size * 1.0) * ((size < 0L) ? ((double)(-1)) : ((double)1));
            if (num < 1024.0)
            {
                str = str + num.ToString() + "B";
            }
            else if (num < 1048576.0)
            {
                num /= 1024.0;
                str = str + num.ToString("0.##") + "K";
            }
            else if (num < 1073741824.0)
            {
                num = (num / 1024.0) / 1024.0;
                str = str + num.ToString("0.##") + "M";
            }
            else
            {
                str = str + ((((num / 1024.0) / 1024.0) / 1024.0)).ToString("0.##") + "G";
            }
            if (size < 0L)
            {
                str = "-" + str;
            }
            return str;
        }

        private void GetDirList(DirectoryInfo dir)
        {
            foreach (FileInfo info in dir.GetFiles("*.js"))
            {
                this.jsCount++;
                this.fileList.Add(info);
            }
            foreach (FileInfo info2 in dir.GetFiles("*.css"))
            {
                this.cssCount++;
                this.fileList.Add(info2);
            }
            DirectoryInfo[] directories = dir.GetDirectories();
            if (directories.Length > 0)
            {
                foreach (DirectoryInfo info3 in directories)
                {
                    this.GetDirList(info3);
                }
            }
        }

        private Encoding GetEncoding()
        {
            Encoding encoding = Encoding.Default;
            string str = this.cbEncoder.SelectedItem.ToString();
            if (str == null)
            {
                return encoding;
            }
            if (!(str == "UTF8"))
            {
                if (str != "ASCII")
                {
                    if (str == "Default")
                    {
                        return Encoding.Default;
                    }
                    if (str != "Unicode")
                    {
                        return encoding;
                    }
                    return Encoding.Unicode;
                }
            }
            else
            {
                return Encoding.UTF8;
            }
            return Encoding.ASCII;
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnCompressor = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.ListFile = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cbEncoder = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.菜单ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.其他ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txt_HHWZ = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_ZXYSJS = new System.Windows.Forms.CheckBox();
            this.opt = new System.Windows.Forms.CheckBox();
            this.semi = new System.Windows.Forms.CheckBox();
            this.cb_YSJS = new System.Windows.Forms.CheckBox();
            this.isObfus = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cb_ZXYSCss = new System.Windows.Forms.CheckBox();
            this.cb_YSCss = new System.Windows.Forms.CheckBox();
            this.cbb_YSLX = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_HCD = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_RemoveRemark = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label6 = new System.Windows.Forms.Label();
            this.txt_CS = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rb_bf2 = new System.Windows.Forms.RadioButton();
            this.rb_bf1 = new System.Windows.Forms.RadioButton();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(12, 30);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(401, 21);
            this.txtPath.TabIndex = 0;
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Location = new System.Drawing.Point(419, 28);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(74, 26);
            this.btnSelect.TabIndex = 1;
            this.btnSelect.Text = "选择目录";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnCompressor
            // 
            this.btnCompressor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompressor.Enabled = false;
            this.btnCompressor.Location = new System.Drawing.Point(626, 28);
            this.btnCompressor.Name = "btnCompressor";
            this.btnCompressor.Size = new System.Drawing.Size(120, 26);
            this.btnCompressor.TabIndex = 3;
            this.btnCompressor.Text = "压缩文件";
            this.btnCompressor.UseVisualStyleBackColor = true;
            this.btnCompressor.Click += new System.EventHandler(this.btnCompressor_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.Location = new System.Drawing.Point(12, 57);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(1155, 21);
            this.txtInfo.TabIndex = 4;
            // 
            // ListFile
            // 
            this.ListFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListFile.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader3});
            this.ListFile.FullRowSelect = true;
            this.ListFile.GridLines = true;
            this.ListFile.HideSelection = false;
            this.ListFile.Location = new System.Drawing.Point(12, 182);
            this.ListFile.MultiSelect = false;
            this.ListFile.Name = "ListFile";
            this.ListFile.ShowGroups = false;
            this.ListFile.Size = new System.Drawing.Size(1155, 428);
            this.ListFile.TabIndex = 5;
            this.ListFile.UseCompatibleStateImageBehavior = false;
            this.ListFile.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "文件";
            this.columnHeader1.Width = 310;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "原大小";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "压缩后大小";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "状态";
            this.columnHeader3.Width = 630;
            // 
            // cbEncoder
            // 
            this.cbEncoder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbEncoder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEncoder.FormattingEnabled = true;
            this.cbEncoder.Location = new System.Drawing.Point(333, 15);
            this.cbEncoder.Name = "cbEncoder";
            this.cbEncoder.Size = new System.Drawing.Size(96, 20);
            this.cbEncoder.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(274, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "编码类型:";
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.Location = new System.Drawing.Point(499, 28);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(120, 26);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "查找文件";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "选择发布项目所在路径";
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.菜单ToolStripMenuItem,
            this.其他ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1179, 25);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 菜单ToolStripMenuItem
            // 
            this.菜单ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.退出ToolStripMenuItem});
            this.菜单ToolStripMenuItem.Name = "菜单ToolStripMenuItem";
            this.菜单ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.菜单ToolStripMenuItem.Text = "菜单";
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.退出ToolStripMenuItem_Click);
            // 
            // 其他ToolStripMenuItem
            // 
            this.其他ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.帮助ToolStripMenuItem,
            this.关于ToolStripMenuItem});
            this.其他ToolStripMenuItem.Name = "其他ToolStripMenuItem";
            this.其他ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.其他ToolStripMenuItem.Text = "其他";
            // 
            // 帮助ToolStripMenuItem
            // 
            this.帮助ToolStripMenuItem.Name = "帮助ToolStripMenuItem";
            this.帮助ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.帮助ToolStripMenuItem.Text = "帮助";
            this.帮助ToolStripMenuItem.Click += new System.EventHandler(this.帮助ToolStripMenuItem_Click);
            // 
            // 关于ToolStripMenuItem
            // 
            this.关于ToolStripMenuItem.Name = "关于ToolStripMenuItem";
            this.关于ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.关于ToolStripMenuItem.Text = "关于";
            this.关于ToolStripMenuItem.Click += new System.EventHandler(this.关于ToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txt_HHWZ);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cb_ZXYSJS);
            this.groupBox1.Controls.Add(this.opt);
            this.groupBox1.Controls.Add(this.semi);
            this.groupBox1.Controls.Add(this.cb_YSJS);
            this.groupBox1.Controls.Add(this.isObfus);
            this.groupBox1.Location = new System.Drawing.Point(12, 93);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(343, 72);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "JS压缩参数";
            // 
            // txt_HHWZ
            // 
            this.txt_HHWZ.Location = new System.Drawing.Point(244, 42);
            this.txt_HHWZ.Name = "txt_HHWZ";
            this.txt_HHWZ.Size = new System.Drawing.Size(81, 21);
            this.txt_HHWZ.TabIndex = 4;
            this.txt_HHWZ.Text = "-1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(184, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "换行位置:";
            // 
            // cb_ZXYSJS
            // 
            this.cb_ZXYSJS.AutoSize = true;
            this.cb_ZXYSJS.Checked = true;
            this.cb_ZXYSJS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_ZXYSJS.Location = new System.Drawing.Point(17, 48);
            this.cb_ZXYSJS.Name = "cb_ZXYSJS";
            this.cb_ZXYSJS.Size = new System.Drawing.Size(72, 16);
            this.cb_ZXYSJS.TabIndex = 2;
            this.cb_ZXYSJS.Text = "在线压缩";
            this.cb_ZXYSJS.UseVisualStyleBackColor = true;
            // 
            // opt
            // 
            this.opt.AutoSize = true;
            this.opt.Location = new System.Drawing.Point(104, 48);
            this.opt.Name = "opt";
            this.opt.Size = new System.Drawing.Size(72, 16);
            this.opt.TabIndex = 2;
            this.opt.Text = "禁用优化";
            this.opt.UseVisualStyleBackColor = true;
            // 
            // semi
            // 
            this.semi.AutoSize = true;
            this.semi.Location = new System.Drawing.Point(188, 20);
            this.semi.Name = "semi";
            this.semi.Size = new System.Drawing.Size(96, 16);
            this.semi.TabIndex = 1;
            this.semi.Text = "是否保留分号";
            this.semi.UseVisualStyleBackColor = true;
            // 
            // cb_YSJS
            // 
            this.cb_YSJS.AutoSize = true;
            this.cb_YSJS.Checked = true;
            this.cb_YSJS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_YSJS.Location = new System.Drawing.Point(16, 21);
            this.cb_YSJS.Name = "cb_YSJS";
            this.cb_YSJS.Size = new System.Drawing.Size(72, 16);
            this.cb_YSJS.TabIndex = 0;
            this.cb_YSJS.Text = "是否启用";
            this.cb_YSJS.UseVisualStyleBackColor = true;
            // 
            // isObfus
            // 
            this.isObfus.AutoSize = true;
            this.isObfus.Checked = true;
            this.isObfus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.isObfus.Location = new System.Drawing.Point(103, 21);
            this.isObfus.Name = "isObfus";
            this.isObfus.Size = new System.Drawing.Size(72, 16);
            this.isObfus.TabIndex = 0;
            this.isObfus.Text = "是否混淆";
            this.isObfus.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cb_ZXYSCss);
            this.groupBox2.Controls.Add(this.cb_YSCss);
            this.groupBox2.Controls.Add(this.cbb_YSLX);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txt_HCD);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cb_RemoveRemark);
            this.groupBox2.Location = new System.Drawing.Point(366, 93);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(347, 72);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "CSS压缩参数";
            // 
            // cb_ZXYSCss
            // 
            this.cb_ZXYSCss.AutoSize = true;
            this.cb_ZXYSCss.Checked = true;
            this.cb_ZXYSCss.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_ZXYSCss.Location = new System.Drawing.Point(6, 48);
            this.cb_ZXYSCss.Name = "cb_ZXYSCss";
            this.cb_ZXYSCss.Size = new System.Drawing.Size(72, 16);
            this.cb_ZXYSCss.TabIndex = 7;
            this.cb_ZXYSCss.Text = "在线压缩";
            this.cb_ZXYSCss.UseVisualStyleBackColor = true;
            // 
            // cb_YSCss
            // 
            this.cb_YSCss.AutoSize = true;
            this.cb_YSCss.Checked = true;
            this.cb_YSCss.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_YSCss.Location = new System.Drawing.Point(6, 22);
            this.cb_YSCss.Name = "cb_YSCss";
            this.cb_YSCss.Size = new System.Drawing.Size(72, 16);
            this.cb_YSCss.TabIndex = 7;
            this.cb_YSCss.Text = "是否启用";
            this.cb_YSCss.UseVisualStyleBackColor = true;
            // 
            // cbb_YSLX
            // 
            this.cbb_YSLX.FormattingEnabled = true;
            this.cbb_YSLX.Items.AddRange(new object[] {
            "YUI模式",
            "混合",
            "正则增强"});
            this.cbb_YSLX.Location = new System.Drawing.Point(170, 42);
            this.cbb_YSLX.Name = "cbb_YSLX";
            this.cbb_YSLX.Size = new System.Drawing.Size(121, 20);
            this.cbb_YSLX.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(104, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "压缩类型:";
            // 
            // txt_HCD
            // 
            this.txt_HCD.Location = new System.Drawing.Point(254, 18);
            this.txt_HCD.Name = "txt_HCD";
            this.txt_HCD.Size = new System.Drawing.Size(81, 21);
            this.txt_HCD.TabIndex = 4;
            this.txt_HCD.Text = "-1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(202, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "行长度:";
            // 
            // cb_RemoveRemark
            // 
            this.cb_RemoveRemark.AutoSize = true;
            this.cb_RemoveRemark.Checked = true;
            this.cb_RemoveRemark.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_RemoveRemark.Location = new System.Drawing.Point(104, 21);
            this.cb_RemoveRemark.Name = "cb_RemoveRemark";
            this.cb_RemoveRemark.Size = new System.Drawing.Size(96, 16);
            this.cb_RemoveRemark.TabIndex = 0;
            this.cb_RemoveRemark.Text = "是否移除备注";
            this.cb_RemoveRemark.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 173);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1157, 4);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(734, 113);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "超时时长(秒)";
            // 
            // txt_CS
            // 
            this.txt_CS.Location = new System.Drawing.Point(812, 108);
            this.txt_CS.Name = "txt_CS";
            this.txt_CS.Size = new System.Drawing.Size(32, 21);
            this.txt_CS.TabIndex = 4;
            this.txt_CS.Text = "5";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(858, 113);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "是否备份:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rb_bf2);
            this.groupBox3.Controls.Add(this.rb_bf1);
            this.groupBox3.Controls.Add(this.cbEncoder);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(727, 93);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(443, 72);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "其他";
            // 
            // rb_bf2
            // 
            this.rb_bf2.AutoSize = true;
            this.rb_bf2.Checked = true;
            this.rb_bf2.Location = new System.Drawing.Point(227, 18);
            this.rb_bf2.Name = "rb_bf2";
            this.rb_bf2.Size = new System.Drawing.Size(35, 16);
            this.rb_bf2.TabIndex = 0;
            this.rb_bf2.TabStop = true;
            this.rb_bf2.Text = "否";
            this.rb_bf2.UseVisualStyleBackColor = true;
            // 
            // rb_bf1
            // 
            this.rb_bf1.AutoSize = true;
            this.rb_bf1.Location = new System.Drawing.Point(190, 18);
            this.rb_bf1.Name = "rb_bf1";
            this.rb_bf1.Size = new System.Drawing.Size(35, 16);
            this.rb_bf1.TabIndex = 0;
            this.rb_bf1.Text = "是";
            this.rb_bf1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1179, 622);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.txt_CS);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.ListFile);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnCompressor);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBox3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "JS&CSS压缩工具V2.1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.gobaidugle.com/search4?keyword=.net%E6%B1%82%E5%AD%A6%E8%80%85&num=10&one=bing&two=baidu&three=sogou&four=so&rsv_enter=1&rsv_bp=1");
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("作者:hy\r\nEmail:631538352@qq.com；想了解吗？", "关于", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Process.Start("https://www.gobaidugle.com/search4?keyword=.net%E6%B1%82%E5%AD%A6%E8%80%85&num=10&one=bing&two=baidu&three=sogou&four=so&rsv_enter=1&rsv_bp=1");
            }
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("1.点击按钮...选择您的项目发布所在文件夹.\r\n2.点击列出按钮,将自动把文件夹下所有CSS&JS列出.\r\n3.设置相应的压缩参数.\r\n4.点击压缩.压缩完成后直接替换原文件,原文件的备份将放在**_Bak目录下.\r\nCSS&JS输入-1为不换行\r\n");//软件使用 YUILibrary和oschina压缩
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


    }
}

