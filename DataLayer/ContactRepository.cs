using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace DataLayer
{
    public class ContactRepository : IContactRepository
    {
        private IDbConnection db;
        public ContactRepository(string connString)
        {
            this.db = new SqlConnection(connString);
        }
        public Contact Add(Contact contact)
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

            var id = this.db.Query<int>(sql, contact).Single();
            contact.Id = id;

            return contact;
        }

        public Contact Find(int id)
        {
            return this.db.Query<Contact>("select * from contacts where Id=@Id", new { Id = id }).SingleOrDefault();
        }

        public void Save(Contact contact)
        {
            using (var txScope = new TransactionScope())
            {
                if (contact.IsNew)
                {
                    this.Add(contact);
                }
                else
                {
                    this.Update(contact);
                }

                foreach (var addr in contact.Addresses.Where(a => !a.IsDeleted))
                {
                    addr.ContactId = contact.Id;

                    if (addr.IsNew)
                    {
                        this.Add(addr);
                    }
                    else
                    {
                        this.Update(addr);
                    }
                }

                foreach (var addr in contact.Addresses.Where(a => a.IsDeleted))
                {
                    this.db.Execute("delete from addresses where Id=@Id", new { addr.Id });
                }

                txScope.Complete();
            }
        }

        public Address Add(Address address)
        {
            string sql = "INSERT INTO [dbo].[Addresses]\n"
                       + "(\n"
                       + "	[ContactId], \n"
                       + "	[AddressType], \n"
                       + "	[StreetAddress], \n"
                       + "	[City], \n"
                       + "	[StateId], \n"
                       + "	[PostalCode]\n"
                       + ")\n"
                       + "VALUES\n"
                       + "(\n"
                       + "	@ContactId, \n"
                       + "	@AddressType, \n"
                       + "	@StreetAddress, \n"
                       + "	@City, \n"
                       + "	@StateId, \n"
                       + "	@PostalCode\n"
                       + ");\n"
                       + "\n"
                       + "SELECT \n"
                       + "	CAST(SCOPE_IDENTITY() AS INT);";
            var id = this.db.Query<int>(sql, address).Single();
            address.Id = id;
            return address;
        }

        public Address Update(Address address)
        {
            string sql = "\n"
           + "UPDATE [dbo].[Addresses]\n"
           + "	  SET \n"
           + "		 [ContactId] = @ContactId, \n"
           + "		 [AddressType] = @AddressType, \n"
           + "		 [StreetAddress] = @StreetAddress, \n"
           + "		 [City] = @City, \n"
           + "		 [StateId] = @StateId, \n"
           + "		 [PostalCode] = @PostalCode\n"
           + "WHERE \n"
           + "	Id = @Id;";

            this.db.Execute(sql, address);

            return address;
        }

        public List<Contact> GetAll()
        {
            return this.db.Query<Contact>("select * from Contacts").ToList();
        }

        public Contact GetFullContact(int id)
        {
            var sql = "select * from contacts where Id=@Id;" +
                "select * from addresses where ContactId=@Id";

            using (var multipleResults = this.db.QueryMultiple(sql, new { Id = id }))
            {
                var contact = multipleResults.Read<Contact>().SingleOrDefault();
                var addresses = multipleResults.Read<Address>().ToList();
                if (contact != null && addresses != null)
                {
                    contact.Addresses.AddRange(addresses);
                }

                return contact;
            }
        }

        public void Remove(int id)
        {
            this.db.Execute("delete from contacts where Id=@Id", new { id });
        }

        public Contact Update(Contact contact)
        {
            string sql = "UPDATE [dbo].[Contacts]\n"
           + "   SET [FirstName] = @FirstName\n"
           + "      ,[LastName] = @LastName\n"
           + "      ,[Email] = @Email\n"
           + "      ,[Company] = @Company\n"
           + "      ,[Title] = @Title\n"
           + " WHERE Id=@Id";

            this.db.Execute(sql, contact);
            return contact;
        }
    }
}
