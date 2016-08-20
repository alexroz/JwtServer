using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace JwtAuthServer.Data
{
    public class TokenEntity : TableEntity
    {
        public TokenEntity()
        {            
        }
        public TokenEntity(string id)
        {
            this.PartitionKey = "directedje_tokens";
            this.RowKey = id;
        }
        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }
        [Required]
        [MaxLength(50)]
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        [Required]
        public string ProtectedTicket { get; set; }

        public string ClientId { get; set; }
    }

    public class AuthRepositoryTableStorage: IDisposable
    {
        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            var table = GetTable();

            var tokenEntity = new TokenEntity(token.Id)
            {
                Subject = token.Subject,
                ExpiresUtc = token.ExpiresUtc,
                IssuedUtc = token.IssuedUtc,
                ProtectedTicket = token.ProtectedTicket,
                ClientId = token.ClientId
            };

            await TryRemoveRefreshToken(token.Id);

            var insertOperation = TableOperation.Insert(tokenEntity);
            await table.ExecuteAsync(insertOperation);
            return true;
        }

        private static CloudTable GetTable()
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=rjpblob;AccountKey=zAM77QTvJEG+4X/mifkbQXmopaY9ANKjj8fEmqs5lBrW6w6Lh67/lb6FhoFUtTOS+igiYwLpCOLBf7nWx0I1PQ==");
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("tokens");
            table.CreateIfNotExists();
            return table;
        }

        public async Task<bool> TryRemoveRefreshToken(string refreshTokenId)
        {
            var retrievedResult = await FindRefreshTokenInternal(refreshTokenId);

            // Assign the result to a CustomerEntity.
            var deleteEntity = retrievedResult.Result as TokenEntity;

            if (deleteEntity != null)
            {
                var table = GetTable();
                var deleteOperation = TableOperation.Delete(deleteEntity);
                await table.ExecuteAsync(deleteOperation);
                return true;
            }
            return false;
        }

        public async Task<bool> TryRemoveRefreshToken(RefreshToken refreshToken)
        {
            return await TryRemoveRefreshToken(refreshToken.Id);
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            var result = await FindRefreshTokenInternal(refreshTokenId);
            var token = (TokenEntity) result.Result;
            return new RefreshToken
            {
                ClientId = token.ClientId,
                ExpiresUtc = token.ExpiresUtc,
                IssuedUtc = token.IssuedUtc,
                ProtectedTicket = token.ProtectedTicket,
                Id = token.RowKey,
                Subject = token.Subject
            };
        }

        private async Task<TableResult> FindRefreshTokenInternal(string refreshTokenId)
        {
            var table = GetTable();

            var retrieveOperation = TableOperation.Retrieve<TokenEntity>("directedje_tokens", refreshTokenId);
            // Execute the operation.
            var result = await table.ExecuteAsync(retrieveOperation);

            return result;

            // Assign the result to a CustomerEntity.
            //return (TokenEntity)retrievedResult.Result;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}