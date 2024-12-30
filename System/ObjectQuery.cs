using System;
using System.Collections.Generic;
using System.Text;

namespace MauiRfidSample
{
    public class ObjectQuery
    {
        // individual match fields for the query
        public class ObjectQueryValues
        {
            public string apiFieldName { get; set; }
            public string value { get; set; }
        }

        // the properties of the query
        public class ObjectQueryItem
        {
            public string type { get; set; }
            public string sortExpression { get; set; }
            public bool useItemGroupHierarchy { get; set; }
            public string sortDirection { get; set; }
            public int maxNumResults { get; set; }
            public ObjectQueryValues[] queryValues { get; set; }
        }

        // the root class that is serialized into the JSON to be posted
        public class ObjectQueryRoot
        {
            public ObjectQueryItem ObjectQuery { get; set; }
        }
    }
}
