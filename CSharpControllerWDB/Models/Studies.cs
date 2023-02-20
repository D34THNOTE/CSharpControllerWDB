using System.ComponentModel.DataAnnotations;

namespace CSharpControllerWDB.Models
{
    public class Studies
    {
        [Required(ErrorMessage = "ID of the Study is required")]
        public int IdStudy { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        

    }
}