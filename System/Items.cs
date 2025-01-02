using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MauiRfidSample
{
    public class Items
    {
        // the service URL to this query
        private const string _ServiceURL = "/services/apexrest/PBSI/ObjectQuery";

        // the class that will be returned by this query
        public class Item
        {
            [JsonProperty("Id")]
            public string ID { get; set; }

            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Use_For_RFID__c")]
            public bool UseForRFID { get; set; }

            [JsonProperty("PBSI__Item_Type__c")]
            public string ItemType { get; set; }

            [JsonProperty("PBSI__Bom_Type__c")]
            public string BOMType { get; set; }

            [JsonProperty("PBSI__description__c")]
            public string Description { get; set; }

            [JsonProperty("PBSI__Item_Group__c")]
            public string ItemGroupID { get; set; }

            [JsonProperty("PBSI__Default_Location__c")]
            public string DefaultLocationID { get; set; }

            [JsonProperty("Item_URL__c")]
            public string ItemURL { get; set; }

            [JsonProperty("Color__c")]
            public string Color { get; set; }

            [JsonProperty("Style__c")]
            public string Style { get; set; }

            [JsonProperty("Material__c")]
            public string Material { get; set; }

            [JsonProperty("PBSI__UPC_Code__c")]
            public string UPC { get; set; }

            [JsonProperty("PBSI__RFID_Item_Reference__c")]
            public string RFIDItemReference { get; set; }

            [JsonProperty("RFID_Tag_Count__c")]
            public int TagCount { get; set; }

            // get the Color, Material, Style as a bindable property
            public string ColorMaterialStyle
            {
                get
                {
                    string strResult = "";

                    // is there a color?
                    if (!string.IsNullOrWhiteSpace(Color))
                    {
                        // add separator, if needed
                        if (strResult.Length > 0) strResult += ", ";

                        // add value
                        strResult += Color;
                    }

                    // is there a material?
                    if (!string.IsNullOrWhiteSpace(Material))
                    {
                        // add separator, if needed
                        if (strResult.Length > 0) strResult += ", ";

                        // add value
                        strResult += Material;
                    }

                    // is there a style?
                    if (!string.IsNullOrWhiteSpace(Style))
                    {
                        // add separator, if needed
                        if (strResult.Length > 0) strResult += ", ";

                        // add value
                        strResult += Style;
                    }

                    // done
                    return strResult;
                }
            }
        }
        // get the items
        public async Task<Item[]> GetItems(AscentWebService ws)
        {
            Item[] ia = null;

            // get a list of items
            ObjectQuery.ObjectQueryItem oq = new ObjectQuery.ObjectQueryItem();
            oq.type = "item";
            oq.sortExpression = "";
            oq.useItemGroupHierarchy = true;
            oq.sortDirection = "ASC";
            oq.maxNumResults = 50000;

            // currently, do not limit to RFID items only
            //oq.queryValues = new ObjectQuery.ObjectQueryValues[0];
            oq.queryValues = new ObjectQuery.ObjectQueryValues[1];
            oq.queryValues[0] = new ObjectQuery.ObjectQueryValues() { apiFieldName = "Use_For_RFID__c", value = "true" };

            // Complete the object query
            ObjectQuery.ObjectQueryRoot oqr = new ObjectQuery.ObjectQueryRoot();
            oqr.ObjectQuery = oq;

            // authenticate and get the data from the web service
            ia = await ws.PostURL<Item[]>(_ServiceURL, oqr);

            // make sure all item Tag Counts for RFID items are at least 1
            foreach (Item ix in ia)
            {
                // see if its an rfid item
                if (ix.UseForRFID)
                {
                    // bounds check
                    if (ix.TagCount <= 0) ix.TagCount = 1;
                }
                else ix.TagCount = 0;
            }

            // done
            return ia;
        }

        // get the items
        public static async Task<Item> GetSingleItem(AscentWebService ws, string strItemID)
        {
            Item itm = null;

            // get a list of item groups
            ObjectQuery.ObjectQueryItem oq = new ObjectQuery.ObjectQueryItem();
            oq.type = "item";
            oq.sortExpression = "";
            oq.useItemGroupHierarchy = true;
            oq.sortDirection = "ASC";
            oq.maxNumResults = 1000;
            oq.queryValues = new ObjectQuery.ObjectQueryValues[1];
            oq.queryValues[0] = new ObjectQuery.ObjectQueryValues() { apiFieldName = "Record_Id__c", value = strItemID };
            ObjectQuery.ObjectQueryRoot oqr = new ObjectQuery.ObjectQueryRoot();
            oqr.ObjectQuery = oq;

            // authenticate and get the data from the web service
            Item[] ia = await ws.PostURL<Item[]>(_ServiceURL, oqr);
            if (ia.Length > 0) itm = ia[0];

            // bounds check
            if (itm.TagCount <= 0) itm.TagCount = 1;

            // done
            return itm;
        }
    }
}
