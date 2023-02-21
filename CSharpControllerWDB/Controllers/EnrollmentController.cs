using CSharpControllerWDB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace CSharpControllerWDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private BadRequestObjectResult? ValidateIdFormat(int id)
        {
            if (id < 1)
            {
                return BadRequest("Invalid Id. Id has to be a positive integer");
            }

            return null;
        }
        
        
        [HttpGet]
        public ActionResult<Enrollment> GetEnrollments()
        {
            var enrollments = new List<Enrollment>();
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True"))
            {
                try
                {
                    connection.Open();
                    
                    try
                    {
                        using (var command = new SqlCommand("SELECT * FROM Enrollment", connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var enrollment = new Enrollment
                                    {
                                        IdEnrollment = int.Parse(reader["IdEnrollment"].ToString()),
                                        Semester = int.Parse(reader["Semester"].ToString()),
                                        StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                        IdStudy = int.Parse(reader["IdStudy"].ToString())
                                    };

                                    enrollments.Add(enrollment);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return StatusCode(500,"An internal error occured while processing your request");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return StatusCode(500,"An internal error occured while processing your request");
                }
                finally
                {
                    connection.Dispose(); 
                } 
            }
            return Ok(enrollments);
        }

        
        [HttpGet("{id}")]
        public ActionResult<Enrollment> Get(int id)
        {
            var validationError = ValidateIdFormat(id);
            if (validationError != null)
            {
                return validationError;
            }
            
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True"))
            {
                try
                {
                    connection.Open();
                    
                    try
                    {
                        using (var command = new SqlCommand("SELECT * FROM Enrollment WHERE IdEnrollment = @id", connection))
                        {
                            command.Parameters.AddWithValue("@id", id);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var enrollment = new Enrollment
                                    {
                                        IdEnrollment = int.Parse(reader["IdEnrollment"].ToString()),
                                        Semester = int.Parse(reader["Semester"].ToString()),
                                        StartDate = Convert.ToDateTime(reader["StartDate"].ToString()),
                                        IdStudy = int.Parse(reader["IdStudy"].ToString())
                                    };

                                    return Ok(enrollment);
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
                        Console.WriteLine(ex);
                        return StatusCode(500,"An internal error occured while processing your request");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return StatusCode(500,"An internal error occured while processing your request");
                }
                finally
                {
                    connection.Dispose(); 
                } 
            }
        }
        
        
        [HttpPost]
        public ActionResult<Enrollment> Post(Enrollment enrollmentData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True"))
            {
                try 
                { 
                    connection.Open();
                    
                    var command = new SqlCommand("SELECT COUNT(*) FROM Enrollment WHERE IdEnrollment = @id", connection);
                    command.Parameters.AddWithValue("@id", enrollmentData.IdEnrollment);
                    int count;
                    
                    try
                    {
                        count = (int)command.ExecuteScalar();
                    }
                    catch (Exception ex) 
                    { 
                        Console.WriteLine(ex); 
                        return StatusCode(500, "There was an error when verifying the uniqueness of the provided Id number"); 
                    }
                    command.Dispose();
                     
                    if (count > 0) 
                    { 
                        return BadRequest("An enrollment with given Id number already exists"); 
                    }

                    var insertCommand = 
                        new SqlCommand(
                        "INSERT INTO Enrollment (IdEnrollment, Semester, StartDate, IdStudy) VALUES " +
                        "(@id, @semester, @startdate, @idstudy)", connection);
                    
                    insertCommand.Parameters.AddWithValue("@id", enrollmentData.IdEnrollment); 
                    insertCommand.Parameters.AddWithValue("@semester", enrollmentData.Semester); 
                    insertCommand.Parameters.AddWithValue("@startdate", enrollmentData.StartDate); 
                    insertCommand.Parameters.AddWithValue("@idstudy", enrollmentData.IdStudy);
                    
                    try 
                    { 
                        insertCommand.ExecuteNonQuery(); 
                    }
                    catch (Exception ex) 
                    { 
                        Console.WriteLine(ex); 
                        return StatusCode(500, "There was an error when trying to insert the new record"); 
                    }
                    insertCommand.Dispose();
                    
                    return Ok(enrollmentData); 
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e); 
                    return StatusCode(500, "An internal error occured while processing your request"); 
                }
            }
        }

        
         
        [HttpPut("{id}")]
        public ActionResult<Enrollment> Update(int id, [FromBody] Enrollment updatedEnrollment) 
        { 
            var validationError = ValidateIdFormat(id); 
            if (validationError != null) 
            { 
                return validationError; 
            }
            
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True")) 
            { 
                try 
                { 
                    connection.Open();
                    

                    var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Enrollment WHERE IdEnrollment = @id", connection); 
                    checkCommand.Parameters.AddWithValue("@id", id);
                    int count;
                    try
                    {
                        count = (int)checkCommand.ExecuteScalar(); 
                    }
                    catch (Exception ex) 
                    { 
                        Console.WriteLine(ex); 
                        return StatusCode(500, "There was an error verifying the uniqueness of the provided Id number"); 
                    }
                    
                    if (count == 0) 
                    { 
                        return NotFound($"Enrollment with ID {id} not found"); 
                    }
                    checkCommand.Dispose();
                    
                    var updateCommand = new SqlCommand(
                        "UPDATE Enrollment SET Semester = @semester, StartDate = @startdate, " + 
                        "IdStudy = @idstudy WHERE IdEnrollment = @id", 
                        connection); 
                    updateCommand.Parameters.AddWithValue("@id", updatedEnrollment.IdEnrollment); 
                    updateCommand.Parameters.AddWithValue("@semester", updatedEnrollment.Semester); 
                    updateCommand.Parameters.AddWithValue("@startdate", updatedEnrollment.StartDate); 
                    updateCommand.Parameters.AddWithValue("@idstudy", updatedEnrollment.IdStudy);
                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    
                    if (rowsAffected == 0) 
                    { 
                        return BadRequest("Update failed, no rows affected"); 
                    }
                    updateCommand.Dispose();
                    
                    return Ok(updatedEnrollment); 
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e); 
                    return StatusCode(500, "An internal error occured while processing your request"); 
                } 
            } 
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var validationError = ValidateIdFormat(id);
            if (validationError != null)
            {
                return validationError;
            }

            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True"))
            {
                try
                {
                    connection.Open();
                    
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SqlCommand("SELECT COUNT(*) FROM Enrollment WHERE IdEnrollment = @id", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", id);
                                int count;
                                try
                                {
                                    count = (int)command.ExecuteScalar();
                                }
                                catch (Exception ex) 
                                { 
                                    Console.WriteLine(ex); 
                                    return StatusCode(500, "There was an error when verifying the uniqueness of the provided Id number"); 
                                }

                                if (count == 0)
                                {
                                    return NotFound("No enrollment with the provided Id was found");
                                }
                            }
                            
                            using (var command = new SqlCommand("DELETE FROM Enrollment WHERE IdEnrollment = @id", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", id);
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex) 
                                { 
                                    Console.WriteLine(ex); 
                                    return StatusCode(500, "An error occured when deleting the enrollment, no rows were affected"); 
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine(ex);
                            return StatusCode(500, "An internal error occured while processing your request");
                        }
                    } 
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e); 
                    return StatusCode(500, "An internal error occured while processing your request"); 
                }
            }
            return Ok($"Enrollment with ID {id} successfully deleted");
        }
    }
}
