using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using DBJ.Divorced.Define;
using DBJ.Divorced.Helper;

namespace DBJ.Tool.Json
{
    public partial class frmInit : Form
    {
        public frmInit()
        {
            InitializeComponent();
        }

        delegate void ShowMsg(string _msg);

        private void WFInit_Load(object sender, EventArgs e)
        {
            //frmMain fm = new frmMain();
            //Source.mainForm = fm;
            Thread thread = new Thread(new ThreadStart(Init));
            thread.Start();
        }

        private void WriteLog(string msg)
        {
            if (msg == "end")
            {
                this.label1.Text = "初始化完毕!";
                StartPorgram();
            }
            else
            {
                this.label1.Text = msg;
            }
        }

        //private void WriteErrorLog(string msg)
        //{
        //    if (msg == "end")
        //    {
        //        this.label1.Text = "初始化完毕!";
        //        StartPorgram();
        //    }
        //    else
        //    {
        //        this.label2.Text = msg;
        //    }
        //}
        string fnLog = "system.log";
        /// <summary>
        /// 初始化某些参数
        /// </summary>
        private void Init()
        {
            string configError = string.Empty;

            try
            {
                Thread.Sleep(100);
                this.BeginInvoke(new ShowMsg(WriteLog), "开始切水果!");

            }
            catch
            {
                Thread.Sleep(100);
                this.BeginInvoke(new ShowMsg(WriteLog), "服务器参数设定配置出错!");
                configError += "\r\n服务器参数设定配置出错";
            }

            try
            {
                Thread.Sleep(100);
                this.BeginInvoke(new ShowMsg(WriteLog), "今天有台风!");

                //if (Source.remotingServerIP.ToLower() == "localhost")
                //{
                //    Source.remotingServerIP = "0.0.0.0";
                //    Source.remotingServer = (URadio.LocatingService.RemotingEntry.IServiceApi)Activator.GetObject(typeof(URadio.LocatingService.RemotingEntry.IServiceApi), "http://0.0.0.0:10607/ServiceMarshal.soap");
                //}
                //else
                //{
                //    Source.remotingServer = (URadio.LocatingService.RemotingEntry.IServiceApi)Activator.GetObject(typeof(URadio.LocatingService.RemotingEntry.IServiceApi), System.Configuration.ConfigurationManager.AppSettings["RemotingServer"].ToString());
                //}
            }
            catch
            {
                Thread.Sleep(100);
                this.BeginInvoke(new ShowMsg(WriteLog), "初始化远程服务器出错!");

                configError += "\r\n初始化远程服务器出错!";
            }

            try
            {
                Thread.Sleep(100);
                this.BeginInvoke(new ShowMsg(WriteLog), "瞎叔太帅了!");

                //LoadSettingInfo();
            }
            catch
            {
                Thread.Sleep(100);
                this.BeginInvoke(new ShowMsg(WriteLog), "初始化远程服务器出错!");

                configError += "\r\n初始化远程服务器出错!";
            }
            var parameterList = new List<ParameterItem>();
            try
            {
                LoadExcelFile();
                Thread.Sleep(100);
                this.BeginInvoke(new ShowMsg(WriteLog), "读取excel!");
            }
            catch
            {
                Thread.Sleep(100);

                this.BeginInvoke(new ShowMsg(WriteLog), "读取EXCEL出错!");

                configError += "\r\n读取EXCEL出错!";
            }

            //Source.marker = DateTime.Now.Ticks.ToString();
            //Source.version = 1;

            //ThreadPool.SetMaxThreads(10, 10);

            if (configError != "")
            {
                this.BeginInvoke(new ShowMsg(WriteLog), configError);
            }
            Thread.Sleep(100);
            this.BeginInvoke(new ShowMsg(WriteLog), "开始初始化主界面");

            Thread.Sleep(100);
            this.BeginInvoke(new ShowMsg(WriteLog), "记得付钱给瞎叔");

            Thread.Sleep(100);
            this.BeginInvoke(new ShowMsg(WriteLog),"end");
        }

        public List<List<KeyValue>> ParameterList { get; set; }
        private bool isParameterExcelLoaded;

        private void LoadExcelFile()
        {
            var path = $"{Environment.CurrentDirectory}\\JsonTemplate.xls";
            //var instanceSheetName = "Default";
            var trunkSheetName = "Trunk";
            var n1SheetName = "N1";
            var n2SheetName = "N2";
            var commonSheetName = "Common";
            var list = new List<List<KeyValue>>();
            ReadExcel(path, commonSheetName, list);
            ReadExcel(path, trunkSheetName, list, true);
            ReadExcel(path, n1SheetName, list, true);
            ReadExcel(path, n2SheetName, list, true);

            ExcelHelper.KillProcess();
            isParameterExcelLoaded = true;
            ParameterList = list;
        }

        private void ReadExcel(string path, string instanceSheetName, List<List<KeyValue>> list, bool isV = false)
        {
            if (isV)
                list.AddRange(ExcelHelper.GetContentVertical(path, instanceSheetName));
            else
                list.AddRange(ExcelHelper.GetContent(path, instanceSheetName));
        }

        private void StartPorgram()
        {
            this.Hide();
            try
            {
                 //var frm = new frmManageIIS();
                var frm = new frmMain();
                frm.ParaInExcel = this.ParameterList;
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Close();
        }
    }
}
