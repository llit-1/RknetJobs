namespace RknetJobs.Global
{
    public static class GlobalFunctions
    {

        public static string ErrorFile { get; set; } = "ErrorLog.txt";
        public static string MessageFile { get; set; } = "MessageLog.txt";

        public static void ErrorLog(Exception ex)
        {
            ClearBigFile(ErrorFile);
            using (StreamWriter writer = new StreamWriter(ErrorFile, true))
            { 
            writer.WriteLine($"{DateTime.Now} {ex.Message}");
            }
        }

        public static void MessageLog(string message)
        {

            ClearBigFile(MessageFile);
            using (StreamWriter writer = new StreamWriter(MessageFile, true))
            {
                writer.WriteLine($"{DateTime.Now} {message}");
            }
        }

        private static void ClearBigFile(string file)
        {
            FileInfo log = new FileInfo(file);
            if (log.Exists)
            {
                var size = log.Length / 1024;
                if (size >= 1024) log.Delete();
            }

        }

    }
}
