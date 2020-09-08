using System;
using System.Collections.Generic;
using System.Linq;
using DBJ.Divorced.Define;
using DBJ.Divorced.Helper;

namespace DBJ.Tool.Json.Sections
{
    public class ErmsConfig
    {
        public string NodeName
        {
            get { return "erms_config"; }
        }

        public List<JsonItem> JsonItems { get; set; }

        private string Parent { get; set; }

        public ErmsConfig(List<JsonItem> jsonItems)
        {
            this.JsonItems = jsonItems;
        }


        public ErmsConfig(string parentId)
        {
            this.Parent = parentId;
        }

        public List<JsonItem> GetJsonItemsByIdWith(string pId, string eId, bool isErms, string sender)
        {
            var list = new List<JsonItem>();
            var tmpNormalErmsConfig = DefaultNormalERMSConfig.Clone<JsonItem>();
            var endPointItem = tmpNormalErmsConfig.FirstOrDefault(x => x.Id == eId);
            endPointItem.ParentId = pId;
            list.Add(endPointItem);
            SetDefaultValue4Endpoints(eId, isErms, sender, list, tmpNormalErmsConfig);
            //list.AddRange(DefaultNormalERMSConfig.Where(x => x.ParentId == eId).ToList());
            return list;
        }

        public void SetDefaultValue4Endpoints(string endpointId, bool isErms, string sender, List<JsonItem> list, List<JsonItem> orgList)
        {
            switch (endpointId)
            {
                case "f1f3e647-028d-4d3d-b59c-7bd445384c75":
                    Set4ResultsConsumer("f1f3e647-028d-4d3d-b59c-7bd445384c75", sender,list,orgList);
                    break;
                case "28eb47e9-19e8-4761-bbe7-e53e711b9f54":
                    Set4AccountsConsumer("28eb47e9-19e8-4761-bbe7-e53e711b9f54", sender,list, orgList);
                    break;
                case "de013f9c-d8da-4e69-8ffd-d1e101875ce8":
                    Set4StatusUpdateAlerts("de013f9c-d8da-4e69-8ffd-d1e101875ce8", list, orgList);
                    break;
                case "fa742054-d244-4290-8be5-ba535d51cf11":
                //ORDERSFROMEOLXML
                case "3b14c2b0-5e98-43d1-ad0d-043d5cd2c162":
                //NOTIFTOEOLXML
                case "a32ee615-7902-477b-8347-f3109dd9b2e8":
                //ORDERSXML
                case "a8f7c37f-a107-402e-8ad2-1808da7fff7c":
                //ORDERPROCESS
                case "60bbb40c-bd2d-4c2f-8f99-dbe90808d9e1":
                //RESULTSXML
                default:
                    Set4Normal(endpointId, isErms, sender,list, orgList);
                    break;
            }
        }

        private string GetUrl(bool isErms)
        {
            return isErms ? "tcp://CN04MQVT002.area1.eurofins.local:61616" : "tcp://CN04MQVT002.area1.eurofins.local:61636";
        }

        private string GetMyUser(bool isErms)
        {
            return isErms ? "system" : "eesbmini";
        }

        private string GetMessagingPlatform(bool isErms)
        {
            return isErms ? "ERMS" : "EESB";
        }

        private void Set4Normal(string id, bool isErms, string sender, List<JsonItem> list, List<JsonItem> orgList)
        {
            //var normalObj = this.DefaultNormalERMSConfig.FirstOrDefault(x => x.ParentId == id && !x.IsDeleted&&x.IsObject);
            var normalList = orgList.Where(x => x.ParentId == id && !x.IsDeleted).ToList();

            var eUrl = normalList.FirstOrDefault(x => x.Key == "Url" && !x.IsDeleted);
            eUrl.Value = GetUrl(isErms);
            var platform = normalList.FirstOrDefault(x => x.Key == "MessagingPlatform" && !x.IsDeleted);
            platform.Value = GetMessagingPlatform(isErms);
            var myUser = normalList.FirstOrDefault(x => x.Key == "mqUser" && !x.IsDeleted);
            myUser.Value = GetMyUser(isErms);
            var user = normalList.FirstOrDefault(x => x.Key == "User" && !x.IsDeleted);
            user.Value = sender;
            list.AddRange(normalList);
        }

