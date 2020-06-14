using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using System.Threading.Tasks;

namespace DataLayer
{
    public class ContactRepositoryEx
    {
        private IDbConnection db;
        public ContactRepositoryEx(string connString)
        {
            this.db = new SqlConnection(connString);
        }

        public async Task<List<Contact>> GetAllAsync()
        {
            var contacts = await this.db.QueryAsync<Contact>("select * from Contacts");

            return contacts.ToList();
        }

        public List<Contact> GetAllContactWithAddresses()
        {
            string sql = "SELECT \n"
           + "	*\n"
           + "	  FROM contacts AS C\n"
           + "		  INNER JOIN Addresses AS A ON C.id = A.ContactId;";

            var contactDict = new Dictionary<int, Contact>();

            var contacts = this.db.Query<Contact, Address, Contact>(sql, (contact, address) =>
            {
                if (!contactDict.TryGetValue(contact.Id,out var currentContact))
                {
                    currentContact = contact;
                    contactDict.Add(currentContact.Id, currentContact);
                }
                currentContact.Addresses.Add(address);
                return currentContact;
            });

            return contacts.Distinct().ToList();
        }

        public List<Address> GetAddressesByState(int stateId)
        {
            return this.db.Query<Address>("Select * from Addresses where StateID= {=stateId}", new { stateId }).ToList();
        }

        public int BulkInsertContacts(List<Contact> contacts)
        {
            var sql = @"INSERT INTO [dbo].[Contacts]
           ([FirstName]
           ,[LastName]
           ,[Email]
           ,[Company]
           ,[Title])
            VALUES
           (@FirstName,
           @LastName,
           @Email,
           @Company,
           @Title);" + "Select cast(scope_identity() as int);";

            return this.db.Execute(sql, contacts);
        }

        public List<Contact> GetContactsById(params int[] ids)
        {
            return this.db.Query<Contact>("Select * from contacts where id in @Ids", new { Ids = ids }).ToList();
        }

        public List<dynamic> GetDynamicContactsById(params int[] ids)
        {
            return this.db.Query<dynamic>("Select * from contacts where id in @Ids", new { Ids = ids }).ToList();
        }
    }
}
