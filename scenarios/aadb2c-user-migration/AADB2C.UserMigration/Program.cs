using AADB2C.GraphService;
using AADB2C.UserMigration.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.UserMigration
{
    class Program
    {
        private static string Tenant = ConfigurationManager.AppSettings["b2c:Tenant"];
        private static string ClientId = ConfigurationManager.AppSettings["b2c:ClientId"];
        private static string ClientSecret = ConfigurationManager.AppSettings["b2c:ClientSecret"];
        private static string MigrationFile = ConfigurationManager.AppSettings["MigrationFile"];
        private static string BlobStorageConnectionString = ConfigurationManager.AppSettings["BlobStorageConnectionString"];

        static void Main(string[] args)
        {

            if (args.Length <= 0)
            {
                Console.WriteLine("Please enter a command as the first argument.");
                Console.WriteLine("\t1                  : Migrate users with password");
                Console.WriteLine("\t2                  : Migrate users with random password");
                Console.WriteLine("\t3 Email-address  : Get user by email address");
                Console.WriteLine("\t4 Display-name   : Get user by display name");
                Console.WriteLine("\t5                : User migration cleanup");
                return;
            }

            try
            {
                switch (args[0])
                {
                    case "1":
                        MigrateUsersWithPasswordAsync().GetAwaiter().GetResult();
                        break;
                    case "2":
                        MigrateUsersWithRandomPasswordAsync().GetAwaiter().GetResult();
                        break;
                    case "3":
                        if (args.Length == 2)
                        {
                            B2CGraphClient b2CGraphClient = new B2CGraphClient(Program.Tenant, Program.ClientId, Program.ClientSecret);
                            string JSON = b2CGraphClient.SearcUserBySignInNames(args[1]).GetAwaiter().GetResult();
                            var deserialized = JsonConvert.DeserializeObject<GraphUsersModel>(JSON);

                            Console.WriteLine(JsonConvert.SerializeObject(deserialized, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                        }
                        else
                        {
                            Console.WriteLine("Email address parameter is missing");
                        }
                        break;
                    case "4":
                        if (args.Length == 2)
                        {
                            B2CGraphClient b2CGraphClient = new B2CGraphClient(Program.Tenant, Program.ClientId, Program.ClientSecret);
                            string JSON = b2CGraphClient.SearchUserByDisplayName(args[1]).GetAwaiter().GetResult();
                            var deserialized = JsonConvert.DeserializeObject<GraphUsersModel>(JSON);

                            Console.WriteLine(JsonConvert.SerializeObject(deserialized, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                        }
                        else
                        {
                            Console.WriteLine("Display name parameter is missing");
                        }
                        break;
                    case "5":
                        UserMigrationCleanupAsync().GetAwaiter().GetResult();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Migrate users with their password
        /// </summary>
        /// <returns></returns>
        private static async Task MigrateUsersWithPasswordAsync()
        {
            string appDirecotyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dataFilePath = Path.Combine(appDirecotyPath, Program.MigrationFile);

            // Check file existence
            if (!File.Exists(dataFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File '{dataFilePath}' not found");
                Console.ResetColor();
                return;
            }

            // Read the data file and convert to object
            UsersModel users = UsersModel.Parse(File.ReadAllText(dataFilePath));

            // Create B2C graph client object
            B2CGraphClient b2CGraphClient = new B2CGraphClient(Program.Tenant, Program.ClientId, Program.ClientSecret);

            foreach (var item in users.Users)
            {
                await b2CGraphClient.CreateUser(item.email,
                    item.password,
                    item.displayName,
                    item.firstName,
                    item.lastName,
                    users.GenerateRandomPassword);
            }

            Console.WriteLine("Users migrated successfully");
        }

        /// <summary>
        /// Migrate users with random password
        /// </summary>
        /// <returns></returns>
        private static async Task MigrateUsersWithRandomPasswordAsync()
        {
            string appDirecotyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dataFilePath = Path.Combine(appDirecotyPath, Program.MigrationFile);

            // Check file existence
            if (!File.Exists(dataFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File '{dataFilePath}' not found");
                Console.ResetColor();
                return;
            }

            // Read the data file and convert to object
            UsersModel users = UsersModel.Parse(File.ReadAllText(dataFilePath));

            // Create B2C graph client object
            B2CGraphClient b2CGraphClient = new B2CGraphClient(Program.Tenant, Program.ClientId, Program.ClientSecret);

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Program.BlobStorageConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("users");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            foreach (var item in users.Users)
            {
                await b2CGraphClient.CreateUser(item.email,
                    item.password,
                    item.displayName,
                    item.firstName,
                    item.lastName,
                    users.GenerateRandomPassword);

                // Create a new customer entity.
                // Note: Azure Blob Table query is case sensitive, always set the email to lower case
                TableEntity user = new TableEntity("B2CMigration", item.email.ToLower());

                // Create the TableOperation object that inserts the customer entity.
                TableOperation insertOperation = TableOperation.InsertOrReplace(user);

                // Execute the insert operation.
                table.Execute(insertOperation);

            }


            Console.WriteLine("Users migrated successfully");
        }

        /// <summary>
        /// Migration clean up
        /// </summary>
        /// <returns></returns>
        private static async Task UserMigrationCleanupAsync()
        {
            string appDirecotyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dataFilePath = Path.Combine(appDirecotyPath, Program.MigrationFile);

            // Check file existence
            if (!File.Exists(dataFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File '{dataFilePath}' not found");
                Console.ResetColor();
                return;
            }

            // Read the data file and convert to object
            UsersModel users = UsersModel.Parse(File.ReadAllText(dataFilePath));

            // Create B2C graph client object
            B2CGraphClient b2CGraphClient = new B2CGraphClient(Program.Tenant, Program.ClientId, Program.ClientSecret);

            foreach (var item in users.Users)
            {
                Console.WriteLine($"Deleting user '{item.email}'");
                await b2CGraphClient.DeleteAADUserBySignInNames(item.email);
            }
        }
    }
}