        private void Set4ResultsConsumer(string id, string sender, List<JsonItem> list, List<JsonItem> orgList)
        {
            var normalObj = orgList.FirstOrDefault(x => x.Id == id && !x.IsDeleted && x.IsObject);
            var normalList = orgList.Where(x => x.ParentId == normalObj.Id && !x.IsDeleted).ToList();

            var sample = normalList.FirstOrDefault(x => x.Key == "Sample" && !x.IsDeleted);
            var sampleQ = $"RESULTSXML.IN.{sender}.ReceiveSampleInformation.V1_0";
            SetResultConsumerDetail(sample.Id, sampleQ,list,orgList);

            var result = normalList.FirstOrDefault(x => x.Key == "Result" && !x.IsDeleted);
            var resultQ = $"RESULTSXML.IN.{sender}.ReceiveResultInformation.V1_0";
            SetResultConsumerDetail(result.Id, resultQ,list, orgList);

            var ResultNG42 = normalList.FirstOrDefault(x => x.Key == "ResultNG42" && !x.IsDeleted);
            var ResultNG42Q = $"RESULTSXML.IN.{sender}.ReceiveResultInformation.V2_0";
            SetResultConsumerDetail(ResultNG42.Id, ResultNG42Q,list, orgList);

            var SampleNG43 = normalList.FirstOrDefault(x => x.Key == "SampleNG43" && !x.IsDeleted);
            var SampleNG43Q = $"RESULTSXML.IN.{sender}.ReceiveSampleInformation.V3_0";
            SetResultConsumerDetail(SampleNG43.Id, SampleNG43Q,list, orgList);

            var ResultNG43 = normalList.FirstOrDefault(x => x.Key == "ResultNG43" && !x.IsDeleted);
            var ResultNG43Q = $"RESULTSXML.IN.{sender}.ReceiveResultInformation.V3_0";
            SetResultConsumerDetail(ResultNG43.Id, ResultNG43Q,list, orgList);

            var platform = normalList.FirstOrDefault(x => x.Key == "MessagingPlatform" && !x.IsDeleted);
            platform.Value = "EESB";

            var EESBScope = normalList.FirstOrDefault(x => x.Key == "EESBScope" && !x.IsDeleted);
            EESBScope.Value = "EOL";

            var EESBEnvironment = normalList.FirstOrDefault(x => x.Key == "EESBEnvironment" && !x.IsDeleted);
            EESBEnvironment.Value = "CNQA";

            var EESBBrokerUri = normalList.FirstOrDefault(x => x.Key == "EESBBrokerUri" && !x.IsDeleted);
            EESBBrokerUri.Value = "tcp://CN04MQVT002.area1.eurofins.local:61636";

            var EESBUserName = normalList.FirstOrDefault(x => x.Key == "EESBUserName" && !x.IsDeleted);
            EESBUserName.Value = "eesbmini";
            list.AddRange(normalList);
        }


        private void SetResultConsumerDetail(string id, string q, List<JsonItem> list, List<JsonItem> orgList)
        {
            var normalObj = orgList.FirstOrDefault(x => x.Id == id && !x.IsDeleted && x.IsObject);
            var normalList = orgList.Where(x => x.ParentId == normalObj.Id && !x.IsDeleted).ToList();

            var queue = normalList.FirstOrDefault(x => x.Key == "queue" && !x.IsDeleted);
            queue.Value = q;

            var brokerUri = normalList.FirstOrDefault(x => x.Key == "brokerUri" && !x.IsDeleted);
            brokerUri.Value = "tcp://CN04MQVT002.area1.eurofins.local:61636";

            var environment = normalList.FirstOrDefault(x => x.Key == "environment" && !x.IsDeleted);
            environment.Value = "CNQA";

            var user = normalList.FirstOrDefault(x => x.Key == "user" && !x.IsDeleted);
            user.Value = "eesbmini";

            list.AddRange(normalList);
        }

