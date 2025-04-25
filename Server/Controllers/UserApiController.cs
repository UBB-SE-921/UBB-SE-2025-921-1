using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
    [Route("/users")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IUserRepository userRepository;

        public UserApiController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        // GET /users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<User?>> GetUserByEmail(string email)
        {
            // Basic validation: Check if email is provided
            if (string.IsNullOrEmpty(email))
            {
                return this.BadRequest("Email address is required."); // Return 400 Bad Request
            }

            try
            {
                User? user = await this.userRepository.GetUserByEmail(email);

                return this.Ok(user);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting user by email.");
            }
        }

        // GET /users/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<User?>> GetUserByUsername(string username)
        {
            // Basic validation: Check if username is provided
            if (string.IsNullOrEmpty(username))
            {
                return this.BadRequest("user name is required."); // Return 400 Bad Request
            }

            try
            {
                User? user = await this.userRepository.GetUserByUsername(username);

                return this.Ok(user);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting user by username.");
            }
        }

        // GET /users/phone-email/{userId}
        [HttpGet("phone-email/{userId}")]
        public async Task<ActionResult<User?>> GetUserPhoneNumberAndEmailById(int userId)
        {
            try
            {
                User user = new User { UserId = userId };
                await this.userRepository.LoadUserPhoneNumberAndEmailById(user);

                return this.Ok(user);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting phone number and email.");
            }
        }

        // POST /users
        [HttpPost]
        public async Task<ActionResult> AddUser([FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required."); // Return 400 Bad Request
            }

            try
            {
                await this.userRepository.AddUser(user);
                return this.Ok();
            }
            catch
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding user.");
            }
        }

        // PUT /users
        [HttpPut]
        public async Task<ActionResult> UpdateUser([FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required."); // Return 400 Bad Request
            }

            try
            {
                await this.userRepository.UpdateUser(user);
                return this.Ok();
            }
            catch
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating user.");
            }
        }

        // GET /users/email-exists?email={email}
        [HttpGet("email-exists")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> EmailExists([FromQuery] string email)
        {
            // Basic validation: Check if email is provided
            if (string.IsNullOrEmpty(email))
            {
                return this.BadRequest("Email address is required."); // Return 400 Bad Request
            }

            try
            {
                bool exists = await this.userRepository.EmailExists(email);
                return this.Ok(exists);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking email existence.");
            }
        }

        // GET /users/username-exists?username={username}
        [HttpGet("username-exists")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> UsernameExists([FromQuery] string username)
        {
            // Basic validation: Check if email is provided
            if (string.IsNullOrEmpty(username))
            {
                return this.BadRequest("Username is required."); // Return 400 Bad Request
            }

            try
            {
                bool exists = await this.userRepository.UsernameExists(username);
                return this.Ok(exists);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking username existence.");
            }
        }

        // GET /users/all
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await this.userRepository.GetAllUsers();
                return this.Ok(users);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting all users.");
            }
        }

        // GET users/failed-logins-count/{userId}
        [HttpGet("failed-logins-count/{userId}")]
        public async Task<ActionResult<int>> GetFailedLoginsCountByUserId(int userId)
        {
            try
            {
                var failedLoginsCount = await this.userRepository.GetFailedLoginsCountByUserId(userId);
                return this.Ok(failedLoginsCount);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting users failed login count.");
            }
        }

        // PUT /users/update-failed-logins/{failedLoginsCount}
        [HttpPut("update-failed-logins/{failedLoginsCount}")]
        public async Task<ActionResult> UpdateFailedLoginsCount(int failedLoginsCount, [FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required."); // Return 400 Bad Request
            }

            try
            {
                await this.userRepository.UpdateUserFailedLoginsCount(user, failedLoginsCount);
                return this.Ok();
            }
            catch
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating failed logins count.");
            }
        }

        // PUT /users/update-phone-number
        [HttpPut("update-phone-number")]
        public async Task<ActionResult> UpdatePhoneNumber([FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required."); // Return 400 Bad Request
            }

            try
            {
                await this.userRepository.UpdateUserPhoneNumber(user);
                return this.Ok();
            }
            catch
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating user phone number.");
            }
        }

        // GET /users/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetTotalNumberOfUsers()
        {
            try
            {
                var usersCount = await this.userRepository.GetTotalNumberOfUsers();
                return this.Ok(usersCount);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting total number of users.");
            }
        }

        // DELETE api/<UserApiController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
