using System.ComponentModel.DataAnnotations;

namespace CloudERP.Web.Areas.HR.Models
{
   
    public class AddAccountViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }
    }

    public class UpdateAccountViewModel
    {
        public int Id { get; set; }

        public string Username { get; set; }

        [Display(Name = "New Password")]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}