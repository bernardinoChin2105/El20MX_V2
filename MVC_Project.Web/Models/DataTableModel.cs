using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MVC_Project.Web.Models
{
    [DataContract]
    public class DataTableModel
    {
        [DataMember(Name = "length")]
        public int Length { get; set; }

        [DataMember(Name = "start")]
        public int Start { get; set; }

        [DataMember(Name = "columns")]
        public DataTableFields[] Columns { get; set; }

        [DataMember(Name = "order")]
        public MVC_Project.Web.Models.DataTableFields.DataTableOrder[] Order { get; set; }
    }

    [DataContract]
    public class DataTableFields
    {
        [DataMember(Name = "data")]
        public string Data { get; set; }
        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;

        }

        [DataContract]
        public class DataTableOrder
        {
            [DataMember(Name = "column")]
            public int Column { get; set; }
            [DataMember(Name = "dir")]
            public string Dir { get; set; }
        }
    }
}