using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DBJ.Controls;
using DBJ.Divorced.Define;
using DBJ.Divorced.Helper;
using DBJ.Tool.Json.Sections;

namespace DBJ.Tool.Json
{
    public partial class frmMain : ParentForm
    {
        public frmMain()
        {
            InitializeComponent();
            base.InitLoadingPanel(this.Size);
        }

        private IList<JsonFileItem> JsonFileList { get; set; }

        //private IList<JsonFileItem> WorkableJsonFileList
        //{
        //    get { return JsonFileList.Where(x => x.HasError == false).ToList(); }
        //}

        private IList<JsonItemControl> JsonControlList { get; set; }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            var fbd1 = new FolderBrowserDialog();
            if (fbd1.ShowDialog() == DialogResult.OK)
            {
                var path = fbd1.SelectedPath;
                this.txtPath.Text = path;
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog _dialog = new OpenFileDialog { Filter = "Json files|*.json|All files|*.*" };

            if (_dialog.ShowDialog() != DialogResult.OK)
                return;
            this.txtPath.Text = _dialog.FileName;
        }

        private void btnMultiFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog _dialog = new OpenFileDialog { Filter = "Json files|*.json|All files|*.*" };
            _dialog.Multiselect = true;
            if (_dialog.ShowDialog() != DialogResult.OK || _dialog.FileNames.Length == 0)
                return;
            var isClear = this.cbClear.Checked;
            var list = new List<object>();
            list.Add(isClear);

            var files = _dialog.FileNames.ToList(); 
            
            list.Add(files);
         

            base.PrepareExecute();
            base.StartExecute(LoadFromMultiFilesDele, list);
        }