        private void Set4AccountsConsumer(string id, string sender, List<JsonItem> list, List<JsonItem> orgList)
        {
            var normalObj = orgList.FirstOrDefault(x => x.Id == id && !x.IsDeleted && x.IsObject);
            var normalList = orgList.Where(x => x.ParentId == normalObj.Id && !x.IsDeleted).ToList();
            var ele1 = normalList.FirstOrDefault(x => x.Key == "MessagingPlatform" && !x.IsDeleted);
            ele1.Value = "EESB";
            var ele2 = normalList.FirstOrDefault(x => x.Key == "EESBScope" && !x.IsDeleted);
            ele2.Value = "CNQA";
            var ele3 = normalList.FirstOrDefault(x => x.Key == "EESBEnvironment" && !x.IsDeleted);
            ele3.Value = sender;
            var ele4 = normalList.FirstOrDefault(x => x.Key == "EESBBrokerUri" && !x.IsDeleted);
            ele4.Value = "tcp://CN04MQVT002.area1.eurofins.local:61636";
            var ele5 = normalList.FirstOrDefault(x => x.Key == "EESBUserName" && !x.IsDeleted);
            ele5.Value = "eesbmini";
            var ele6 = normalList.FirstOrDefault(x => x.Key == "AccountWriteServiceURI" && !x.IsDeleted);
            ele6.Value = @"http://localhost:8004/api/accounts";
            //"MessagingPlatform": "EESB",
            //"EESBScope": "CI",
            //"EESBEnvironment": "#TEMPLATE#",
            //"EESBBrokerUri": "tcp://CN04MQVA.area1.eurofins.local:61636",
            //"EESBUserName": "eesbmini",
            //"AccountWriteServiceURI": "http://localhost:8004/api/accounts"
            list.AddRange(normalList);
        }

        private void Set4StatusUpdateAlerts(string id,  List<JsonItem> list, List<JsonItem> orgList)
        {
            var normalObj = orgList.FirstOrDefault(x => x.Id == id && !x.IsDeleted && x.IsObject);
            var normalList = orgList.Where(x => x.ParentId == normalObj.Id && !x.IsDeleted).ToList();
            var ele1 = normalList.FirstOrDefault(x => x.Key == "order1Subject" && !x.IsDeleted);
            ele1.Value = "Order not received";
            var ele2 = normalList.FirstOrDefault(x => x.Key == "order1Body" && !x.IsDeleted);
            ele2.Value = "The order {OrderNumber} was sent on the {DateTime} (sending status = 1), but no response has been received so far";
            var ele3 = normalList.FirstOrDefault(x => x.Key == "order3Subject" && !x.IsDeleted);
            ele3.Value = "Order: negative acknowledgement";
            var ele4 = normalList.FirstOrDefault(x => x.Key == "order3Body" && !x.IsDeleted);
            ele4.Value = "{DateTime}: A negative acknowledgement was sent for the order {OrderNumber} (sending status = 3)";
            var ele5 = normalList.FirstOrDefault(x => x.Key == "order4Subject" && !x.IsDeleted);
            ele5.Value = "Order not sent";
            var ele6 = normalList.FirstOrDefault(x => x.Key == "waitTimeInMins" && !x.IsDeleted);
            ele6.Value = "60";
            //"recipients": "",
            //"order1Subject": "Order not received",
            //"order1Body": "The order {OrderNumber} was sent on the {DateTime} (sending status = 1), but no response has been received so far",
            //"order3Subject": "Order: negative acknowledgement",
            //"order3Body": "{DateTime}: A negative acknowledgement was sent for the order {OrderNumber} (sending status = 3)",
            //"order4Subject": "Order not sent",
            //"order4Body": "{DateTime}: The order {OrderNumber} was not sent (sending status = 4)",
            //"waitTimeInMins": "60"
            list.AddRange(normalList);
        }


