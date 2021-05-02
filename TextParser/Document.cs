using System.Text;
using System.IO;

namespace TextParser
{
    class Document
    {
        private string path = @"..\..\..\voyna-i-mir.txt";

        private string content = "";

        public Document()
        {
            if (fileExist(path))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding(1251)))
                    content = sr.ReadToEnd();
            }
        }

        public string getText()
        {
            return content;
        }

        private bool fileExist(string path)
        {
            return File.Exists(path);
        }
    }
}
