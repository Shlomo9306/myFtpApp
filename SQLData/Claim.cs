//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SQLData
{
    using System;
    using System.Collections.Generic;
    
    public partial class Claim
    {
        public int ID { get; set; }
        public string ClaimNumber { get; set; }
        public string Code { get; set; }
        public System.DateTime DataImported { get; set; }
        public int FileName { get; set; }
        public int Status { get; set; }
    }
}
