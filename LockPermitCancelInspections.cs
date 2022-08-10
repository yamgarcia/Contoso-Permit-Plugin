using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.Text.RegularExpressions;
using ContosoPackagePoject;
using Microsoft.Xrm.Sdk.Query;
using System.Web.UI.WebControls;
using System.ServiceModel.Channels;

namespace ContosoPackageProject
{
    public class LockPermitCancelInspections : PluginBase
    {
        /**
         * Override the ExecuteCDSPlugin method
         */
        protected override void ExecuteCDSPlugin(LocalPluginContext localcontext)
        {
            base.ExecuteCDSPlugin(localcontext);

            //Get the target Entity Reference and Entity (Table)
            var permitEntityRef = localcontext.PluginExecutionContext.InputParameters["Target"] as EntityReference;
            Entity permitEntity = new Entity(permitEntityRef.LogicalName, permitEntityRef.Id);

            //Add Trace message and Set the Status Reason to Lock. 463270000 is the lock value of the Status Reason option-set and statuscode is the name of the status reason Column.
            localcontext.Trace("Updating Permit Id : " + permitEntityRef.Id);
            permitEntity["statuscode"] = new OptionSetValue(463270000);

            //Update the Permit record and add Trace message.
            localcontext.OrganizationService.Update(permitEntity);
            localcontext.Trace("Updated Permit Id " + permitEntityRef.Id);

            //Create the QueryExpression
            QueryExpression qe = new QueryExpression();
            qe.EntityName = "contoso_inspection";
            qe.ColumnSet = new ColumnSet("statuscode");

            //Create the ConditionExpression
            ConditionExpression condition = new ConditionExpression();
            condition.Operator = ConditionOperator.Equal;
            condition.AttributeName = "contoso_permit";
            condition.Values.Add(permitEntityRef.Id);

            //Set the Criteria of the query
            qe.Criteria = new FilterExpression(LogicalOperator.And);

            //Add the ConditionExpression to the Criteria of the QueryExpression
            qe.Criteria.Conditions.Add(condition);

            //Retrieve the Inspections and add Trace messages
            localcontext.Trace("Retrieving inspections for Permit Id " + permitEntityRef.Id);
            var inspectionsResult = localcontext.OrganizationService.RetrieveMultiple(qe);
            localcontext.Trace("Retrievied " + inspectionsResult.TotalRecordCount + " inspection records");

            //Create a variable that will keep track of the canceled Inspections count and Iterate through the returned Tables
            int canceledInspectionsCount = 0;
            foreach (var inspection in inspectionsResult.Entities)
            {
                //Get the currently selected value of the Status Reason option-set
                var currentValue = inspection.GetAttributeValue<OptionSetValue>("statuscode");

                //Check if the selected option is New Request or Pending and increment the count. 1 is the value of the New Request option and 463270000 id the value of the Pending option
                if (currentValue.Value == 1 || currentValue.Value == 463270000)
                {
                    canceledInspectionsCount++;

                    //Set the Status Reason selected value to Canceled.
                    //Add the code below inside the if statement inside the foreach loop.
                    //Make sure that 463270003 is the value for Canceled Status Reason in the Inspections Table.
                    //If this differs, please update the value with actual value for Canceled Status Reason.
                    inspection["statuscode"] = new OptionSetValue(463270003);

                    //Update the Inspection and add Trace messages
                    localcontext.Trace("Canceling inspection Id : " + inspection.Id);
                    localcontext.OrganizationService.Update(inspection);
                    localcontext.Trace("Canceled inspection Id : " + inspection.Id);
                }

                //Check if at least one Inspection was canceled
                if (canceledInspectionsCount > 0)
                {
                    //Set the CanceledInspectionsCount output parameter
                    localcontext.PluginExecutionContext.OutputParameters["CanceledInspectionsCount"] = canceledInspectionsCount + " Inspections were canceled";
                }

                //Check if Reason contains in the InputParameter
                if (localcontext.PluginExecutionContext.InputParameters.ContainsKey("Reason"))
                {
                    //Build the Note record and add Trace Message
                    localcontext.Trace("building a note reocord");
                    Entity note = new Entity("annotation");
                    note["subject"] = "Permit Locked";
                    note["notetext"] = "Reason for locking this permit: " + localcontext.PluginExecutionContext.InputParameters["Reason"];
                    note["objectid"] = permitEntityRef;
                    note["objecttypecode"] = permitEntityRef.LogicalName;

                    //Add Trace Message and create the Note record.
                    localcontext.Trace("Creating a note reocord");
                    var createdNoteId = localcontext.OrganizationService.Create(note);

                    //Check if the Note record was created and add Trace Message
                    if (createdNoteId != Guid.Empty)
                    {
                        localcontext.Trace("Note record was created");
                    }
                }
            }

        }
    }
}
