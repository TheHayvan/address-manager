//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Address_Manager
{
    using System;
    using System.Collections.Generic;
    
    public partial class Addresses
    {
        public int AddressID { get; set; }
        public string Vorname { get; set; }
        public string Name { get; set; }
        public string Firma { get; set; }
        public string Strasse { get; set; }
        public string Hausnummer { get; set; }
        public int LocationID { get; set; }
    
        public virtual Locations Locations { get; set; }
    }
}
