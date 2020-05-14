using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace AADB2C.UserMigration.API
{
    public class UserMigrationService
    {
        /// <summary>
        /// Create a reference to Azure Blob migration table
        /// </summary>
        private readonly CloudTable _azureCloudTable;

        public UserMigrationService(CloudStorageAccount storageAccount)
        {
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            _azureCloudTable = tableClient.GetTableReference("users");
        }

        /// <summary>
        /// Retrieve user entity from migration table
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<TableEntity> RetrieveUserAsync(string email)
        {
            // Create a retrieve operation that takes a user entity.
            var retrieveOperation = TableOperation.Retrieve<TableEntity>("B2CMigration", email);

            // Execute the retrieve operation.
            var retrievedResult = await _azureCloudTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);

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
        public async Task RemoveUser(string email)
        {
            // Get the customer to remove from the table
            var deleteEntity = await RetrieveUserAsync(email).ConfigureAwait(false);

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {
                var deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                var result = await _azureCloudTable.ExecuteAsync(deleteOperation);
            }
        }
    }
}