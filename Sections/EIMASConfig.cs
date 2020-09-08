using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBJ.Divorced.Define;

namespace DBJ.Tool.Json.Sections
{
    public class EIMASConfig
    {
        public List<List<KeyValue>> ParameterList { get; set; }

        public List<JsonFileItem> NeedUpdateJsons { get; set; }


        private List<KeyValue> GetPara(string fileName)
        {
            foreach (var item in ParameterList)
            {
                //FileName
                var fileNameObj = item.FirstOrDefault(x => x.Key == "FileName").Value;
                if (fileName == fileName)
                {
                    return item;
                }
            }
            return null;
        }

        public string SetEIMAS(int isEnable=2)
        {
            var errorMessage = "";
            foreach (var jfi in NeedUpdateJsons)
            {
                var instance = jfi.JsonItems.FirstOrDefault(x => x.Id == "id").Value;
                var instanceParaList = GetPara(jfi.Name);
                if (instanceParaList == null)
                {
                    errorMessage += $"Instance：{instance} 没有EXCEL里找到参数";
                    continue;
                }               
               

                RemoveEIMASElements(jfi);

                var needEIMAS = instanceParaList.FirstOrDefault(x => x.Key == "enableEIMAS").Value; 
                if(isEnable==0)
                {
                    needEIMAS = "false";

                }
                else if(isEnable==1)
                {
                    needEIMAS = "true";
                }

                var jiParentId = jfi.JsonItems.FirstOrDefault(x => x.Key == "eol_config" && !x.IsDeleted);
                var ji = jfi.JsonItems.FirstOrDefault(x => x.Key == "enableEIMAS" && !x.IsDeleted);
                if (needEIMAS.ToLower() == "true")
                {
                    if (ji != null)
                        ji.Value = "true";
                    else
                    {
                        var newJfi = new JsonItem();
                        newJfi.Id = Guid.NewGuid().ToString();
                        newJfi.Key = "enableEIMAS";
                        newJfi.Order = jfi.JsonItems.Count + 1;
                        newJfi.Value = "true";
                        newJfi.ParentId = jiParentId.Id;
                        jfi.JsonItems.Add(newJfi);
                    }
                    if (jfi.IsMode)
                    {
                        AddEIMASElementsForMode(jfi, jiParentId.Id, instanceParaList);
                    }
                    else
                    {
                        AddEIMASElements(jfi, jiParentId.Id, instanceParaList);
                    }
                   
                }
                else
                {
                    if (ji != null)
                        ji.Value = "false";
                    else
                    {
                        var newJfi = new JsonItem();
                        newJfi.Id = Guid.NewGuid().ToString();
                        newJfi.Key = "enableEIMAS";
                        newJfi.Order = jfi.JsonItems.Count + 1;
                        newJfi.Value = "false";
                        newJfi.ParentId = jiParentId.Id;
                        jfi.JsonItems.Add(newJfi);
                    }
                }
            }
            return errorMessage;
        }
        //                                "APIBaseURL": "#EIMASAPIBaseUrl#",
        //                        "ida_clientId": "#EIMASClientId#",
        //                        "ida_appKey": "#EIMASAppKey#",
        //                        "ida_metadataAddress": "#EIMASUrl#/adfs/.well-known/openid-configuration",
        //                        "ida_redirectUri": "https://qa-#Release#-#Instance##SiteNumber#-testing.eol-test.eurofins.local/",
        //                        "ida_postLogoutRedirectUri": "https://qa-#Release#-#Instance##SiteNumber#-testing.eol-test.eurofins.local/Impersonation/Stopimpersonate",
        //                        "ida_authority": "#EIMASUrl#/adfs",