        //fa742054-d244-4290-8be5-ba535d51cf11
        //3b14c2b0-5e98-43d1-ad0d-043d5cd2c162
        //a32ee615-7902-477b-8347-f3109dd9b2e8
        //a8f7c37f-a107-402e-8ad2-1808da7fff7c
        //60bbb40c-bd2d-4c2f-8f99-dbe90808d9e1
        //f1f3e647-028d-4d3d-b59c-7bd445384c75
        //28eb47e9-19e8-4761-bbe7-e53e711b9f54
        //de013f9c-d8da-4e69-8ffd-d1e101875ce8


        #region Default
        private List<JsonItem> _defaultNormalERMSConfig;

        public List<JsonItem> DefaultNormalERMSConfig
        {
            get
            {
                if (_defaultNormalERMSConfig == null)
                {
                    var index = 1;
                    _defaultNormalERMSConfig = new List<JsonItem>();
                    var ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "fa742054-d244-4290-8be5-ba535d51cf11";
                    ji.IsObject = true;
                    ji.Key = "ORDERSFROMEOLXML";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(NormalList1(ji.Id));

                    ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "3b14c2b0-5e98-43d1-ad0d-043d5cd2c162";
                    ji.IsObject = true;
                    ji.Key = "NOTIFTOEOLXML";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(NormalList1(ji.Id));

                    ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "a32ee615-7902-477b-8347-f3109dd9b2e8";
                    ji.IsObject = true;
                    ji.Key = "ORDERSXML";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(NormalList1(ji.Id));

                    ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "a8f7c37f-a107-402e-8ad2-1808da7fff7c";
                    ji.IsObject = true;
                    ji.Key = "ORDERPROCESS";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(NormalList1(ji.Id));

                    ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "60bbb40c-bd2d-4c2f-8f99-dbe90808d9e1";
                    ji.IsObject = true;
                    ji.Key = "RESULTSXML";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(NormalList1(ji.Id));

                    ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "f1f3e647-028d-4d3d-b59c-7bd445384c75";
                    ji.IsObject = true;
                    ji.Key = "ResultsConsumer";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(ResultConsumerList(ji.Id));
                    BindDefaultResultConsumerConfig(ji.Id);

                    ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "28eb47e9-19e8-4761-bbe7-e53e711b9f54";
                    ji.IsObject = true;
                    ji.Key = "AccountsConsumer";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(AccountConsumerList(ji.Id));

                    ji = new JsonItem();
                    ji.Order = index++;
                    ji.Id = "de013f9c-d8da-4e69-8ffd-d1e101875ce8";
                    ji.IsObject = true;
                    ji.Key = "StatusUpdateAlerts";
                    ji.ParentId = this.Parent;
                    _defaultNormalERMSConfig.Add(ji);
                    _defaultNormalERMSConfig.AddRange(StatusUpdateAlertList(ji.Id));

                }
                return _defaultNormalERMSConfig;
            }
        }

