using System;
using System.Collections.Generic;
using System.Linq;
using ContosoBot.Models;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContosoBot
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<Transactions> transactionsTable;
        private IMobileServiceTable<Users> usersTable;


        private AzureManager()
        {
            this.client = new MobileServiceClient("https://contosobankbotbot.azurewebsites.net");
            this.transactionsTable = this.client.GetTable<Transactions>();
            this.usersTable = this.client.GetTable<Users>();

        }

        public async Task AddUser(Users user)
        {
            await this.usersTable.InsertAsync(user);
        }

        public async Task<List<Users>> GetUsers()
        {
            return await this.usersTable.ToListAsync();
        }

        public async Task<List<Users>> GetSpecificUser(string name)
        {
            return await usersTable.Where(user => user.username == name).ToListAsync();
        }

        public async Task UpdateUser(Users user)
        {
            await this.usersTable.UpdateAsync(user);
        }

        public async Task<List<Transactions>> GetTransactions()
        {
            return await this.transactionsTable.ToListAsync();
        }

        public async Task<List<Transactions>> GetUserTransactions(string name)
        {
            return await this.transactionsTable.Where(transaction => transaction.username == name).ToListAsync();
        }
        public async Task AddTransaction(Transactions transaction)
        {
            await this.transactionsTable.InsertAsync(transaction);
        }

        public async Task DeleteTransaction(Transactions transaction)
        {
            await this.transactionsTable.DeleteAsync(transaction);
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }
    }
}