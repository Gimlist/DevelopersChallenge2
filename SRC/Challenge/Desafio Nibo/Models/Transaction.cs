using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Desafio_Nibo.Models
{
    public class Transaction
    {
        /// <summary>
        /// Type of trasanction (Debit or Credit)
        /// </summary>
        public TypesOftTransaction TRNType { get; set; }

        /// <summary>
        /// Date of trade
        /// </summary>
        public DateTime DtPosted { get; set; }

        /// <summary>
        /// Transaction Value
        /// </summary>
        public string TRNAMT { get; set; }

        /// <summary>
        ///  Description of trade
        /// </summary>
        public string Memo { get; set; }
    }

    /// <summary>
    /// Types of transactions
    /// </summary>
    public enum TypesOftTransaction
    {
        /// <summary>
        /// Debit
        /// </summary>
        [Description("Transcation with debit")]
        Debit = 0,

        /// <summary>
        /// Credit
        /// </summary>
        [Description("Transcation with credit")]
        Credit = 1
        
    }
}