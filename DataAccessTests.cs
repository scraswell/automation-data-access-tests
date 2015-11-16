using NUnit.Framework;
using System;
using System.Collections.Generic;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Craswell.Automation.DataAccess.Tests
{
    [TestFixture()]
    public class Test
    {
        private ISessionFactory sessionFactory;

        private Configuration hibernateConfiguration;

        [Test()]
        public void CreateSchema()
        {
            this.hibernateConfiguration = new Configuration()
                .Configure()
                .AddAssembly(typeof(AccountData).Assembly);

            new SchemaExport(this.hibernateConfiguration)
                .Execute(false, true, false);

            this.sessionFactory = this.hibernateConfiguration
                .BuildSessionFactory();
        }

        [Test()]
        public void TestCase()
        {
            this.sessionFactory = new Configuration()
                .Configure()
                .AddAssembly(typeof(AccountData).Assembly)
                .BuildSessionFactory();

            AccountData account = new AccountData()
            {
                Name = "Test Account",
                Number = "01122311",
                Balance = 0.00,
                Transactions = new List<IAccountTransaction>(),
                Statements = new List<IAccountStatement>()
            };

            AccountData account2 = null;

            AccountTransactionData transaction = new AccountTransactionData()
            {
                Timestamp = DateTime.UtcNow,
                Amount = 1000.00,
                Subject = "Test Subject"
            };

            AccountTransactionData transaction2 = new AccountTransactionData()
            {
                Timestamp = DateTime.UtcNow,
                Amount = 2000.00,
                Subject = "Test Subject"
            };

            AccountStatementData statement1 = new AccountStatementData()
            {
                Timestamp = new DateTime(2015, 10, 31),
                AccountNumber = "01122311",
                FileName = string.Format("{0}.pdf", Guid.NewGuid())
            };

            account.Transactions.Add(transaction);
            account.Statements.Add(statement1);

            using (ISession session = this.sessionFactory.OpenSession())
            using (ITransaction tx = session.BeginTransaction())
            {
                session.Save(account);
                tx.Commit();
            }

            using (ISession session = this.sessionFactory.OpenSession())
            {
                account2 = session.Get<AccountData>(account.Id);
                Assert.AreEqual(1, account2.Transactions.Count);
                Assert.AreEqual(1, account2.Statements.Count);
            }

            Assert.AreEqual(account.Id, account2.Id);
            Assert.AreEqual(account.Name, account2.Name);
            Assert.AreEqual(account.Number, account2.Number);
            Assert.AreEqual(account.Balance, account2.Balance);

            account.Transactions.Add(transaction2);

            using (ISession session = this.sessionFactory.OpenSession())
            using (ITransaction tx = session.BeginTransaction())
            {
                session.Update(account);
                tx.Commit();
            }

            using (ISession session = this.sessionFactory.OpenSession())
            {
                account2 = session.Get<AccountData>(account.Id);
                Assert.AreEqual(2, account2.Transactions.Count);
            }

            using (ISession session = this.sessionFactory.OpenSession())
            using (ITransaction tx = session.BeginTransaction())
            {
                session.Delete(account);
                tx.Commit();
            }

            this.sessionFactory.Dispose();
            this.sessionFactory = null;
        }
    }
}

