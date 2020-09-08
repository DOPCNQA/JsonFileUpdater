using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DBJ.Controls;
using DBJ.Divorced.Define;
using DBJ.Tool.Json.Sections;

namespace DBJ.Tool.Json
{
    public partial class frmManageERMS : Form
    {
        public frmManageERMS()
        {
            InitializeComponent();
        }

        public List<ParameterItem> ParameterList
        {
            get; set;
        }

        public List<ParameterItem> ValueList
        {
            get; set;
        }
        public List<JsonFileItem> NeedUpdateJsons
        {
            get; set;
        }

        private void frmManageERMS_Load(object sender, EventArgs e)
        {
            
        }

        ErmsConfig ermsConfig;
        public void BindNodes(string parentId)
        {
            ermsConfig = new ErmsConfig(parentId);
            this.treeView1.Nodes.Clear();

            foreach (var item in ermsConfig.DefaultNormalERMSConfig)
            {
                if (item.ParentId != parentId)
                    continue;
                var tn = new TreeNode();
                tn.Text = item.Key;
                tn.Tag = item.Id;
                this.treeView1.Nodes.Add(tn);
                //if(item.Key == "ResultsConsumer")
                //{
                //    foreach (var subItem in ermsConfig.DefaultResultConsumerConfig)
                //    {
                //        var subTn = new TreeNode();
                //        subTn.Text = subItem.Key;
                //        tn.Nodes.Add(subTn);
                //    }
                //}
            }
            this.treeView1.ExpandAll();
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            var tn = this.treeView1.SelectedNode; 
            var id = tn.Tag.ToString();

            var list = this.ermsConfig.DefaultNormalERMSConfig.Where(x => x.ParentId == id).ToList();

            this.pnlLeft.Controls.Clear();
            foreach (var item in list)
            {
                var fv = new FieldVariable();
                fv.Id = item.Id;
                fv.Name = item.Key;
                fv.InitList(this.ValueList, this.ParameterList);
                fv.Dock = DockStyle.Top;
                this.pnlLeft.Controls.Add(fv);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode tn in this.treeView1.Nodes)
            {
                tn.Checked = true;
            }            
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var isErms = this.rbERMS.Checked;
            var selectedEndpoints = new List<string>();
            foreach (TreeNode tn in this.treeView1.Nodes)
            {
                if (tn.Checked)
                    selectedEndpoints.Add(tn.Tag.ToString());
            }

            RemoveAllEndpoints();

            foreach (var needUpdatJson in this.NeedUpdateJsons)
            {
                Add41Endpoint(needUpdatJson, selectedEndpoints, isErms);
            }

            MessageBox.Show("DONE");
        }

        private void Add41Endpoint(JsonFileItem jfi, List<string> selectedEndpints, bool isErms)
        {
            var jId = jfi.JsonItems.FirstOrDefault(x => x.Key == "id" && !x.IsDeleted && !x.IsObject);
            var idValue = jId.Value;
            var strs = idValue.Split('_');
            var sender = "";
            for (int i = 1; i < strs.Length; i++)
            {
                sender += strs[i].ToUpper();
            }

            foreach (var eId in selectedEndpints)
            {
                var ermsBase = jfi.JsonItems.FirstOrDefault(x => x.Key == "erms_config" && !x.IsDeleted);
                //var ji = new JsonItem();
                //ji.Id = Guid.NewGuid().ToString();
                //ji.IsObject = true;
                //ji.ParentId = ermsBase.Id;
                //jfi.JsonItems.Add(ji);
                jfi.JsonItems.AddRange(this.ermsConfig.GetJsonItemsByIdWith(ermsBase.Id, eId, isErms, sender));
            }
        }

        private void RemoveAllEndpoints()
        {
            foreach (var nuj in this.NeedUpdateJsons)
            {
                var ermsBase = nuj.JsonItems.FirstOrDefault(x => x.Key == "erms_config" && !x.IsDeleted);
                //var ermsElement = nuj.JsonItems.FirstOrDefault(x => x.ParentId == ermsBase.Id && x.IsObject && !x.IsDeleted);
                RemoveEndpoints(nuj, ermsBase);
                ermsBase.IsDeleted = false;
            }
        }

        private void RemoveEndpoints(JsonFileItem jfi, JsonItem ji)
        {
            var jis = jfi.JsonItems.Where(x => x.ParentId == ji.Id && !ji.IsDeleted);

            foreach (var item in jis)
            {
                RemoveEndpoints(jfi, item);
            }
            ji.IsDeleted = true;
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            foreach (TreeNode tn in this.treeView1.Nodes)
            {
                tn.Checked = !tn.Checked;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSetAsDefault_Click(object sender, EventArgs e)
        {

        }
    }
}
