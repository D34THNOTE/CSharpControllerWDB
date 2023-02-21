using CSharpControllerWDB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace CSharpControllerWDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudiesController : ControllerBase
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
        public ActionResult<Studies> GetStudies()
        {
            var studiesList = new List<Studies>();
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s23375;Integrated Security=True"))
            {
                try
                {
                    connection.Open();
                    
                    try
                    {
                        using (var command = new SqlCommand("SELECT * FROM Studies", connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var study = new Studies
                                    {
                                        IdStudy = int.Parse(reader["IdStudy"].ToString()),
                                        Name = reader["Name"].ToString()
                                    };

                                    studiesList.Add(study);
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
            return Ok(studiesList);
        }

        
        [HttpGet("{id}")]
        public ActionResult<Studies> Get(int id)
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
                        using (var command = new SqlCommand("SELECT * FROM Studies WHERE IdStudy = @id", connection))
                        {
                            command.Parameters.AddWithValue("@id", id);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var study = new Studies
                                    {
                                        IdStudy = int.Parse(reader["IdStudy"].ToString()),
                                        Name = reader["Name"].ToString()
                                    };

                                    return Ok(study);
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
        public ActionResult<Studies> Post(Studies studyData)
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
                    
                    var command = new SqlCommand("SELECT COUNT(*) FROM Studies WHERE IdStudy = @id", connection);
                    command.Parameters.AddWithValue("@id", studyData.IdStudy);
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
                        return BadRequest("A study with given Id number already exists"); 
                    }

                    var insertCommand = 
                        new SqlCommand(
                        "INSERT INTO Studies (IdStudy, Name) VALUES " +
                        "(@id, @name)", connection);
                    
                    insertCommand.Parameters.AddWithValue("@id", studyData.IdStudy);
                    insertCommand.Parameters.AddWithValue("@name", studyData.Name);
                    
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
                    
                    return Ok(studyData); 
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e); 
                    return StatusCode(500, "An internal error occured while processing your request"); 
                }
            }
        }

        
         
        [HttpPut("{id}")]
        public ActionResult<Studies> Update(int id, [FromBody] Studies updatedStudy) 
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
                    
                    var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Studies WHERE IdStudy = @id", connection); 
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
                        return NotFound($"Study with ID {id} not found"); 
                    }
                    checkCommand.Dispose();
                    
                    var updateCommand = new SqlCommand(
                        "UPDATE Studies SET IdStudy = @id, Name = @name WHERE IdStudy = @id",
                        connection);
                    updateCommand.Parameters.AddWithValue("@id", updatedStudy.IdStudy); 
                    updateCommand.Parameters.AddWithValue("@name", updatedStudy.Name);
                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    
                    if (rowsAffected == 0) 
                    { 
                        return BadRequest("Update failed, no rows affected"); 
                    }
                    updateCommand.Dispose();
                    
                    return Ok(updatedStudy); 
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
                            using (var command = new SqlCommand("SELECT COUNT(*) FROM Studies WHERE IdStudy = @id", connection, transaction))
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
                                    return NotFound("No study with the provided Id was found");
                                }
                            }
                            
                            using (var command = new SqlCommand("DELETE FROM Studies WHERE IdStudy = @id", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", id);
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex) 
                                { 
                                    Console.WriteLine(ex); 
                                    return StatusCode(500, "An error occured when deleting the study, no rows were affected"); 
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
            return Ok($"Study with ID {id} successfully deleted");
        }
    }
}
