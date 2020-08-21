using Desafio_Nibo.DAL;
using Desafio_Nibo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Desafio_Nibo.Controllers
{
    public class StatementController : Controller
    {
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Return list of transactions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult List()
        {
            try
            {
                List<Transaction> tranList = new List<Transaction>();
                var statementDal = new StatementDal();

                //Get List of Transactions
                tranList = statementDal.ListTransactions();

                return Content(JsonConvert.SerializeObject(tranList), "application/json", Encoding.UTF8);
            }
            catch
            {
                //Erros Return
                Response.StatusCode = 500;
                Response.StatusDescription = "Error of list data";
                return Json(new List<Transaction>(), JsonRequestBehavior.AllowGet);
            }
        }

        #region Upload statement

        /// <summary>
        /// Import ofx file of client statement
        /// </summary>
        /// <param name="cacheKey">automatic generate Control Key </param>
        /// <returns>Status of success or error </returns>        
        [HttpPost]
        public HttpStatusCodeResult UploadFile(string cacheKey)
        {
            try
            {
                #region Validations

                var statement = HttpContext.Request.Files.Get("statement");

                // If empty file 
                if (statement == null)
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Empty File. Please select a file.");
                if (statement.ContentLength == 0)
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, string.Format("The file {0} is empty. Please select a valid file", statement.FileName));

                #endregion

                #region Read and save in Object
                // Get file cache of memory
                HttpContext.Cache[cacheKey] = statement.InputStream;

                // File cache save for 20 min.
                HttpContext.Response.Cache.SetExpires(DateTime.Now.AddMinutes(20));

                var ofx = new OfxFile();

                // Read file 
                using (var memoryStream = new MemoryStream())
                {
                    statement.InputStream.CopyTo(memoryStream);
                    MemoryStream stream = new MemoryStream(memoryStream.ToArray());

                    ofx = ReadOfxFile(stream);
                };

                // Save Transactions in Database
                var statementDal = new StatementDal();
                statementDal.SaveTransactions(ofx);

                #endregion

                #region Success return

                return new HttpStatusCodeResult(HttpStatusCode.OK);

                #endregion
            }
            catch (Exception ex)
            {
                #region Error Return

                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message.ToString());

                #endregion
            }
        }

        #endregion


        #region Persist Statement

        /// <summary>
        /// Read olx file from memory and save in Object
        /// </summary>
        /// <param name="stream">memory file</param>
        /// <returns>OlxFile Object</returns>
        public OfxFile ReadOfxFile(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                var ofxFile = new OfxFile();
                ofxFile.Transactions = new List<Transaction>();
                bool inHeader = true;
                while (!reader.EndOfStream)
                {
                    string temp = reader.ReadLine();
                    if (inHeader)
                    {
                        if (temp.ToLower().Contains("<ofx>"))
                        {
                            inHeader = false;
                        }
                        #region Read Header
                        else
                        {
                            string[] tempSplit = temp.Split(":".ToCharArray());
                            switch (tempSplit[0].ToLower())
                            {
                                case "ofxheader":
                                    ofxFile.OfxHeader = tempSplit[1];
                                    break;
                                case "data":
                                    ofxFile.Data = tempSplit[1];
                                    break;
                                case "version":
                                    ofxFile.Version = tempSplit[1];
                                    break;
                                case "security":
                                    ofxFile.Security = tempSplit[1];
                                    break;
                                case "encoding":
                                    ofxFile.Encoding = tempSplit[1];
                                    break;
                                case "charset":
                                    ofxFile.Charset = tempSplit[1];
                                    break;
                                case "compression":
                                    ofxFile.Compression = tempSplit[1];
                                    break;
                                case "oldfileuid":
                                    ofxFile.OldFileUID = tempSplit[1];
                                    break;
                                case "newfileuid":
                                    ofxFile.NewFileUID = tempSplit[1];
                                    break;
                            }
                        }
                        #endregion
                    }
                    #region Transactions
                    if (!inHeader) // have to make different if statement so it rolls over correctly
                    {
                        string restOfFile = temp + reader.ReadToEnd();
                        restOfFile = Regex.Replace(restOfFile, Environment.NewLine, "");
                        restOfFile = Regex.Replace(restOfFile, "\n", "");
                        restOfFile = Regex.Replace(restOfFile, "\t", "");
                        ofxFile.BankID = Regex.Match(restOfFile, @"(?<=bankid>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;
                        ofxFile.AcctID = Regex.Match(restOfFile, @"(?<=acctid>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;
                        ofxFile.AcctType = Regex.Match(restOfFile, @"(?<=accttype>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;
                        ofxFile.DtStart = Regex.Match(restOfFile, @"(?<=dtstart>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;
                        ofxFile.DtEnd = Regex.Match(restOfFile, @"(?<=dtend>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;
                        string banktranlist = Regex.Match(restOfFile, @"(?<=<banktranlist>).+(?=<\/banktranlist>)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;

                        MatchCollection m = Regex.Matches(banktranlist, @"<stmttrn>.+?<\/stmttrn>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        foreach (Match match in m)
                        {
                            foreach (Capture capture in match.Captures)
                            {
                                Transaction trans = new Transaction();
                                if (Regex.Match(capture.Value, @"(?<=<TRNType>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value.ToLower().Equals("credit"))
                                    trans.TRNType = TypesOftTransaction.Credit;
                                if (Regex.Match(capture.Value, @"(?<=<TRNType>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value.ToLower().Equals("debit"))
                                    trans.TRNType = TypesOftTransaction.Debit;
                                trans.DtPosted = DateTime.ParseExact(Regex.Match(capture.Value, @"(?<=<DTPOSTED>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value.Substring(0, 14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                trans.Memo = Regex.Match(capture.Value, @"(?<=<MEMO>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;
                                trans.TRNAMT = Regex.Match(capture.Value, @"(?<=<TRNAMT>).+?(?=<)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Value;

                                ofxFile.Transactions.Add(trans);
                            }
                        }
                        #endregion
                    }
                }
                return ofxFile;
            }

        }
        #endregion

    }
}