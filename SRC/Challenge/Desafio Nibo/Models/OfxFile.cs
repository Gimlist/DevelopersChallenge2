using System.Collections.Generic;

namespace Desafio_Nibo.Models
{
    public class OfxFile
    {
        /// <summary>
        /// List of transactions inside the file
        /// </summary>
        public List<Transaction> Transactions;
        /// <summary>
        /// Account Type
        /// </summary>
        public string AcctType;
        /// <summary>
        /// Account Id
        /// </summary>
        public string AcctID;
        /// <summary>
        /// Bank Id
        /// </summary>
        public string BankID;
        /// <summary>
        /// The start date 
        /// </summary>
        public string DtStart;
        /// <summary>
        /// The end date 
        /// </summary>
        public string DtEnd;


        //Fixed datas from Olx File
        public string OfxHeader;
        public string Data;
        public string Version;
        public string Security;
        public string Encoding;
        public string Charset;
        public string Compression;
        public string OldFileUID;
        public string NewFileUID;
    }
}















