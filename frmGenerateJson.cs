using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBJ.Controls;
using DBJ.Divorced.Define;
using DBJ.Divorced.Helper;

namespace DBJ.Tool.Json
{
    public partial class frmGenerateJson : ParentForm
    {
        public frmGenerateJson()
        {
            InitializeComponent();
            base.InitLoadingPanel(this.Size);
        }

        public List<List<KeyValue>> InstaceList { get; set; }


        public List<KeyValue> CommonList { get; set; }

        public List<string> NeedGenerateList { get; set; }

        private List<string> GetCheckedList()
        {
            var tmp = new List<string>();
            for (int i = 0; i < this.checkedListBox1.CheckedItems.Count; i++)
            {
                tmp.Add(this.checkedListBox1.CheckedItems[i].ToString());
            }
            return tmp;
        }

        private string JsonTemplate { get; set; }

        private string TemplatePath { get; set; }

        private string OutputPath { get; set; }

        private string DataPath { get; set; }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            TemplatePath = this.txtTemplate.Text;
            OutputPath = this.txtOutput.Text;
            //DataPath = this.txtData.Text;
            if (string.IsNullOrEmpty(TemplatePath) || string.IsNullOrEmpty(OutputPath) ||
                string.IsNullOrEmpty(DataPath))
            {
                MessageBox.Show("Pelase enter");
                return;
            }
            if (!Check())
            {
                MessageBox.Show("File not eixsted");
                return;
            }

            ReadJsonTemplate();

            NeedGenerateList = GetCheckedList();

            ReplaceAll();

            MessageBox.Show("完成");
        }

        private void ReplaceAll()
        {
            foreach (var instance in this.InstaceList)
            {
                ReplaceOne(instance);
            }
        }

        private void ReplaceOne(List<KeyValue> instanceList)
        {
            var fileKey = "FileName";
            var fileName = instanceList.FirstOrDefault(x => x.Key == fileKey).Value;

            if (!this.NeedGenerateList.Contains(fileName))
                return;
            
            var path = $"{this.OutputPath}\\{fileName}.json";
            var tempTemplate = this.JsonTemplate.Clone().ToString();

            for (int i = 1; i < instanceList.Count; i++)
            {
                var item = instanceList[i];
                var key = $"#{item.Key}#";
                var value = item.Value;
                tempTemplate = tempTemplate.Replace(key, value);
            }

            foreach (var item in this.CommonList)
            {
                var key = $"#{item.Key}#";
                var value = item.Value;
                tempTemplate = tempTemplate.Replace(key, value);
            }

            FileHelper.SaveFile(path, tempTemplate);
        }


        private void ReadJsonTemplate()
        {
            this.JsonTemplate = FileHelper.ReadFile(TemplatePath);
        }

        private void ReadDataDele()
        {
            var instanceSheetName = "Default";
            var commonSheetName = "Common";
            this.InstaceList = ExcelHelper.GetContentVertical(DataPath, instanceSheetName);
            var temp = ExcelHelper.GetContent(DataPath, commonSheetName);
            this.CommonList = new List<KeyValue>();
            foreach (var item in temp)
            {
                var kv = new KeyValue();
                kv.Key = item[0].Value;
                kv.Value = item[1].Value;
                CommonList.Add(kv);
            }
            ExcelHelper.KillProcess();

            this.BeginInvoke(new DeleParaNone(ReadDataAction));

            base.EndExecute(null);
        }

        private void ReadDataAction()
        {
            this.checkedListBox1.Items.Clear();;
            foreach (var item in this.InstaceList)
            {
                this.checkedListBox1.Items.Add(item[0].Value);
            }
        }

     

        private bool Check()
        {
            if (!FileHelper.ExistFile(TemplatePath) || !FileHelper.ExistFile(DataPath) ||
                !FileHelper.ExistFolder(OutputPath))
            {
                return false;
            }
            return true;
        }

        private void btnSelectTemplate_Click(object sender, EventArgs e)
        {
            OpenFileDialog _dialog = new OpenFileDialog { Filter = "Json files|*.json|All files|*.*" };

            if (_dialog.ShowDialog() != DialogResult.OK)
                return;
            this.txtTemplate.Text = _dialog.FileName;
        }

        private void btnSelectData_Click(object sender, EventArgs e)
        {
            OpenFileDialog _dialog = new OpenFileDialog { Filter = "Excel files|*.xls|All files|*.*" };

            if (_dialog.ShowDialog() != DialogResult.OK)
                return;
            this.DataPath = _dialog.FileName;


            base.PrepareExecute();
            base.StartExecute(ReadDataDele);
        }



        private void btnSelectOutputFolder_Click(object sender, EventArgs e)
        {
            var fbd1 = new FolderBrowserDialog();
            if (fbd1.ShowDialog() == DialogResult.OK)
            {
                var path = fbd1.SelectedPath;
                this.txtOutput.Text = path;
            }
        }

        private void frmGenerateJson_Load(object sender, EventArgs e)
        {

        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.checkedListBox1.Items.Count; i++)
            {
                this.checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.checkedListBox1.Items.Count; i++)
            {
                this.checkedListBox1.SetItemChecked(i, !this.checkedListBox1.GetItemChecked(i));
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //ExcelHelper.GetContentVertical(@"D:\Json\JsonTemplate.xls", "Sheet1");
            this.Close();
        }
    }
}