        private void AddEIMASElementsForMode(JsonFileItem jfi, string parentId, List<KeyValue> paraList)
        {
          
            var newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "APIBaseURL",
                Order = jfi.JsonItems.Count + 1,
                Value = "#EIMASAPIBaseUrl#",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_clientId",
                Order = jfi.JsonItems.Count + 1,
                Value = "#EIMASClientId#",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_appKey",
                Order = jfi.JsonItems.Count + 1,
                Value = "#EIMASAppKey#",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
            var eimasUrl = paraList.FirstOrDefault(x => x.Key == "EIMASUrl").Value;
            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_metadataAddress",
                Order = jfi.JsonItems.Count + 1,
                Value = "#EIMASUrl#/adfs/.well-known/openid-configuration",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);

            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_redirectUri",
                Order = jfi.JsonItems.Count + 1,
                Value = "https://qa-#Release#-#Instance##SiteNumber#-testing.eol-test.eurofins.local/",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);

            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_postLogoutRedirectUri",
                Order = jfi.JsonItems.Count + 1,
                Value = "https://qa-#Release#-#Instance##SiteNumber#-testing.eol-test.eurofins.local/Impersonation/Stopimpersonate",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);

            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_authority",
                Order = jfi.JsonItems.Count + 1,
                Value = "#EIMASUrl#/adfs",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
        }

        private void AddEIMASElements(JsonFileItem jfi,string parentId, List<KeyValue> paraList)
        {
            var value = paraList.FirstOrDefault(x => x.Key == "EIMASAPIBaseUrl").Value;
            var newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "APIBaseURL",
                Order = jfi.JsonItems.Count + 1,
                Value = value,
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
            value = paraList.FirstOrDefault(x => x.Key == "EIMASClientId").Value;
            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_clientId",
                Order = jfi.JsonItems.Count + 1,
                Value = value,
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
            value = paraList.FirstOrDefault(x => x.Key == "EIMASAppKey").Value;
            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_appKey",
                Order = jfi.JsonItems.Count + 1,
                Value = value,
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
            var eimasUrl = paraList.FirstOrDefault(x => x.Key == "EIMASUrl").Value;
            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_metadataAddress",
                Order = jfi.JsonItems.Count + 1,
                Value = $"{eimasUrl}/adfs/.well-known/openid-configuration",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);

            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_redirectUri",
                Order = jfi.JsonItems.Count + 1,
                Value = $"https://qa-{jfi.Release}-{jfi.InstanceName}{jfi.Number}-testing.eol-test.eurofins.local/",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);

            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_postLogoutRedirectUri",
                Order = jfi.JsonItems.Count + 1,
                Value = $"https://qa-{jfi.Release}-{jfi.InstanceName}{jfi.Number}-testing.eol-test.eurofins.local/Impersonation/Stopimpersonate",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);

            newJfi = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "ida_authority",
                Order = jfi.JsonItems.Count + 1,
                Value = $"{eimasUrl}/adfs",
                ParentId = parentId
            };
            jfi.JsonItems.Add(newJfi);
        }

        private void RemoveEIMASElements(JsonFileItem jfi)
        {
            var list = new List<string>
            {
                "APIBaseURL",
                "ida_clientId",
                "ida_appKey",
                "ida_metadataAddress",
                "ida_redirectUri",
                "ida_postLogoutRedirectUri",
                "ida_authority"
            };

            foreach (var item in list)
            {
                var needToRemove = jfi.JsonItems.FirstOrDefault(x => x.Key == item && !x.IsDeleted);
                if (needToRemove != null)
                    needToRemove.IsDeleted = true;
            }
        }

        public void SetModel()
        {
            //"APIBaseURL": "#EIMASAPIBaseUrl#",
            //                    "ida_clientId": "#EIMASClientId#",
            //                    "ida_appKey": "#EIMASAppKey#",
            //                    "ida_metadataAddress": "#EIMASUrl#/adfs/.well-known/openid-configuration",
            //                    "ida_redirectUri": "https://qa-#Release#-#Instance##SiteNumber#-testing.eol-test.eurofins.local/",
            //                    "ida_postLogoutRedirectUri": "https://qa-#Release#-#Instance##SiteNumber#-testing.eol-test.eurofins.local/Impersonation/Stopimpersonate",
            //                    "ida_authority": "#EIMASUrl#/adfs",

        }


    }
}
