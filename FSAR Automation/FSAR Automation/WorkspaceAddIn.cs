using System.AddIn;
using System.Drawing;
using System.Windows.Forms;
using RightNow.AddIns.AddInViews;
using System.Collections.Generic;
using System.Linq;
using System;

////////////////////////////////////////////////////////////////////////////////
//
// File: WorkspaceAddIn.cs
//
// Comments:
//
// Notes: 
//
// Pre-Conditions: 
//
////////////////////////////////////////////////////////////////////////////////
namespace FSAR_Automation
{
    public class WorkspaceAddIn : Panel, IWorkspaceComponent2
    {/// <summary>
     /// The current workspace record context.
     /// </summary>
        private IRecordContext _recordContext;
        public static IGlobalContext _globalContext;
        public static IGenericObject _fsarRecord;
        private System.Windows.Forms.Label label1;
        public static int _reportID;
        List<string> _affectedBusId;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="inDesignMode">Flag which indicates if the control is being drawn on the Workspace Designer. (Use this flag to determine if code should perform any logic on the workspace record)</param>
        /// <param name="RecordContext">The current workspace record context.</param>
        public WorkspaceAddIn(bool inDesignMode, IRecordContext RecordContext, IGlobalContext GlobalContext,
                              int ReportID)
        {
            if (!inDesignMode)
            {
                _recordContext = RecordContext;
                _globalContext = GlobalContext;
                _reportID = ReportID;
                _recordContext.DataLoaded += _recordContext_DataLoaded;
            }
            else
            {
                InitializeComponent();
            }
        }
        /// <summary>
        /// Method called by the Add-In framework initialize in design mode.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "FSAR Automation Add-in";
            this.label1.Size = new System.Drawing.Size(20, 10);
            this.label1.TabIndex = 0;
            this.label1.Text = "FSAR Automation to select/build Multi SAR and VIN relationship for FSAR";
            label1.Margin = new Padding(10);
            Controls.Add(this.label1);
            this.Size = new System.Drawing.Size(20, 10);
            this.ResumeLayout(false);
        }
        /// <summary>
        /// Method called by data load event. It does the following:
        /// 1> Get Onload FSAR value
        /// 2> Get all affected VIN mapped to currently opened FSAR
        /// </summary>
        private void _recordContext_DataLoaded(object sender, System.EventArgs e)
        {
            _affectedBusId = new List<string>();
            _fsarRecord  = (IGenericObject)_recordContext.GetWorkspaceRecord("CO$FSAR");
            if (_fsarRecord != null)
            {
                string[] response = RightNowConnectService.GetService().GetAffectedBus(_fsarRecord.Id);
                if (response != null)
                {
                    _affectedBusId = response.ToList();
                }
            }            
        }
        /// <summary>
        /// Method for unsubsribing events.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (null != _recordContext))
            {
                // unsubscribe from all the events
                _recordContext.DataLoaded -= _recordContext_DataLoaded;
            }
            base.Dispose(disposing);
        }
        #region IAddInControl Members

        /// <summary>
        /// Method called by the Add-In framework to retrieve the control.
        /// </summary>
        /// <returns>The control, typically 'this'.</returns>
        public Control GetControl()
        {
            return this;
        }

        #endregion

        #region IWorkspaceComponent2 Members

        /// <summary>
        /// Sets the ReadOnly property of this control.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Method which is called when any Workspace Rule Action is invoked.
        /// </summary>
        /// <param name="ActionName">The name of the Workspace Rule Action that was invoked.</param>
        public void RuleActionInvoked(string ActionName)
        {
            if (ActionName == "selectVIN")
            {
                _fsarRecord = (IGenericObject)_recordContext.GetWorkspaceRecord("CO$FSAR");

                //Get the List of Unique Model Names from Sales Release CO 
                string[] modelList = RightNowConnectService.GetService().GetModel();


                SalesReleaseVINSelection form = new SalesReleaseVINSelection(modelList, _recordContext, _fsarRecord.Id, _affectedBusId);
                form.ShowDialog();

            }
        }

        /// <summary>
        /// Method which is called when any Workspace Rule Condition is invoked.
        /// </summary>
        /// <param name="ConditionName">The name of the Workspace Rule Condition that was invoked.</param>
        /// <returns>The result of the condition.</returns>
        public string RuleConditionInvoked(string ConditionName)
        {
            return string.Empty;
        }
        /// <summary>
        /// Method which is called to to show info/error message.
        /// </summary>
        /// <param name="message">Tesx message to be displayed in a pop-up</param>
        public static void InfoLog(string message)
        {
            //form.Hide();
            MessageBox.Show(message);
        }
        #endregion
    }

    [AddIn("Workspace Factory AddIn", Version = "1.0.0.0")]
    public class WorkspaceAddInFactory : IWorkspaceComponentFactory2
    {

        #region IWorkspaceComponentFactory2 Members

        public static IGlobalContext _globalContext;
        public  int _reportID;
        public int _inicdentType;
        public  int _internalIncidentType;
        [ServerConfigProperty(DefaultValue = "100268")]
        public int ReportID
        {
            get { return _reportID; }
            set { _reportID = value; }
        }

        /// <summary>
        /// Method which is invoked by the AddIn framework when the control is created.
        /// </summary>
        /// <param name="inDesignMode">Flag which indicates if the control is being drawn on the Workspace Designer. (Use this flag to determine if code should perform any logic on the workspace record)</param>
        /// <param name="RecordContext">The current workspace record context.</param>
        /// <returns>The control which implements the IWorkspaceComponent2 interface.</returns>
        public IWorkspaceComponent2 CreateControl(bool inDesignMode, IRecordContext RecordContext)
        {
            return new WorkspaceAddIn(inDesignMode, RecordContext, _globalContext,_reportID);
        }

        #endregion

        #region IFactoryBase Members

        /// <summary>
        /// The 16x16 pixel icon to represent the Add-In in the Ribbon of the Workspace Designer.
        /// </summary>
        public Image Image16
        {
            get { return Properties.Resources.AddIn16; }
        }

        /// <summary>
        /// The text to represent the Add-In in the Ribbon of the Workspace Designer.
        /// </summary>
        public string Text
        {
            get { return "FSAR Automation"; }
        }

        /// <summary>
        /// The tooltip displayed when hovering over the Add-In in the Ribbon of the Workspace Designer.
        /// </summary>
        public string Tooltip
        {
            get { return "FSAR Automation to select/build Multi SAR and VIN relationship for FSAR"; }
        }

        #endregion

        #region IAddInBase Members

        /// <summary>
        /// Method which is invoked from the Add-In framework and is used to programmatically control whether to load the Add-In.
        /// </summary>
        /// <param name="GlobalContext">The Global Context for the Add-In framework.</param>
        /// <returns>If true the Add-In to be loaded, if false the Add-In will not be loaded.</returns>
        public bool Initialize(IGlobalContext GlobalContext)
        {
            _globalContext = GlobalContext;
            return true;
        }

        #endregion
    }
}