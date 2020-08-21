using Desafio_Nibo.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Desafio_Nibo.DAL
{
    public class StatementDal
    {
        /// <summary>
        /// Save transactions of Statement
        /// </summary>
        /// <param name="file">Object of Ofx File</param>
        public void SaveTransactions(OfxFile file)
        {
            using (var db = new LiteDatabase(@"BankData.db"))
            {
                // Get transaction collection
                var col = db.GetCollection<Transaction>("transctions");

                //Insert each transction
                foreach (Transaction tran in file.Transactions)
                {
                    //Validation for eliminate duplication of data
                    bool exists = col.Exists(x => x.DtPosted == tran.DtPosted && x.TRNAMT == tran.TRNAMT && x.TRNType == tran.TRNType);

                    //If data not exists, insert new data
                    if (!exists)
                        col.Insert(tran);

                }
            };

        }

        /// <summary>
        /// Save transactions of Statement
        /// </summary>
        /// <param name="file">Object of Ofx File</param>
        public List<Transaction> ListTransactions()
        {
            List<Transaction> transactions = new List<Transaction>();
            using (var db = new LiteDatabase(@"BankData.db"))
            {
                // Get transaction collection
                var col = db.GetCollection<Transaction>("transctions");

                //Find all data list
                List<Transaction> results = col.FindAll().ToList();

                return results;
            };
        }

        /// <summary>
        /// Class for clean data from database
        /// </summary>
        public void CleanDatabase()
        {
            List<Transaction> transactions = new List<Transaction>();
            using (var db = new LiteDatabase(@"BankData.db"))
            {
                // Get transaction collection
                var col = db.GetCollection<Transaction>("transctions");

                //Delete All data, used for test database
                col.DeleteAll();
            };
        }


    }
}