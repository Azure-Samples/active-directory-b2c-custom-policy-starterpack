using AADB2C.UserMigration.API.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;

namespace AADB2C.UserMigration.API
{
    public class UserMigrationService
    {
        private static string BlobStorageConnectionString = ConfigurationManager.AppSettings["BlobStorageConnectionString"];

        /// <summary>
        /// Retrieve user entity from migration table
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static TableEntity RetrieveUser(string email)
        {
            CloudTable table = GetAzurBlobTable();

            // Create a retrieve operation that takes a user entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<TableEntity>("B2CMigration", email);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
            {
                return (TableEntity)retrievedResult.Result;
            }

            return null;
        }

        /// <summary>
        /// Remove user entity from migration table
        /// </summary>
        /// <param name="email"></param>
        public static void RemoveUser(string email)
        {
            CloudTable table = GetAzurBlobTable();

            // Create a retrieve operation that takes a user entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<TableEntity>("B2CMigration", email);
            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity.
            TableEntity deleteEntity = (TableEntity)retrievedResult.Result;

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);
            }
        }

        /// <summary>
        /// Create a reference to Azure Blob migration table
        /// </summary>
        /// <returns></returns>
        private static CloudTable GetAzurBlobTable()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(BlobStorageConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("users");
            return table;
        }
    }
}