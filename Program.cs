using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;

using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;

namespace CreateAndInitialization
{
    struct Equipment
    {
        public string EquipmentID;
        public string EquipmentTagName;
        public string EquipmentComponentClass;
        public string[] NodeID;

        public string ChildEquipmentID;
        public string ChildEquipmentComponentClass;
        public string ChildTagName;

        public GenericAttribute[] EquipmentGenericAttribute;
    }

    struct PipingNetworkSegment
    {
        public string PipingNetworkSegmentID;
        public string PipingNetworkSegmentTagName;
        public string PipingNetworkSegmentComponentClass;

        public List<Equipment> PipingNetworkSegmentComponent;
        public List<Connection> PipingNetworkSegmentConnection;

        public GenericAttribute[] PipingNetworkSegmentGenericAttribute;
    }

    struct Connection
    {
        public string ConnectionFromID;
        public string ConnectionFromNode;
        public string ConnectionToID;
        public string ConnectionToNode;
    }

    struct GenericAttribute
    {
        public string GenericAttributeName;
        public string GenericAttributeFormat;
        public string GenericAttributeValue;
    }

    class XMLExtract
    {

        // Extract the information from DEXPI XML, and save in two types of struct respectively.
        public static async Task Main2()
        {
            List<Equipment> EQ = new List<Equipment>();
            List<PipingNetworkSegment> PNS = new List<PipingNetworkSegment>();

            XmlDocument Document = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;//Ignore commends in the documents
            XmlReader reader = XmlReader.Create(@"D:\Studium\SemesterArbeit\Sync+Share\Semesterarbeit -- Yu Mu\Material\MyJogurt\DEXPI\kopie.xml", settings);
            Document.Load(reader);
            string token;
            // Read token from local file
            using (StreamReader readtext = new StreamReader(@"D:\Studium\SemesterArbeit\Sync+Share\Semesterarbeit -- Yu Mu\Material\MyJogurt\token.txt"))
            {
                token = readtext.ReadLine();
            }

            // select root node
            XmlNode root = Document.SelectSingleNode("PlantModel");
            // get all child nodes of root node
            if (root != null)
            {
                XmlElement PlantModel = (XmlElement)root;
                XmlNodeList PlantModelChild = PlantModel.ChildNodes;
                for (int a = 0; a < PlantModelChild.Count; a++)
                {
                    string pointname1 = PlantModelChild.Item(a).Name;

                    if (pointname1 == "Equipment")
                    {
                        Equipment newEQ = new Equipment();
                        newEQ.EquipmentID = PlantModelChild.Item(a).Attributes["ID"].Value;
                        newEQ.EquipmentComponentClass = PlantModelChild.Item(a).Attributes["ComponentClass"].Value;
                        newEQ.EquipmentTagName = PlantModelChild.Item(a).Attributes["TagName"].Value;
                        XmlNodeList EquipmentChild = PlantModelChild.Item(a).ChildNodes;
                        for (int b = 0; b < EquipmentChild.Count; b++)
                        {
                            string pointname2 = EquipmentChild.Item(b).Name;
                            if (pointname2 == "ConnectionPoints")
                            {
                                XmlNodeList Node = EquipmentChild.Item(b).ChildNodes;
                                newEQ.NodeID = new string[Node.Count];
                                for (int c = 0; c < Node.Count; c++)
                                {
                                    newEQ.NodeID[c] = Node.Item(c).Attributes["ID"].Value;
                                }
                            }
                            else if (pointname2 == "GenericAttributes")
                            {
                                XmlNodeList GenericAttributes = EquipmentChild.Item(b).ChildNodes;
                                newEQ.EquipmentGenericAttribute = new GenericAttribute[GenericAttributes.Count];
                                for (int c = 0; c < GenericAttributes.Count; c++)
                                {
                                    string pointname3 = GenericAttributes.Item(c).Name;
                                    if (pointname3 == "GenericAttribute")
                                    {
                                        newEQ.EquipmentGenericAttribute[c].GenericAttributeName = GenericAttributes.Item(c).Attributes["Name"].Value;
                                        newEQ.EquipmentGenericAttribute[c].GenericAttributeFormat = GenericAttributes.Item(c).Attributes["Units"].Value; // incert units into Format
                                        newEQ.EquipmentGenericAttribute[c].GenericAttributeValue = GenericAttributes.Item(c).Attributes["Value"].Value;
                                    }
                                }
                            }
                            else if (pointname2 == "Equipment")
                            {
                                newEQ.ChildEquipmentID = EquipmentChild.Item(b).Attributes["ID"].Value;
                                newEQ.ChildEquipmentComponentClass = EquipmentChild.Item(b).Attributes["ComponentClass"].Value;
                                if (EquipmentChild.Item(b).Attributes["TagName"] is null)
                                {
                                    newEQ.ChildTagName = "";
                                }
                                else
                                {
                                    newEQ.ChildTagName = EquipmentChild.Item(b).Attributes["TagName"].Value;
                                }

                            }
                        }
                        EQ.Add(newEQ);
                    }
                    else if (pointname1 == "PipingNetworkSystem")
                    {
                        XmlNodeList PipingNetworkSegment = PlantModelChild.Item(a).ChildNodes;
                        for (int b = 0; b < PipingNetworkSegment.Count; b++)
                        {
                            string pointname2 = PipingNetworkSegment.Item(b).Name;
                            if (pointname2 == "PipingNetworkSegment")
                            {
                                PipingNetworkSegment newPNS = new PipingNetworkSegment();
                                newPNS.PipingNetworkSegmentComponentClass = PipingNetworkSegment.Item(b).Attributes["ComponentClass"].Value;
                                newPNS.PipingNetworkSegmentID = PipingNetworkSegment.Item(b).Attributes["ID"].Value;
                                newPNS.PipingNetworkSegmentTagName = PipingNetworkSegment.Item(b).Attributes["TagName"].Value;
                                newPNS.PipingNetworkSegmentComponent = new List<Equipment>();
                                newPNS.PipingNetworkSegmentConnection = new List<Connection>();
                                XmlNodeList PipingNetworkSegmentChild = PipingNetworkSegment.Item(b).ChildNodes;
                                for (int c = 0; c < PipingNetworkSegmentChild.Count; c++)
                                {
                                    string pointname3 = PipingNetworkSegmentChild.Item(c).Name;
                                    if (pointname3 == "GenericAttributes")
                                    {
                                        XmlNodeList GenericAttributes = PipingNetworkSegmentChild.Item(c).ChildNodes;
                                        newPNS.PipingNetworkSegmentGenericAttribute = new GenericAttribute[GenericAttributes.Count];
                                        for (int d = 0; d < GenericAttributes.Count; d++)
                                        {
                                            string pointname4 = GenericAttributes.Item(d).Name;
                                            if (pointname4 == "GenericAttribute")
                                            {
                                                if (GenericAttributes.Item(d).Attributes["Name"] is null)
                                                {
                                                    newPNS.PipingNetworkSegmentGenericAttribute[d].GenericAttributeName = "";
                                                }
                                                else
                                                {
                                                    newPNS.PipingNetworkSegmentGenericAttribute[d].GenericAttributeName = GenericAttributes.Item(d).Attributes["Name"].Value;
                                                }

                                                if (GenericAttributes.Item(d).Attributes["Format"] is null)
                                                {
                                                    newPNS.PipingNetworkSegmentGenericAttribute[d].GenericAttributeFormat = "";
                                                }
                                                else
                                                {
                                                    newPNS.PipingNetworkSegmentGenericAttribute[d].GenericAttributeFormat = GenericAttributes.Item(d).Attributes["Format"].Value;
                                                }

                                                if (GenericAttributes.Item(d).Attributes["Value"] is null)
                                                {
                                                    newPNS.PipingNetworkSegmentGenericAttribute[d].GenericAttributeValue = "";
                                                }
                                                else
                                                {
                                                    newPNS.PipingNetworkSegmentGenericAttribute[d].GenericAttributeValue = GenericAttributes.Item(d).Attributes["Value"].Value;
                                                }

                                            }
                                        }
                                    }

                                    else if (pointname3 == "PipingComponent")
                                    {
                                        Equipment newComponent = new Equipment();
                                        newComponent.EquipmentID = PipingNetworkSegmentChild.Item(c).Attributes["ID"].Value;
                                        newComponent.EquipmentComponentClass = PipingNetworkSegmentChild.Item(c).Attributes["ComponentClass"].Value;
                                        newComponent.EquipmentTagName = PipingNetworkSegmentChild.Item(c).Attributes["TagName"].Value;
                                        XmlNodeList PipingComponent = PipingNetworkSegmentChild.Item(c).ChildNodes;

                                        for (int d = 0; d < PipingComponent.Count; d++)
                                        {
                                            string pointname4 = PipingComponent.Item(d).Name;
                                            if (pointname4 == "ConnectionPoints")
                                            {
                                                XmlNodeList Node = PipingComponent.Item(d).ChildNodes;
                                                newComponent.NodeID = new string[Node.Count];
                                                for (int e = 0; e < Node.Count; e++)
                                                {
                                                    string pointname5 = Node.Item(e).Name;
                                                    if (pointname5 == "Node")
                                                    {
                                                        newComponent.NodeID[e] = Node.Item(e).Attributes["ID"].Value;
                                                    }
                                                }
                                            }
                                            else if (pointname4 == "GenericAttributes")
                                            {
                                                XmlNodeList GenericAttributes = PipingComponent.Item(d).ChildNodes;
                                                newComponent.EquipmentGenericAttribute = new GenericAttribute[GenericAttributes.Count];
                                                for (int e = 0; e < GenericAttributes.Count; e++)
                                                {
                                                    string pointname5 = GenericAttributes.Item(e).Name;
                                                    if (pointname5 == "GenericAttribute")
                                                    {
                                                        if (GenericAttributes.Item(e).Attributes["Name"] is null)
                                                        {
                                                            newComponent.EquipmentGenericAttribute[e].GenericAttributeName = "";
                                                        }
                                                        else
                                                        {
                                                            newComponent.EquipmentGenericAttribute[e].GenericAttributeName = GenericAttributes.Item(e).Attributes["Name"].Value;
                                                        }

                                                        if (GenericAttributes.Item(e).Attributes["Value"] is null)
                                                        {
                                                            newComponent.EquipmentGenericAttribute[e].GenericAttributeValue = "";
                                                        }
                                                        else
                                                        {
                                                            newComponent.EquipmentGenericAttribute[e].GenericAttributeValue = GenericAttributes.Item(e).Attributes["Value"].Value;
                                                        }

                                                        if (GenericAttributes.Item(e).Attributes["Format"] is null)
                                                        {
                                                            newComponent.EquipmentGenericAttribute[e].GenericAttributeFormat = "";
                                                        }
                                                        else
                                                        {
                                                            newComponent.EquipmentGenericAttribute[e].GenericAttributeFormat = GenericAttributes.Item(e).Attributes["Format"].Value;
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                        newPNS.PipingNetworkSegmentComponent.Add(newComponent);
                                    }
                                    else if (pointname3 == "Connection")
                                    {
                                        Connection newConnection = new Connection();

                                        if (PipingNetworkSegmentChild.Item(c).Attributes["FromID"] is null)
                                        {
                                            newConnection.ConnectionFromID = "";
                                        }
                                        else
                                        {
                                            newConnection.ConnectionFromID = PipingNetworkSegmentChild.Item(c).Attributes["FromID"].Value;
                                        }

                                        if (PipingNetworkSegmentChild.Item(c).Attributes["FromNode"] is null)
                                        {
                                            newConnection.ConnectionFromNode = "";
                                        }
                                        else
                                        {
                                            newConnection.ConnectionFromNode = PipingNetworkSegmentChild.Item(c).Attributes["FromNode"].Value;
                                        }

                                        if (PipingNetworkSegmentChild.Item(c).Attributes["ToID"] != null) // how to extend to anthoer variable? 
                                        {
                                            newConnection.ConnectionToID = PipingNetworkSegmentChild.Item(c).Attributes["ToID"].Value;
                                        }
                                        else
                                        {
                                            newConnection.ConnectionToID = "";
                                        }
                                        if (PipingNetworkSegmentChild.Item(c).Attributes["ToNode"] != null)
                                        {
                                            newConnection.ConnectionToNode = PipingNetworkSegmentChild.Item(c).Attributes["ToNode"].Value;
                                        }
                                        else
                                        {
                                            newConnection.ConnectionToNode = "";
                                        }

                                        newPNS.PipingNetworkSegmentConnection.Add(newConnection);
                                    }
                                }
                                PNS.Add(newPNS);
                            }
                        }
                    }
                }
            }

            string[] EQDT = new string[EQ.Count]; // storing Equipment digital twin 
            List<string> ChildEQDT = new List<string>(); // 
            string[] PNSDT = new string[PNS.Count]; // storing PNS digital twin
            string postUrl = "https://Yogurtmachine.api.wcus.digitaltwins.azure.net/digitaltwins/THISMODEL?api-version=2020-10-31"; // post link

            string EQjson = "{ \"$metadata\": {" +
                "  \"$model\": \"dtmi:dtdl:K1;1\" }, " +
                "\"ID\": \"K3\"," +
                "\"TagName\": \"K4\"}";

            string PNSJson = "{ \"$metadata\": {" +
                "  \"$model\": \"dtmi:dtdl:K1;1\" }, " +
                "\"ID\": \"K2\"," +
                "\"TagName\": \"K3\"," +
                "\"ComponentClass\": \"K4\"," +
                "\"ColorCodeAssignmentClass\": \"K5\"," +
                "\"NominalDiameterRepresentationAssignmentClass\":\"K6\"}";

            // extract info from struct and merge into a json string only for creating DT (not DTDL models) without relationships
            if (root != null)
            {
                for (int a = 0; a < EQ.Count; a++)
                {
                    string tmp = EQjson;
                    string url = postUrl;
                    //EQDT[a] = tmp.Replace("K1", EQ[a].EquipmentComponentClass).Replace("K2", EQ[a].EquipmentComponentClass).Replace("K3", EQ[a].EquipmentID).Replace("K4", EQ[a].EquipmentTagName);
                    if (EQ[a].ChildEquipmentID != null) // Acquire the subequipment of a equipment. Such as heater or motor.
                    {
                        tmp = tmp.Replace("K1", EQ[a].ChildEquipmentComponentClass).Replace("K3", EQ[a].ChildEquipmentID).Replace("K4", EQ[a].ChildTagName);
                        url = url.Replace("THISMODEL", EQ[a].ChildEquipmentID);
                        Console.WriteLine(tmp);
                        Console.WriteLine(url);
                        //await AddTwin(url, tmp, token);
                        tmp = EQjson;
                        url = postUrl;
                    }
                    
                    tmp = tmp.Replace("K1", EQ[a].EquipmentComponentClass).Replace("K3", EQ[a].EquipmentID).Replace("K4", EQ[a].EquipmentTagName);
                    url = url.Replace("THISMODEL", EQ[a].EquipmentID);
                    
                    Console.WriteLine(tmp);
                    Console.WriteLine(url);
                    //await AddTwin(url,tmp, token);
                    

                }

                for (int a = 0; a < PNS.Count; a++)
                {

                    for (int b = 0; b < PNS[a].PipingNetworkSegmentComponent.Count; b++)
                    {
                        string tmp = PNSJson;
                        string url = postUrl;
                        if (PNS[a].PipingNetworkSegmentComponent[b].EquipmentComponentClass == "CustomOperatedValve")
                        {
                            tmp = tmp.Replace("K1", "CustomOperatedValve");
                        }
                        else
                        {
                            tmp = tmp.Replace("K1", "StandardOperatedValve");
                        }

                        string color = "";
                        string NominalDiameter = "";
                        for (int c = 0; c < PNS[a].PipingNetworkSegmentGenericAttribute.Length; c++)
                        {
                            if (PNS[a].PipingNetworkSegmentGenericAttribute[c].GenericAttributeName == "ColorCodeAssignmentClass")
                            {
                                color = PNS[a].PipingNetworkSegmentGenericAttribute[c].GenericAttributeValue;
                            }
                            else if (PNS[a].PipingNetworkSegmentGenericAttribute[c].GenericAttributeName == "NominalDiameterRepresentationAssignmentClass")
                            {
                                NominalDiameter = PNS[a].PipingNetworkSegmentGenericAttribute[c].GenericAttributeValue;
                            }
                        }

                        tmp = tmp.Replace("K2", PNS[a].PipingNetworkSegmentComponent[b].EquipmentID).Replace("K3", PNS[a].PipingNetworkSegmentComponent[b].EquipmentTagName).Replace("K4", PNS[a].PipingNetworkSegmentComponent[b].EquipmentComponentClass).Replace("K5", color).Replace("K6", NominalDiameter);
                        url = url.Replace("THISMODEL", PNS[a].PipingNetworkSegmentComponent[b].EquipmentID);
                        Console.WriteLine(tmp);
                        Console.WriteLine(url);
                        //await AddTwin(url, tmp, token);
                        // ToDo: how to extract the attribute for customed value
                    }
                }
            }

            // create json for relationship.
            string relationship = "{ " +
                "\"$targetId\": \"myTargetTwin\"," +
                "\"$relationshipName\": \"myRelationship\" }";
            string AddRelationshipUrl = "https://Yogurtmachine.api.wcus.digitaltwins.azure.net/digitaltwins/SourceTwin/relationships/relationshipId?api-version=2020-10-31"; // post relationship link

            if (root != null)
            {
                string tmp = relationship;
                string url = AddRelationshipUrl;
                for (int a = 0; a < PNS.Count; a++)
                {
                    foreach (var x in PNS[a].PipingNetworkSegmentConnection)
                    {
                        tmp = relationship;
                        url = AddRelationshipUrl;
                        
                        if (x.ConnectionToID !="")
                        {
                            tmp = tmp.Replace("myRelationship", "To").Replace("myTargetTwin", x.ConnectionToID);
                            url = url.Replace("SourceTwin", x.ConnectionFromID).Replace("relationshipId", x.ConnectionFromID + "To" + x.ConnectionToID);
                            Console.WriteLine(tmp);
                            Console.WriteLine(url);
                            await AddTwin(url, tmp, token);

                        }
                        
                    }
                }

            }


        }
        private static async Task AddTwin(string postUrl,string json, string token)
        {
            Console.WriteLine($"Add Twin");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";

            // OAuth 2.0 authentication using bearer token 
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

            // Read local Json file
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            // Catch response from server
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

        }
    }
    // main class of this program
    class Program
    {
        private const string adtInstanceUrl = "https://Yogurtmachine.api.wcus.digitaltwins.azure.net/models?api-version=2020-10-31";

        static async Task Main(string[] args)
        {
            string token; 
            // Read token from local file
            using (StreamReader readtext = new StreamReader(@"D:\Studium\SemesterArbeit\Sync+Share\Semesterarbeit -- Yu Mu\Material\MyJogurt\token.txt"))
            {
                token = readtext.ReadLine();
            }

            // Upload models
            //await CreateModel(token);

            //Add Twins using REST API
            await XMLExtract.Main2();
   
        }

        private static async Task CreateModel(string token)
        {
            Console.WriteLine($"Upload a model");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(adtInstanceUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            // OAuth 2.0 authentication using bearer token 
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

            // Read local Json file
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                StreamReader model = new StreamReader(@"D:\Studium\SemesterArbeit\Sync+Share\Semesterarbeit -- Yu Mu\Material\MyJogurt\DTDLmodels\DTDL.json");
                string json = model.ReadToEnd();
                streamWriter.Write(json);
            }

            // Catch response from server
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

        /*
        private static async Task AddTwin(string postUrl, string token)
        {
            Console.WriteLine($"Add Twin");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";

            // OAuth 2.0 authentication using bearer token 
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

            // Read local Json file
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                StreamReader model = new StreamReader(@"D:\Studium\SemesterArbeit\Sync+Share\Semesterarbeit -- Yu Mu\Material\MyJogurt\DEXPI\DT\PTLB.json");
                string json = model.ReadToEnd();
                streamWriter.Write(json);
            }
           
            // Catch response from server
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
            
        }
        */
        

    }
}
