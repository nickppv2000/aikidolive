using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class LibraryContent
    {
        public string LibraryName { get; set; }
        public List<Chapter> Chapters { get; set; }

        LibraryContent()
        {
            LibraryName = "";
            Chapters = new List<Chapter>();
        }
        LibraryContent(string libraryName, List<Chapter> chapters)
        {
            LibraryName = libraryName;
            Chapters = chapters;
        }
    }
}