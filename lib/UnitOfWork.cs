using System;
using System.Collections.Generic;
using Npgsql;

namespace Hdq.RestBus
{
    public class UnitOfWork : IDisposable
    {
        public readonly NpgsqlConnection DbConnection;
        public readonly NpgsqlTransaction Transaction;


        public UnitOfWork(string connectionString)
        {
            DbConnection = new NpgsqlConnection(connectionString);
            DbConnection.Open();
            Transaction = DbConnection.BeginTransaction();
        }

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Dispose(){  
            Dispose(true);  
            GC.SuppressFinalize(this);  
        }  

        protected virtual void Dispose(bool disposing){
            if (!disposing) return;
            Transaction?.Dispose();
            DbConnection?.Close();
            DbConnection?.Dispose();
        }  
    }
}