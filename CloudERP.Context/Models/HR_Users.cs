using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_Users")]
    public class HR_Users
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }
        
        public string NfcRfidSerial { get; set; }

        public byte[] Image { get; set; }

        public bool IsDeleted { get; set; }
    }
}
