using Microsoft.EntityFrameworkCore;
using Quartz;
using RknetJobs.DB.MSSQLDBContext;
using RknetJobs.Global;
using System.Data.OleDb;
using System.Text.RegularExpressions;

namespace RknetJobs.Jobs
{
    public class GetGateAccessDataJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {


                GlobalFunctions.MessageLog("GetGateAccessDataJob запущен");
                string path = "";
                string workgroupfile = "";
                string user = "";
                string password = "";
                string configFile = "";
                string photoPath = "";
                DateTime dateTimeDepth = DateTime.Now;
                using (MSSQLDBContext mssql = new MSSQLDBContext())
                {
                    try
                    {
                        GateSettings gateSettings = mssql.GateSettings.FirstOrDefault(c => c.LastUpdate != null);
                        path = gateSettings.EventsPath;
                        workgroupfile = gateSettings.WorkGroupFile;
                        photoPath = gateSettings.PhotoPath;
                        user = gateSettings.User;
                        password = gateSettings.Password;
                        configFile = gateSettings.ConfigFile;
                        dateTimeDepth = gateSettings.LastUpdate;
                    }
                    catch (Exception ex)
                    {
                        GlobalFunctions.ErrorLog(ex);
                    }

                }


                DirectoryInfo dir = new DirectoryInfo(path);
                List<FileInfo> files = dir.GetFiles("*.mdb", SearchOption.TopDirectoryOnly).ToList();
                files = files.Where(c => c.LastWriteTime > dateTimeDepth).ToList();
                if (File.Exists(configFile) && File.GetLastWriteTime(configFile) > dateTimeDepth)
                {
                    string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = " + configFile + "; Persist Security Info=False; Jet OLEDB:System Database=" + workgroupfile + "; User ID=" + user + "; Password=" + password;
                    OleDbConnection connection = new(connectionString);
                    try
                    {
                        UpdateUsers(connection);
                        SetPhotoColumn(photoPath);
                        SetFirstDateColumn();
                        UpdateReaders(connection);
                        UpdateGroups(connection);
                        
                    }

                    catch (Exception ex)
                    {
                        GlobalFunctions.ErrorLog(ex);
                    }
                }
                foreach (FileInfo file in files)
                {
                    string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = " + path + file.Name + "; Persist Security Info=False; Jet OLEDB:System Database=" + workgroupfile + "; User ID=" + user + "; Password=" + password;
                    OleDbConnection connection = new(connectionString);
                    try
                    {

                        using (MSSQLDBContext mssql = new MSSQLDBContext())
                        {
                            connection.Open();
                            string query = "Select * from Events";
                            OleDbCommand oleDbCommand = new(query, connection);
                            OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
                            while (oleDbDataReader.Read())
                            {
                                GateEvent gateEvent = GetEventFromAccess(oleDbDataReader, file.Name);
                                if (mssql.GateEvents.FirstOrDefault(c => c.FileId == gateEvent.FileId && c.FileName == gateEvent.FileName && c.DateTime == gateEvent.DateTime) == null)
                                {
                                    mssql.GateEvents.Add(gateEvent);
                                }
                            }
                            mssql.SaveChanges();
                            connection.Close();
                        }

                    }
                    catch (Exception ex)
                    {
                        Global.GlobalFunctions.ErrorLog(ex);
                    }
                }
                Global.GlobalFunctions.MessageLog("Данные успешно скопированы");
                using (MSSQLDBContext mssql = new MSSQLDBContext())
                {
                    try
                    {
                        DateTime LastDate = files.OrderByDescending(c => c.LastWriteTime).First().LastWriteTime;
                        GateSettings gateSettings = mssql.GateSettings.FirstOrDefault(c => c.LastUpdate != null);
                        gateSettings.LastUpdate = LastDate;
                        mssql.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        GlobalFunctions.ErrorLog(ex);
                    }

                }
            }
            catch (Exception ex)
            {

                GlobalFunctions.ErrorLog(ex);
            }
            return Task.CompletedTask;
        }


        private GateEvent GetEventFromAccess(OleDbDataReader oleDbDataReader, string fileName)
        {
            GateEvent gateEvent = new();
            gateEvent.FileId = (int)oleDbDataReader[0];
            gateEvent.FileName = fileName;
            gateEvent.DateTime = (DateTime)oleDbDataReader[1];
            gateEvent.EventType = Int32.Parse(oleDbDataReader[2].ToString());
            gateEvent.EventCode = Int32.Parse(oleDbDataReader[3].ToString());
            gateEvent.RdrPtr = (int)oleDbDataReader[5];
            gateEvent.UserPtr = (int)oleDbDataReader[6];
            gateEvent.OperatorID = (int)oleDbDataReader[7];
            gateEvent.AlarmStatus = Int32.Parse(oleDbDataReader[8].ToString());
            gateEvent.Unit = oleDbDataReader[9].ToString();
            gateEvent.Message = oleDbDataReader[10].ToString();
            gateEvent.Name = oleDbDataReader[11].ToString();
            return gateEvent;
        }


        private GateUser GetUserFromAccess(OleDbDataReader oleDbDataReader)
        {
            GateUser gateUser = new GateUser();
            gateUser.UserPtr = (int)oleDbDataReader[0];
            gateUser.Number = oleDbDataReader[2].ToString();
            gateUser.LastName = oleDbDataReader[8].ToString();
            gateUser.FirstName = oleDbDataReader[9].ToString();
            gateUser.FatherName = oleDbDataReader[10].ToString();
            gateUser.GroupPtr = (int)oleDbDataReader[11];
            gateUser.Deleted = (bool)oleDbDataReader[12];
            gateUser.Status = (short)oleDbDataReader[30];
            return gateUser;
        }


        private GateGroup GetGroupFromAccess(OleDbDataReader oleDbDataReader)
        {
            GateGroup gateGroup = new GateGroup();
            gateGroup.GroupPtr = (int)oleDbDataReader[0];
            gateGroup.Name = oleDbDataReader[1].ToString();
            return gateGroup;
        }

        private GateReader GetReaderFromAccess(OleDbDataReader oleDbDataReader)
        {
            GateReader gateReader = new GateReader();
            gateReader.RdrPtr = (int)oleDbDataReader[0];
            gateReader.Name = oleDbDataReader[3].ToString();
            return gateReader;
        }


        private void UpdateReaders(OleDbConnection connection)
        {
            using (MSSQLDBContext mssql = new MSSQLDBContext())
            {
                connection.Open();
                string query = "Select * from Readers";
                OleDbCommand oleDbCommand = new(query, connection);
                OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
                while (oleDbDataReader.Read())
                {
                    int rdrPtr = (int)oleDbDataReader[0];
                    GateReader ExistingGateReader = mssql.GateReaders.FirstOrDefault(c => c.RdrPtr == rdrPtr);
                    if (ExistingGateReader != null)
                    {
                        GateReader NewGateReader = GetReaderFromAccess(oleDbDataReader);
                        mssql.Entry(ExistingGateReader).CurrentValues.SetValues(NewGateReader);
                    }
                    else
                    {
                        mssql.GateReaders.Add(GetReaderFromAccess(oleDbDataReader));
                    }
                }
                mssql.SaveChanges();
                connection.Close();
            }
        }

        private void UpdateGroups(OleDbConnection connection)
        {
            using (MSSQLDBContext mssql = new MSSQLDBContext())
            {
                connection.Open();
                string query = "Select * from Groups";
                OleDbCommand oleDbCommand = new(query, connection);
                OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
                while (oleDbDataReader.Read())
                {
                    int groupPtr = (int)oleDbDataReader[0];
                    GateGroup ExistingGateGroup = mssql.GateGroups.FirstOrDefault(c => c.GroupPtr == groupPtr);
                    if (ExistingGateGroup != null)
                    {
                        GateGroup NewGateGroup = GetGroupFromAccess(oleDbDataReader);
                        mssql.Entry(ExistingGateGroup).CurrentValues.SetValues(NewGateGroup);
                    }
                    else
                    {
                        mssql.GateGroups.Add(GetGroupFromAccess(oleDbDataReader));
                    }
                }
                mssql.SaveChanges();
                connection.Close();
            }

        }


        private void UpdateUsers(OleDbConnection connection)
        {
            using (MSSQLDBContext mssql = new MSSQLDBContext())
            {
                connection.Open();
                string query = "Select * from Users";
                OleDbCommand oleDbCommand = new(query, connection);
                OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
                while (oleDbDataReader.Read())
                {

                    int userPtr = (int)oleDbDataReader[0];
                    GateUser ExistingGateUser = mssql.GateUsers.FirstOrDefault(c => c.UserPtr == userPtr);
                    if (ExistingGateUser != null)
                    {
                        GateUser NewGateUser = GetUserFromAccess(oleDbDataReader);
                        mssql.Entry(ExistingGateUser).CurrentValues.SetValues(NewGateUser);
                    }
                    else
                    {
                        mssql.GateUsers.Add(GetUserFromAccess(oleDbDataReader));
                    }

                }
                mssql.SaveChanges();
                connection.Close();
            }
        }



        private void SetPhotoColumn(string photoPath)
        {
            string[] a = Directory.GetFiles(photoPath);
            List<int> UsersWithPhotos = ConvertArray(Directory.GetFiles(photoPath));
            using (MSSQLDBContext mssql = new MSSQLDBContext())
            {
                var usersToUpdate = mssql.GateUsers.ToList(); // Получаем всех пользователей
                foreach (var user in usersToUpdate)
                {
                    user.Photo = UsersWithPhotos.Contains(user.UserPtr);
                }
                mssql.SaveChanges();
            }
        }

        private List<int> ConvertArray(string[] photos)
        {
            string pattern = @"\d+";
            List<int> returnArray = new();
            foreach (var photo in photos)
            {
                int value = 0;
                if (Regex.Matches(photo, pattern).Count == 2 && int.TryParse((Regex.Matches(photo, pattern)[1]?.Value), out value));
                {
                    if (value >= 1000 && value <= 9999)
                    {
                        returnArray.Add(value);
                    }
                }                
            }
            return returnArray;
        }

        private void SetFirstDateColumn()
        {
            using (MSSQLDBContext mssql = new MSSQLDBContext())
            {
                var GateEvents = mssql.GateEvents.ToList();
                var GateUsers = mssql.GateUsers.Where(c => c.Date == null).ToList();
                foreach (var item in GateUsers)
                {
                    DateTime? minDateTime = GateEvents.Where(c => c.UserPtr == item.UserPtr)?
                                          .Min(c => c.DateTime)?
                                          .Date;
                    item.Date = minDateTime;
                }
                mssql.SaveChanges();
            }
        }
    }
}
