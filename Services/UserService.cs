using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RealtYeahBackend.Entities;
using RealtYeahBackend.Helpers;
using RealtYeahBackend.Models;
using RealtYeahBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace RealtYeahBackend.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        User GetByName(string login);
        User Create(User user, string password);
        void Update(UserUpdateModel user);
        string GenerateToken(User user);
        string Decrypt(string data);
    }

    public class UserService : IUserService
    {
        private readonly RealtyeahContext context;
        private readonly CspParameters csp;
        private readonly AppSettings appSettings;

        public UserService(RealtyeahContext context, CspParameters csp, IOptions<AppSettings> appSettings)
        {
            this.context = context;
            this.csp = csp;
            this.appSettings = appSettings.Value;
        }

        public User Authenticate(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) 
            {
                return null;
            }                

            var user = context.Users.SingleOrDefault(x => x.Login.Equals(login));

            if (user == null)
            {
                return null;
            }               

            if (!PasswordVerification(password, user.Password))
            {
                User error = new User();
                error.Login = "Incorrect password";
                return error;
            }

            return user;
        }

        public User GetByName(string login)
        {
            return context.Users.Where(x => x.Login == login).FirstOrDefault();
        }

        public User Create(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("No password provided or it's empty");
            }               

            if (context.Users.Any(checkUser => checkUser.Login == user.Login)) 
            { 
                throw new ArgumentException("Username \"" + user.Login + "\" is already taken");
            }

            if (!context.Employees.Any(x => x.EmployeeId == user.EmployeeId)) 
            {
                throw new ArgumentException("There is no user with ID \"" + user.EmployeeId + "\" in database");
            }              

            byte[] passwordHash;
            PasswordHashing(password, out passwordHash);

            user.Password = passwordHash;

            if (string.IsNullOrWhiteSpace(user.Role)) {
                user.Role = Role.Agent;
            }          

            context.Users.Add(user);
            context.SaveChanges();

            return user;
        }

        public async void Update(UserUpdateModel userToFind)
        {
            User user = context.Users.Find(userToFind.UserId);

            if (user == null)
            {
                throw new ArgumentNullException("No such user");
            }

            if (!string.IsNullOrWhiteSpace(userToFind.Login) && !userToFind.Login.Equals(user.Login))
            {
                if (context.Users.Any(checkUser => checkUser.Login.Equals(userToFind.Login) && checkUser.UserId != userToFind.UserId)) 
                {
                    throw new ArgumentException("Username \"" + userToFind.Login + "\" is already taken");
                } 

                user.Login = userToFind.Login;
            }

            if (string.IsNullOrWhiteSpace(userToFind.Role) 
                || !(userToFind.Role.Equals(Role.MainAdmin)
                     || userToFind.Role.Equals(Role.Admin)
                     || userToFind.Role.Equals(Role.Agent)))
            {
                throw new ArgumentException("Invalid user role");
            }

            if (!string.IsNullOrWhiteSpace(Encoding.UTF8.GetString(userToFind.Password)))
            {
                if (!PasswordVerification(Encoding.UTF8.GetString(userToFind.Password), user.Password))
                {
                    byte[] passwordHash;
                    PasswordHashing(Encoding.UTF8.GetString(userToFind.Password), out passwordHash);

                    user.Password = passwordHash;
                }               
            }

            if (userToFind.EmployeeId != user.EmployeeId && context.Users.Where(checkUser => checkUser.EmployeeId == userToFind.EmployeeId).Count() > 0)
            {
                throw new ArgumentException("New user employee data assignment is already taken");
            }

            context.Entry(user).State = EntityState.Modified;

            try
            {
                context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        private static void PasswordHashing(string password, out byte[] passwordHash)
        {          
            if (string.IsNullOrWhiteSpace(password)) 
            {
                throw new ArgumentException("No password provided or it's empty");
            }

            string generatedSalt = BCrypt.Net.BCrypt.GenerateSalt(12);
            string generatedHash = BCrypt.Net.BCrypt.HashPassword(password, generatedSalt, true);
            passwordHash = Encoding.UTF8.GetBytes(generatedHash);
        }

        public static bool PasswordVerification(string password, byte[] storedHash)
        {        
            if (string.IsNullOrWhiteSpace(password)) 
            {
                throw new ArgumentException("No password provided or it's empty");
            }
            if (storedHash.Length != 60) 
            {
                throw new ArgumentException("Invalid length of password hash (60 bytes expected).", "passwordHash");
            }  

            return BCrypt.Net.BCrypt.Verify(password, Encoding.UTF8.GetString(storedHash), true);
        }

        public static bool CheckId(ClaimsPrincipal user, int id)
        {
            if (user == null)
            {
                return false;
            }

            if (user.Claims.Where(m => m.Type == ClaimTypes.Role).FirstOrDefault().Value == Role.Admin) 
            {
                return true;
            } 

            var employeeData = user.Claims.Where(m => m.Type == ClaimTypes.UserData).FirstOrDefault();

            int employeeID;
            if (employeeData != null)
            {
                employeeID = int.Parse(employeeData.Value);
                if (employeeID != id)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.UserData, user.EmployeeId.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Audience = AuthOptions.AUDIENCE,
                Issuer = AuthOptions.ISSUER,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public string Decrypt(string data)
        {
            // here container already exists so key from that container is used            
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096, csp))
            {
                return Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(data), false));
            }
        }
    }
}