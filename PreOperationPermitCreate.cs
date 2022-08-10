using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ContosoPackagePoject;

namespace ContosoPackageProject
{
    public class PreOperationPermitCreate : PluginBase
    {
        protected override void ExecuteCDSPlugin(LocalPluginContext localcontext)

        {

            base.ExecuteCDSPlugin(localcontext);
            //get the Target Table
            var permitEntity = localcontext.PluginExecutionContext.InputParameters["Target"] as Entity;
            //get the Build Site Table reference
            var buildSiteRef = permitEntity["contoso_buildsite"] as EntityReference;
            // add Trace Messages
            localcontext.Trace("Primary Entity Id: " + permitEntity.Id);
            localcontext.Trace("Build Site Entity Id: " + buildSiteRef.Id);
            //Create Fetch xml and that will get the count of locked permits matching the build site id and call retrieve multiple.
            string fetchString = 
                "<fetch output-format='xml-platform' distinct='false' version='1.0' mapping='logical' " +
                "aggregate='true'><entity name='contoso_permit'><attribute name='contoso_permitid' " +
                "alias='Count' aggregate='count' /><filter type='and' ><condition attribute='contoso_buildsite' " +
                "uitype='contoso_buildsite' operator='eq' value='{" + buildSiteRef.Id + "}'/><condition " +
                "attribute='statuscode' operator='eq' value='463270000'/></filter></entity></fetch>";

        }
    }
}
