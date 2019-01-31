using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using FSAR_Automation.RightNowService;
using RightNow.AddIns.AddInViews;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace FSAR_Automation
{
    public partial class SalesReleaseVINSelection : Form
    {
        IRecordContext _recordContext;
        ProgressForm form;
        List<string> _selectedIDs = new List<string>();
        List<string> _existingAffectedVins = new List<string>();
        List<string> _unselectedIDs = new List<string>();
        int _fsarID;
        int _buildCount = 0;
        bool _showDropDownList = false;

        public SalesReleaseVINSelection(string[] ModelList, IRecordContext RecordContext,
                                        int FsarID, List<string> ExistingAffetcedList)
        {
            InitializeComponent();
            _recordContext = RecordContext;
            _fsarID = FsarID;
            _existingAffectedVins = ExistingAffetcedList;
            
            //To Show List of Unique Models in combobox
            int i = 0;
            Model_ComboBox.Items.Add(new KeyValuePair<int, string>(i, "[No Value]"));
            foreach (string model in ModelList)
            {
                i++;
                int key = i;
                string value = model;
                Model_ComboBox.Items.Add(new KeyValuePair<int, string>(key, value));
            }
            Model_ComboBox.DisplayMember = "Value";
            Model_ComboBox.ValueMember = "Key";

            form = new ProgressForm();
        }

        /// <summary>
        /// Search for VIN records based on SR No or VIN No
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Button_Click(object sender, EventArgs e)
        {
            SelectAll_Checkbox.Checked = false;
            ClearAll_CheckBox.Checked = false;
           
            var filters = new List<KeyValuePair<string, string>>();
            #region SR and VIN null
            if ((SR_Combobox.SelectedItem == null || ((KeyValuePair<int, string>)SR_Combobox.SelectedItem).Key == 0)
                 && VIN_Textbox.Text == String.Empty && CustomerName_txtbx.Text==String.Empty && Model_ComboBox.SelectedItem == null)
            {
                if (DataGridView != null)
                {
                    clearDataGrid();//clear old view
                }
                MessageBox.Show("Select at least one Filter", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            #endregion

            #region SR not null
            
            if ((SR_Combobox.SelectedItem != null && ((KeyValuePair<int, string>)SR_Combobox.SelectedItem).Key != 0))              
            {                
                filters.Add(new KeyValuePair<string, string>("Sales Release", ((KeyValuePair<int,string>)SR_Combobox.SelectedItem).Value));                
            }
            #endregion

            #region VIN not null
            if (VIN_Textbox.Text != String.Empty)
            {
                filters.Add(new KeyValuePair<string, string>("VIN", VIN_Textbox.Text));
            }
            #endregion

            #region if Customer Name is not null
            if (CustomerName_txtbx.Text != String.Empty)
            {
                filters.Add(new KeyValuePair<string, string>("Customer Name", CustomerName_txtbx.Text));
            }
            #endregion

            #region if Model Name is not null
            if (Model_ComboBox.SelectedItem != null && ((KeyValuePair<int, string>)Model_ComboBox.SelectedItem).Key != 0)
            {
                filters.Add(new KeyValuePair<string, string>("Model Name", ((KeyValuePair<int, string>)Model_ComboBox.SelectedItem).Value));
            }
            #endregion
            CSVTable resulttable = RightNowConnectService.GetService().GetReportDetails(filters);
            
            if (resulttable.Rows.Length > 0)
            {
                PopulateGrid(resulttable);
                SelectExistingVins();
            }else
            {
                if (DataGridView != null)
                {
                    clearDataGrid();//clear old view
                }
                MessageBox.Show("No data found");
            }            
        }


        /// <summary>
        /// Fucntion to Populate DataGridView with Report Result
        /// </summary>
        /// <param name="resulttable"></param>
        private void PopulateGrid(CSVTable resulttable)
        {
            clearDataGrid();//clear old view

            DataTable dt = new DataTable();
            string[] tablerow = resulttable.Rows;
            string[] tablecolumn = resulttable.Columns.Split('~');
            foreach (string col in tablecolumn)
            {
                dt.Columns.Add(col);
            }
            foreach (string row in tablerow)
            {
                string[] rowdata = row.Split('~');
                dt.Rows.Add(rowdata);
            }
            DataGridViewCheckBoxColumn dgvcheckbox = new DataGridViewCheckBoxColumn();
            dgvcheckbox.ValueType = typeof(bool);
            dgvcheckbox.Name = "Select_CheckBox";
            dgvcheckbox.HeaderText = "Select VIN";
            DataGridView.DataSource = dt;
            
            DataGridView.Columns.Add(dgvcheckbox);

            DataGridView.Columns[0].Visible = false;//hide the first column which is "Bus ID"
        }

        /// <summary>
        /// bydefault select existing affected bus
        /// </summary>
        private void SelectExistingVins()
        {
            
            if (_existingAffectedVins != null && _existingAffectedVins.Count > 0)
            {
                
                foreach (DataGridViewRow row in DataGridView.Rows)
                {
                    
                    foreach (string vin in _existingAffectedVins)
                    {
                        int busId = Convert.ToInt32(vin.Split('~')[0]);// First element is bus id and second is Incident_VIN ID
                        if (Convert.ToInt32(row.Cells["Bus ID"].Value) == busId)
                        {
                            
                            row.Cells["Select_CheckBox"].Value = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// clear the datagridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearDataGrid()
        {
            DataGridView.DataSource = null;
            DataGridView.Columns.Clear();
            DataGridView.Refresh();
        }

        /// <summary>
        /// Build Button CLick Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Build_Button_Click(object sender, EventArgs e)
        {
            System.Threading.Thread newthread = new System.Threading.Thread(CreateRecords);
            newthread.Start();
            form.Show();
        }

        /// <summary>
        /// Fucntion to Create Records
        /// </summary>
        private void CreateRecords()
        {
            try
            {
                int srOrganizationID = 0;
                int srID = 0;              
                int primaryContact = 0;
                int internalIncidentID = 0;
                int reportingIncID = 0;
                List<string> srOrgAndContact = new List<string>();
                //Get Unselected and Selected Vins
                foreach (DataGridViewRow _row in DataGridView.Rows)
                {
                    if (Convert.ToBoolean(_row.Cells["Select_CheckBox"].Value) == false)
                    {
                        _unselectedIDs.Add(_row.Cells["Bus ID"].Value.ToString());
                    }
                    if (Convert.ToBoolean(_row.Cells["Select_CheckBox"].Value) == true)
                    {
                        _selectedIDs.Add(_row.Cells["Bus ID"].Value.ToString());
                    }
                }

                #region Unselected Vins
                if (_unselectedIDs != null && _unselectedIDs.Count > 0)
                {
                    List<int> deleteVins = new List<int>();

                    if (_existingAffectedVins != null && _existingAffectedVins.Count > 0)
                    {
                        foreach (string uvin in _unselectedIDs)
                        {
                            foreach (string evin in _existingAffectedVins)
                            {
                                int busId = Convert.ToInt32(evin.Split('~')[0]);// First element is bus id and second is Incident_VIN ID

                                if (Convert.ToInt32(uvin) == busId)
                                {
                                    // First element is bus id and second is Incident_VIN ID
                                    deleteVins.Add(Convert.ToInt32(evin.Split('~')[1]));
                                }
                            }
                        }
                    }
                    
                    if (deleteVins.Count > 0)
                    {
                        _buildCount++;
                        RightNowConnectService.GetService().DeleteIncidentVIN(deleteVins);
                    }
                }
                #endregion

                #region Selected Vins
                if (_selectedIDs != null && _selectedIDs.Count > 0)
                {
                    KeyValuePair<int, string> selectedItem = (KeyValuePair<int, string>)SR_Combobox.SelectedItem;
                    srID = selectedItem.Key;
                    
                    string response = RightNowConnectService.GetService().GetOrgAndPrimaryContact(srID);
                    if (response != null)
                    {
                        srOrganizationID = Convert.ToInt32(response.Split('~')[1]);
                        primaryContact = Convert.ToInt32(response.Split('~')[0]);
                    }
                    if (primaryContact != 0)
                    {
                        reportingIncID = RightNowConnectService.GetService().CreateReportingIncident(primaryContact, _fsarID,
                                                                                                     srOrganizationID);

                        if (srID != 0 && reportingIncID != 0)
                        {
                            internalIncidentID = RightNowConnectService.GetService().CreateInternalIncident(primaryContact, srID, 
                                                                                                             reportingIncID, _fsarID,
                                                                                                             srOrganizationID);
                        }

                        if (internalIncidentID != 0)
                        {
                            List<int> addVins = new List<int>();

                            for (int i = 0; i < _selectedIDs.Count; i++)
                            {
                                if (CheckIfVinExist(_selectedIDs[i]) == false) //to make sure duplicate records are not created
                                {
                                    addVins.Add(Convert.ToInt32(_selectedIDs[i]));
                                    _existingAffectedVins.Add(_selectedIDs[i]);
                                }
                            }
                            if (addVins.Count > 0)
                            {
                                RightNowConnectService.GetService().createIncidentVIN(addVins, internalIncidentID);
                            }

                            if (this.IsHandleCreated)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    form.Hide();
                                }));
                            }
                            _buildCount++;
                            MessageBox.Show(this, "Build Complete");
                            
                        }
                        else
                        {
                            if (this.IsHandleCreated)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    form.Hide();
                                }));
                            }

                            MessageBox.Show(this, "Internal Incident is not created", "Attention");
                        }
                    }
                    else
                    {
                        if (this.IsHandleCreated)
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                form.Hide();
                            }));
                        }
                        MessageBox.Show(this, "No Primary contact found for sale release :: "+selectedItem.Value, "Attention");
                    }
                    #endregion
                }
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        form.Hide();
                    }));
                }
            }
            catch (Exception ex)
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        form.Hide();
                    }));
                }             
                WorkspaceAddIn.InfoLog("Exception in CreateRecords: " + ex.Message);
            }
           
        }
        /// <summary>
        /// Check if VIN already exit by comparing selected VIN against list of existing VINS
        /// </summary>
        /// <param name="vinId"> VIn ID</param>
        /// <param name="e"></param>
        private bool CheckIfVinExist(string vinId)
        {
            foreach (string existingAffectedVin in _existingAffectedVins)
            {
                if (existingAffectedVin.Split('~')[0] == vinId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check All the Checkboxes 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAll_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectAll_Checkbox.Checked == true)
            {
                foreach (DataGridViewRow checkrow in DataGridView.Rows)
                {
                    checkrow.Cells["Select_CheckBox"].Value = true;
                }
                ClearAll_CheckBox.Checked = false;
            }
        }

        /// <summary>
        /// Uncheck all the Checkboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearAll_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ClearAll_CheckBox.Checked == true)
            {
                foreach (DataGridViewRow checkrow in DataGridView.Rows)
                {
                    checkrow.Cells["Select_CheckBox"].Value = false;
                }
                SelectAll_Checkbox.Checked = false;
            }
        }
        /// <summary>
        /// Keep Select All and Clear All checkboxes unchecked during form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncidentVinFormLoad(object sender, EventArgs e)
        {
            ClearAll_CheckBox.Checked = false;
            SelectAll_Checkbox.Checked = false;
            _showDropDownList = true;
        }
        /// <summary>
        /// Uncheck Select All or Clear All checkboxes if any of the row checkbox is unchecked or checked respectively
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                ClearAll_CheckBox.Checked = false;
                SelectAll_Checkbox.Checked = false;
            }
        }

        private void SalesReleaseVINSelection_FormClosed(object sender, FormClosedEventArgs e)
        {
           
            if (_buildCount != 0)
            {
                MessageBox.Show("In order to see newly added or removed SRs make sure to refresh workspace report by selecting the SR qty link. ");
                _recordContext.ExecuteEditorCommand(RightNow.AddIns.Common.EditorCommand.Save);
         
            }
        }

        /// <summary>
        /// Populate SR ComboBox based on Customer Name Entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void SR_Combobox_DropDown(object sender, EventArgs e)
        {
            if (_showDropDownList == true)
            {
                SR_Combobox.Items.Clear();
                string customerName = CustomerName_txtbx.Text;
                if (customerName!=string.Empty && customerName.Contains("*"))
                {
                    customerName = customerName.Replace('*', '%');
                }
                string[] srList = RightNowConnectService.GetService().GetSR(customerName);
                if (srList != null)
                {
                    SR_Combobox.Items.Add(new KeyValuePair<int, string>(0, "[No Value]"));

                    foreach (string sr in srList)
                    {
                        int key = Convert.ToInt32(sr.Split('~')[0]);
                        string value = sr.Split('~')[1];
                        SR_Combobox.Items.Add(new KeyValuePair<int, string>(key, value));
                    }
                    SR_Combobox.DisplayMember = "Value";
                    SR_Combobox.ValueMember = "Key";
                }
                else
                {
                    MessageBox.Show("No Sales Release Record Found");
                }
            }
            _showDropDownList = false;
        }      
       
        /// <summary>
        /// Set a flag once Customer Name is populated for SR search
        /// To avoid mulitple ROQL execution when SR drop down is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void CustomerName_txtbx_MouseLeave(object sender, EventArgs e)
        {
            _showDropDownList = true;
        }
    }
}
