//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SEO.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SEO_AtividadeUsuario
    {
        [Key]
        public System.Guid AU_Id { get; set; }
        public System.Guid AU_IdUsuario { get; set; }
        public System.Guid AU_IdAtividadeObra { get; set; }

        public virtual SEO_AtividadeObra SEO_AtividadeObra { get; set; }
        public virtual SEO_Usuario SEO_Usuario { get; set; }
    }
}