        private void BindDefaultResultConsumerConfig(string parentId)
        {
            var ji = new JsonItem();
            ji.Id = Guid.NewGuid().ToString();
            ji.IsObject = true;
            ji.Key = "Sample";
            ji.ParentId = parentId;
            _defaultNormalERMSConfig.Add(ji);
            _defaultNormalERMSConfig.AddRange(NormalList2(ji.Id));

            ji = new JsonItem();
            ji.Id = Guid.NewGuid().ToString();
            ji.IsObject = true;
            ji.Key = "Result";
            ji.ParentId = parentId;
            _defaultNormalERMSConfig.Add(ji);
            _defaultNormalERMSConfig.AddRange(NormalList2(ji.Id));

            ji = new JsonItem();
            ji.Id = Guid.NewGuid().ToString();
            ji.IsObject = true;
            ji.Key = "ResultNG42";
            ji.ParentId = parentId;
            _defaultNormalERMSConfig.Add(ji);
            _defaultNormalERMSConfig.AddRange(NormalList2(ji.Id));

            ji = new JsonItem();
            ji.Id = Guid.NewGuid().ToString();
            ji.IsObject = true;
            ji.Key = "SampleNG43";
            ji.ParentId = parentId;
            _defaultNormalERMSConfig.Add(ji);
            _defaultNormalERMSConfig.AddRange(NormalList2(ji.Id));

            ji = new JsonItem();
            ji.Id = Guid.NewGuid().ToString();
            ji.IsObject = true;
            ji.Key = "ResultNG43";
            ji.ParentId = parentId;
            _defaultNormalERMSConfig.Add(ji);
            _defaultNormalERMSConfig.AddRange(NormalList2(ji.Id));


            ji = new JsonItem();
            ji.Id = Guid.NewGuid().ToString();
            ji.IsObject = true;
            ji.Key = "AccountsConsumer";
            ji.ParentId = parentId;
            _defaultNormalERMSConfig.Add(ji);
            _defaultNormalERMSConfig.AddRange(AccountConsumerList(ji.Id));

            ji = new JsonItem();
            ji.Id = Guid.NewGuid().ToString();
            ji.IsObject = true;
            ji.Key = "StatusUpdateAlerts";
            ji.ParentId = parentId;
            _defaultNormalERMSConfig.Add(ji);
            _defaultNormalERMSConfig.AddRange(StatusUpdateAlertList(ji.Id));
        }

        private List<JsonItem> AccountConsumerList(string parentId)
        {
            var list = new List<JsonItem>();
            var kv = new JsonItem
            {
                Key = "MessagingPlatform",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "EESBScope",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 2,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "EESBEnvironment",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 3,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "EESBBrokerUri",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);
            kv = new JsonItem
            {
                Key = "EESBUserName",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 5,
                ParentId = parentId
            };
            list.Add(kv);
            kv = new JsonItem
            {
                Key = "AccountWriteServiceURI",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 6,
                ParentId = parentId
            };
            list.Add(kv);
            return list;

        }

        private List<JsonItem> ResultConsumerList(string parentId)
        {
            var list = new List<JsonItem>();
            var kv = new JsonItem
            {
                Key = "MessagingPlatform",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "EESBScope",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 2,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "EESBEnvironment",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 3,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "EESBBrokerUri",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);
            kv = new JsonItem
            {
                Key = "EESBUserName",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 5,
                ParentId = parentId
            };
            list.Add(kv);

            return list;
        }

        private List<JsonItem> StatusUpdateAlertList(string parentId)
        {

            var list = new List<JsonItem>();
            var kv = new JsonItem
            {
                Key = "recipients",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "order1Subject",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 2,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "order1Body",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 3,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "order3Subject",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);
            kv = new JsonItem
            {
                Key = "order3Body",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 5,
                ParentId = parentId
            };
            list.Add(kv);
            kv = new JsonItem
            {
                Key = "order4Subject",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 6,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "order4Body",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 7,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "waitTimeInMins",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 8,
                ParentId = parentId
            };
            list.Add(kv);
            return list;
        }

        private List<JsonItem> NormalList1(string parentId)
        {
            var list = new List<JsonItem>();
            var kv = new JsonItem
            {
                Id = Guid.NewGuid().ToString(),
                Key = "Url",
                Value = "",
                Order = 1,
                ParentId=parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "mqUser",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 2,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "User",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 3,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "MessagingPlatform",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);
            return list;
        }

        private List<JsonItem> NormalList2(string parentId)
        {
            var list = new List<JsonItem>();
            var kv = new JsonItem
            {
                Key = "queue",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "brokerUri",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "environment",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);

            kv = new JsonItem
            {
                Key = "user",
                Value = "",
                Id = Guid.NewGuid().ToString(),
                Order = 4,
                ParentId = parentId
            };
            list.Add(kv);
            return list;
        }

        #endregion
    }
}
