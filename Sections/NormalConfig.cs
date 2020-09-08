using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBJ.Divorced.Define;
using DBJ.Divorced.Helper;

namespace DBJ.Tool.Json.Sections
{
    public class NormalConfig
    {
        public List<List<KeyValue>> ParameterList { get; set; }

        public List<JsonFileItem> NeedUpdateJsons { get; set; }

        public JsonItem LastElement { get; set; }

        //public string LastElementKey { get; set; }

        public string KeyElement { get; set; }

        public List<JsonItem> Fragment { get; set; }

        public string ValueElement { get; set; }

        public bool IsObject { get; internal set; }

        public JsonItem ParentElement { get; internal set; }
        public JsonItem CurrentElement { get; internal set; }

        //public string ParentElementKey { get; internal set; }

        public void Set()
        {
            foreach (JsonFileItem jfi in NeedUpdateJsons)
            {
                string parentId = null;
                var lastIndex = 0;
                //Get last json item
                var lastJis = jfi.JsonItems.Where(x => x.Key == LastElement.Key && !x.IsDeleted);
                JsonItem lastJi=null;
                foreach (var item in lastJis)
                {
                    //如果父为空，那就是最外层
                    if (item.ParentId == "" && this.LastElement.ParentKey == "")
                    {
                        lastJi = item;
                        parentId = "";
                        break;
                    }
                    var parent = jfi.JsonItems.FirstOrDefault(x => x.Id == item.ParentId && !x.IsDeleted);

                    if(parent!=null&& parent.Key == LastElement.ParentKey)
                    {
                        lastJi = item;
                        parentId = parent.Id;
                        break;
                    }
                }

                if(parentId == null)
                {
                    System.Diagnostics.Debug.WriteLine($"************ERROR:{jfi.Path}  ELEMENTNOTFOUND:{LastElement.Key}");
                    continue;
                }

                lastIndex = lastJi.Order;
                parentId = lastJi.ParentId;
                
                //Deleted Existed
                var isExisted = jfi.JsonItems.FirstOrDefault(x => x.ParentId == parentId && x.Key == KeyElement && !x.IsDeleted);
                if(isExisted != null)
                {
                    isExisted.IsDeleted = true;
                }
                jfi.ResetOrder(parentId, lastIndex);
                //Create new one
                var ji = new JsonItem();
                ji.Id = Guid.NewGuid().ToString();
                ji.ParentId = parentId;
                ji.Key = KeyElement;
                ji.Order = lastIndex + 1;
                ji.IsObject = IsObject;
                ji.Value = SetValue(jfi,ValueElement);
                jfi.JsonItems.Add(ji);
            }
        }

        public void SetFragment4Last()
        {
            foreach (JsonFileItem jfi in NeedUpdateJsons)
            {
                string parentId = null;
                var lastIndex = 0;
                //Get last json item
                var lastJis = jfi.JsonItems.Where(x => x.Key == LastElement.Key && !x.IsDeleted);
                JsonItem lastJi = null;
                foreach (var item in lastJis)
                {
                    //如果父为空，那就是最外层
                    if (item.ParentId == "" && this.LastElement.ParentKey == "")
                    {
                        lastJi = item;
                        parentId = "";
                        break;
                    }
                    var parent = jfi.JsonItems.FirstOrDefault(x => x.Id == item.ParentId && !x.IsDeleted);

                    if (parent != null && parent.Key == LastElement.ParentKey)
                    {
                        lastJi = item;
                        parentId = parent.Id;
                        break;
                    }
                }

                if (parentId == null)
                {
                    System.Diagnostics.Debug.WriteLine($"************ERROR:{jfi.Path}  ELEMENTNOTFOUND:{LastElement.Key}");
                    continue;
                }

                lastIndex = lastJi.Order;
                parentId = lastJi.ParentId;
                var newFragment = this.Fragment.Clone<JsonItem>();
                ReplaceValue4Fragment(jfi, newFragment);
                foreach (var fr in newFragment.Where(x => x.ParentId == "" && !x.IsDeleted))
                {
                    var isExisted = jfi.JsonItems.FirstOrDefault(x => x.ParentId == parentId && x.Key == fr.Key && !x.IsDeleted);
                    if (isExisted != null)
                    {
                        isExisted.IsDeleted = true;
                    }
                    jfi.ResetOrder(parentId, lastIndex);
                    //Create new one
                    fr.ParentId = parentId;
                    fr.Order = lastIndex + 1;

                }
                jfi.JsonItems.AddRange(newFragment);
                //Deleted Existed
            }
        }

        private string SetValue(JsonFileItem jfi, string value)
        {
            value = value.Replace("{R}", jfi.Release == null ? "" : jfi.Release.ToUpper());
            value = value.Replace("{N}", jfi.Number == null ? "" : jfi.Number.ToUpper());
            value = value.Replace("{I}", jfi.InstanceName == null ? "" : jfi.InstanceName.ToUpper());
            value = value.Replace("{r}", jfi.Release == null ? "" : jfi.Release.ToLower());
            value = value.Replace("{n}", jfi.Number == null ? "" : jfi.Number.ToLower());
            value = value.Replace("{i}", jfi.InstanceName == null ? "" : jfi.InstanceName.ToLower());
            return value;
        }

