using System;
using System.Configuration;
using PartnerServiceReference;
using MetadataServiceReference;


namespace tc
{
    class Program
    {
        static void Main(string[] args)
        {
            const string CUSTOM_METADATA_TYPE_NAME = "Machine_Part_Config__mdt";
            const string CUSTOM_METADATA_ITEM_NAME = "Machine_Part_Config__mdt.Part_A";
            const string FIELD_TO_UPDATE = "Canvas_Scale__c";
            const string NEW_VALUE = "100";

            try
            {
                String serverUrl = ConfigurationManager.AppSettings.Get("serverurl");
                String username = ConfigurationManager.AppSettings.Get("username");
                String password = ConfigurationManager.AppSettings.Get("password");
                String orgId = ConfigurationManager.AppSettings.Get("orgId");

                MetadataServiceReference.CustomObject fs = getObjectDefinition(serverUrl, username, password, orgId, CUSTOM_METADATA_TYPE_NAME);

                // display the retrieved field sets
                if (null == fs)
                {
                    Console.WriteLine("Failed to locate object");
                    return;
                }

                // looks like we got something, render out the fields
                foreach (CustomField fld in fs.fields)
                    Console.WriteLine(fld.label);

                // locate to item to update, report out the current values, update the target field
                MetadataServiceReference.CustomMetadata mdt = getCustomMetadata(serverUrl, username, password, orgId, CUSTOM_METADATA_ITEM_NAME); 
                Console.WriteLine(mdt.fullName);
                Boolean found = false;
                foreach (CustomMetadataValue val in mdt.values)
                {
                    Console.WriteLine(val.field + ": " + val.value);

                    if(FIELD_TO_UPDATE == val.field)
                    {
                        found = true;
                        val.value = NEW_VALUE;
                    }
                }
                if( false == found)
                    throw new Exception("Failed to locate field to update");

                // update the found item and then output the new values
                writeCustomMetadata(serverUrl, username, password, orgId, mdt);
                foreach (CustomMetadataValue val in mdt.values)
                    Console.WriteLine(val.field + ": " + val.value);
            }
            catch (Exception x)
            {
                Console.WriteLine("Failed!" + x.Message);
            }
        }

        /// <summary>
        /// Get the metadata definition for a give standard object/custom object/custom metadata type
        /// </summary>
        /// <param name="serverUrl">URL to SF server</param>
        /// <param name="username">Login username</param>
        /// <param name="password">Login password</param>
        /// <param name="orgId">Organization ID found at settings/company settings/company information/salesforce.com organization id</param>
        /// <param name="objName">API name of custom object</param>
        /// <returns>CustomObject definition, null if requested object not found</returns>        
        static MetadataServiceReference.CustomObject getObjectDefinition(String serverUrl, String username, String password, String orgId, String objName)
        {
            Metadata[] metadata = readObjects(serverUrl, username, password, orgId, "CustomObject", new string[] { objName });
            if (metadata[0].fullName == null)
            {
                return null;
            }

            return (MetadataServiceReference.CustomObject)metadata[0];
        }

        /// <summary>
        /// Get a custom metadata value object for the passed objName
        /// </summary>
        /// <param name="serverUrl">URL to SF server</param>
        /// <param name="username">Login username</param>
        /// <param name="password">Login password</param>
        /// <param name="orgId">Organization ID found at settings/company settings/company information/salesforce.com organization id</param>
        /// <param name="objName">API name of custom object</param>
        /// <returns>CustomMetadata for custom metadata instance</returns>
        static MetadataServiceReference.CustomMetadata getCustomMetadata(String serverUrl, String username, String password, String orgId, String objName)
        {
            Metadata[] metadata = readObjects(serverUrl, username, password, orgId, "CustomMetadata", new string[] { objName });
            if (metadata[0].fullName == null)
            {
                return null;
            }

            return (MetadataServiceReference.CustomMetadata)metadata[0];
        }

        /// <summary>
        /// Update the field values of a custom metadata value
        /// </summary>
        /// <param name="serverUrl">URL to SF server</param>
        /// <param name="username">Login username</param>
        /// <param name="password">Login password</param>
        /// <param name="orgId">Organization ID found at settings/company settings/company information/salesforce.com organization id</param>
        /// <param name="mdt">Contians the new values of the custom metadata value</param>
        static void writeCustomMetadata(String serverUrl, String username, String password, String orgId, MetadataServiceReference.CustomMetadata mdt)
        {
            writeObjects(serverUrl, username, password, orgId, new MetadataServiceReference.CustomMetadata[] { mdt });
        }

        /// Helper functions
        ///
        static MetadataServiceReference.Metadata[] readObjects(String serverUrl, String username, String password, String orgId, String objectType, String[] objectNames)
        {
            // establish session
            SoapClient client = new SoapClient();
            var res = client.login(new PartnerServiceReference.LoginScopeHeader { organizationId = orgId }, null, username, password);
            var sessionId = res.sessionId;
            Console.WriteLine("Session ID: " + sessionId);

            // get object metadata, including fieldsets
            MetadataPortTypeClient mclient = new MetadataPortTypeClient(MetadataPortTypeClient.EndpointConfiguration.Metadata, res.metadataServerUrl);
            Metadata[] metadata = mclient.readMetadata(new MetadataServiceReference.SessionHeader { sessionId = sessionId }, null, objectType, objectNames);

            // close session
            client = new SoapClient(SoapClient.EndpointConfiguration.Soap, res.serverUrl);
            client.logout(new PartnerServiceReference.SessionHeader { sessionId = sessionId }, null);

            return metadata;
        }

        static void writeObjects(String serverUrl, String username, String password, String orgId, MetadataServiceReference.Metadata[] mdt)
        {
            // establish session
            SoapClient client = new SoapClient();
            var res = client.login(new PartnerServiceReference.LoginScopeHeader { organizationId = orgId }, null, username, password);
            var sessionId = res.sessionId;
            Console.WriteLine("Session ID: " + sessionId);

            // get object metadata, including fieldsets
            MetadataPortTypeClient mclient = new MetadataPortTypeClient(MetadataPortTypeClient.EndpointConfiguration.Metadata, res.metadataServerUrl);
            var response = mclient.updateMetadata(new MetadataServiceReference.SessionHeader { sessionId = sessionId }, null, null, mdt);

            // close session
            client = new SoapClient(SoapClient.EndpointConfiguration.Soap, res.serverUrl);
            client.logout(new PartnerServiceReference.SessionHeader { sessionId = sessionId }, null);
        }


    }
}
