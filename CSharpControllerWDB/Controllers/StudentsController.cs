using CSharpControllerWDB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace CSharpControllerWDB.Controllers
{
    [ApiController]
    // [controller] is replaced with the name of this class minus the "Controller" word, so if it was called CEyyghgfhfController, then the URL would be api/CEyyghgfhf
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        // has to be the "Object" version because BadRequest contains a string, without the string the return type would be BadRequestResult?
        private BadRequestObjectResult? ValidateIdFormat(string id)
        {
            bool isValidIndexNumber = Regex.IsMatch(id, "^s\\d+$");
            if (!isValidIndexNumber)
            {
                return BadRequest("Invalid ID format. The accepted format looks like: 's1234' ");
            }

            return null;
        }
        
        
        [HttpGet]
        public ActionResult<Student> GetStudents()
        {
            var students = new List<Student>();
            // Create the connection to the database
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True"))
            {
                // In case connection doesn't work application won't crash
                try
                {
                    // Open the connection
                    connection.Open();

                    // In case the sql query doesn't work application won't crash
                    try
                    {
                        // Create the command to retrieve the list of students
                        using (var command = new SqlCommand("SELECT * FROM Student", connection))
                        {
                            // Execute the command and read the results
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var student = new Student
                                    {
                                        IndexNumber = reader["IndexNumber"].ToString(),
                                        FirstName = reader["FirstName"].ToString(),
                                        LastName = reader["LastName"].ToString(),
                                        BirthDate = DateTime.Parse(reader["BirthDate"].ToString()),
                                        IdEnrollment = int.Parse(reader["IdEnrollment"].ToString())
                                    };

                                    students.Add(student);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500,"An internal error occured while processing your request");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return StatusCode(500,"An internal error occured while processing your request");
                }
                finally
                {
                    connection.Dispose(); 
                } 
            }
            return Ok(students);
        }

        
        [HttpGet("{id}")]
        public ActionResult<Student> Get(string id)
        {
            var validationError = ValidateIdFormat(id);
            if (validationError != null)
            {
                return validationError;
            }

            // Create the connection to the database
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True"))
            {
                // In case connecting to the database doesn't work the app won't crash
                try
                {
                    // Open the connection
                    connection.Open();

                    // In case the sql query doesn't work application won't crash
                    try
                    {
                        // Create the command to retrieve the list of students
                        using (var command = new SqlCommand("SELECT * FROM Student WHERE IndexNumber = @id", connection))
                        {
                            command.Parameters.AddWithValue("@id", id);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var student = new Student
                                    {
                                        IndexNumber = reader["IndexNumber"].ToString(),
                                        FirstName = reader["FirstName"].ToString(),
                                        LastName = reader["LastName"].ToString(),
                                        BirthDate = Convert.ToDateTime(reader["BirthDate"]),
                                    };

                                    return Ok(student);
                                }
                                else
                                {
                                    return NotFound();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return StatusCode(500,"An internal error occured while processing your request");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return StatusCode(500,"An internal error occured while processing your request");
                }
                finally
                {
                    connection.Dispose(); 
                } 
            }
        }
        
        
        [HttpPost]
        public ActionResult<Student> Post(Student studentData)
        {
            // Built-in class, ModelState will be replaced with "student" after successful binding incoming data to the Student object(model binding)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create the connection to the database
            using var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True");
            // This query returns the number of rows in Student that much IndexNumber value specified as the parameter
            using var command = new SqlCommand("SELECT COUNT(*) FROM Student WHERE IndexNumber = @id", connection);
            command.Parameters.AddWithValue("@id", studentData.IndexNumber);

            try
            {
                connection.Open();
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    return BadRequest("A student with given index number already exists");
                }
                
                // Basic SqlCommand methods:
                // ExecuteNonQuery: Used for executing SQL commands that do not return any result set, like INSERT, UPDATE, or DELETE.
                // ExecuteScalar: Used for executing SQL commands that return a single value, such as COUNT(*), SUM(column), or MAX(column).
                // ExecuteReader: Used for executing SQL commands that return a result set, and allows you to read the results row by row.
            
                using var insertCommand =
                    new SqlCommand(
                        "INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES " +
                        "(@id, @firstname, @lastname, @birthdate, @idenrollment)");
            
                insertCommand.Parameters.AddWithValue("@id", studentData.IndexNumber);
                insertCommand.Parameters.AddWithValue("@firstname", studentData.FirstName);
                insertCommand.Parameters.AddWithValue("@lastname", studentData.LastName);
                insertCommand.Parameters.AddWithValue("@birthdate", studentData.BirthDate);
                insertCommand.Parameters.AddWithValue("@idenrollment", studentData.IdEnrollment);

                try
                {
                    insertCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return StatusCode(500, "An internal error occured while processing your request");
                }

                return Ok(studentData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "An internal error occured while processing your request");
            }
            
        }

        /*
         
        [HttpPut("{id}")]
        public ActionResult<Student> Update(string id, [FromBody] Student updatedStudent)
        {
            var validationError = ValidateIdFormat(id);
            if (validationError != null)
            {
                return validationError;
            }

            // UpdateStudent returns a false if no match was found and true after overwriting the .csv file
            bool success = CsvDataHandler.UpdateStudent(updatedStudent);
            if (!success)
            {
                return BadRequest("There was no match for the provided student, update failed");
            }

            return Ok(updatedStudent);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var validationError = ValidateIdFormat(id);
            if (validationError != null)
            {
                return validationError;
            }

            var deleted = CsvDataHandler.DeleteStudent(id);

            if(deleted)
            {
                return Ok($"Student with ID {id} successfully deleted");
            } 
            else
            {
                return NotFound($"Student with ID {id} not found");
            }
        }

        public StudentsController()
        {

        }
        */
    }
}
