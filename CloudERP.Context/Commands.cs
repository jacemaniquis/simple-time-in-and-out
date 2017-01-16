using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CloudERP.Context.Models;

namespace CloudERP.Context
{
    public static class Commands
    {
        public static void CreateDatabase()
        {
            using (var context = new CloudERPDbContext())
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();
                }

                if (!context.HR_Users.Any())
                {
                    context.HR_Roles.Add(new Context.Models.HR_Roles()
                    {
                        RoleName = "Employee"
                    });

                    var adminRole = new Context.Models.HR_Roles()
                    {
                        RoleName = "Global Admin"
                    };

                    context.HR_Roles.Add(adminRole);
                    context.HR_Roles.Add(new Context.Models.HR_Roles()
                    {
                        RoleName = "Admin"
                    });

                    context.SaveChanges();

                    using (var md5Hash = MD5.Create())
                    {
                        var adminAccount = new Context.Models.HR_Users()
                        {
                            Username = "admin",
                            Password = GetMd5Hash(md5Hash, "admin"),
                            FirstName = "administrator",
                            LastName = "administrator",
                        };


                        context.HR_Users.Add(adminAccount);
                        context.HR_UserRoles.Add(new HR_UserRoles()
                        {
                            From = GetTimezoneDateTime(),
                            HR_Roles = adminRole,
                            HR_Users = adminAccount,
                        });
                    }

                    context.SaveChanges();

                    context.HR_MainSchedule.Add(new HR_MainSchedule()
                    {
                        ScheduleNickname = "Flexitime",
                        IsDefault = true
                    });

                    context.SaveChanges();
                }
            }
        }


        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        public static DateTime GetTimezoneDateTime()
        {
            var serverTime = DateTime.Now;
            var localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime, TimeZoneInfo.Local.Id, ConfigurationManager.AppSettings["Timezone"]);
            return localTime;
        }
    }
}