        public void Set4Sub()
        {
            foreach (JsonFileItem jfi in NeedUpdateJsons)
            {
                string parentId = null;
                var lastIndex = 0;
                //Get last json item
                var parentJis = jfi.JsonItems.Where(x => x.Key == ParentElement.Key && !x.IsDeleted);
                JsonItem parentJi = null;
                foreach (var item in parentJis)
                {
                    //如果父为空，那就是最外层
                    if (item.ParentId == "" && this.ParentElement.ParentKey == "")
                    {
                        parentId = parentJi.Id;
                        parentJi = item;
                        break;
                    }
                    var tmpParentJi = jfi.JsonItems.FirstOrDefault(x => x.Id == item.ParentId && !x.IsDeleted);
                    if (tmpParentJi != null && tmpParentJi.Key == ParentElement.ParentKey)
                    {
                        parentJi = item;
                        parentId = parentJi.Id;
                        break;
                    }
                }

                if (parentId == null)
                {
                    System.Diagnostics.Debug.WriteLine($"************ERROR:{jfi.Path}  ELEMENTNOTFOUND:{ParentElement.Key}");
                    continue;
                }
                //lastIndex = lastJi.Order; 

                //Deleted Existed
                var isExisted = jfi.JsonItems.FirstOrDefault(x => x.ParentId == parentId && x.Key == KeyElement && !x.IsDeleted);
                if (isExisted != null)
                {
                    isExisted.IsDeleted = true;
                }
                jfi.ResetOrder(parentId, lastIndex);
                //Create new one
                var ji = new JsonItem();
                ji.Id = Guid.NewGuid().ToString();
                ji.ParentId = parentId;
                ji.Key = KeyElement;
                ji.Order = 1;
                ji.IsObject = IsObject;
                ji.Value = SetValue(jfi, ValueElement);
                jfi.JsonItems.Add(ji);
            }
        }

        public void SetFragment4Sub()
        {
            foreach (JsonFileItem jfi in NeedUpdateJsons)
            {
                string parentId = null;
                var lastIndex = 0;
                //Get last json item
                var parentJis = jfi.JsonItems.Where(x => x.Key == ParentElement.Key && !x.IsDeleted);
                JsonItem parentJi = null;
                foreach (var item in parentJis)
                {
                    //如果父为空，那就是最外层
                    if (item.ParentId == "" && this.ParentElement.ParentKey == "")
                    {                        
                        parentJi = item;
                        parentId = parentJi.Id;
                        break;
                    }
                    var tmpParentJi = jfi.JsonItems.FirstOrDefault(x => x.Id == item.ParentId && !x.IsDeleted);
                    if (tmpParentJi != null && tmpParentJi.Key == ParentElement.ParentKey)
                    {
                        parentJi = item;
                        parentId = parentJi.Id;
                        break;
                    }
                }

                if (parentId == null)
                {
                    System.Diagnostics.Debug.WriteLine($"************ERROR:{jfi.Path}  ELEMENTNOTFOUND:{ParentElement.Key}");
                    continue;
                }
                //lastIndex = lastJi.Order; 
                var newFragment = this.Fragment.Clone<JsonItem>();
                ReplaceValue4Fragment(jfi, newFragment);
                foreach (var fr in newFragment.Where(x=>x.ParentId=="" && !x.IsDeleted))
                {
                    //Deleted
                    var isExisted = jfi.JsonItems.FirstOrDefault(x => x.ParentId == parentId && x.Key == fr.Key && !x.IsDeleted);
                    if (isExisted != null)
                    {
                        isExisted.IsDeleted = true;
                    }

                    jfi.ResetOrder(parentId, lastIndex);
                    //Create new one
                    fr.ParentId = parentId;
                    fr.Order = 1;
                }
                jfi.JsonItems.AddRange(newFragment);
                //Deleted Existed
               
            }
        }

        private void ReplaceValue4Fragment(JsonFileItem jfi, List<JsonItem> fragments)
        {
            foreach (var fragment in fragments)
            {
                if(!fragment.IsObject&&!fragment.IsDeleted)
                {
                    fragment.Value = SetValue(jfi, fragment.Value);
                }
            }
        }
      
        public void Delete()
        {
            foreach (JsonFileItem jfi in NeedUpdateJsons)
            {    
                //Deleted Existed
                var isExisted = jfi.JsonItems.Where(x => x.Key == CurrentElement.Key && !x.IsDeleted);
                string parentId = null;
                foreach (var item in isExisted)
                {
                    //如果父为空，那就是最外层
                    if (item.ParentId == "" && this.CurrentElement.ParentKey == "")
                    {
                        parentId = item.ParentId;
                        break;
                    }
                    var parent = jfi.JsonItems.FirstOrDefault(x => x.Id == item.ParentId && !x.IsDeleted);
                    if (parent.Key == CurrentElement.ParentKey)
                    {
                        parentId = item.ParentId;
                        break;
                    }
                }               
                if(parentId==null)
                {
                    continue;
                }
                var ji = jfi.JsonItems.FirstOrDefault(x => x.Key == CurrentElement.Key&&x.ParentId== parentId && !x.IsDeleted);
                ji.IsDeleted = true;
            }
        }
    }
}