        private void LoadFromMultiFilesDele(object obj)
        {
            var list = (List<object>)obj;

            var isClear = (bool)list[0];           

            if (isClear)
                this.JsonFileList.Clear();

            AnalysePathes((IList<string>)list[1]);

            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear All", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.pnlLeft.Controls.Clear();
                this.JsonControlList = new List<JsonItemControl>();
                this.JsonFileList = new List<JsonFileItem>();
            }
        }

        JsonFileItem modelJson = new JsonFileItem();

        private List<JsonFileItem> jsonFiles = new List<JsonFileItem>();

        private void frmMain_Load(object sender, EventArgs e)
        {
            JsonFileList = new List<JsonFileItem>();
            this.txtNewJsonFolder.Text = JsonPool.SaveFolder;
            this.txtPath.Text = JsonPool.LoadFolder;
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            //base.PrepareExecute();
            //base.StartExecute(BindFilePath, folderName);
        }

        #region LoadFolder
        private void btnLoadFolder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtPath.Text))
            {
                MessageBox.Show("没有地址");
                return;
            }

            var path = this.txtPath.Text;
            var isClear = this.cbClear.Checked;
            var list = new List<object>();
            list.Add(path);
            list.Add(isClear);

            base.PrepareExecute();
            base.StartExecute(LoadFromFolderDele, list);
        }

        private void AnalysePathes(IList<string> filePathes)
        {
            foreach (var item in filePathes)
            {
                var jfi = new JsonFileItem();
                jfi.Id = Guid.NewGuid().ToString();
                jfi.Path = item;
                if (!FileHelper.ExistFile(item))
                {
                    jfi.HasError = true;
                    jfi.ErrorMessage += "文件不存在\r\n";
                    this.JsonFileList.Add(jfi);
                    continue;
                }
                if(Path.GetExtension(item)!=".json")
                {
                    continue;
                }

                if (this.JsonFileList.FirstOrDefault(x => x.Path == item) != null)
                {
                    jfi.HasError = true;
                    jfi.ErrorMessage += "已经存在\r\n";
                    continue;
                    //this.JsonFileList.Add(jfi);
                }
                jfi.Name = GetPathName(jfi.Path);

                GetInstaceInfo(jfi.Name, jfi);

                jfi.NameAsPara = jfi.Name;
                //jfi.Order = 0;
                try
                {
                    jfi.Content = FileHelper.ReadFile(jfi.Path);
                    JsonOperator jo = new JsonOperator();
                    jo.SetJsonToItemList(jfi.Content);
                    jfi.JsonItems = jo.JsonItemList;
                }
                catch (Exception ex)
                {
                    jfi.HasError = true;
                    jfi.ErrorMessage = ex.Message;
                }
                this.JsonFileList.Add(jfi);
            }

        }

        private void GetInstaceInfo(string name, JsonFileItem jfi)
        {
            try
            {
                var strs = name.Split('_');
                jfi.Release = strs[1];
                jfi.InstanceName = strs[2];
                if (strs.Length == 3)
                {
                    jfi.Number = FormatHelper.GetLastNumber(strs[2]).ToString();
                    jfi.InstanceName= FormatHelper.GetExecptNumber(strs[2]).ToString();
                }
                 
                if (strs.Length >= 4)
                    jfi.Number = strs[3];
            }
            catch
            {

            }
        }

        private void LoadFromFolderDele(object obj)
        {
            var list = (List<object>)obj;

            var path = list[0].ToString();

            var isClear = (bool)list[1];

            var filePathes = FileHelper.GetFilesInFolder(path);

            if (isClear)
                this.JsonFileList.Clear();

            AnalysePathes(filePathes);

            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        #endregion

        private void ClearInput()
        {
            this.txtKey.Text = "";
            this.txtParent.Text = "";
            this.txtValue.Text = "";
            this.cbIsObject.Checked = false;
            this.SelectedLastElement = null;
            this.SelectedCurrentElement = null;
        }

        private void BindJsonFileItemAction()
        {
            this.pnlLeft.Controls.Clear();
            this.JsonControlList = new List<JsonItemControl>();
            if (this.JsonFileList.Count > 0)
            {
                this.lblNoResult.Visible = false;
            }
            else
            {
                this.lblNoResult.Visible = true;
            }
            int i = 0;
            foreach (JsonFileItem item in this.JsonFileList)
            {
                JsonItemControl jic = new JsonItemControl();
                jic.GUID = item.Id;
                item.Order = i++;
                jic.JsonFileItem = item;
                jic.SetPosition(this.pnlLeft);
                jic.DisplayMode = BindMode;
                jic.DisplayJsonInTree = BindTreeView;
                this.JsonControlList.Add(jic);
            }

            ClearInput();
            this.tvJsonMain.Nodes.Clear();
        }

        public void BindTreeView(IList<JsonItem> jsonItems, string parentId, TreeNode parentNode,string fileId, bool flag)
        {
            var roots = jsonItems.Where(x => x.ParentId == parentId && !x.IsDeleted).OrderBy(x => x.Order);
            if (flag == false)
            {
                this.tvJsonMain.Nodes.Clear();
                flag = true;
            }
            foreach (var root in roots)
            {
                var keyValue = $"\"{root.Key}\" :";
                if (!root.IsObject)
                    keyValue += $"\"{root.Value}\"";
                var tn = new TreeNode
                {
                    Text = keyValue,
                    Tag =$"{fileId}|{root.Id}"
                };
                //SetTNColor(iisItem.IisItemType, tn);
                //tn.Checked = iisItem.IsSelected;
                if (parentNode == null)
                {
                    tvJsonMain.Nodes.Add(tn);
                }
                else
                {
                    parentNode.Nodes.Add(tn);
                }

                var subList = jsonItems.Where(x => x.ParentId == root.Id && !x.IsDeleted).ToList().OrderBy(x => x.Order);
                BindTreeView(jsonItems, root.Id, tn, fileId, flag);
            }
        }

        public void BindMode(string id)
        {
            foreach (var item in this.JsonControlList)
            {
                var ji = JsonFileList.FirstOrDefault(x => x.Id == item.GUID);
                if (item.GUID != id)
                {
                    item.IsModel = false;
                    ji.IsMode = false;
                }
                else
                {
                    item.IsModel = true;
                    ji.IsMode = true;
                }
            }
        }

        private string GetPathName(string path)
        {
            var wholeName = PathHelper.GetWholeFileName(path);
            var name = wholeName.Substring(0, wholeName.Length - 5);
            return name;
        }

        private void btnDisplayTreeNodeText_Click(object sender, EventArgs e)
        {
            if (this.tvJsonMain != null && this.tvJsonMain.Nodes.Count > 0 && this.tvJsonMain.SelectedNode != null)
                MessageBox.Show(this.tvJsonMain.SelectedNode.Text);
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            this.tvJsonMain.ExpandAll();
        }

        private void btnCollapse_Click(object sender, EventArgs e)
        {
            this.tvJsonMain.CollapseAll();
        }

        private void btnLBSelectAll_Click(object sender, EventArgs e)
        {
            foreach (var item in this.JsonFileList)
            {
                item.IsSeleted = true;
                JsonControlList.FirstOrDefault(x => x.GUID == item.Id).JsonFileItem = item;
            }
        }

        private void btnLBInvertSelect_Click(object sender, EventArgs e)
        {
            foreach (var item in this.JsonFileList)
            {
                item.IsSeleted = !item.IsSeleted;
                JsonControlList.FirstOrDefault(x => x.GUID == item.Id).JsonFileItem = item;
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            for (int i = JsonFileList.Count - 1; i >= 0; i--)
            {
                var jif = JsonFileList[i];
                var jic = JsonControlList.FirstOrDefault(x => x.GUID == jif.Id);
                if (jic == null || !jic.IsSelected)
                    continue;
                JsonControlList.Remove(jic);
                JsonFileList.RemoveAt(i);
            }

            this.BindJsonFileItemAction();
        }

        private void btnClearTree_Click(object sender, EventArgs e)
        {
            this.tvJsonMain.Nodes.Clear();
        }

        private string SavedFolder { get; set; }

        private void btnCopyFromModel_Click(object sender, EventArgs e)
        {
            var model = JsonFileList.FirstOrDefault(x => x.IsMode);
            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (model == null || selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }

            this.SavedFolder = this.txtNewJsonFolder.Text;
            if (!FileHelper.ExistFolder(this.SavedFolder))
            {
                MessageBox.Show("Saved Folder not existed");
                return;
            }

            var list = new List<object>();
            list.Add(model);
            list.Add(selected);

            base.PrepareExecute();
            base.StartExecute(CopyFromModelDele, list);
        }

        private void CopyFromModelDele(object obj)
        {
            var paras = (List<object>)obj;
            var model = (JsonFileItem)paras[0];
            var selected = (IList<JsonFileItem>)paras[1];

            var jo = new JsonOperator();

            foreach (var item in selected)
            {
                jo.SetAsModel(model.JsonItems, item.JsonItems);
                var newJson = jo.SetToJson();
                var fileName = PathHelper.GetWholeFileName(item.Path);
                var path = $"{SavedFolder}\\{fileName}";
                FileHelper.SaveFile(path, newJson);
            }

            this.BeginInvoke(new DeleParaNone(CopyFromModelAction));

            base.EndExecute(null);
        }

        private void CopyFromModelAction()
        {
            System.Diagnostics.Process.Start("explorer.exe", this.SavedFolder);
        }

        private void btnERMS_Click(object sender, EventArgs e)
        {
            var model = JsonFileList.FirstOrDefault(x => x.IsMode);
            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }

            //this.SavedFolder = this.txtNewJsonFolder.Text;
            //if (!FileHelper.ExistFolder(this.SavedFolder))
            //{
            //    MessageBox.Show("Saved Folder not existed");
            //    return;
            //}             

            var frm = new frmManageERMS();
            frm.ParameterList = ParameterList;
            frm.ValueList = ValueList;
            frm.NeedUpdateJsons = selected;
            frm.BindNodes("");
            frm.ShowDialog();
            BindJsonFileItemAction();
        }

        private void btnLoadTemplateData_Click(object sender, EventArgs e)
        {
            OpenFileDialog _dialog = new OpenFileDialog { Filter = "Excel files|*.xls|All files|*.*" };

            if (_dialog.ShowDialog() != DialogResult.OK)
                return;
            var path = _dialog.FileName;


            base.PrepareExecute();
            base.StartExecute(ReadDataDele, path);
        }

        private void ReadDataDele(object obj)
        {
            var path = obj.ToString();
            var instanceSheetName = "Default";
            var commonSheetName = "Common";
            var temp = ExcelHelper.GetContentVertical(path, instanceSheetName);
            var temp1 = ExcelHelper.GetContent(path, commonSheetName);
            var list1 = new List<ParameterItem>();
            foreach (var item in temp1)
            {
                var kv = new ParameterItem();
                kv.Id = Guid.NewGuid().ToString();
                kv.Group = "Variable";
                kv.Name = item[0].Value;
                kv.Value = item[1].Value;
                list1.Add(kv);
            }


            foreach (var item in temp)
            {
                var kv = new ParameterItem();
                kv.Id = Guid.NewGuid().ToString();
                kv.Group = "Variable";
                kv.Name = item[0].Value;
                kv.Value = item[1].Value;
                list1.Add(kv);
            }

            this.ParameterList = list1;

            ExcelHelper.KillProcess();

            this.BeginInvoke(new DeleParaObject(ReadDataAction), list1);

            base.EndExecute(null);
        }

        public List<ParameterItem> ParameterList
        {
            get; set;
        }

        public List<List<KeyValue>> ParaInExcel { get; set; }

        public List<ParameterItem> ValueList
        {
            get; set;
        }

        private void ReadDataAction(object obj)
        {
            try
            {
                var collection = (List<ParameterItem>)obj;
                this.cbbVariable.AutoCompleteSource = AutoCompleteSource.CustomSource;
                this.cbbVariable.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                this.cbbVariable.AutoCompleteCustomSource = InitCompleteCollection(collection.Select(x => x.Name).ToList());

                this.cbbVariable.DataSource = collection;
                this.cbbVariable.DisplayMember = "Name";
                this.cbbVariable.ValueMember = "Id";
            }
            catch (Exception ex)
            {

            }
        }

        private AutoCompleteStringCollection InitCompleteCollection(IList<string> list)
        {
            var collection = new AutoCompleteStringCollection();

            foreach (var item in list)
                collection.Add(item);

            return collection;
        }

        private void btnLoadValue_Click(object sender, EventArgs e)
        {

            base.PrepareExecute();
            base.StartExecute(ReadValueDataDele);
        }

        private void ReadValueDataDele()
        {
            var path = $"{Environment.CurrentDirectory}\\ValueData.txt";

            var content = FileHelper.ReadFile(path);

            var list = content.Split(Environment.NewLine.ToCharArray());

            var result = "";
            var list1 = new List<ParameterItem>();
            foreach (string item in list)
            {
                if (item == "")
                    continue;
                var kv = new ParameterItem();
                kv.Id = Guid.NewGuid().ToString();
                kv.Group = "Value";
                kv.Name = "";
                kv.Value = item;
                list1.Add(kv);
            }

            ValueList = list1;

            this.BeginInvoke(new DeleParaObject(ReadValueDataAction), list1);

            base.EndExecute(null);
        }

        private void ReadValueDataAction(object obj)
        {
            try
            {
                var collection = (List<ParameterItem>)obj;
                this.cbbValue.AutoCompleteSource = AutoCompleteSource.CustomSource;
                this.cbbValue.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                this.cbbValue.AutoCompleteCustomSource = InitCompleteCollection(collection.Select(x => x.Value).ToList());

                this.cbbValue.DataSource = collection;
                this.cbbValue.DisplayMember = "Value";
                this.cbbValue.ValueMember = "Id";
            }
            catch (Exception ex)
            {

            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            this.JsonControlList.Clear();
            this.JsonFileList.Clear();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var selected = JsonFileList.Where(x => x.IsSeleted).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Selected");
                return;
            }

            this.SavedFolder = this.txtNewJsonFolder.Text;
            if (!FileHelper.ExistFolder(this.SavedFolder))
            {
                MessageBox.Show("Saved Folder not existed");
                return;
            }         

            base.PrepareExecute();
            base.StartExecute(SaveFilesDele, selected);
        }

        private void SaveFilesDele(object obj)
        {
            var selected = (List<JsonFileItem>)obj;        

            var jo = new JsonOperator();

            foreach (JsonFileItem item in selected)
            {
                //jo.SetAsModel(model.JsonItems, item.JsonItems);
                jo.JsonItemList = item.JsonItems;
                var newJson = jo.SetToJson();

                var x = JsonHelper.ConvertJsonString(newJson);

                var fileName = PathHelper.GetWholeFileName(item.Path);
                var path = $"{SavedFolder}\\{fileName}";
                FileHelper.SaveFile(path, x);
            }

            this.BeginInvoke(new DeleParaNone(CopyFromModelAction));

            base.EndExecute(null);
        }

        private void btnSetEIMASbyExcel_Click(object sender, EventArgs e)
        {
            //var model = JsonFileList.FirstOrDefault(x => x.IsMode);
            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }

            this.SavedFolder = this.txtNewJsonFolder.Text;
            if (!FileHelper.ExistFolder(this.SavedFolder))
            {
                MessageBox.Show("Saved Folder not existed");
                return;
            }

            var list = new List<object>();         
            list.Add(selected);
            list.Add(2);
            base.PrepareExecute();
            base.StartExecute(SetEIMASByExcel, list);
        }

        private void SetEIMASByExcel(object obj)
        {
            var l = (List<object>)obj;

            var selected = (List<JsonFileItem>)l[0];

            var isEnable = (int)l[1];

            var ec = new EIMASConfig();
            ec.ParameterList = this.ParaInExcel;
            ec.NeedUpdateJsons = selected;
            
            ec.SetEIMAS(isEnable);
            //var jo = new JsonOperator();
            //foreach (JsonFileItem item in selected)
            //{
            //    //jo.SetAsModel(model.JsonItems, item.JsonItems);
            //    jo.JsonItemList = item.JsonItems;
            //    var newJson = jo.SetToJson();

            //    var x = JsonHelper.ConvertJsonString(newJson);

            //    var fileName = PathHelper.GetWholeFileName(item.Path);
            //    var path = $"{SavedFolder}\\{fileName}";
            //    FileHelper.SaveFile(path, x);
            //}


            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        private void btnSetAsCB_Click(object sender, EventArgs e)
        {
            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }

            this.SavedFolder = this.txtNewJsonFolder.Text;
            if (!FileHelper.ExistFolder(this.SavedFolder))
            {
                MessageBox.Show("Saved Folder not existed");
                return;
            }
            var isEnable = this.cbEIMAS.Checked == true ? 1 : 0;
            var list = new List<object>();
            list.Add(selected);
            list.Add(isEnable);
            base.PrepareExecute();
            base.StartExecute(SetEIMASByExcel, list);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }

            if(string.IsNullOrEmpty(this.txtKey.Text)||string.IsNullOrEmpty(this.txtValue.Text)&&!this.cbIsObject.Checked)
            {
                MessageBox.Show("No Key or No Value");
                return;
            }



            //this.SavedFolder = this.txtNewJsonFolder.Text;
            //if (!FileHelper.ExistFolder(this.SavedFolder))
            //{
            //    MessageBox.Show("Saved Folder not existed");
            //    return;
            //}
            var parent = this.SelectedLastElement;
            var key = this.txtKey.Text;
            var value = this.txtValue.Text;
            var isObj = this.cbIsObject.Checked;
            var list = new List<object>();
            list.Add(selected);
            list.Add(parent);
            list.Add(key);
            list.Add(value);
            list.Add(isObj);
            base.PrepareExecute();
            base.StartExecute(AddElement, list);
        }

        private void AddElement(object obj)
        {
            var l = (List<object>)obj;

            var selected = (List<JsonFileItem>)l[0];

            var lastOne = (JsonItem)l[1];
            var key = l[2].ToString();
            var value = l[3].ToString();
            var isObj = bool.Parse(l[4].ToString());
            var ec = new NormalConfig();
            ec.NeedUpdateJsons = selected;
            ec.LastElement = lastOne;
            //ec.LastElementKey = lastOne.Key;
            ec.KeyElement = key;
            ec.ValueElement = value;
            ec.IsObject = isObj;
            ec.Set();

            //SaveUpdatedFiles(selected);

            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        private void AddSubElement(object obj)
        {
            var l = (List<object>)obj;

            var selected = (List<JsonFileItem>)l[0];

            var parentOne = (JsonItem)l[1];
            var key = l[2].ToString();
            var value = l[3].ToString();
            var isObj = bool.Parse(l[4].ToString());
            var ec = new NormalConfig();
            ec.NeedUpdateJsons = selected;
            ec.ParentElement = parentOne;
           
            ec.KeyElement = key;
            ec.ValueElement = value;
            ec.IsObject = isObj;
            ec.Set4Sub();

            //SaveUpdatedFiles(selected);

            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        private void SaveUpdatedFiles(List<JsonFileItem> selected)
        {
            var jo = new JsonOperator();
            foreach (JsonFileItem item in selected)
            {
                //jo.SetAsModel(model.JsonItems, item.JsonItems);
                jo.JsonItemList = item.JsonItems;
                var newJson = jo.SetToJson();

                var x = JsonHelper.ConvertJsonString(newJson);

                var fileName = PathHelper.GetWholeFileName(item.Path);
                var path = $"{SavedFolder}\\{fileName}";
                FileHelper.SaveFile(path, x);
            }
        }

        private JsonItem _selectedLastElement;
        private JsonItem SelectedLastElement
        {
            get { return _selectedLastElement; }
            set
            {
                _selectedLastElement = value;
                this.txtParent.Text = _selectedLastElement==null?"": _selectedLastElement.Key;
            }
        }

        private JsonItem _selectedCurrentElement;
        public JsonItem SelectedCurrentElement
        {
            get { return _selectedCurrentElement; }
            set
            {
                _selectedCurrentElement = value;
                this.txtKey.Text = _selectedCurrentElement == null ? "" : _selectedCurrentElement.Key;
            }
        }

        private void btnSetParent_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedTN = this.tvJsonMain.SelectedNode;

                if (selectedTN == null)
                {
                    return;
                }

                var ids = selectedTN.Tag.ToString().Split('|');
                var fileId = ids[0];
                var elementId = ids[1];
                var file = this.JsonFileList.FirstOrDefault(x => x.Id == fileId);

                var element = file.JsonItems.FirstOrDefault(x => x.Id == elementId && !x.IsDeleted);
                var parentElement = file.JsonItems.FirstOrDefault(x => x.Id == element.ParentId && !x.IsDeleted);
                element.ParentKey = parentElement == null ? "" : parentElement.Key;
                SelectedLastElement = element;
            }
            catch
            {
                MessageBox.Show("No element selected.");
            }
        }

        private void btnSelectKey_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedTN = this.tvJsonMain.SelectedNode;

                if (selectedTN == null)
                {
                    return;
                }

                var ids = selectedTN.Tag.ToString().Split('|');
                var fileId = ids[0];
                var elementId = ids[1];
                var file = this.JsonFileList.FirstOrDefault(x => x.Id == fileId);
                var element = file.JsonItems.FirstOrDefault(x => x.Id == elementId && !x.IsDeleted);
                var parentElement = file.JsonItems.FirstOrDefault(x => x.Id == element.ParentId && !x.IsDeleted);
                element.ParentKey = parentElement == null ? "" : parentElement.Key;
                SelectedCurrentElement = element;
            }
            catch
            {
                MessageBox.Show("No element selected.");
            }
        }

        private void cbIsObject_CheckedChanged(object sender, EventArgs e)
        {
            this.txtValue.Enabled = !this.cbIsObject.Checked;
        }

        private void btnRemoveElement_Click(object sender, EventArgs e)
        {
            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }

            if (string.IsNullOrEmpty(this.txtKey.Text))
            {
                MessageBox.Show("No Key or No Value");
                return;
            }

            //this.SavedFolder = this.txtNewJsonFolder.Text;
            //if (!FileHelper.ExistFolder(this.SavedFolder))
            //{
            //    MessageBox.Show("Saved Folder not existed");
            //    return;
            //}
            //var parent = this.txtParent.Text;
            var key = this.SelectedCurrentElement;
            //var value = this.txtValue.Text;
            //var isObj = this.cbIsObject.Checked;
            var list = new List<object>();
            list.Add(selected);
            list.Add(key);
            base.PrepareExecute();
            base.StartExecute(DeleteElement, list);
        }

        private void DeleteElement(object obj)
        {

            var l = (List<object>)obj;

            var selected = (List<JsonFileItem>)l[0];


            var key = (JsonItem)l[1];
            var ec = new NormalConfig();
            ec.NeedUpdateJsons = selected;       
            ec.CurrentElement = key;
            ec.Delete();

            //SaveUpdatedFiles(selected);


            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        private void btnAddSub_Click(object sender, EventArgs e)
        {
            if(this.SelectedLastElement==null||this.SelectedLastElement.IsDeleted||!this.SelectedLastElement.IsObject)
            {
                MessageBox.Show("Not an object element");
                return;
            }

            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }
            if (string.IsNullOrEmpty(this.txtKey.Text) || string.IsNullOrEmpty(this.txtValue.Text) && !this.cbIsObject.Checked)
            {
                MessageBox.Show("No Key or No Value");
                return;
            }

            //this.SavedFolder = this.txtNewJsonFolder.Text;
            //if (!FileHelper.ExistFolder(this.SavedFolder))
            //{
            //    MessageBox.Show("Saved Folder not existed");
            //    return;
            //}
            var parent = this.SelectedLastElement;
            var key = this.txtKey.Text;
            var value = this.txtValue.Text;
            var isObj = this.cbIsObject.Checked;
            var list = new List<object>();
            list.Add(selected);
            list.Add(parent);
            list.Add(key);
            list.Add(value);
            list.Add(isObj);
            base.PrepareExecute();
            base.StartExecute(AddSubElement, list);
        }

        private void tvJsonMain_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAddGroupSub_Click(object sender, EventArgs e)
        {
            if (this.SelectedLastElement == null || this.SelectedLastElement.IsDeleted || !this.SelectedLastElement.IsObject||string.IsNullOrEmpty(this.txtGrouped.Text))
            {
                MessageBox.Show("Not an object element");
                return;
            }

            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }
            var str = this.txtGrouped.Text;
            if(!SetFragment(str))
            {
                return;
            }

            var parent = this.SelectedLastElement;         
            var isObj = this.cbIsObject.Checked;
            var list = new List<object>();
            list.Add(selected);
            list.Add(parent);
            list.Add(isObj);
            base.PrepareExecute();
            base.StartExecute(AddFragmentSubElement, list);
        }

        private void AddFragmentSubElement(object obj)
        {
            var l = (List<object>)obj;

            var selected = (List<JsonFileItem>)l[0];

            var parentOne = (JsonItem)l[1];         
            var isObj = bool.Parse(l[2].ToString());
            var ec = new NormalConfig();
            ec.NeedUpdateJsons = selected;
            ec.ParentElement = parentOne;
            ec.Fragment = this.Fragment;
            ec.IsObject = isObj;
            ec.SetFragment4Sub();

            //SaveUpdatedFiles(selected);

            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        private void btnAddGroupLast_Click(object sender, EventArgs e)
        {
            var selected = JsonFileList.Where(x => x.IsSeleted && x.IsMode == false).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No Model or No Selected");
                return;
            }

            if (this.SelectedLastElement == null || this.SelectedLastElement.IsDeleted || !this.SelectedLastElement.IsObject || string.IsNullOrEmpty(this.txtGrouped.Text))
            {
                MessageBox.Show("Not an object element");
                return;
            }

            var str = this.txtGrouped.Text;
            if (!SetFragment(str))
            {
                return;
            }


            var parent = this.SelectedLastElement;
            var isObj = this.cbIsObject.Checked;
            var list = new List<object>();
            list.Add(selected);
            list.Add(parent);
            list.Add(isObj);
            base.PrepareExecute();
            base.StartExecute(AddFragmentElement, list);
        }

        private void AddFragmentElement(object obj)
        {
            var l = (List<object>)obj;

            var selected = (List<JsonFileItem>)l[0];

            var lastOne = (JsonItem)l[1];
            var isObj = bool.Parse(l[2].ToString());
            var ec = new NormalConfig();
            ec.NeedUpdateJsons = selected;
            ec.LastElement = lastOne;
            //ec.LastElementKey = lastOne.Key;
            ec.Fragment = this.Fragment;
            ec.IsObject = isObj;
            ec.SetFragment4Last();

            //SaveUpdatedFiles(selected);

            this.BeginInvoke(new DeleParaNone(BindJsonFileItemAction));

            base.EndExecute(null);
        }

        private List<JsonItem> Fragment
        {
            get;set;
        }
        private bool SetFragment(string str)
        {
            try
            {
                str = $"{{{str}}}";
                Fragment = new List<JsonItem>();
                JsonOperator jo = new JsonOperator();
                jo.SetJsonToItemList(str);
                Fragment = jo.JsonItemList;
                return true;
            }
            catch
            {
                MessageBox.Show("转换失败");
            }
            return false;
        }
    }
}
