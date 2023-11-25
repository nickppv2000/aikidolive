using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class LibraryDocument
    {
        public string Id { get; set; }
        public string Tenantid { get; set; }
        public List<LibraryContent> LibraryContents { get; set; }

        LibraryDocument()
        {
            Id = "";
            Tenantid = "";
            LibraryContents = new List<LibraryContent>();
        }
        LibraryDocument(string id, string tenantid, List<LibraryContent> libraryContent)
        {
            Id = id;
            Tenantid = tenantid;
            LibraryContents = libraryContent;
        }
    }
}