using FSAR_Automation.RightNowService;
using RightNow.AddIns.AddInViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace FSAR_Automation
{
    class RightNowConnectService
    {
        private static RightNowConnectService _rightnowConnectService;
        private static object _sync = new object();
        private static RightNowSyncPortClient _rightNowClient;
        private RightNowConnectService()
        {

        }
        public static RightNowConnectService GetService()
        {
            if (_rightnowConnectService != null)
            {
                return _rightnowConnectService;
            }

            try
            {
                lock (_sync)
                {
                    if (_rightnowConnectService == null)
                    {
                        // Initialize client with current interface soap url 
                        string url = WorkspaceAddIn._globalContext.GetInterfaceServiceUrl(ConnectServiceType.Soap);
                        EndpointAddress endpoint = new EndpointAddress(url);

                        BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
                        binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

                        // Optional depending upon use cases
                        binding.MaxReceivedMessageSize = 1024 * 1024;
                        binding.MaxBufferSize = 1024 * 1024;
                        binding.MessageEncoding = WSMessageEncoding.Mtom;

                        _rightNowClient = new RightNowSyncPortClient(binding, endpoint);

                        BindingElementCollection elements = _rightNowClient.Endpoint.Binding.CreateBindingElements();
                        elements.Find<SecurityBindingElement>().IncludeTimestamp = false;
                        _rightNowClient.Endpoint.Binding = new CustomBinding(elements);
                        WorkspaceAddIn._globalContext.PrepareConnectSession(_rightNowClient.ChannelFactory);

                        _rightnowConnectService = new RightNowConnectService();
                    }

                }
            }
            catch (Exception e)
            {
                _rightnowConnectService = null;
                WorkspaceAddIn.InfoLog(e.Message);
            }
            return _rightnowConnectService;
        }
        /// <summary>
        /// Return individual fields as per query
        /// </summary>
        /// <param name="ApplicationID"></param>
        /// <param name="Query"></param>
        /// <returns> array of string delimited by '~'</returns>
        private string[] GetRNData(string ApplicationID, string Query)
        {
            string[] rnData = null;
            ClientInfoHeader hdr = new ClientInfoHeader() { AppID = ApplicationID };

            byte[] output = null;
            CSVTableSet data = null;

            try
            {
                data = _rightNowClient.QueryCSV(hdr, Query, 1000, "~", false, false, out output);
                string dataRow = String.Empty;
                if (data != null && data.CSVTables.Length > 0 && data.CSVTables[0].Rows.Length > 0)
                {
                    return data.CSVTables[0].Rows;
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog(ex.Message);
            }
            return rnData;
        }
        /// <summary>
        /// Get all affected Bus for all the Internal Incidents associated with FSAR
        /// </summary>
        /// <param name="fsarID"></param>
        /// <returns> array of bus id in string format</returns>
        public string[] GetAffectedBus(int fsarID)
        {
            try
            {
                string query = "select V.Bus, V.ID from CO.Incident_VIN V where " +
                               "V.Incident.CustomFields.CO.reporting_incident.CustomFields.CO.FSAR = " + fsarID;
                string[] resultSet = GetRNData("Get Affected Bus",query);
                if (resultSet != null && resultSet.Length > 0)
                {
                    return resultSet;
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in getting exisitng affected Bus IDs: " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Get List of all the SR
        /// </summary>
        /// <param name=""></param>
        /// <returns> string[] </returns>
        public string[] GetSR(string orgName)
        {
            try
            {
                string query = "";
                if (orgName != string.Empty)
                {
                    query = "select S.ID, S.sr_nmbr from CO.SalesRelease S where S.organization.Name LIKE" +"'" + orgName + "'" + "ORDER BY S.sr_nmbr";
                }
                else
                {
                    query = "select S.ID, S.sr_nmbr from CO.SalesRelease S";
                }
                string[] resultSet = GetRNData("Get All SR",query);
                if (resultSet != null && resultSet.Length > 0)
                {
                    return resultSet;
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in getting SR List " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Get List of all the Model
        /// </summary>
        /// <param name=""></param>
        /// <returns> string[] </returns>
        public string[] GetModel()
        {
            try
            {
                string query = "select S.model from CO.SalesRelease S GROUP BY S.model ORDER BY S.model";
                string[] resultSet = GetRNData("Get All Model", query);
                if (resultSet != null && resultSet.Length > 0)
                {
                    return resultSet;
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in getting Model List " + ex.Message);
            }
            return null;
        }            

        /// <summary>
        /// Get Organization and Primary Contact for the selected SR
        /// </summary>
        /// <param name="salesreleaseID"></param>
        /// <returns></returns>
        public string GetOrgAndPrimaryContact(int salesReleaseID)
        {
            try
            {
                string query = "select O.Contacts.ID as c_id, O.ID as org_id"+
                               " from CO.SalesRelease SR INNER JOIN SR.organization O"+
                               " where O.Contacts.CustomFields.CO.primary_contact = 1 AND SR.ID =" + salesReleaseID;
                string[] resultset = GetRNData("Get SR Org/Contact",query);
                if (resultset != null && resultset.Length > 0)
                {
                    return resultset[0];
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in getting exisitng affected Bus IDs: " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Get Result of Report based on Filter's passed
        /// </summary>
        /// <param name="filters">List of filter name and Values</param>
        /// <returns>CSVTable</returns>
        public CSVTable GetReportDetails(List<KeyValuePair<string, string>> filters)
        {
            try
            {
                AnalyticsReport analyticsreport = new AnalyticsReport { ID = new ID { id = WorkspaceAddIn._reportID, idSpecified = true } };
                List<AnalyticsReportFilter> analyticsfilter = new List<AnalyticsReportFilter>();
                
                foreach (KeyValuePair<string, string> filter in filters)
                {
                    string filterName = filter.Key;
                    if (filterName == "Customer Name")
                    {
                        analyticsfilter.Add(new AnalyticsReportFilter
                        {
                            Name = filter.Key,
                            Operator = new NamedID
                            {
                                ID = new ID
                                {
                                    id = 7,
                                    idSpecified = true
                                }
                            },
                            Values = new string[] { filter.Value }
                        });
                    }
                    else
                    {
                        analyticsfilter.Add(new AnalyticsReportFilter
                        {
                            Name = filter.Key,
                            Operator = new NamedID
                            {
                                ID = new ID
                                {
                                    id = 1,
                                    idSpecified = true
                                }
                            },
                            Values = new string[] { filter.Value }
                        });
                    }
                }
                analyticsreport.Filters = analyticsfilter.ToArray();
                CSVTableSet tableset = RunReport(analyticsreport);
                if (tableset.CSVTables.Length > 0)
                {
                    return tableset.CSVTables[0];
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in GetReportDetails: " + ex.Message);
            }
            return null;
        }
        /// <summary>
        /// Funtion to Run the report
        /// </summary>
        /// <param name="report">AnalyticsReport info like report ID and Filter detail</param>
        /// <returns>CSVTableSet</returns>
        public CSVTableSet RunReport(AnalyticsReport report)
        {
            CSVTableSet reportdata = null;
            byte[] bytearray = null;
            try
            {
                ClientInfoHeader hdr = new ClientInfoHeader() { AppID = "Get Report Data" };
                reportdata = _rightNowClient.RunAnalyticsReport(hdr, report, 10000, 0, "~", false, true, out bytearray);
                if (reportdata != null && reportdata.CSVTables.Length > 0)
                {
                    return reportdata;
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in RunReport: " + ex.Message);
            }
            return null;
        }
        /// <summary>
        /// Batch Request to Delete Incident_VIN records
        /// </summary>
        /// <param name="delete_id">List of Vins need to be deleted</param>
        /// <returns></returns>
        public void DeleteIncidentVIN(List<int> deleteVins)
        {
            try
            {
                List<RNObject> deleteObject = new List<RNObject>();
                List<int> internalIncIDs = new List<int>(); // Didi - Nov 2018 - neeed to delete inc vin
                String query;
                String[] resultset; 
                
                for (int i = 0; i < deleteVins.Count; i++)
                {

                    GenericObject genObj = new GenericObject
                    {
                        ObjectType = new RNObjectType
                        {
                            Namespace = "CO",
                            TypeName = "Incident_VIN"
                        }
                    };
                    genObj.ID = new ID
                    {
                        id = deleteVins[i],
                        idSpecified = true
                    };

                    query = "SELECT IV.Incident.ID as iiID FROM CO.Incident_VIN IV WHERE ID = " + deleteVins[i];
                    resultset = GetRNData("Get interntal inc info", query);
                    if (resultset != null && resultset.Length > 0)
                    {
                        
                        internalIncIDs.Add(Convert.ToInt32(resultset[0]));//store the internal inc ID
                    }

                    deleteObject.Add(genObj);
                    
                }
                //BatchResponseItem[] batchRes = rspc.Batch(clientInfoHeader, requestItems);
                callBatchJob(getDestroyMsg(deleteObject));

                // Delete internal inc
                internalIncIDs = internalIncIDs.Distinct().ToList();
                
                foreach (int internalIncID in internalIncIDs)
                {
                    IncidentVinCountForInternalInc(internalIncID);//check if Internal incident is empty, if so then delete that
                }
                //*end delete intenal inc
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Excpetion in Deleting FSAR_VIN record: " + ex.Message);
            }
            return;
        }
        /// <summary>
        /// Create Reporting Incident
        /// </summary>
        /// <param name="contactID"></param>
        /// <param name="fsarID"></param>
        /// <param name="orgID"></param>
        /// <returns></returns>     
        public int CreateReportingIncident(int contactID, int fsarID, int orgID)
        {
            try
            {
                string response = checkIfReportingIncidentExistforFSAR(fsarID, contactID);
                if (response != null)
                    return Convert.ToInt32(response);
                /*Set OOTB fields*/
                Incident reportedIncident = new Incident();
                IncidentContact primarycontact = new IncidentContact { Contact = new NamedID { ID = new ID { id = contactID, idSpecified = true } } };
                reportedIncident.PrimaryContact = primarycontact;
                reportedIncident.Organization = new NamedID { ID = new ID { id = orgID, idSpecified = true} };
                reportedIncident.StatusWithType = new StatusWithType
                {
                    Status = new NamedID
                    {
                        Name = "Solution Development"
                    }
                };

                /*Set Custom Attributes*/
                List<GenericField> customAttributes = new List<GenericField>();
                customAttributes.Add(createGenericField("FSAR", createNamedIDDataValue(fsarID), DataTypeEnum.NAMED_ID));
                customAttributes.Add(createGenericField("FSAR_required", createBooleanDataValue(true), DataTypeEnum.BOOLEAN));
                GenericObject customAttributeobj = genericObject(customAttributes.ToArray(), "IncidentCustomFieldsc");
                GenericField caPackage = createGenericField("CO", createObjDataValue(customAttributeobj), DataTypeEnum.OBJECT);
                
                /*Set Custom fields*/
                List<GenericField> customFields = new List<GenericField>();
                customFields.Add(createGenericField("incident_type", createNamedIDDataValueForName("Reported Incident"), DataTypeEnum.NAMED_ID));//55 is id of "Internal incident"
                GenericObject customfieldobj = genericObject(customFields.ToArray(), "IncidentCustomFieldsc");
                GenericField cfpackage = createGenericField("c", createObjDataValue(customfieldobj), DataTypeEnum.OBJECT);

                reportedIncident.CustomFields = genericObject(new[] { caPackage, cfpackage }, "IncidentCustomFields");

                ClientInfoHeader hdr = new ClientInfoHeader() { AppID = "Create Reported Incident" };
                RNObject[] resultobj = _rightNowClient.Create(hdr, new RNObject[] { reportedIncident }, new CreateProcessingOptions { SuppressExternalEvents = false, SuppressRules = false });
                if (resultobj != null)
                {
                    return Convert.ToInt32(resultobj[0].ID.id);
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Excpetion in creating reported incident: " + ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// Check if reporting incident exist for the FSAR
        /// </summary>
        /// <param name="fsarID"></param>
        /// <returns></returns>
        public string checkIfReportingIncidentExistforFSAR(int fsarID, int contactID)
        {
            try
            {
                string query = "select I.ID from Incident I where I.CustomFields.c.incident_type.LookupName = 'Reported Incident'" +
                               " AND I.CustomFields.CO.FSAR = " + fsarID + " AND I.PrimaryContact.ParentContact.ID = " + contactID;
                string[] resultset = GetRNData("Get Reported Incident", query);
                if (resultset != null && resultset.Length > 0)
                {
                    return resultset[0];
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in Checking exisitng reported incident: " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Create Internal Incident Records
        /// </summary>
        /// <param name="contactID"></param>
        /// <param name="srID"></param>
        /// <param name="reportingIncID"></param>
        /// <param name="fsarID"></param>
        /// <param name="orgID"></param>
        /// <returns></returns>
        public int CreateInternalIncident(int contactID, int srID, int reportingIncID, int fsarID, int orgID)
        {
            try
            {
                //Check if it exist in order to avoid duplicate record
                Incident internalIncident = new Incident();
                string response = checkIfInternalIncidentExistForSR(reportingIncID, srID);
                if (response != null)
                {
                    internalIncident.ID = new ID
                    {
                        id = Convert.ToInt32(response),
                        idSpecified = true
                    };
                }
                /*Set OOTB fields*/
                IncidentContact primarycontact = new IncidentContact { Contact = new NamedID { ID = new ID { id = contactID, idSpecified = true } } };
                internalIncident.PrimaryContact = primarycontact;
                internalIncident.Organization = new NamedID { ID = new ID { id = orgID, idSpecified = true } };

                /*Set Custom fields*/
                List<GenericField> customAttributes = new List<GenericField>();
                customAttributes.Add(createGenericField("FSAR", createNamedIDDataValue(fsarID), DataTypeEnum.NAMED_ID));
                customAttributes.Add(createGenericField("SalesRelease", createNamedIDDataValue(srID), DataTypeEnum.NAMED_ID));
                customAttributes.Add(createGenericField("reporting_incident", createNamedIDDataValue(reportingIncID), DataTypeEnum.NAMED_ID));
                GenericObject customAttributeobj = genericObject(customAttributes.ToArray(), "IncidentCustomFieldsc");
                GenericField caPackage = createGenericField("CO", createObjDataValue(customAttributeobj), DataTypeEnum.OBJECT);

                /*Set Custom fields*/
                List<GenericField> customFields = new List<GenericField>();
                customFields.Add(createGenericField("incident_type", createNamedIDDataValueForName("Internal Incident"), DataTypeEnum.NAMED_ID));//55 is id of "Internal incident"
                GenericObject customfieldobj = genericObject(customFields.ToArray(), "IncidentCustomFieldsc");
                GenericField cfpackage = createGenericField("c", createObjDataValue(customfieldobj), DataTypeEnum.OBJECT);

                internalIncident.CustomFields = genericObject(new[] { caPackage, cfpackage }, "IncidentCustomFields");

                ClientInfoHeader hdr = new ClientInfoHeader() { AppID = "Create Internal Incident" };
                //Update if internal incident exist
                if (response != null)
                {
                    _rightNowClient.Update(hdr, new RNObject[] { internalIncident}, new UpdateProcessingOptions { SuppressExternalEvents=false, SuppressRules=false});
                    return Convert.ToInt32(response);
                }
                else//create a new internal incident
                {
                    RNObject[] resultobj = _rightNowClient.Create(hdr, new RNObject[] { internalIncident }, new CreateProcessingOptions { SuppressExternalEvents = false, SuppressRules = false });
                    if (resultobj != null)
                    {
                        return Convert.ToInt32(resultobj[0].ID.id);
                    }
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Excpetion in creating internal incident: " + ex.Message);
            }
            return 0;
        }
        /// <summary>
        /// Check if Internal Incident exist for the selected SR
        /// </summary>
        /// <param name="repIncID">Reported Incident ID</param>
        /// <param name="srID"></param>
        /// <returns></returns>
        public string checkIfInternalIncidentExistForSR(int repIncID, int srID)
        {
            try
            {
                string query = "Select ID from Incident where CustomFields.CO.reporting_incident = " + repIncID +
                               " AND CustomFields.CO.SalesRelease = " + srID;
                string[] resultset = GetRNData("Get Internal Incident", query);
                if (resultset != null && resultset.Length > 0)
                {
                    return resultset[0];
                }
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in getting exisitng internal incident for SR: " + ex.Message);
            }
            return null;
        }
        /// Check if Internal incident has any Incident_VIN child record, if not then delete internal incident
        /// </summary>
        /// <param name="internalIncID"> Reported Incident Org ID</param>
        /// <returns> </returns>
        public void IncidentVinCountForInternalInc(int internalIncID)
        {
            string query = "SELECT count(ID) as count FROM CO.Incident_VIN WHERE incident = " + internalIncID;
            string[] resultSet = GetRNData("Get incident_VIN count", query);

            if (resultSet != null && resultSet.Length > 0)
            {
                
                if (resultSet[0] == "0")//if count is 0 then delete internal incident too
                {

                    List<int> incId = new List<int>();
                    incId.Add(internalIncID);                    
                    DeleteInternalIncident(incId);
                }
            }
            return;
        }
        /// <summary>
        /// Delete Internal Incident Records
        /// </summary>
        /// <param name="deleteIncIDs"></param>
        /// <returns></returns>
        public void DeleteInternalIncident(List<int> deleteIncIDs)
        {
            try
            {
                List<RNObject> deleteObject = new List<RNObject>();
                for (int i = 0; i < deleteIncIDs.Count; i++)
                {

                    Incident incObj = new Incident();
                    incObj.ID = new ID
                    {
                        id = deleteIncIDs[i],
                        idSpecified = true
                    };

                    deleteObject.Add(incObj);
                }

                //BatchResponseItem[] batchRes = rspc.Batch(clientInfoHeader, requestItems);
                callBatchJob(getDestroyMsg(deleteObject));
            }
            catch (Exception ex)
            {
                //SR mean internal incident
                WorkspaceAddIn.InfoLog("Exception in Deleting Internal Incident record: " + ex.Message);
            }
            return;
        }
        /// <summary>
        /// Batch Operation to create Incident_VIN records 
        /// </summary>
        /// <param name="busIDs">List of Bus Ids</param>
        /// <param name="internalIncID">Internal Incident ID</param>
        /// <returns></returns>
        public BatchRequestItem createIncidentVIN(List<int> busIDs, int internalIncID)
        {
            try
            {
                List<RNObject> createObject = new List<RNObject>();
                foreach (int busID in busIDs)
                {
                    GenericObject genObj = new GenericObject
                    {
                        ObjectType = new RNObjectType
                        {
                            Namespace = "CO",
                            TypeName = "Incident_VIN"
                        }
                    };
                    List<GenericField> gfs = new List<GenericField>();
                    gfs.Add(createGenericField("Bus", createNamedIDDataValue(busID), DataTypeEnum.NAMED_ID));
                    gfs.Add(createGenericField("Incident", createNamedIDDataValue(internalIncID), DataTypeEnum.NAMED_ID));
                    genObj.GenericFields = gfs.ToArray();

                    createObject.Add(genObj);
                }
                callBatchJob(getCreateMsg(createObject));
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog("Exception in creating FSAR_VIN records: " + ex.Message);
            }
            return null;
        }

        #region Miscellaneous Functions
        /// <summary>
        /// Create CreateMsg object
        /// </summary>
        /// <param name="coList">RNObject List</param>
        /// <returns> CreateMsg</returns>
        private CreateMsg getCreateMsg(List<RNObject> coList)
        {
            CreateMsg createMsg = new CreateMsg();
            CreateProcessingOptions createProcessingOptions = new CreateProcessingOptions();
            createProcessingOptions.SuppressExternalEvents = true;
            createProcessingOptions.SuppressRules = true;
            createMsg.ProcessingOptions = createProcessingOptions;
            createMsg.RNObjects = coList.ToArray();
            return createMsg;
        }

        /// <summary>
        /// Create DestroyMsg object
        /// </summary>
        /// <param name="coList">RNObject List</param>
        /// <returns> DestroyMsg</returns>
        private DestroyMsg getDestroyMsg(List<RNObject> coList)
        {
            DestroyMsg deleteMsg = new DestroyMsg();
            DestroyProcessingOptions deleteProcessingOptions = new DestroyProcessingOptions();
            deleteProcessingOptions.SuppressExternalEvents = true;
            deleteProcessingOptions.SuppressRules = true;
            deleteMsg.ProcessingOptions = deleteProcessingOptions;
            deleteMsg.RNObjects = coList.ToArray();
            return deleteMsg;
        }

        /// <summary>
        /// Perform Batch operation
        /// </summary>
        /// <param name="msg">BatchRequestItem Item</param>
        public void callBatchJob(Object msg)
        {
            try
            {
                /*** Form BatchRequestItem structure ********************/

                BatchRequestItem[] requestItems = new BatchRequestItem[1];

                BatchRequestItem requestItem = new BatchRequestItem();
                requestItem.Item = msg;

                requestItems[0] = requestItem;
                requestItems[0].CommitAfter = true;
                requestItems[0].CommitAfterSpecified = true;
                /*********************************************************/

                ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
                clientInfoHeader.AppID = "Batcher";

                BatchResponseItem[] batchRes = _rightNowClient.Batch(clientInfoHeader, requestItems);

                if (batchRes[0].Item.GetType().Name == "RequestErrorFaultType")
                {
                    RequestErrorFaultType requestErrorFault = (RequestErrorFaultType)batchRes[0].Item;
                    WorkspaceAddIn.InfoLog("There is an error with batch job :: " + requestErrorFault.exceptionMessage);
                }
            }
            catch (FaultException ex)
            {
                WorkspaceAddIn.InfoLog(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                WorkspaceAddIn.InfoLog(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Create DataValue object
        /// </summary>
        /// <param name="val">DataValue Item </param>
        /// <returns> DataValue</returns>
        private DataValue createObjDataValue(object val)
        {
            DataValue dv = new DataValue();
            dv.Items = new Object[] { val };
            dv.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.ObjectValue };  //Change this to the type of field
            return dv;
        }

        /// <summary>
        /// Create GenericField object
        /// </summary>
        /// <param name="name">Name Of Generic Field</param>
        /// <param name="dataValue">Vlaue of generic field</param>
        /// <param name="type">Type of generic field</param>
        /// <returns> GenericField</returns>
        private GenericField createGenericField(string name, DataValue dataValue, DataTypeEnum type)
        {
            GenericField genericField = new GenericField();

            genericField.dataType = type;
            genericField.dataTypeSpecified = true;
            genericField.name = name;
            genericField.DataValue = dataValue;
            return genericField;
        }

        /// <summary>
        /// Create Named ID type data value
        /// </summary>
        /// <param name="idVal"></param>
        /// <returns> DataValue</returns>
        private DataValue createNamedIDDataValue(long idVal)
        {
            ID id = new ID();
            id.id = idVal;
            id.idSpecified = true;

            NamedID namedID = new NamedID();
            namedID.ID = id;

            DataValue dv = new DataValue();
            dv.Items = new Object[] { namedID };
            dv.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.NamedIDValue };

            return dv;
        }
        /// <summary>
        /// Create Named ID type data value for Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns> DataValue</returns>
        private DataValue createNamedIDDataValueForName(string name)
        {
            NamedID namedID = new NamedID();
            namedID.Name = name;

            DataValue dv = new DataValue();
            dv.Items = new Object[] { namedID };
            dv.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.NamedIDValue };

            return dv;
        }
        /// <summary>
        ///  Create Boolean type data value 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private DataValue createBooleanDataValue(Boolean val)
        {
            DataValue dv = new DataValue();
            dv.Items = new Object[] { val };
            dv.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.BooleanValue };

            return dv;
        }

        /// <summary>
        /// Create Generic Object type data value
        /// </summary>
        /// <param name="gF">Array of Generic Field</param>
        /// <param name="typeName">RNObjectType Type name</param>
        /// <returns> GenericObject</returns>
        private GenericObject genericObject(GenericField[] gF, string typeName)
        {
            RNObjectType rnObjType = new RNObjectType();
            rnObjType.TypeName = typeName;

            GenericObject gObj = new GenericObject();
            gObj.GenericFields = gF;
            gObj.ObjectType = rnObjType;

            return gObj;
        }
        #endregion 

    }
}
