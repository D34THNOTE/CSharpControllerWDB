using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSharpControllerWDB.Models
{
    public class Enrollment
    {
        [Required(ErrorMessage = "ID of enrollment is required")]
        public int IdEnrollment { get; set; }
        
        [Required(ErrorMessage = "Semester number is required")]
        public int Semester { get; set; }
        
        [Required(ErrorMessage = "Start date of the semester is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [ForeignKey("Studies")]
        [Required(ErrorMessage = "ID of the related study is required")]
        public int IdStudy { get; set; }
    }
}