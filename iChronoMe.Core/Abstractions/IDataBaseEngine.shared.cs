using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iChronoMe.Core.Abstractions
{
    public interface IDataBaseEngine
    {
        IDataBase GetDataBase(string databasePath);
    }

    public interface IDataBase
    {
        int DropTable<T>();
        int DropTable(Type t);

        int CreateTable<T>();
        int CreateTable(Type t);

        int CreateIndex(string indexName, string tableName, IEnumerable<string> columnNames, bool unique = false);
        int CreateIndex(string indexName, string tableName, string columnName, bool unique = false);

        int Execute(string query, params object[] args);
        IEnumerable<T> ExecuteSimpleQuery<T>(string query, params object[] args);
        List<T> Query<T>(string query, params object[] args) where T : class;

        int InsertAll(IEnumerable objects, bool runInTransaction = true);
        int InsertAll(IEnumerable objects, Type objType, bool runInTransaction = true);
        int InsertOrIgnoreAll(IEnumerable objects);
        int InsertOrIgnore(object obj);
        int Insert(object obj);
        int InsertOrReplace(object obj);
        int InsertOrReplaceAll(IEnumerable objects);

        int Update(object obj);
        int Update(object obj, Type objType);
        int UpdateAll(IEnumerable objects, bool runInTransaction = true);

        int Delete(object objectToDelete);
        int Delete<T>(IEnumerable<T> objects);
        int Delete<T>(IEnumerable primaryKey);
        int DeleteAll<T>();
        int DeleteAll(Type t);

        Task<string> CreateDatabaseBackup();


        void Close();
    }
}