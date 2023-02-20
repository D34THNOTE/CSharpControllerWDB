using System.ComponentModel.DataAnnotations;

namespace CSharpControllerWDB.Models
{
    public class Student
    {
        [Required(ErrorMessage = "Index number is required")]
        [RegularExpression(@"^s\d+$", ErrorMessage = "Invalid index number format")]
        public string IndexNumber { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Date of birth is required")]
        // Formatting Date
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }
        
        [Required(ErrorMessage = "ID of the enrollment is required")]
        public int IdEnrollment { get; set; }
    }
}
