using NUnit.Framework;
using System;
using System.Collections.Generic;

using NHibernate;
using NHibernate.Cfg;

namespace Craswell.Automation.DataAccess.Tests
{
    [TestFixture()]
    public class Test
    {
        private ISessionFactory sessionFactory = new Configuration()
            .Configure()
            .AddAssembly(typeof(AccountData).Assembly)
            .BuildSessionFactory();

        [Test()]
        public void TestCase()
        {
            AccountData account = new AccountData()
            {
                Name = "Test Account",
                Number = "01122311",
                Balance = 0.00,
                Transactions = new List<IAccountTransaction>()
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

            account.Transactions.Add(transaction);

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
        }
    }
}

