using Beesys.Wasp.Workflow;
using BeeSys.Wasp.Communicator;
using BeeSys.Wasp.KernelController;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace BeeSys.Wasp3d.Utilities
{
    /*
      -	Before running the application, please ensure that Common is installed.
      -	We added 'WaspMosDataEntry.dll' to the 'bin' folder of the application to eliminate the dependency on the 'DataBuzz' setup.


     */



    public partial class frmPlaylist : Form
    {
        DateTime _dtDwnTime = DateTime.Now;
        string _PlaylistID = string.Empty;
        TreeNode _Rootnode = null;
        private const string PlaylistNODE = "dgn/action/node";
        private const string SlugNode = "dgn/action/node/node/metadata/slug/font";
        private const string GroupSlugNode = "dgn/action/node/node/metadata/slug";
        private const string InstanceNode = "dgn/action/node/node";
        private const string ActionNode = "dgn/action";
        private const string ModuleName = "playlisthandler";
        private const string ID = "id";
        BindingList<PlaylistData> _lstInstances = null;

        DataEntryControl _dataEntryControl = null;
        KCHelper kCHelper = null;
        string remoteurl = "";
        CProgramHelper _ProgramHelper;
        CPlaylistInstanceHelper _PlaylistHelper;
        private IMQReceiver _receiver = null;
        private string _sKCMachineName = null;

        public frmPlaylist()
        {         
            InitializeComponent();
            //Read the KC url from app config
            if (ConfigurationManager.AppSettings["REMOTEMANAGERURL"] != null)
                remoteurl = ConfigurationManager.AppSettings["REMOTEMANAGERURL"];

            InitDataEntry();
            GetKcName(remoteurl);
            InitKCEventChannelHandler(remoteurl);
        }


        /// <summary>
        /// Get KC name from url
        /// </summary>
        /// <param name="serviceurl"></param>
        private void GetKcName(string serviceurl)
        {
            //S.No.: -06
            string[] sarrKcName = null;
            string sKcName = null;
            IPAddress webserverip = null;
            try
            {
                sarrKcName = serviceurl.Split(new char[] { '/' });
                sKcName = sarrKcName[2].Split(new char[] { ':' })[0];

                //03
                //Check if KC is installed on current machine or not
                //If not then check if we are receiving data on basis of ip or machine name
                if (string.Compare(sKcName, "localhost", true) == 0)
                    sKcName = GetMachineInfo.GetCurrentMachineInfo().m_sMcname;
                else
                {
                    if (IPAddress.TryParse(sKcName, out webserverip))
                        sKcName = GetMachineInfo.GetMachineNameByIpAddress(sKcName);
                }

                _sKCMachineName = sKcName;
            }//end (try)
            catch (Exception ex)
            {
                WriteLog(ex);
            }//end (catch)
            finally
            {
                sarrKcName = null;
                sKcName = null;
                webserverip = null;
            }//end (finally)
        }//end (GetKcName)


        /// <summary>
        /// This method is used to initialize multicast response
        /// </summary>
        /// <param name="kcurl"></param>
        /// <param name="multicastip"></param>
        /// <param name="multicastport"></param>
        private void InitKCEventChannelHandler(string kcurl)
        {
            Tuple<string, string> eventchannelinfo = null;
            try
            {
                eventchannelinfo = GetKCConnectionInfo(remoteurl);

                //Check if kc is connected and any information is received. 
                //otherwise configure msmq multicast
                if (eventchannelinfo != null)
                {
                    //Check if any information about event channel is found from kc.
                    //otherwise configure msmq multicast
                    if (!string.IsNullOrEmpty(eventchannelinfo.Item1))
                    {
                        //Configure the receiver based on the information received from KC
                        //default is msmq multicast
                        switch (eventchannelinfo.Item1.ToLower())
                        {
                            case "rabbitmq":
                                {
                                    //Only configure raabitmq if rabbitmq config data found. otherwise configure msmq multicast
                                    if (!string.IsNullOrEmpty(eventchannelinfo.Item2))
                                        InitializeRabbitMQ(eventchannelinfo.Item2);
                                    break;
                                }//end (rabbitmq)
                            default:
                                    break;
                        }//end (switch(eventchannelinfo.Item1.ToLower()))
                    }//end (if (!string.IsNullOrEmpty(eventchannelinfo.Item1)))
                }//end (if (eventchannelinfo != null))   
            }//end (try)
            catch (Exception ex)
            {
                WriteLog(ex);
            }//end (catch)
            finally
            {
                eventchannelinfo = null;
            }//end (finally)
        }

        /// <summary>
        /// Initialize RabbitMQ
        /// </summary>
        /// <param name="mqdata"></param>
        private void InitializeRabbitMQ(string mqdata)
        {
            _receiver = RabbitMQReceiver.GetObject();
            _receiver.MessageReceived += MQMessageReceived;
            _receiver.Initialize(mqdata);
            _receiver.Start();
        }//end (InitializeRabbitMQ)

        /// <summary>
        /// Check connection and Get channel data from KC
        /// </summary>
        /// <param name="kcurl"></param>
        /// <returns></returns>
        private Tuple<string, string> GetKCConnectionInfo(string kcurl)
        {
            ConnectionStatus kcstatus;
            CRemoteHelper remotehelper = null;
            Tuple<string, string> eventchannelinfo = null;
            try
            {

                kcstatus = CRemoteHelper.CurrentConnectionStatus;

                if (!kcstatus.Status)
                {
                    remotehelper = new CRemoteHelper(kcurl);
                    kcstatus = remotehelper.Connect();
                }

                if (kcstatus.Status)
                    eventchannelinfo = new Tuple<string, string>(kcstatus.EventChannel, kcstatus.EventChannelData);

                return eventchannelinfo;
            }//end (try)
            catch (Exception ex)
            {
                WriteLog(ex);
                return null;
            }//end (catch)
        }//end (GetKCConnectionInfo)
      
        /// <summary>
        /// Event handle to receive data from RabbitMQ
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private void MQMessageReceived(string subject, string body)
        {
            try
            {
                UpdateMessageReceived(body, subject);
            }//end (try)
            catch (Exception ex)
            {
                WriteLog(ex);
            }//end (catch)
        }//end (MQMessageReceived)
        private void UpdateMessageReceived(string sMessageBody, string sMessageSubject)
        {
            string[] sArrMessageSubject = null;
            string sMachineName = null;
            string sTypeOfMessage = null;
            string sPlaylistIdAndVersion = null;
            string[] sArrPlaylistIdAndVersion = null;
            try
            {
                sArrMessageSubject = sMessageSubject.Split(':');
                if (sArrMessageSubject != null)
                {
                    if (sArrMessageSubject.Length > 2)
                    {
                        sMachineName = sArrMessageSubject[0];
                        if (string.Compare(_sKCMachineName, sMachineName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            sTypeOfMessage = sArrMessageSubject[1];

                            if (string.Compare(sTypeOfMessage.ToLower(), "playlistinstance", true) == 0)
                            {
                                sPlaylistIdAndVersion = sArrMessageSubject[2];
                                sArrPlaylistIdAndVersion = sPlaylistIdAndVersion.Split('_');

                                if (sArrPlaylistIdAndVersion != null && sArrPlaylistIdAndVersion.Length > 1)
                                {
                                    string sPlayListID = sArrPlaylistIdAndVersion[0];
                                    if (!string.IsNullOrEmpty(sPlayListID) && string.Compare(_PlaylistID, sPlayListID, true) == 0)
                                    {
                                        ParseReceivedMessage(sMessageBody);
                                    }

                                }
                            }
                        }//end (if (string.Compare(m_sWebServerName, sMachineName, StringComparison.OrdinalIgnoreCase) == 0))
                    }

                }
            }//end of try block
            catch (Exception ex)
            {
                WriteLog(ex);
            }//end oftry block
            finally
            {
                sArrMessageSubject = null;
                sMachineName = null;
                sTypeOfMessage = null;
                sPlaylistIdAndVersion = null;
            }
        }//end of UpdateMessageReceived

        /// <summary>
        /// parse the received message in MSMQ/RabbitMQ and do the changes according to that.
        /// </summary>
        /// <param name="messagebody"></param>
        private void ParseReceivedMessage(string messagebody)
        {
            XmlDocument xmlDocument = null;
            try
            {
                xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(messagebody);

                XmlNode xaction = xmlDocument.SelectSingleNode(ActionNode);
                if (xaction != null)
                {

                    string action = xaction.Attributes["name"].Value;
                    string type = xaction.Attributes["type"].Value;
                    if (!string.IsNullOrEmpty(type) && string.Compare(type, "response", true) == 0)
                    {
                        switch (action)
                        {
                            /*<dgn><action type="response" name="addinstance" username="" clientid="" isinsertbefore="False"><node type="playlist" id="301757f0-c778-4001-a2e2-d9596175eca6" version="1"><node type="instance" id="16678ffb-ad0e-4c0f-a9a2-55ed8a187170" nodeid="75c6ffbe-a6cf-4c8d-a9b6-2864e4f63490" targetid="" targetgroupid="" parentid="301757f0-c778-4001-a2e2-d9596175eca6"><metadata><loops><![CDATA[-1,0]]></loops><visibility><![CDATA[Checked]]></visibility><slug><font fontname="Tahoma" fontsize="8.25" fontstyle="Regular" backcolor="16777215"><![CDATA[Anim3.wspx]]></font></slug><created_by><![CDATA[madhur.jain]]></created_by><created_date><![CDATA[10/25/2023 12:38:53 PM]]></created_date><templateid><![CDATA[4cfd1782-cb96-4cc9-99d0-9aa26e53e5fd]]></templateid><templatename><![CDATA[Anim3.wspx]]></templatename><instanceinfo><![CDATA[]]></instanceinfo><externalmetadata><![CDATA[mos]]></externalmetadata><lastmodifyby><![CDATA[]]></lastmodifyby><waspfilename><![CDATA[\Anim3.wspx]]></waspfilename><instancerefid><![CDATA[]]></instancerefid><templaterefid><![CDATA[]]></templaterefid><templatechannel><![CDATA[Any]]></templatechannel><instancetype><![CDATA[Normal Instance]]></instancetype><approve><![CDATA[true]]></approve><templateedition><![CDATA[enterprise]]></templateedition></metadata></node></node></action></dgn>*/
                            case "addinstance":
                                AddInstance(xmlDocument);
                                break;

                            /*<dgn><action type="response" name="addgroup" username="" clientid="" isinsertbefore="False"><node type="playlist" id="301757f0-c778-4001-a2e2-d9596175eca6" version="3"><node type="group" id="06695c79-4781-45c7-aa08-933255db3d2d" nodeid="265af02d-6695-44d4-9d0e-0e229cc7013c" parentid="" targetid="" targetgroupid=""><metadata><loops><![CDATA[-1,0]]></loops><visibility><![CDATA[Checked]]></visibility><slug><![CDATA[T1]]></slug><description><![CDATA[T1]]></description><grouptype><![CDATA[Group]]></grouptype><approve><![CDATA[true]]></approve></metadata></node></node></action></dgn>*/
                            case "addgroup":
                                AddGroup(xmlDocument);
                                break;

                            /*<dgn><action type="response" name="deleteinstances" username="" clientid=""><node type="playlist" id="301757f0-c778-4001-a2e2-d9596175eca6" version="2"><node type="instance" id="75c6ffbe-a6cf-4c8d-a9b6-2864e4f63490" /></node></action></dgn>*/
                            /*<dgn><action type="response" name="deletegroup" username="" clientid=""><node type="playlist" id="301757f0-c778-4001-a2e2-d9596175eca6" version="4"><node type="group" id="265af02d-6695-44d4-9d0e-0e229cc7013c" /></node></action></dgn>*/
                            case "deleteinstances":
                            case "deletegroup":
                                DeleteInstanceorgroup(xmlDocument);
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }

        /// <summary>
        /// Add instance in treelist
        /// </summary>
        /// <param name="xml"></param>
        private void AddInstance(XmlDocument xml)
        {
            try
            {
                XmlNode slugNode = xml.SelectSingleNode(SlugNode);
                XmlNode instanceNode = xml.SelectSingleNode(InstanceNode);
                if (slugNode != null && instanceNode != null)
                {
                    PlaylistData data = new PlaylistData()
                    {
                        Id = instanceNode.Attributes["id"].Value,
                        Node = instanceNode.Attributes["nodeid"].Value,
                        Slug = GetSlug(slugNode),
                        ParentID = instanceNode.Attributes["parentid"].Value,
                        Type = "Instance"
                    };


                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            AddInstanceInTree(data);
                        }));
                    }
                    else
                    {
                        AddInstanceInTree(data);
                    }
                }
            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }

        /// <summary>
        /// Add instance node in treelist
        /// </summary>
        /// <param name="data"></param>
        private void AddInstanceInTree(PlaylistData data)
        {
            try
            {
                if (data.ParentID == _PlaylistID)
                {
                    _Rootnode.Nodes.Add(new TreeNode() { Text = data.Slug, Tag = data, Name = data.Node, BackColor = Color.LightGray });
                }
                else
                {
                    TreeNode node = _Rootnode.Nodes.Cast<TreeNode>().Where(x => (x.Tag as PlaylistData).Node == data.ParentID).First();
                    if (node != null)
                        node.Nodes.Add(new TreeNode() { Text = data.Slug, Tag = data, Name = data.Node, BackColor = Color.LightGray });
                }
            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }




        /// <summary>
        /// Add group in treelist
        /// </summary>
        /// <param name="xml"></param>
        private void AddGroup(XmlDocument xml)
        {
            try
            {
                XmlNode slugNode = xml.SelectSingleNode(GroupSlugNode);
                XmlNode instanceNode = xml.SelectSingleNode(InstanceNode);
                if (slugNode != null && instanceNode != null)
                {
                    PlaylistData data = new PlaylistData()
                    {
                        Id = instanceNode.Attributes["id"].Value,
                        Node = instanceNode.Attributes["nodeid"].Value,
                        Slug = (slugNode.FirstChild as XmlCDataSection).Data,
                        ParentID = instanceNode.Attributes["parentid"].Value,
                        Type = "Group"
                    };
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            _Rootnode.Nodes.Add(new TreeNode() { Text = data.Slug, Tag = data, Name = data.Node, BackColor = Color.LightGray });
                        }));

                    }
                    else
                        _Rootnode.Nodes.Add(new TreeNode() { Text = data.Slug, Tag = data, Name = data.Node, BackColor = Color.LightGray });

                }
            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }

        /// <summary>
        /// Remove instance or group on response
        /// </summary>
        /// <param name="xml"></param>
        private void DeleteInstanceorgroup(XmlDocument xml)
        {
            try
            {
                XmlNode instanceNode = xml.SelectSingleNode(InstanceNode);
                if (instanceNode != null)
                {
                    var Nodeid = instanceNode.Attributes["id"].Value;
                    TreeNode node = tvwPlaylist.Nodes.Find(Nodeid, true).FirstOrDefault();
                    if (node != null)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                node.Remove();
                            }));

                        }
                        else
                            node.Remove();
                    }

                }
            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }

        /// <summary>
        /// Get slug of instance from CDATA
        /// </summary>
        /// <param name="slugnode"></param>
        /// <returns></returns>
        private string GetSlug(XmlNode slugnode)
        {
            string slug = string.Empty;
            try
            {
                if (slugnode != null && slugnode.FirstChild is XmlCDataSection)
                {

                    slug = ((XmlCDataSection)(slugnode.FirstChild)).Data;
                }
            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
            return slug;
        }



        //Initialize template pool and KC helper
        private void InitDataEntry()
        {
            try
            {
                //Load template pool and Instance pool
                if (_dataEntryControl == null)
                {
                    _dataEntryControl = new DataEntryControl();
                    _dataEntryControl.InitialiseObject("", "", "");
                    _dataEntryControl.OnDataInstancePostUpdate += OnDataInstancePost;
                    _dataEntryControl.Dock = DockStyle.Fill;
                    pnlTemplate.Controls.Add(_dataEntryControl);


                }
                kCHelper = new KCHelper(remoteurl);
                if (kCHelper.KCConnected)
                {
                    //Intializing all the service helpers
                    var objProHelper = kCHelper.GetService(Services.ProgramManager);
                    if (objProHelper != null && objProHelper is CProgramHelper)
                    {
                        _ProgramHelper = objProHelper as CProgramHelper;
                    }
                    var objplayHelper = kCHelper.GetService(Services.PlaylistInstance);
                    if (objplayHelper != null && objplayHelper is CPlaylistInstanceHelper)
                    {
                        _PlaylistHelper = objplayHelper as CPlaylistInstanceHelper;
                    }

                }

            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }

        //Handle Exception
        void WriteLog(Exception ex)
        {

            //ToDo: Write log here
        }

        /// <summary>
        /// Raised when user post the instance data
        /// </summary>
        /// <param name="sInstanceID"></param>
        /// <param name="sInstanceSlug"></param>
        /// <param name="sTemplatename"></param>
        private void OnDataInstancePost(string sInstanceID, string sInstanceSlug, string sTemplatename)
        {
            try
            {
                if (_lstInstances == null)
                {
                    _lstInstances = new BindingList<PlaylistData>();
                    lstBoxInstances.DataSource = _lstInstances;
                    lstBoxInstances.DisplayMember = "Slug";
                }

                _lstInstances.Add(new PlaylistData()
                {
                    Id = sInstanceID,
                    Slug = sInstanceSlug,
                    Type = "Instance"
                });

            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        /// <summary>
        /// this event is used to create new playlist 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlaylist_Click(object sender, EventArgs e)
        {
            try
            {
                string playlistslug = txtBoxPlaylistslug.Text;
                if (!string.IsNullOrEmpty(playlistslug))
                {
                    //Helper call to check playlist present or not
                    //Arg1= Playlist slug/ID
                    //Arg2= IsSlug or not
                    var data = _ProgramHelper.GetPlaylistData(playlistslug, true);
                    if (data == null)
                    {

                        //Helper call To add new playlist
                        //Arg1= Slug of playlist
                        //Arg2= Description
                        //Arg3= Playouttype ID   //CG: 3711AFD0-E8D3-4939-A990-9554F31F2A80  //standard: "03AD7130-FAF2-4540-834F-24A95201DC44"
                        //Arg4= CreatedBy
                        var response = _ProgramHelper.AddNewPlaylist(playlistslug, "", "03AD7130-FAF2-4540-834F-24A95201DC44", "Wasp");

                        if (response != null)
                        {
                            var xdResponse = new XmlDocument();
                            xdResponse.LoadXml(response);

                            //To get new PlaylistID from XML
                            if (xdResponse.SelectSingleNode(PlaylistNODE) != null && xdResponse.SelectSingleNode(PlaylistNODE).Attributes[ID] != null)
                                _PlaylistID = xdResponse.SelectSingleNode(PlaylistNODE).Attributes[ID].Value;

                            var plstdata = new PlaylistData()
                            {
                                Id = _PlaylistID,
                                Slug = playlistslug,
                                Type = "Playlist"
                            };

                            tvwPlaylist.Nodes.Clear();
                            _Rootnode = null;
                            _Rootnode = new TreeNode();
                            _Rootnode.Text = plstdata.Slug;
                            _Rootnode.Tag = plstdata;
                            _Rootnode.BackColor = Color.LightGreen;
                            tvwPlaylist.Nodes.Add(_Rootnode);
                            tvwPlaylist.SelectedNode = _Rootnode;
                        }
                        else
                            MessageBox.Show("Playlist not created");
                    }
                    else
                        MessageBox.Show("Playlist already exist");
                }
                else
                {
                    MessageBox.Show("Please enter playlist slug");
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        /// <summary>
        /// this method is used to create the group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            try
            {

                string Groupslug = txtGroup.Text;
                if (!string.IsNullOrEmpty(_PlaylistID))
                {
                    if (!string.IsNullOrEmpty(Groupslug))
                    {

                        //Helper call to add group in playlist
                        //Arg1= Playlist Id
                        //Arg2=Target instance/group ID i.e. instance in which you want to add
                        //Arg3= Target group if group is present otherwise playlistid
                        //Arg4= Slug name of group 
                        //Arg5= Description of group
                        //Arg6= Group type Ex {Group, TimeBase Group, Date TimeBase Group}
                        //Arg7= Loop counter of group
                        //Arg8= Visibility of group i.e. Checked or Unchecked
                        //Arg9= Insert before or not
                        //Arg10= Metadata xml if required
                        //Arg11= Approved or not
                        string[] groupdata = _PlaylistHelper.AddGroupInPlaylist(_PlaylistID
                                                                            , string.Empty
                                                                            , string.Empty
                                                                            , Groupslug
                                                                            , Groupslug
                                                                            , "Group"
                                                                            , "-1,0"
                                                                            , "Checked"
                                                                            , false
                                                                            , ""
                                                                            , true);

                    }
                    else
                    {
                        MessageBox.Show("Group Slug is empty");
                    }
                }
                else
                {
                    MessageBox.Show("Playlist not loaded");
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);

            }
        }
        /// <summary>
        /// Validate draged data 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreePlaylist_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(typeof(PlaylistData)))
                {
                    e.Effect = DragDropEffects.All;
                    return;
                }

                e.Effect = DragDropEffects.None;
            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }

        /// <summary>
        /// Add Instance in playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreePlaylist_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(typeof(PlaylistData)))
                {
                    PlaylistData objDraggedInstance = e.Data.GetData(typeof(PlaylistData)) as PlaylistData;
                    if (objDraggedInstance.Type == "Instance")
                    {
                        TreeNode selected = tvwPlaylist.SelectedNode;
                        if (selected != null && selected.Tag != null && selected.Tag is PlaylistData)
                        {
                            PlaylistData data = selected.Tag as PlaylistData;
                            var parent = selected;

                            //Root lavel
                            if (data.Type == "Playlist")
                            {
                                //Helper call to add instance in playlist
                                //Arg1=Playlist id
                                //Arg2=InstanceId
                                //Arg3=Target instance/group ID i.e. instance in which you want to add
                                //Arg4= Target group if group is present otherwise playlistid
                                _PlaylistHelper.AddInstanceInPlylstByID(_PlaylistID, objDraggedInstance.Id, "", "");
                            }
                            //In group
                            else if (data.Type == "Group")
                            {
                                _PlaylistHelper.AddInstanceInPlylstByID(_PlaylistID, objDraggedInstance.Id, data.Id, data.Node);
                            }
                            //Below instance
                            else
                            {
                                PlaylistData groupdata = selected.Parent.Tag as PlaylistData;
                                parent = selected.Parent;
                                _PlaylistHelper.AddInstanceInPlylstByID(_PlaylistID, objDraggedInstance.Id, data.Id, groupdata.Node);

                            }


                            ////Helper call to get Playlist xml of instances and group.
                            //string xml = _PlaylistHelper.GetInstanceInfo(_PlaylistID, objDraggedInstance.Id, false);
                            //string nodeid = string.Empty;
                            //if (!string.IsNullOrEmpty(xml))
                            //{
                            //    XmlDocument instancexml = new XmlDocument();
                            //    instancexml.LoadXml(xml);

                            //    XmlNode xmlNode = instancexml.SelectSingleNode("dgn/action/node/instance_mst/nodeid");
                            //    if (xmlNode != null)
                            //        nodeid = xmlNode.InnerText;
                            //}

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);

            }
        }

        /// <summary>
        /// This event is used to drag drop posted data to create the playlist instance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstBoxInstances_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    //check for Time 
                    if (DateTime.Now.Subtract(_dtDwnTime).TotalMilliseconds > 200)
                    {
                        var data = lstBoxInstances.SelectedItem;
                        if (data != null)
                            DoDragDropInstance(data);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        /// <summary>
        /// Handle Drag Drop
        /// </summary>
        /// <param name="data"></param>
        public void DoDragDropInstance(object data)
        {
            try
            {
                lstBoxInstances.DoDragDrop(data, DragDropEffects.Move | DragDropEffects.Copy);
                _dtDwnTime = DateTime.Now;

            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Used to delete the Instance/Group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreePlaylist_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    TreeNode selected = tvwPlaylist.SelectedNode;
                    if (selected != null && selected.Tag is PlaylistData)
                    {
                        PlaylistData data = selected.Tag as PlaylistData;
                        if (data.Type != "Playist")
                        {
                            //nodeids array contains multiple node id of instances
                            string[] nodeids = new string[] { data.Node };
                            //Call to remove instance or group from playlist
                            //Arg1= Playlist id
                            //arg2= nodeids
                            //Arg3=USername
                            //Args4= Machinename
                            _PlaylistHelper.RemoveItem(_PlaylistID, nodeids, "Wasp", "Wasp");


                        }

                    }
                }
            }
            catch (Exception ex)
            {

                WriteLog(ex);
            }
        }

        /// <summary>
        /// Release resources
        /// </summary>
        private void Dispose()
        {
            try
            {
                if (_lstInstances != null)
                {
                    _lstInstances = null;
                }

                if (_dataEntryControl != null)
                {
                    _dataEntryControl.Dispose();
                    _dataEntryControl = null;
                }
                if (kCHelper != null)
                {
                    kCHelper = null;
                }
                if (_ProgramHelper != null)
                {
                    _ProgramHelper.Dispose();
                    _ProgramHelper = null;
                }
                if (_PlaylistHelper != null)
                {
                    _PlaylistHelper.Dispose();
                    _PlaylistHelper = null;
                }


                if (_receiver != null)
                {
                    _receiver.MessageReceived -= MQMessageReceived;
                    _receiver.Dispose();
                    _receiver = null;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }
    }
    public class PlaylistData
    {
        public string Id { get; set; }

        public string Slug { get; set; }

        public string Type { get; set; }
        public string Node { get; set; }
        public string ParentID { get; set; }
    }
}
